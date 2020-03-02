using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RevolutionaryStuff.Core.Caching;
using Serilog;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    public class HomeController : BasePageController
    {
        public const string Name = "Home";
        private readonly RazorTemplateProcessor Dt;

        public static class ActionNames
        {
            public const string About = "About";
        }

        public HomeController(UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger logger, RazorTemplateProcessor dt)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            Dt = dt;
        }

        public IActionResult Index()
            => RedirectToAction(UloController.ActionNames.Home, UloController.Name);

        [AllowAnonymous]
        [ActionName(ActionNames.About)]
        public async System.Threading.Tasks.Task<ActionResult> About()
        {
            var u = await DB.AspNetUsers.FindAsync("2DEF2B8B-3EE9-48FC-AB0B-BEE0978E141C");
            var items = DB.Workflows.Include(z => z.TargetUlo).ThenInclude(z => z.Review).Take(10).ToList();
            var model = new WorkflowsEmailViewModel(u, items);
            var s = await Dt.ProcessAsync(
@"@model GSA.UnliquidatedObligations.Web.Models.WorkflowsEmailViewModel
@{Layout=null;}
Dear @Model.UserName,         <br/><br/>    You have been assigned @Model.ItemCount ULOs for your review. Please click on the PDN to link to the ULO database. If you have any questions, please contact your regional ULO coordinator.  <br/><br/>      <html>   <body>    <style>     table {      border-collapse: collapse;     }    </style>    <table border = ""1""  CELLPADDING=""7"">       <thead>          <tr>            <th>PDN</th>               <th>Org Code</th>            <th>BA Code</th>            <th>Contract #</th>           <th>Lease #</th>         <th>Building #</th>         <th>Project #</th>        </tr>        </thead>          <tbody>       @foreach(var i in Model.Items)         {              <tr>            <td><a href=""@Model.SiteUrl / Ulos / @i.TargetUloId / @i.WorkflowId"">@i.TargetUlo.PegasysDocumentNumber</a></td>                <td>@i.TargetUlo.Organization</td>            <td>@i.TargetUlo.Prog</td>            <td>@i.TargetUlo.ContractNum</td>        <td>@i.TargetUlo.LeaseNumber</td>        <td>@i.TargetUlo.Bldg</td>       <td>@i.TargetUlo.Project</td>             </tr>  }         </tbody>      </table>   </body>  </html>", model);
            RevolutionaryStuff.Core.Stuff.Noop(s);
            return View();
        }
//            => View();
    }
}
