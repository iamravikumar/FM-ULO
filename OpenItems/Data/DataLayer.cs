using GSA.OpenItems;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using OpenItems;
using OpenItems.Data;
using OpenItems.Properties;

namespace Data
{
    /// <summary>
    /// Summary description for DataAccess
    /// </summary>
    /// TODO: Split into partial classes for clarity?
    public class DataLayer : IDataLayer, IDisposable
    {
        private readonly string ConnectionString;
        private readonly IZoneFinder ZoneFinder;
        private readonly DataAccess Da;
        private readonly ULOContext UloContext;
        public DataLayer(IZoneFinder zoneFinder, ULOContext uloContext)
        {
            //ConnectionString = OpenItems.Properties.Settings.Default.DefaultConn;
            ZoneFinder = zoneFinder;
            //Da = new DataAccess(ConnectionString);
            UloContext = uloContext;
        }


        IEnumerable<spGetLoadListNotArchived_Result> IAdminDataLayer.GetLoadListNotArchived()
        {
            using (UloContext)
            {
                return UloContext.spGetLoadListNotArchived();
            }

        }

        IEnumerable<spGetLoadListWithActiveDueDate_Result> IAdminDataLayer.GetLoadListWithActiveDueDate()
        {
            using (UloContext)
            {
                return UloContext.spGetLoadListWithActiveDueDate();
            }
        }

        IEnumerable<spGetLoadDetails_Result> IAdminDataLayer.GetLoadDetails(int iLoadID)
        {
            using (UloContext)
            {
                return UloContext.spGetLoadDetails(iLoadID);
            }
        }

        DataSet IAdminDataLayer.GetAllAttachmentTypes()
        {
            return Da.GetDataSet("spGetDocTypes");
        }

        IEnumerable<spGetDefaultJustifications_Result> IAdminDataLayer.GetDefaultJustificationTypes()
        {
            using (UloContext)
            {
                return UloContext.spGetDefaultJustifications();
            }
        }

        IEnumerable<spGetJustificationTypeByID_Result> IAdminDataLayer.GetJustificationTypeByID(int iID)
        {
            using (UloContext)
            {
                return UloContext.spGetJustificationTypeByID(iID);
            }
        }

        int IAdminDataLayer.ArchiveLoad(int iLoadID)
        {
            using (UloContext)
            {
                return UloContext.spArchiveLoad(iLoadID);
            }
        }

        int IAdminDataLayer.UnarchiveLoad(int iLoadID)
        {
            using (UloContext)
            {
                return UloContext.spUnarchiveLoad(iLoadID);
            }
        }

        DataSet IAdminDataLayer.GetPendingOItemsByUser(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            return Da.GetDataSet("spPendingOItemsByUser", arrParams);
        }

        IEnumerable<spPendingOItemsByOrg_Result> IAdminDataLayer.GetPendingOItemsByOrg(int iLoadID)
        {
            using (UloContext)
            {
                return UloContext.spPendingOItemsByOrg(iLoadID);
            }
        }

        IEnumerable<spPendingDeobligateByOrg_Result> IAdminDataLayer.GetPendingDeobligateByOrg(int iLoadID)
        {
            using (UloContext)
            {
                return UloContext.spPendingDeobligateByOrg(iLoadID);
            }
        }

        DataSet IAdminDataLayer.GetUsersByRole(UserRoles role)
        {
            using (UloContext)
            {
                return SpGetUsersByRole(role);
            }
        }

        DataSet IAdminDataLayer.GetPendingNAOItemsByOrg(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            return Da.GetDataSet("spPendingNAOItemsByOrg", arrParams); //get not assigned items  
        }

        DataSet IAdminDataLayer.GetPendingOItems(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            return Da.GetDataSet("spPendingOItems", arrParams);
        }

        DataSet IAdminDataLayer.GetAllULOUsers()
        {
            return Da.GetDataSet("spGetAllULOUsers");
        }

        DataSet IAdminDataLayer.GetUserEmailByUserID(int iUserID)
        {
            var arrParams = new SqlParameter[1]; //*** always check the correct amount of parameters!***
            arrParams[0] = new SqlParameter("@UserID", SqlDbType.Int);
            arrParams[0].Value = iUserID;
            return Da.GetDataSet("spGetUserEmailByUserID", arrParams);
        }

        IEnumerable<string> IAdminDataLayer.GetOrgNameByOrgCode(string sOrgCode)
        {
            using (UloContext)
            {
                return UloContext.spGetOrgNameByOrgCode(sOrgCode);
            }
        }

        IEnumerable<string> IAdminDataLayer.GetDocNumByItemID(int iItemID)
        {
            using (UloContext)
            {
                return UloContext.spGetDocNumByItemID(iItemID);
            }
        }

        int IAdminDataLayer.SaveJustification(string sJustificationDescription, int iJustification, bool bDisplayAddOnField, string sAddOnDescription)
        {
            using (UloContext)
            {
                return UloContext.spSaveJustification(sJustificationDescription, iJustification, bDisplayAddOnField,
                    sAddOnDescription);
            }
        }

        int IAssignDataLayer.VerifyItemReroute(int iRequestID, OpenItemStatus Status)
        {
            using (UloContext)
            {
                //spVerifyItemReroute(Nullable<int> requestID, Nullable<int> status, ObjectParameter returnCode)
                ObjectParameter returnCode = new ObjectParameter("ReturnCode", typeof(int));
                UloContext.spVerifyItemReroute(iRequestID, Convert.ToInt32(Status), returnCode);
                return Convert.ToInt32(returnCode.Value);
            }
        }

        DataSet IAssignDataLayer.GetItemAssignUsers(int iOItemID, string sOrgCode)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@OItemID", SqlDbType.Int);
            arrParams[0].Value = iOItemID;
            arrParams[1] = new SqlParameter("@OrgCode", SqlDbType.VarChar);
            arrParams[1].Value = sOrgCode;
            return Da.GetDataSet("spGetItemAssignUsers", arrParams);
        }

        IEnumerable<spGetReassignOrgList_Result> IAssignDataLayer.GetReassignOrgList()
        {
            using (UloContext)
            {
                return UloContext.spGetReassignOrgList();
            }
        }

        DataSet IAssignDataLayer.GetReassignUsersOrgList()
        {
            return Da.GetDataSet("spGetReassignUsersOrgList");
        }

