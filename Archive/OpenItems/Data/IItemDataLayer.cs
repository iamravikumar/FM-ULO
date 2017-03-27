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
    public interface IItemDataLayer
    {
        DataSet GetItemContacts(string sDocNumber);
        IEnumerable<string> GetDocNumByItemID(int iOItemID);
        IEnumerable<spGetOIFeedbackDetails_Result> GetOIFeedbackDetails(int iOItemID);
        DataSet GetLinesOrgCodes(int iOItemID, string sLines);

        int AddDocumentContact(string sDocNumber, int iPersonnelID, string sRoleDesc);
        int DeleteDocumentContact(string sDocNumber, string sRoleDesc, string sFirstName, string sLastName);
        int CalculateItemStatus(int iOItemID, string sULOOrgCode, int iReviewerUserID);

        int UpdateFeedback(int iOItemID, string sDocNumber, int iLoadID, int iValid, string sResponse,
            decimal dUDOShouldBe, decimal dDOShouldBe);

        int UpdateItemStatus(int iOItemID, int iLoadID, string sULOOrgCode, int iReviewerUserID, int iStatusCode);

        int UpdateItemProperties(int iOItemID, string sULOOrgCode, string sUDOShouldBe, string sDOShouldBe,
            DateTime dtExpCompDate, string sComments);
    }
}