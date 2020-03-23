using System.Collections.Generic;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [ViewComponent(Name=nameof(ReassignInfoViewComponent))]
    public class ReassignInfoViewComponent : BaseViewComponent
    {
        private readonly IWorkflowManager Manager;

        public ReassignInfoViewComponent(IWorkflowManager manager, UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, Serilog.ILogger logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            Manager = manager;
        }

        private string MungeReviewerName(string username, bool? isQualified)
            => string.Format(
                isQualified.GetValueOrDefault() ? PortalHelpers.GetEligibleReviewersQualifiedUsernameFormat : PortalHelpers.GetEligibleReviewersNotQualifiedUsernameFormat, username);

        public IViewComponentResult Invoke(int? id, int workflowId, int uloRegionId, string wfDefintionOwnerName = "", bool isAdmin = false, RequestForReassignmentsController.DetailsBulkToken bulkToken = null)
        {
            bulkToken = (bulkToken != null && bulkToken.IsValid) ? bulkToken : new RequestForReassignmentsController.DetailsBulkToken(CurrentUser, DB, workflowId);
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
            if (PortalHelpers.UseOldGetEligibleReviewersAlgorithm)
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
            return View("~/Views/Ulo/Details/Workflow/RequestForReassignments/" + detailsView, new RequestForReassignmentViewModel(suggestedReviewerId, justificationKey, requestForReassignmentId, comments, workflowId, uloRegionId, userSelectItems, wfDesc.GetResassignmentJustifications()));
        }
    }
}
