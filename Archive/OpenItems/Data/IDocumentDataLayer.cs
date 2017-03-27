
using System;
using System.Collections.Generic;
using System.Data;
using GSA.OpenItems;
using OpenItems.Data;

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
        IEnumerable<spGetAttachRecommend_Result> GetAttachRecommend(string sPegasysDocType);
        DataSet GetLoadAttachments(int iLoadID, string sOrganization);

        int SaveAttachment(string sDocNumber, int iDocID, int iLoadID, int[] aDocTypeCode, string sLineNums,
            string sComments, int iUpdateUserID);

        void SaveAttachmentType(string sDocTypeName, int iDocTypeCode, out int iID);
        int DeleteAttachment(string sDocNumber, int iDocID, int iUpdateUserID);
        int SelectSendAttachment(string sDocNumber, int iDocID, int iLoadID, bool bSelectedValue);
        int SelectAttForRevision(string sDocNumber, int iDocID, int iLoadID, bool bSelectedValue);
        int UpdateSentAttForRevision(string sDocNumber, string sDocsArray, int iLoadID);
        int UpdateSendAttachments(string sDocNumber, string sDocsArray, int iLoadID);
        int UpdateDocRevision(int iLoadID, string sDocNumber, int iDocID, int iDocTypeCode, Int16 iStatus);

    }
}