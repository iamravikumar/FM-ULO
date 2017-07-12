using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Caching;
using System.Web.Mvc;

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
            Controller = new DocumentsController(ApplicationUserManager, DbContext, ComponentContext, Cache.DataCacher);
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
