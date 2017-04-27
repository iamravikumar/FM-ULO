using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class DocumentData
    {
        public static List<AspNetUser> GenerateData(int listSize, string withUserID)
        {
            return Builder<AspNetUser>
                .CreateListOfSize(3)
                .Random(1)
                .With(u => u.Id = withUserID)
                .Build()
                .ToList();
        }
    }
}