        DataSet IAssignDataLayer.GetRerouteReqList(int iLoadID, string sOrganization)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            arrParams[1] = new SqlParameter("@Organization", SqlDbType.VarChar);
            arrParams[1].Value = sOrganization;
            return Da.GetDataSet("spGetRerouteReqList", arrParams);
        }

        DataSet IAssignDataLayer.GetRerouteRequestDetails(int iRequestID, int iBeforeReroute)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@RequestID", SqlDbType.Int);
            arrParams[0].Value = iRequestID;
            arrParams[1] = new SqlParameter("@BeforeReroute", SqlDbType.Bit);
            arrParams[1].Value = iBeforeReroute;
            return Da.GetDataSet("spGetRerouteRequestDetails", arrParams);
        }

        int IAssignDataLayer.CancelRerouteRequest(int iRequestID)
        {
            using (UloContext)
            {
                return UloContext.spCancelRerouteRequest(iRequestID);
            }

        }

        int IAssignDataLayer.RequestRerouteItem(int iOItemID, string sOrgCode, int iLineNum, int iReviewerUserID,
            string sResponsibleOrg, string sNewOrganization, string sNewOrgCode, int iNewReviewerUserID,
            string sComments, OpenItemStatus status)
        {
          
            using (UloContext)
            {
                ObjectParameter requestID = new ObjectParameter("RequestID", typeof(int));
                UloContext.spRequestRerouteItem(iOItemID, sOrgCode, iLineNum, iReviewerUserID, sResponsibleOrg,
                    sNewOrganization, sNewOrgCode, iNewReviewerUserID, sComments, (int)status, requestID);
                return Convert.ToInt32(requestID.Value);
            }
            

        }

        int IAssignDataLayer.RerouteItem(int iOItemID, int iLineNum, int iReviewerUserID, string sOrgCode,
            string sNewRespOrg, string sNewOrgCode, int iNewReviewerID, string sComments)
        {
            using (UloContext)
            {
                ObjectParameter returnCode = new ObjectParameter("ReturnCode", typeof(int));
                UloContext.spRerouteItem(iOItemID, iLineNum, iReviewerUserID, sOrgCode, sNewRespOrg, sNewOrgCode,
                    iNewReviewerID, sComments, null, returnCode);
                return Convert.ToInt32(returnCode.Value);
            }
        }

        int IAssignDataLayer.RerouteItemByRequest(int iRequestID, string sNewRespOrg, string sNewOrgCode,
            int iNewReviewerID, string sComments)
        {
            using (UloContext)
            {
                ObjectParameter returnCode = new ObjectParameter("ReturnCode", typeof(int));
                UloContext.spRerouteItemByRequest(iRequestID, sNewRespOrg, sNewOrgCode, iNewReviewerID, sComments,
                    returnCode);
                return Convert.ToInt32(returnCode.Value);
            }
        }

        DataSet IDocumentDataLayer.GetOrgCodesByDocNumber(int iLoadID, string sDocNumber)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@DocNumber", SqlDbType.VarChar);
            arrParams[0].Value = sDocNumber;
            arrParams[1] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[1].Value = iLoadID;

            return Da.GetDataSet("spGetOrgCodesByDocNumber", arrParams);
        }

        DataSet IDocumentDataLayer.GetDocAttachments(int iLoadID, string sDocNumber)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@DocNumber", SqlDbType.VarChar);
            arrParams[0].Value = sDocNumber;
            arrParams[1] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[1].Value = iLoadID;
            return Da.GetDataSet("spGetDocAttachments", arrParams);
        }

        int IDocumentDataLayer.SaveAttachment(string sDocNumber, int iDocID, int iLoadID, int[] aDocTypeCode, string sLineNums, string sComments, int iUpdateUserID)
        {
            using (UloContext)
            {
                return UloContext.spSaveAttachment(sDocNumber, iDocID, sLineNums, sComments, iLoadID, iUpdateUserID);
            }
        }

        void IDocumentDataLayer.SaveAttachmentType(string sDocTypeName, int iDocTypeCode, out int iID)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@DocTypeName", SqlDbType.VarChar);
            arrParams[0].Value = sDocTypeName;
            arrParams[1] = new SqlParameter("@DocTypeCode", SqlDbType.Int);
            arrParams[1].Value = Convert.ToInt16(iDocTypeCode);
            Da.SaveData("spSaveAttachmentType", arrParams, out iID);
        }

        int IDocumentDataLayer.DeleteAttachment(string sDocNumber, int iDocID, int iUpdateUserID)
        {
            using (UloContext)
            {
                return UloContext.spDeleteAttachment(sDocNumber, iDocID, iUpdateUserID);
            }
        }

        int IDocumentDataLayer.SelectSendAttachment(string sDocNumber, int iDocID, int iLoadID, bool bSelectedValue)
        {
            using (UloContext)
            {
                return UloContext.spSelectSendAttachment(sDocNumber, iDocID, iLoadID, bSelectedValue);
            }
        }

        int IDocumentDataLayer.SelectAttForRevision(string sDocNumber, int iDocID, int iLoadID, bool bSelectedValue)
        {
            using (UloContext)
            {
                return UloContext.spSelectAttForRevision(sDocNumber, iDocID, iLoadID, bSelectedValue);
            }
        }

        int IDocumentDataLayer.UpdateSentAttForRevision(string sDocNumber, string sDocsArray, int iLoadID)
        {
            using (UloContext)
            {
                return UloContext.spUpdateSendAttForRevision(sDocNumber, sDocsArray, iLoadID);
            }
        }

        int IDocumentDataLayer.UpdateSendAttachments(string sDocNumber, string sDocsArray, int iLoadID)
        {
            using (UloContext)
            {
                return UloContext.spUpdateSendAttachment(sDocNumber, sDocsArray, iLoadID);
            }
        }

        IEnumerable<spGetAttachRecommend_Result> IDocumentDataLayer.GetAttachRecommend(string sPegasysDocType)
        {
            using (UloContext)
            {
                return UloContext.spGetAttachRecommend(sPegasysDocType);
            }
        }

        DataSet IDocumentDataLayer.GetLoadAttachments(int iLoadID, string sOrganization)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            arrParams[1] = new SqlParameter("@Organization", SqlDbType.VarChar);
            arrParams[1].Value = sOrganization;
            return Da.GetDataSet("spGetLoadAttachments", arrParams);
        }

        int IDocumentDataLayer.UpdateDocRevision(int iLoadID, string sDocNumber, int iDocID, int iDocTypeCode, Int16 iStatus)
        {
            using (UloContext)
            {
                return UloContext.spUpdateDocRevision(sDocNumber, iDocID, iDocTypeCode, iLoadID, iStatus);
            }
        }

        DataSet IEmailDataLayer.GetUserByUsername(string sUsername)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@Username", SqlDbType.VarChar);
            arrParams[0].Value = sUsername;
            return Da.GetDataSet("spGetUserByUsername", arrParams);
        }

        int IEmailDataLayer.InsertEmailRequest(int iCurrentUserID, int iHistoryAction, bool bSendNow)
        {

            var arrParams = new SqlParameter[4];
            arrParams[0] = new SqlParameter("@SenderUserID", SqlDbType.Int);
            arrParams[0].Value = iCurrentUserID;
            arrParams[1] = new SqlParameter("@HistoryAction", SqlDbType.Int);
            arrParams[1].Value = iHistoryAction;
            arrParams[2] = new SqlParameter("@EmailStatus", SqlDbType.Int);
            if (bSendNow)
                arrParams[2].Value = (int)EmailStatus.emReadyToSend;
            else
                arrParams[2].Value = (int)EmailStatus.emPending;
            arrParams[3] = new SqlParameter("@EmailRequestID", SqlDbType.Int);
            arrParams[3].Direction = ParameterDirection.Output;
            Da.ExecuteCommand("spInsertEmailRequest", arrParams);

            ObjectParameter emailrequestID = new ObjectParameter("EmailrequestID", typeof(int));

            using (UloContext)
            {
                UloContext.spInsertEmailRequest(iCurrentUserID, iHistoryAction,
                    Convert.ToInt32(EmailStatus.emReadyToSend), emailrequestID);
                return Convert.ToInt32(emailrequestID.Value);
            }

            return (int)arrParams[3].Value;
        }

        int IEmailDataLayer.ActivateAssignEmailRequest(int iCurrentUserID, int iSelectedLoad)
        {
            using (UloContext)
            {
                return UloContext.spActivateAssignEmailRequest(iCurrentUserID, iSelectedLoad);
            }
        }

        IEnumerable<spGetFYAllowance_Result> IFundAllowanceDataLayer.GetFYAllowance(string sFiscalYear)
        {
            using (UloContext)
            {
                return UloContext.spGetFYAllowance(sFiscalYear);
            }
        }

        SqlDataReader IFundAllowanceDataLayer.GetFSReportConfig()
        {
            return Da.GetDataReader("spGetFSReportConfig");
        }

        DataSet IFundAllowanceDataLayer.GetFYAllowanceTotals(string sFiscalYear)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@FiscalYear", SqlDbType.VarChar);
            arrParams[0].Value = sFiscalYear;
            return Da.GetDataSet("spGetFYAllowanceTotals", arrParams);
        }

        DataSet IFundAllowanceDataLayer.GetFYAllowanceTotalsOrg(string sFiscalYear, string sOrg)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@FiscalYear", SqlDbType.VarChar);
            arrParams[0].Value = sFiscalYear;
            arrParams[1] = new SqlParameter("@OrgCode", SqlDbType.VarChar);
            arrParams[1].Value = sOrg;
            return Da.GetDataSet("spGetFYAllowanceTotalsOrg", arrParams);
        }

        DataSet IFundAllowanceDataLayer.GetFYAllowanceTotalsBL(string sFiscalYear, string sBusinessLine)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@FiscalYear", SqlDbType.VarChar);
            arrParams[0].Value = sFiscalYear;
            arrParams[1] = new SqlParameter("@BusinessLine", SqlDbType.VarChar);
            arrParams[1].Value = sBusinessLine;
            return Da.GetDataSet("spGetFYAllowanceTotalsBL", arrParams);
        }

        int IFundAllowanceDataLayer.SaveFYAllowance(int iAllowRecordID, string sFiscalYear, decimal dAmount, string sMonthList, int iMonthCount,
            int iUpdateUserID)
        {
            using (UloContext)
            {
                return UloContext.spSaveFYAllowance(iAllowRecordID, sFiscalYear, dAmount, sMonthList, iMonthCount,
                    iUpdateUserID);
            }
        }

        DataSet IFundAllowanceDataLayer.GetAllowanceHistory(string sFiscalYear, HistoryActions code1, HistoryActions code2)
        {
            var arrParams = new SqlParameter[3];
            arrParams[0] = new SqlParameter("@FiscalYear", SqlDbType.VarChar);
            arrParams[0].Value = sFiscalYear;
            arrParams[1] = new SqlParameter("@Code1", SqlDbType.Int);
            arrParams[1].Value = (int)code1;
            arrParams[2] = new SqlParameter("@Code2", SqlDbType.Int);
            arrParams[2].Value = (int)code2;

            return Da.GetDataSet("spGetAllowanceHistory", arrParams);
        }

        DataSet IFundAllowanceDataLayer.GetFundStatusUpdateHistory2(HistoryActions code, string sFisalYear, string sBookMonth, string sOrg)
        {
            var da = new DataAccess(Settings.Default.DefaultConn);
            var arrParams = new SqlParameter[4];
            arrParams[0] = new SqlParameter("@HistoryAction", SqlDbType.Int);
            arrParams[0].Value = code;
            arrParams[1] = new SqlParameter("@FiscalYear", SqlDbType.VarChar);
            arrParams[1].Value = sFisalYear;
            arrParams[2] = new SqlParameter("@BookMonth", SqlDbType.VarChar);
            arrParams[2].Value = sBookMonth;
            arrParams[3] = new SqlParameter("@OrgCode", SqlDbType.VarChar);
            arrParams[3].Value = sOrg;
            return Da.GetDataSet("spGetFundStatusUpdateHistory2", arrParams);
        }
        IEnumerable<string> IFundStatusDataLayer.GetBALList()
        {
            using (UloContext)
            {
                return UloContext.spGetBAList();
            }
        }

        IEnumerable<spGetFundsOrganizations_Result> IFundStatusDataLayer.GetFundsOrganizations()
        {
            using (UloContext)
            {
                return UloContext.spGetFundsOrganizations();
            }
        }

        DataSet IFundStatusDataLayer.GetReportFuncGroupList()
        {
            return Da.GetDataSet("spGetReportFuncGroupList");
        }

        IEnumerable<string> IFundStatusDataLayer.GetFundsSumFunctionList()
        {
            using (UloContext)
            {
                return UloContext.spGetFundsSumFunctionList();
            }
        }

        IEnumerable<string> IFundStatusDataLayer.GetFundsAllFunctionList()
        {
            using (UloContext)
            {
                return UloContext.spGetFundsAllFunctionList();
            }
        }

        IEnumerable<spGetObjectClassCodeList_Result> IFundStatusDataLayer.GetObjectClassCodeList()
        {
            using (UloContext)
            {
                return UloContext.spGetObjectClassCodeList();
            }
        }

        IEnumerable<spGetCostElementList_Result> IFundStatusDataLayer.GetCostElementList()
        {
            using (UloContext)
            {
                return UloContext.spGetCostElementList();
            }
        }

        IEnumerable<spGetBusinessLineList_Result> IFundStatusDataLayer.GetBusinessLineList()
        {
            using (UloContext)
            {
                return UloContext.spGetBusinessLineList();
            }
        }


        //TODO: Come back to this.  It seems that the stored procedure being called is based on Fiscal year. which the latest is one in the db is 2010.
        DataSet IFundStatusDataLayer.GetSearchResults(int iFundsViewMode, string sFiscalYear, string sBudgetActivity, out int iRecordsCount, out decimal dTotalAmount,
            object oOrgCode, object oBookMonth, object oGroupCD, object oSumFunction, object oOCCode, object oCostElem, object oDocNumber, int iMaxResultRecords, bool bGetAllRecords)
        {
            iRecordsCount = 0;
            dTotalAmount = 0;
            var stored_proc_name = "";
            switch (iFundsViewMode)
            {
                case (int)FundsReviewViewMode.fvObligations:
                    stored_proc_name = "spGetObligation_Search";
                    break;
                case (int)FundsReviewViewMode.fvIncome:
                    stored_proc_name = "spGetIncome_Search";
                    break;
                case (int)FundsReviewViewMode.fvOneTimeAdjustments:
                    stored_proc_name = "spGetAdjustment_Search";
                    break;
            }
            var da = new DataAccess(Settings.Default.DefaultConn);
            stored_proc_name = stored_proc_name + sFiscalYear.Substring(2, 2);
            var arrParams = new SqlParameter[12];
            arrParams[0] = new SqlParameter("@OrgCode", SqlDbType.VarChar);
            if (oOrgCode != null)
                arrParams[0].Value = (string)oOrgCode;
            arrParams[1] = new SqlParameter("@DocNumber", SqlDbType.VarChar);
            if (oDocNumber != null)
                arrParams[1].Value = (string)oDocNumber;
            arrParams[2] = new SqlParameter("@BookMonth", SqlDbType.VarChar);
            if (oBookMonth != null)
                arrParams[2].Value = (string)oBookMonth;
            arrParams[3] = new SqlParameter("@SumFunction", SqlDbType.VarChar);
            if (oSumFunction != null)
                arrParams[3].Value = (string)oSumFunction;
            arrParams[4] = new SqlParameter("@OCCode", SqlDbType.VarChar);
            if (oOCCode != null)
                arrParams[4].Value = (string)oOCCode;
            arrParams[5] = new SqlParameter("@CostElem", SqlDbType.VarChar);
            if (oCostElem != null)
                arrParams[5].Value = (string)oCostElem;
            arrParams[6] = new SqlParameter("@RecordsCount", SqlDbType.Int);
            arrParams[6].Direction = ParameterDirection.Output;
            arrParams[7] = new SqlParameter("@TotalAmount", SqlDbType.Money);
            arrParams[7].Direction = ParameterDirection.Output;
            arrParams[8] = new SqlParameter("@GetAllRecords", SqlDbType.Bit);
            arrParams[8].Value = bGetAllRecords;
            arrParams[9] = new SqlParameter("@GetLimitedRecords", SqlDbType.Bit);
            arrParams[9].Value = bGetAllRecords ? 0 : 1;
            arrParams[10] = new SqlParameter("@BudgetActivity", SqlDbType.VarChar);
            arrParams[10].Value = sBudgetActivity;
            arrParams[11] = new SqlParameter("@FuncGroup", SqlDbType.Int);
            if (oGroupCD != null)
                arrParams[11].Value = Int32.Parse((string)oGroupCD);

            var ds = da.GetDataSet(stored_proc_name, arrParams);

            if (arrParams[6].Value != DBNull.Value)
                iRecordsCount = (int)arrParams[6].Value;

            if (arrParams[7].Value != DBNull.Value)
                dTotalAmount = (decimal)arrParams[7].Value;

            return ds;

        }

        DataSet IFundStatusDataLayer.GetEmptyRWAProjection()
        {
            return Da.GetDataSet("spFS_GetEmptyRWAProjection");
        }

        void IFundStatusDataLayer.ClearRwaProjection(string sFiscalYear, string sOrgCode, string sMonthList)
        {
            var arrParams = new SqlParameter[3];
            arrParams[0] = new SqlParameter("@FiscalYear", SqlDbType.VarChar);
            arrParams[0].Value = sFiscalYear;
            arrParams[1] = new SqlParameter("@OrgCode", SqlDbType.VarChar);
            arrParams[1].Value = sOrgCode;
            arrParams[2] = new SqlParameter("@BookMonthList", SqlDbType.VarChar);
            arrParams[2].Value = sMonthList;
            Da.ExecuteCommand("spFS_ClearRWAProjection", arrParams);
        }

        bool IFundStatusDataLayer.InsertRWAProjection(string sFiscalYear, string sStartBookMonth, string sOrgCode, string sProjectionArray, int iUpdateUserID)
        {
            throw new NotImplementedException();
        }

        int IFundStatusDataLayer.DeleteEntryData(int iEntryID, int iUpdateUserID)
        {
            using (UloContext)
            {
                return UloContext.spDeleteEntryData_FSReport(iEntryID, iUpdateUserID);
            }
        }

        int IFundStatusDataLayer.UpdateEntryData(int iEntryID, string sDocNumber, decimal dAmount, string sExplanation, int iUpdateUserID)
        {
            using (UloContext)
            {
                return UloContext.spUpdateEntryData_FSReport(iEntryID, sDocNumber, dAmount, sExplanation, iUpdateUserID);
            }
        }

        int IFundStatusDataLayer.InsertEntryData(int iUserEntryType, string sFiscalYear, string sBookMonth,
            string sOrganization, int iReportGroupCode, string sDocNumber, decimal dAmount, string sExplanation,
            int iUpdateUserID)
        {
            using (UloContext)
            {
                return UloContext.spInsertEntryData_FSReport(iUserEntryType, sFiscalYear, sBookMonth, sOrganization, "", iReportGroupCode.ToString(), sDocNumber, dAmount, sExplanation, iUpdateUserID);
            }
        }

        DataSet IFundStatusDataLayer.GetAdjDocList(string sFiscalYear, string sOrganization, string sBookMonth,
            string sBusinessLineCode, int iReportGroupCode)
        {
            var arrParams = new SqlParameter[4];
            arrParams[0].Value = sOrganization;
            arrParams[1] = new SqlParameter("@BookMonth", SqlDbType.VarChar);
            arrParams[1].Value = sBookMonth;
            arrParams[2] = new SqlParameter("@BL_CD", SqlDbType.VarChar);
            if (sBusinessLineCode != "")
                arrParams[2].Value = sBusinessLineCode;
            arrParams[3] = new SqlParameter("@GroupCode", SqlDbType.Int);
            arrParams[3].Value = iReportGroupCode;

            var sp_name = "spFS_GetAdjDocList_FY" + sFiscalYear.Substring(2, 2);
            return Da.GetDataSet(sp_name, arrParams);
        }

        DataSet IFundStatusDataLayer.GetAwardDocList(string sFiscalYear, string sOrganization, string sBookMonth,
            string sBusinessLineCode)
        {
            var arrParams = new SqlParameter[3];
            arrParams[0] = new SqlParameter("@OrgCode", SqlDbType.VarChar);
            arrParams[0].Value = sOrganization;
            arrParams[1] = new SqlParameter("@BookMonth", SqlDbType.VarChar);
            arrParams[1].Value = sBookMonth;
            arrParams[2] = new SqlParameter("@BL_CD", SqlDbType.VarChar);
            if (sBusinessLineCode != "")
                arrParams[2].Value = sBusinessLineCode;

            var sp_name = "spFS_GetAwardDocList_FY" + sFiscalYear.Substring(2, 2);
            return Da.GetDataSet(sp_name, arrParams);
        }

        DataSet IFundStatusDataLayer.GetTrainingDocList(string sFiscalYear, string sOrganization, string sBookMonth,
           string sBusinessLineCode)
        {
            var arrParams = new SqlParameter[3];
            arrParams[0] = new SqlParameter("@OrgCode", SqlDbType.VarChar);
            arrParams[0].Value = sOrganization;
            arrParams[1] = new SqlParameter("@BookMonth", SqlDbType.VarChar);
            arrParams[1].Value = sBookMonth;
            arrParams[2] = new SqlParameter("@BL_CD", SqlDbType.VarChar);
            if (sBusinessLineCode != "")
                arrParams[2].Value = sBusinessLineCode;

            var sp_name = "spFS_GetTrainingDocList_FY" + sFiscalYear.Substring(2, 2);
            return Da.GetDataSet(sp_name, arrParams);
        }

        DataSet IFundStatusDataLayer.GetTravelDocList(string sFiscalYear, string sOrganization, string sBookMonth,
            string sBusinessLineCode)
        {
            var arrParams = new SqlParameter[3];
            arrParams[0] = new SqlParameter("@OrgCode", SqlDbType.VarChar);
            arrParams[0].Value = sOrganization;
            arrParams[1] = new SqlParameter("@BookMonth", SqlDbType.VarChar);
            arrParams[1].Value = sBookMonth;
            arrParams[2] = new SqlParameter("@BL_CD", SqlDbType.VarChar);
            if (sBusinessLineCode != "")
                arrParams[2].Value = sBusinessLineCode;

            var sp_name = "spFS_GetTravelDocList_FY" + sFiscalYear.Substring(2, 2);
            return Da.GetDataSet(sp_name, arrParams);
        }

        DataSet IItemDataLayer.GetItemContacts(string sDocNumber)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@DocNumber", SqlDbType.VarChar);
            arrParams[0].Value = sDocNumber;
            return Da.GetDataSet("spGetItemContacts", arrParams);
        }

        IEnumerable<string> IItemDataLayer.GetDocNumByItemID(int iOItemID)
        {
            using (UloContext)
            {
                return UloContext.spGetDocNumByItemID(iOItemID);
            }
        }

        IEnumerable<spGetOIFeedbackDetails_Result> IItemDataLayer.GetOIFeedbackDetails(int iOItemID)
        {
            using (UloContext)
            {
                return UloContext.spGetOIFeedbackDetails(iOItemID);
            }
        }

        void IItemDataLayer.UpdateFeedback(int iOItemID, string sDocNumber, int iLoadID, int iValid, string sResponse,
            decimal dUDOShouldBe, decimal dDOShouldBe)
        {
            var arrParams = new SqlParameter[7];
            arrParams[0] = new SqlParameter("@OItemID", SqlDbType.Int);
            arrParams[0].Value = iOItemID;
            arrParams[1] = new SqlParameter("@DocNumber", SqlDbType.VarChar);
            arrParams[1].Value = sDocNumber;
            arrParams[2] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[2].Value = iLoadID;
            arrParams[3] = new SqlParameter("@Valid", SqlDbType.Int);
            arrParams[3].Value = iValid;
            arrParams[4] = new SqlParameter("@Response", SqlDbType.VarChar);
            arrParams[4].Value = sResponse;
            arrParams[5] = new SqlParameter("@UDOShouldBe", SqlDbType.Money);
            arrParams[5].Value = dUDOShouldBe;
            arrParams[6] = new SqlParameter("@DOShouldBe", SqlDbType.Money);
            arrParams[6].Value = dDOShouldBe;
            Da.ExecuteCommand("spUpdateFeedback", arrParams);
        }

        int IItemDataLayer.AddDocumentContact(string sDocNumber, int iPersonnelID, string sRoleDesc)
        {
            //the function returns UserID of the Item Contact
            var arrParams = new SqlParameter[4];
            arrParams[0] = new SqlParameter("@DocNumber", SqlDbType.VarChar);
            arrParams[0].Value = sDocNumber;
            arrParams[1] = new SqlParameter("@PersonnelID", SqlDbType.Int);
            arrParams[1].Value = iPersonnelID;
            arrParams[2] = new SqlParameter("@RoleDesc", SqlDbType.VarChar);
            arrParams[2].Value = sRoleDesc;
            arrParams[3] = new SqlParameter("@ContactUserID", SqlDbType.Int);
            arrParams[3].Direction = ParameterDirection.Output;
            Da.ExecuteCommand("spAddDocumentContact", arrParams);
            return (int)arrParams[3].Value;
        }

        int IItemDataLayer.DeleteDocumentContact(string sDocNumber, string sRoleDesc, string sFirstName,
            string sLastName)
        {
            using (UloContext)
            {
                ObjectParameter contactUserID = new ObjectParameter("ContactUserID", typeof(int));
                UloContext.spDeleteDocumentContact(sDocNumber, sRoleDesc, sFirstName, sLastName, contactUserID);
                return Convert.ToInt32(contactUserID.Value);
            }
        }

        int IItemDataLayer.CalculateItemStatus(int iOItemID, string sULOOrgCode, int iReviewerUserID)
        {
            var arrParams = new SqlParameter[4];
            arrParams[0] = new SqlParameter("@OItemID", SqlDbType.Int);
            arrParams[0].Value = iOItemID;
            arrParams[1] = new SqlParameter("@ULOOrgCode", SqlDbType.VarChar);
            arrParams[1].Value = sULOOrgCode;
            arrParams[2] = new SqlParameter("@ReviewerUserID", SqlDbType.Int);
            arrParams[2].Value = iReviewerUserID;
            arrParams[3] = new SqlParameter("@Status", SqlDbType.Int);
            arrParams[3].Direction = ParameterDirection.Output;
            Da.ExecuteCommand("spCalculateItemStatus", arrParams);
            return (int)arrParams[3].Value;
        }

        void IItemDataLayer.UpdateItemStatus(int iOItemID, int iLoadID, string sULOOrgCode, int iReviewerUserID,
            int iStatusCode)
        {
            var arrParams = new SqlParameter[5];
            arrParams[0] = new SqlParameter("@OItemID", SqlDbType.Int);
            arrParams[0].Value = iOItemID;
            arrParams[1] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[1].Value = iLoadID;
            arrParams[2] = new SqlParameter("@ULOOrgCode", SqlDbType.VarChar);
            arrParams[2].Value = sULOOrgCode;
            arrParams[3] = new SqlParameter("@ReviewerUserID", SqlDbType.Int);
            arrParams[3].Value = iReviewerUserID;
            arrParams[4] = new SqlParameter("@Status", SqlDbType.Int);
            arrParams[4].Value = iStatusCode;
            Da.ExecuteCommand("spUpdateItemStatus", arrParams);
        }

        void IItemDataLayer.UpdateItemProperties(int iOItemID, string sULOOrgCode, string sUDOShouldBe,
            string sDOShouldBe,
            DateTime dtExpCompDate, string sComments)
        {
            var arrParams = new SqlParameter[6];
            arrParams[0] = new SqlParameter("@OItemID", SqlDbType.Int);
            arrParams[0].Value = iOItemID;
            arrParams[1] = new SqlParameter("@ULOOrgCode", SqlDbType.VarChar);
            arrParams[1].Value = sULOOrgCode;
            arrParams[2] = new SqlParameter("@UDOShouldBe", SqlDbType.Money);
            if (sUDOShouldBe.Trim().Length > 0 && sUDOShouldBe.Trim() != "$")
                arrParams[2].Value = Utility.GetDecimalFromDisplayedMoney(sUDOShouldBe);
            arrParams[3] = new SqlParameter("@DOShouldBe", SqlDbType.Money);
            if (sDOShouldBe.Trim().Length > 0 && sDOShouldBe.Trim() != "$")
                arrParams[3].Value = Utility.GetDecimalFromDisplayedMoney(sDOShouldBe);
            arrParams[4] = new SqlParameter("@ExpCompDate", SqlDbType.DateTime);
            if (dtExpCompDate != DateTime.MinValue)
                arrParams[4].Value = dtExpCompDate;
            arrParams[5] = new SqlParameter("@Comments", SqlDbType.VarChar);
            arrParams[5].Value = sComments;
            Da.ExecuteCommand("spUpdateItemProperties", arrParams);
        }

        DataSet IItemDataLayer.GetLinesOrgCodes(int iOItemID, string sLines)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@OItemID", SqlDbType.Int);
            arrParams[0].Value = iOItemID;
            arrParams[1] = new SqlParameter("@Lines", SqlDbType.VarChar);
            if (sLines != "")
                arrParams[1].Value = sLines;
            return Da.GetDataSet("spGetLinesOrgCodes", arrParams);
        }

        DataSet IItemsDataLayer.GetOIList(int iLoadID, string sOrganization, int iUserID)
        {
            var arrParams = new SqlParameter[3];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            arrParams[1] = new SqlParameter("@Organization", SqlDbType.VarChar);
            arrParams[1].Value = sOrganization;
            arrParams[2] = new SqlParameter("@UserID", SqlDbType.Int);
            arrParams[2].Value = iUserID;
            return Da.GetDataSet("spGetOIList", arrParams);
        }

        DataSet IItemsDataLayer.GetBA53ItemsList(int iLoadID, string sOrganization, int iUserID)
        {
            var arrParams = new SqlParameter[3];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            arrParams[1] = new SqlParameter("@Organization", SqlDbType.VarChar);
            arrParams[1].Value = sOrganization;
            arrParams[2] = new SqlParameter("@UserID", SqlDbType.Int);
            arrParams[2].Value = iUserID;
            return Da.GetDataSet("spGetBA53OIList", arrParams);
        }

        DataSet IItemsDataLayer.SearchItems(int iLoadID, string sOrganization, string sDocNumber, string sProjNumber,
            string sBA, string sAwardNumber)
        {
            var arrParams = new SqlParameter[6];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            arrParams[1] = new SqlParameter("@Organization", SqlDbType.VarChar);
            arrParams[1].Value = sOrganization;
            arrParams[2] = new SqlParameter("@DocNumber", SqlDbType.VarChar);
            if (sDocNumber != "")
                arrParams[2].Value = sDocNumber;
            arrParams[3] = new SqlParameter("@ProjNum", SqlDbType.VarChar);
            if (sProjNumber != "")
                arrParams[3].Value = sProjNumber;
            arrParams[4] = new SqlParameter("@BA", SqlDbType.VarChar);
            if (sBA != "")
                arrParams[4].Value = sBA;
            arrParams[5] = new SqlParameter("@AwardNumber", SqlDbType.VarChar);
            if (sAwardNumber != "")
                arrParams[5].Value = sAwardNumber;
            return Da.GetDataSet("spSearchItems", arrParams);
        }

        DataSet IItemsDataLayer.GetItemsLinesToDeobligate(int iLoadID, string sOrganization)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            arrParams[1] = new SqlParameter("@Organization", SqlDbType.VarChar);
            arrParams[1].Value = sOrganization;
            return Da.GetDataSet("spGetOILinesForDeobligation", arrParams);
        }

        object ILineNumDataLayer.CertifyDeobligation(int iOItemID, int iItemLNum)
        {
            var arrParams = new SqlParameter[3];
            arrParams[0] = new SqlParameter("@OItemID", SqlDbType.Int);
            arrParams[0].Value = iOItemID;
            arrParams[1] = new SqlParameter("@ItemLNum", SqlDbType.Int);
            arrParams[1].Value = iItemLNum;
            arrParams[2] = new SqlParameter("@DeobligatedDate", SqlDbType.DateTime);
            arrParams[2].Direction = ParameterDirection.Output;
            Da.ExecuteCommand("spCertifyDeobligation", arrParams);
            return arrParams[2].Value;
        }

        object ILineNumDataLayer.LineOnReassignRequest(int iOItemID, int iLineNum, string sULOOrgCode,
            int iReviewerUserID)
        {
            var arrParams = new SqlParameter[5];
            arrParams[0] = new SqlParameter("@OItemID", SqlDbType.Int);
            arrParams[0].Value = iOItemID;
            arrParams[1] = new SqlParameter("@ItemLNum", SqlDbType.Int);
            arrParams[1].Value = iLineNum;
            arrParams[2] = new SqlParameter("@ULOOrgCode", SqlDbType.VarChar);
            arrParams[2].Value = sULOOrgCode;
            arrParams[3] = new SqlParameter("@ReviewerUserID", SqlDbType.Int);
            arrParams[3].Value = iReviewerUserID;
            arrParams[4] = new SqlParameter("@ReturnCode", SqlDbType.Bit);
            arrParams[4].Direction = ParameterDirection.Output;
            Da.ExecuteCommand("spCheckLineStatus", arrParams);
            return arrParams[4].Value;
        }

        DataSet IOrgDataLayer.GetOrgAndOrgCodeList()
        {
            return Da.GetDataSet("spGetOrgAndOrgCodeList");
        }

        DataSet IReportDataLayer.GetHistoryByEmailRequest(int iEmailRequestID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@EmailRequestID", SqlDbType.Int);
            arrParams[0].Value = iEmailRequestID;
            return Da.GetDataSet("spGetHistoryByEmailRequest", arrParams);
        }

        DataSet IReportDataLayer.GetReportDocuments(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            return Da.GetDataSet("spReportDocuments", arrParams);
        }

        DataSet IReportDataLayer.GetReportCOTotal(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            return Da.GetDataSet("spReportCOTotal", arrParams);
        }

        DataSet IReportDataLayer.GetReportValidationByLine(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            return Da.GetDataSet("spReportValidationByLine", arrParams);
        }

        DataSet IReportDataLayer.GetReportDaily(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            return Da.GetDataSet("spReportDaily", arrParams);
        }

        DataSet IReportDataLayer.GetReportTotalSum(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            return Da.GetDataSet("spReportTotalSum", arrParams);
        }

        DataSet IReportDataLayer.GetReportTotalByValid(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            return Da.GetDataSet("spReportTotalByValid", arrParams);
        }

        DataSet IReportDataLayer.GetTotalByOrganization(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            return Da.GetDataSet("spReportTotalByOrg", arrParams);
        }

        DataSet IReportDataLayer.GetDaraByDocNum(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            return Da.GetDataSet("spDaraByDocNum", arrParams);
        }

        object IUploadServiceDataLayer.InsertNewLoad(int iDataSource, int iOpenItemsType, DateTime dtDueDate,
            int iFileID, int iParentLoadID, int iReviewRound, string sLoadName)
        {
            var arrParams = new SqlParameter[8];
            arrParams[0] = new SqlParameter("@DataSource", SqlDbType.Int);
            arrParams[0].Value = iDataSource;
            arrParams[1] = new SqlParameter("@OIType", SqlDbType.Int);
            arrParams[1].Value = iOpenItemsType;
            arrParams[2] = new SqlParameter("@DueDate", SqlDbType.DateTime);
            arrParams[2].Value = dtDueDate;
            arrParams[3] = new SqlParameter("@FileID", SqlDbType.Int);
            arrParams[3].Value = iFileID;
            arrParams[4] = new SqlParameter("@ParentLoadID", SqlDbType.Int);
            arrParams[4].Value = iParentLoadID;
            arrParams[5] = new SqlParameter("@ReviewRound", SqlDbType.Int);
            arrParams[5].Value = iReviewRound;
            arrParams[6] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[6].Direction = ParameterDirection.Output;
            arrParams[7] = new SqlParameter("@LoadName", SqlDbType.VarChar);
            arrParams[7].Value = sLoadName;
            Da.ExecuteCommand("spInsertNewLoad", arrParams);
            return arrParams[6].Value;
        }

        void IUploadServiceDataLayer.InsertReviewFeedback(int iLoadID, int iParentLoadID)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            arrParams[1] = new SqlParameter("@ParentLoadID", SqlDbType.Int);
            arrParams[1].Value = iParentLoadID;
            Da.ExecuteCommand("spInsertReviewFeedback", arrParams);
        }


        void IUploadServiceDataLayer.InsertOIMain(int iLoadID, int iOpenItemsType)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            arrParams[1] = new SqlParameter("@OIType", SqlDbType.Int);
            arrParams[1].Value = iOpenItemsType;
            Da.ExecuteCommand("spInsertOIMain", arrParams);
        }

        void IUploadServiceDataLayer.InsertOIDetails(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            Da.ExecuteCommand("spInsertOIDetails", arrParams);
        }

        void IUploadServiceDataLayer.InsertOIOrganization(int iLoadID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[0].Value = iLoadID;
            Da.ExecuteCommand("spInsertOIOrganization", arrParams);
        }

        void IUploadServiceDataLayer.InsertOILease(int iLoadID, DateTime dtReportDate)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@LoadID", SqlDbType.Int);
            arrParams[1] = new SqlParameter("@ReportDate", SqlDbType.DateTime);
            arrParams[0].Value = iLoadID;
            arrParams[1].Value = dtReportDate;
            Da.ExecuteCommand("spInsertOILease", arrParams);
        }

        object IUploadServiceDataLayer.InsertEmailRequest(int iCurrentUserID, int iHistoryAction, bool bSendNow)
        {
            var arrParams = new SqlParameter[4];
            arrParams[0] = new SqlParameter("@SenderUserID", SqlDbType.Int);
            arrParams[0].Value = iCurrentUserID;
            arrParams[1] = new SqlParameter("@HistoryAction", SqlDbType.Int);
            arrParams[1].Value = iHistoryAction;
            arrParams[2] = new SqlParameter("@EmailStatus", SqlDbType.Int);
            if (bSendNow)
                arrParams[2].Value = (int)EmailStatus.emReadyToSend;
            else
                arrParams[2].Value = (int)EmailStatus.emPending;
            arrParams[3] = new SqlParameter("@EmailRequestID", SqlDbType.Int);
            arrParams[3].Direction = ParameterDirection.Output;
            Da.ExecuteCommand("spInsertEmailRequest", arrParams);
            return arrParams[3].Value;
        }

        DataSet IUsersDataLayer.GetUserByUserEmail(string sEmail)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@UserEmail", SqlDbType.VarChar);
            arrParams[0].Value = sEmail;
            return Da.GetDataSet("spGetUserByUserEmail", arrParams);
        }

        DataSet IUsersDataLayer.GetULOUserByUserEmail(string sUserEmail)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@Email", SqlDbType.VarChar);
            arrParams[0].Value = sUserEmail;
            return Da.GetDataSet("spGetULOUserByUserEmail", arrParams);
        }

        DataSet IUsersDataLayer.GetNCRUserByUserEmail(string sUserEmail)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@Email", SqlDbType.VarChar);
            arrParams[0].Value = sUserEmail;
            return Da.GetDataSet("spGetNCRUserByUserEmail", arrParams);
        }

        DataSet IUsersDataLayer.GetUserByUserID(int iUserID)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@UserID", SqlDbType.Int);
            arrParams[0].Value = iUserID;
            return Da.GetDataSet("spGetUserByUserID", arrParams);
        }

        DataSet IUsersDataLayer.SearchPersonnel(string sFirstName, string sLastName)
        {
            var arrParams = new SqlParameter[2];
            arrParams[0] = new SqlParameter("@FirstName", SqlDbType.VarChar);
            arrParams[0].Value = sFirstName;
            arrParams[1] = new SqlParameter("@LastName", SqlDbType.VarChar);
            arrParams[1].Value = sLastName;
            return Da.GetDataSet("spSearchPersonnel", arrParams);
        }

        DataSet IUsersDataLayer.GetUsersByRole(UserRoles Role)
        {
            return SpGetUsersByRole(Role);
        }

        DataSet IUsersDataLayer.GetAllActiveInactiveUsers()
        {
            return Da.GetDataSet("spGetAllActiveInactiveUsers");
        }

        DataSet IUsersDataLayer.GetAllNCRUsers()
        {
            return Da.GetDataSet("spGetAllNCRUsers");
        }

        DataSet IUsersDataLayer.GetUserRoleForFSOrg(int iCurrentUserID, string sBusinessLineCode, string sOrganization)
        {
            var arrParams = new SqlParameter[3];
            arrParams[0] = new SqlParameter("@UserID", SqlDbType.Int);
            arrParams[0].Value = iCurrentUserID;
            arrParams[1] = new SqlParameter("@BL_CD", SqlDbType.VarChar);
            if (sBusinessLineCode != "")
                arrParams[1].Value = sBusinessLineCode;
            arrParams[2] = new SqlParameter("@OrgCode", SqlDbType.VarChar);
            if (sOrganization != "")
                arrParams[2].Value = sOrganization;
            return Da.GetDataSet("spGetUserRoleForFSOrg", arrParams);
        }

        void IUsersDataLayer.SaveUser(int iUserID, string sEmail, string sPassword, string sRoleCode, int iActive, string sFirstName,
            string sLastName, string sMiddleInitial, string sOrganization, string sPhone, string sDefaultApplication, out int iID)
        {
            var arrParams = new SqlParameter[11]; //*** always check the correct amount of parameters!***

            arrParams[0] = new SqlParameter("@UserID", SqlDbType.Int);
            arrParams[0].Value = iUserID;
            arrParams[1] = new SqlParameter("@Email", SqlDbType.VarChar);
            arrParams[1].Value = sEmail;
            arrParams[2] = new SqlParameter("@Password", SqlDbType.VarChar);
            arrParams[2].Value = sPassword;
            arrParams[3] = new SqlParameter("@RoleCode", SqlDbType.VarChar);
            arrParams[3].Value = sRoleCode;
            arrParams[4] = new SqlParameter("@Active", SqlDbType.Int);
            arrParams[4].Value = iActive;
            arrParams[5] = new SqlParameter("@FirstName", SqlDbType.VarChar);
            arrParams[5].Value = sFirstName;
            arrParams[6] = new SqlParameter("@LastName", SqlDbType.VarChar);
            arrParams[6].Value = sLastName;
            arrParams[7] = new SqlParameter("@MiddleInitial", SqlDbType.VarChar);
            arrParams[7].Value = sMiddleInitial;
            arrParams[8] = new SqlParameter("@Organization", SqlDbType.VarChar);
            arrParams[8].Value = sOrganization;
            arrParams[9] = new SqlParameter("@Phone", SqlDbType.VarChar);
            arrParams[9].Value = sPhone;
            arrParams[10] = new SqlParameter("@DefaultApplication", SqlDbType.SmallInt);
            arrParams[10].Value = sDefaultApplication;

            Da.SaveData("spSaveUser", arrParams, out iID);

        }


        DataSet ILookupDataLayer.GetOpenItemsTypes()
        {
            return Da.GetDataSet("spGetOpenItemsTypes");
        }

        DataSet ILookupDataLayer.GetDataSourceTypes()
        {
            return Da.GetDataSet("spGetDataSourceTypes");
        }

        DataSet ILookupDataLayer.GetBA53AccrualTypes()
        {
            return Da.GetDataSet("spGetBA53AccrualTypes");
        }

        DataSet ILookupDataLayer.GetBA53AccrualTypeActions(int iAccrualTypeCode)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@AccrualTypeCode", SqlDbType.Int);
            arrParams[0].Value = iAccrualTypeCode;
            return Da.GetDataSet("spGetBA53AccrualTypeActions", arrParams);
        }

        DataSet ILookupDataLayer.GetLoadList()
        {
            return Da.GetDataSet("spGetLoadList");
        }

        DataSet ILookupDataLayer.GetOrganizationsList()
        {
            return Da.GetDataSet("spGetOrganizationsList");
        }

        DataSet ILookupDataLayer.GetJustifications()
        {
            return Da.GetDataSet("spGetJustifications");
        }

        DataSet ILookupDataLayer.GetDefaultJustifications()
        {
            return Da.GetDataSet("spGetDefaultJustifications");
        }

        DataSet ILookupDataLayer.GetActiveCodeList()
        {
            return Da.GetDataSet("spGetActiveCodeList");
        }

        DataSet ILookupDataLayer.GetCodeList()
        {
            return Da.GetDataSet("spGetCodeList");
        }

        DataSet ILookupDataLayer.GetValidationValues()
        {
            return Da.GetDataSet("spGetValidationValues");
        }

        DataSet ILookupDataLayer.GetContactsRoles()
        {
            return Da.GetDataSet("spGetContactsRoles");
        }

        DataSet ILookupDataLayer.GetWholeOrgList()
        {
            return Da.GetDataSet("spGetWholeOrgList");
        }

        private DataSet SpGetUsersByRole(UserRoles Role)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@Role", SqlDbType.Char, 2);
            arrParams[0].Value = ((int)Role).ToString();
            return Da.GetDataSet("spGetUsersByRole", arrParams);
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Da.Dispose();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~DataLayer()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            //GC.SuppressFinalize(this);
        }



        #endregion
    }

    //Ideally through DI, but if not, via a factory, the constructor of a base page would get a IDataLayer object
    //The inherited page could then grab an IAdmin and do it's thing
}