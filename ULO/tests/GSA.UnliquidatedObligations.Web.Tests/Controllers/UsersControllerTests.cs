using System.Linq;
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
            UsersController = new UsersController(ApplicationUserManager, DbContext, ComponentContext)
            {
                ControllerContext = ControllerContext
            };
        }

        [TestMethod]
        public async Task Index_Gets_Model_Information()
        {
            var view = await UsersController.Index() as ViewResult;
            var returnedModel = (UsersModel)view.Model;
            Assert.AreEqual(returnedModel.Regions[0].Value, 1.ToString());
            Assert.AreEqual(returnedModel.Regions[1].Value, 4.ToString());
            Assert.AreEqual(returnedModel.RegionId, 1);
            Assert.AreEqual(returnedModel.Regions.Count, 2);
            Assert.AreEqual(returnedModel.Users.Count, 1);
            Assert.AreEqual(returnedModel.Users[0].Claims.Count, 4);
            Assert.AreEqual(returnedModel.Users[0].Groups.Count, 1);
            Assert.AreEqual(returnedModel.Users[0].OtherRegions.Count, 1);
        }

        [TestMethod]
        public async Task Edit_Gets_Model_Information()
        {
            //var view = await UsersController.Edit(PersonUserId, 1) as PartialViewResult;
            //var returnedModel = (EditUserModel)view.Model;
            //Assert.AreEqual(returnedModel.ApplicationPermissionClaims.Count, 4);
            //var selecteApplicationPermissionClaims = returnedModel.ApplicationPermissionClaims.Where(ac => ac.Selected).ToList();
            //Assert.AreEqual(selecteApplicationPermissionClaims.Count, 3);
            //Assert.AreEqual(returnedModel.SubjectCategoryClaims.Count, 1);
            //Assert.AreEqual(returnedModel.Groups.Count, 1);
            //var selectedGroups = returnedModel.Groups.Where(g => g.Selected).ToList();
            //Assert.AreEqual(selectedGroups.Count, 1);
            //Assert.AreEqual(returnedModel.SubjectCategoryClaims.Count, 1);
            //Assert.AreEqual(returnedModel.SubjectCategoryClaims[0].DocTypes.Count, 15);
            //Assert.AreEqual(returnedModel.SubjectCategoryClaims[0].DocType, "UE");
            //Assert.AreEqual(returnedModel.SubjectCategoryClaims[0].BACode, "F40000");
            //Assert.AreEqual(returnedModel.SubjectCategoryClaims[0].OrgCode, "G1234");
        }
    }
}
