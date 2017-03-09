using System.Collections.Generic;
using System.Linq;
using Data;
using OpenItems.Data;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Configuration;


    /// <summary>
    /// Summary description for DocumentBO - DocNumber of Open Item
    /// </summary>
    public class DocumentBO
    {
        private readonly IDocumentDataLayer Dal;

        public DocumentBO(IDocumentDataLayer dal)
        {
            Dal = dal;
        }

        public DataSet GetOrgListByDocNumber(int iLoadID, string sDocNumber)
        {
            return Dal.GetOrgCodesByDocNumber(iLoadID, sDocNumber);
        }

        public DataSet GetDocumentAttachments(int iLoadID, string sDocNumber)
        {
            return Dal.GetDocAttachments(iLoadID, sDocNumber);
        }

        public void SaveAttachment(string sDocNumber, int iDocID, int iLoadID, int[] aDocTypeCode, string sLineNums, string sComments, int iUpdateUserID)
        {
            //first update general document indexes (like Document Type Code) at DocStorage database:
            var doc = new Document(iDocID);
            doc.DocumentTypeCode = aDocTypeCode;
            doc.UpdateDocProperties();

            //then update attachment properties for OpenItems database:
            Dal.SaveAttachment(sDocNumber, iDocID, iLoadID, aDocTypeCode, sLineNums, sComments, iUpdateUserID);
        }

        public void DeleteAttachment(string sDocNumber, int iDocID, int iUpdateUserID)
        {
            Dal.DeleteAttachment(sDocNumber, iDocID, iUpdateUserID);
        }

        public void SelectSendAttachment(string sDocNumber, int iDocID, int iLoadID, bool bSelectedValue)
        {
            Dal.SelectSendAttachment(sDocNumber, iDocID, iLoadID, bSelectedValue);
        }

        public void SelectAttachmentForRevision(string sDocNumber, int iDocID, int iLoadID, bool bSelectedValue)
        {
            Dal.SelectAttForRevision(sDocNumber, iDocID, iLoadID, bSelectedValue);
        }

        public void UpdateSentAttForRevision(string sDocNumber, string sDocsArray, int iLoadID)
        {
            Dal.UpdateSentAttForRevision(sDocNumber, sDocsArray, iLoadID);
        }

        public void UpdateSentAttachments(string sDocNumber, string sDocsArray, int iLoadID)
        {
            Dal.UpdateSendAttachments(sDocNumber, sDocsArray, iLoadID);
        }

        public List<spGetAttachRecommend_Result> RecommendedAttachments(string sPegasysDocType)
        {
            var attachRecommend = Dal.GetAttachRecommend(sPegasysDocType).ToList();
            return attachRecommend.Any() ? attachRecommend : null;

        }

        public DataSet GetLoadAttachments(int iLoadID, string sOrganization)
        {
            return Dal.GetLoadAttachments(iLoadID, sOrganization);

        }

        public void UpdateDocRevision(int iLoadID, string sDocNumber, int iDocID, int iDocTypeCode, Int16 iStatus)
        {
            Dal.UpdateDocRevision(iLoadID, sDocNumber, iDocID, iDocTypeCode, iStatus);
        }

        public void SaveAttachmentType(string sDocTypeName, int iDocTypeCode, out int iID)
        {
            Dal.SaveAttachmentType(sDocTypeName, iDocTypeCode, out iID);
        }
    }
}