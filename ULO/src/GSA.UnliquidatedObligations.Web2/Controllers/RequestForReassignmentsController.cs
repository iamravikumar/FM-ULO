using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using GSA.UnliquidatedObligations.Web.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Newtonsoft.Json;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GSA.UnliquidatedObligations.Web.Controllers
{

    //[ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    //[Authorize(Policy = "ApplicationUser")]
    public class RequestForReassignmentsController : BasePageController
    {
        public const string Name = "RequestForReassignments";

        public static class ActionNames
        {
            public const string BulkReassign = "BulkReassign";
            public const string Details = "Details";
            public const string GetCommonReassignees = "GetCommonReassignees";
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
        {
            return DB.Workflows.Where(PortalHelpers.GetWorkflowsWorkflowIdPredicate(workflowIds));
        }

        [HttpPost]
        [Route("rfr/getCommonReassignees")]
        [ActionName(ActionNames.GetCommonReassignees)]
        public JsonResult GetCommonReassignees()
        {
            var json = new System.IO.StreamReader(Request.Body).ReadToEnd();
            var workflowIds = (JsonConvert.DeserializeObject<int[]>(json) ?? Empty.IntArray).Distinct().ToArray();
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
        {
            bulkToken = (bulkToken != null && bulkToken.IsValid) ? bulkToken : new DetailsBulkToken(CurrentUser, DB, workflowId);
            var db = bulkToken.DB;
            RequestForReassignment requestForReassignment = null;
            if (id.HasValue)
            {
                requestForReassignment = db.RequestForReassignment.Include(z => z.UnliqudatedWorkflowQuestions).FirstOrDefault(r => r.RequestForReassignmentID == id.Value);
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


            IList<SelectListItem> userSelectItems;
            if (UseOldGetEligibleReviewersAlgorithm)
            {
                var prohibitedUserIds = bulkToken.ProhibitedOwnerIdsByWorkflowId[workflowId];

                userSelectItems = Cacher.FindOrCreateValue(
                    Cache.CreateKey(groupOwnerId, uloRegionId, "fdsfdsaf"),
                    () => db.UserUsers
                        .Where(uu => uu.ParentUserId == groupOwnerId && uu.RegionId == uloRegionId && uu.ChildUser.UserType == AspNetUser.UserTypes.Person)
                        .Select(uu => new { UserName = uu.ChildUser.UserName, UserId = uu.ChildUserId }).ToList(),
                        PortalHelpers.MediumCacheTimeout
                        ).ConvertAll(z => UserHelpers.CreateUserSelectListItem(z.UserId, z.UserName, prohibitedUserIds.Contains(z.UserId))).ToList();

                if (Cacher.FindOrCreateValue(
                    Cache.CreateKey(uloRegionId, User.Identity.Name),
                    () =>
                    {
                        var userReassignRegions = UserHelpers.GetReassignmentGroupRegions(User);
                        return true;//User.HasClaim("Application", ApplicationPermissionNames.CanReassign.ToString()) && userReassignRegions.Contains(uloRegionId); //sreen : need change back to this statement after Claims fix

                    },
                    PortalHelpers.MediumCacheTimeout
                    ))
                {
                    userSelectItems.Add(UserHelpers.ToSelectListItem(bulkToken.CurrentUser));
                }
            }
            else
            {
                userSelectItems = new List<SelectListItem>();
                foreach (var p in bulkToken.PotentialReviewersByWorkflowId[workflowId])
                {
                    string text = MungeReviewerName(p.UserName, p.IsQualified);
                    userSelectItems.Add(UserHelpers.CreateUserSelectListItem(p.UserId, text));
                }
            }

            if (workflow.OwnerUserId == CurrentUserId)
            {
                userSelectItems.Remove(userSelectItems.Where(z => z.Value == CurrentUserId).ToList());
            }

            userSelectItems = userSelectItems.OrderBy(z => z.Text).ToList();

            userSelectItems.Insert(0, UserHelpers.CreateUserSelectListItem(UserHelpers.ReassignGroupUserId, UserHelpers.ReassignGroupUserName));

            var requestForReassignmentId = requestForReassignment?.RequestForReassignmentID;
            var suggestedReviewerId = requestForReassignment != null ? requestForReassignment.SuggestedReviewerId : "";
            var justificationKey = requestForReassignment?.UnliqudatedWorkflowQuestions.JustificationKey;

            var comments = requestForReassignment != null
                ? requestForReassignment.UnliqudatedWorkflowQuestions.Comments : "";

            var detailsView = isAdmin ? "_DetailsMasterList.cshtml" : "_Details.cshtml";
            return PartialView(
                "~/Views/Ulo/Details/Workflow/RequestForReassignments/" + detailsView,
                new RequestForReassignmentViewModel(suggestedReviewerId, justificationKey, requestForReassignmentId, comments, workflowId, uloRegionId, userSelectItems, wfDesc.GetResassignmentJustifications()));
        }

        private async Task<IActionResult> HandleReassignmentRequestAsync(int workflowId, RequestForReassignmentViewModel m)
        {
            var wf = await FindWorkflowAsync(workflowId, false);
            if (wf == null) return StatusCode(404);

            var canHandleReassignment = true;
            //User.HasPermission(ApplicationPermissionNames.CanReassign);
            //if (!canHandleReassignment && m.SuggestedReviewerId == CurrentUserId)
            //{
            //    canHandleReassignment = UserHelpers.GetUserGroupRegions(wf.OwnerUser.GetClaims, wf.OwnerUserId).Contains(wf.TargetUlo.RegionId);
            //}
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
        //[ApplicationPermissionAuthorize(ApplicationPermissionNames.CanReassign)]
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
            AddPageAlert($"Reassigned {workflows.Count}/{workflowIds.Length} items", true, PageAlert.AlertTypes.Info, true);
            return Redirect(Request.Headers["Referer"].ToString());
        }

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
                if (wf == null) return StatusCode(404); 
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
                return StatusCode(400);
            }
            var requestForReassignment = await DB.RequestForReassignment.FindAsync(id);
            if (requestForReassignment == null)
            {
                return StatusCode(404);
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
