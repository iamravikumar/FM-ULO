using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class DocumentTypesData
    {
        public static List<DocumentType> GenerateData(int listSize, string withName)
        {
            return Builder<DocumentType>
                .CreateListOfSize(listSize)
                .Random(1)
                .With(u => u.Name = withName)
                .Build()
                .ToList();
        }
    }
}