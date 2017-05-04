using System;
using System.Linq;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Services;
using GSA.UnliquidatedObligations.Web.Tests.Mocks;
using GSA.UnliquidatedObligations.Web.Tests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSA.UnliquidatedObligations.Web.Tests.Controllers
{
    [TestClass]
    public abstract class ControllerTests
    {
        protected ULODBEntities DbContext { get; private set; }
        protected IWorkflowManager WorkflowManager { get; private set; }
        protected ApplicationUserManager ApplicationUserManager { get; private set; }
        protected ControllerContext ControllerContext { get; private set; }
        protected string PersonUserId { get; private set; }
        protected string GroupUserId { get; private set; }
        protected int UloId { get; private set; }
        protected int WorkflowId = 3;
        protected string WorklowKey { get; private set; }
        protected string CurrentWorkflowActivityKey { get; private set; }
        protected int RequestedForreassignmentId {get; private set; }
        protected int DocumentId { get; private set; }
        protected int DocumentTypeId { get; private set; }

        [TestInitialize]
        public virtual void Initialize()
        {
            PersonUserId = Guid.NewGuid().ToString();
            GroupUserId = Guid.NewGuid().ToString();
            UloId = 2;
            WorkflowId = 3;
            WorklowKey = Guid.NewGuid().ToString();
            CurrentWorkflowActivityKey = "B2";
            RequestedForreassignmentId = 5;
            DocumentId = 3;
            DocumentTypeId = 2;

            var personUserData = UsersData.GenerateData(5, PersonUserId);
            var groupUserData = UsersData.GenerateData(1, GroupUserId, "Group");
            var userData = personUserData.Concat(groupUserData).ToList();
            var currentUser = personUserData.First(u => u.Id == PersonUserId);

            DbContext = new DbContextMock(PersonUserId, GroupUserId, UloId, WorkflowId,
                WorklowKey, CurrentWorkflowActivityKey, RequestedForreassignmentId, DocumentId, DocumentTypeId, userData).SetUpEntityMocks();
            WorkflowManager = new WorkflowManagerMock().SetupWorkflowManagerMock();

            ApplicationUserManager = SetupApplicationUserMocks(currentUser);

            ControllerContext = new ControllerContextMock().SetupControllerContextMock(currentUser);
        }

        private ApplicationUserManager SetupApplicationUserMocks(AspNetUser currentUser)
        {
            var mockStore = new UserStoreMock().SetupUserStoreMock(currentUser);
            var mockIdentityFactoryOptions = new IdentityFactoryOptionsMock().SetupIdentityFactoryOptionsMock(); 

            return new ApplicationUserManager(mockStore, mockIdentityFactoryOptions);
        }
    }
}
