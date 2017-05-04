using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class RequestForReassignmentData
    {
        public static List<RequestForReassignment> GenerateData(int listSize, int withRequestForReassignmentId, int withWorkflowId,  string UserId)
        {

            return Builder<RequestForReassignment>
                .CreateListOfSize(listSize)
                .TheFirst(1)
                .With(r => r.UnliqudatedObjectsWorkflowQuestion = new UnliqudatedObjectsWorkflowQuestion() {UnliqudatedWorkflowQuestionsId = 1})
                .With(r => r.RequestForReassignmentID = withRequestForReassignmentId)
                .With(r => r.WorkflowId = withWorkflowId)
                .With(r => r.SuggestedReviewerId = UserId)
                .Build().ToList();
        }
    }
}