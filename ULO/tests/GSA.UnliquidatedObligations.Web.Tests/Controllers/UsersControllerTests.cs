using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSA.UnliquidatedObligations.Web.Tests.Controllers
{
    [TestClass]
    public class UsersControllerTests : ControllerTests
    {
        private UsersController UsersController;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            UsersController = new UsersController(DbContext, ComponentContext)
            {
                ControllerContext = ControllerContext
            };

        }

        [TestMethod]
        public async Task Gets_Users_Information()
        {
            var view = await UsersController.Index() as ViewResult;
            var returnedModel = (UsersModels)view.Model;
            Assert.AreEqual(returnedModel.Regions[0].Value, 1.ToString());
            Assert.AreEqual(returnedModel.Regions[1].Value, 4.ToString());
            Assert.AreEqual(returnedModel.RegionId, 1);

        }
    }
}
