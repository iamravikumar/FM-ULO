using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Utility;
using GSA.UnliquidatedObligations.Web.Models;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;
using Serilog;
using R = RevolutionaryStuff.Core;
using U = GSA.UnliquidatedObligations.Utility;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class BackgroundTasks : IBackgroundTasks
    {
        private readonly IEmailServer EmailServer;
        private readonly UloDbContext DB;
        private readonly IWorkflowManager WorkflowManager;
        private readonly RazorTemplateProcessor RazorTemplateProcessor;
        private readonly IReportRunner ReportRunner;
        private readonly IOptions<Config> ConfigOptions;
        private readonly IConfiguration Configuration;
        private readonly UserHelpers UserHelpers;
        private readonly PortalHelpers PortalHelpers;
        protected readonly ILogger Log;
        IBackgroundTasks BT => (IBackgroundTasks)this;

        public class Config
        {
            public const string ConfigSectionName = "BackgroundTasksConfig";
            public TimeSpan CommandTimeout { get; set; }
            public string ConnectionStringName { get; set; }
            public int BatchAssignmentNotificationEmailTemplateId { get; set; }
            public bool SendBatchEmailsDuringAssignWorkflows { get; set; }
            public ImportConfig Upload192ImportConfig { get; set; }
            public ImportConfig RetaImportConfig { get; set; }
            public ImportConfig CreditCardAliasConfig { get; set; }
            public ImportConfig PegasysOpenItemsCreditCardsSheetsConfig { get; set; }
            public ImportConfig ActiveCardholderImportConfig { get; set; }

            public class ImportConfig
            { 
                public string SheetsCsv { get; set; }
                public int SkipRawRows { get; set; }
            }
        }

        public BackgroundTasks(RazorTemplateProcessor razorTemplateProcessor, IReportRunner reportRunner, IOptions<Config> configOptions, IConfiguration configuration, UserHelpers userHelpers, IEmailServer emailServer, UloDbContext db, IWorkflowManager workflowManager, ILogger log, PortalHelpers portalHelpers)
        {
            RazorTemplateProcessor = razorTemplateProcessor;
            ReportRunner = reportRunner;
            ConfigOptions = configOptions;
            Configuration = configuration;
            UserHelpers = userHelpers;
            EmailServer = emailServer;
            DB = db;
            WorkflowManager = workflowManager;
            PortalHelpers = portalHelpers;
            Log = log.ForContext(GetType());
        }

        async Task IBackgroundTasks.EmailReport(string[] recipients, string subjectTemplate, string bodyTemplate, string htmlBodyTemplate, object model, string reportName, IDictionary<string, string> paramValueByParamName)
        {
            var res = await ReportRunner.ExecuteAsync(reportName, paramValueByParamName);
            await EmailAsync(recipients, subjectTemplate, bodyTemplate, htmlBodyTemplate, model, new[] { res });
        }

        private Microsoft.Data.SqlClient.SqlConnection CreateSqlConnection()
            => new Microsoft.Data.SqlClient.SqlConnection(Configuration.GetConnectionString(ConfigOptions.Value.ConnectionStringName));

        private static DataTable MergeIntoSingleTable(DataSet ds, Func<DataTable> creator = null)
        {
            var dt = creator == null ? new DataTable() : creator();
            for (int z = 0; z < ds.Tables.Count; ++z)
            {
                dt.Append(ds.Tables[z], creator == null && z == 0 ? true : false);
            }
            return dt;
        }

        private void FinalizeAndUpload(DataTable dt, int reviewId, RowsTransferredEventHandler rowsTransferred)
        {
            dt.SetColumnWithValue(ReviewIdColumnName, reviewId);
            dt.MakeDateColumnsFitSqlServerBounds();
            dt.IdealizeStringColumns();
            dt.UploadIntoSqlServer(CreateSqlConnection, new UploadIntoSqlServerSettings { RowsTransferredEventHandler = rowsTransferred });
        }

        private static readonly object ProcessRazorTemplateLocker = new object();
        private string ProcessRazorTemplate(string template, object model)
        {
            if (template == null) return null;
            lock (ProcessRazorTemplateLocker)
            {
                return RazorTemplateProcessor.ProcessAsync(template, model).ExecuteSynchronously();
            }
        }

        Task IBackgroundTasks.Email(string recipient, string subjectTemplate, string bodyTemplate, string bodyHtmlTemplate, object model)
            => EmailAsync(new[] { recipient }, subjectTemplate, bodyTemplate, bodyHtmlTemplate, model);

        Task EmailAsync(string[] recipients, string subjectTemplate, string bodyTemplate, string bodyHtmlTemplate, object model, IEnumerable<System.Net.Mail.Attachment> attachments = null)
        {
            var subject = ProcessRazorTemplate(subjectTemplate, model);
            var body = ProcessRazorTemplate(bodyTemplate, model);
            var bodyHtml = ProcessRazorTemplate(bodyHtmlTemplate, model);
            EmailServer.SendEmail(subject, body, bodyHtml, recipients, attachments);
            return Task.CompletedTask;
        }

        //TODO: Email on exception or let user know what happened
        public void UploadFiles(UploadFilesModel files)
        {
            var reviewId = files.ReviewId;
            string importer = null;
            int rowErrorCount = 0;

            Action<Exception, int> onRowAddError = (ex, rowNum) =>
            {
                Log.Error(ex, "UploadFiles: OnRowAddError with {rowNum} in {importer} with {reviewId}", rowNum, importer, reviewId);
                ++rowErrorCount;
            };

            RowsTransferredEventHandler rowsTransferred = (sender, e) =>
            {
                Log.Information("UploadFiles: Loading progress. {rowCount} in {importer} with {reviewId}", e.RowsTransferred, importer, reviewId);
            };

            try
            {
                importer = nameof(Load442s);
                foreach (var fn in files.PegasysFilePathsList)
                {
                    Load442s(reviewId, fn, onRowAddError, rowsTransferred);
                }

                importer = nameof(LoadReta);
                foreach (var fn in files.RetaFileList)
                {
                    LoadReta(reviewId, fn, onRowAddError, rowsTransferred);
                }

                importer = nameof(LoadEasi);
                foreach (var fn in files.EasiFileList)
                {
                    LoadEasi(reviewId, fn, onRowAddError, rowsTransferred);
                }

                importer = nameof(Load192s);
                foreach (var fn in files.One92FileList)
                {
                    Load192s(reviewId, fn, onRowAddError, rowsTransferred);
                }

                importer = nameof(LoadCreditCardAliases);
                foreach (var fn in files.CreditCardAliasCrosswalkFiles)
                {
                    LoadCreditCardAliases(reviewId, fn, onRowAddError, rowsTransferred);
                }

                importer = nameof(LoadPegasysOpenItemsCreditCards);
                foreach (var fn in files.PegasysOpenItemsCreditCards)
                {
                    LoadPegasysOpenItemsCreditCards(reviewId, fn, onRowAddError, rowsTransferred);
                }

                importer = nameof(LoadActiveCardholders);
                foreach (var fn in files.ActiveCardholderFiles)
                {
                    LoadActiveCardholders(reviewId, fn, onRowAddError, rowsTransferred);
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
            var config = ConfigOptions.Value;
            DB.CreateULOAndAssignWfAsync(reviewId, workflowDefinitionId, reviewDate, config.CommandTimeout).ExecuteSynchronously();
        }

        private async Task SendAssignWorkFlowsBatchNotifications(IQueryable<Workflow> workflows, string userId)
        {
            var config = ConfigOptions.Value;

            var u = await DB.AspNetUsers.FindAsync(userId);
            var et = PortalHelpers.GetEmailTemplate(config.BatchAssignmentNotificationEmailTemplateId);
            var wfs = await (workflows.Include(z => z.TargetUlo.Review).Include(wf => wf.TargetUlo).ToListAsync());
            var m = new WorkflowsEmailViewModel(u, wfs);
            await BT.Email(u.Email, et.EmailSubject, et.EmailBody, et.EmailHtmlBody, m);
        }

        async Task IBackgroundTasks.SendAssignWorkFlowsBatchNotifications(int reviewId, string userId)
        {
            try
            {
                var workflows = DB.Workflows.
                    Where(wf => wf.TargetUlo.ReviewId == reviewId && wf.OwnerUserId == userId).
                    OrderBy(wf => wf.TargetUlo.PegasysDocumentNumber);
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
                var workflows = DB.GetWorkflows(workflowIds);
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
                DB.Workflows.Where(wf => wf.TargetUlo.ReviewId == reviewId && wf.OwnerUser.UserType == AspNetUser.UserTypes.Person).Select(wf => wf.OwnerUserId).Distinct().ToListAsync();
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
            var config = ConfigOptions.Value;

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
                    DB.Workflows.Include(wf => wf.TargetUlo).Include(wf => wf.OwnerUser).
                    Where(wf => wf.OwnerUserId == UserHelpers.PreAssignmentUserUserId).
                    OrderBy(wf => wf.TargetUlo.ReviewId == reviewId ? 0 : 1).
                    ToList();

                Log.Information("AssignWorkFlows {reviewId} assigning up to {totalRecords}", reviewId, workflows.Count);

                int z = 0;
                foreach (var workflow in workflows)
                {
                    await WorkflowManager.AdvanceAsync(workflow, null, null, true, true, !sendBatchNotifications);
                    if (++z % 10 == 0)
                    {
                        await DB.SaveChangesAsync();
                        Log.Debug("AssignWorkFlows {reviewId} save after {recordsProcessed}/{totalRecords}", reviewId, z, workflows.Count);
                        var r = await DB.Reviews.FindAsync(reviewId);
                        if (r == null)
                        {
                            Log.Information("AssignWorkFlows cancelled as {reviewId} has been deleted", reviewId);
                            break;
                        }
                    }
                }
                review.SetStatusDependingOnClosedBit();
                await DB.SaveChangesAsync();
                Log.Information("AssignWorkFlows {reviewId} completed after {recordsProcessed}/{totalRecords}", reviewId, z, workflows.Count);

                if (config.SendBatchEmailsDuringAssignWorkflows)
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

        #region Loaders

        private void LoadExcelDataTable(int reviewId, string uploadPath, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred, Config.ImportConfig importConfig, Func<DataTable> dataTableCreator)
        {
            string sheetNamesCsv = importConfig.SheetsCsv;
            int skipRawRows = importConfig.SkipRawRows;

            DataTable dt;
            using (var st = File.OpenRead(uploadPath))
            {
                var ds = new DataSet();
                var settings = new LoadTablesFromSpreadsheetSettings()
                {
                    SheetSettings = new List<LoadRowsFromSpreadsheetSettings>(),
                    CreateDataTable = dataTableCreator,
                };
                var sheetNames = R.CSV.ParseLine(sheetNamesCsv);
                foreach (var sheetName in sheetNames)
                {
                    settings.SheetSettings.Add(
                    new LoadRowsFromSpreadsheetSettings
                    {
                        SheetName = sheetName,
                        TypeConverter = U.SpreadsheetHelpers.ExcelTypeConverter,
                        UseSheetNameForTableName = true,
                        RowAddErrorHandler = onRowAddError,
                        SkipRawRows = skipRawRows,
                    });
                }
                ds.LoadSheetsFromExcel(st, settings);
                dt = MergeIntoSingleTable(ds, dataTableCreator);
            }
            FinalizeAndUpload(dt, reviewId, rowsTransferred);
        }

        private void Load442s(int reviewId, string uploadPath, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
        {
            var dt = CreatePegasysOpenObligationsDataTable();
            using (var st = File.OpenRead(uploadPath))
            {
                dt.LoadRowsFromDelineatedText(st, new LoadRowsFromDelineatedTextSettings
                {
                    SkipRawRows = 2,
                    RowAddErrorHandler = onRowAddError
                });
                dt.SetColumnWithValue(ReviewIdColumnName, reviewId);
                dt.UploadIntoSqlServer(CreateSqlConnection, new UploadIntoSqlServerSettings { RowsTransferredEventHandler = rowsTransferred });
            }
        }

        private void LoadReta(int reviewId, string uploadPath, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
             => LoadExcelDataTable(reviewId, uploadPath, onRowAddError, rowsTransferred, ConfigOptions.Value.RetaImportConfig, CreateRetaDataTable);

        private void LoadEasi(int reviewId, string uploadPath, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
        {
            DataTable dt;
            using (var st = File.OpenRead(uploadPath))
            {
                var ds = new DataSet();
                var settings = new LoadTablesFromSpreadsheetSettings()
                {
                    CreateDataTable = CreateEasiDataTable,
                    LoadAllSheetsDefaultSettings = new LoadRowsFromSpreadsheetSettings
                    {
                        TypeConverter = U.SpreadsheetHelpers.ExcelTypeConverter,
                        RowAddErrorHandler = onRowAddError
                    }
                };
                ds.LoadSheetsFromExcel(st, settings);
                dt = MergeIntoSingleTable(ds, CreateEasiDataTable);
            }
            FinalizeAndUpload(dt, reviewId, rowsTransferred);
        }


        private void Load192s(int reviewId, string uploadPath, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
            => LoadExcelDataTable(reviewId, uploadPath, onRowAddError, rowsTransferred, ConfigOptions.Value.Upload192ImportConfig, Create192DataTable);

        private void LoadCreditCardAliases(int reviewId, string uploadPath, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
            => LoadExcelDataTable(reviewId, uploadPath, onRowAddError, rowsTransferred, ConfigOptions.Value.CreditCardAliasConfig, CreateCreditCardAliasDataTable);

        private void LoadPegasysOpenItemsCreditCards(int reviewId, string uploadPath, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
            => LoadExcelDataTable(reviewId, uploadPath, onRowAddError, rowsTransferred, ConfigOptions.Value.PegasysOpenItemsCreditCardsSheetsConfig, CreatePegasysOpenItemsCreditCardsDataTable);

        private void LoadActiveCardholders(int reviewId, string uploadPath, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
            => LoadExcelDataTable(reviewId, uploadPath, onRowAddError, rowsTransferred, ConfigOptions.Value.ActiveCardholderImportConfig, CreateActiveCardholdersDataTable);

        #endregion

        #region DataTable Creation

        private static DataTable CreateDataTable(string tableName, params DataColumn[] columns)
        {
            var dt = new DataTable()
            {
                TableName = tableName
            };
            foreach (var col in columns)
            {
                dt.Columns.Add(col);
            }
            return dt;
        }

        private const string ReviewIdColumnName = "ReviewId";

        private static DataTable CreatePegasysOpenObligationsDataTable()
            => CreateDataTable(
                "PegasysObligations442",
                new DataColumn(ReviewIdColumnName, typeof(int)),
                //dt.Columns.Add("SourceRowNumber", typeof(int)),
                new DataColumn("Region", typeof(int)),
                new DataColumn("Fund", typeof(string)),
                new DataColumn("Organization", typeof(string)),
                new DataColumn("Prog", typeof(string)),
                new DataColumn("Doc Type", typeof(string)), //renamed from the excel to prove the test
                new DataColumn("Doc Num", typeof(string)),
                new DataColumn("Itmz Ln Num", typeof(int)),
                new DataColumn("Actg Ln Num", typeof(int)),
                new DataColumn("Title", typeof(string)),
                new DataColumn("BBFY", typeof(string)),
                new DataColumn("EBFY", typeof(string)),
                new DataColumn("Acty", typeof(string)),
                new DataColumn("O/C", typeof(string)),
                new DataColumn("SOC", typeof(string)),
                new DataColumn("Project", typeof(string)),
                new DataColumn("Agreement", typeof(string)),
                new DataColumn("Contract Num", typeof(string)),
                new DataColumn("Bldg", typeof(string)),
                new DataColumn("Sys/Loc", typeof(string)),
                new DataColumn("Veh Tag", typeof(string)),
                new DataColumn("WI", typeof(string)),
                new DataColumn("Lease Number", typeof(string)),
                new DataColumn("Vendor Name", typeof(string)),
                new DataColumn("Actg Pd", typeof(string)),
                new DataColumn("Total Line", typeof(decimal)),
                new DataColumn("Commitments", typeof(decimal)),
                new DataColumn("Prepayments", typeof(decimal)),
                new DataColumn("Undel Orders", typeof(decimal)),
                new DataColumn("Rcpt", typeof(decimal)),
                new DataColumn("Accrual", typeof(decimal)),
                new DataColumn("Pend Payments", typeof(decimal)),
                new DataColumn("Pymts(In Transit)", typeof(decimal)),
                new DataColumn("Pymts(Confirmed)", typeof(decimal)),
                new DataColumn("Holdbacks", typeof(decimal)),
                new DataColumn("TAFS", typeof(string)),
                new DataColumn("DUNS #", typeof(string)),
                new DataColumn("Date of Last Activity", typeof(string)),
                new DataColumn("Days Since First Activity", typeof(int)),
                new DataColumn("Days Since Last Activity", typeof(int)),
                new DataColumn("Date of First Activity", typeof(string)),
                new DataColumn("Trading Partner Type", typeof(string)),
                new DataColumn("Vendor Agency Code", typeof(string)),
                new DataColumn("Vendor Bureau Code", typeof(string))
                );

        private static DataTable CreateRetaDataTable()
            => CreateDataTable(
                "Reta",
                new DataColumn(ReviewIdColumnName, typeof(int)),
                new DataColumn("Agreement", typeof(int)),
                new DataColumn("RWA Type", typeof(string)),
                new DataColumn("Completion Date", typeof(DateTime)),
                new DataColumn("Agency Bureau Code", typeof(string)),
                new DataColumn("RETA POC", typeof(string)),
                new DataColumn("Region", typeof(int)),
                new DataColumn("Fund", typeof(string)),
                new DataColumn("Organization", typeof(string)),
                new DataColumn("Prog", typeof(string)),
                new DataColumn("Doc Type", typeof(string)), //renamed from the excel to prove the test
                new DataColumn("Doc Num", typeof(string)),
                new DataColumn("Itmz Ln Num", typeof(int)),
                new DataColumn("Actg Ln Num", typeof(int)),
                new DataColumn("Title", typeof(string)),
                new DataColumn("BBFY", typeof(string)),
                new DataColumn("EBFY", typeof(string)),
                new DataColumn("Acty", typeof(string)),
                new DataColumn("O/C", typeof(string)),
                new DataColumn("SOC", typeof(string)),
                new DataColumn("Project", typeof(string)),
                //dt.Columns.Add("Agreement", typeof(string)),
                new DataColumn("Contract Num", typeof(string)),
                new DataColumn("Bldg", typeof(string)),
                new DataColumn("Sys/Loc", typeof(string)),
                new DataColumn("Veh Tag", typeof(string)),
                new DataColumn("WI", typeof(string)),
                new DataColumn("Lease Number", typeof(string)),
                new DataColumn("Vendor Name", typeof(string)),
                new DataColumn("Actg Pd", typeof(string)),
                new DataColumn("Total Line", typeof(decimal)),
                new DataColumn("Commitments", typeof(decimal)),
                new DataColumn("Prepayments", typeof(decimal)),
                new DataColumn("Undel Orders", typeof(decimal)),
                new DataColumn("Rcpt", typeof(decimal)),
                new DataColumn("Accrual", typeof(decimal)),
                new DataColumn("Pend Payments", typeof(decimal)),
                new DataColumn("Pymts(In Transit)", typeof(decimal)),
                new DataColumn("Pymts(Confirmed)", typeof(decimal)),
                new DataColumn("Holdbacks", typeof(decimal)),
                new DataColumn("TAFS", typeof(string)),
                new DataColumn("DUNS #", typeof(string)),
                new DataColumn("Date of Last Activity", typeof(DateTime)),
                new DataColumn("Days Since First Activity", typeof(int)),
                new DataColumn("Days Since Last Activity", typeof(int)),
                new DataColumn("Date of First Activity", typeof(DateTime)),
                new DataColumn("Trading Partner Type", typeof(string)),
                new DataColumn("Vendor Agency Code", typeof(string)),
                new DataColumn("Vendor Bureau Code", typeof(string))
                );

        private static DataTable CreateEasiDataTable()
            => CreateDataTable(
                "EASI",
                new DataColumn(ReviewIdColumnName, typeof(int)),
                //dt.Columns.Add("SourceRowNumber", typeof(int)),
                new DataColumn("Created by users Region", typeof(string)),
                new DataColumn("Office", typeof(string)),
                new DataColumn("Created By (User Full Name)", typeof(string)),
                new DataColumn("Contracting Officers Name", typeof(string)),
                new DataColumn("Contracting Userid", typeof(string)),
                new DataColumn("Awd Obligated Amt", typeof(string)),
                new DataColumn("Current Awd Amt", typeof(string)),
                new DataColumn("Document Nbr", typeof(string)),
                new DataColumn("Award Nbr", typeof(string)),
                new DataColumn("Base Contract Nbr", typeof(string)),
                new DataColumn("GSA FSS/Other #", typeof(string)),
                new DataColumn("Vendor Name", typeof(string)),
                new DataColumn("Status Cd", typeof(string)),
                new DataColumn("Status Ds", typeof(string)),
                new DataColumn("Procurement Status", typeof(string)),
                new DataColumn("Create Dt", typeof(DateTime)),
                new DataColumn("Signed On Date", typeof(DateTime)),
                new DataColumn("Awd Effective Dt", typeof(DateTime)),
                new DataColumn("Awd Expiration Dt", typeof(DateTime)),
                new DataColumn("CLIN ADN", typeof(string)),
                new DataColumn("ACT/PDN", typeof(string)),
                new DataColumn("Data Source", typeof(string)),
                new DataColumn("Region Cd", typeof(string)),
                new DataColumn("Contracting Officer Email", typeof(string)),
                new DataColumn("Contracting Specialist Name", typeof(string)),
                new DataColumn("Contracting Specialist Email", typeof(string)),
                new DataColumn("Budget Analyst Email", typeof(string))
                );

        private static DataTable Create192DataTable()
            => CreateDataTable(
                "PegasysObligations192",
                new DataColumn(ReviewIdColumnName, typeof(int)),
                new DataColumn("Region", typeof(string)),
                new DataColumn("Fund", typeof(string)),
                new DataColumn("Organization", typeof(string)),
                new DataColumn("Prog", typeof(string)),
                new DataColumn("Doc Type", typeof(string)), //renamed from the excel to prove the test
                new DataColumn("Doc Num", typeof(string)),
                new DataColumn("Itmz Ln Num", typeof(int)),
                new DataColumn("Actg Ln Num", typeof(int)),
                new DataColumn("Title", typeof(string)),
                new DataColumn("BBFY", typeof(string)),
                new DataColumn("EBFY", typeof(string)),
                new DataColumn("Acty", typeof(string)),
                new DataColumn("O/C", typeof(string)),
                new DataColumn("SOC", typeof(string)),
                new DataColumn("Project", typeof(string)),
                new DataColumn("Agreement", typeof(string)),
                new DataColumn("Contract Num", typeof(string)),
                new DataColumn("Bldg", typeof(string)),
                new DataColumn("Sys/Loc", typeof(string)),
                new DataColumn("Veh Tag", typeof(string)),
                new DataColumn("WI", typeof(string)),
                new DataColumn("Lease Number", typeof(string)),
                new DataColumn("Vendor Name", typeof(string)),
                new DataColumn("Actg Pd", typeof(string)),
                new DataColumn("Total Line", typeof(decimal)),
                new DataColumn("Commitments", typeof(decimal)),
                new DataColumn("Prepayments", typeof(decimal)),
                new DataColumn("Undel Orders", typeof(decimal)),
                new DataColumn("Rcpt", typeof(decimal)),
                new DataColumn("Accrual", typeof(decimal)),
                new DataColumn("Pend Payments", typeof(decimal)),
                new DataColumn("Pymts(In Transit)", typeof(decimal)),
                new DataColumn("Pymts(Confirmed)", typeof(decimal)),
                new DataColumn("Holdbacks", typeof(decimal)),
                new DataColumn("TAFS", typeof(string)),
                new DataColumn("DUNS #", typeof(string)),
                new DataColumn("Date of Last Activity", typeof(DateTime)),
                new DataColumn("Days Since First Activity", typeof(int)),
                new DataColumn("Days Since Last Activity", typeof(int)),
                new DataColumn("Date of First Activity", typeof(DateTime)),
                new DataColumn("Trading Partner Type", typeof(string)),
                new DataColumn("Vendor Agency Code", typeof(string)),
                new DataColumn("Vendor Bureau Code", typeof(string))
            );

        private static DataTable CreateCreditCardAliasDataTable()
            => CreateDataTable(
                "CreditCardAliases",
                new DataColumn(ReviewIdColumnName, typeof(int)),
                new DataColumn("Alias", typeof(string)),
                new DataColumn("ALIAS - No #", typeof(string)),
                new DataColumn("Cardholder Name", typeof(string))
                );

        private static DataTable CreatePegasysOpenItemsCreditCardsDataTable()
            => CreateDataTable(
                "PegasysOpenItemsCreditCards",
                new DataColumn(ReviewIdColumnName, typeof(int)),
                new DataColumn("Reg", typeof(string)),
                new DataColumn("OrgCode", typeof(string)),
                new DataColumn("Doc Num", typeof(string)),
                new DataColumn("BBFY", typeof(int)),
                new DataColumn("LNUM", typeof(int)),
                new DataColumn("UDO", typeof(decimal)),
                new DataColumn("Accrued", typeof(decimal)),
                new DataColumn("Payments", typeof(decimal)),
                new DataColumn("Holdback", typeof(decimal)),
                new DataColumn("Total Order", typeof(decimal)),
                new DataColumn("Fund", typeof(string)),
                new DataColumn("Vendor", typeof(string)),
                new DataColumn("RWA", typeof(string)),
                new DataColumn("BA", typeof(string)),
                new DataColumn("FuncCode", typeof(string)),
                new DataColumn("WorkItems", typeof(string)),
                new DataColumn("ProjNo", typeof(string)),
                new DataColumn("BldgNO", typeof(string)),
                new DataColumn("OC", typeof(string)),
                new DataColumn("SOC", typeof(string)),
                new DataColumn("DocStatus", typeof(string)),
                new DataColumn("Alias", typeof(string)),
                new DataColumn("UserId", typeof(string)),
                new DataColumn("Last Activity Date", typeof(DateTime)),
                new DataColumn("Days Since Last Activity", typeof(int)),
                new DataColumn("Contract", typeof(string))
                );

        private static DataTable CreateActiveCardholdersDataTable()
            => CreateDataTable(
                "ActiveCardholders",
                new DataColumn(ReviewIdColumnName, typeof(int)),
                new DataColumn("Program", typeof(string)),
                new DataColumn("Region", typeof(string)),
                new DataColumn("Service/Staff Office", typeof(string)),
                new DataColumn("Cardholder First Name", typeof(string)),
                new DataColumn("Cardholder Middle Name", typeof(string)),
                new DataColumn("Cardholder Last Name", typeof(string)),
                new DataColumn("Account e-mail Address", typeof(string))
                );
        #endregion
    }
}
