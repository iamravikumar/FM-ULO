using Microsoft.AspNetCore.Mvc;
using Serilog;
using RevolutionaryStuff.Core.Caching;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Authorization;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : BasePageController
    {
        public const string Name = "Home";

        public static class ActionNames
        {
            public const string About = "About";
        }

        public HomeController(UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, ILogger logger)
            : base(db, cacher, portalHelpers, logger)
        { }

        public IActionResult Index()
            => RedirectToAction(UloController.ActionNames.Home, UloController.Name);

        [ActionName(ActionNames.About)]
        public ActionResult About() 
            => View();
    }
}
