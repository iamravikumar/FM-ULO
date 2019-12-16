using GSA.UnliquidatedObligations.BusinessLayer.Authorization;

namespace GSA.UnliquidatedObligations.Web.Authorization
{
    public class ApplicationPermissionAuthorize : PermissionAuthorizeAttributeBase
    {
        public ApplicationPermissionAuthorize(ApplicationPermissionNames permission)
            : base(nameof(ApplicationPermissionNames), typeof(ApplicationPermissionNames), permission)
        { }

        public static string GetPolicyName(ApplicationPermissionNames permission)
            => CreatePolicyName(nameof(ApplicationPermissionNames), typeof(ApplicationPermissionNames), permission);
    }
}
