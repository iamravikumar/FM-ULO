using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
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
    public class DocumentControllerTests
    {

        private DocumentsController Controller;
        private const int REQUESTFORREASSIGNMENTID = 5;
        private const int WORKFLOWID = 3;
        private const string WORKFLOWKEY = "65754ae8-6d5d-49a8-9f3d-15d63e5a0521";
        private const string PERSONUSERID = "04bf9024-b454-42b9-8671-92d743832483";
        private const string GROUPUSERID = "77cb2b95-4cd2-4c72-a132-6ceb2b14dde6";
        private const string CURRENTWORKFLOWACTIVITYKEY = "B2";
        private ULODBEntities dbContext;

        [TestInitialize]
        public void Initialize()
        {
            var personUserData = UsersData.GenerateData(5, PERSONUSERID);
            var groupUserData = UsersData.GenerateData(1, GROUPUSERID, "Group");
            var userData = personUserData.Concat(groupUserData).ToList();
            var currentUser = userData.First(u => u.Id == PERSONUSERID);

            //var mockContext = new Mock<ControllerContext>();
            //mockContext.SetupGet(p => p.HttpContext.User.Identity.Name).Returns(currentUser.UserName);
            //mockContext.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(true);


            dbContext = SetUpEntityMocks(userData);
            var wfManager = SetupWorkflowManagerMock();
            var applicationManager = SetupApplicationUserMocks(currentUser);

            Controller = new DocumentsController(dbContext);

        }

        [TestMethod]
        public void Details_returns_view_with_correct_information()
        {
            var view = Controller.Details(REQUESTFORREASSIGNMENTID) as PartialViewResult;
            var returnedModel = (RequestForReassignmentViewModel)view.Model;
            Assert.IsInstanceOfType(returnedModel, typeof(RequestForReassignmentViewModel));
        }

        private ULODBEntities SetUpEntityMocks(List<AspNetUser> users)
        {

            var requestForReassignmentList = RequestForReassignmentData.GenerateData(10, REQUESTFORREASSIGNMENTID, WORKFLOWID, PERSONUSERID).AsQueryable();
            var userList = users.AsQueryable();
            var unliqudatedObjectsWorkflowQuestionsList = UnliqudatedObjectsWorkflowQuestionsData.GenerateData(20);
            var unliqudatedObjectsWorkflowQuestionsListQueryable = unliqudatedObjectsWorkflowQuestionsList.AsQueryable();
            var workflowList = WorkflowData.GenerateData(10, WORKFLOWID, users, PERSONUSERID, WORKFLOWKEY, CURRENTWORKFLOWACTIVITYKEY).AsQueryable();
            var userUsersList = UserUsersData.GenerateData(1, PERSONUSERID, GROUPUSERID).AsQueryable();

            var mockRequestForReassignmentSet = new Mock<DbSet<RequestForReassignment>>();
            mockRequestForReassignmentSet.As<IDbAsyncEnumerable<RequestForReassignment>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<RequestForReassignment>(requestForReassignmentList.GetEnumerator()));
            mockRequestForReassignmentSet.As<IQueryable<RequestForReassignment>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<RequestForReassignment>(requestForReassignmentList.Provider));
            mockRequestForReassignmentSet.As<IQueryable<RequestForReassignment>>().Setup(m => m.Expression).Returns(requestForReassignmentList.Expression);
            mockRequestForReassignmentSet.As<IQueryable<RequestForReassignment>>().Setup(m => m.ElementType).Returns(requestForReassignmentList.ElementType);
            mockRequestForReassignmentSet.As<IQueryable<RequestForReassignment>>().Setup(m => m.GetEnumerator()).Returns(requestForReassignmentList.GetEnumerator());
            //mockRequestForReassignmentSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockRequestForReassignmentSet.Object);


            var mockUserSet = new Mock<DbSet<AspNetUser>>();
            mockUserSet.As<IDbAsyncEnumerable<AspNetUser>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<AspNetUser>(userList.GetEnumerator()));
            mockUserSet.As<IQueryable<AspNetUser>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<AspNetUser>(userList.Provider));
            mockUserSet.As<IQueryable<AspNetUser>>().Setup(m => m.Expression).Returns(userList.Expression);
            mockUserSet.As<IQueryable<AspNetUser>>().Setup(m => m.ElementType).Returns(userList.ElementType);
            mockUserSet.As<IQueryable<AspNetUser>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());
            //mockRequestForReassignmentSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockRequestForReassignmentSet.Object);

            var mockQuestionsSet = new Mock<DbSet<UnliqudatedObjectsWorkflowQuestion>>();
            mockQuestionsSet.As<IDbAsyncEnumerable<UnliqudatedObjectsWorkflowQuestion>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<UnliqudatedObjectsWorkflowQuestion>(unliqudatedObjectsWorkflowQuestionsListQueryable.GetEnumerator()));
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<UnliqudatedObjectsWorkflowQuestion>(unliqudatedObjectsWorkflowQuestionsListQueryable.Provider));
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>().Setup(m => m.Expression).Returns(unliqudatedObjectsWorkflowQuestionsListQueryable.Expression);
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>().Setup(m => m.ElementType).Returns(unliqudatedObjectsWorkflowQuestionsListQueryable.ElementType);
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>().Setup(m => m.GetEnumerator()).Returns(unliqudatedObjectsWorkflowQuestionsListQueryable.GetEnumerator());
            mockQuestionsSet.Setup(d => d.Add(It.IsAny<UnliqudatedObjectsWorkflowQuestion>())).Callback<UnliqudatedObjectsWorkflowQuestion>((s) => unliqudatedObjectsWorkflowQuestionsList.Add(s));


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

            var mockULODBEntities = new Mock<ULODBEntities>();
            mockULODBEntities.Setup(c => c.RequestForReassignments).Returns(mockRequestForReassignmentSet.Object);
            mockULODBEntities.Setup(c => c.AspNetUsers).Returns(mockUserSet.Object);
            mockULODBEntities.Setup(c => c.Workflows).Returns(mockWorkflowSet.Object);
            mockULODBEntities.Setup(c => c.UserUsers).Returns(mockUserUsersSet.Object);
            mockULODBEntities.Setup(c => c.UnliqudatedObjectsWorkflowQuestions).Returns(mockQuestionsSet.Object);

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

            //var d = (IWorkflowDescription)JsonConvert.DeserializeObject<WorkflowDescription>(WorkflowDescriptionData.GenerateData(CURRENTWORKFLOWACTIVITYKEY));

            //wfManager.Setup(wfm => wfm.GetWorkflowDescription(It.IsAny<Workflow>()))
            //   .ReturnsAsync(d);

            return wfManager.Object;
        }
    }
}
