using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Moq;

namespace GSA.UnliquidatedObligations.Web.Tests.Mocks
{
    internal class ControllerContextMock
    {
        public ControllerContext SetupControllerContextMock(AspNetUser currentUser)
        {
            var mockContext = new Mock<ControllerContext>();
            mockContext.SetupGet(p => p.HttpContext.User.Identity.Name).Returns(currentUser.UserName);
            mockContext.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(true);
            return mockContext.Object;
        }
    }
}
