using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Utility;
using GSA.UnliquidatedObligations.Web.Models;
using RazorEngine;
using RazorEngine.Templating;
using R = RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hangfire;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class BackgroundTasks : IBackgroundTasks
    {
        private readonly IEmailServer EmailServer;
        private readonly ULODBEntities DB;
        private readonly IWorkflowManager WorkflowManager;
        private readonly IBackgroundJobClient BackgroundJobClient;
        protected readonly ILogger Log;
        IBackgroundTasks BT => (IBackgroundTasks)this;

        public BackgroundTasks(IBackgroundJobClient backgroundJobClient, IEmailServer emailServer, ULODBEntities db, IWorkflowManager workflowManager, ILogger log)
        {
            BackgroundJobClient = backgroundJobClient;
            EmailServer = emailServer;
            DB = db;
            WorkflowManager = workflowManager;
            Log = log.ForContext(GetType());
        }

        private System.Data.SqlClient.SqlConnection CreateSqlConnection()
            => new System.Data.SqlClient.SqlConnection(PortalHelpers.DefaultUloConnectionString);

        private static DataTable MergeIntoSingleTable(DataSet ds, Func<DataTable> creator = null)
        {
            var dt = creator == null ? new DataTable() : creator();
            for (int z = 0; z < ds.Tables.Count; ++z)
            {
                dt.Append(ds.Tables[z], creator == null && z == 0 ? true : false);
            }
            return dt;
        }

        private void FinalizeAndUpload(DataTable dt, int reviewId)
        {
            dt.SetColumnWithValue("ReviewId", reviewId);
            dt.MakeDateColumnsFitSqlServerBounds();
            dt.IdealizeStringColumns();
            dt.UploadIntoSqlServer(CreateSqlConnection);
        }

        private static readonly object EmptyModel = new object();
        private static readonly IDictionary<string, string> RazorNameByKey = new Dictionary<string, string>();
        private static string GetRazorName(string template, object model)
        {
            model = model ?? EmptyModel;
            var names = new List<string>();
            names.Add(template);
            foreach (var p in model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                names.Add(p.Name);
                names.Add(p.PropertyType.Name);
            }
            var key = Cache.CreateKey(names);
            lock (RazorNameByKey)
            {
                string rn;
                if (!RazorNameByKey.TryGetValue(key, out rn))
                {
                    RazorNameByKey[key] = rn = $"N{RazorNameByKey.Count}";
                }
                return rn;
            }
        }

        private static readonly object ProcessRazorTemplateLocker = new object();
        private static string ProcessRazorTemplate(string template, object model)
        {
            if (template == null) return null;
            var name = GetRazorName(template, model);
            lock (ProcessRazorTemplateLocker)
            {
                return Engine.Razor.RunCompile(template, name, null, model);
            }
        }

        Task IBackgroundTasks.Email(string recipient, string subjectTemplate, string bodyTemplate, string bodyHtmlTemplate, object model)
        {
            var subject = ProcessRazorTemplate(subjectTemplate, model);
            var body = ProcessRazorTemplate(bodyTemplate, model);
            var bodyHtml = ProcessRazorTemplate(bodyHtmlTemplate, model);
            EmailServer.SendEmail(subject, body, bodyHtml, recipient);
            return Task.CompletedTask;
        }

        void IBackgroundTasks.Email(string subjectTemplate, string recipient, string bodyTemplate, object model)
            => BT.Email(recipient, subjectTemplate, bodyTemplate, null, model);

        //TODO: Email on exception or let user know what happened
        public void UploadFiles(UploadFilesModel files)
        {
            var reviewId = files.ReviewId;
            string importer = null;
            int rowErrorCount = 0;

            Action<Exception, int> onRowAddError = (ex, rowNum) =>
            {
                Log.Error(ex, "OnRowAddError with {rowNum} in {importer} with {reviewId}", rowNum, importer, reviewId);
                ++rowErrorCount;
            };

            try
            {
                importer = nameof(Upload442Table);
                foreach (var fn in files.PegasysFilePathsList)
                {
                    Upload442Table(reviewId, fn, onRowAddError);
                }

                importer = nameof(UploadRetaTable);
                foreach (var fn in files.RetaFileList)
                {
                    UploadRetaTable(reviewId, fn, onRowAddError);
                }

                importer = nameof(UploadEasiTable);
                foreach (var fn in files.EasiFileList)
                {
                    UploadEasiTable(reviewId, fn, onRowAddError);
                }

                importer = nameof(Upload192Table);
                foreach (var fn in files.One92FileList)
                {
                    Upload192Table(reviewId, fn, onRowAddError);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Problem with UploadFiles in {reviewId}", reviewId);
                throw;
            }
            finally
            {
                var level = rowErrorCount == 0 ? Serilog.Events.LogEventLevel.Information : Serilog.Events.LogEventLevel.Warning;
                Log.Write(level, "Importing of {reviewId} yielded {rowErrorCount}.  Note, this does not indicate either overall success or failure.", reviewId, rowErrorCount);
            }
        }


        //TODO: Email on exception or let user know what happened
        public void CreateULOsAndAssign(int reviewId, int workflowDefinitionId, DateTime reviewDate)
        {
            using (ULODBEntities _db = DB)
            {
                _db.Database.CommandTimeout = 60*15;
                _db.CreateULOAndAssignWf(reviewId, workflowDefinitionId, reviewDate.Date);
            }
        }

        private async Task SendAssignWorkFlowsBatchNotifications(ICollection<Workflow> workflows, string userId)
        {
            var u = await DB.AspNetUsers.FindAsync(userId);
            var et = await DB.EmailTemplates.FindAsync(Properties.Settings.Default.BatchAssignmentNotificationEmailTemplateId);
            var m = new ItemsEmailViewModel<Workflow>(u, workflows);
            await BT.Email(u.Email, et.EmailSubject, et.EmailBody, et.EmailHtmlBody, m);
        }

        async Task IBackgroundTasks.SendAssignWorkFlowsBatchNotifications(int reviewId, string userId)
        {
            try
            {
                var workflows = await DB.Workflows.Include(wf => wf.UnliquidatedObligation).
                    Where(wf => wf.UnliquidatedObligation.ReviewId == reviewId && wf.OwnerUserId == userId).
                    OrderBy(wf => wf.UnliquidatedObligation.PegasysDocumentNumber).
                    ToListAsync();
                await SendAssignWorkFlowsBatchNotifications(workflows, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, 
                    "Problem in {methodName}", 
                    nameof(IBackgroundTasks.SendAssignWorkFlowsBatchNotifications));
                throw;
            }
        }

        async Task IBackgroundTasks.SendAssignWorkFlowsBatchNotifications(int[] workflowIds, string userId)
        {
            try
            {
                var workflows = await PortalHelpers.GetWorkflows(DB, workflowIds).ToListAsync();
                await SendAssignWorkFlowsBatchNotifications(workflows, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "Problem in {methodName}",
                    nameof(IBackgroundTasks.SendAssignWorkFlowsBatchNotifications));
                throw;
            }
        }

        async Task IBackgroundTasks.SendAssignWorkFlowsBatchNotifications(int reviewId)
        {
            int assigneesProcessed = 0;
            try
            {
                var assignees = await
                DB.Workflows.Where(wf => wf.UnliquidatedObligation.ReviewId == reviewId && wf.AspNetUser.UserType == AspNetUser.UserTypes.Person).Select(wf => wf.OwnerUserId).Distinct().ToListAsync();
                foreach (var assignee in assignees)
                {
                    BackgroundJob.Enqueue<IBackgroundTasks>(bt => bt.SendAssignWorkFlowsBatchNotifications(reviewId, assignee));
                    ++assigneesProcessed;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, 
                    "Problem in {methodName} after having queued {assigneesProcessed} user batch notifications", 
                    nameof(IBackgroundTasks.SendAssignWorkFlowsBatchNotifications), 
                    assigneesProcessed);
                throw;
            }
        }

        //TODO: Email on exception or let user know what happened.
        async Task IBackgroundTasks.AssignWorkFlows(int reviewId, bool sendBatchNotifications)
        {
            try
            {
                var review = await DB.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    Log.Information("AssignWorkFlows could not find {reviewId}", reviewId);
                    return;
                }
                review.Status = Review.StatusNames.Assigning;
                await DB.SaveChangesAsync();

                var workflows =
                    DB.Workflows.Include(wf => wf.UnliquidatedObligation).Include(wf => wf.AspNetUser).
                    Where(wf => wf.OwnerUserId == PortalHelpers.PreAssignmentUserUserId).
                    OrderBy(wf => wf.UnliquidatedObligation.ReviewId == reviewId ? 0 : 1).
                    ToList();

                Log.Information("AssignWorkFlows {reviewId} assigning up to {totalRecords}", reviewId, workflows.Count);

                int z = 0;
                foreach (var workflow in workflows)
                {
                    await WorkflowManager.AdvanceAsync(workflow, null, true, true, !sendBatchNotifications);
                    if (++z % 10 == 0)
                    {
                        await DB.SaveChangesAsync();
                        Log.Debug("AssignWorkFlows {reviewId} save after {recordsProcessed}/{totalRecords}", reviewId, z, workflows.Count);
                        using (var zdb = PortalHelpers.UloDbCreator())
                        {
                            var r = await zdb.Reviews.FindAsync(reviewId);
                            if (r == null)
                            {
                                Log.Information("AssignWorkFlows cancelled as {reviewId} has been deleted", reviewId);
                                break;
                            }
                        }
                    }
                }
                review.SetStatusDependingOnClosedBit();
                await DB.SaveChangesAsync();
                Log.Information("AssignWorkFlows {reviewId} completed after {recordsProcessed}/{totalRecords}", reviewId, z, workflows.Count);

                if (Properties.Settings.Default.SendBatchEmailsDuringAssignWorkflows)
                {
                    BackgroundJob.Enqueue<IBackgroundTasks>(bt => bt.SendAssignWorkFlowsBatchNotifications(reviewId));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "Problem in {methodName}",
                    nameof(IBackgroundTasks.AssignWorkFlows));
                throw;
            }
        }

        private void Upload442Table(int reviewId, string uploadPath, Action<Exception, int> onRowAddError)
        {
            var dt = CreatePegasysOpenObligationsDataTable();
            using (var st = File.OpenRead(uploadPath))
            {
                dt.LoadRowsFromDelineatedText(st, new LoadRowsFromDelineatedTextSettings
                {
                    SkipRawRows = 2,
                    RowAddErrorHandler = onRowAddError
                });
                dt.SetColumnWithValue("ReviewId", reviewId);
                dt.UploadIntoSqlServer(CreateSqlConnection);
            }
        }

        private void UploadRetaTable(int reviewId, string uploadPath, Action<Exception, int> onRowAddError)
        {
            DataTable dt;
            using (var st = File.OpenRead(uploadPath))
            {
                var ds = new DataSet();
                var settings = new LoadTablesFromSpreadsheetSettings()
                {
                    SheetSettings = new List<LoadRowsFromSpreadsheetSettings>(),
                    CreateDataTable = CreateRetaDataTable
                };

                var sheetNames = R.CSV.ParseLine(Properties.Settings.Default.RetaSheetsToImport);
                foreach (var sheetName in sheetNames)
                {
                    settings.SheetSettings.Add(
                    new LoadRowsFromSpreadsheetSettings
                    {
                        SheetName = sheetName,
                        TypeConverter = SpreadsheetHelpers.ExcelTypeConverter,
                        UseSheetNameForTableName = true,
                        RowAddErrorHandler = onRowAddError,
                        ThrowOnMissingSheet = false,
                    });
                }

                ds.LoadSheetsFromExcel(st, settings);
                dt = MergeIntoSingleTable(ds, CreateRetaDataTable);
            }
            FinalizeAndUpload(dt, reviewId);
        }

        private void UploadEasiTable(int reviewId, string uploadPath, Action<Exception, int> onRowAddError)
        {
            DataTable dt;
            using (var st = File.OpenRead(uploadPath))
            {
                var ds = new DataSet();
                var settings = new LoadTablesFromSpreadsheetSettings()
                {
                    CreateDataTable = CreateEasiTable,
                    LoadAllSheetsDefaultSettings = new LoadRowsFromSpreadsheetSettings
                    {
                        TypeConverter = SpreadsheetHelpers.ExcelTypeConverter,
                        RowAddErrorHandler = onRowAddError
                    }
                };
                ds.LoadSheetsFromExcel(st, settings);
                dt = MergeIntoSingleTable(ds, CreateEasiTable);
            }
            FinalizeAndUpload(dt, reviewId);
        }

        private void Upload192Table(int reviewId, string uploadPath, Action<Exception, int> onRowAddError)
        {
            DataTable dt;
            using (var st = File.OpenRead(uploadPath))
            {
                var ds = new DataSet();
                var settings = new LoadTablesFromSpreadsheetSettings()
                {
                    SheetSettings = new List<LoadRowsFromSpreadsheetSettings>(),
                    CreateDataTable = Create192Table,
                };
                var sheetNames = R.CSV.ParseLine(Properties.Settings.Default.Upload192SheetsToImport);
                foreach (var sheetName in sheetNames)
                {
                    settings.SheetSettings.Add(
                    new LoadRowsFromSpreadsheetSettings
                    {
                        SheetName = sheetName,
                        TypeConverter = SpreadsheetHelpers.ExcelTypeConverter,
                        UseSheetNameForTableName = true,
                        RowAddErrorHandler = onRowAddError,
                        SkipRawRows = Properties.Settings.Default.Upload192SkipRawRows,
                    });
                }
                ds.LoadSheetsFromExcel(st, settings);
                dt = MergeIntoSingleTable(ds, Create192Table);
            }
            FinalizeAndUpload(dt, reviewId);
        }

        private static DataTable CreatePegasysOpenObligationsDataTable()
        {
            var dt = new DataTable()
            {
                TableName = "PegasysObligations442"
            };
            dt.Columns.Add("ReviewId", typeof(int));
            //dt.Columns.Add("SourceRowNumber", typeof(int));
            dt.Columns.Add("Region", typeof(int));
            dt.Columns.Add("Fund", typeof(string));
            dt.Columns.Add("Organization", typeof(string));
            dt.Columns.Add("Prog", typeof(string));
            dt.Columns.Add("Doc Type", typeof(string)); //renamed from the excel to prove the test
            dt.Columns.Add("Doc Num", typeof(string));
            dt.Columns.Add("Itmz Ln Num", typeof(int));
            dt.Columns.Add("Actg Ln Num", typeof(int));
            dt.Columns.Add("Title", typeof(string));
            dt.Columns.Add("BBFY", typeof(string));
            dt.Columns.Add("EBFY", typeof(string));
            dt.Columns.Add("Acty", typeof(string));
            dt.Columns.Add("O/C", typeof(string));
            dt.Columns.Add("SOC", typeof(string));
            dt.Columns.Add("Project", typeof(string));
            dt.Columns.Add("Agreement", typeof(string));
            dt.Columns.Add("Contract Num", typeof(string));
            dt.Columns.Add("Bldg", typeof(string));
            dt.Columns.Add("Sys/Loc", typeof(string));
            dt.Columns.Add("Veh Tag", typeof(string));
            dt.Columns.Add("WI", typeof(string));
            dt.Columns.Add("Lease Number", typeof(string));
            dt.Columns.Add("Vendor Name", typeof(string));
            dt.Columns.Add("Actg Pd", typeof(string));
            dt.Columns.Add("Total Line", typeof(decimal));
            dt.Columns.Add("Commitments", typeof(decimal));
            dt.Columns.Add("Prepayments", typeof(decimal));
            dt.Columns.Add("Undel Orders", typeof(decimal));
            dt.Columns.Add("Rcpt", typeof(decimal));
            dt.Columns.Add("Accrual", typeof(decimal));
            dt.Columns.Add("Pend Payments", typeof(decimal));
            dt.Columns.Add("Pymts(In Transit)", typeof(decimal));
            dt.Columns.Add("Pymts(Confirmed)", typeof(decimal));
            dt.Columns.Add("Holdbacks", typeof(decimal));
            dt.Columns.Add("TAFS", typeof(string));
            dt.Columns.Add("DUNS #", typeof(string));
            dt.Columns.Add("Date of Last Activity", typeof(string));
            dt.Columns.Add("Days Since First Activity", typeof(int));
            dt.Columns.Add("Days Since Last Activity", typeof(int));
            dt.Columns.Add("Date of First Activity", typeof(string));
            dt.Columns.Add("Trading Partner Type", typeof(string));
            dt.Columns.Add("Vendor Agency Code", typeof(string));
            dt.Columns.Add("Vendor Bureau Code", typeof(string));
            return dt;
        }

        private static DataTable CreateRetaDataTable()
        {
            var dt = new DataTable()
            {
                TableName = "Reta"
            };

            dt.Columns.Add("ReviewId", typeof(int));
            dt.Columns.Add("Agreement", typeof(int));
            dt.Columns.Add("RWA Type", typeof(string));
            dt.Columns.Add("Completion Date", typeof(DateTime));
            dt.Columns.Add("Agency Bureau Code", typeof(string));
            dt.Columns.Add("RETA POC", typeof(string));
            dt.Columns.Add("Region", typeof(int));
            dt.Columns.Add("Fund", typeof(string));
            dt.Columns.Add("Organization", typeof(string));
            dt.Columns.Add("Prog", typeof(string));
            dt.Columns.Add("Doc Type", typeof(string)); //renamed from the excel to prove the test
            dt.Columns.Add("Doc Num", typeof(string));
            dt.Columns.Add("Itmz Ln Num", typeof(int));
            dt.Columns.Add("Actg Ln Num", typeof(int));
            dt.Columns.Add("Title", typeof(string));
            dt.Columns.Add("BBFY", typeof(string));
            dt.Columns.Add("EBFY", typeof(string));
            dt.Columns.Add("Acty", typeof(string));
            dt.Columns.Add("O/C", typeof(string));
            dt.Columns.Add("SOC", typeof(string));
            dt.Columns.Add("Project", typeof(string));
            //dt.Columns.Add("Agreement", typeof(string));
            dt.Columns.Add("Contract Num", typeof(string));
            dt.Columns.Add("Bldg", typeof(string));
            dt.Columns.Add("Sys/Loc", typeof(string));
            dt.Columns.Add("Veh Tag", typeof(string));
            dt.Columns.Add("WI", typeof(string));
            dt.Columns.Add("Lease Number", typeof(string));
            dt.Columns.Add("Vendor Name", typeof(string));
            dt.Columns.Add("Actg Pd", typeof(string));
            dt.Columns.Add("Total Line", typeof(decimal));
            dt.Columns.Add("Commitments", typeof(decimal));
            dt.Columns.Add("Prepayments", typeof(decimal));
            dt.Columns.Add("Undel Orders", typeof(decimal));
            dt.Columns.Add("Rcpt", typeof(decimal));
            dt.Columns.Add("Accrual", typeof(decimal));
            dt.Columns.Add("Pend Payments", typeof(decimal));
            dt.Columns.Add("Pymts(In Transit)", typeof(decimal));
            dt.Columns.Add("Pymts(Confirmed)", typeof(decimal));
            dt.Columns.Add("Holdbacks", typeof(decimal));
            dt.Columns.Add("TAFS", typeof(string));
            dt.Columns.Add("DUNS #", typeof(string));
            dt.Columns.Add("Date of Last Activity", typeof(DateTime));
            dt.Columns.Add("Days Since First Activity", typeof(int));
            dt.Columns.Add("Days Since Last Activity", typeof(int));
            dt.Columns.Add("Date of First Activity", typeof(DateTime));
            dt.Columns.Add("Trading Partner Type", typeof(string));
            dt.Columns.Add("Vendor Agency Code", typeof(string));
            dt.Columns.Add("Vendor Bureau Code", typeof(string));
            return dt;
        }

        private static DataTable CreateEasiTable()
        {
            var dt = new DataTable()
            {
                TableName = "EASI"
            };
            dt.Columns.Add("ReviewId", typeof(int));
            //dt.Columns.Add("SourceRowNumber", typeof(int));
            dt.Columns.Add("Created by users Region", typeof(string));
            dt.Columns.Add("Office", typeof(string));
            dt.Columns.Add("Created By (User Full Name)", typeof(string));
            dt.Columns.Add("Contracting Officers Name", typeof(string));
            dt.Columns.Add("Contracting Userid", typeof(string));
            dt.Columns.Add("Awd Obligated Amt", typeof(string));
            dt.Columns.Add("Current Awd Amt", typeof(string));
            dt.Columns.Add("Document Nbr", typeof(string));
            dt.Columns.Add("Award Nbr", typeof(string));
            dt.Columns.Add("Base Contract Nbr", typeof(string));
            dt.Columns.Add("GSA FSS/Other #", typeof(string));
            dt.Columns.Add("Vendor Name", typeof(string));
            dt.Columns.Add("Status Cd", typeof(string));
            dt.Columns.Add("Status Ds", typeof(string));
            dt.Columns.Add("Procurement Status", typeof(string));
            dt.Columns.Add("Create Dt", typeof(DateTime));
            dt.Columns.Add("Signed On Date", typeof(DateTime));
            dt.Columns.Add("Awd Effective Dt", typeof(DateTime));
            dt.Columns.Add("Awd Expiration Dt", typeof(DateTime));
            dt.Columns.Add("CLIN ADN", typeof(string));
            dt.Columns.Add("ACT/PDN", typeof(string));
            dt.Columns.Add("Data Source", typeof(string));
            dt.Columns.Add("Region Cd", typeof(string));
            dt.Columns.Add("Contracting Officer Email", typeof(string));
            dt.Columns.Add("Contracting Specialist Name", typeof(string));
            dt.Columns.Add("Contracting Specialist Email", typeof(string));
            dt.Columns.Add("Budget Analyst Email", typeof(string));
            return dt;
        }

        private static DataTable Create192Table()
        {
            var dt = new DataTable()
            {
                TableName = "PegasysObligations192"
            };
            dt.Columns.Add("ReviewId", typeof(int));
            dt.Columns.Add("Region", typeof(string));
            dt.Columns.Add("Fund", typeof(string));
            dt.Columns.Add("Organization", typeof(string));
            dt.Columns.Add("Prog", typeof(string));
            dt.Columns.Add("Doc Type", typeof(string)); //renamed from the excel to prove the test
            dt.Columns.Add("Doc Num", typeof(string));
            dt.Columns.Add("Itmz Ln Num", typeof(int));
            dt.Columns.Add("Actg Ln Num", typeof(int));
            dt.Columns.Add("Title", typeof(string));
            dt.Columns.Add("BBFY", typeof(string));
            dt.Columns.Add("EBFY", typeof(string));
            dt.Columns.Add("Acty", typeof(string));
            dt.Columns.Add("O/C", typeof(string));
            dt.Columns.Add("SOC", typeof(string));
            dt.Columns.Add("Project", typeof(string));
            dt.Columns.Add("Agreement", typeof(string));
            dt.Columns.Add("Contract Num", typeof(string));
            dt.Columns.Add("Bldg", typeof(string));
            dt.Columns.Add("Sys/Loc", typeof(string));
            dt.Columns.Add("Veh Tag", typeof(string));
            dt.Columns.Add("WI", typeof(string));
            dt.Columns.Add("Lease Number", typeof(string));
            dt.Columns.Add("Vendor Name", typeof(string));
            dt.Columns.Add("Actg Pd", typeof(string));
            dt.Columns.Add("Total Line", typeof(decimal));
            dt.Columns.Add("Commitments", typeof(decimal));
            dt.Columns.Add("Prepayments", typeof(decimal));
            dt.Columns.Add("Undel Orders", typeof(decimal));
            dt.Columns.Add("Rcpt", typeof(decimal));
            dt.Columns.Add("Accrual", typeof(decimal));
            dt.Columns.Add("Pend Payments", typeof(decimal));
            dt.Columns.Add("Pymts(In Transit)", typeof(decimal));
            dt.Columns.Add("Pymts(Confirmed)", typeof(decimal));
            dt.Columns.Add("Holdbacks", typeof(decimal));
            dt.Columns.Add("TAFS", typeof(string));
            dt.Columns.Add("DUNS #", typeof(string));
            dt.Columns.Add("Date of Last Activity", typeof(DateTime));
            dt.Columns.Add("Days Since First Activity", typeof(int));
            dt.Columns.Add("Days Since Last Activity", typeof(int));
            dt.Columns.Add("Date of First Activity", typeof(DateTime));
            dt.Columns.Add("Trading Partner Type", typeof(string));
            dt.Columns.Add("Vendor Agency Code", typeof(string));
            dt.Columns.Add("Vendor Bureau Code", typeof(string));
            return dt;
        }
    }
}
