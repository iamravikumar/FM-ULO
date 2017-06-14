using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RazorEngine;
using RazorEngine.Templating;
using System.Linq;
using System;
using System.Data;
using System.IO;
using GSA.UnliquidatedObligations.UploadTable;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Autofac;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class BackgroundTasks : IBackgroundTasks
    {
        private readonly IEmailServer EmailServer;
        private readonly ULODBEntities DB;
        private readonly IWorkflowManager WorkflowManager;

        public BackgroundTasks(IEmailServer emailServer, ULODBEntities db, IWorkflowManager workflowManager)
        {
            EmailServer = emailServer;
            DB = db;
            WorkflowManager = workflowManager;
        }

        public void Email(string subject, string recipient, string template, object model)
        {
            var compiledEmailBody = Engine.Razor.RunCompile(template, "email", null, model);
            EmailServer.SendEmail(subject, compiledEmailBody, recipient);
        }



        public void UploadReviewHoldIngTable(int reviewId, string uploadPath)
        {
            var connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var dt = CreatePegasysOpenObligationsDataTable();
            using (var st = File.OpenRead(uploadPath))
            {
                dt.LoadRowsFromDelineatedText(st, new LoadRowsFromDelineatedTextSettings
                {
                    SkipRawRows = 2,
                    RowNumberColumnName = "SourceRowNumber",
                    ColumnMapper =
                        DataTableHelpers.CreateDictionaryMapper(
                            new Dictionary<string, string>(Comparers.CaseInsensitiveStringComparer)
                            {
                                {"Doc Type", "DocType"}
                            }, true)
                });
                dt.SetColumnWithValue("ReviewId", reviewId);
                dt.UploadIntoSqlServer(
                    delegate ()
                    {
                        return
                            new System.Data.SqlClient.SqlConnection(connString);
                    },
                    new UploadIntoSqlServerSettings
                    {
                        GenerateTable = false,
                        RowsCopiedNotifyIncrement = 1

                    });
            }
        }

        private static DataTable CreatePegasysOpenObligationsDataTable()
        {
            var dt = new DataTable()
            {
                TableName = "PegasysObligations"
            };
            dt.Columns.Add("ReviewId", typeof(int));
            dt.Columns.Add("SourceRowNumber", typeof(int));
            dt.Columns.Add("Region", typeof(int));
            dt.Columns.Add("Fund", typeof(string));
            dt.Columns.Add("Organization", typeof(string));
            dt.Columns.Add("Prog", typeof(string));
            dt.Columns.Add("DocType", typeof(string)); //renamed from the excel to prove the test
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
            dt.Columns.Add("Total Line", typeof(string));
            dt.Columns.Add("Commitments", typeof(string));
            dt.Columns.Add("Prepayments", typeof(string));
            dt.Columns.Add("Undel Orders", typeof(string));
            dt.Columns.Add("Rcpt", typeof(string));
            dt.Columns.Add("Accrual", typeof(string));
            dt.Columns.Add("Pend Payments", typeof(string));
            dt.Columns.Add("Pymts(In Transit)", typeof(string));
            dt.Columns.Add("Pymts(Confirmed)", typeof(string));
            dt.Columns.Add("Holdbacks", typeof(string));
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

        public void CreateULOsAndAssign(int reviewId, int workflowDefinitionId)
        {
            DB.CreateULOAndAssignWf(reviewId, workflowDefinitionId);

        }

        public async Task AssignWorkFlows(int reviewId)
        {
            var workflows = DB.Workflows.Where(wf => wf.UnliquidatedObligation.ReviewId == reviewId).ToList();

            foreach (var workflow in workflows)
            {
                await WorkflowManager.AdvanceAsync(workflow, null, true);
                await DB.SaveChangesAsync();

            }
            
        }
    }
}
