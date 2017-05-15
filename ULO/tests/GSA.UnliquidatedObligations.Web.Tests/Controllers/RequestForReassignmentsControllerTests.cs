using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using GSA.UnliquidatedObligations.Web.Tests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace GSA.UnliquidatedObligations.Web.Tests.Controllers
{
    //TODO: Need to add more tests
    [TestClass]
    public class RequestForReassignmentsControllerTests : ControllerTests
    {

        private RequestForReassignmentsController Controller;
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            Controller = new RequestForReassignmentsController(WorkflowManager, DbContext, ApplicationUserManager, ComponentContext)
            {
                ControllerContext = ControllerContext
            };

        }

        [TestMethod]
        public void Details_returns_view_with_correct_information()
        {
            var view = Controller.Details(RequestedForreassignmentId, WorkflowId) as PartialViewResult;
            var returnedModel = (RequestForReassignmentViewModel)view.Model;
            Assert.IsInstanceOfType(returnedModel, typeof(RequestForReassignmentViewModel));
        }
    }
}
