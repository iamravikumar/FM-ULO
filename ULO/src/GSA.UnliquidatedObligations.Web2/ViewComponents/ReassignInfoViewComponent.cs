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

        public ReassignInfoViewComponent(IWorkflowManager manager,UloDbContext context)
        {
            UloDb = context;
            Manager = manager;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? id, int workflowId, int uloRegionId, string wfDefintionOwnerName = "", bool isAdmin = false, DetailsBulkToken bulkToken = null)
        {
           // bulkToken = (bulkToken != null && bulkToken.IsValid) ? bulkToken : new DetailsBulkToken(CurrentUser, DB, workflowId);
            var db = UloDb;
            var CurrentUser = bulkToken.CurrentUser;           

            RequestForReassignment requestForReassignment = null;
            if (id.HasValue)
            {
                requestForReassignment = await db.RequestForReassignment.Include(z => z.UnliqudatedWorkflowQuestions).FirstOrDefaultAsync(r => r.RequestForReassignmentID == id.Value);
            }

            var workflow = await db.Workflows.FindAsync(workflowId);
            
            var wfDesc = Manager.GetWorkflowDescriptionAsync(workflow).Result;

            IList<SelectListItem> userSelectItems = new List<SelectListItem>(); ;
           
            if (workflow.OwnerUserId == GetUserId(CurrentUser.UserName,db))
            {
                userSelectItems.Remove(userSelectItems.Where(z => z.Value == GetUserId(CurrentUser.UserName,db)).ToList());
            }

            userSelectItems = userSelectItems.OrderBy(z => z.Text).ToList();

            userSelectItems.Insert(0, CreateUserSelectListItem(GetUserId("Reassign Group",db), "Reassign Group"));

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


        public string GetUserId(string username, UloDbContext uloDbContext)
        {
            string strUserID = string.Empty; 
            if (username != null)
            { 
                strUserID = uloDbContext.AspNetUsers.Where(z => z.UserName == username).Select(z => z.Id).FirstOrDefault(); 
            }                             
            
            return strUserID;
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
