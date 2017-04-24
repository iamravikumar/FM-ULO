using System;
using System.Data.Entity;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Models;
using Hangfire;
using Microsoft.AspNet.Identity;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    //[ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    public class UloController : Controller
    {
        protected readonly IWorkflowManager Manager;
        protected readonly ULODBEntities DB;
        private readonly ApplicationUserManager UserManager;

        public UloController(IWorkflowManager manager, ULODBEntities db, ApplicationUserManager userManager)
        {
            Manager = manager;
            DB = db;
            UserManager = userManager;
        }

        // GET: Ulo

        public async Task<ActionResult> Index()
        {
            var currentUser = await UserManager.FindByNameAsync(this.User.Identity.Name);
            var workflows = DB.Workflows.Where(wf => wf.OwnerUserId == currentUser.Id).Include(wf => wf.UnliquidatedObligation);
            return View(workflows);
        }

        [Route("Ulo/{id}")]
        public async Task<ActionResult> Details(int uloId, int workflowId)
        {
            var ulo = await DB.UnliquidatedObligations.Include(u => u.Notes).FirstOrDefaultAsync(u => u.UloId == uloId);
            var workflow = await FindWorkflowAsync(workflowId);
            var workflowDesc = await FindWorkflowDescAsync(workflow);
            return View("Details/Index", new UloViewModel(ulo, workflow, workflowDesc));
        }


        private async Task<IWorkflowDescription> FindWorkflowDescAsync(Workflow wf)
        {
            return await Manager.GetWorkflowDescription(wf);
        }


        private async Task<Workflow> FindWorkflowAsync(int workflowId)
        {
            var wf = await DB.Workflows.Include(q => q.AspNetUser).Include(q => q.UnliquidatedObligation).FirstOrDefaultAsync(q => q.WorkflowId == workflowId);
            if (wf != null)
            {
                var currentUser = await UserManager.FindByNameAsync(this.User.Identity.Name);
                if (currentUser != null)
                {
                    if (wf.OwnerUserId == currentUser.Id) return wf;
                    if (wf.AspNetUser.UserType == UserTypes.Group.ToString())
                    {
                        //TODO: Write recursive then call recursive sproc to see if current user is in the group
                    }
                }
            }
            return null;
        }

        //Referred to by WebActionWorkflowActivity
        //TODO: Attributes will probably change
        [ActionName("Advance")]
        [Route("Advance/{workflowId}")]
        public async Task<ActionResult> Advance(int workflowId)
        {
            var wf = await FindWorkflowAsync(workflowId);
            if (wf == null) return HttpNotFound();
            return View(new FormAModel(wf));
        }

        //TODO: be able to open either ULO or workflow
        //TODO: Attributes will probably change
        [HttpPost]
        [ActionName("Advance")]
        [Route("Advance/{workflowId}")]
        public async Task<ActionResult> Advance(
            int workflowId,
            int uloId,
            [Bind(Include = "DOShouldBe,UDOShouldBe")]
            UloViewModel uloModel,
            [Bind(Include = "Justification,Answer")]
            AdvanceViewModel advanceModel)
        {
            var wf = await FindWorkflowAsync(workflowId);
            if (wf == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                //
               
                wf.UnliquidatedObligation.DOShouldBe = uloModel.DOShouldBe;
                wf.UnliquidatedObligation.UDOShouldBe = uloModel.UDOShouldBe;
                var user = await DB.AspNetUsers.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                var question = new UnliqudatedObjectsWorkflowQuestion
                {
                    Date = DateTime.Now,
                    Justification = advanceModel.Justification,
                    UserId = user.Id,
                    Answer = advanceModel.Answer,
                    WorkflowId = workflowId,

                };
                return await AdvanceAsync(wf, question);
            }
            return await Details(uloId, workflowId);
        }

        private async Task<ActionResult> AdvanceAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question)
        {
            var ret = await Manager.AdvanceAsync(wf, question);
            await DB.SaveChangesAsync();
            return ret;
        }
    }

    
}