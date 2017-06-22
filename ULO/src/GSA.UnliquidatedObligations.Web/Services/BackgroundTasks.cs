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
using GSA.UnliquidatedObligations.Web.Models;

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


        public void UploadFiles(UploadFilesModel files)
        {
            var reviewId = files.ReviewId;

            foreach (var csvFilePath in files.PegasysFilePathsList)
            {
                UploadCSVTable(reviewId, csvFilePath);
            }

            foreach (var retaFilePath in files.RetaFileList)
            {
                UploadRetaTable(reviewId, retaFilePath);
            }

            foreach (var easiFile in files.EasiFileList)
            {
                UploadEasiTable(reviewId, easiFile);
            }

            foreach (var One92File in files.One92FileList)
            {
                Upload192Table(reviewId, One92File);
            }
        }



        public void CreateULOsAndAssign(int reviewId, int workflowDefinitionId, DateTime? reviewDate)
        {
            DB.Database.CommandTimeout = 600;
            DB.CreateULOAndAssignWf(reviewId, workflowDefinitionId, reviewDate);

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

        private void UploadCSVTable(int reviewId, string uploadPath)
        {
            var connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var dt = CreatePegasysOpenObligationsDataTable();
            using (var st = File.OpenRead(uploadPath))
            {
                dt.LoadRowsFromDelineatedText(st, new LoadRowsFromDelineatedTextSettings
                {
                    SkipRawRows = 2,
                    RowAddErrorHandler = DataTableHelpers.RowAddErrorIgnore
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

        private void UploadRetaTable(int reviewId, string uploadPath)
        {
            DataTable dt;
            var connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var st = File.OpenRead(uploadPath))
            {
                var ds = new DataSet();
                var settings = new LoadSheetsFromExcelSettings()
                {
                    SheetSettings = new List<LoadRowsFromExcelSettings>(),
                    CreateDataTable = CreateRetaDataTable
                };

                foreach (var sheetName in new[] { "R.00", "R.01", "R.02", "R.03", "R.04", "R.05", "R.06", "R.07", "R.08", "R.09", "R.10", "R.11" })
                {
                    settings.SheetSettings.Add(
                    new LoadRowsFromExcelSettings
                    {
                        SheetName = sheetName,
                        TypeConverter = DataTableHelpers.ExcelTypeConverter,
                        UseSheetNameForTableName = true,
                        RowAddErrorHandler = DataTableHelpers.RowAddErrorIgnore
                    });
                }

                ds.LoadSheetsFromExcel(st, settings);




                dt = CreateRetaDataTable();

                for (int z = 0; z < ds.Tables.Count; ++z)
                {
                    dt.Append(ds.Tables[z]);
                }
                //Stuff.Noop(ds);
            }

            dt.SetColumnWithValue("ReviewId", reviewId);
            dt.UploadIntoSqlServer(
                delegate ()
                {
                    return new System.Data.SqlClient.SqlConnection(connString);
                },
                new UploadIntoSqlServerSettings
                {
                    GenerateTable = false
                });
        }

        private void UploadEasiTable(int reviewId, string uploadPath)
        {
            DataSet ds;
            DataTable dt;
            var connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var st = File.OpenRead(uploadPath))
            {
                ds = new DataSet();
                var settings = new LoadSheetsFromExcelSettings()
                {
                    CreateDataTable = CreateEasiTable,

                };
                ds.LoadSheetsFromExcel(st, settings);


                dt = CreateEasiTable();
                //dt.TableName = "All Regions";
                for (int z = 0; z < ds.Tables.Count; ++z)
                {
                    dt.Append(ds.Tables[z]);
                }
                //Stuff.Noop(ds);
            }

            dt.SetColumnWithValue("ReviewId", reviewId);
            dt.MakeDateColumnsFitSqlServerBounds();
            dt.UploadIntoSqlServer(
                delegate ()
                {
                    return new System.Data.SqlClient.SqlConnection(connString);
                },
                new UploadIntoSqlServerSettings
                {
                    GenerateTable = false
                }
                );
        }

        private void Upload192Table(int reviewId, string uploadPath)
        {
            var connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            DataSet ds;
            DataTable dt;
            using (var st = File.OpenRead(uploadPath))
            {
                ds = new DataSet();
                var settings = new LoadSheetsFromExcelSettings()
                {
                    SheetSettings = new List<LoadRowsFromExcelSettings>(),
                    CreateDataTable = Create192Table,
                };
                foreach (var sheetName in new[] { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11" })
                {
                    settings.SheetSettings.Add(
                    new LoadRowsFromExcelSettings
                    {
                        SkipRawRows = 3,
                        SheetName = sheetName,
                        TypeConverter = DataTableHelpers.ExcelTypeConverter,
                        UseSheetNameForTableName = true,
                        RowAddErrorHandler = DataTableHelpers.RowAddErrorIgnore
                    });
                }
                ds.LoadSheetsFromExcel(st, settings);


                dt = Create192Table();
                //dt.TableName = "AllRegions";
                for (int z = 0; z < ds.Tables.Count; ++z)
                {
                    dt.Append(ds.Tables[z]);
                }
            }

            dt.SetColumnWithValue("ReviewId", reviewId);
            dt.MakeDateColumnsFitSqlServerBounds();
            dt.UploadIntoSqlServer(
                delegate ()
                {
                    return new System.Data.SqlClient.SqlConnection(connString);
                },
                new UploadIntoSqlServerSettings
                {
                    GenerateTable = false
                });

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
            dt.Columns.Add("Completion Date", typeof(string));
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
            return dt;
        }

        private static DataTable Create192Table()
        {
            var dt = new DataTable()
            {
                TableName = "PegasysObligations192"
            };
            dt.Columns.Add("ReviewId", typeof(int));
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
