using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
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
    public interface IAssignDataLayer
    {
        int VerifyItemReroute(int iRequestID, OpenItemStatus Status);
        DataSet GetItemAssignUsers(int iOItemID, string sOrgCode);
        IEnumerable<spGetReassignOrgList_Result> GetReassignOrgList();
        DataSet GetReassignUsersOrgList();
        DataSet GetRerouteReqList(int iLoadID, string sOrganization);
        DataSet GetRerouteRequestDetails(int iRequestID, int iBeforeReroute);
        int RequestRerouteItem(int iOItemID, string sOrgCode, int iLineNum, int iReviewerUserID, string sResponsibleOrg, string sNewOrganization, string sNewOrgCode, int iNewReviewerUserID, string sComments, OpenItemStatus status);

        int RerouteItem(int iOItemID, int iLineNum, int iReviewerUserID, string sOrgCode, string sNewRespOrg,
            string sNewOrgCode, int iNewReviewerID, string sComments);

        int RerouteItemByRequest(int iRequestID, string sNewRespOrg, string sNewOrgCode, int iNewReviewerID,
            string sComments);
        int CancelRerouteRequest(int iRequestID);
    }
}