﻿using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Newtonsoft.Json;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core.Collections;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
            public const string Home = MyTasks;
            public const string Search = "Search";
            public const string RequestForReassignments = "RequestForReassignments";
            public const string Unassigned = "Unassigned";
            public const string Save = "Save";
            public const string Details = "Details";
        }

        private static class NoDataMessages
        {
            public const string NoTasks = "You do not currently have any tasks";
            public const string NoUnassigned = "There are no unassigned items in groups to which you belong at this time";
            public const string NoReassignments = "There are no reassigment requests at this time";
            public const string NoSearchResults = "Your criteria yielded no results";
        }

        private void SetNoDataMessage(string message)
        {
            ViewBag.NoDataMessage = message;
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

        private void PopulateTabsIntoViewBag(IEnumerable<WorkflowListTab> tabs)
        {
            var items = (tabs ?? WorkflowListTab.None).ToList();
            if (items.Count > 0 && !items.Exists(z => z.IsCurrent))
            {
                (items.FirstOrDefault(z => z.ItemCount > 0) ?? items.First()).IsCurrent = true;
            }
            ViewBag.Tabs = items;
        }

        private void PopulateRequestForReassignmentsControllerDetailsBulkTokenIntoViewBag(IQueryable<Workflow> workflows)
        {
            var dbt = new RequestForReassignmentsController.DetailsBulkToken(CurrentUser, this.DB, workflows);
            ViewBag.DetailsBulkToken = dbt;
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

        [ActionName(ActionNames.Index)]
        public ActionResult Index()
            => RedirectToAction(ActionNames.MyTasks);

        [ActionName(ActionNames.MyTasks)]
        [Route("ulos/myTasks")]
        public ActionResult MyTasks(string t, string sortCol, string sortDir, int? page, int? pageSize)
        {
            SetNoDataMessage(NoDataMessages.NoTasks);

            var workflows = DB.Workflows.Where(wf => wf.OwnerUserId == CurrentUserId).WhereReviewExists();
            var countByKey = new Dictionary<string, int>(workflows.GroupBy(w => w.CurrentWorkflowActivityKey).Select(g => new { CurrentWorkflowActivityKey = g.Key, Count = g.Count() }).ToDictionaryOnConflictKeepLast(z => z.CurrentWorkflowActivityKey, z => z.Count), Comparers.CaseInsensitiveStringComparer);

            var keyByName = Cacher.FindOrCreateValWithSimpleKey("workflowKeyByActivityNameForAllActiveWorkflows", () => {
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
            }, UloHelpers.MediumCacheTimeout);

            var tabs = new List<WorkflowListTab>();
            foreach (var name in CSV.ParseLine(Properties.Settings.Default.MyTasksTabsCsv))
            {
                var tab = new WorkflowListTab { TabName = StringHelpers.TrimOrNull(name) };
                if (tab.TabName == null) continue;
                tab.TabKey = keyByName.GetValue(tab.TabName);
                if (tab.TabKey == null) continue;
                tab.ItemCount = countByKey.GetValue(tab.TabKey);
                tab.IsCurrent = tab.TabKey == t;
                tabs.Add(tab);
            }

            var items = DB.ListableWorkflows(CurrentUserId, CurrentUserId);
            if (t != null)
            {
                items = items.Where(z => z.CurrentWorkflowActivityKey == t);
            }

            sortCol = sortCol ?? nameof(ListableWorkflows_Result.Status);
            if (sortCol == nameof(ListableWorkflows_Result.Status))
            {
                items = ApplyBrowse(
                    items,
                    sortCol,
                    CSV.ParseLine(Properties.Settings.Default.ReviewStatusOrdering),
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
        public async Task<ActionResult> Unassigned(string sortCol, string sortDir, int? page, int? pageSize)
        {
            SetNoDataMessage(NoDataMessages.NoUnassigned);
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

            var prohibitedWorkflowIds = await DB.WorkflowProhibitedOwners.Where(z => z.ProhibitedOwnerUserId == CurrentUserId).Select(z => z.WorkflowId).ToListAsync();

            var workflows = from wf in DB.Workflows.Where(predicate)
                            where !prohibitedWorkflowIds.Contains(wf.WorkflowId)
                            select wf;

            workflows = ApplyBrowse(
                workflows.WhereReviewExists().ApplyStandardIncludes(),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);

            PopulateRequestForReassignmentsControllerDetailsBulkTokenIntoViewBag(workflows);
            PopulateViewInfoIntoViewBag(workflows);

            return View(workflows);
        }

        [ActionName(ActionNames.RequestForReassignments)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanReassign)]
        [Route("ulos/reassignments")]
        public ActionResult RequestForReassignments(string sortCol, string sortDir, int? page, int? pageSize)
        {
            SetNoDataMessage(NoDataMessages.NoReassignments);
            ViewBag.ShowReassignButton = true;
            var regionIds = (IList<int?>)new List<int?>();
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
                DB.Workflows.Where(wf => wf.OwnerUserId == reassignGroupUserId && regionIds.Contains(wf.UnliquidatedObligation.RegionId)).WhereReviewExists()
                .ApplyStandardIncludes(),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);

            PopulateRequestForReassignmentsControllerDetailsBulkTokenIntoViewBag(workflows);
            PopulateViewInfoIntoViewBag(workflows);

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
        public ActionResult Search(int? uloId, string pegasysDocumentNumber, string organization, int[] region, int[] zone, string fund, string[] baCode, string pegasysTitleNumber, string pegasysVendorName, string[] docType, string contractingOfficersName, string currentlyAssignedTo, string hasBeenAssignedTo, string awardNumber, string[] reasons, bool[] valid, string[] status, int[] reviewId, bool? reassignableByMe,
            string sortCol = null, string sortDir = null, int? page = null, int? pageSize = null)
        {
            SetNoDataMessage(NoDataMessages.NoSearchResults);
            var wfPredicate = PortalHelpers.GenerateWorkflowPredicate(this.User, uloId, pegasysDocumentNumber, organization, region, zone, fund,
              baCode, pegasysTitleNumber, pegasysVendorName, docType, contractingOfficersName, currentlyAssignedTo, hasBeenAssignedTo, awardNumber, reasons, valid, status, reviewId, reassignableByMe);
            bool hasFilters = wfPredicate != null || Request["f"] != null;
            if (!hasFilters)
            {
                wfPredicate = PredicateBuilder.Create<Workflow>(wf => false);
            }
            else if (wfPredicate == null)
            {
                wfPredicate = PredicateBuilder.Create<Workflow>(wf => true);
            }

            var workflows = ApplyBrowse(
                DB.Workflows.AsNoTracking().Where(wfPredicate).
                Include(wf => wf.UnliquidatedObligation).AsNoTracking().
                Include(wf => wf.UnliquidatedObligation.Region).AsNoTracking().
                Include(wf => wf.UnliquidatedObligation.Region.Zone).AsNoTracking().
                Include(wf => wf.RequestForReassignments).AsNoTracking().
                Include(wf => wf.AspNetUser).AsNoTracking().
                WhereReviewExists(),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize).ToList();

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

            PopulateViewInfoIntoViewBag(workflows);

            return View(
                "~/Views/Ulo/Search/Index.cshtml",
                new FilterViewModel(
                    workflows,
                    PortalHelpers.CreateDocumentTypeSelectListItems().Select(docType),
                    PortalHelpers.CreateZoneSelectListItems().Select(zone),
                    PortalHelpers.CreateRegionSelectListItems().Select(region),
                    baCodes,
                    activityNames,
                    statuses,
                    Cacher.FindOrCreateValWithSimpleKey(
                        "ReasonsIncludedInReview",
                        () => DB.UnliquidatedObligations.Select(z => z.ReasonIncludedInReview).Distinct().WhereNotNull().OrderBy().AsReadOnly(),
                        UloHelpers.MediumCacheTimeout),
                    hasFilters
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

        private async Task<JsonResult> CreateFromJsonBody<TJsonBody>(Func<TJsonBody, Task> addAsync)
        {
            try
            {
                var d = this.Request.BodyAsJsonObject<TJsonBody>();
                await addAsync(d);
                await DB.SaveChangesAsync();
                return Json(true);
            }
            catch (Exception ex)
            {
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
                FinancialActivity fa = null;
                if (Properties.Settings.Default.OverwriteFinancialActivityWithSameUloAndReferenceNumber)
                {
                    fa = await DB.FinancialActivities.FirstOrDefaultAsync(z => z.UloId == uloId && z.ReferenceNumber == d.ReferenceNumber);
                }
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
            var notes = (await DB.Notes.Include(z=>z.AspNetUser).Where(z => z.UloId == uloId).ToListAsync()).OrderByDescending(z=>z.CreatedAtUtc).Select(z=>new { z.NoteId, CreatedBy = z.AspNetUser.UserName, z.Body, CreatedAt = z.CreatedAtUtc.ToLocalizedDisplayDateString(true) }).ToList();
            return Json(notes, JsonRequestBehavior.AllowGet);
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

        [Route("ulos/{uloId}/{workflowId}", Order = 1)]
        [Route("ulos/{uloId}", Order = 2)]
        [ActionName(ActionNames.Details)]
        public async Task<ActionResult> Details(int uloId = 0, int workflowId = 0)
        {
            //TODO: check if current user is able to view
            var ulo = await DB.UnliquidatedObligations.
                Include(u => u.Notes).
                Include(u => u.Region).
                Include(u => u.Region.Zone).
                Include(u => u.FinancialActivities).
                WhereReviewExists().
                FirstOrDefaultAsync(u => u.UloId == uloId);

            if (ulo == null) return HttpNotFound();
            if (workflowId == 0)
            {
                workflowId = (await DB.Workflows.OrderByDescending(z => z.WorkflowId).FirstOrDefaultAsync(z => z.TargetUloId == ulo.UloId)).WorkflowId;
                return RedirectToAction(ActionNames.Details, new { uloId, workflowId });
            }

            Log.Information("Viewing ULO {UloId} with Workflow {WorkflowId}", uloId, workflowId);

            var workflow = await DB.FindWorkflowAsync(workflowId);
            var workflowAssignedToCurrentUser = CurrentUserId == workflow.OwnerUserId;

            var belongs =
                workflow.AspNetUser.UserType == AspNetUser.UserTypes.Group &&
                BelongsToMyUnassignmentGroup(workflow.OwnerUserId, ulo.RegionId.GetValueOrDefault(-1));

            var workflowDesc = await FindWorkflowDescAsync(workflow);

            var others = DB.GetUloSummariesByPdn(ulo.PegasysDocumentNumber).ToList().Where(z => z.WorkflowId != workflowId).OrderBy(z => z.WorkflowId).ToList();

            var otherDocs = DB.GetUniqueMissingLineageDocuments(workflow, others.Select(o => o.WorkflowId));

            DB.WorkflowViews.Add(new WorkflowView { ActionAtUtc = DateTime.UtcNow, UserId = CurrentUserId, ViewAction = WorkflowView.CommonActions.Opened, WorkflowId = workflow.WorkflowId });
            await DB.SaveChangesAsync();

            return View("Details/Index", new UloViewModel(ulo, workflow, workflowDesc, workflowAssignedToCurrentUser, others, otherDocs, belongs));
        }

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
            var md = this.Request.BodyAsJsonObject<MarkData>();
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
            return Json(ret, JsonRequestBehavior.DenyGet);
        }


        private async Task<IWorkflowDescription> FindWorkflowDescAsync(Workflow wf)
        {
            var workflowDescription = await Manager.GetWorkflowDescriptionAsync(wf);
            PopulateWorkflowDescriptionIntoViewBag(workflowDescription, wf, wf.UnliquidatedObligation.DocType, wf.MostRecentNonReassignmentAnswer, wf.MostRecentRealAnswer);
            return workflowDescription;
        }

        //Referred to by WebActionWorkflowActivity
        //TODO: Attributes will probably change
        [ActionName("Advance")]
        [Route("Advance/{workflowId}")]
        public async Task<ActionResult> Advance(int workflowId)
        {
            var wf = await DB.FindWorkflowAsync(workflowId);
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
                nameof(AdvanceViewModel.WorkflowRowVersionString)+","+
                nameof(AdvanceViewModel.EditingBeganAtUtc)+","+
                nameof(AdvanceViewModel.UnliqudatedWorkflowQuestionsId))]
            AdvanceViewModel advanceModel=null)
        {
            var wf = await DB.FindWorkflowAsync(workflowId);
            if (wf == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                Log.Information("Altering ULO {UloId} with Workflow {WorkflowId} via command {AlterCommand}", uloId, workflowId, Request["WhatNext"]);

                if (wf.WorkflowRowVersionString != advanceModel.WorkflowRowVersionString)
                {
                    LogStaleWorkflowError(wf, advanceModel.WorkflowRowVersionString, advanceModel.EditingBeganAtUtc);
                    var staleMessage = GetStaleWorkflowErrorMessage(wf, advanceModel.WorkflowRowVersionString, advanceModel.EditingBeganAtUtc);
                    AddPageAlert(staleMessage, false, PageAlert.AlertTypes.Danger, true);
                    return RedirectToAction(ActionNames.Details, new { uloId = wf.TargetUloId, workflowId = wf.WorkflowId });
                }

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
                    var ulo = await DB.UnliquidatedObligations.FindAsync(uloId);
                    var groupNames = ulo != null && ulo.RegionId != null ? User.GetUserGroupNames(ulo.RegionId.Value) : Empty.StringArray.ToList();
                    var ret = await Manager.AdvanceAsync(wf, question, groupNames, Properties.Settings.Default.ForceAdvanceFromUloSubmit);
                    await DB.SaveChangesAsync();
                    AddPageAlert($"WorkflowId={workflowId} for UloId={uloId} on PDN={wf.UnliquidatedObligation.PegasysDocumentNumber} was submitted.", false, PageAlert.AlertTypes.Success, true);
                    return ret;
                }
                else
                {
                    AddPageAlert($"WorkflowId={workflowId} for UloId={uloId} on PDN={wf.UnliquidatedObligation.PegasysDocumentNumber} was saved.", false, PageAlert.AlertTypes.Success, true);
                    return RedirectToIndex();
                }
            }
            return await Details(uloId, workflowId);
        }
    }
}
