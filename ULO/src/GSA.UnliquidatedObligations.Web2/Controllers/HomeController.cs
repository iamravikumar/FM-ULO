using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    public class HomeController : BasePageController
    {
        public static readonly string Name = AspHelpers.GetControllerName<HomeController>();

        public static class ActionNames
        {
            public const string About = "About";
        }

        public HomeController(UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger<HomeController> logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        { }

        public IActionResult Index()
            => RedirectToAction(UloController.ActionNames.Home, UloController.Name);

        [AllowAnonymous]
        [ActionName(ActionNames.About)]
        public ActionResult About()
            => View();
    }
}
