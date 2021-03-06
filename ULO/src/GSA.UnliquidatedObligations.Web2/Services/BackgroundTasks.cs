﻿using System;
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.AspNetCore.Services;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.ApplicationParts;
using Traffk.StorageProviders;
using R = RevolutionaryStuff.Core;
using U = GSA.UnliquidatedObligations.Utility;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class BackgroundTasks : BaseLoggingDisposable, IBackgroundTasks
    {
        private readonly IEmailServer EmailServer;
        private readonly UloDbContext DB;
        private readonly IWorkflowManager WorkflowManager;
        private readonly IConnectionStringProvider ConnectionStringProvider;
        private readonly SpecialFolderProvider SpecialFolderProvider;
        private readonly ITemplateProcessor RazorTemplateProcessor;
        private readonly IReportRunner ReportRunner;
        private readonly IOptions<Config> ConfigOptions;
        private readonly UserHelpers UserHelpers;
        private readonly PortalHelpers PortalHelpers;
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
            public ImportConfig WorkingCapitalFundReportConfig { get; set; }
            public ImportConfig ActiveCardholderImportConfig { get; set; }

            public int AssignWorkFlowsBatchSize { get; set; } = 500;

            public class ImportConfig
            { 
                public string SheetsCsv { get; set; }
                public int SkipRawRows { get; set; }
            }
        }

        public BackgroundTasks(IConnectionStringProvider connectionStringProvider, SpecialFolderProvider specialFolderProvider, RazorTemplateProcessor razorTemplateProcessor, IReportRunner reportRunner, IOptions<Config> configOptions, UserHelpers userHelpers, IEmailServer emailServer, UloDbContext db, IWorkflowManager workflowManager, ILogger<BackgroundTasks> logger, PortalHelpers portalHelpers)
            : base(logger)
        {
            ConnectionStringProvider = connectionStringProvider;
            SpecialFolderProvider = specialFolderProvider;
            RazorTemplateProcessor = razorTemplateProcessor;
            ReportRunner = reportRunner;
            ConfigOptions = configOptions;
            UserHelpers = userHelpers;
            EmailServer = emailServer;
            DB = db;
            WorkflowManager = workflowManager;
            PortalHelpers = portalHelpers;
        }

        async Task IBackgroundTasks.EmailReport(string[] recipients, string subjectTemplate, string bodyTemplate, string htmlBodyTemplate, object model, string reportName, IDictionary<string, string> paramValueByParamName)
        {
            var res = await ReportRunner.ExecuteAsync(reportName, paramValueByParamName);
            await EmailAsync(recipients, subjectTemplate, bodyTemplate, htmlBodyTemplate, model, new[] { res });
        }

        private Microsoft.Data.SqlClient.SqlConnection CreateSqlConnection()
            => new Microsoft.Data.SqlClient.SqlConnection(ConnectionStringProvider.GetConnectionString(ConfigOptions.Value.ConnectionStringName));

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
        public async Task UploadFiles(UploadFilesModel model)
        {
            var reviewId = model.ReviewId;
            var folder = await SpecialFolderProvider.GetReviewFolderAsync(model.ReviewId);
            string importer = null;
            int rowErrorCount = 0;

            Action<Exception, int> onRowAddError = (ex, rowNum) =>
            {
                LogError(ex, "UploadFiles: OnRowAddError with {rowNum} in {importer} with {reviewId}", rowNum, importer, reviewId);
                ++rowErrorCount;
            };

            RowsTransferredEventHandler rowsTransferred = (sender, e) =>
            {
                LogInformation("UploadFiles: Loading progress. {rowCount} in {importer} with {reviewId}", e.RowsTransferred, importer, reviewId);
            };

            try
            {
                var allFiles =
                    model.PegasysFilePathsList
                    .Union(model.RetaFileList)
                    .Union(model.EasiFileList)
                    .Union(model.One92FileList)
                    .Union(model.CreditCardAliasCrosswalkFiles)
                    .Union(model.PegasysOpenItemsCreditCards)
                    .Union(model.WorkingCapitalFundReportFiles)
                    .Union(model.ActiveCardholderFiles)
                    .ToList();
                LogInformation("Will ultimately process the following files:\n{filePaths}", allFiles.Format("\n", "\t{0}"));

                importer = nameof(Load442sAsync);
                foreach (var fn in model.PegasysFilePathsList)
                {
                    using (var st = await folder.OpenFileReadStreamAsync(fn))
                    {
                        await Load442sAsync(reviewId, st, onRowAddError, rowsTransferred);
                    }
                }

                importer = nameof(LoadReta);
                foreach (var fn in model.RetaFileList)
                {
                    using (var st = await folder.OpenFileReadStreamAsync(fn))
                    {
                        LoadReta(reviewId, st, onRowAddError, rowsTransferred);
                    }
                }

                importer = nameof(LoadEasi);
                foreach (var fn in model.EasiFileList)
                {
                    using (var st = await folder.OpenFileReadStreamAsync(fn))
                    {
                        LoadEasi(reviewId, st, onRowAddError, rowsTransferred);
                    }
                }

                importer = nameof(Load192s);
                foreach (var fn in model.One92FileList)
                {
                    using (var st = await folder.OpenFileReadStreamAsync(fn))
                    {
                        Load192s(reviewId, st, onRowAddError, rowsTransferred);
                    }
                }

                importer = nameof(LoadCreditCardAliases);
                foreach (var fn in model.CreditCardAliasCrosswalkFiles)
                {
                    using (var st = await folder.OpenFileReadStreamAsync(fn))
                    {
                        LoadCreditCardAliases(reviewId, st, onRowAddError, rowsTransferred);
                    }
                }

                importer = nameof(LoadPegasysOpenItemsCreditCards);
                foreach (var fn in model.PegasysOpenItemsCreditCards)
                {
                    using (var st = await folder.OpenFileReadStreamAsync(fn))
                    {
                        LoadPegasysOpenItemsCreditCards(reviewId, st, onRowAddError, rowsTransferred);
                    }
                }

                importer = nameof(LoadCreateWorkingCapitalFundItems);
                foreach (var fn in model.WorkingCapitalFundReportFiles)
                {
                    using (var st = await folder.OpenFileReadStreamAsync(fn))
                    {
                        LoadCreateWorkingCapitalFundItems(reviewId, st, onRowAddError, rowsTransferred);
                    }
                }

                importer = nameof(LoadActiveCardholders);
                foreach (var fn in model.ActiveCardholderFiles)
                {
                    using (var st = await folder.OpenFileReadStreamAsync(fn))
                    {
                        LoadActiveCardholders(reviewId, st, onRowAddError, rowsTransferred);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Problem with UploadFiles in {reviewId}", reviewId);
                throw;
            }
            finally
            {
                var level = rowErrorCount == 0 ? LogLevel.Information : LogLevel.Warning;
                Logger.Log(level, "Importing of {reviewId} yielded {rowErrorCount}.  Note, this does not indicate either overall success or failure.", reviewId, rowErrorCount);
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
                LogError(ex,
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
                LogError(ex,
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
                LogError(ex,
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
                    LogInformation("AssignWorkFlows could not find {reviewId}", reviewId);
                    return;
                }
                review.Status = Review.StatusNames.Assigning;
                await DB.SaveChangesAsync();

                var workflows =
                    DB.Workflows.Include(wf => wf.TargetUlo).Include(wf => wf.OwnerUser).
                    Where(wf => wf.OwnerUserId == UserHelpers.PreAssignmentUserUserId && wf.TargetUlo.ReviewId==reviewId).
                    Take(config.AssignWorkFlowsBatchSize).
                    //OrderBy(wf => wf.TargetUlo.ReviewId == reviewId ? 0 : 1).
                    ToList();

                LogInformation("AssignWorkFlows {reviewId} assigning up to {totalRecords} with {batchSize}", reviewId, workflows.Count, config.AssignWorkFlowsBatchSize);

                int z = 0;
                foreach (var workflow in workflows)
                {
                    await WorkflowManager.AdvanceAsync(workflow, null, null, true, true, !sendBatchNotifications);
                    if (++z % 10 == 0)
                    {
                        await DB.SaveChangesAsync();
                        LogDebug("AssignWorkFlows {reviewId} save after {recordsProcessed}/{totalRecords}", reviewId, z, workflows.Count);
                        var r = await DB.Reviews.FindAsync(reviewId);
                        if (r == null)
                        {
                            LogInformation("AssignWorkFlows cancelled as {reviewId} has been deleted", reviewId);
                            break;
                        }
                    }
                }
                review.SetStatusDependingOnClosedBit();
                await DB.SaveChangesAsync();
                LogInformation("AssignWorkFlows {reviewId} completed after {recordsProcessed}/{totalRecords}", reviewId, z, workflows.Count);

                if (z > 0)
                {
                    BackgroundJob.Enqueue<IBackgroundTasks>(bt => bt.AssignWorkFlows(reviewId, sendBatchNotifications));
                    if (config.SendBatchEmailsDuringAssignWorkflows)
                    {
                        BackgroundJob.Enqueue<IBackgroundTasks>(bt => bt.SendAssignWorkFlowsBatchNotifications(reviewId));
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex,
                    "Problem in {methodName}",
                    nameof(IBackgroundTasks.AssignWorkFlows));
                throw;
            }
        }

        #region Loaders

        private void LoadExcelDataTable(int reviewId, Stream st, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred, Config.ImportConfig importConfig, Func<DataTable> dataTableCreator)
        {
            string sheetNamesCsv = importConfig.SheetsCsv;
            int skipRawRows = importConfig.SkipRawRows;

            DataTable dt;
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

            FinalizeAndUpload(dt, reviewId, rowsTransferred);
        }

        private async Task Load442sAsync(int reviewId, Stream st, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
        {
            var dt = CreatePegasysOpenObligationsDataTable();
            await dt.LoadRowsFromDelineatedTextAsync(st, new LoadRowsFromDelineatedTextSettings
            {
                SkipRawRows = 2,
                RowAddErrorHandler = onRowAddError
            });
            dt.SetColumnWithValue(ReviewIdColumnName, reviewId);
            dt.UploadIntoSqlServer(CreateSqlConnection, new UploadIntoSqlServerSettings { RowsTransferredEventHandler = rowsTransferred });
        }

        private void LoadReta(int reviewId, Stream st, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
             => LoadExcelDataTable(reviewId, st, onRowAddError, rowsTransferred, ConfigOptions.Value.RetaImportConfig, CreateRetaDataTable);

        private void LoadEasi(int reviewId, Stream st, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
        {
            DataTable dt;
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
            FinalizeAndUpload(dt, reviewId, rowsTransferred);
        }


        private void Load192s(int reviewId, Stream st, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
            => LoadExcelDataTable(reviewId, st, onRowAddError, rowsTransferred, ConfigOptions.Value.Upload192ImportConfig, Create192DataTable);

        private void LoadCreditCardAliases(int reviewId, Stream st, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
            => LoadExcelDataTable(reviewId, st, onRowAddError, rowsTransferred, ConfigOptions.Value.CreditCardAliasConfig, CreateCreditCardAliasDataTable);

        private void LoadPegasysOpenItemsCreditCards(int reviewId, Stream st, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
            => LoadExcelDataTable(reviewId, st, onRowAddError, rowsTransferred, ConfigOptions.Value.PegasysOpenItemsCreditCardsSheetsConfig, CreatePegasysOpenItemsCreditCardsDataTable);

        private void LoadCreateWorkingCapitalFundItems(int reviewId, Stream st, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
            => LoadExcelDataTable(reviewId, st, onRowAddError, rowsTransferred, ConfigOptions.Value.WorkingCapitalFundReportConfig, CreateWorkingCapitalFundItemsDataTable);

        private void LoadActiveCardholders(int reviewId, Stream st, Action<Exception, int> onRowAddError, RowsTransferredEventHandler rowsTransferred)
            => LoadExcelDataTable(reviewId, st, onRowAddError, rowsTransferred, ConfigOptions.Value.ActiveCardholderImportConfig, CreateActiveCardholdersDataTable);

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

        private static DataTable CreateWorkingCapitalFundItemsDataTable()
            => CreateDataTable(
                "WorkingCapitalFundItems",
                new DataColumn(ReviewIdColumnName, typeof(int)),
                new DataColumn("Region", typeof(string)), // tinyint NOT NULL,
                new DataColumn("Fund", typeof(string)), // varchar(4) NOT NULL,
                new DataColumn("Organization", typeof(string)), // varchar(18) NOT NULL,
                new DataColumn("Prog", typeof(string)), // varchar(4) NULL,
                new DataColumn("Doc Type", typeof(string)), // varchar(3) NULL,
                new DataColumn("Doc Num", typeof(string)), // varchar(16) NOT NULL,
                new DataColumn("Itmz Ln Num", typeof(int)), // bit NULL,
                new DataColumn("Actg Ln Num", typeof(int)), // tinyint NULL,
                new DataColumn("Title", typeof(string)), // varchar(50) NULL,
                new DataColumn("BBFY", typeof(int)), // smallint NOT NULL,
                new DataColumn("EBFY", typeof(int)), // bit NULL,
                new DataColumn("Acty", typeof(string)), // varchar(5) NULL,
                new DataColumn("O/C", typeof(int)), // tinyint NULL,
                new DataColumn("SOC", typeof(string)), // varchar(3) NULL,
                new DataColumn("Project", typeof(string)), // varchar(8) NULL,
                new DataColumn("Agreement", typeof(string)), // varchar(18) NULL,
                new DataColumn("Contract Num", typeof(string)), // varchar(28) NULL,
                new DataColumn("Bldg", typeof(string)), // varchar(8) NULL,
                new DataColumn("Sys/Loc", typeof(string)), // bit NULL,
                new DataColumn("Veh Tag", typeof(string)), // bit NULL,
                new DataColumn("WI", typeof(int)), // bit NULL,
                new DataColumn("Lease Number", typeof(string)), // int NULL,
                new DataColumn("Vendor Name", typeof(string)), // varchar(62) NULL,
                new DataColumn("Actg Pd", typeof(string)), // varchar(7) NULL,
                new DataColumn("Total Line", typeof(double)), // float NOT NULL,
                new DataColumn("Commitments", typeof(double)), // float NOT NULL,
                new DataColumn("Prepayments", typeof(double)), // float NOT NULL,
                new DataColumn("Undel Orders", typeof(double)), // float NOT NULL,
                new DataColumn("Rcpt", typeof(double)), // float NOT NULL,
                new DataColumn("Accrual", typeof(double)), // float NOT NULL,
                new DataColumn("Pend Payments", typeof(double)), // float NOT NULL,
                new DataColumn("Pymts(In Transit)", typeof(double)), // float NOT NULL,
                new DataColumn("Pymts(Confirmed)", typeof(double)), // float NOT NULL,
                new DataColumn("Holdbacks", typeof(double)), // bit NOT NULL,
                new DataColumn("TAFS", typeof(string)), // varchar(11) NOT NULL,
                new DataColumn("DUNS #", typeof(string)), // varchar(14) NULL,
                new DataColumn("Date of Last Activity", typeof(DateTime)), // varchar(10) NULL,
                new DataColumn("Days Since First Activity", typeof(int)), // smallint NOT NULL,
                new DataColumn("Days Since Last Activity", typeof(int)), // int NOT NULL,
                new DataColumn("Date of First Activity", typeof(DateTime)), // int NULL,
                new DataColumn("Trading Partner Type", typeof(string)), // varchar(1) NULL,
                new DataColumn("Vendor Agency Code", typeof(string)), // varchar(3) NULL,
                new DataColumn("Vendor Bureau Code", typeof(string)) // tinyint NULL
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
