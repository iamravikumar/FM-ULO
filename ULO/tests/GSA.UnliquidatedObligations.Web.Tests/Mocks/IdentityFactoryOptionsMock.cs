using Microsoft.AspNet.Identity.Owin;
using Moq;

namespace GSA.UnliquidatedObligations.Web.Tests.Mocks
{
    internal class IdentityFactoryOptionsMock
    {
        public IdentityFactoryOptions<ApplicationUserManager> SetupIdentityFactoryOptionsMock()
        {
            return new Mock<IdentityFactoryOptions<ApplicationUserManager>>().Object;
        }
    }
}
