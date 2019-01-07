#if false

using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;

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

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var c = filterContext.Controller as Controllers.BaseController;
            if (c != null)
            {
                c.Log.Error("Unauthorized access attempt.  User does not have any of the specified application persissions {@ApplicationPermissions}", ApplicationPermissions);
            }
            base.HandleUnauthorizedRequest(filterContext);
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

#endif
