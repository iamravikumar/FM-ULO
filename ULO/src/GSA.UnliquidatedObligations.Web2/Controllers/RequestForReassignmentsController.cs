using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Authorization;
using GSA.UnliquidatedObligations.Web.Identity;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RevolutionaryStuff.AspNetCore;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core.Collections;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    public class RequestForReassignmentsController : BasePageController
    {
        public static readonly string Name = AspHelpers.GetControllerName<RequestForReassignmentsController>();

        public static class ActionNames
        {
            public const string BulkReassign = "BulkReassign";
            public const string Details = "Details";
            public const string GetCommonReassignees = "GetCommonReassignees";
            public const string ReassignFromList = "ReassignFromList";
        }

        protected readonly IWorkflowManager Manager;
        private readonly UloUserManager UserManager;
        private readonly IBackgroundJobClient BackgroundJobClient;


        public RequestForReassignmentsController(IBackgroundJobClient backgroundJobClient, IWorkflowManager manager, UloUserManager userManager, UloDbContext db, PortalHelpers portalHelpers, UserHelpers userHelpers, ICacher cacher, Serilog.ILogger logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            BackgroundJobClient = backgroundJobClient;
            Manager = manager;
            UserManager = userManager;
        }

        private IQueryable<Workflow> GetWorkflows(IEnumerable<int> workflowIds)
            => DB.Workflows.Where(PortalHelpers.GetWorkflowsWorkflowIdPredicate(workflowIds));

        [HttpPost]
        [Route("rfr/getCommonReassignees")]
        [ActionName(ActionNames.GetCommonReassignees)]
        public async Task<JsonResult> GetCommonReassignees()
        {
            var workflowIds = ((await Request.BodyAsJsonObjectAsync<int[]>())??Empty.IntArray).Distinct().ToArray();
            var workflows = GetWorkflows(workflowIds);
            var dbt = new DetailsBulkToken(CurrentUser, DB, workflows);
            var eligibleReviewers = new List<GetEligibleReviewers_Result0>();
            if (dbt.PotentialReviewersByWorkflowId.Count > 0)
            {
                foreach (var p in dbt.PotentialReviewersByWorkflowId.First().Value)
                {
                    eligibleReviewers.Add(new GetEligibleReviewers_Result0
                    {
                        UserId = p.UserId,
                        UserName = MungeReviewerName(p.UserName, p.IsQualified),
                    });
                }
                foreach (var kvp in dbt.PotentialReviewersByWorkflowId.Skip(1))
                {
                    var erByUserId = kvp.Value.ToDictionaryOnConflictKeepLast(p => p.UserId, p => p);
                    eligibleReviewers.Remove(eligibleReviewers.Where(er => !erByUserId.ContainsKey(er.UserId)).ToList());
                    foreach (var p in kvp.Value)
                    {
                        if (!p.IsQualified) continue;
                        var er = eligibleReviewers.FirstOrDefault(z => z.UserId == p.UserId);
                        if (er == null) continue;
                        er.UserName = MungeReviewerName(p.UserName, false);
                        er.IsQualified = false;
                    }
                }
            }
            return Json(eligibleReviewers.OrderBy(z => z.UserName));
        }



        public class DetailsBulkToken
        {
            internal AspNetUser CurrentUser { get; private set; }
            internal MultipleValueDictionary<int, string> ProhibitedOwnerIdsByWorkflowId { get; private set; }
            internal MultipleValueDictionary<int, GetEligibleReviewers_Result0> PotentialReviewersByWorkflowId { get; private set; }
            internal UloDbContext DB { get; private set; }
            internal IDictionary<int, Workflow> WorkflowById { get; private set; }

            internal bool IsValid { get; private set; }
            public bool UseOldGetEligibleReviewersAlgorithm { get; private set; }

            public bool GetEligibleReviewersQualifiedOnly { get; private set; }

            public DetailsBulkToken()
            { }

            internal DetailsBulkToken(AspNetUser currentUser, UloDbContext db, IQueryable<Workflow> workflows)
            {
                CurrentUser = currentUser;
                DB = db;
                WorkflowById = workflows.ToDictionary(z => z.WorkflowId);
                var ids = WorkflowById.Keys;
                if (UseOldGetEligibleReviewersAlgorithm)
                {
                    ProhibitedOwnerIdsByWorkflowId = db.WorkflowProhibitedOwners.Where(z => ids.Contains(z.WorkflowId)).ToMultipleValueDictionary(z => z.WorkflowId, z => z.ProhibitedOwnerUserId);
                }
                else
                {
                    PotentialReviewersByWorkflowId = db.GetEligibleReviewersAsync(CSV.FormatLine(ids, false), true, false).ExecuteSynchronously().ToList().ToMultipleValueDictionary(z => z.WorkflowId, z => z);
                }
                IsValid = true;
            }
            internal DetailsBulkToken(AspNetUser currentUser, UloDbContext db, int workflowId)
                : this(currentUser, db, new[] { db.Workflows.Find(workflowId) }.AsQueryable())
            { }

        }

        private string MungeReviewerName(string username, bool? isQualified)
            => string.Format(
                isQualified.GetValueOrDefault() ? PortalHelpers.GetEligibleReviewersQualifiedUsernameFormat : PortalHelpers.GetEligibleReviewersNotQualifiedUsernameFormat, username);



        [ActionName(ActionNames.Details)]
        public IActionResult Details(int? id, int workflowId, int uloRegionId, string wfDefintionOwnerName = "", bool isAdmin = false, DetailsBulkToken bulkToken = null) 
            =>ViewComponent(nameof(ReassignInfoViewComponent), new { id, workflowId, uloRegionId, wfDefintionOwnerName, isAdmin, bulkToken });

        private async Task<IActionResult> HandleReassignmentRequestAsync(int workflowId, RequestForReassignmentViewModel m)
        {
            var wf = await FindWorkflowAsync(workflowId, false);
            if (wf == null) return NotFound();

            var canHandleReassignment = true;
            UserHelpers.HasPermission(User,ApplicationPermissionNames.CanReassign);
            if (!canHandleReassignment && m.SuggestedReviewerId == CurrentUserId)
            {
                canHandleReassignment = UserHelpers.GetUserGroupRegions(User, wf.OwnerUserId).Contains(wf.TargetUlo.RegionId);
            }
            var rfr = new RequestForReassignment
            {
                UnliqudatedWorkflowQuestions = new UnliqudatedObjectsWorkflowQuestion
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
            DB.RequestForReassignment.Add(rfr);
            try
            {
                if (!canHandleReassignment && wf.OwnerUserId == CurrentUserId)
                {
                    wf.OwnerUserId = UserHelpers.ReassignGroupUserId;
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
        [ActionName(ActionNames.BulkReassign)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanReassign)]
        public async Task<ActionResult> BulkReassign()
        {
            var workflowIds = JsonConvert.DeserializeObject<int[]>(Request.Form["WorkflowIds"]);
            var reviewerId = Request.Form["SuggestedReviewerId"];
            var comment = Request.Form["Comments"];
            var regionIds = UserHelpers.GetUserGroupRegions(CurrentUserName,UserHelpers.ReassignGroupUserId);
            var regionIdPredicate = PortalHelpers.GetWorkflowsRegionIdPredicate(regionIds);
            var workflowIdPredicate = PortalHelpers.GetWorkflowsWorkflowIdPredicate(workflowIds);
            var workflows = await DB.Workflows.Where(regionIdPredicate.And(workflowIdPredicate)).ToListAsync();
            workflows.ForEach(wf => wf.OwnerUserId = reviewerId);
            await DB.SaveChangesAsync();
            BackgroundJobClient.Enqueue<IBackgroundTasks>(bt => bt.SendAssignWorkFlowsBatchNotifications(workflowIds, reviewerId));
            AddPageAlert($"Reassigned {workflows.Count}/{workflowIds.Length} items", true, PageAlert.AlertTypes.Info);
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [ActionName(ActionNames.ReassignFromList)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignFromList(
            int workflowId,
            [Bind(new[]{
                    nameof(RequestForReassignmentViewModel.SuggestedReviewerId),
                    nameof(RequestForReassignmentViewModel.Comments),
                    nameof(RequestForReassignmentViewModel.JustificationKey)
                   }
                )]
                RequestForReassignmentViewModel requestForReassignmentViewModel)
        {
            if (ModelState.IsValid)
            {
                return await HandleReassignmentRequestAsync(workflowId, requestForReassignmentViewModel) ?? RedirectToHome();
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reassign(
            int workflowId,
            [Bind(new[]{
                    nameof(RequestForReassignmentViewModel.SuggestedReviewerId),
                    nameof(RequestForReassignmentViewModel.Comments),
                    nameof(RequestForReassignmentViewModel.JustificationKey)
                   }
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
        public async Task<IActionResult> RequestReassign(
            int workflowId,
            [Bind(new[]{
                nameof(RequestForReassignmentViewModel.WorkflowId),
                nameof(RequestForReassignmentViewModel.SuggestedReviewerId),
                nameof(RequestForReassignmentViewModel.JustificationKey),
                nameof(RequestForReassignmentViewModel.Comments)
            })]
            RequestForReassignmentViewModel requestForReassignmentViewModel)
        {
            if (ModelState.IsValid)
            {
                var wf = await FindWorkflowAsync(workflowId);
                if (wf == null) return NotFound(); 
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


        //    // GET: RequestForReassignments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var requestForReassignment = await DB.RequestForReassignment.FindAsync(id);
            if (requestForReassignment == null)
            {
                return NotFound();
            }
            return View(requestForReassignment);
        }

        // POST: RequestForReassignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var requestForReassignment = await DB.RequestForReassignment.FindAsync(id);
            DB.RequestForReassignment.Remove(requestForReassignment);
            await DB.SaveChangesAsync();
            return View(requestForReassignment); 
        }

        //    //TODO: Move to Manager?
        private async Task<Workflow> FindWorkflowAsync(int workflowId, bool checkOwner = true)
        {
            var wf = await DB.Workflows.Include(q => q.OwnerUser).Include(q => q.TargetUlo).FirstOrDefaultAsync(q => q.WorkflowId == workflowId);
            if (wf != null)
            {
                if (checkOwner == false) return wf;
                if (CurrentUserId != null)
                {
                    var groupsUserBelongsTo = GetUserGroups().ConvertAll(z => z.UserId).ToSet();
                    if (wf.OwnerUserId == CurrentUserId || groupsUserBelongsTo.Contains(wf.OwnerUserId)) return wf;
                    if (wf.OwnerUser.UserType == UserTypes.Group.ToString())
                    {
                        //TODO: Write recursive then call recursive sproc to see if current user is in the group
                    }
                }
            }
            return null;
        }

        private async Task<IActionResult> RequestReassign(Workflow wf, UnliqudatedObjectsWorkflowQuestion question, string suggestedReviewerId)
        {
            var requestForReassignment = new RequestForReassignment
            {
                SuggestedReviewerId = suggestedReviewerId,
                UnliqudatedWorkflowQuestionsId = question.UnliqudatedWorkflowQuestionsId,
                WorkflowId = wf.WorkflowId,
                IsActive = true
            };
            DB.RequestForReassignment.Add(requestForReassignment);
            var ret = await Manager.RequestReassignAsync(wf);
            await DB.SaveChangesAsync();
            return ret;
        }
    }
}
