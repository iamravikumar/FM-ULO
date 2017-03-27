using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using GSA.OpenItems;
using OpenItems.Data;

/// <summary>
/// Summary description for IDataLayer
/// </summary>
///
namespace Data
{
    public interface IFundStatusDataLayer
    {
        IEnumerable<string> GetBALList();
        IEnumerable<spGetFundsOrganizations_Result> GetFundsOrganizations();
        DataSet GetReportFuncGroupList();
        IEnumerable<string> GetFundsSumFunctionList();
        IEnumerable<string> GetFundsAllFunctionList();
        IEnumerable<spGetObjectClassCodeList_Result> GetObjectClassCodeList();
        IEnumerable<spGetCostElementList_Result> GetCostElementList();
        IEnumerable<spGetBusinessLineList_Result> GetBusinessLineList();
        DataSet GetEmptyRWAProjection();
        DataSet GetSearchResults(int iFundsViewMode, string sFiscalYear, string sBudgetActivity, out int iRecordsCount,
            out decimal dTotalAmount, object oOrgCode, object oBookMonth, object oGroupCD, object oSumFunction, object oOCCode, object CostElem, object oDocNumber, int iMaxResultRecords, bool bGetAllRecords);



        void ClearRwaProjection(string sFiscalYear, string sOrgCode, string sMonthList);
        int DeleteEntryData(int iEntryID, int iUpdateUserID);
        int UpdateEntryData(int iEntryID, string sDocNumber, decimal dAmount, string sExplanation, int iUpdateUserID);
        int InsertEntryData(int iUserEntryType, string sFiscalYear, string sBookMonth, string sOrganization, int iReportGroupCode, string sDocNumber, decimal dAmount, string sExplanation, int iUpdateUserID);
        DataSet GetAdjDocList(string sFiscalYear, string sOrganization, string sBookMonth, string sBusinessLineCode, int iReportGroupCode);
        DataSet GetAwardDocList(string sFiscalYear, string sOrganization, string sBookMonth, string sBusinessLineCode);
        DataSet GetTrainingDocList(string sFiscalYear, string sOrganization, string sBookMonth, string sBusinessLineCode);
        DataSet GetTravelDocList(string sFiscalYear, string sOrganization, string sBookMonth, string sBusinessLineCode);
        bool InsertRWAProjection(string sFiscalYear, string sStartBookMonth, string sOrgCode, string sProjectionArray, int iUpdateUserID);
    }
}