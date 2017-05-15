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
            HomeController = new HomeController(DbContext, ComponentContext)
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


    }
}
