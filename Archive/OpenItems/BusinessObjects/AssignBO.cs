using System.Collections.Generic;
using System.Linq;
using Data;
using OpenItems.Data;
using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data; 
    using System.Data.SqlClient;
    using System.Configuration;

    /// <summary>
    /// Summary description for AssignBO
    /// </summary>
    public class AssignBO
    {
        private readonly IAssignDataLayer Dal;
        private readonly ItemBO Item;

        public AssignBO(IAssignDataLayer dal, ItemBO item)
        {
            Dal = dal;
            Item = item;
        }

        public void VerifyAssignment(int OItemID, int LoadID, string OrgCode, int ReviewerUserID)
        {

            if (Settings.Default.WriteExtendedLog)
            {
                //ApplicationAssert app_log = new ApplicationAssert();
                var str = String.Format("{0:MM/dd/yyyy hh:mm}   On VerifyAssignment: LoadID {1}, OItemID {2}, OrgCode {3}, ReviewerUserID {4}",
                    DateTime.Now, LoadID, OItemID, OrgCode, ReviewerUserID);
                Utility.WriteLogEntry(str);
            }

            Item.UpdateItemStatus(OItemID, LoadID, OrgCode, ReviewerUserID, (int)OpenItemStatus.stAssigned);                          
        }

        public int VerifyReroute(int RequestID)
        {
            //ApplicationAssert app_log = new ApplicationAssert();
            if (Settings.Default.WriteExtendedLog)
            {
                var str = String.Format("{0:MM/dd/yyyy hh:mm}   On VerifyReroute: RequestID {1}", DateTime.Now, RequestID);
                Utility.WriteLogEntry(str);
            }

            var return_code = Dal.VerifyItemReroute(RequestID, OpenItemStatus.stAssigned);

            if (Settings.Default.WriteExtendedLog)
            {
                var str = String.Format("{0:MM/dd/yyyy hh:mm}   After VerifyReroute: RequestID {1}, Return Code {2}", DateTime.Now, RequestID, return_code);
                Utility.WriteLogEntry(str);
            }
            return return_code;
        }


        public int RequestReassignItem(int iOItemID, string sOrgCode, string sLines, int iPrevReviewerUserID,
            string sResponsibleOrg, string sNewOrganization, string sNewOrgCode, int iNewReviewerUserID, string sComments)
        {
            //ApplicationAssert app_log = new ApplicationAssert();
            if (Settings.Default.WriteExtendedLog)
            {
                var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RequestReassignItem before calculating lines: OItemID {1}, OrgCode {2}, Lines {3}, PrevReviewerUserID {4}, ResponsibleOrg {5}, NewOrganization {6}, NewOrgCode {7}, NewReviewerUserID {8}, Comments: {9}",
                    DateTime.Now, iOItemID, sOrgCode, sLines, iPrevReviewerUserID, sResponsibleOrg, sNewOrganization, sNewOrgCode, iNewReviewerUserID, sComments);
                Utility.WriteLogEntry(str);
            }

            int return_value;
            var request_id = 0;

            var arr_lines = sLines.Split(new char[] { ',' });
            DataTable dt = null;
            DataRow[] dr;

            if (arr_lines.Length > 1)
            {
                //could be more than one ULOOrgCodes for these lines
                var dsLines = Item.GetULOOrganizationsByItemLines(iOItemID);
                if (dsLines != null && dsLines.Tables[0].Rows.Count > 1)
                    dt = dsLines.Tables[0];
            }

            if (Settings.Default.WriteExtendedLog)
            {
                var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RequestReassignItem before Transaction: OItemID {1}, OrgCode {2}, Lines {3}, PrevReviewerUserID {4}, ResponsibleOrg {5}, NewOrganization {6}, NewOrgCode {7}, NewReviewerUserID {8}",
                    DateTime.Now, iOItemID, sOrgCode, sLines, iPrevReviewerUserID, sResponsibleOrg, sNewOrganization, sNewOrgCode, iNewReviewerUserID);
                Utility.WriteLogEntry(str);
            }

           
            try
            {
                string ULOOrgCode;
                int ReviewerUserID;
                foreach (var line_num in arr_lines)
                {
                    if (dt == null)
                    {
                        ULOOrgCode = sOrgCode;
                        ReviewerUserID = iPrevReviewerUserID;
                    }
                    else
                    {
                        dr = dt.Select("ItemLNum = " + line_num);
                        if (dr.Length > 0)
                        {
                            ULOOrgCode = (string)dr[0]["ULOOrgCode"];
                            ReviewerUserID = (int)dr[0]["ReviewerUserID"];
                        }
                        else
                        {
                            ULOOrgCode = sOrgCode;
                            ReviewerUserID = iPrevReviewerUserID;
                        }
                    }                    

                    return_value = Dal.RequestRerouteItem(iOItemID, ULOOrgCode, Convert.ToInt32(line_num), ReviewerUserID, sResponsibleOrg, sNewOrganization, sNewOrgCode, iNewReviewerUserID, sComments, OpenItemStatus.stReassignRequest);
                    if (return_value != 0 && request_id == 0)
                        request_id = return_value;

                    if (Settings.Default.WriteExtendedLog)
                    {
                        var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RequestReassignItem In Loop: OItemID {1}, OrgCode {2}, Line {3}, PrevReviewerUserID {4}, ResponsibleOrg {5}, NewOrganization {6}, NewOrgCode {7}, NewReviewerUserID {8}, Return Code {9}",
                            DateTime.Now, iOItemID, sOrgCode, line_num, iPrevReviewerUserID, sResponsibleOrg, sNewOrganization, sNewOrgCode, iNewReviewerUserID, return_value);
                        Utility.WriteLogEntry(str);
                    }
                }
               


                if (Settings.Default.WriteExtendedLog)
                {
                    var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RequestReassignItem after Commit Transaction: OItemID {1}, OrgCode {2}, Lines {3}, PrevReviewerUserID {4}, ResponsibleOrg {5}, NewOrganization {6}, NewOrgCode {7}, NewReviewerUserID {8}",
                        DateTime.Now, iOItemID, sOrgCode, sLines, iPrevReviewerUserID, sResponsibleOrg, sNewOrganization, sNewOrgCode, iNewReviewerUserID);
                    Utility.WriteLogEntry(str);
                }
            }
            catch (SqlException ex)
            {

                if (Settings.Default.WriteExtendedLog)
                {
                    var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RequestReassignItem on ROLLBACK TRANSACTION: OItemID {1}, OrgCode {2}, Lines {3}, PrevReviewerUserID {4}, ResponsibleOrg {5}, NewOrganization {6}, NewOrgCode {7}, NewReviewerUserID {8}, Error: {9}",
                        DateTime.Now, iOItemID, sOrgCode, sLines, iPrevReviewerUserID, sResponsibleOrg, sNewOrganization, sNewOrgCode, iNewReviewerUserID, ex.Message);
                    Utility.WriteLogEntry(str);
                }
                //send an email notification to administrator
                new EmailUtility().SendAlertToSysAdmin("ULO/FundStatus application alert",
                    String.Format("An error has occurred in ULO/FundStatus application. Function: {0}. Error: {1}", "AssignBO - RequestReassignItem", ex.Message));                
                
                throw new Exception("Item reroute did not succeed. Please try again.");
            }
            return request_id;
        }

        public DataTable RerouteItemDirectly(int iOItemID, string sLines, int iPrevReviewerUserID, string sULOOrgCode,
            string sNewOrganization, string sNewOrgCode, int iNewReviewerID, string sComments)
        {
            var return_code = -1;
            //ApplicationAssert app_log = new ApplicationAssert();
            if (Settings.Default.WriteExtendedLog)
            {
                var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RerouteItemDirectly before calculating lines: OItemID {1}, Lines {2}, PrevReviewerUserID {3}, ULOOrgCode {4}, NewOrganization {5}, NewOrgCode {6}, NewReviewerID {7}, Comments: {8}",
                    DateTime.Now, iOItemID, sLines, iPrevReviewerUserID, sULOOrgCode, sNewOrganization, sNewOrgCode, iNewReviewerID, sComments);
                Utility.WriteLogEntry(str);
            }

            DataTable dt = null;
            DataRow[] dr;
            var arr_lines = sLines.Split(new char[] { ',' });

            if (arr_lines.Length > 1)
            {
                //could be more than one ULOOrgCodes for these lines
                var dsLines = Item.GetULOOrganizationsByItemLines(iOItemID);
                if (dsLines != null && dsLines.Tables[0].Rows.Count > 1)
                    dt = dsLines.Tables[0];
            }

            if (Settings.Default.WriteExtendedLog)
            {
                var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RerouteItemDirectly before Transaction: OItemID {1}, Lines {2}, PrevReviewerUserID {3}, ULOOrgCode {4}, NewOrganization {5}, NewOrgCode {6}, NewReviewerID {7}",
                    DateTime.Now, iOItemID, sLines, iPrevReviewerUserID, sULOOrgCode, sNewOrganization, sNewOrgCode, iNewReviewerID);
                Utility.WriteLogEntry(str);
            }

            try
            {
                int ReviewUser;
                string ULOOrgCode;
                foreach (var line_num in arr_lines)
                {
                 
                    if (dt == null)
                    {
                        ReviewUser = iPrevReviewerUserID;
                        ULOOrgCode = sULOOrgCode;
                    }
                    else
                    {
                        dr = dt.Select("ItemLNum = " + line_num);
                        if (dr.Length > 0)
                        {
                            ReviewUser = (int)dr[0]["ReviewerUserID"];
                            ULOOrgCode = (string)dr[0]["ULOOrgCode"];
                        }
                        else
                        {
                            ReviewUser = iPrevReviewerUserID;
                            ULOOrgCode = sULOOrgCode;
                        }
                    }

                    return_code = Dal.RerouteItem(iOItemID, Convert.ToInt32(line_num), ReviewUser, ULOOrgCode, sNewOrganization, sNewOrgCode, iNewReviewerID, sComments);

                    if (Settings.Default.WriteExtendedLog)
                    {
                        var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RerouteItemDirectly In Loop: OItemID {1}, line {2}, PrevReviewerUserID {3}, ULOOrgCode {4}, NewOrganization {5}, NewOrgCode {6}, NewReviewerID {7}, Return Code: {8}",
                            DateTime.Now, iOItemID, line_num, iPrevReviewerUserID, ULOOrgCode, sNewOrganization, sNewOrgCode, iNewReviewerID, return_code);
                        Utility.WriteLogEntry(str);
                    }
                }


                if (Settings.Default.WriteExtendedLog)
                {
                    var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RerouteItemDirectly after Commit Transaction: OItemID {1}, Lines {2}, PrevReviewerUserID {3}, ULOOrgCode {4}, NewOrganization {5}, NewOrgCode {6}, NewReviewerID {7}",
                        DateTime.Now, iOItemID, sLines, iPrevReviewerUserID, sULOOrgCode, sNewOrganization, sNewOrgCode, iNewReviewerID);
                    Utility.WriteLogEntry(str);
                }
            }
            catch (SqlException ex)
            {

                if (Settings.Default.WriteExtendedLog)
                {
                    var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RerouteItemDirectly on ROLLBACK TRANSACTION: OItemID {1}, Lines {2}, PrevReviewerUserID {3}, ULOOrgCode {4}, NewOrganization {5}, NewOrgCode {6}, NewReviewerID {7}, Error: {8}",
                        DateTime.Now, iOItemID, sLines, iPrevReviewerUserID, sULOOrgCode, sNewOrganization, sNewOrgCode, iNewReviewerID, ex.Message);
                    Utility.WriteLogEntry(str);
                }
                //send an email notification to administrator
                new EmailUtility().SendAlertToSysAdmin("ULO/FundStatus application alert",
                    String.Format("An error has occurred in ULO/FundStatus application. Function: {0}. Error: {1}", "AssignBO - RerouteItemDirectly", ex.Message));                

                throw new Exception("Item reroute did not succeed. Please try again.");
            }
            return dt;
        }

        public int RerouteItem(int iRequestID, string sNewOrganization, string sNewOrgCode, int iNewReviewerUserID, string sComments)
        {
            var return_code = -1;
            //ApplicationAssert app_log = new ApplicationAssert();
            if (Settings.Default.WriteExtendedLog)
            {
                var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RerouteItem before Transaction: RequestID {1}, NewOrganization {2}, NewOrgCode {3}, NewReviewerUserID {4}, Comments: {5}",
                    DateTime.Now, iRequestID, sNewOrganization, sNewOrgCode, iNewReviewerUserID, sComments);
                Utility.WriteLogEntry(str);
            }

            try
            {
                var arrParams = new SqlParameter[6];
                arrParams[0] = new SqlParameter("@RequestID", SqlDbType.Int);
                arrParams[0].Value = iRequestID;
                arrParams[1] = new SqlParameter("@NewRespOrg", SqlDbType.VarChar);
                arrParams[1].Value = sNewOrganization;
                arrParams[2] = new SqlParameter("@NewOrgCode", SqlDbType.VarChar);
                arrParams[2].Value = sNewOrgCode;
                arrParams[3] = new SqlParameter("@NewReviewerID", SqlDbType.Int);
                arrParams[3].Value = iNewReviewerUserID;
                arrParams[4] = new SqlParameter("@Comments", SqlDbType.VarChar);
                arrParams[4].Value = sComments;                
                arrParams[5] = new SqlParameter("@ReturnCode", SqlDbType.Int);
                arrParams[5].Direction = ParameterDirection.Output;

                return_code = (int)Dal.RerouteItemByRequest(iRequestID, sNewOrganization, sNewOrgCode, iNewReviewerUserID, sComments);
                

                if (Settings.Default.WriteExtendedLog)
                {
                    var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RerouteItem after Commit Transaction: RequestID {1}, NewOrganization {2}, NewOrgCode {3}, NewReviewerUserID {4}, Return Code: {5}",
                        DateTime.Now, iRequestID, sNewOrganization, sNewOrgCode, iNewReviewerUserID, return_code);
                    Utility.WriteLogEntry(str);
                }
            }
            catch (SqlException ex)
            {

                if (Settings.Default.WriteExtendedLog)
                {
                    var str = String.Format("{0:MM/dd/yyyy hh:mm}   On RerouteItem on ROLLBACK TRANSACTION: RequestID {1}, NewOrganization {2}, NewOrgCode {3}, NewReviewerUserID {4}, Error: {5}",
                        DateTime.Now, iRequestID, sNewOrganization, sNewOrgCode, iNewReviewerUserID, ex.Message);
                    Utility.WriteLogEntry(str);
                }
                //send an email notification to administrator
                new EmailUtility().SendAlertToSysAdmin("ULO/FundStatus application alert",
                    String.Format("An error has occurred in ULO/FundStatus application. Function: {0}. Error: {1}", "AssignBO - RerouteItem", ex.Message));                

                throw new Exception("Item reroute did not succeed. Please try again.");
            }
            return return_code;
        }

        public DataSet GetAssignUsers(int iOItemID, string sOrgCode)
        {
            return Dal.GetItemAssignUsers(iOItemID, sOrgCode);
        }

        public List<spGetReassignOrgList_Result> GetOrganizationsList()
        {
            var ds = Dal.GetReassignOrgList().ToList();
            ApplicationAssert.CheckCondition(ds.Any(), "There is the problem to load application configurations. Please contact your system administrator.");
            return ds;
        }

        public DataSet GetUsersOrganizationsList()
        {
            var ds = Dal.GetReassignUsersOrgList();
            ApplicationAssert.CheckCondition(ds != null && ds.Tables[0].Rows.Count > 0, "There is the problem to load application configurations. Please contact your system administrator.");
            return ds;
        }

        public DataSet GetRequestsList(int iLoadID, string sOrganization)
        {
            return Dal.GetRerouteReqList(iLoadID, sOrganization);
        }

        public DataRow GetRerouteRequestProperties(int iRequestID)
        {
            var ds = Dal.GetRerouteRequestDetails(iRequestID, 1);

            DataRow dr = null;
            if (ds != null && ds.Tables[0].Rows.Count > 0)
                dr = ds.Tables[0].Rows[0];
            return dr;
        }

        public void CancelRerouteRequest(int iRequestID)
        {
           Dal.CancelRerouteRequest(iRequestID);
        }

    }
}