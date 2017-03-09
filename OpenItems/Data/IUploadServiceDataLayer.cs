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
        object InsertNewLoad(int iDataSource, int iOpenItemsType, DateTime dtDueDate, int iFileID, int iParentLoadID,
            int iReviewRound, string sLoadName);

        object InsertEmailRequest(int iCurrentUserID, int iHistoryAction, bool bSendNow);

        void InsertReviewFeedback(int iLoadID, int iParentLoadID);
        void InsertOIMain(int iLoadID, int iOpenItemsType);
        void InsertOIDetails(int iLoadID);
        void InsertOIOrganization(int iLoadID);
        void InsertOILease(int iLoadID, DateTime dtReportDate);
    }
}