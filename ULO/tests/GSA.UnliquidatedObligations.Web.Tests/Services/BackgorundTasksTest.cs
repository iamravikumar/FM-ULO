using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GSA.UnliquidatedObligations.Web.Tests.Services
{
    [TestClass]
    public class BackgroundTasksTest
    {

        private BackgroundTasks BackgroundTasks;
        private Mock<IEmailServer> EmailServerMock;
        private Mock<ULODBEntities> DB;
        private Mock<IWorkflowManager> WorkflowManager;
        [TestInitialize]
        public void Initialize()
        {
           
            EmailServerMock = new Mock<IEmailServer>();
            DB = new Mock<ULODBEntities>();
            WorkflowManager = new Mock<IWorkflowManager>();
            BackgroundTasks = new BackgroundTasks(EmailServerMock.Object, DB.Object, WorkflowManager.Object);
        }
        [TestMethod]
        public void It_exists()
        {
            Assert.IsInstanceOfType(BackgroundTasks, typeof(BackgroundTasks));
        }

        [TestMethod]
        public void Email_Send_called_with_params()
        {

            var workflowModel = new Workflow
            {
                AspNetUser = new AspNetUser {UserName = "testUser"},
                UnliquidatedObligation = new UnliquidatedObligation {UloId = 1, PegasysDocumentNumber = "CL12345"}
            };
            var expectedBody = "Dear testUser, Ulo for for PDN: CL12345 is now assigned to you";
            BackgroundTasks.Email("subject", "recipient", "Dear @Model.AspNetUser.UserName, Ulo for for PDN: @Model.UnliquidatedObligation.PegasysDocumentNumber is now assigned to you", workflowModel);
            EmailServerMock.Verify(e => e.SendEmail("subject", expectedBody, "recipient"));
        }
    }
}
