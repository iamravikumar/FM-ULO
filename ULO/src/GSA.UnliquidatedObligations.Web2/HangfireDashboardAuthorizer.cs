using Hangfire.Dashboard;

namespace GSA.UnliquidatedObligations.Web
{
    public class HangfireDashboardAuthorizer : IDashboardAuthorizationFilter
    {
        bool IDashboardAuthorizationFilter.Authorize(DashboardContext context)
        {
            return true;
        }
    }
}
