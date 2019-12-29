using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GSA.UnliquidatedObligations.Web.Identity
{
    public class UloSignInManager : SignInManager<AspNetUser>
    {
        public UloSignInManager(
            UloUserManager userManager, 
            IHttpContextAccessor contextAccessor, 
            IUserClaimsPrincipalFactory<AspNetUser> claimsFactory, 
            IOptions<IdentityOptions> optionsAccessor, 
            ILogger<SignInManager<AspNetUser>> logger, 
            IAuthenticationSchemeProvider schemes)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, null)
        { }
    }
}
