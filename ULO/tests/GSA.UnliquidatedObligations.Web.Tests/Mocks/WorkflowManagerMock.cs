using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Services;
using GSA.UnliquidatedObligations.Web.Tests.TestData;
using Moq;
using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.Web.Tests.Mocks
{
    internal class WorkflowManagerMock
    {
        public IWorkflowManager SetupWorkflowManagerMock(string currentActivityMock = "B2")
        {
            var wfManager = new Mock<IWorkflowManager>();

            var d = (IWorkflowDescription)JsonConvert.DeserializeObject<WorkflowDescription>(WorkflowDescriptionData.GenerateData(currentActivityMock));

            wfManager.Setup(wfm => wfm.GetWorkflowDescription(It.IsAny<Workflow>()))
               .ReturnsAsync(d);

            return wfManager.Object;
        }
    }
}
