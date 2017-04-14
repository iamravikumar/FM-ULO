using System;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Services;
using GSA.UnliquidatedObligations.Web.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GSA.UnliquidatedObligations.Web.Tests.Services
{
    [TestClass]
    public class WorkflowManagerTest
    {

        private IWorkflowManager WorkflowManager; 

        [TestInitialize]
        public void Initialize()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var DatabaseWorkflowDescriptionFinderMock = new DatabaseWorkflowDescriptionFinderMock();
                var BackgroundTasksMock = new BackgroundTasksMock();

                var FieldComparisonActivityChooser = new FieldComparisonActivityChooser();

                mock.Provide<IWorkflowDescriptionFinder>(DatabaseWorkflowDescriptionFinderMock);
                mock.Provide<IBackgroundTasks>(BackgroundTasksMock);

                WorkflowManager = mock.Create<WorkflowManager>();
                
            }
        }
        [TestMethod]
        public void It_exists()
        {
            Assert.IsInstanceOfType(WorkflowManager, typeof(WorkflowManager));
        }

        [Ignore]
        [TestMethod]
        public async Task AdvanceAsync_should_call_Email_with_proper_params()
        {
            //TODO: Need to figure out how to mock out FieldActivityChooser
            var wf = new Mock<Workflow>().Object;
            wf.WorkflowKey = "4a41abad-bac3-47fb-a8cf-5d667439d7c3";
            wf.OwnerUserId = "f2860baf-a555-4834-baf3-62b929d1b6b1";
            var questions = new Mock<UnliqudatedObjectsWorkflowQuestion>().Object;
            var actionReslut = await WorkflowManager.AdvanceAsync(wf, questions);
        }
    }
}
