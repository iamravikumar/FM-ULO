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
    [TestClass]
    public class DocumentControllerTests : ControllerTests
    {

        private DocumentsController Controller;
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            //Controller = new DocumentsController(DbContext, U);

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
