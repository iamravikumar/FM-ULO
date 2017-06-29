using Microsoft.Owin;
using Hangfire.Dashboard;
using System;

[assembly: OwinStartupAttribute(typeof(GSA.UnliquidatedObligations.Web.Startup))]
namespace GSA.UnliquidatedObligations.Web
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            try
            {
                var owinContext = new OwinContext(context.GetOwinEnvironment());

                var user = owinContext.Authentication.User;

                return
                    user.Identity.IsAuthenticated &&
                    ApplicationPermissionAuthorizeAttribute.HasPermission(user, BusinessLayer.Authorization.ApplicationPermissionNames.BackgroundJobDashboard);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
