using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class DocumentTypesData
    {
        public static List<DocumentType> GenerateData(int listSize, string withName, int withDocumentTypeId = 1)
        {
            return Builder<DocumentType>
                .CreateListOfSize(listSize)
                .Random(1)
                .With(dt => dt.Name = withName)
                .With(dt => dt.DocumentTypeId = withDocumentTypeId)
                .Build()
                .ToList();
        }
    }
}