using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GSA.UnliquidatedObligations.Web.Authorization
{
    public abstract class PermissionRequirement : IAuthorizationRequirement
    {
        protected PermissionRequirement(string claimType, string permissionName)
        {
            ClaimType = claimType;
            PermissionName = permissionName;
        }

        public string ClaimType { get; protected set; }
        public string PermissionName { get; protected set; }

        public override string ToString() => $"{GetType().Name} permission={PermissionName} claimType=[{ClaimType}]";

        public virtual bool Matches(Claim c)
            => c.Type == ClaimType && c.Value == PermissionName;
    }
}
