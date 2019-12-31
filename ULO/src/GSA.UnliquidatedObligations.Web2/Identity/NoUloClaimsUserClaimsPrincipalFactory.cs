using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GSA.UnliquidatedObligations.Web.Identity
{
    /// <summary>
    /// This is here to remove the any subject category claims from the claims principal
    /// This is needed because this app has gobs of claims, and when serialized, these blow out the cookie
    /// </summary>
    public class NoUloClaimsUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AspNetUser>
    {
        public NoUloClaimsUserClaimsPrincipalFactory(UserManager<AspNetUser> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        { }

        protected async override Task<ClaimsIdentity> GenerateClaimsAsync(AspNetUser user)
        {
            var ret = await base.GenerateClaimsAsync(user);
            var toRemove = ret.Claims.Where(c => c.Type==SubjectCatagoryClaimValue.ClaimType).ToList();
            foreach (var c in toRemove)
            {
                ret.TryRemoveClaim(c);
            }
            return ret;
        }
    }
}
