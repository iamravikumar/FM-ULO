using System.Web.Mvc;
using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class HomeController : BaseController
    {
        protected readonly ULODBEntities DB;

        public HomeController(ULODBEntities db, IComponentContext componentContext) 
            : base(componentContext)
        {
            DB = db;
        }



        public ActionResult Index()
        {
            return View();
        }



    }
}