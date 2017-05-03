using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class AttachmentsData
    {
        public static List<Attachment> GenerateData(int listSize, int withDocumentId)
        {
            return Builder<Attachment>
                .CreateListOfSize(listSize)
                .Random(1)
                .With(u => u.DocumentId = withDocumentId)
                .Build()
                .ToList();
        }
    }
}