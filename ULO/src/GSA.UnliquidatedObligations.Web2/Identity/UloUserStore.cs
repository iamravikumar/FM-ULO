using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GSA.UnliquidatedObligations.Web.Identity
{
    public class UloUserStore :
        UserStore<AspNetUser, AspNetRole, UloDbContext, string, AspNetUserClaim, AspNetUserRole, AspNetUserLogin, AspNetUserToken, AspNetRoleClaim>,
        IUserStore<AspNetUser>
    {
        public UloUserStore(UloDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }
    }
}
