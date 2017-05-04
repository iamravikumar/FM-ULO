using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class DocumentData
    {
        public static List<Document> GenerateData(int listSize, string withUploadedByUserId, int workFlowId = 1,  int documentId = 1, int documentTypeId = 1)
        {

            var attachments = AttachmentsData.GenerateData(10, documentId);

            return Builder<Document>
                .CreateListOfSize(listSize)
                .TheFirst(1)
                .With(d => d.DocumentId = documentId)
                .With(d => d.UploadedByUserId = withUploadedByUserId)
                .With(d => d.Attachments = attachments)
                .With(d => d.DocumentTypeId = documentTypeId)
                .With(d => d.WorkflowId = workFlowId)
                .Build()
                .ToList();
        }
    }
}