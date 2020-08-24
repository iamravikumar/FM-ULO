using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Authorization;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    public class DebuggingController : BasePageController
    {
        private readonly IConfiguration Configuration;
        private readonly IOptions<Config> ConfigOptions;

        public static class ActionNames
        {
            public const string Settings = "Settings";
        }

        public class Config
        {
            public const string ConfigSectionName = "DebuggingControllerConfig";
            public bool EnableSettingsPage { get; set; }
        }

        public DebuggingController(IConfiguration configuration, IOptions<Config> configOptions, UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger<DebuggingController> logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            Configuration = configuration;
            ConfigOptions = configOptions;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ApplicationPermissionAuthorize(ApplicationPermissionNames.BackgroundJobDashboard)]
        [ActionName(ActionNames.Settings)]
        public ActionResult Settings()
        {
            if (ConfigOptions.Value.EnableSettingsPage)
            {
                return View(new SettingsModel(Configuration));
            }
            else
            {
                return Content($"{nameof(Settings)} Not Enabled");
            }
        }
    }
}
