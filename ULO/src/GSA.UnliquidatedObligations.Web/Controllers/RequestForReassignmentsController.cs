using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    public class RequestForReassignmentsController : BaseController
    {
        public const string Name = "RequestForReassignments";

        public static class ActionNames
        {
            public const string Details = "Details";
        }

        protected readonly IWorkflowManager Manager;
        private readonly ApplicationUserManager UserManager;

        public RequestForReassignmentsController(IWorkflowManager manager, ApplicationUserManager userManager, ULODBEntities db, IComponentContext componentContext, ICacher cacher, Serilog.ILogger logger)
            : base(db, componentContext, cacher, logger)
        {
            Manager = manager;
            UserManager = userManager;
        }

        [ActionName(ActionNames.Details)]
        public ActionResult Details(int? id, int workflowId, int uloRegionId, string wfDefintionOwnerName = "", bool isAdmin = false)
        {
            RequestForReassignment requestForReassignment = null;
            if (id.HasValue)
            {
                requestForReassignment = DB.RequestForReassignments.Include(z=>z.UnliqudatedObjectsWorkflowQuestion).FirstOrDefault(r => r.RequestForReassignmentID == id.Value);
            }

            var workflow = DB.Workflows.FirstOrDefault(wf => wf.WorkflowId == workflowId);
            var wfDesc = Manager.GetWorkflowDescriptionAsync(workflow).Result;

            string groupOwnerId;
            if (wfDefintionOwnerName == "")
            {
                var currentActivity = wfDesc.WebActionWorkflowActivities
                    .FirstOrDefault(a => a.WorkflowActivityKey == workflow.CurrentWorkflowActivityKey);
                groupOwnerId = PortalHelpers.GetUserId(currentActivity.OwnerUserName);
            }
            else
            {
                groupOwnerId = PortalHelpers.GetUserId(wfDefintionOwnerName);
            }

            var userSelectItems = Cacher.FindOrCreateValWithSimpleKey(
                Cache.CreateKey(groupOwnerId, uloRegionId, "fdsfdsaf"),
                () => DB.UserUsers
                    .Where(uu => uu.ParentUserId == groupOwnerId && uu.RegionId == uloRegionId && uu.ChildUser.UserType == AspNetUser.UserTypes.Person)
                    .Select(uu => new { UserName = uu.ChildUser.UserName, UserId = uu.ChildUserId }).ConvertAll(z=>PortalHelpers.CreateUserSelectListItem(z.UserId, z.UserName)).AsReadOnly(),
                UloHelpers.MediumCacheTimeout
                    ).ToList();

            if (Cacher.FindOrCreateValWithSimpleKey(
                Cache.CreateKey(uloRegionId, User.Identity.Name),
                () => 
                {
                    var userReassignRegions = User.GetReassignmentGroupRegions();
                    return User.HasPermission(ApplicationPermissionNames.CanReassign) && userReassignRegions.Contains(uloRegionId);
                },
                UloHelpers.MediumCacheTimeout
                ))
            {
                userSelectItems.Add(CurrentUser.ToSelectListItem());
            }

            userSelectItems = userSelectItems.OrderBy(z => z.Text).ToList();

            var requestForReassignmentId = requestForReassignment?.RequestForReassignmentID;
            var suggestedReviewerId = requestForReassignment != null ? requestForReassignment.SuggestedReviewerId : "";
            var justificationKey = requestForReassignment?.UnliqudatedObjectsWorkflowQuestion.JustificationKey;

            var comments = requestForReassignment != null
                ? requestForReassignment.UnliqudatedObjectsWorkflowQuestion.Comments : "";

            var detailsView = isAdmin ? "_DetailsMasterList.cshtml" : "_Details.cshtml"; 
            return PartialView(
                "~/Views/Ulo/Details/Workflow/RequestForReassignments/" + detailsView, 
                new RequestForReassignmentViewModel(suggestedReviewerId, justificationKey, requestForReassignmentId, comments, workflowId, uloRegionId, userSelectItems, wfDesc.GetResassignmentJustifications()));
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
                var ret = await Manager.ReassignAsync(wf, requestForReassignmentViewModel.SuggestedReviewerId, UloController.ActionNames.Index);
                await DB.SaveChangesAsync();
//                return ret;
            }
            return Redirect(Request.UrlReferrer?.ToString());
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
                    var groupsUserBelongsTo = GetUserGroups().ConvertAll(z => z.UserId).ToSet();
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
    }
}
