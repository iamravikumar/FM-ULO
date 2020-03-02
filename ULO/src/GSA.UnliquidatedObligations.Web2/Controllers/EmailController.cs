using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [AllowAnonymous]
    public class EmailController : Controller
    {
        private readonly UloDbContext DB;

        public EmailController(UloDbContext db)
        {
            DB = db;
        }

        [Route("/Email/{emailTemplateId}/{emailPart}")]
        public async Task<IActionResult> Index(int emailTemplateId, string emailPart)
        {
            var u = await DB.AspNetUsers.FindAsync("2DEF2B8B-3EE9-48FC-AB0B-BEE0978E141C");
            var items = DB.Workflows.Include(z=>z.TargetUlo).ThenInclude(z=>z.Review).Take(10).ToList();
            var m = new WorkflowsEmailViewModel(u, items);
            return View($"/Views/Email/{emailTemplateId}/{emailPart}.cshtml", m);
        } 
    }
}
