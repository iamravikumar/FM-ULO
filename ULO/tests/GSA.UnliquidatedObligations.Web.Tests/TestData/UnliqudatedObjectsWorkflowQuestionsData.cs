using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class UnliqudatedObjectsWorkflowQuestionsData
    {
        public static List<UnliqudatedObjectsWorkflowQuestion> GenerateData(int listSize)
        {

            return Builder<UnliqudatedObjectsWorkflowQuestion>
                .CreateListOfSize(listSize)
                .Build().ToList();
        }
    }
}