
using System.Data;
using GSA.OpenItems;

/// <summary>
/// Summary description for IDataLayer
/// </summary>
///
namespace Data
{
    public interface IDocumentDataLayer
    {
        DataSet GetOrgCodesByDocNumber(int iLoadID, string sDocNumber);
        DataSet GetDocAttachments(int iLoadID, string sDocNumber);
        DataSet GetAttachRecommend(string sPegasysDocType);
        DataSet GetLoadAttachments(int iLoadID, string sOrganization);

        void SaveAttachment(string sDocNumber, int iDocID, int iLoadID, int[] aDocTypeCode, string sLineNums,
            string sComments, int iUpdateUserID);

        void SaveAttachmentType(string sDocTypeName, int iDocTypeCode, out int iID);
        void DeleteAttachment(string sDocNumber, int iDocID, int iUpdateUserID);
        void SelectSendAttachment(string sDocNumber, int iDocID, int iLoadID, bool bSelectedValue);
        void SelectAttForRevision(string sDocNumber, int iDocID, int iLoadID, bool bSelectedValue);
        void UpdateSentAttForRevision(string sDocNumber, string sDocsArray, int iLoadID);
        void UpdateSentAttachments(string sDocNumber, string sDocsArray, int iLoadID);
        void UpdateDocRevision(int iLoadID, string sDocNumber, int iDocID, int iDocTypeCode, int iStatus);

    }
}