using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevolutionaryStuff.Core.Caching;
using Serilog;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    public class HomeController : BasePageController
    {
        public const string Name = "Home";
        private readonly IBackgroundTasks BackgroundTasks;

        public static class ActionNames
        {
            public const string About = "About";
        }

        public HomeController(UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger logger, IBackgroundTasks backgroundTasks)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            BackgroundTasks = backgroundTasks;
        }

        public IActionResult Index()
            => RedirectToAction(UloController.ActionNames.Home, UloController.Name);

        [AllowAnonymous]
        [ActionName(ActionNames.About)]
        public async System.Threading.Tasks.Task<ActionResult> About()
        {
            await BackgroundTasks.Email("jason@jasonthomas.com", 9, null);
            return View();
        }
//            => View();
    }
}
