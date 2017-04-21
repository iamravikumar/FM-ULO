using System;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GSA.UnliquidatedObligations.Web.Tests.Controllers
{
    [TestClass]
    public class UloControllerTests
    {
        private UloController uloController;
        [TestInitialize]
        public void Initialize()
        {
            var wfManager = new Mock<IWorkflowManager>();
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
