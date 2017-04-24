using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class WorkflowDefinitionData
    {
        public static List<WorkflowDefinition> GenerateData(int listSize, string withWorkflowDefitionKey, string descriptionJsonString)
        {
            return Builder<WorkflowDefinition>
                .CreateListOfSize(listSize)
                .TheFirst(1)
                .With(wd => wd.WorkflowKey == withWorkflowDefitionKey)
                .With(wd => wd.DescriptionJson == descriptionJsonString)
                .Build()
                .ToList(); 
        }
    }
}