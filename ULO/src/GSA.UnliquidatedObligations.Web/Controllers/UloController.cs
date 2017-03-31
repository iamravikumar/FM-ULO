//TODO: Ask Jason about

using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    public class UloController : Controller
    {
        protected readonly IWorkflowManager Manager;
        protected readonly ULODBEntities DB;
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public UloController(IWorkflowManager manager, ULODBEntities db)
        {
            Manager = manager;
            DB = db;
        }

        // GET: Ulo
        public async Task<ActionResult> Index()
        {
            var currentUser = await UserManager.FindByNameAsync(this.User.Identity.Name);
            var workflows = DB.Workflows.Where(wf => wf.OwnerUserId == currentUser.Id);
            return View(workflows);
        }

        //private async Task<Workflow> FindWorkflowAsync(int workflowId)
        //{
        //    var wf = await DB.Workflows.Include(q => q.OwnerUser).Include(q => q.TargetUlo).FirstOrDefaultAsync(q => q.WorkflowId == workflowId);
        //    if (wf != null)
        //    {
        //        var currentUser = await UserManager.FindByNameAsync(this.User.Identity.Name);
        //        if (currentUser != null)
        //        {
        //            if (wf.OwnerUserId == currentUser.Id) return wf;
        //            if (wf.OwnerUser.UserType == UserTypes.Group.ToString())
        //            {
        //                //TODO: Write recursive then call recursive sproc to see if current user is in the group
        //            }
        //        }
        //    }
        //    return null;
        //}

        //Referred to by WebActionWorkflowActivity
        //[ActionName("FormA")]
        //[Route("FormA/{workflowId}")]
        //public async Task<ActionResult> FormA(int workflowId)
        //{
        //    var wf = await FindWorkflowAsync(workflowId);
        //    if (wf == null) return HttpNotFound();
        //    return View(new FormAModel(wf));
        //}

        //[HttpPost]
        //[ActionName("FormA")]
        //[Route("FormA/{workflowId}")]
        //public async Task<ActionResult> FormA(
        //    int workflowId,
        //    [Bind(Include = nameof(FormAModel.Field0Value))]
        //    FormAModel model)
        //{
        //    var wf = await FindWorkflowAsync(workflowId);
        //    if (wf == null) return HttpNotFound();
        //    if (ModelState.IsValid)
        //    {
        //        wf.TargetUlo.FieldS0 = model.Field0Value;
        //        return await AdvanceAsync(wf);
        //    }
        //    return View(model);
        //}

        //private async Task<ActionResult> AdvanceAsync(Workflow wf)
        //{
        //    var ret = await Manager.AdvanceAsync(wf);
        //    await DB.SaveChangesAsync();
        //    return ret;
        //}
    }
}