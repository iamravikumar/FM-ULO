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
        protected readonly IWorkflowManager Manager;
        private readonly ApplicationUserManager UserManager;


        public UloController(IWorkflowManager manager, ApplicationUserManager userManager, ULODBEntities db, IComponentContext componentContext, ICacher cacher)
            : base(db, componentContext, cacher)
        {
            Manager = manager;
            UserManager = userManager;
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

        // GET: Ulo
        public ActionResult Index(string sortCol, string sortDir, int? page, int? pageSize)
        {
            //TODO: Due dates: calculate in model or add additional column in workflow table (ExpectedActivityDurationInSeconds, nullable, DueAt = null) 
            var workflows = ApplyBrowse(
                DB.Workflows.Where(wf => wf.OwnerUserId == CurrentUserId).Include(wf => wf.UnliquidatedObligation).Include(wf=>wf.UnliquidatedObligation.Region),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);
            //TODO: A little hacky
            ViewBag.ShowReassignButton = false;
            return View(workflows);
        }

        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanViewUnassigned)]
        public async Task<ActionResult> Unassigned(string sortCol, string sortDir, int? page, int? pageSize)
        {
            //TODO: Due dates: calculate in model or add additional column in workflow table (ExpectedActivityDurationInSeconds, nullable, DueAt = null) 
            var userIds = await GetUsersGroupsAsync(CurrentUserId);
            var workflows = ApplyBrowse(
                DB.Workflows.Where(wf => userIds.Contains(wf.OwnerUserId)).Include(wf => wf.UnliquidatedObligation),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);
            ViewBag.ShowReassignButton = true;
            return View(workflows);
        }

        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanReassign)]
        public async Task<ActionResult> RequestForReassignments(string sortCol, string sortDir, int? page, int? pageSize)
        {
            var reassignGroupUserId = PortalHelpers.ReassignGroupUserId;

            var reassignGroupRegionIds = await DB.UserUsers
                .Where(uu => uu.ParentUserId == reassignGroupUserId && uu.ChildUserId == CurrentUserId)
                .Select(uu => uu.RegionId)
                .Distinct()
                .ToListAsync();

            var workflows = ApplyBrowse(
                DB.Workflows.Where(wf => wf.OwnerUserId == reassignGroupUserId && reassignGroupRegionIds.Contains(wf.UnliquidatedObligation.RegionId))
                .Include(wf => wf.UnliquidatedObligation),
                sortCol ?? nameof(Workflow.DueAtUtc), sortDir, page, pageSize);

            ViewBag.ShowReassignButton = true;

            return View(workflows);
        }

        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanViewOtherWorkflows)]
        [Route("Ulo/Search")]
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

            var allSubjectCategoryClaimsValues =
                Enum.GetValues(typeof(SubjectCatagoryNames))
                    .Cast<SubjectCatagoryNames>()
                    .Select(scc => scc.GetDisplayName())
                    .OrderBy(scc => scc)
                    .ToList();

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
                    allSubjectCategoryClaimsValues, 
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

        public async Task<ActionResult> Details(int uloId, int workflowId)
        {
            //TODO: check if current user is able to view
            var ulo = await DB.UnliquidatedObligations.Include(u => u.Notes).FirstOrDefaultAsync(u => u.UloId == uloId);

            var comingFromReassignmentsPage = isReassignmentReferral();

            var workflow = await FindWorkflowAsync(workflowId, !isUnassignedReferral(), checkReassignmentsGroup: comingFromReassignmentsPage);
            var workflowAssignedToCurrentUser = CurrentUserId == workflow.OwnerUserId;

            var workflowDesc = await FindWorkflowDescAsync(workflow);

            return View("Details/Index", new UloViewModel(ulo, workflow, workflowDesc, workflowAssignedToCurrentUser));
        }

        private bool isReassignmentReferral()
        {
            return Request.UrlReferrer?.LocalPath == "/Ulo/RequestForReassignments";
        }

        private bool isUnassignedReferral()
        {
            return Request.UrlReferrer?.LocalPath == "/Ulo/Unassigned";
        }
        private bool isReassignment()
        {
            return Request.Url.Host == "/RequestForReassignments";
        }

        public async Task<ActionResult> RegionWorkflowDetails(int uloId, int workflowId)
        {
            //TODO: check if current user is able to view
            var ulo = await DB.UnliquidatedObligations.Include(u => u.Notes).FirstOrDefaultAsync(u => u.UloId == uloId);
            var workflow = await FindWorkflowAsync(workflowId, false);
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
        private async Task<Workflow> FindWorkflowAsync(int workflowId, bool checkOwner = true, bool checkReassignmentsGroup = false)
        {
            var wf = await DB.Workflows.Include(q => q.AspNetUser).Include(q => q.UnliquidatedObligation).FirstOrDefaultAsync(q => q.WorkflowId == workflowId);
            if (wf != null)
            {
                if (checkOwner == false) return wf;
                if (CurrentUserId != null)
                {
                    var groupsUserBelongsTo = await GetUsersGroupsAsync(CurrentUserId, checkReassignmentsGroup);
                    if (wf.OwnerUserId == CurrentUserId || groupsUserBelongsTo.Contains(wf.OwnerUserId)) return wf;
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
        [SubmitButtonSelector(Name = "Advance")]
        public async Task<ActionResult> Advance(
            int workflowId,
            int uloId,
            [Bind(Include = 
                nameof(AdvanceViewModel.JustificationKey)+","+
                nameof(AdvanceViewModel.Answer)+","+
                nameof(AdvanceViewModel.ExpectedDateForCompletion)+","+
                nameof(AdvanceViewModel.Comments)+","+
                nameof(AdvanceViewModel.UnliqudatedWorkflowQuestionsId))]
            AdvanceViewModel advanceModel)
        {
            var wf = await FindWorkflowAsync(workflowId);
            if (wf == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                var question = new UnliqudatedObjectsWorkflowQuestion
                {
                    JustificationKey = advanceModel.JustificationKey,
                    UserId = CurrentUserId,
                    Answer = advanceModel.Answer,
                    WorkflowId = workflowId,
                    Comments = advanceModel.Comments,
                    Pending = false,
                    UnliqudatedWorkflowQuestionsId = advanceModel.UnliqudatedWorkflowQuestionsId,
                    WorkflowRowVersion = wf.WorkflowRowVersion,
                    CreatedAtUtc = DateTime.UtcNow
                };
                wf.UnliquidatedObligation.ExpectedDateForCompletion = advanceModel.ExpectedDateForCompletion;
                return await AdvanceAsync(wf, question);
            }
            return await Details(uloId, workflowId);
        }

        [HttpPost]
        [SubmitButtonSelector(Name = "Save")]
        public async Task<ActionResult> SaveQuestion(int workflowId, int uloId,
            [Bind(Include =
                nameof(AdvanceViewModel.JustificationKey)+","+
                nameof(AdvanceViewModel.Answer)+","+
                nameof(AdvanceViewModel.Comments)+","+
                nameof(AdvanceViewModel.ExpectedDateForCompletion)+","+
                nameof(AdvanceViewModel.UnliqudatedWorkflowQuestionsId))]
        AdvanceViewModel advanceModel)
        {
            var wf = await FindWorkflowAsync(workflowId);
            if (wf == null) return HttpNotFound();
            var question = new UnliqudatedObjectsWorkflowQuestion
            {
                JustificationKey = advanceModel.JustificationKey,
                UserId = CurrentUserId,
                Answer = advanceModel.Answer,
                WorkflowId = workflowId,
                Comments = advanceModel.Comments,
                Pending = true,
                UnliqudatedWorkflowQuestionsId = advanceModel.UnliqudatedWorkflowQuestionsId,
                WorkflowRowVersion = wf.WorkflowRowVersion,
                CreatedAtUtc = DateTime.UtcNow
            };
            wf.UnliquidatedObligation.ExpectedDateForCompletion = advanceModel.ExpectedDateForCompletion;
            await Manager.SaveQuestionAsync(wf, question);
            await DB.SaveChangesAsync();
            return await Details(uloId, workflowId);
        }

        private Task<IEnumerable<string>> GetUsersGroupsAsync(string userId, bool includeReassignmentGroup=false)
        {
            IEnumerable<string> ids;
            if (!includeReassignmentGroup)
            {
                ids = GetUserGroups(userId).ConvertAll(z => z.UserId).Where(z => z != PortalHelpers.ReassignGroupUserId);
            }
            else
            {
                ids = GetUserGroups(userId).ConvertAll(z => z.UserId);
            }
            return Task.FromResult(ids);
        }

        private async Task<ActionResult> AdvanceAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question)
        {
            await Manager.SaveQuestionAsync(wf, question);
            var ret = await Manager.AdvanceAsync(wf, question);
            await DB.SaveChangesAsync();
            return ret;
        }
    }
}