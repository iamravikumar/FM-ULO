using System.Web.Mvc;
using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ULODBEntities db, IComponentContext componentContext)
            : base(db, componentContext)
        { }

        public ActionResult Index()
        {
            return View();
        }
    }
}