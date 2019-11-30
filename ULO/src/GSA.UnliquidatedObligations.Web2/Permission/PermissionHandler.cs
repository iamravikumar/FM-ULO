using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;


namespace GSA.UnliquidatedObligations.Web.Permission
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider ServiceProvider;
        public PermissionHandler(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var UserManager = ServiceProvider.GetRequiredService<UserManager<AspNetUser>>();

            var user = await UserManager.GetUserAsync(context.User);

            var claimList = (await UserManager.GetClaimsAsync(user)).Select(p => p.Value);

            foreach (var currentClaim in claimList)
            {
                var pcv = ApplicationPermissionClaimValue.Load(currentClaim);
                if (pcv.ApplicationPermissionName == requirement.PermissionName)
                {
                    context.Succeed(requirement);
                    break;
                }
            }
        }

    }
}
