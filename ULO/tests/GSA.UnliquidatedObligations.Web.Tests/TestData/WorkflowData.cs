using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    
    public static class WorkflowData
    {
        public static List<Workflow> GenerateData(int listSize, int withWorkflowId, List<AspNetUser> userData, string userId, string WorkflowKey = "b6b75381-5fe5-4492-a8df-2d3699aa8dfe", string currentActivityKey = "A1")
        {
            var ownerUser = userData.First(u => u.Id == userId);
            var workflows = Builder<Workflow>
                .CreateListOfSize(listSize)
                .TheFirst(1)
                .With(wf => wf.WorkflowId = withWorkflowId)
                .With(wf => wf.OwnerUserId = ownerUser.Id)
                .With(wf => wf.AspNetUser = ownerUser)
                .With(wf => wf.CurrentWorkflowActivityKey = currentActivityKey)
                .With(wf => wf.WorkflowKey = WorkflowKey)
                .Build()
                .ToList();

            return workflows;
        }
    }
}