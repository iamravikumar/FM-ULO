using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using GSA.UnliquidatedObligations.Web.Tests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.Web.Tests.Controllers
{
    [TestClass]
    public class UloControllerTests
    {
        private UloController UloController;
        private const string USERID = "04bf9024-b454-42b9-8671-92d743832483";
        private const int ULOID = 2;
        private const int WORKFLOWID = 3;
        private const string WORKFLOWKEY = "65754ae8-6d5d-49a8-9f3d-15d63e5a0521";
        private const string CURRENTWORKFLOWACTIVITYKEY = "B2";

        [TestInitialize]
        public void Initialize()
        {
            var userData = UsersData.GenerateData(5, USERID);
            var currentUser = userData.First(u => u.Id == USERID);

            var dbContext = SetUpEntityMocks(userData);
            var wfManager = SetupWorkflowManagerMock();
            var applicationManager = SetupApplicationUserMocks(currentUser);

            var mockContext = new Mock<ControllerContext>();
            mockContext.SetupGet(p => p.HttpContext.User.Identity.Name).Returns(currentUser.UserName);
            mockContext.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(true);

            UloController = new UloController(wfManager, dbContext, applicationManager)
            {
                ControllerContext = mockContext.Object
            };

        }

        [TestMethod]
        public async Task Details_returns_view_with_correct_model()
        {
            var view = await UloController.Details(ULOID, WORKFLOWID) as ViewResult;
            var returnedModel = (UloViewModel)view.Model;
            Assert.AreEqual(returnedModel.CurretUnliquidatedObligation.UloId, ULOID);
            Assert.AreEqual(returnedModel.WorkflowViewModel.Workflow.WorkflowId, WORKFLOWID);
        }

        [TestMethod]
        public async Task Details_returns_view_with_correct_workflow_information()
        {
            var view = await UloController.Details(ULOID, WORKFLOWID) as ViewResult;
            var returnedModel = (UloViewModel)view.Model;
            Assert.AreEqual(returnedModel.CurretUnliquidatedObligation.UloId, ULOID);
            Assert.AreEqual(returnedModel.WorkflowViewModel.Workflow.WorkflowId, WORKFLOWID);
            Assert.AreEqual(returnedModel.WorkflowViewModel.WorkflowDescriptionViewModel.CurrentActivity.WorkflowActivityKey, CURRENTWORKFLOWACTIVITYKEY);
            Assert.AreEqual(returnedModel.WorkflowViewModel.WorkflowDescriptionViewModel.Activites.Count, 4);

            var currentViewModelActivity = returnedModel.WorkflowViewModel.WorkflowDescriptionViewModel.CurrentActivity;
            var advanceViewModel = returnedModel.WorkflowViewModel.AdvanceViewModel;

            Assert.AreEqual(currentViewModelActivity.WorkflowActivityKey, CURRENTWORKFLOWACTIVITYKEY);
            Assert.AreEqual(advanceViewModel.WorkflowQuestionChoices.QuestionLabel, "Concur");
            var expectedChoices = new Dictionary<string, string>
            {
                {"Concur", "Concur"},
                {"Don't Concur", "Don't Concur"}
            }.ToList();
            CollectionAssert.AreEquivalent(advanceViewModel.WorkflowQuestionChoices.Choices.ToList(), expectedChoices);
        }

        private ULODBEntities SetUpEntityMocks(List<AspNetUser> userData)
        {

            var uloList = UloData.GenerateData(10, ULOID, userData).AsQueryable();
            var workflowList = WorkflowData.GenerateData(10, WORKFLOWID, userData, USERID, WORKFLOWKEY, CURRENTWORKFLOWACTIVITYKEY).AsQueryable();
            //var notesList = NotesData.GenerateData(3, ULOID, userData).AsQueryable();

            var mockUloSet = new Mock<DbSet<UnliquidatedObligation>>();
            mockUloSet.As<IDbAsyncEnumerable<UnliquidatedObligation>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<UnliquidatedObligation>(uloList.GetEnumerator()));
            mockUloSet.As<IQueryable<UnliquidatedObligation>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<UnliquidatedObligation>(uloList.Provider));
            mockUloSet.As<IQueryable<UnliquidatedObligation>>().Setup(m => m.Expression).Returns(uloList.Expression);
            mockUloSet.As<IQueryable<UnliquidatedObligation>>().Setup(m => m.ElementType).Returns(uloList.ElementType);
            mockUloSet.As<IQueryable<UnliquidatedObligation>>().Setup(m => m.GetEnumerator()).Returns(uloList.GetEnumerator());
            mockUloSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockUloSet.Object);

            var mockWorkflowSet = new Mock<DbSet<Workflow>>();
            mockWorkflowSet.As<IDbAsyncEnumerable<Workflow>>()
               .Setup(m => m.GetAsyncEnumerator())
               .Returns(new TestDbAsyncEnumerator<Workflow>(workflowList.GetEnumerator()));
            mockWorkflowSet.As<IQueryable<Workflow>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<Workflow>(workflowList.Provider));
            mockWorkflowSet.As<IQueryable<Workflow>>().Setup(m => m.Expression).Returns(workflowList.Expression);
            mockWorkflowSet.As<IQueryable<Workflow>>().Setup(m => m.ElementType).Returns(workflowList.ElementType);
            mockWorkflowSet.As<IQueryable<Workflow>>().Setup(m => m.GetEnumerator()).Returns(workflowList.GetEnumerator());
            mockWorkflowSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockWorkflowSet.Object);

            //mockWorkflowDefinitionSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockWorkflowDefinitionSet.Object);

            //var mockNoteSet = new Mock<DbSet<Note>>();
            //mockNoteSet.As<IQueryable<Note>>().Setup(m => m.Provider).Returns(notesList.Provider);
            //mockNoteSet.As<IQueryable<Note>>().Setup(m => m.Expression).Returns(notesList.Expression);
            //mockNoteSet.As<IQueryable<Note>>().Setup(m => m.ElementType).Returns(notesList.ElementType);
            //mockNoteSet.As<IQueryable<Note>>().Setup(m => m.GetEnumerator()).Returns(notesList.GetEnumerator());


            var mockULODBEntities = new Mock<ULODBEntities>();
            mockULODBEntities.Setup(c => c.UnliquidatedObligations).Returns(mockUloSet.Object);
            mockULODBEntities.Setup(c => c.Workflows).Returns(mockWorkflowSet.Object);

            //mockULODBEntities.Setup(c => c.Notes).Returns(mockNoteSet.Object);
            return mockULODBEntities.Object;

        }

        private ApplicationUserManager SetupApplicationUserMocks(AspNetUser currentUser)
        {
            var mockStore = new Mock<IUserStore<ApplicationUser>>();
            var mockIdentityFactoryOptions = new Mock<IdentityFactoryOptions<ApplicationUserManager>>();

            var dummyUser = new ApplicationUser() { Id = USERID, UserName = currentUser.UserName, Email = currentUser.Email };
            mockStore.Setup(x => x.FindByNameAsync(currentUser.UserName))
                        .Returns(Task.FromResult(dummyUser));

            return new ApplicationUserManager(mockStore.Object, mockIdentityFactoryOptions.Object);
        }

        private IWorkflowManager SetupWorkflowManagerMock()
        {
            var wfManager = new Mock<IWorkflowManager>();

            var d = (IWorkflowDescription)JsonConvert.DeserializeObject<WorkflowDescription>(WorkflowDescriptionData.GenerateData(CURRENTWORKFLOWACTIVITYKEY));

            wfManager.Setup(wfm => wfm.GetWorkflowDescription(It.IsAny<Workflow>()))
               .ReturnsAsync(d);

            return wfManager.Object;
        }
    }
}
