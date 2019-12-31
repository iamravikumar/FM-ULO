using System.Security.Claims;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;

namespace GSA.UnliquidatedObligations.Web.Authorization
{
    public class ApplicationPermissionRequirement : PermissionRequirement
    {
        public ApplicationPermissionRequirement(ApplicationPermissionNames permission)
            : base(PermissionHelpers.CreateClaimType(permission), permission.ToString())
        { }

        public override bool Matches(Claim c)
        {
            if (c.Type != ClaimType) return false;
            var cv = ApplicationPermissionClaimValue.Load(c.Value);
            return cv!=null && cv.ApplicationPermissionName.ToString()== PermissionName;
        }
    }
}
