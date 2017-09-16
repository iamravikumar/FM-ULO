using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core.Caching;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class HomeController : BaseController
    {
        public const string Name = "Home";

        public static class ActionNames
        {
            public const string About = "About";
        }

        public HomeController(ULODBEntities db, IComponentContext componentContext, ICacher cacher, Serilog.ILogger logger)
            : base(db, componentContext, cacher, logger)
        { }

        [AllowAnonymous]
        [ActionName(ActionNames.About)]
        public ActionResult About() => View();
    }
}