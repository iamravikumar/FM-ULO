using System.Web.Mvc;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSA.UnliquidatedObligations.Web.Tests.Controllers
{
    [TestClass]
    public class DocumentControllerTests : ControllerTests
    {

        private DocumentsController Controller;
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            Controller = new DocumentsController(DbContext, ApplicationUserManager, ComponentContext);

        }

        [TestMethod]
        public void Details_returns_view_with_correct_model()
        {
            var view = Controller.View(DocumentId) as PartialViewResult;
            var returnedModel = (DocumentModalViewModel)view.Model;
            Assert.AreEqual(returnedModel.DocumentTypeId, DocumentTypeId);
            Assert.AreEqual(returnedModel.DocumentTypes.Count, 5);
        }



    }
}
