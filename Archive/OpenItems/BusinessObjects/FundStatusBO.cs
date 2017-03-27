using System.Collections.Generic;
using System.Linq;
using OpenItems.Data;
using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Configuration;
    using System.Collections;
    using System.Threading;
    using GSA.OpenItems;
    using Data;

    /// <summary>
    /// Summary description for FundStatus
    /// </summary>
    public class FundStatusBO
    {

        private readonly IFundStatusDataLayer Dal;

        public FundStatusBO(IFundStatusDataLayer dal)
        {
            Dal = dal;
        } 
        public void RecalculateFSReport(string FiscalYear, string Organization, string BusinessLine)
        {
            var obj = new FSReportBuilder();
            obj.FiscalYear = FiscalYear;
            obj.Organization = Organization;
            obj.BusinessLine = BusinessLine;
            var obj_thread = new Thread(new ThreadStart(obj.RebuildReport));
            obj_thread.Start();
        }

        public void RecalculateTotalsReviewReport(string FiscalYear)
        {
            var obj = new FSReportBuilder();
            obj.FiscalYear = FiscalYear;
            var obj_thread = new Thread(new ThreadStart(obj.RebuildTotalsReviewReport));
            obj_thread.Start();
        }

        public List<string> GetBAList()
        {
            return Dal.GetBALList().ToList();
        }

        public List<spGetFundsOrganizations_Result> GetOrganizationList()
        {
            //olga 07/26/10
            return Dal.GetFundsOrganizations().ToList();
        }

        public DataSet GetReportFunctionGroupList()
        {
            return Dal.GetReportFuncGroupList();
        }

        public List<string> GetSummaryFunctionsList()
        {
            return Dal.GetFundsSumFunctionList().ToList();
        }

        public List<spGetObjectClassCodeList_Result> GetObjectClassCodeList()
        {
            return Dal.GetObjectClassCodeList().ToList();
        }

        public List<spGetCostElementList_Result> GetCostElemList()
        {
            return Dal.GetCostElementList().ToList();
        }

        public List<spGetBusinessLineList_Result> GetBusinessLineList()
        {
            //olga 07/26/10
            return Dal.GetBusinessLineList().ToList();
        }

   
       public DataSet GetSearchResults(int iFundsViewMode, string sFiscalYear, string sBudgetActivity, out int iRecordsCount, out decimal dTotalAmount,
            object oOrgCode, object oBookMonth, object oGroupCD, object oSumFunction, object oOCCode, object oCostElem, object oDocNumber, int iMaxResultRecords, bool bGetAllRecords)
        {
            iRecordsCount = 0;
            dTotalAmount = 0;
            try
            {
                return Dal.GetSearchResults(iFundsViewMode, sFiscalYear, sBudgetActivity, out iRecordsCount,
                    out dTotalAmount, oOrgCode, oBookMonth, oGroupCD, oSumFunction, oOCCode, oCostElem, oDocNumber, iMaxResultRecords, bGetAllRecords);
            }
            catch (SqlException ex)
            {
                //"Could not find stored procedure ..."
                if (ex.Number == 2812)
                {
                    //send email to the system administrator:
                    (new EmailUtility()).SendAlertToSysAdmin("Automated Fund Status. Missing stored procedure for the Fiscal Year " + sFiscalYear + ". Function name: GetSearchResults.");

                    //give a message to the user:
                    throw new Exception("Application Error. Missing database objects for the Fiscal Year " + sFiscalYear + ". Please contact your System Administrator.");
                }
                else
                    throw ex;
            }
        }

       
        private static string GetBookMonthList(string sMonthToEnd)
        {
            var whole_list = "'10','11','12','01','02','03','04','05','06','07','08','09'";

            var position = whole_list.IndexOf(sMonthToEnd);

            if (position != -1)
                return whole_list.Substring(0, position + 3);
            else
                return "";
        }

        private static string GetBookMonthListByFirstMonth(string sMonthToStart)
        {
            var whole_list = "10,11,12,01,02,03,04,05,06,07,08,09";

            var position = whole_list.IndexOf(sMonthToStart);

            if (position != -1)
                return whole_list.Substring(position);
            else
                return "";
        }


        //TODO: Need to do this with proc
        public bool UpdateRWAProjection(string sFiscalYear, string sStartBookMonth, string sOrgCode, string sProjectionArray, int iUpdateUserID)
        {
            bool return_result;
            /**********************************************************************************************************/
            var conn = new SqlConnection(Settings.Default.DefaultConn);
            conn.Open();
            /*** begin transaction ***/
            var trans = conn.BeginTransaction();
            try
            {
                decimal projection;
                int func_group_code;
                string[] pp;

                var proj_arr = sProjectionArray.Split(new char[] { ';' });
                var month_list = GetBookMonthListByFirstMonth(sStartBookMonth);
                var month_arr = month_list.Split(new char[] { ',' });

                var dsState = Dal.GetEmptyRWAProjection();

                foreach (var bm in month_arr)
                {
                    foreach (var p in proj_arr)
                    {
                        if (p != "")
                        {
                            pp = p.Split(new char[] { '|' });
                            func_group_code = Int32.Parse(pp[0]);
                            projection = Decimal.Parse(pp[1]);

                            //add new row to the insert batch:
                            var drInsert = dsState.Tables[0].NewRow();
                            drInsert["FiscalYear"] = sFiscalYear;
                            drInsert["BookMonth"] = bm;
                            drInsert["OrgCode"] = sOrgCode;
                            drInsert["GROUP_CD"] = func_group_code;
                            drInsert["Amount"] = projection;
                            drInsert["UpdateUserID"] = iUpdateUserID;
                            dsState.Tables[0].Rows.Add(drInsert);
                        }
                    }
                }
                //then create array of SQLParameters for insert command:                               
                var fields = new ArrayList();
                var param1 = new SqlParameter("@FiscalYear", SqlDbType.VarChar);
                param1.SourceColumn = "FiscalYear";
                fields.Add(param1);
                var param2 = new SqlParameter("@BookMonth", SqlDbType.VarChar);
                param2.SourceColumn = "BookMonth";
                fields.Add(param2);
                var param3 = new SqlParameter("@OrgCode", SqlDbType.VarChar);
                param3.SourceColumn = "OrgCode";
                fields.Add(param3);
                var param4 = new SqlParameter("@GROUP_CD", SqlDbType.Int);
                param4.SourceColumn = "GROUP_CD";
                fields.Add(param4);
                var param5 = new SqlParameter("@Amount", SqlDbType.Money);
                param5.SourceColumn = "Amount";
                fields.Add(param5);
                var param6 = new SqlParameter("@UpdateUserID", SqlDbType.Int);
                param6.SourceColumn = "UpdateUserID";
                fields.Add(param6);


                /*** clear the previous state records ***/

                Dal.ClearRwaProjection(sFiscalYear, sOrgCode, month_list);

                /*** insert new records ***/
                var ds = new DataSet();
                var cmd = new SqlCommand("spFS_GetEmptyRWAProjection", conn);
                cmd.Transaction = trans;
                cmd.CommandType = CommandType.StoredProcedure;
                var adpt = new SqlDataAdapter(cmd);
                adpt.Fill(ds);
                ds.Merge(dsState);

                var CommandText = "INSERT INTO tblFS_RWAProjection (";
                foreach (SqlParameter param in fields)
                {
                    CommandText += param.SourceColumn + ",";
                }
                CommandText = CommandText.Substring(0, CommandText.Length - 1);
                CommandText += ") VALUES (";
                foreach (SqlParameter param in fields)
                {
                    CommandText += param.ParameterName + ",";
                }
                CommandText = CommandText.Substring(0, CommandText.Length - 1);
                CommandText += ");";
                adpt.InsertCommand = new SqlCommand(CommandText, conn);
                adpt.InsertCommand.Transaction = trans;
                foreach (SqlParameter param in fields)
                {
                    adpt.InsertCommand.Parameters.Add(param);
                }
                adpt.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
                adpt.Update(ds);

                /*** commit transaction ***/
                trans.Commit();
            }
            catch (SqlException ex)
            {
                trans.Rollback();
                return_result = false;
            }
            finally
            {
                conn.Close();
                return_result = true;
            }


            return return_result;
        }

        public void DeleteUserEntryRecord(int iEntryID, int iUpdateUserID)
        {
            Dal.DeleteEntryData(iEntryID, iUpdateUserID);
        }

        public void UpdateUserEntryData(int iUserEntryType, string sFiscalYear, string sBookMonth, string sOrganization, int iReportGroupCode,
                int iEntryID, string sDocNumber, decimal dAmount, string sExplanation, int iUpdateUserID)
        {

            //insert new record:
            if (iEntryID == 0)
            {
                Dal.InsertEntryData(iUserEntryType, sFiscalYear, sBookMonth, sOrganization, iReportGroupCode, sDocNumber, dAmount, sExplanation, iUpdateUserID);
            }
            //update existing record:
            else
            {
                Dal.UpdateEntryData(iEntryID, sDocNumber, dAmount, sExplanation, iUpdateUserID);
            }
        }

        public DataSet GetUserEntryDataList(int iUserEntryType, string sFiscalYear, string sBookMonth, string sOrganization, string sBusinessLineCode, int iReportGroupCode, int iOneTimeAdjDataTypes)
        {
            try
            {
                DataSet ds = null;
                SqlParameter[] arrParams = null;
                var sp_name = "";

                if (iOneTimeAdjDataTypes == (int)OneTimeAdjDataTypes.PX_PJ)
                {
                    ds = Dal.GetAdjDocList(sFiscalYear, sOrganization, GetBookMonthList(sBookMonth), sBusinessLineCode, iReportGroupCode);
                }
                if (iOneTimeAdjDataTypes == (int)OneTimeAdjDataTypes.Awards)
                {
                    ds = Dal.GetAwardDocList(sFiscalYear, sOrganization, GetBookMonthList(sBookMonth), sBusinessLineCode);
                }
                if (iOneTimeAdjDataTypes == (int)OneTimeAdjDataTypes.Training)
                {
                    ds = Dal.GetTrainingDocList(sFiscalYear, sOrganization, GetBookMonthList(sBookMonth), sBusinessLineCode);
                }
                if (iOneTimeAdjDataTypes == (int)OneTimeAdjDataTypes.Travel)
                {
                    ds = Dal.GetTravelDocList(sFiscalYear, sOrganization, GetBookMonthList(sBookMonth), sBusinessLineCode);
                }

                return ds;
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                //"Could not find stored procedure ..."
                if (ex.Number == 2812)
                {
                    //send email to the system administrator:
                    (new EmailUtility()).SendAlertToSysAdmin("Automated Fund Status. Missing stored procedure for the Fiscal Year " + sFiscalYear + ". Function name: GetUserEntryDataList.");

                    //give a message to the user:
                    throw new Exception("Application Error. Missing database objects for the Fiscal Year " + sFiscalYear + ". Please contact your System Administrator.");
                }
                else
                    throw ex;
            }
        }

    }
}