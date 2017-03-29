using System.Web.Mvc;
using System.Web.Routing;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IRedirectToAction
    {
        ActionResult Redirect(string controllerName, string actionName, RouteValueDictionary routeValues);
    }
}
