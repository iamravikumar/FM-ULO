using System;
using System.Collections.Generic;
using System.Data.Entity;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
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
            //TODO: wrire stored procedure for nested groups
            var currentUser = await UserManager.FindByNameAsync(this.User.Identity.Name);
            var workflowsAssignedtoCurrentUser = DB.Workflows.Where(wf => wf.OwnerUserId == currentUser.Id).Include(wf => wf.UnliquidatedObligation);
            var groupsUserBelongsTo = await GetUsersGroups(currentUser.Id);
            var workflowsAssignedToUsersGroups = DB.Workflows.Where(wf => groupsUserBelongsTo.Contains(wf.OwnerUserId));
            var workflows = workflowsAssignedtoCurrentUser.Concat(workflowsAssignedToUsersGroups);
            return View(workflows);
        }


        [Route("Ulo/{id}")]
        public async Task<ActionResult> Details(int uloId, int workflowId)
        {
            //TODO: check if current user is able to view
            var ulo = await DB.UnliquidatedObligations.Include(u => u.Notes).FirstOrDefaultAsync(u => u.UloId == uloId);
            var workflow = await FindWorkflowAsync(workflowId);
            var workflowDesc = await FindWorkflowDescAsync(workflow);
            return View("Details/Index", new UloViewModel(ulo, workflow, workflowDesc));
        }

        private async Task<IWorkflowDescription> FindWorkflowDescAsync(Workflow wf)
        {
            return await Manager.GetWorkflowDescription(wf);
        }

        
        //TODO: Move to Manager?
        private async Task<Workflow> FindWorkflowAsync(int workflowId)
        {
            var wf = await DB.Workflows.Include(q => q.AspNetUser).Include(q => q.UnliquidatedObligation).FirstOrDefaultAsync(q => q.WorkflowId == workflowId);
            if (wf != null)
            {
                var currentUser = await UserManager.FindByNameAsync(this.User.Identity.Name);
                var groupsUserBelongsTo = await GetUsersGroups(currentUser.Id);
                if (currentUser != null)
                {
                    if (wf.OwnerUserId == currentUser.Id || groupsUserBelongsTo.Contains(wf.OwnerUserId)) return wf;
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
            [Bind(Include = "Justification,Answer,Comments")]
            AdvanceViewModel advanceModel)
        {
            var wf = await FindWorkflowAsync(workflowId);
            if (wf == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                wf.UnliquidatedObligation.DOShouldBe = uloModel.DOShouldBe;
                wf.UnliquidatedObligation.UDOShouldBe = uloModel.UDOShouldBe;
                var user = await DB.AspNetUsers.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                var question = new UnliqudatedObjectsWorkflowQuestion
                {
                    Date = DateTime.Now,
                    JustificationId = advanceModel.JustificationId,
                    UserId = user.Id,
                    Answer = advanceModel.Answer,
                    WorkflowId = workflowId,
                    Comments = advanceModel.Comments
                };
                return await AdvanceAsync(wf, question);
            }
            return await Details(uloId, workflowId);
        }

        private async Task<List<string>> GetUsersGroups(string userId)
        {
            return await DB.UserUsers.Where(uu => uu.ChildUserId == userId).Select(uu => uu.ParentUserId).ToListAsync();
        }

        private async Task<ActionResult> AdvanceAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question)
        {
            var ret = await Manager.AdvanceAsync(wf, question);
            await DB.SaveChangesAsync();
            return ret;
        }
    }

    
}