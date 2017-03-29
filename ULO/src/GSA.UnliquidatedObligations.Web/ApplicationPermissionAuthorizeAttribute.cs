using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using System;
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
                return httpContext.GetOwinContext().Authentication.User.Claims.GetApplicationPerimissionRegions(ApplicationPermission) != RegionNumbers.NoRegions;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
