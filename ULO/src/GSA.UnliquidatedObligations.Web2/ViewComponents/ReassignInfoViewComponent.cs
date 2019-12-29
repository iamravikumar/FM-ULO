using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core.Collections;
using static GSA.UnliquidatedObligations.Web.Controllers.RequestForReassignmentsController;

namespace GSA.UnliquidatedObligations.Web.ViewComponents
{
    public class ReassignInfoViewComponent :   ViewComponent
    {
        private readonly IWorkflowManager Manager;

        private readonly UloDbContext UloDb;

        private readonly PortalHelpers PortalHelpers;

        private readonly UserHelpers UserHelpers;

        private readonly ICacher Cacher;

        public ReassignInfoViewComponent(IWorkflowManager manager,UloDbContext context, PortalHelpers portalHelpers, UserHelpers userHelpers, ICacher cacher)
        {
            UloDb = context;
            Manager = manager;
            PortalHelpers = portalHelpers;
            UserHelpers = userHelpers;
            Cacher = cacher;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? id, int workflowId, int uloRegionId, string wfDefintionOwnerName = "", bool isAdmin = false, DetailsBulkToken bulkToken = null)
        {           
            var db = UloDb;
            var CurrentUser = GetCurrentUser(db);

            //Bulk Token
            bulkToken = (bulkToken != null && bulkToken.IsValid) ? bulkToken : new DetailsBulkToken(CurrentUser, db, workflowId);

            //RequestForReassignment
            RequestForReassignment requestForReassignment = null;
            if (id.HasValue)
            {
                requestForReassignment = await db.RequestForReassignment.Include(z => z.UnliqudatedWorkflowQuestions).FirstOrDefaultAsync(r => r.RequestForReassignmentID == id.Value);
            }


            //workflow
            var workflow = await db.Workflows.FindAsync(workflowId);            
            var wfDesc = Manager.GetWorkflowDescriptionAsync(workflow).Result;

            string groupOwnerId;
            if (wfDefintionOwnerName == "")
            {
                var currentActivity = wfDesc.WebActionWorkflowActivities
                    .FirstOrDefault(a => a.WorkflowActivityKey == workflow.CurrentWorkflowActivityKey);
                groupOwnerId = GetUserId(currentActivity.OwnerUserName, db);
            }
            else
            {
                groupOwnerId = GetUserId(wfDefintionOwnerName, db);
            }

            //User
            IList<SelectListItem> userSelectItems = new List<SelectListItem>();
            if (PortalHelpers.UseOldGetEligibleReviewersAlgorithm)
            {
                var prohibitedUserIds = bulkToken.ProhibitedOwnerIdsByWorkflowId[workflowId];

                userSelectItems = Cacher.FindOrCreateValue(
                    Cache.CreateKey(groupOwnerId, uloRegionId, "fdsfdsaf"),
                    () => db.UserUsers
                        .Where(uu => uu.ParentUserId == groupOwnerId && uu.RegionId == uloRegionId && uu.ChildUser.UserType == AspNetUser.UserTypes.Person)
                        .Select(uu => new { UserName = uu.ChildUser.UserName, UserId = uu.ChildUserId }).ToList(),
                        PortalHelpers.MediumCacheTimeout
                        ).ConvertAll(z => CreateUserSelectListItem(z.UserId, z.UserName, prohibitedUserIds.Contains(z.UserId))).ToList();

                if (Cacher.FindOrCreateValue(
                    Cache.CreateKey(uloRegionId, User.Identity.Name),
                    () =>
                    {
                        var userReassignRegions = UserHelpers.GetReassignmentGroupRegions(User);
                        return UserHelpers.HasPermission(User, ApplicationPermissionNames.CanReassign) && userReassignRegions.Contains(uloRegionId); 

                    },
                    PortalHelpers.MediumCacheTimeout
                    ))
                {
                    userSelectItems.Add(ToSelectListItem(bulkToken.CurrentUser));
                }
            }
            else
            {
                userSelectItems = new List<SelectListItem>();
                foreach (var p in bulkToken.PotentialReviewersByWorkflowId[workflowId])
                {
                    string text = MungeReviewerName(p.UserName, p.IsQualified);
                    userSelectItems.Add(CreateUserSelectListItem(p.UserId, text));
                }
            }

            if (workflow.OwnerUserId == GetUserId(CurrentUser.UserName,db))
            {
                userSelectItems.Remove(userSelectItems.Where(z => z.Value == GetUserId(CurrentUser.UserName,db)).ToList());
            }
            userSelectItems = userSelectItems.OrderBy(z => z.Text).ToList();
            userSelectItems.Insert(0, CreateUserSelectListItem(GetUserId("Reassign Group",db), "Reassign Group"));


            // parameters values
            var requestForReassignmentId = requestForReassignment?.RequestForReassignmentID;
            var suggestedReviewerId = requestForReassignment != null ? requestForReassignment.SuggestedReviewerId : "";
            var justificationKey =  requestForReassignment?.UnliqudatedWorkflowQuestions.JustificationKey;

            var comments = requestForReassignment != null
                ? requestForReassignment.UnliqudatedWorkflowQuestions.Comments : "";

            var detailsView = isAdmin ? "_DetailsMasterList.cshtml" : "_Details.cshtml";
            return View(
                "~/Views/Ulo/Details/Workflow/RequestForReassignments/" + detailsView,
                new RequestForReassignmentViewModel(suggestedReviewerId, justificationKey, requestForReassignmentId, comments, workflowId, uloRegionId, userSelectItems, null));
        }

        private string MungeReviewerName(string username, bool? isQualified)
           => string.Format(
               isQualified.GetValueOrDefault() ? PortalHelpers.GetEligibleReviewersQualifiedUsernameFormat : PortalHelpers.GetEligibleReviewersNotQualifiedUsernameFormat, username);

        public string GetUserId(string username, UloDbContext uloDbContext)
        {
            string strUserID = string.Empty; 
            if (username != null)
            { 
                strUserID = uloDbContext.AspNetUsers.Where(z => z.UserName == username).Select(z => z.Id).FirstOrDefault(); 
            }                             
            
            return strUserID;
        }

        public AspNetUser GetCurrentUser(UloDbContext uloDbContext)
        {
            return uloDbContext.AspNetUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);            
        }

        public IList<SelectListItem> CreateSelectList(IEnumerable<AspNetUser> aspNetUsers)
         => aspNetUsers.Select(z => CreateUserSelectListItem(z.Id, z.UserName)).ToList();

        public SelectListItem ToSelectListItem(AspNetUser u, bool disabled = false)
            => CreateUserSelectListItem(u.Id, u.UserName, disabled);

        public SelectListItem CreateUserSelectListItem(string userId, string username, bool disabled = false)
            => new SelectListItem
            {
                Text = username,
                Value = userId,
                Disabled = disabled
            };
    }
}
