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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    public class UloController : BaseController
    {
        public const string Name = "Ulo";

        public static class ActionNames
        {
            public const string Index = "Index";
            public const string MyTasks = "MyTasks";
            public const string Search = "Search";
            public const string RequestForReassignments = "RequestForReassignments";
            public const string Unassigned = "Unassigned";
            public const string Save = "Save";
            public const string Details = "Details";
        }

        protected readonly IWorkflowManager Manager;
        private readonly ApplicationUserManager UserManager;


        public UloController(IWorkflowManager manager, ApplicationUserManager userManager, ULODBEntities db, IComponentContext componentContext, ICacher cacher, Serilog.ILogger logger)
            : base(db, componentContext, cacher, logger)
        {
            Manager = manager;
            UserManager = userManager;
            PopulateDocumentTypeNameByDocumentTypeIdInViewBag();
        }

        private void PopulateWorkflowDescriptionInViewBag(IWorkflowDescription workflowDescription, Workflow wf, string docType)
        {
            ViewBag.JustificationByKey = workflowDescription.GetJustificationByKey();
            ViewBag.WorkflowDescription = workflowDescription;
            var wawa = workflowDescription.Activities.FirstOrDefault(a => a.WorkflowActivityKey == wf.CurrentWorkflowActivityKey) as WebActionWorkflowActivity;
            var d = new Dictionary<string, QuestionChoice>();
            ViewBag.QuestionChoiceByQuestionChoiceValue = d;
            wawa?.QuestionChoices?.WhereApplicable(docType).ForEach(z => d[z.Value] = z);
        }

        [ActionName(ActionNames.Index)]
        public ActionResult Index()
            => RedirectToAction(ActionNames.MyTasks);

        [ActionName(ActionNames.MyTasks)]
        [Route("ulos/myTasks")]
        public ActionResult MyTasks(string sortCol, string sortDir, int? page, int? pageSize)
        {
            //TODO: Due dates: calculate in model or add additional column in workflow table (ExpectedActivityDurationInSeconds, nullable, DueAt = null) 
            var workflows = ApplyBrowse(
                DB.Workflows.Where(wf => wf.OwnerUserId == CurrentUserId).ApplyStandardIncludes(),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);
            return View(workflows);
        }

        private bool BelongsToMyUnassignmentGroup(string ownerUserId, int regionId)
        {
            foreach (var g in GetUserGroups(CurrentUserId))
            {
                if (g.UserId == PortalHelpers.ReassignGroupUserId) continue;
                var regionIds = User.GetUserGroupRegions(g.UserName).OrderBy(z => z.GetValueOrDefault()).ToList();
                if (ownerUserId == g.UserId && regionIds.Contains(regionId)) return true;
            }
            return false;
        }

        [ActionName(ActionNames.Unassigned)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanViewUnassigned)]
        [Route("ulos/unassigned")]
        public ActionResult Unassigned(string sortCol, string sortDir, int? page, int? pageSize)
        {
            ViewBag.AllAreUnassigned = true;         

            var predicate = PredicateBuilder.Create<Workflow>(wf => false);

            var m = new MultipleValueDictionary<string, Tuple<List<int?>, string>>();
            foreach (var g in GetUserGroups(CurrentUserId))
            {
                if (g.UserId == PortalHelpers.ReassignGroupUserId) continue;
                var regionIds = User.GetUserGroupRegions(g.UserName).OrderBy(z => z.GetValueOrDefault()).ToList();
                predicate = predicate.Or(wf => wf.OwnerUserId == g.UserId && regionIds.Contains(wf.UnliquidatedObligation.RegionId));
                m.Add(Cache.CreateKey(regionIds), Tuple.Create(regionIds, g.UserName));
            }

            if (m.Count > 0)
            {
                foreach (var k in m.Keys)
                {
                    var tuples = m[k];
                    var groupNames = tuples.Select(z => z.Item2).OrderBy();
                    ShowGroupRegionMembershipAlert(groupNames, tuples.First().Item1);
                }
            }
            else
            {
                AddPageAlert($"You're not a member of any related groups and will not have any unassigned items.", false, PageAlert.AlertTypes.Warning);
            }

            var workflows = ApplyBrowse(
                DB.Workflows.Where(predicate).ApplyStandardIncludes(),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);
            return View(workflows);
        }

        [ActionName(ActionNames.RequestForReassignments)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanReassign)]
        [Route("ulos/reassignments")]
        public ActionResult RequestForReassignments(string sortCol, string sortDir, int? page, int? pageSize)
        {
            ViewBag.ShowReassignButton = true;
            var regionIds = (IList<int?>) new List<int?>();
            var reassignGroupUserId = PortalHelpers.ReassignGroupUserId;

            foreach (var g in GetUserGroups(CurrentUserId))
            {
                if (g.UserId != reassignGroupUserId) continue;
                regionIds = User.GetReassignmentGroupRegions();
                ShowGroupRegionMembershipAlert(new[] { Properties.Settings.Default.ReassignGroupUserName }, regionIds);
                goto Browse;
            }
            AddPageAlert($"You're not a member of the reassignments group and will not see any items.", false, PageAlert.AlertTypes.Warning);

            Browse:
            var workflows = ApplyBrowse(
                DB.Workflows.Where(wf => wf.OwnerUserId == reassignGroupUserId && regionIds.Contains(wf.UnliquidatedObligation.RegionId))
                .ApplyStandardIncludes(),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);

            return View(workflows);
        }

        private bool ShowGroupRegionMembershipAlert(IEnumerable<string> groupNames, ICollection<int?> regionIds)
        {
            if (regionIds.Count > 0)
            {
                var myRegions = regionIds.ConvertAll(rid => PortalHelpers.GetRegionName(rid.GetValueOrDefault())).WhereNotNull().Format(", ");
                AddPageAlert($"You're a member of the groups: {groupNames.Format(", ")}; with regions: {myRegions}", false, PageAlert.AlertTypes.Info);
            }
            else
            {
                AddPageAlert($"You're a member of the groups: {groupNames.Format(", ")}; but haven't been assigned any regions", false, PageAlert.AlertTypes.Warning);
            }
            return true;
        }

        [ActionName(ActionNames.Search)]
        [Route("ulos/search")]
        public async Task<ActionResult> Search(int? uloId, string pegasysDocumentNumber, string organization, int? region, int? zone, string fund, string baCode, string pegasysTitleNumber, string pegasysVendorName, string docType, string contractingOfficersName, string currentlyAssignedTo, string hasBeenAssignedTo, string awardNumber, string reasons, bool? valid, string status, int? reviewId,
            string sortCol = null, string sortDir = null, int? page = null, int? pageSize = null)
        {
            var test = HttpUtility.UrlEncode(reasons);
            var wfPredicate = PredicateBuilder.Create<Workflow>(wf => true);
            wfPredicate = wfPredicate.GenerateWorkflowPredicate(uloId, pegasysDocumentNumber, organization, region, zone, fund,
              baCode, pegasysTitleNumber, pegasysVendorName, docType, contractingOfficersName, currentlyAssignedTo, hasBeenAssignedTo, awardNumber, reasons, valid, status, reviewId);

            var workflows = await ApplyBrowse(
                DB.Workflows.AsNoTracking().Where(wfPredicate).
                Include(wf => wf.UnliquidatedObligation).AsNoTracking().
                Include(wf => wf.UnliquidatedObligation.Region).AsNoTracking().
                Include(wf => wf.UnliquidatedObligation.Region.Zone).AsNoTracking().
                Include(wf => wf.RequestForReassignments).AsNoTracking().
                Include(wf => wf.AspNetUser).AsNoTracking(),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize).ToListAsync();

            var baCodes = Cacher.FindOrCreateValWithSimpleKey(
                    Cache.CreateKey(nameof(Search), "baCodes"),
                    () => DB.UnliquidatedObligations.Select(u => u.Prog).Distinct().OrderBy(p => p).ToList().AsReadOnly(),
                    UloHelpers.MediumCacheTimeout
                    );

            var activityNames = GetOrderedActivityNameByWorkflowName().AtomEnumerable.ConvertAll(z => z.Value).Distinct().OrderBy().ToList();

            var statuses = Cacher.FindOrCreateValWithSimpleKey(
                "AllWorkflowStatusNames",
                () => 
                {
                    var names = new List<string>();
                    foreach (var wd in DB.WorkflowDefinitions.Where(wfd => wfd.IsActive == true))
                    {
                        names.AddRange(wd.Description.WebActionWorkflowActivities.Select(z => z.ActivityName));
                    }
                    return names.Distinct().OrderBy();
                },
                UloHelpers.MediumCacheTimeout);

            return View(
                "~/Views/Ulo/Search/Index.cshtml", 
                new FilterViewModel(
                    workflows, 
                    PortalHelpers.CreateDocumentTypeSelectListItems(),
                    PortalHelpers.CreateZoneSelectListItems(),
                    PortalHelpers.CreateRegionSelectListItems(),
                    baCodes,
                    activityNames,
                    statuses,
                    Cacher.FindOrCreateValWithSimpleKey(
                        "ReasonsIncludedInReview",
                        () => DB.UnliquidatedObligations.Select(z => z.ReasonIncludedInReview).Distinct().WhereNotNull().OrderBy().AsReadOnly(),
                        UloHelpers.MediumCacheTimeout)
                ));
        }

        private MultipleValueDictionary<string, string> GetOrderedActivityNameByWorkflowName()
            => Cacher.FindOrCreateValWithSimpleKey(
                nameof(GetOrderedActivityNameByWorkflowName),
                () =>
                {
                    var m = new MultipleValueDictionary<string, string>(null, () => new List<string>());
                    foreach (var wd in DB.WorkflowDefinitions.Where(wfd => wfd.IsActive == true))
                    {
                        foreach (var activityName in wd.Description.WebActionWorkflowActivities.OrderBy(a => a.SequenceNumber).Select(a => a.ActivityName))
                        {
                            m.Add(wd.WorkflowDefinitionName, activityName);
                        }
                    }
                    return m;
                },
                UloHelpers.MediumCacheTimeout
                );

        [Route("ulos/{uloId}/{workflowId}", Order = 1)]
        [Route("ulos/{uloId}", Order = 2)]
        [ActionName(ActionNames.Details)]
        public async Task<ActionResult> Details(int uloId, int workflowId=0)
        {
            //TODO: check if current user is able to view
            var ulo = await DB.UnliquidatedObligations.
                Include(u => u.Notes).
                Include(u => u.Region).
                Include(u => u.Region.Zone).
                FirstOrDefaultAsync(u => u.UloId == uloId);
            if (workflowId==0)
            {
                workflowId = (await DB.Workflows.SingleAsync(z => z.TargetUloId == ulo.UloId)).WorkflowId;
            }

            Log.Information("Viewing ULO {UloId} with Workflow {WorkflowId}", uloId, workflowId);

            var workflow = await FindWorkflowAsync(workflowId);
            var workflowAssignedToCurrentUser = CurrentUserId == workflow.OwnerUserId;

            var belongs =
                workflow.AspNetUser.UserType == AspNetUser.UserTypes.Group &&
                BelongsToMyUnassignmentGroup(workflow.OwnerUserId, ulo.RegionId.GetValueOrDefault(-1));

            var workflowDesc = await FindWorkflowDescAsync(workflow);

            var others = DB.GetUloSummariesByPdn(ulo.PegasysDocumentNumber).ToList().Where(z => z.WorkflowId != workflowId).OrderBy(z => z.WorkflowId).ToList();

            return View("Details/Index", new UloViewModel(ulo, workflow, workflowDesc, workflowAssignedToCurrentUser, others, belongs));
        }

        private async Task<IWorkflowDescription> FindWorkflowDescAsync(Workflow wf)
        {
            var workflowDescription = await Manager.GetWorkflowDescriptionAsync(wf);
            PopulateWorkflowDescriptionInViewBag(workflowDescription, wf, wf.UnliquidatedObligation.DocType);
            return workflowDescription;
        }

        //TODO: Move to Manager?
        private async Task<Workflow> FindWorkflowAsync(int workflowId)
            => await DB.Workflows
                .Include(q => q.AspNetUser)
                .Include(q => q.Documents)
                .Include(q => q.UnliquidatedObligation)
                .FirstOrDefaultAsync(q => q.WorkflowId == workflowId);

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

        [HttpPost]
        [ActionName(ActionNames.Save)]
        [Route("ulos/{uloId}/{workflowId}/Save")]
        public async Task<ActionResult> Save(
            int uloId,
            int workflowId,
            [Bind(Include = 
                nameof(AdvanceViewModel.JustificationKey)+","+
                nameof(AdvanceViewModel.Answer)+","+
                nameof(AdvanceViewModel.ExpectedDateForCompletion)+","+
                nameof(AdvanceViewModel.Comments)+","+
                nameof(AdvanceViewModel.UnliqudatedWorkflowQuestionsId))]
            AdvanceViewModel advanceModel=null)
        {
            var wf = await FindWorkflowAsync(workflowId);
            if (wf == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                Log.Information("Altering ULO {UloId} with Workflow {WorkflowId} via command {AlterCommand}", uloId, workflowId, Request["WhatNext"]);

                var submit = Request["WhatNext"] == "Submit";
                var question = await DB.UnliqudatedObjectsWorkflowQuestions.Where(z => z.WorkflowId == workflowId).OrderByDescending(z => z.UnliqudatedWorkflowQuestionsId).FirstOrDefaultAsync();
                if (question == null || !question.Pending)
                {
                    if (question != null)
                    {
                        question.Pending = false;
                    }
                    question = new UnliqudatedObjectsWorkflowQuestion
                    {
                        WorkflowId = workflowId
                    };
                    DB.UnliqudatedObjectsWorkflowQuestions.Add(question);
                }
                question.JustificationKey = advanceModel.JustificationKey;
                question.UserId = CurrentUserId;
                question.Answer = advanceModel.Answer;
                question.Comments = advanceModel.Comments;
                question.Pending = !submit;
                question.UnliqudatedWorkflowQuestionsId = advanceModel.UnliqudatedWorkflowQuestionsId;
                question.WorkflowRowVersion = wf.WorkflowRowVersion;
                question.CreatedAtUtc = DateTime.UtcNow;
                wf.UnliquidatedObligation.ExpectedDateForCompletion = advanceModel.ExpectedDateForCompletion;
                await DB.SaveChangesAsync();
                if (submit)
                {
                    var ret = await Manager.AdvanceAsync(wf, question);
                    await DB.SaveChangesAsync();
                    return ret;
                }
                else
                {
                    return RedirectToIndex();
                }
            }
            return await Details(uloId, workflowId);
        }
    }
}