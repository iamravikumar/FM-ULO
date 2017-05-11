using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using static GSA.UnliquidatedObligations.BusinessLayer.Authorization.RegionNumbers;

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