using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;


namespace GSA.UnliquidatedObligations.Web.Permission
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        public const string GsaUrn = "urn:gsa.gov/";
        public const string UloUrn = GsaUrn + "unliquidatedObligation/";
        public const string ClaimTypePrefix = UloUrn + "claims/";
        public const string ClaimType = ClaimTypePrefix + "ApplicationPermissionClaim";

        private readonly IServiceProvider ServiceProvider;

        public PermissionHandler(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            
            var UserManager = ServiceProvider.GetRequiredService<UserManager<AspNetUser>>();

            var user = await UserManager.GetUserAsync(context.User);           

            var claimList = (await UserManager.GetClaimsAsync(user)).Select(p => p.Type);
            if (!claimList.Contains("ApplicationPermissionClaim"))
            {
                await UserManager.AddClaimAsync(user, new Claim("ApplicationPermissionClaim", "CanViewUnassigned"));
            }
                                                 
                foreach (var c in claimList)
                {                
                        context.Succeed(requirement);
                        break;                   
                }            
        }
       
    }
}
