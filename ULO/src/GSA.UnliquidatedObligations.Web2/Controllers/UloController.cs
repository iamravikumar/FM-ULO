﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Authorization;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core.Collections;
using Serilog;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    public class UloController : BasePageController
    {
        public const string Name = "Ulo";

        public static class ActionNames
        {
            public const string Index = "Index";
            public const string MyTasks = "MyTasks";
            public const string Home = MyTasks;
            public const string Search = "Search";
            public const string RequestForReassignments = "RequestForReassignments";
            public const string Unassigned = "Unassigned";
            public const string Save = "Save";
            public const string Details = "Details";
        }

        public class Config
        {
            public const string ConfigSectionName = "UloControllerConfig";

            public string NoTasks { get; set; }

            public string NoUnassigned { get; set; }

            public string NoReassignments { get; set; }

            public string NoSearchResults { get; set; }

            public string[] MyTasksTabs { get; set; }

            public string[] ReviewStatusOrdering { get; set; }
        }

        private readonly IOptions<Config> ConfigOptions;
        protected readonly IWorkflowManager Manager;

        public UloController(IWorkflowManager manager, IOptions<Config> configOptions, UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            ConfigOptions = configOptions;
            Manager = manager;
            PopulateDocumentTypeNameByDocumentTypeIdInViewBag();
        }

        private void PopulateTabsIntoViewBag(IEnumerable<WorkflowListTab> tabs)
        {
            var items = (tabs ?? WorkflowListTab.None).ToList();
            if (items.Count > 0 && !items.Exists(z => z.IsCurrent))
            {
                (items.FirstOrDefault(z => z.ItemCount > 0) ?? items.First()).IsCurrent = true;
            }
            ViewBag.Tabs = items;
        }

        private void PopulateWorkflowDescriptionIntoViewBag(IWorkflowDescription workflowDescription, Workflow wf, string docType, string mostRecentNonReassignmentAnswer, string mostRecentRealAnswer)
        {
            ViewBag.JustificationByKey = workflowDescription.GetJustificationByKey();
            ViewBag.WorkflowDescription = workflowDescription;
            var wawa = workflowDescription.Activities.FirstOrDefault(a => a.WorkflowActivityKey == wf.CurrentWorkflowActivityKey) as WebActionWorkflowActivity;
            var d = new Dictionary<string, QuestionChoice>();
            ViewBag.QuestionChoiceByQuestionChoiceValue = d;
            wawa?.QuestionChoices?.WhereMostApplicable(docType, mostRecentNonReassignmentAnswer, mostRecentRealAnswer).ForEach(z => d[z.Value] = z);
        }

        private async Task<IWorkflowDescription> FindWorkflowDescAsync(Workflow wf)
        {
            var workflowDescription = await Manager.GetWorkflowDescriptionAsync(wf);
            PopulateWorkflowDescriptionIntoViewBag(workflowDescription, wf, wf.TargetUlo.DocType, wf.MostRecentNonReassignmentAnswer, wf.MostRecentRealAnswer);
            return workflowDescription;
        }

        private IQueryable<Workflow> Workflows
            => DB.Workflows
                .Include(wf => wf.OwnerUser)
                .Include(wf => wf.WorkflowDocuments)
                .Include(wf => wf.TargetUlo).ThenInclude(u => u.Review)
                .Include(wf => wf.TargetUlo).ThenInclude(u=>u.Region).ThenInclude(r=>r.Zone)
                .Include(wf => wf.WorkflowUnliqudatedObjectsWorkflowQuestions).ThenInclude(q => q.User)
                .WhereReviewExists();

        private async Task<Workflow> FindWorkflowAsync(int workflowId)
            => await Workflows.FirstOrDefaultAsync(q => q.WorkflowId == workflowId);

        private void SetNoDataMessage(string message)
        {
            ViewBag.NoDataMessage = message;
        }

        [Route("ulos/{uloId}/{workflowId}", Order = 1)]
        [Route("ulos/{uloId}", Order = 2)]
        [ActionName(ActionNames.Details)]
        public async Task<IActionResult> Details(int uloId = 0, int workflowId = 0)
        {
            //TODO: check if current user is able to view
            var ulo = await DB.UnliquidatedObligations.
                Include(u => u.UloNotes).
                Include(u => u.Region).
                Include(u => u.Region.Zone).
                Include(u => u.UloFinancialActivitys).
                Include(u => u.Review).AsNoTracking().
                WhereReviewExists().
                FirstOrDefaultAsync(u => u.UloId == uloId);

            if (ulo == null) return NotFound();
            if (workflowId == 0)
            {
                workflowId = (await DB.Workflows.OrderByDescending(z => z.WorkflowId).FirstOrDefaultAsync(z => z.TargetUloId == ulo.UloId)).WorkflowId;
                return RedirectToAction(ActionNames.Details, new { uloId, workflowId });
            }

            Log.Information("Viewing ULO {UloId} with Workflow {WorkflowId}", uloId, workflowId);

            var workflow = await FindWorkflowAsync(workflowId);
            var workflowAssignedToCurrentUser = CurrentUserId == workflow.OwnerUserId;

            var belongs =
                workflow.OwnerUser.UserType == AspNetUser.UserTypes.Group &&
                BelongsToMyUnassignmentGroup(workflow.OwnerUserId, ulo.RegionId.GetValueOrDefault(-1));

            var workflowDesc = await FindWorkflowDescAsync(workflow);

            var others = (await DB.GetUloSummariesByPdnAsync(ulo.PegasysDocumentNumber)).ToList().Where(z => z.WorkflowId != workflowId).OrderBy(z => z.WorkflowId).ToList();

            var otherDocs = DB.GetUniqueMissingLineageDocuments(workflow, others.Select(o => o.WorkflowId));

            DB.WorkflowViews.Add(new WorkflowView { ActionAtUtc = DateTime.UtcNow, UserId = CurrentUserId, ViewAction = WorkflowView.CommonActions.Opened, WorkflowId = workflow.WorkflowId });
            await DB.SaveChangesAsync();

            return View("Details/Index", new UloViewModel(ulo, workflow, workflowDesc, workflowAssignedToCurrentUser, others, otherDocs, belongs));
        }


        private bool BelongsToMyUnassignmentGroup(string ownerUserId, int regionId)
        {

            foreach (var g in GetUserGroups(CurrentUserId))
            {
                if (g.UserId == UserHelpers.ReassignGroupUserId) continue;
                var regionIds = UserHelpers.GetUserGroupRegions(g.UserName, UserHelpers.ReassignGroupUserName).OrderBy(z => z.GetValueOrDefault()).ToList();
                if (ownerUserId == g.UserId && regionIds.Contains(regionId)) return true;
            }
            return false;
        }

        [ActionName(ActionNames.Index)]
        public ActionResult Index()
            => RedirectToAction(ActionNames.MyTasks);

        [ActionName(ActionNames.MyTasks)]
        [Route("ulos/myTasks")]
        public IActionResult MyTasks(string t, string sortCol, string sortDir, int? page, int? pageSize)
        {
            Logger.Information("{method}({t}, {sortCol}, {sortDir}, {page}, {pageSize})", nameof(MyTasks), t, sortCol, sortDir, page, pageSize);

            SetNoDataMessage(ConfigOptions.Value.NoTasks);

            var workflows = Workflows.Where(wf => wf.OwnerUserId == CurrentUserId);

            var countByKey = new Dictionary<string, int>(workflows.GroupBy(w => w.CurrentWorkflowActivityKey).Select(g => new { CurrentWorkflowActivityKey = g.Key, Count = g.Count() }).ToDictionaryOnConflictKeepLast(z => z.CurrentWorkflowActivityKey, z => z.Count), Comparers.CaseInsensitiveStringComparer);

            var keyByName = Cacher.FindOrCreateValue(
                "workflowKeyByActivityNameForAllActiveWorkflows",
                () =>
                {
                    var d = new Dictionary<string, string>(Comparers.CaseInsensitiveStringComparer);
                    foreach (var wfd in DB.WorkflowDefinitions.Where(z => z.IsActive))
                    {
                        if (wfd.Description == null || wfd.Description.Activities == null) continue;
                        foreach (var a in wfd.Description.Activities)
                        {
                            d[a.ActivityName] = a.WorkflowActivityKey;
                        }
                    }
                    return d;
                }, PortalHelpers.MediumCacheTimeout);

            var tabs = new List<WorkflowListTab>();
            foreach (var name in ConfigOptions.Value.MyTasksTabs)
            {
                var tab = new WorkflowListTab { TabName = StringHelpers.TrimOrNull(name) };
                if (tab.TabName == null) continue;
                tab.TabKey = keyByName.GetValue(tab.TabName);
                if (tab.TabKey == null) continue;
                tab.ItemCount = countByKey.GetValue(tab.TabKey);
                if (t == null && tab.ItemCount > 0)
                {
                    t = tab.TabKey;
                    Logger.Information("Tabkey was null.  Setting it to the first tab that has data = {tabTabKey}", t);
                }
                tab.IsCurrent = tab.TabKey == t;
                Logger.Information(
                    "TABITERATION: {controller} {name} {tabIsCurrent} {tabTabKey} {t}",
                    this.GetType().Name, name, tab.IsCurrent, tab.TabKey, t);
                tabs.Add(tab);
            }

            var items = DB.ListableWorkflows.Execute(CurrentUserId, CurrentUserId);
            if (t != null)
            {
                items = items.Where(z => z.CurrentWorkflowActivityKey == t);
            }

            sortCol = sortCol ?? nameof(ListableWorkflows_Result0.Status);
            if (sortCol == nameof(ListableWorkflows_Result0.Status))
            {
                items = ApplyBrowse(
                    items,
                    sortCol,
                    ConfigOptions.Value.ReviewStatusOrdering,
                    sortDir, page, pageSize);
            }
            else
            {
                items = ApplyBrowse(
                    items,
                    sortCol, sortDir, page, pageSize);
            }

            PopulateTabsIntoViewBag(tabs);
            return View(items);
        }

        ////Sreeni - Search tab
        ///
        [ActionName(ActionNames.Search)]
        [Route("ulos/search")]
        public ActionResult Search(int? uloId, string pegasysDocumentNumber, string organization, int[] region, int[] zone, string fund, string[] baCode, string pegasysTitleNumber, string pegasysVendorName, string[] docType, string contractingOfficersName, string currentlyAssignedTo, string hasBeenAssignedTo, string awardNumber, string[] reasons, bool[] valid, string[] status, int[] reviewId, bool? reassignableByMe,
            string sortCol = null, string sortDir = null, int? page = null, int? pageSize = null)
        {
            SetNoDataMessage(ConfigOptions.Value.NoSearchResults);
            var wfPredicate = PortalHelpers.GenerateWorkflowPredicate(this.User, uloId, pegasysDocumentNumber, organization, region, zone, fund,
              baCode, pegasysTitleNumber, pegasysVendorName, docType, contractingOfficersName, currentlyAssignedTo, hasBeenAssignedTo, awardNumber, reasons, valid, status, reviewId, reassignableByMe);
            bool hasFilters = wfPredicate != null || !string.IsNullOrEmpty(Request.Query["f"]);
            if (!hasFilters)
            {
                wfPredicate = PredicateBuilder.Create<Workflow>(wf => false);
            }
            else if (wfPredicate == null)
            {
                wfPredicate = PredicateBuilder.Create<Workflow>(wf => true);
            }

            var workflows = ApplyBrowse(
                Workflows.Where(wfPredicate).AsNoTracking(),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize).ToList();

            var baCodes = Cacher.FindOrCreateValue(
                    Cache.CreateKey(nameof(Search), "baCodes"),
                    () => DB.UnliquidatedObligations.Select(u => u.Prog).Distinct().OrderBy(p => p).ToList().AsReadOnly(),
                    PortalHelpers.MediumCacheTimeout
                    );

            var ReasonIncludedInReviewList = Cacher.FindOrCreateValue(
                    Cache.CreateKey(nameof(Search), "ReasonIncludedInReviewList"),
                    () => DB.UnliquidatedObligations.Select(u => u.ReasonIncludedInReview).Distinct().OrderBy(p => p).ToList().AsReadOnly(),
                    PortalHelpers.MediumCacheTimeout
                    );

            var activityNames = GetOrderedActivityNameByWorkflowName().AtomEnumerable.ConvertAll(z => z.Value).Distinct().OrderBy().ToList();


            var reviewListItems = Cacher.FindOrCreateValue(Cache.CreateKey(nameof(Search), "reviewListItems"),
               () =>
                   DB.Reviews.OrderByDescending(r => r.ReviewId).ConvertAll(
                                   r => new SelectListItem
                                   {
                                       Text = $"{r.ReviewName} (#{r.ReviewId}) - {AspHelpers.GetDisplayName(r.ReviewScopeId)} - {AspHelpers.GetDisplayName(r.ReviewTypeId)}",
                                       Value = r.ReviewId.ToString()
                                   }).
                                   ToList().
                                   AsReadOnly(),
               PortalHelpers.ShortCacheTimeout
               );

            var statuses = Cacher.FindOrCreateValue(
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
                PortalHelpers.MediumCacheTimeout);

            PopulateViewInfoIntoViewBag(workflows);

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
                    ReasonIncludedInReviewList,
                    reviewListItems,
                    hasFilters
                ));
        }

        private MultipleValueDictionary<string, string> GetOrderedActivityNameByWorkflowName()
           => Cacher.FindOrCreateValue(
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
               PortalHelpers.MediumCacheTimeout
               );
        private void PopulateViewInfoIntoViewBag(IEnumerable<Workflow> workflows)
        {
            var wfs = workflows.ToList();
            var ids = wfs.Select(z => z.WorkflowId).ToList();
            ViewBag.ViewDateByWorkflowId = (from v in DB.MostRecentWorkflowViews
                                            where ids.Contains(v.WorkflowId) && v.UserId == CurrentUserId
                                            select new { v.WorkflowId, v.ActionAtUtc, v.ViewAction }).
                         ToList().
                         Where(z => z.ViewAction == WorkflowView.CommonActions.Opened || z.ViewAction == WorkflowView.CommonActions.Seen).
                         ToDictionaryOnConflictKeepLast(z => z.WorkflowId, z => z.ActionAtUtc);
        }


        //unassigned tab
        [ActionName(ActionNames.Unassigned)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanViewUnassigned)]
        [Route("ulos/unassigned")]
        public async Task<ActionResult> Unassigned(string sortCol, string sortDir, int? page, int? pageSize)
        {
            SetNoDataMessage(ConfigOptions.Value.NoUnassigned);
            ViewBag.AllAreUnassigned = true;

            var predicate = PredicateBuilder.Create<Workflow>(wf => false);

            var m = new MultipleValueDictionary<string, Tuple<List<int?>, string>>();
            foreach (var g in GetUserGroups(CurrentUserId))
            {
                if (g.UserId == UserHelpers.ReassignGroupUserId) continue;
                var regionIds = UserHelpers.GetUserGroupRegions(g).OrderBy(z => z.GetValueOrDefault()).ToList();
                predicate = predicate.Or(wf => wf.OwnerUserId == g.UserId && regionIds.Contains(wf.TargetUlo.RegionId));
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

            var prohibitedWorkflowIds = await DB.WorkflowProhibitedOwners.Where(z => z.ProhibitedOwnerUserId == CurrentUserId).Select(z => z.WorkflowId).ToListAsync();

            var workflows = from wf in Workflows.Where(z => z.OwnerUserId == CurrentUserId)
                            where !prohibitedWorkflowIds.Contains(wf.WorkflowId)
                            select wf;

            workflows = ApplyBrowse(workflows,sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);

            PopulateRequestForReassignmentsControllerDetailsBulkTokenIntoViewBag(workflows);
            PopulateViewInfoIntoViewBag(workflows);

            return View(workflows);
        }

        private void PopulateRequestForReassignmentsControllerDetailsBulkTokenIntoViewBag(IQueryable<Workflow> workflows)
        {
            var dbt = new RequestForReassignmentsController.DetailsBulkToken(CurrentUser, DB, workflows);
            ViewBag.DetailsBulkToken = dbt;
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


        [ActionName(ActionNames.RequestForReassignments)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanReassign)]
        [Route("ulos/reassignments")]
        public ActionResult RequestForReassignments(string sortCol, string sortDir, int? page, int? pageSize)
        {
            SetNoDataMessage(ConfigOptions.Value.NoReassignments);
            ViewBag.ShowReassignButton = true;
            var regionIds = new List<int?>();
            var reassignGroupUserId = UserHelpers.ReassignGroupUserId;

            var predicate = PredicateBuilder.Create<Workflow>(wf => false);

            var m = new MultipleValueDictionary<string, Tuple<List<int?>, string>>();
            foreach (var g in GetUserGroups(CurrentUserId))
            {
                if (g.UserId == UserHelpers.ReassignGroupUserId) continue;
                regionIds = UserHelpers.GetUserGroupRegions(g).OrderBy(z => z.GetValueOrDefault()).ToList();
                predicate = predicate.Or(wf => wf.OwnerUserId == g.UserId && regionIds.Contains(wf.TargetUlo.RegionId));
                m.Add(Cache.CreateKey(regionIds), Tuple.Create(regionIds, g.UserName));
            }

            if (m.Count > 0)
            {
                foreach (var k in m.Keys)
                {
                    var tuples = m[k];
                    var groupNames = tuples.Select(z => z.Item2).OrderBy();
                    ShowGroupRegionMembershipAlert(groupNames, tuples.First().Item1);
                    goto Browse;
                }
            }


            AddPageAlert($"You're not a member of the reassignments group and will not see any items.", false, PageAlert.AlertTypes.Warning);

        Browse:
            var workflows = ApplyBrowse(
                Workflows.Where(wf => wf.OwnerUserId == reassignGroupUserId && regionIds.Contains(wf.TargetUlo.RegionId)),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);

            PopulateRequestForReassignmentsControllerDetailsBulkTokenIntoViewBag(workflows);
            PopulateViewInfoIntoViewBag(workflows);

            return View(workflows);
        }

        private async Task<JsonResult> CreateFromJsonBody<TJsonBody>(Func<TJsonBody, Task> addAsync)
        {
            try
            {
                var d = await Request.BodyAsJsonObjectAsync<TJsonBody>();
                await addAsync(d);
                await DB.SaveChangesAsync();
                return Json(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Issue in {nameof(CreateFromJsonBody)}");
                return base.CreateJsonError(ex);
            }
        }

        private class CreateFinancialActivityData
        {
            [JsonProperty("activityDate")]
            public System.DateTime ActivityDate { get; set; }

            [JsonProperty("activityType")]
            public string ActivityType { get; set; }

            [JsonProperty("referenceNumber")]
            public string ReferenceNumber { get; set; }

            [JsonProperty("amount")]
            public decimal Amount { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }
        }

        /// <remarks>THIS IS FOR THE BOT!  DO NOT CHANGE THE SIGNATURE</remarks>
        [HttpPost]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CreateFinancialActivity)]
        [Route("ulos/{uloId}/financialActivities/create")]
        public Task<JsonResult> CreateFinancialActivityAsync(int uloId)
            => CreateFromJsonBody<CreateFinancialActivityData>(async d =>
            {
                var fa = await DB.FinancialActivities.FirstOrDefaultAsync(z => z.UloId == uloId && z.ReferenceNumber == d.ReferenceNumber);
                if (fa == null)
                {
                    DB.FinancialActivities.Add(new FinancialActivity
                    {
                        UloId = uloId,
                        ActivityDate = d.ActivityDate,
                        ActivityType = d.ActivityType,
                        ReferenceNumber = d.ReferenceNumber,
                        Amount = d.Amount,
                        Description = d.Description
                    });
                }
                else
                {
                    fa.ActivityDate = d.ActivityDate;
                    fa.ActivityType = d.ActivityType;
                    fa.Amount = d.Amount;
                    fa.Description = d.Description;
                }
            });

        [HttpGet]
        [Route("ulos/{uloId}/notes")]
        public async Task<JsonResult> GetNotesAsync(int uloId)
        {
            var notes = (await DB.Notes.Include(z => z.User).Where(z => z.UloId == uloId).ToListAsync()).OrderByDescending(z => z.CreatedAtUtc).Select(z => new { z.NoteId, CreatedBy = z.User.UserName, z.Body, CreatedAt = z.CreatedAtUtc.ToLocalizedDisplayDateString(true) }).ToList();
            return Json(notes);
        }

        private class CreateNoteData
        {
            [JsonProperty("body")]
            public string Body { get; set; }
        }      


        [HttpPost]
        [Route("ulos/{uloId}/notes/create")]
        public Task<JsonResult> CreateNoteAsync(int uloId)
            => CreateFromJsonBody<CreateNoteData>(d =>
            {
                DB.Notes.Add(new Note { UloId = uloId, Body = d.Body, UserId = CurrentUserId, CreatedAtUtc = DateTime.UtcNow });
                return Task.CompletedTask;
            });

        private class MarkData
        {
            [JsonProperty("workflowIds")]
            public int[] WorkflowIds { get; set; }

            [JsonProperty("viewed")]
            public bool Viewed { get; set; }
        }


        [HttpPost]
        [Route("ulo/mark")]
        public async Task<JsonResult> Mark()
        {
            var md = await AspHelpers.BodyAsJsonObjectAsync<MarkData>(Request);
            var ret = new List<int>();
            if (md != null && md.WorkflowIds != null && md.WorkflowIds.Length > 0)
            {
                var viewAction = md.Viewed ? WorkflowView.CommonActions.Seen : WorkflowView.CommonActions.Unread;
                foreach (var workflowId in md.WorkflowIds.Distinct())
                {
                    DB.WorkflowViews.Add(new WorkflowView { ActionAtUtc = DateTime.UtcNow, UserId = CurrentUserId, ViewAction = viewAction, WorkflowId = workflowId });
                    ret.Add(workflowId);
                }
                await DB.SaveChangesAsync();
            }
            return Json(ret);
        }

        //Referred to by WebActionWorkflowActivity
        //TODO: Attributes will probably change
        [ActionName("Advance")]
        [Route("Advance/{workflowId}")]
        public async Task<ActionResult> Advance(int workflowId)
        {
            var wf = await FindWorkflowAsync(workflowId);
            if (wf == null) return NotFound();
            return View(new FormAModel(wf));
        }

        [HttpPost]
        [ActionName(ActionNames.Save)]
        [Route("ulos/{uloId}/{workflowId}/Save")]
        public async Task<IActionResult> Save(
            int uloId,
            int workflowId,
            [Bind(new[]{
                nameof(AdvanceViewModel.JustificationKey),
                nameof(AdvanceViewModel.Answer),
                nameof(AdvanceViewModel.ExpectedDateForCompletion),
                nameof(AdvanceViewModel.Comments),
                nameof(AdvanceViewModel.WorkflowRowVersionString),
                nameof(AdvanceViewModel.EditingBeganAtUtc),
                nameof(AdvanceViewModel.UnliqudatedWorkflowQuestionsId)})]
            AdvanceViewModel advanceModel=null)
        {
            
            var wf = await FindWorkflowAsync(workflowId);
            if (wf == null) return NotFound();
            if (ModelState.IsValid)
            {
                Log.Information("Altering ULO {UloId} with Workflow {WorkflowId} via command {AlterCommand}", uloId, workflowId, Request.Query["WhatNext"]);

                if (wf.WorkflowRowVersionString != advanceModel.WorkflowRowVersionString)
                {
                    LogStaleWorkflowError(wf, advanceModel.WorkflowRowVersionString, advanceModel.EditingBeganAtUtc);
                    var staleMessage = GetStaleWorkflowErrorMessage(wf, advanceModel.WorkflowRowVersionString, advanceModel.EditingBeganAtUtc);
                    AddPageAlert(staleMessage, false, PageAlert.AlertTypes.Danger, true);
                    return RedirectToAction(ActionNames.Details, new { uloId = wf.TargetUloId, workflowId = wf.WorkflowId });
                }

                var submit = Request.Query["WhatNext"] == "Submit";
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
                wf.TargetUlo.ExpectedDateForCompletion = advanceModel.ExpectedDateForCompletion;
                await DB.SaveChangesAsync();
                if (submit)
                {
                    var ulo = await DB.UnliquidatedObligations.FindAsync(uloId);
                    var groupNames = ulo != null && ulo.RegionId != null ? UserHelpers.GetUserGroupNames(User,ulo.RegionId.Value) : Empty.StringArray.ToList();
                    var ret = await Manager.AdvanceAsync(wf, question, groupNames, false);
                    await DB.SaveChangesAsync();
                    AddPageAlert($"WorkflowId={workflowId} for UloId={uloId} on PDN={wf.TargetUlo.PegasysDocumentNumber} was submitted.", false, PageAlert.AlertTypes.Success, true);
                    return ret;
                }
                else
                {
                    //AddPageAlert($"WorkflowId={workflowId} for UloId={uloId} on PDN={wf.TargetUlo.PegasysDocumentNumber} was saved.", false, PageAlert.AlertTypes.Success, true);
                    return RedirectToIndex();
                }
            }
            return await Details(uloId, workflowId);
        }

    }
}
