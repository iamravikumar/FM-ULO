using Hangfire.Dashboard;

namespace GSA.UnliquidatedObligations.Web2
{
    public class HangfireDashboardAuthorizer : IDashboardAuthorizationFilter
    {
        bool IDashboardAuthorizationFilter.Authorize(DashboardContext context)
        {
            return true;
        }
    }
}
