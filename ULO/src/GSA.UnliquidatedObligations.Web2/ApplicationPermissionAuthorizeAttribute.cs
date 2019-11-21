using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GSA.UnliquidatedObligations.Web
{
    public class ApplicationPermissionAuthorizeAttribute : TypeFilterAttribute
    {
        private readonly ApplicationPermissionNames[] ApplicationPermissions;
        public ApplicationPermissionAuthorizeAttribute(params ApplicationPermissionNames[] applicationPermissions)
        : base(typeof(AuthorizeActionFilter))
        {
            ApplicationPermissions = applicationPermissions;
        }
    }

    public class AuthorizeActionFilter : IAuthorizationFilter
    {
        private readonly ApplicationPermissionNames[] AppPermissions;
        public AuthorizeActionFilter(ApplicationPermissionNames[] ApplicationPermissions)
        {
            AppPermissions = ApplicationPermissions;
            
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool isAuthorized = false;

            foreach (var p in AppPermissions)
            {                
                isAuthorized = HasPermission(context.HttpContext.User, p);
            }             

            if (!isAuthorized)
            {
                context.Result = new ForbidResult();
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


