using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using System;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web
{
    public class ApplicationPermissionAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly ApplicationPermissionNames ApplicationPermission;

        public ApplicationPermissionAuthorizeAttribute(ApplicationPermissionNames applicationPermission)
        {
            ApplicationPermission = applicationPermission;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            try
            {
                var user = httpContext.GetOwinContext().Authentication.User;
                return HasPermission(user, ApplicationPermission);
            }
            catch (Exception)
            {
                return false;
            }
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
