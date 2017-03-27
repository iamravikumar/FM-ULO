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
    public interface IUploadServiceDataLayer
    {
        int InsertNewLoad(int iDataSource, int iOpenItemsType, DateTime dtDueDate, int iFileID, int iParentLoadID,
            int iReviewRound, string sLoadName);

        int InsertEmailRequest(int iCurrentUserID, int iHistoryAction, bool bSendNow);

        int InsertReviewFeedback(int iLoadID, int iParentLoadID);
        int InsertOIMain(int iLoadID, int iOpenItemsType);
        int InsertOIDetails(int iLoadID);
        int InsertOIOrganization(int iLoadID);
        int InsertOILease(int iLoadID, DateTime dtReportDate);
    }
}