using System.Collections.Generic;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data.Entity;
using System.Linq;

namespace GSA.UnliquidatedObligations.Web.Tests.Services
{
    [TestClass]
    public class BackgroundTasksTest
    {

        private BackgroundTasks BackgroundTasks;
        private Mock<IEmailServer> EmailServerMock;
        [TestInitialize]
        public void Initialize()
        {
           
            var emailTemplateData = new List<EmailTemplate>
            {
                new EmailTemplate
                {
                    EmailTemplateId = 1,
                    EmailBody =
                        "Dear @Model.AspNetUser.UserName, Ulo for for PDN: @Model.UnliquidatedObligation.PegasusDocumentNumber is now assigned to you"
                },
                new EmailTemplate {EmailTemplateId = 2, EmailBody = "aaaaaaaaa"},
                new EmailTemplate {EmailTemplateId = 3, EmailBody = "bbbbbbbb"}
            }.AsQueryable();

            var mockSet = new Mock<DbSet<EmailTemplate>>();
            mockSet.As<IQueryable<EmailTemplate>>().Setup(m => m.Provider).Returns(emailTemplateData.Provider);
            mockSet.As<IQueryable<EmailTemplate>>().Setup(m => m.Expression).Returns(emailTemplateData.Expression);
            mockSet.As<IQueryable<EmailTemplate>>().Setup(m => m.ElementType).Returns(emailTemplateData.ElementType);
            mockSet.As<IQueryable<EmailTemplate>>().Setup(m => m.GetEnumerator()).Returns(emailTemplateData.GetEnumerator());

            var mockULOContext =  new Mock<ULODBEntities>();
            mockULOContext.Setup(m => m.EmailTemplates).Returns(mockSet.Object);

            EmailServerMock = new Mock<IEmailServer>();
            BackgroundTasks = new BackgroundTasks(EmailServerMock.Object, mockULOContext.Object);



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
                UnliquidatedObligation = new UnliquidatedObligation {UloId = 1, PegasusDocumentNumber = "CL12345"}
            };
            var expectedBody = "Dear testUser, Ulo for for PDN: CL12345 is now assigned to you";
            BackgroundTasks.Email("subject", "recipient", "Dear @Model.AspNetUser.UserName, Ulo for for PDN: @Model.UnliquidatedObligation.PegasusDocumentNumber is now assigned to you", workflowModel);
            EmailServerMock.Verify(e => e.SendEmail("subject", expectedBody, "recipient"));
        }
    }
}
