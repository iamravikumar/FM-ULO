using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core.Collections;
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

        public class DetailsBulkToken
        {
            internal AspNetUser CurrentUser { get; private set; }
            internal MultipleValueDictionary<int, string> ProhibitedOwnerIdsByWorkflowId { get; private set; }
            internal ULODBEntities DB { get; private set; }
            internal IDictionary<int, Workflow> WorkflowById { get; private set; }

            internal bool IsValid
                => CurrentUser != null & DB != null && ProhibitedOwnerIdsByWorkflowId != null && WorkflowById != null;

            public DetailsBulkToken()
            { }

            internal DetailsBulkToken(AspNetUser currentUser, ULODBEntities db, IQueryable<Workflow> workflows)
            {
                CurrentUser = currentUser;
                DB = db;
                WorkflowById = workflows.ToDictionary(z => z.WorkflowId);
                var ids = WorkflowById.Keys;
                ProhibitedOwnerIdsByWorkflowId = db.WorkflowProhibitedOwners.Where(z => ids.Contains(z.WorkflowId)).ToMultipleValueDictionary(z => z.WorkflowId, z => z.ProhibitedOwnerUserId);
            }
            internal DetailsBulkToken(AspNetUser currentUser, ULODBEntities db, int workflowId)
                : this(currentUser, db, new[] { db.Workflows.Find(workflowId) }.AsQueryable())
            { }
        }

        [ActionName(ActionNames.Details)]
        public ActionResult Details(int? id, int workflowId, int uloRegionId, string wfDefintionOwnerName = "", bool isAdmin = false, DetailsBulkToken bulkToken=null)
        {
            bulkToken = (bulkToken != null && bulkToken.IsValid) ? bulkToken : new DetailsBulkToken(CurrentUser, DB, workflowId);
            var db = bulkToken.DB; 
            RequestForReassignment requestForReassignment = null;
            if (id.HasValue)
            {
                requestForReassignment = db.RequestForReassignments.Include(z=>z.UnliqudatedObjectsWorkflowQuestion).FirstOrDefault(r => r.RequestForReassignmentID == id.Value);
            }

            var workflow = db.Workflows.Find(workflowId);
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

            var prohibitedUserIds = bulkToken.ProhibitedOwnerIdsByWorkflowId[workflowId];

            var userSelectItems = Cacher.FindOrCreateValWithSimpleKey(
                Cache.CreateKey(groupOwnerId, uloRegionId, "fdsfdsaf"),
                () => db.UserUsers
                    .Where(uu => uu.ParentUserId == groupOwnerId && uu.RegionId == uloRegionId && uu.ChildUser.UserType == AspNetUser.UserTypes.Person)
                    .Select(uu => new { UserName = uu.ChildUser.UserName, UserId = uu.ChildUserId }).ToList(),
                    UloHelpers.MediumCacheTimeout
                    ).ConvertAll(z => PortalHelpers.CreateUserSelectListItem(z.UserId, z.UserName, prohibitedUserIds.Contains(z.UserId))).ToList();

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
                userSelectItems.Add((bulkToken.CurrentUser).ToSelectListItem(prohibitedUserIds.Contains(CurrentUserId)));
            }

            if (workflow.OwnerUserId == CurrentUserId)
            {
                userSelectItems.Remove(userSelectItems.Where(z => z.Value == CurrentUserId).ToList());
            }

            userSelectItems = userSelectItems.OrderBy(z => z.Text).ToList();

            userSelectItems.Insert(0, PortalHelpers.CreateUserSelectListItem(PortalHelpers.ReassignGroupUserId, PortalHelpers.ReassignGroupUserName));

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

        private async Task<ActionResult> HandleReassignmentRequestAsync(int workflowId, RequestForReassignmentViewModel m)
        {
            var wf = await FindWorkflowAsync(workflowId, false);
            if (wf == null) return HttpNotFound();

            var canHandleReassignment = User.HasPermission(ApplicationPermissionNames.CanReassign);
            if (!canHandleReassignment && m.SuggestedReviewerId == CurrentUserId)
            {
                canHandleReassignment = User.GetUserGroupRegions(wf.OwnerUserId).Contains(wf.UnliquidatedObligation.RegionId);
            }
            var rfr = new RequestForReassignment
            {
                UnliqudatedObjectsWorkflowQuestion = new UnliqudatedObjectsWorkflowQuestion
                {
                    JustificationKey = m.JustificationKey,
                    UserId = CurrentUserId,
                    Answer = UnliqudatedObjectsWorkflowQuestion.CommonAnswers.Reassignment,
                    WorkflowId = workflowId,
                    Comments = m.Comments,
                    WorkflowRowVersion = wf.WorkflowRowVersion,
                    CreatedAtUtc = DateTime.UtcNow
                },
                WorkflowId = workflowId,
                SuggestedReviewerId = m.SuggestedReviewerId,
                IsActive = !canHandleReassignment
            };
            DB.RequestForReassignments.Add(rfr);
            try
            {
                if (!canHandleReassignment && wf.OwnerUserId == CurrentUserId)
                {
                    wf.OwnerUserId = PortalHelpers.ReassignGroupUserId;
                }
                else if (canHandleReassignment)
                {
                    return await Manager.ReassignAsync(wf, m.SuggestedReviewerId, UloController.ActionNames.MyTasks);
                }
                return null;
            }
            finally
            {
                await DB.SaveChangesAsync();
            }
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
                return await HandleReassignmentRequestAsync(workflowId, requestForReassignmentViewModel) ?? RedirectToHome();
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
                return await HandleReassignmentRequestAsync(workflowId, requestForReassignmentViewModel) ?? RedirectToHome();
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
                    Answer = UnliqudatedObjectsWorkflowQuestion.CommonAnswers.RequestForReasssignment,
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
    }
}
