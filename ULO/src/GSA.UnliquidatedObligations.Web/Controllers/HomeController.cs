using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core.Caching;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ULODBEntities db, IComponentContext componentContext, ICacher cacher)
            : base(db, componentContext, cacher)
        { }

        public ActionResult Index()
        {
            return View();
        }
    }
}