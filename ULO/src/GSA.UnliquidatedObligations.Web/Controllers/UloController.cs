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
    //[ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
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
        }

        protected readonly IWorkflowManager Manager;
        private readonly ApplicationUserManager UserManager;


        public UloController(IWorkflowManager manager, ApplicationUserManager userManager, ULODBEntities db, IComponentContext componentContext, ICacher cacher)
            : base(db, componentContext, cacher)
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
        [Route("Ulos/MyTasks")]
        public ActionResult MyTasks(string sortCol, string sortDir, int? page, int? pageSize)
        {
            //TODO: Due dates: calculate in model or add additional column in workflow table (ExpectedActivityDurationInSeconds, nullable, DueAt = null) 
            var workflows = ApplyBrowse(
                DB.Workflows.Where(wf => wf.OwnerUserId == CurrentUserId).Include(wf => wf.UnliquidatedObligation).Include(wf=>wf.UnliquidatedObligation.Region),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);
            return View(workflows);
        }

        [ActionName(ActionNames.Unassigned)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanViewUnassigned)]
        [Route("Ulos/Unassigned")]
        public ActionResult Unassigned(string sortCol, string sortDir, int? page, int? pageSize)
        {
            ViewBag.ShowReassignButton = true;

            var predicate = PredicateBuilder.Create<Workflow>(wf => false);

            foreach (var g in GetUserGroups(CurrentUserId))
            {
                var regionIds = User.GetUserGroupRegions(g.UserName);
                predicate = predicate.Or(wf => wf.OwnerUserId == g.UserId && regionIds.Contains(wf.UnliquidatedObligation.RegionId));
                ShowGroupRegionMembershipAlert(g.UserName, regionIds);
            }

            var workflows = ApplyBrowse(
                DB.Workflows.Where(predicate)
                .Include(wf => wf.UnliquidatedObligation),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);
            return View(workflows);
        }

        [ActionName(ActionNames.RequestForReassignments)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanReassign)]
        [Route("Ulos/Reassignments")]
        public ActionResult RequestForReassignments(string sortCol, string sortDir, int? page, int? pageSize)
        {
            ViewBag.ShowReassignButton = true;
            var regionIds = User.GetReassignmentGroupRegions();
            var reassignGroupUserId = PortalHelpers.ReassignGroupUserId;
            ShowGroupRegionMembershipAlert(Properties.Settings.Default.ReassignGroupUserName, regionIds);

            var workflows = ApplyBrowse(
                DB.Workflows.Where(wf => wf.OwnerUserId == reassignGroupUserId && regionIds.Contains(wf.UnliquidatedObligation.RegionId))
                .Include(wf => wf.UnliquidatedObligation),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);

            return View(workflows);
        }

        private bool ShowGroupRegionMembershipAlert(string groupName, ICollection<int?> regionIds)
        {
            if (regionIds.Count > 0)
            {
                var myRegions = regionIds.ConvertAll(rid => PortalHelpers.GetRegionName(rid.GetValueOrDefault())).WhereNotNull().Format(", ");
                AddPageAlert($"You're a member of the {groupName} group with regions: {myRegions}", false, PageAlert.AlertTypes.Info);
            }
            else
            {
                AddPageAlert($"You're a member of the {groupName} group but haven't been assigned any regions", false, PageAlert.AlertTypes.Warning);
            }
            return true;
        }

        [ActionName(ActionNames.Search)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanViewOtherWorkflows)]
        [Route("Ulos/Search")]
        public async Task<ActionResult> Search(int? uloId, string pegasysDocumentNumber, string organization, int? region, int? zone, string fund, string baCode, string pegasysTitleNumber, string pegasysVendorName, string docType, string contractingOfficersName, string currentlyAssignedTo, string hasBeenAssignedTo, string awardNumber, string reasonIncludedInReview, bool? valid, string status, int? reviewId,
            string sortCol = null, string sortDir = null, int? page = null, int? pageSize = null)
        {
            var claimRegionIds = CurrentUser.GetApplicationPerimissionRegions(ApplicationPermissionNames.CanViewOtherWorkflows);
            var wfPredicate =
                PredicateBuilder.Create<Workflow>(
                    wf => claimRegionIds.Contains((int)wf.UnliquidatedObligation.RegionId));
            var test = HttpUtility.UrlEncode(reasonIncludedInReview);
            wfPredicate = wfPredicate.GenerateWorkflowPredicate(uloId, pegasysDocumentNumber, organization, region, zone, fund,
              baCode, pegasysTitleNumber, pegasysVendorName, docType, contractingOfficersName, currentlyAssignedTo, hasBeenAssignedTo, awardNumber, reasonIncludedInReview, valid, status, reviewId);

            var workflows = await ApplyBrowse(
                DB.Workflows.Where(wfPredicate).
                Include(wf => wf.UnliquidatedObligation).
                Include(wf => wf.UnliquidatedObligation.Region).
                Include(wf => wf.UnliquidatedObligation.Region.Zone).
                Include(wf => wf.RequestForReassignments).
                Include(wf => wf.AspNetUser),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize).ToListAsync();

            var baCodes = Cacher.FindOrCreateValWithSimpleKey(
                    Cache.CreateKey(nameof(Search), "baCodes"),
                    () => DB.UnliquidatedObligations.Select(u => u.Prog).Distinct().OrderBy(p => p).ToList().AsReadOnly(),
                    UloHelpers.MediumCacheTimeout
                    );

            var wd = await DB.WorkflowDefinitions.Where(wfd => wfd.WorkflowDefinitionName == "ULO Workflow" && wfd.IsActive == true).OrderByDescending(wfd=>wfd.Version).FirstOrDefaultAsync();
            var activityNames = GetOrderedActivityNameByWorkflowName().AtomEnumerable.ConvertAll(z => z.Value).Distinct().OrderBy().ToList();
            var statuses = wd.Description.WebActionWorkflowActivities.OrderBy(a => a.SequenceNumber).Select(a => a.ActivityName).ToList();

            return View(
                "~/Views/Ulo/Search/Index.cshtml", 
                new FilterViewModel(
                    workflows, 
                    PortalHelpers.CreateDocumentTypeSelectListItems(),
                    PortalHelpers.CreateZoneSelectListItems(),
                    PortalHelpers.CreateRegionSelectListItems(),
                    baCodes,
                    activityNames));
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

        [Route("Ulos/{uloId}/{workflowId}", Order = 1)]
        [Route("Ulos/{uloId}", Order = 2)]
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

            var workflow = await FindWorkflowAsync(workflowId);
            var workflowAssignedToCurrentUser = CurrentUserId == workflow.OwnerUserId;

            var workflowDesc = await FindWorkflowDescAsync(workflow);

            return View("Details/Index", new UloViewModel(ulo, workflow, workflowDesc, workflowAssignedToCurrentUser));
        }

        public async Task<ActionResult> RegionWorkflowDetails(int uloId, int workflowId)
        {
            //TODO: check if current user is able to view
            var ulo = await DB.UnliquidatedObligations.Include(u => u.Notes).FirstOrDefaultAsync(u => u.UloId == uloId);
            var workflow = await FindWorkflowAsync(workflowId);
            var workflowDesc = await FindWorkflowDescAsync(workflow);
            var workflowAssignedToCurrentUser = CurrentUserId == workflow.OwnerUserId;
            return View("Details/Index", new UloViewModel(ulo, workflow, workflowDesc, workflowAssignedToCurrentUser));
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
        [Route("Ulos/{uloId}/{workflowId}/Save")]
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
                var submit = Request["WhatNext"] == "Submit";
                var question = await DB.UnliqudatedObjectsWorkflowQuestions.Where(z => z.WorkflowId == workflowId).OrderByDescending(z => z.UnliqudatedWorkflowQuestionsId).FirstOrDefaultAsync();
                if (question == null || !question.Pending)
                {
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