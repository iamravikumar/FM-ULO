using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GSA.UnliquidatedObligations.Web.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        public PermissionHandler()
        { }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var claims = context.User.FindAll(c => c.Type == requirement.ClaimType).ToList();
            if (claims.Count != 0)
            {
                foreach (var c in claims)
                {
                    if (requirement.Matches(c))
                    { 
                        context.Succeed(requirement);
                        break;
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
