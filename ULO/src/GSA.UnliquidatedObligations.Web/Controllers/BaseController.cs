using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Autofac;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IComponentContext ComponentContext;
        public BaseController(IComponentContext componentContext)
        {
            ComponentContext = componentContext;
            System.Web.HttpContext.Current.Items["ComponentContext"] = ComponentContext;
        }
        // GET: Base

    }
}