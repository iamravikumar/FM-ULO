using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    
    public static class WorkflowData
    {
        public static List<Workflow> GenerateData(int listSize, int withWorkflowId, List<AspNetUser> userData, string userId)
        {
            var ownerUser = userData.First(u => u.Id == userId);
            var workflows = Builder<Workflow>
                .CreateListOfSize(listSize)
                .TheFirst(1)
                .With(wf => wf.WorkflowId = withWorkflowId)
                .With(wf => wf.OwnerUserId = ownerUser.Id)
                .With(wf => wf.AspNetUser = ownerUser)
                .Build()
                .ToList();

            return workflows;
        }
    }
}