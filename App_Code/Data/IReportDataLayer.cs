using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using GSA.OpenItems;

/// <summary>
/// Summary description for IDataLayer
/// </summary>
///
namespace Data
{
    public interface IReportDataLayer
    {
        DataSet GetHistoryByEmailRequest(int iEmailRequestID);
        DataSet GetReportDocuments(int iLoadID);
        DataSet GetReportCOTotal(int iLoadID);
        DataSet GetReportValidationByLine(int LoadID);
        DataSet GetReportDaily(int iLoadID);
        DataSet GetReportTotalSum(int iLoadID);
        DataSet GetReportTotalByValid(int iLoadID);
        DataSet GetTotalByOrganization(int iLoadID);
        DataSet GetDaraByDocNum(int iLoadID);
    }
}