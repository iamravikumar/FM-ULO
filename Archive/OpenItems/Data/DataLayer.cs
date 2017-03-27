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
            return UloContext.spGetLoadListNotArchived();
        }

        IEnumerable<spGetLoadListWithActiveDueDate_Result> IAdminDataLayer.GetLoadListWithActiveDueDate()
        {
            return UloContext.spGetLoadListWithActiveDueDate();
        }

        IEnumerable<spGetLoadDetails_Result> IAdminDataLayer.GetLoadDetails(int iLoadID)
        {
            return UloContext.spGetLoadDetails(iLoadID);
        }

        DataSet IAdminDataLayer.GetAllAttachmentTypes()
        {
            return Da.GetDataSet("spGetDocTypes");
        }

        IEnumerable<spGetDefaultJustifications_Result> IAdminDataLayer.GetDefaultJustificationTypes()
        {
            return UloContext.spGetDefaultJustifications();
        }

        IEnumerable<spGetJustificationTypeByID_Result> IAdminDataLayer.GetJustificationTypeByID(int iID)
        {
            return UloContext.spGetJustificationTypeByID(iID);
        }

        int IAdminDataLayer.ArchiveLoad(int iLoadID)
        {
            return UloContext.spArchiveLoad(iLoadID);
        }

        int IAdminDataLayer.UnarchiveLoad(int iLoadID)
        {
            return UloContext.spUnarchiveLoad(iLoadID);
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
            return UloContext.spPendingOItemsByOrg(iLoadID);
        }

        IEnumerable<spPendingDeobligateByOrg_Result> IAdminDataLayer.GetPendingDeobligateByOrg(int iLoadID)
        {
            return UloContext.spPendingDeobligateByOrg(iLoadID);
        }

        DataSet IAdminDataLayer.GetUsersByRole(UserRoles role)
        {
            return SpGetUsersByRole(role);
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
            return UloContext.spGetOrgNameByOrgCode(sOrgCode);
        }

        IEnumerable<string> IAdminDataLayer.GetDocNumByItemID(int iItemID)
        {
            return UloContext.spGetDocNumByItemID(iItemID);
        }

        int IAdminDataLayer.SaveJustification(string sJustificationDescription, int iJustification, bool bDisplayAddOnField, string sAddOnDescription)
        {
            return UloContext.spSaveJustification(sJustificationDescription, iJustification, bDisplayAddOnField,
                sAddOnDescription);
        }

        int IAssignDataLayer.VerifyItemReroute(int iRequestID, OpenItemStatus Status)
        {
            ObjectParameter returnCode = new ObjectParameter("ReturnCode", typeof(int));
            UloContext.spVerifyItemReroute(iRequestID, Convert.ToInt32(Status), returnCode);
            return Convert.ToInt32(returnCode.Value);
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
            return UloContext.spGetReassignOrgList();
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
            return UloContext.spCancelRerouteRequest(iRequestID);
        }

        int IAssignDataLayer.RequestRerouteItem(int iOItemID, string sOrgCode, int iLineNum, int iReviewerUserID,
            string sResponsibleOrg, string sNewOrganization, string sNewOrgCode, int iNewReviewerUserID,
            string sComments, OpenItemStatus status)
        {
            ObjectParameter requestID = new ObjectParameter("RequestID", typeof(int));
            UloContext.spRequestRerouteItem(iOItemID, sOrgCode, iLineNum, iReviewerUserID, sResponsibleOrg,
                sNewOrganization, sNewOrgCode, iNewReviewerUserID, sComments, (int)status, requestID);
            return Convert.ToInt32(requestID.Value);
        }

        int IAssignDataLayer.RerouteItem(int iOItemID, int iLineNum, int iReviewerUserID, string sOrgCode,
            string sNewRespOrg, string sNewOrgCode, int iNewReviewerID, string sComments)
        {
            ObjectParameter returnCode = new ObjectParameter("ReturnCode", typeof(int));
            UloContext.spRerouteItem(iOItemID, iLineNum, iReviewerUserID, sOrgCode, sNewRespOrg, sNewOrgCode,
                iNewReviewerID, sComments, null, returnCode);
            return Convert.ToInt32(returnCode.Value);
        }

        int IAssignDataLayer.RerouteItemByRequest(int iRequestID, string sNewRespOrg, string sNewOrgCode,
            int iNewReviewerID, string sComments)
        {
            ObjectParameter returnCode = new ObjectParameter("ReturnCode", typeof(int));
            UloContext.spRerouteItemByRequest(iRequestID, sNewRespOrg, sNewOrgCode, iNewReviewerID, sComments,
                returnCode);
            return Convert.ToInt32(returnCode.Value);
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
            return UloContext.spSaveAttachment(sDocNumber, iDocID, sLineNums, sComments, iLoadID, iUpdateUserID);
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
            return UloContext.spDeleteAttachment(sDocNumber, iDocID, iUpdateUserID);
        }

        int IDocumentDataLayer.SelectSendAttachment(string sDocNumber, int iDocID, int iLoadID, bool bSelectedValue)
        {
            return UloContext.spSelectSendAttachment(sDocNumber, iDocID, iLoadID, bSelectedValue);
        }

        int IDocumentDataLayer.SelectAttForRevision(string sDocNumber, int iDocID, int iLoadID, bool bSelectedValue)
        {
            return UloContext.spSelectAttForRevision(sDocNumber, iDocID, iLoadID, bSelectedValue);
        }

        int IDocumentDataLayer.UpdateSentAttForRevision(string sDocNumber, string sDocsArray, int iLoadID)
        {
            return UloContext.spUpdateSendAttForRevision(sDocNumber, sDocsArray, iLoadID);
        }

        int IDocumentDataLayer.UpdateSendAttachments(string sDocNumber, string sDocsArray, int iLoadID)
        {
            return UloContext.spUpdateSendAttachment(sDocNumber, sDocsArray, iLoadID);
        }

        IEnumerable<spGetAttachRecommend_Result> IDocumentDataLayer.GetAttachRecommend(string sPegasysDocType)
        {
            return UloContext.spGetAttachRecommend(sPegasysDocType);
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
            return UloContext.spUpdateDocRevision(sDocNumber, iDocID, iDocTypeCode, iLoadID, iStatus);
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

            ObjectParameter emailrequestID = new ObjectParameter("EmailrequestID", typeof(int));

            UloContext.spInsertEmailRequest(iCurrentUserID, iHistoryAction,
                Convert.ToInt32(EmailStatus.emReadyToSend), emailrequestID);
            return Convert.ToInt32(emailrequestID.Value);

        }

        int IEmailDataLayer.ActivateAssignEmailRequest(int iCurrentUserID, int iSelectedLoad)
        {
            return UloContext.spActivateAssignEmailRequest(iCurrentUserID, iSelectedLoad);
        }

        IEnumerable<spGetFYAllowance_Result> IFundAllowanceDataLayer.GetFYAllowance(string sFiscalYear)
        {
            return UloContext.spGetFYAllowance(sFiscalYear);
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
            return UloContext.spSaveFYAllowance(iAllowRecordID, sFiscalYear, dAmount, sMonthList, iMonthCount,
                iUpdateUserID);
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
            return UloContext.spGetBAList();
        }

        IEnumerable<spGetFundsOrganizations_Result> IFundStatusDataLayer.GetFundsOrganizations()
        {

            return UloContext.spGetFundsOrganizations();
        }

        DataSet IFundStatusDataLayer.GetReportFuncGroupList()
        {
            return Da.GetDataSet("spGetReportFuncGroupList");
        }

        IEnumerable<string> IFundStatusDataLayer.GetFundsSumFunctionList()
        {
            return UloContext.spGetFundsSumFunctionList();
        }

        IEnumerable<string> IFundStatusDataLayer.GetFundsAllFunctionList()
        {
            return UloContext.spGetFundsAllFunctionList();
        }

        IEnumerable<spGetObjectClassCodeList_Result> IFundStatusDataLayer.GetObjectClassCodeList()
        {
            return UloContext.spGetObjectClassCodeList();
        }

        IEnumerable<spGetCostElementList_Result> IFundStatusDataLayer.GetCostElementList()
        {
            return UloContext.spGetCostElementList();
        }

        IEnumerable<spGetBusinessLineList_Result> IFundStatusDataLayer.GetBusinessLineList()
        {
            return UloContext.spGetBusinessLineList();
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
            return UloContext.spDeleteEntryData_FSReport(iEntryID, iUpdateUserID);
        }

        int IFundStatusDataLayer.UpdateEntryData(int iEntryID, string sDocNumber, decimal dAmount, string sExplanation, int iUpdateUserID)
        {
            return UloContext.spUpdateEntryData_FSReport(iEntryID, sDocNumber, dAmount, sExplanation, iUpdateUserID);
        }

        int IFundStatusDataLayer.InsertEntryData(int iUserEntryType, string sFiscalYear, string sBookMonth,
            string sOrganization, int iReportGroupCode, string sDocNumber, decimal dAmount, string sExplanation,
            int iUpdateUserID)
        {
            return UloContext.spInsertEntryData_FSReport(iUserEntryType, sFiscalYear, sBookMonth, sOrganization, "", iReportGroupCode.ToString(), sDocNumber, dAmount, sExplanation, iUpdateUserID);
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
            return UloContext.spGetDocNumByItemID(iOItemID);
        }

        IEnumerable<spGetOIFeedbackDetails_Result> IItemDataLayer.GetOIFeedbackDetails(int iOItemID)
        {
            return UloContext.spGetOIFeedbackDetails(iOItemID);
        }

        int IItemDataLayer.UpdateFeedback(int iOItemID, string sDocNumber, int iLoadID, int iValid, string sResponse,
            decimal dUDOShouldBe, decimal dDOShouldBe)
        {
            return UloContext.spUpdateFeedback(iOItemID, sDocNumber, iLoadID, iValid, sResponse, dUDOShouldBe,
                dDOShouldBe);
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
            ObjectParameter contactUserID = new ObjectParameter("ContactUserID", typeof(int));
            UloContext.spDeleteDocumentContact(sDocNumber, sRoleDesc, sFirstName, sLastName, contactUserID);
            return Convert.ToInt32(contactUserID.Value);
        }

        int IItemDataLayer.CalculateItemStatus(int iOItemID, string sULOOrgCode, int iReviewerUserID)
        {
            ObjectParameter status = new ObjectParameter("Status", typeof(int));
            UloContext.spCalculateItemStatus(iOItemID, sULOOrgCode, iReviewerUserID, status);
            return Convert.ToInt32(status);
        }

        int IItemDataLayer.UpdateItemStatus(int iOItemID, int iLoadID, string sULOOrgCode, int iReviewerUserID,
            int iStatusCode)
        {
            return UloContext.spUpdateItemStatus(iOItemID, iLoadID, sULOOrgCode, iReviewerUserID, iStatusCode);
        }

        int IItemDataLayer.UpdateItemProperties(int iOItemID, string sULOOrgCode, string sUDOShouldBe,
            string sDOShouldBe,
            DateTime dtExpCompDate, string sComments)
        {
            var udoShouldBe = sUDOShouldBe.Trim().Length > 0 && sUDOShouldBe.Trim() != "$"
                ? Utility.GetDecimalFromDisplayedMoney(sUDOShouldBe)
                : (decimal?)null;

            var doShouldBe = sDOShouldBe.Trim().Length > 0 && sDOShouldBe.Trim() != "$"
                ? Utility.GetDecimalFromDisplayedMoney(sDOShouldBe)
                : (decimal?)null;
            return UloContext.spUpdateItemProperties(iOItemID, sULOOrgCode, udoShouldBe, doShouldBe, dtExpCompDate,
                sComments);
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

        IEnumerable<spGetOILinesForDeobligation_Result> IItemsDataLayer.GetItemsLinesToDeobligate(int iLoadID, string sOrganization)
        {
            return UloContext.spGetOILinesForDeobligation(iLoadID, sOrganization);
        }

        object ILineNumDataLayer.CertifyDeobligation(int iOItemID, int iItemLNum)
        {
            ObjectParameter deobligatedDate = new ObjectParameter("DeobligatedDate", typeof(DateTime));
            UloContext.spCertifyDeobligation(iOItemID, iItemLNum, deobligatedDate);
            return deobligatedDate;
        }

        object ILineNumDataLayer.LineOnReassignRequest(int iOItemID, int iLineNum, string sULOOrgCode,
            int iReviewerUserID)
        {
            ObjectParameter returnCode = new ObjectParameter("ReturnCode", typeof(int));
            UloContext.spCheckLineStatus(iOItemID, iLineNum, sULOOrgCode, iReviewerUserID, returnCode);
            return returnCode;
        }

        IEnumerable<spGetOrgAndOrgCodeList_Result> IOrgDataLayer.GetOrgAndOrgCodeList()
        {
            return UloContext.spGetOrgAndOrgCodeList();
        }

        IEnumerable<spGetHistoryByEmailRequest_Result> IReportDataLayer.GetHistoryByEmailRequest(int iEmailRequestID)
        {
            return UloContext.spGetHistoryByEmailRequest(iEmailRequestID);
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

        IEnumerable<spReportDaily_Result> IReportDataLayer.GetReportDaily(int iLoadID)
        {
            return UloContext.spReportDaily(iLoadID);
        }

        IEnumerable<spReportTotalSum_Result> IReportDataLayer.GetReportTotalSum(int iLoadID)
        {
            return UloContext.spReportTotalSum(iLoadID);
        }

        IEnumerable<spReportTotalByValid_Result> IReportDataLayer.GetReportTotalByValid(int iLoadID)
        {
            return UloContext.spReportTotalByValid(iLoadID);
        }

        IEnumerable<spReportTotalByOrg_Result> IReportDataLayer.GetTotalByOrganization(int iLoadID)
        {
            return UloContext.spReportTotalByOrg(iLoadID);
        }

        IEnumerable<spDaraByDocNum_Result> IReportDataLayer.GetDaraByDocNum(int iLoadID)
        {
            return UloContext.spDaraByDocNum(iLoadID);
        }

        int IUploadServiceDataLayer.InsertNewLoad(int iDataSource, int iOpenItemsType, DateTime dtDueDate,
            int iFileID, int iParentLoadID, int iReviewRound, string sLoadName)
        {
            ObjectParameter loadId = new ObjectParameter("LoadID", typeof(int));
            UloContext.spInsertNewLoad(iDataSource, iOpenItemsType, dtDueDate, iFileID, iParentLoadID,
                iReviewRound, loadId, sLoadName);

            return Convert.ToInt32(loadId);
        }

        int IUploadServiceDataLayer.InsertReviewFeedback(int iLoadID, int iParentLoadID)
        {
            return UloContext.spInsertReviewFeedback(iLoadID, iParentLoadID);
        }


        int IUploadServiceDataLayer.InsertOIMain(int iLoadID, int iOpenItemsType)
        {
            return UloContext.spInsertOIMain(iLoadID, iOpenItemsType);
        }

        int IUploadServiceDataLayer.InsertOIDetails(int iLoadID)
        {
            return UloContext.spInsertOIDetails(iLoadID);
        }

        int IUploadServiceDataLayer.InsertOIOrganization(int iLoadID)
        {
            return UloContext.spInsertOIOrganization(iLoadID);
        }

        int IUploadServiceDataLayer.InsertOILease(int iLoadID, DateTime dtReportDate)
        {
            return UloContext.spInsertOILease(iLoadID, dtReportDate);
        }

        int IUploadServiceDataLayer.InsertEmailRequest(int iCurrentUserID, int iHistoryAction, bool bSendNow)
        {
            var emailStatus = bSendNow ? (int)EmailStatus.emReadyToSend : (int)EmailStatus.emPending;
            ObjectParameter emailRequestID = new ObjectParameter("EmailRequestID", typeof(int));
            UloContext.spInsertEmailRequest(iCurrentUserID, iHistoryAction, emailStatus, emailRequestID);
            return Convert.ToInt32(emailRequestID);
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

        IEnumerable<spGetAllActiveInactiveUsers_Result> IUsersDataLayer.GetAllActiveInactiveUsers()
        {
            return UloContext.spGetAllActiveInactiveUsers();
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

        int IUsersDataLayer.SaveUser(int iUserID, string sEmail, string sPassword, string sRoleCode, int iActive, string sFirstName,
            string sLastName, string sMiddleInitial, string sOrganization, string sPhone, string sDefaultApplication)
        {
            return UloContext.spSaveUser(iUserID, sEmail, sPassword, sRoleCode, Convert.ToBoolean(iActive), sFirstName, sLastName,
                sMiddleInitial, sOrganization, sPhone, Convert.ToInt16(sDefaultApplication));

        }


        IEnumerable<spGetOpenItemsTypes_Result> ILookupDataLayer.GetOpenItemsTypes()
        {
            return UloContext.spGetOpenItemsTypes();
        }

        IEnumerable<spGetDataSourceTypes_Result> ILookupDataLayer.GetDataSourceTypes()
        {
            return UloContext.spGetDataSourceTypes();
        }

        IEnumerable<spGetBA53AccrualTypes_Result> ILookupDataLayer.GetBA53AccrualTypes()
        {
            return UloContext.spGetBA53AccrualTypes();
        }

        IEnumerable<spGetBA53AccrualTypeActions_Result> ILookupDataLayer.GetBA53AccrualTypeActions(int iAccrualTypeCode)
        {
            return UloContext.spGetBA53AccrualTypeActions(iAccrualTypeCode);
        }

        IEnumerable<spGetLoadList_Result> ILookupDataLayer.GetLoadList()
        {
            return UloContext.spGetLoadList();
        }

        IEnumerable<string> ILookupDataLayer.GetOrganizationsList()
        {
            return UloContext.spGetOrganizationsList();
        }

        IEnumerable<spGetJustifications_Result> ILookupDataLayer.GetJustifications()
        {
            return UloContext.spGetJustifications();
        }

        IEnumerable<spGetDefaultJustifications_Result> ILookupDataLayer.GetDefaultJustifications()
        {
            return UloContext.spGetDefaultJustifications();
        }

        IEnumerable<spGetActiveCodeList_Result> ILookupDataLayer.GetActiveCodeList()
        {
            return UloContext.spGetActiveCodeList();
        }

        IEnumerable<spGetCodeList_Result> ILookupDataLayer.GetCodeList()
        {
            return UloContext.spGetCodeList();
        }

        IEnumerable<spGetValidationValues_Result> ILookupDataLayer.GetValidationValues()
        {
            return UloContext.spGetValidationValues();
        }

        IEnumerable<spGetContactsRoles_Result> ILookupDataLayer.GetContactsRoles()
        {
            return UloContext.spGetContactsRoles();
        }

        IEnumerable<spGetWholeOrgList_Result> ILookupDataLayer.GetWholeOrgList()
        {
            return UloContext.spGetWholeOrgList();
        }

        private DataSet SpGetUsersByRole(UserRoles Role)
        {
            var arrParams = new SqlParameter[1];
            arrParams[0] = new SqlParameter("@Role", SqlDbType.Char, 2);
            arrParams[0].Value = ((int)Role).ToString();
            return Da.GetDataSet("spGetUsersByRole", arrParams);
        }


        //TODO: Once we fully implement Entity, we can get rid of this since DBContext implements IDisposable
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