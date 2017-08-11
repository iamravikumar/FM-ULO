using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class RequestForReassignmentsController : BaseController
    {
        protected readonly IWorkflowManager Manager;
        private readonly ApplicationUserManager UserManager;

        public RequestForReassignmentsController(IWorkflowManager manager, ApplicationUserManager userManager, ULODBEntities db, IComponentContext componentContext, ICacher cacher)
            : base(db, componentContext, cacher)
        {
            Manager = manager;
            UserManager = userManager;
        }

        // GET: RequestForReassignments
        public async Task<ActionResult> Index(string sortCol, string sortDir, int? page, int? pageSize)
        {
            //var currentUser = await UserManager.FindByNameAsync(this.User.Identity.Name);
            //var reassignGroupUser = await UserManager.FindByNameAsync(Properties.Settings.Default.ReassignGroupUserName);

            //var reassignGroupRegionIds = await DB.UserUsers
            //    .Where(uu => uu.ParentUserId == reassignGroupUser.Id && uu.ChildUserId == currentUser.Id)
            //    .Select(uu => uu.RegionId)
            //    .Distinct()
            //    .ToListAsync();

            //var workflows = ApplyBrowse(
            //    DB.Workflows.Where(wf => wf.OwnerUserId == reassignGroupUser.Id && reassignGroupRegionIds.Contains(wf.UnliquidatedObligation.RegionId))
            //    .Include(wf => wf.UnliquidatedObligation),
            //    sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);
            //ViewBag.ShowReassignButton = true;
            //return View("~/Views/Ulo/Index.cshtml", workflows);
            return View();
        }

        // GET: RequestForReassignments/Details/5
        public ActionResult Details(int? id, int workflowId, int uloRegionId, string wfDefintionOwnerName = "", bool isAdmin = false)
        {
            var requestForReassignment = DB.RequestForReassignments.Where(rr => rr.IsActive).FirstOrDefault(r => r.RequestForReassignmentID == id);

            var workflow = DB.Workflows.FirstOrDefault(wf => wf.WorkflowId == workflowId);
            var wfDesc = Manager.GetWorkflowDescriptionAsync(workflow).Result;
            AspNetUser groupOwnerUser;
            if (wfDefintionOwnerName == "")
            {
                var currentActivity = wfDesc.WebActionWorkflowActivities
                    .FirstOrDefault(a => a.WorkflowActivityKey == workflow.CurrentWorkflowActivityKey);
                groupOwnerUser = DB.AspNetUsers.FirstOrDefault(u => u.UserName == currentActivity.OwnerUserName);
            }
            else
            {
                groupOwnerUser = DB.AspNetUsers.FirstOrDefault(u => u.UserName == wfDefintionOwnerName);   
            }
            

            var usersIds = DB.UserUsers
                .Where(uu => uu.ParentUserId == groupOwnerUser.Id && uu.RegionId == uloRegionId)
                .Select(uu => uu.ChildUserId).ToList();
            var users = DB.AspNetUsers
                .Where(u => u.UserType == AspNetUser.UserTypes.Person && usersIds.Contains(u.Id)).OrderBy(u => u.UserName).ToList();

            var userReassignRegions = User.GetReassignmentGroupRegions();
            if (User.HasPermission(ApplicationPermissionNames.CanReassign) && userReassignRegions.Contains(uloRegionId))
            {
                users.Add(CurrentUser);
            }

            users = users.OrderBy(u => u.UserName).ToList();

            var requestForReassignmentId = requestForReassignment?.RequestForReassignmentID;
            var suggestedReviewerId = requestForReassignment != null ? requestForReassignment.SuggestedReviewerId : "";
            var justificationKey = requestForReassignment?.UnliqudatedObjectsWorkflowQuestion.JustificationKey;

            var comments = requestForReassignment != null
                ? requestForReassignment.UnliqudatedObjectsWorkflowQuestion.Comments : "";

            var detailsView = isAdmin ? "_DetailsMasterList.cshtml" : "_Details.cshtml"; 
            return PartialView(
                "~/Views/Ulo/Details/Workflow/RequestForReassignments/" + detailsView, 
                new RequestForReassignmentViewModel(suggestedReviewerId, justificationKey, requestForReassignmentId, comments, workflowId, uloRegionId, users, wfDesc.GetResassignmentJustifications()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReassignFromList(
            int workflowId, 
            [Bind(Include =
                nameof(RequestForReassignmentViewModel.SuggestedReviewerId)+","+
                nameof(RequestForReassignmentViewModel.Comments)+","+
                nameof(RequestForReassignmentViewModel.JustificationKey)
            )]
            RequestForReassignmentViewModel requestForReassignmentViewModel)
        {
            var pageToRedirectTo = Request.UrlReferrer.AbsolutePath.Replace("/Ulo/", "");
            if (ModelState.IsValid)
            {
                var wf = await FindWorkflowAsync(workflowId, false);
                if (wf == null) return HttpNotFound();
                var question = new UnliqudatedObjectsWorkflowQuestion
                {
                    JustificationKey = requestForReassignmentViewModel.JustificationKey,
                    UserId = CurrentUserId,
                    Answer = "Reassigned",
                    WorkflowId = workflowId,
                    Comments = requestForReassignmentViewModel.Comments,
                    WorkflowRowVersion = wf.WorkflowRowVersion,
                    CreatedAtUtc = DateTime.UtcNow
                };
                var ret = await Manager.ReassignAsync(wf, requestForReassignmentViewModel.SuggestedReviewerId, pageToRedirectTo);
                await DB.SaveChangesAsync();
                return ret;
            }
            
            return RedirectToAction(pageToRedirectTo, "Ulo");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reassign(
            int workflowId,
            [Bind(Include =
                nameof(RequestForReassignmentViewModel.SuggestedReviewerId)+","+
                nameof(RequestForReassignmentViewModel.Comments)+","+
                nameof(RequestForReassignmentViewModel.JustificationKey)
            )]
            RequestForReassignmentViewModel requestForReassignmentViewModel)
        {
            if (ModelState.IsValid)
            {
                var wf = await FindWorkflowAsync(workflowId, false);
                if (wf == null) return HttpNotFound();
                var requestForReassignment =
                    await DB.RequestForReassignments.FirstOrDefaultAsync(
                        rr => rr.WorkflowId == workflowId);
                var question = new UnliqudatedObjectsWorkflowQuestion
                {
                    JustificationKey = requestForReassignmentViewModel.JustificationKey,
                    UserId = CurrentUserId,
                    Answer = "Reassigned",
                    WorkflowId = workflowId,
                    Comments = requestForReassignmentViewModel.Comments,
                    WorkflowRowVersion = wf.WorkflowRowVersion,
                    CreatedAtUtc = DateTime.UtcNow
                };
                DB.UnliqudatedObjectsWorkflowQuestions.Add(question);
                await DB.SaveChangesAsync();

                if (requestForReassignment == null && User.HasPermission(ApplicationPermissionNames.CanReassign))
                {
                    var ret = await Manager.ReassignAsync(wf, requestForReassignmentViewModel.SuggestedReviewerId, "Index");
                    await DB.SaveChangesAsync();
                    return ret;
                }
                else if (requestForReassignment != null)
                {
                    return await Reassign(wf, question, requestForReassignment, requestForReassignmentViewModel.SuggestedReviewerId);
                }
            }

            return PartialView("~/Views/Ulo/Details/Workflow/RequestForReassignments/_Details.cshtml", requestForReassignmentViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RequestReassign(
            int workflowId, 
            [Bind(Include =
            nameof(RequestForReassignmentViewModel.WorkflowId)+","+
            nameof(RequestForReassignmentViewModel.SuggestedReviewerId)+","+
            nameof(RequestForReassignmentViewModel.JustificationKey)+","+
            nameof(RequestForReassignmentViewModel.Comments))]
        RequestForReassignmentViewModel requestForReassignmentViewModel)
        {
            if (ModelState.IsValid)
            {
                var wf = await FindWorkflowAsync(workflowId);
                if (wf == null) return HttpNotFound();
                var question = new UnliqudatedObjectsWorkflowQuestion
                {
                    JustificationKey = requestForReassignmentViewModel.JustificationKey,
                    UserId = CurrentUserId,
                    Answer = "Request for Reasssignment",
                    WorkflowId = workflowId,
                    Comments = requestForReassignmentViewModel.Comments,
                    WorkflowRowVersion = wf.WorkflowRowVersion,
                    CreatedAtUtc = DateTime.UtcNow
                };
                DB.UnliqudatedObjectsWorkflowQuestions.Add(question);
                await DB.SaveChangesAsync();

                return await RequestReassign(wf, question, requestForReassignmentViewModel.SuggestedReviewerId);
            }

            return PartialView("~/Views/Ulo/Details/Workflow/RequestForReassignments/_Details.cshtml", requestForReassignmentViewModel);
        }


        // GET: RequestForReassignments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestForReassignment requestForReassignment = await DB.RequestForReassignments.FindAsync(id);
            if (requestForReassignment == null)
            {
                return HttpNotFound();
            }
            return View(requestForReassignment);
        }

        // POST: RequestForReassignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RequestForReassignment requestForReassignment = await DB.RequestForReassignments.FindAsync(id);
            DB.RequestForReassignments.Remove(requestForReassignment);
            await DB.SaveChangesAsync();
            return RedirectToIndex();
        }

        //TODO: Move to Manager?
        private async Task<Workflow> FindWorkflowAsync(int workflowId, bool checkOwner = true)
        {
            var wf = await DB.Workflows.Include(q => q.AspNetUser).Include(q => q.UnliquidatedObligation).FirstOrDefaultAsync(q => q.WorkflowId == workflowId);
            if (wf != null)
            {
                if (checkOwner == false) return wf;
                if (CurrentUserId != null)
                {
                    var groupsUserBelongsTo = await GetUsersGroups(CurrentUserId);
                    if (wf.OwnerUserId == CurrentUserId || groupsUserBelongsTo.Contains(wf.OwnerUserId)) return wf;
                    if (wf.AspNetUser.UserType == UserTypes.Group.ToString())
                    {
                        //TODO: Write recursive then call recursive sproc to see if current user is in the group
                    }
                }
            }
            return null;
        }

        private async Task<ActionResult> RequestReassign(Workflow wf, UnliqudatedObjectsWorkflowQuestion question, string suggestedReviewerId)
        {
            var requestForReassignment = new RequestForReassignment
            {
                SuggestedReviewerId = suggestedReviewerId,
                UnliqudatedWorkflowQuestionsId = question.UnliqudatedWorkflowQuestionsId,
                WorkflowId = wf.WorkflowId,
                IsActive = true

            };
            DB.RequestForReassignments.Add(requestForReassignment);
            var ret = await Manager.RequestReassignAsync(wf);
            await DB.SaveChangesAsync();
            return ret;
        }

        private async Task<ActionResult> Reassign(Workflow wf, UnliqudatedObjectsWorkflowQuestion question, RequestForReassignment requestForReassignment, string suggestedReviewerId)
        {
            requestForReassignment.SuggestedReviewerId = suggestedReviewerId;
            requestForReassignment.UnliqudatedWorkflowQuestionsId = question.UnliqudatedWorkflowQuestionsId;
            requestForReassignment.WorkflowId = wf.WorkflowId;
            requestForReassignment.IsActive = false;
            var ret = await Manager.ReassignAsync(wf, suggestedReviewerId, "Index");
            //TODO: add redirect method here.
            await DB.SaveChangesAsync();
            return ret;
        }


        private async Task<List<string>> GetUsersGroups(string userId)
        {
            return await DB.UserUsers.Where(uu => uu.ChildUserId == userId).Select(uu => uu.ParentUserId).ToListAsync();
        }

    }
}
