using System.Web.Mvc;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSA.UnliquidatedObligations.Web.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest : ControllerTests
    {

        private HomeController HomeController;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            HomeController = new HomeController(DbContext)
            {
                ControllerContext = ControllerContext
            };

        }

        [TestMethod]
        public void IndexTest()
        {
            var result = HomeController.Index() as ViewResult;
            Assert.AreEqual("", result.ViewName);
        }

        [TestMethod]
        public void Navigation_returns_view_model_based_on_persmissions()
        {
            var view = HomeController.Navigation() as PartialViewResult;
            var returnedModel = (NavigationViewModel)view.Model;
            Assert.IsTrue(returnedModel.CanViewOtherWorkflows);
        }


    }
}
