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
    public interface IReportDataLayer
    {
        IEnumerable<spGetHistoryByEmailRequest_Result> GetHistoryByEmailRequest(int iEmailRequestID);
        //TODO: Change after db is fixed
        DataSet GetReportDocuments(int iLoadID);
        //TODO: Change after db is fixed
        DataSet GetReportCOTotal(int iLoadID);
        //TODO: Change after db is fixed
        DataSet GetReportValidationByLine(int LoadID);

        IEnumerable<spReportDaily_Result> GetReportDaily(int iLoadID);
        IEnumerable<spReportTotalSum_Result> GetReportTotalSum(int iLoadID);
        IEnumerable<spReportTotalByValid_Result> GetReportTotalByValid(int iLoadID);
        IEnumerable<spReportTotalByOrg_Result> GetTotalByOrganization(int iLoadID);
        IEnumerable<spDaraByDocNum_Result> GetDaraByDocNum(int iLoadID);
    }
}