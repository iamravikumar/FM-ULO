using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNet.Identity;
using Moq;

namespace GSA.UnliquidatedObligations.Web.Tests.Mocks
{
    internal class UserStoreMock
    {
        public IUserStore<ApplicationUser> SetupUserStoreMock(AspNetUser currentUser)
        {
            var mockStore = new Mock<IUserStore<ApplicationUser>>();

            var dummyUser = new ApplicationUser() { Id = currentUser.Id, UserName = currentUser.UserName, Email = currentUser.Email };
            mockStore.Setup(x => x.FindByNameAsync(currentUser.UserName))
                        .Returns(Task.FromResult(dummyUser));

            return mockStore.Object;
        }
    }
}
