using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.OleDb;
    using System.Configuration;
    using Data;

    /// <summary>
    /// Summary description for UploadService
    /// </summary>
    public class UploadServiceBO
    {

        private readonly IUploadServiceDataLayer Dal;
        public UploadServiceBO(IUploadServiceDataLayer dal)
        {
            Dal = dal;
        }


        public void LoadData(string sExcelFilePath, int iFileID, string sFileName, int iDataSource,
            int iOpenItemsType, DateTime dtDueDate, int iParentLoadID, int iReviewRound, int iUpdateUser, string sLoadName, DateTime dtReportDate)
        {
            var da = new DataAccess(Settings.Default.DefaultConn);

            try
            {
                string sSQLTable;
                string sWorkbook;
                // !!!!!!!! commented out per Olga !!!!!!! //
                /*******************************************************************************************/
                ////first of all we need to delete previous data from the tables,
                ////that serve as temporary for importing data from Excel:     
                //DataAccess da = new DataAccess(ConfigurationManager.ConnectionStrings["DefaultConn"].ConnectionString);
                //da.ExecuteCommand("spEmptyTablesForImport");
                //da.CloseConnection();

                //if (ParentLoadID > 0)
                //{
                //    //load feedback file from the Central Office:
                //    /*******************************************************************************************/
                //    //import open items records:
                //    sSQLTable = ConfigurationSettings.AppSettings["SQLTableNameForImport_FeedbackList"];
                //    sWorkbook = ConfigurationSettings.AppSettings["ExcelWorkSheetName_FeedbackList"];

                //    ImportFromExcel(ExcelFilePath, sWorkbook, sSQLTable);

                //}
                //else
                //{
                //    //regular new data load:                    
                //    /*******************************************************************************************/

                //    //import main open items records:
                //    sSQLTable = ConfigurationSettings.AppSettings["SQLTableNameForImport_MainList"];
                //    sWorkbook = ConfigurationSettings.AppSettings["ExcelWorkSheetName_MainList"];

                //    ImportFromExcel(ExcelFilePath, sWorkbook, sSQLTable);

                //    //import detailed lines records:
                //    sSQLTable = ConfigurationSettings.AppSettings["SQLTableNameForImport_DetailsList"];
                //    sWorkbook = ConfigurationSettings.AppSettings["ExcelWorkSheetName_DetailsList"];

                //    ImportFromExcel(ExcelFilePath, sWorkbook, sSQLTable);
                //}

                da.BeginTrans();

                if (iParentLoadID == 0)
                {
                    iReviewRound = 1;
                }

                var load_id = GetNewLoadID(iDataSource, iOpenItemsType, dtDueDate, iFileID, iParentLoadID, iReviewRound, sLoadName);

                /*******************************************************************************************/
                //now we can format imported data and insert it into the application tables:
                if (iParentLoadID > 0)
                {
                    //load feedback file from the Central Office:
                    InsertData_FeedbackLoad(load_id, iParentLoadID);
                }
                else
                {
                    if (iOpenItemsType == 6)//BA53
                    {
                        //regular new BA53 data load: 
                        InsertData_BA53Load(load_id, iOpenItemsType, dtReportDate);
                    }
                    else
                    {
                        //regular new data load:                                     
                        InsertData_RegularLoad(load_id, iOpenItemsType);
                    }
                }
                /*******************************************************************************************/
                //insert History record
                var email_request = 0;

                if (Settings.Default.SendEmailOnNewLoad)
                    email_request = InsertEmailRequest(iUpdateUser, (int)HistoryActions.haUploadData, true);

                History.InsertHistoryOnUploadData(load_id, iUpdateUser, sFileName, email_request);

                da.CommitTrans();

            }
            catch (Exception ex)
            {
                da.RollbackTrans();
            }
        }

        private int GetNewLoadID(int iDataSource, int iOpenItemsType, DateTime dtDueDate, int iFileID, int iParentLoadID, int iReviewRound, string sLoadName)
        {
            return (int)Dal.InsertNewLoad(iDataSource, iOpenItemsType, dtDueDate, iFileID, iParentLoadID, iReviewRound, sLoadName);
        }

        private void InsertData_FeedbackLoad(int iLoadID, int iParentLoadID)
        {
            Dal.InsertReviewFeedback(iLoadID, iParentLoadID);

        }

        private void InsertData_RegularLoad(int iLoadID, int iOpenItemsType)
        {

            //1. insert into main table
            Dal.InsertOIMain(iLoadID, iOpenItemsType);

            //2. insert into details table
            Dal.InsertOIDetails(iLoadID);

            //3. route items to users
            Dal.InsertOIOrganization(iLoadID);
        }
        private void InsertData_BA53Load(int iLoadID, int iOpenItemsType, DateTime dtReportDate)
        {
            //1. insert into main table
            Dal.InsertOIMain(iLoadID, iOpenItemsType);

            //2. insert into details table
            Dal.InsertOIDetails(iLoadID);

            //3. route items to users
            Dal.InsertOIOrganization(iLoadID);

            //4. insert into Lease table
            Dal.InsertOILease(iLoadID, dtReportDate);
        }

        private int InsertEmailRequest(int iCurrentUserID, int iHistoryAction, bool bSendNow)
        {
            return (int)Dal.InsertEmailRequest(iCurrentUserID, iHistoryAction, bSendNow);
        }
    }
}