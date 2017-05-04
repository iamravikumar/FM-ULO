using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class RequestForReassignmentsController : Controller
    {
        protected readonly IWorkflowManager Manager;
        protected readonly ULODBEntities DB;
        private readonly ApplicationUserManager UserManager;

        public RequestForReassignmentsController(IWorkflowManager manager, ULODBEntities db, ApplicationUserManager userManager)
        {
            Manager = manager;
            DB = db;
            UserManager = userManager;
        }

        // GET: RequestForReassignments
        public async Task<ActionResult> Index()
        {
            var requestForReassignments = DB.RequestForReassignments.Include(r => r.AspNetUser).Include(r => r.UnliqudatedObjectsWorkflowQuestion).Include(r => r.Workflow);
            return View(await requestForReassignments.ToListAsync());
        }

        // GET: RequestForReassignments/Details/5
        public ActionResult Details(int? id)
        {
            var requestForReassignment = DB.RequestForReassignments.Where(rr => rr.IsActive).FirstOrDefault(r => r.RequestForReassignmentID == id);

            var users = DB.AspNetUsers.OrderBy(u => u.UserName).Where(u => u.UserType == "Person").ToList();
            var justEnums = new List<JustificationEnum>()
            {
                JustificationEnum.ReassignNeedHelp,
                JustificationEnum.ReassignVaction,
                JustificationEnum.Other
            };
            var requestForReassignmentId = requestForReassignment?.RequestForReassignmentID;
            var suggestedReviewerId = requestForReassignment != null ? requestForReassignment.SuggestedReviewerId : "";
            var justificationId = requestForReassignment?.UnliqudatedObjectsWorkflowQuestion.JustificationId;
            var comments = requestForReassignment != null
                ? requestForReassignment.UnliqudatedObjectsWorkflowQuestion.Comments : "";

            return PartialView("~/Views/RequestForReassignments/_Index.cshtml", new RequestForReassignmentViewModel(suggestedReviewerId, justificationId, requestForReassignmentId, comments, users, justEnums));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reassign(int workflowId, [Bind(Include = "SuggestedReviewerId,JustificationId,Comments")] RequestForReassignmentViewModel requestForReassignmentViewModel)
        {
            if (ModelState.IsValid)
            {
                var wf = await FindWorkflowAsync(workflowId);
                if (wf == null) return HttpNotFound();
                var user = await DB.AspNetUsers.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                var requestForReassignment =
                    await DB.RequestForReassignments.FirstOrDefaultAsync(
                        rr => rr.WorkflowId == workflowId);
                var question = new UnliqudatedObjectsWorkflowQuestion
                {
                    Date = DateTime.Now,
                    JustificationId = requestForReassignmentViewModel.JustificationId,
                    UserId = user.Id,
                    Answer = "Reassigned",
                    WorkflowId = workflowId,
                    Comments = requestForReassignmentViewModel.Comments
                };
                DB.UnliqudatedObjectsWorkflowQuestions.Add(question);
                await DB.SaveChangesAsync();

                return await Reassign(wf, question, requestForReassignment, requestForReassignmentViewModel.SuggestedReviewerId);
            }

            return PartialView("~/Views/RequestForReassignments/_Index.cshtml", requestForReassignmentViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RequestReassign(int workflowId, [Bind(Include = "WorkflowId,SuggestedReviewerId,JustificationId,Comments")] RequestForReassignmentViewModel requestForReassignmentViewModel)
        {
            if (ModelState.IsValid)
            {
                var wf = await FindWorkflowAsync(workflowId);
                if (wf == null) return HttpNotFound();
                var user = await DB.AspNetUsers.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                var question = new UnliqudatedObjectsWorkflowQuestion
                {
                    Date = DateTime.Now,
                    JustificationId = requestForReassignmentViewModel.JustificationId,
                    UserId = user.Id,
                    Answer = "Request for Reasssignment",
                    WorkflowId = workflowId,
                    Comments = requestForReassignmentViewModel.Comments
                };
                DB.UnliqudatedObjectsWorkflowQuestions.Add(question);
                await DB.SaveChangesAsync();

                return await RequestReassign(wf, question, requestForReassignmentViewModel.SuggestedReviewerId);
            }

            return PartialView("~/Views/RequestForReassignments/_Index.cshtml", requestForReassignmentViewModel);
        }


        // GET: RequestForReassignments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestForReassignment requestForReassignment = await DB.RequestForReassignments.FindAsync(id);
            if (requestForReassignment == null)
            {
                return HttpNotFound();
            }
            return View(requestForReassignment);
        }

        // POST: RequestForReassignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RequestForReassignment requestForReassignment = await DB.RequestForReassignments.FindAsync(id);
            DB.RequestForReassignments.Remove(requestForReassignment);
            await DB.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        //TODO: Move to Manager?
        private async Task<Workflow> FindWorkflowAsync(int workflowId)
        {
            var wf = await DB.Workflows.Include(q => q.AspNetUser).Include(q => q.UnliquidatedObligation).FirstOrDefaultAsync(q => q.WorkflowId == workflowId);
            if (wf != null)
            {
                var currentUser = await UserManager.FindByNameAsync(this.User.Identity.Name);
                var groupsUserBelongsTo = await GetUsersGroups(currentUser.Id);
                if (currentUser != null)
                {
                    if (wf.OwnerUserId == currentUser.Id || groupsUserBelongsTo.Contains(wf.OwnerUserId)) return wf;
                    if (wf.AspNetUser.UserType == UserTypes.Group.ToString())
                    {
                        //TODO: Write recursive then call recursive sproc to see if current user is in the group
                    }
                }
            }
            return null;
        }

        private async Task<ActionResult> RequestReassign(Workflow wf, UnliqudatedObjectsWorkflowQuestion question, string suggestedReviewerId)
        {
            var requestForReassignment = new RequestForReassignment
            {
                SuggestedReviewerId = suggestedReviewerId,
                UnliqudatedWorkflowQuestionsId = question.UnliqudatedWorkflowQuestionsId,
                WorkflowId = wf.WorkflowId,
                IsActive = true

            };
            DB.RequestForReassignments.Add(requestForReassignment);
            var ret = await Manager.RequestReassign(wf);
            await DB.SaveChangesAsync();
            return ret;
        }

        private async Task<ActionResult> Reassign(Workflow wf, UnliqudatedObjectsWorkflowQuestion question, RequestForReassignment requestForReassignment, string suggestedReviewerId)
        {
            requestForReassignment.SuggestedReviewerId = suggestedReviewerId;
            requestForReassignment.UnliqudatedWorkflowQuestionsId = question.UnliqudatedWorkflowQuestionsId;
            requestForReassignment.WorkflowId = wf.WorkflowId;
            requestForReassignment.IsActive = false;
            var ret = await Manager.Reassign(wf, suggestedReviewerId);
            await DB.SaveChangesAsync();
            return ret;
        }


        private async Task<List<string>> GetUsersGroups(string userId)
        {
            return await DB.UserUsers.Where(uu => uu.ChildUserId == userId).Select(uu => uu.ParentUserId).ToListAsync();
        }

    }
}
