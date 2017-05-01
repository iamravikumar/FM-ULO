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
    public class RequestForReassignmentsControllerTests
    {

        private RequestForReassignmentsController Controller;
        private const int REQUESTFORREASSIGNMENTID = 5;
        private const int WORKFLOWID = 3;
        private const string USERID = "04bf9024-b454-42b9-8671-92d743832483";
        private ULODBEntities dbContext;

        [TestInitialize]
        public void Initialize()
        {
            var userData = UsersData.GenerateData(5, USERID);
            var currentUser = userData.First(u => u.Id == USERID);

            var mockContext = new Mock<ControllerContext>();
            mockContext.SetupGet(p => p.HttpContext.User.Identity.Name).Returns(currentUser.UserName);
            mockContext.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(true);


            dbContext = SetUpEntityMocks(userData);
            var wfManager = SetupWorkflowManagerMock();
            var applicationManager = SetupApplicationUserMocks(currentUser);

            Controller = new RequestForReassignmentsController(wfManager, dbContext, applicationManager)
            {
                ControllerContext = mockContext.Object
            };

        }

        [TestMethod]
        public async Task Details_returns_view_with_correct_information()
        {
            var view = Controller.Details(REQUESTFORREASSIGNMENTID) as PartialViewResult;
            var returnedModel = (RequestForReassignmentViewModel)view.Model;
            Assert.IsInstanceOfType(returnedModel, typeof(RequestForReassignment));
        }



        [TestMethod]
        public async Task ReassignRequest_makes_correct_changes()
        {
            var requestForReassignmentViewModel = new RequestForReassignmentViewModel()
            {
                Comments = "Comments",
                JustificationId = Convert.ToInt32(JustificationEnum.ReassignNeedHelp),
                SuggestedReviewerId = USERID
            };

            var requestForReassignmentCountBefore = dbContext.RequestForReassignments.ToList().Count;
            Assert.AreEqual(10, requestForReassignmentCountBefore);
                  
            await Controller.RequestReassign(WORKFLOWID, requestForReassignmentViewModel);
            var requestForReassignmentCountAfter = dbContext.RequestForReassignments.ToList().Count;
            Assert.AreEqual(11, requestForReassignmentCountAfter);
        }


        private ULODBEntities SetUpEntityMocks(List<AspNetUser> users)
        {

            var requestForReassignmentList = RequestForReassignmentData.GenerateData(10,REQUESTFORREASSIGNMENTID, WORKFLOWID, USERID).AsQueryable();
            var userData = users.AsQueryable();
            var unliqudatedObjectsWorkflowQuestionsData = UnliqudatedObjectsWorkflowQuestionsData.GenerateData(20).AsQueryable();

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
                .Returns(new TestDbAsyncEnumerator<AspNetUser>(userData.GetEnumerator()));
            mockUserSet.As<IQueryable<AspNetUser>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<AspNetUser>(userData.Provider));
            mockUserSet.As<IQueryable<AspNetUser>>().Setup(m => m.Expression).Returns(userData.Expression);
            mockUserSet.As<IQueryable<AspNetUser>>().Setup(m => m.ElementType).Returns(userData.ElementType);
            mockUserSet.As<IQueryable<AspNetUser>>().Setup(m => m.GetEnumerator()).Returns(userData.GetEnumerator());
            //mockRequestForReassignmentSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockRequestForReassignmentSet.Object);

            var mockQuestionsSet = new Mock<DbSet<UnliqudatedObjectsWorkflowQuestion>>();
            mockQuestionsSet.As<IDbAsyncEnumerable<UnliqudatedObjectsWorkflowQuestion>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<UnliqudatedObjectsWorkflowQuestion>(unliqudatedObjectsWorkflowQuestionsData.GetEnumerator()));
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<UnliqudatedObjectsWorkflowQuestion>(userData.Provider));
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>().Setup(m => m.Expression).Returns(userData.Expression);
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>().Setup(m => m.ElementType).Returns(userData.ElementType);
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>().Setup(m => m.GetEnumerator()).Returns(unliqudatedObjectsWorkflowQuestionsData.GetEnumerator());
            //mockRequestForReassignmentSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockRequestForReassignmentSet.Object);


            var mockULODBEntities = new Mock<ULODBEntities>();
            mockULODBEntities.Setup(c => c.RequestForReassignments).Returns(mockRequestForReassignmentSet.Object);
            mockULODBEntities.Setup(c => c.AspNetUsers).Returns(mockUserSet.Object);
            mockULODBEntities.Setup(c => c.UnliqudatedObjectsWorkflowQuestions).Returns(mockQuestionsSet.Object);

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

            //var d = (IWorkflowDescription)JsonConvert.DeserializeObject<WorkflowDescription>(WorkflowDescriptionData.GenerateData(CURRENTWORKFLOWACTIVITYKEY));

            //wfManager.Setup(wfm => wfm.GetWorkflowDescription(It.IsAny<Workflow>()))
            //   .ReturnsAsync(d);

            return wfManager.Object;
        }
    }
}
