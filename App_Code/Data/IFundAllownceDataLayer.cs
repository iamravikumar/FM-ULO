using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using GSA.OpenItems;

/// <summary>
/// Summary description for IDataLayer
/// </summary>
///
namespace Data
{
    public interface IFundAllowanceDataLayer
    {
        DataSet GetFYAllowance(string sFiscalYear);
        DataSet GetFYAllowanceTotals(string sFiscalYear);
        DataSet GetFYAllowanceTotalsOrg(string sFiscalYear, string sOrg);
        DataSet GetFYAllowanceTotalsBL(string sFiscalYear, string sBusinessLine);
        DataSet GetAllowanceHistory(string sFiscalYear, HistoryActions coe1, HistoryActions code2 );
        DataSet GetFYAllowanceByBL(string sFiscalYear, string sBookMonth);
        DataSet GetFundStatusUpdateHistory2(HistoryActions code, string sFisalYear, string sBookMonth, string sOrg);
        SqlDataReader GetFSReportConfig();
        int SaveFYAllowance(int iAllowRecordID, string sFiscalYear, decimal dAmount, string sMonthList, int iMonthCount, int iUpdateUserID);
    }
}