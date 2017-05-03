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
        private const string PERSONUSERID = "04bf9024-b454-42b9-8671-92d743832483";
        private const string GROUPUSERID = "77cb2b95-4cd2-4c72-a132-6ceb2b14dde6";
        private const int ULOID = 2;
        private const int WORKFLOWID = 3;
        private const string WORKFLOWKEY = "65754ae8-6d5d-49a8-9f3d-15d63e5a0521";
        private const string CURRENTWORKFLOWACTIVITYKEY = "B2";

        [TestInitialize]
        public void Initialize()
        {
            var personUserData = UsersData.GenerateData(5, PERSONUSERID);
            var groupUserData = UsersData.GenerateData(1, GROUPUSERID, "Group");
            var userData = personUserData.Concat(groupUserData).ToList();
            var currentUser = personUserData.First(u => u.Id == PERSONUSERID);

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
        public async Task Details_returns_view_with_correct_Ulo_information()
        {
            var view = await UloController.Details(ULOID, WORKFLOWID) as ViewResult;
            var returnedModel = (UloViewModel)view.Model;
            Assert.AreEqual(returnedModel.CurretUnliquidatedObligation.UloId, ULOID);
        }

        [TestMethod]
        public async Task Details_returns_view_with_correct_workflow_information()
        {
            var view = await UloController.Details(ULOID, WORKFLOWID) as ViewResult;
            var returnedModel = (UloViewModel)view.Model;
            Assert.AreEqual(returnedModel.CurretUnliquidatedObligation.UloId, ULOID);
            Assert.AreEqual(returnedModel.WorkflowViewModel.Workflow.WorkflowId, WORKFLOWID);
            Assert.AreEqual(returnedModel.WorkflowViewModel.WorkflowDescriptionViewModel.CurrentActivity.WorkflowActivityKey, CURRENTWORKFLOWACTIVITYKEY);
            Assert.AreEqual(returnedModel.WorkflowViewModel.WorkflowDescriptionViewModel.Activites.Count, 3);

            var currentViewModelActivity = returnedModel.WorkflowViewModel.WorkflowDescriptionViewModel.CurrentActivity;
            var advanceViewModel = returnedModel.WorkflowViewModel.AdvanceViewModel;

            Assert.AreEqual(currentViewModelActivity.WorkflowActivityKey, CURRENTWORKFLOWACTIVITYKEY);
            Assert.AreEqual(advanceViewModel.QuestionLabel, "Do you Concur");
            var yesJustificationEnums = new List<JustificationEnum>()
            {
                JustificationEnum.ContractNotComplete,
                JustificationEnum.ServicePeriodNotExpired,
                JustificationEnum.ContractorFiledClaim,
                JustificationEnum.WatingOnRelease,
                JustificationEnum.NoRecentActivity,
                JustificationEnum.ValidRecurringContract,
                JustificationEnum.Other
            };
            var noJustificationEnums = new List<JustificationEnum>()
            {
                JustificationEnum.ItemInvalid,
                JustificationEnum.InvalidRecurringContract,
                JustificationEnum.Other
            };

            var expectedChoices = new List<QuestionChoice>()
            {
                new QuestionChoice()
                {
                    Value = "Concur",
                    Text = "Concur",
                    JustificationsEnums = yesJustificationEnums
                },
                new QuestionChoice()
                {
                    Value = "Don't concur",
                    Text = "Don't Concur",
                    JustificationsEnums = noJustificationEnums
                },
            };

            //TODO: create custom commparer to compare whole object
            //CollectionAssert.AreEquivalent(advanceViewModel.WorkflowQuestionChoices.Choices, expectedChoices, IEqualityComparer<QuestionChoice>);
            Assert.AreEqual(advanceViewModel.QuestionChoices.Count, expectedChoices.Count);
        }

        [TestMethod]
        public async Task Details_returns_view_with_correct_Document_information()
        {
            var view = await UloController.Details(ULOID, WORKFLOWID) as ViewResult;
            var returnedModel = (UloViewModel)view.Model;
            Assert.AreEqual(returnedModel.CurretUnliquidatedObligation.UloId, ULOID);
        }

        private ULODBEntities SetUpEntityMocks(List<AspNetUser> userData)
        {

            var uloList = UloData.GenerateData(10, ULOID, userData).AsQueryable();
            var workflowList = WorkflowData.GenerateData(10, WORKFLOWID, userData, PERSONUSERID, WORKFLOWKEY, CURRENTWORKFLOWACTIVITYKEY).AsQueryable();
            var userUsersList = UserUsersData.GenerateData(1, PERSONUSERID, GROUPUSERID).AsQueryable();
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

            var mockUserUsersSet = new Mock<DbSet<UserUser>>();
            mockUserUsersSet.As<IDbAsyncEnumerable<UserUser>>()
               .Setup(m => m.GetAsyncEnumerator())
               .Returns(new TestDbAsyncEnumerator<UserUser>(userUsersList.GetEnumerator()));
            mockUserUsersSet.As<IQueryable<Workflow>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<UserUser>(userUsersList.Provider));
            mockUserUsersSet.As<IQueryable<UserUser>>().Setup(m => m.Expression).Returns(userUsersList.Expression);
            mockUserUsersSet.As<IQueryable<UserUser>>().Setup(m => m.ElementType).Returns(userUsersList.ElementType);
            mockUserUsersSet.As<IQueryable<UserUser>>().Setup(m => m.GetEnumerator()).Returns(userUsersList.GetEnumerator());
            mockUserUsersSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockUserUsersSet.Object);

            //mockWorkflowDefinitionSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockWorkflowDefinitionSet.Object);

            //var mockNoteSet = new Mock<DbSet<Note>>();
            //mockNoteSet.As<IQueryable<Note>>().Setup(m => m.Provider).Returns(notesList.Provider);
            //mockNoteSet.As<IQueryable<Note>>().Setup(m => m.Expression).Returns(notesList.Expression);
            //mockNoteSet.As<IQueryable<Note>>().Setup(m => m.ElementType).Returns(notesList.ElementType);
            //mockNoteSet.As<IQueryable<Note>>().Setup(m => m.GetEnumerator()).Returns(notesList.GetEnumerator());


            var mockULODBEntities = new Mock<ULODBEntities>();
            mockULODBEntities.Setup(c => c.UnliquidatedObligations).Returns(mockUloSet.Object);
            mockULODBEntities.Setup(c => c.Workflows).Returns(mockWorkflowSet.Object);
            mockULODBEntities.Setup(c => c.UserUsers).Returns(mockUserUsersSet.Object);
            //mockULODBEntities.Setup(c => c.Notes).Returns(mockNoteSet.Object);
            return mockULODBEntities.Object;

        }

        private ApplicationUserManager SetupApplicationUserMocks(AspNetUser currentUser)
        {
            var mockStore = new Mock<IUserStore<ApplicationUser>>();
            var mockIdentityFactoryOptions = new Mock<IdentityFactoryOptions<ApplicationUserManager>>();

            var dummyUser = new ApplicationUser() { Id = PERSONUSERID, UserName = currentUser.UserName, Email = currentUser.Email };
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
