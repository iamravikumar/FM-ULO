using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace GSA.UnliquidatedObligations.Web.Tests.Mocks
{
    internal class ApplicationUserManagerMock
    {
        public ApplicationUserManager SetupApplicationUserManagerMock(IUserStore<ApplicationUser> store, IdentityFactoryOptions<ApplicationUserManager> factoryOptions)
        {
            return new ApplicationUserManager(store, factoryOptions);
        }
    }
}
