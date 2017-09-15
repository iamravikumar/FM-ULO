using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using System;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web
{
    public class ApplicationPermissionAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly ApplicationPermissionNames[] ApplicationPermissions;

        public ApplicationPermissionAuthorizeAttribute(params ApplicationPermissionNames[] applicationPermissions)
        {
            ApplicationPermissions = applicationPermissions;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            try
            {
                var user = httpContext.GetOwinContext().Authentication.User;
                foreach (var p in ApplicationPermissions)
                {
                    if (HasPermission(user, p)) return true;
                }
            }
            catch (Exception)
            { }
            return false;
        }

        public static bool HasPermission(ClaimsPrincipal user, ApplicationPermissionNames permission)
        {
            try
            {
                return user.Claims.GetApplicationPerimissionRegions(permission).Count > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
