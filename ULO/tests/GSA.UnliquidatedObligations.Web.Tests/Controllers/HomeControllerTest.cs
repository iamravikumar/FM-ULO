using GSA.UnliquidatedObligations.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Caching;
using System.Web.Mvc;

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
            HomeController = new HomeController(DbContext, ComponentContext, Cache.DataCacher)
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
