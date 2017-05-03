﻿using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class DocumentData
    {
        public static List<Document> GenerateData(int listSize, int withUploadedById)
        {

            return Builder<Document>
                .CreateListOfSize(listSize)
                .TheFirst(1)
                .With(u => u.UploadedByUserId = withUploadedById)
                .Build()
                .ToList();
        }
    }
}