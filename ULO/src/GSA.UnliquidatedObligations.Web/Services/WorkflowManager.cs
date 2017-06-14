using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using GSA.UnliquidatedObligations.Web.Models;
using Hangfire;
using Microsoft.AspNet.Identity;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class WorkflowManager : IWorkflowManager
    {
        public const string WorkflowIdRouteValueName = "workflowId";

        private readonly IComponentContext ComponentContext;
        private readonly IWorkflowDescriptionFinder Finder;
        private readonly IBackgroundJobClient BackgroundJobClient;
        protected readonly ULODBEntities DB;

        public WorkflowManager(IComponentContext componentContext, IWorkflowDescriptionFinder finder, IBackgroundJobClient backgroundJobClient, ULODBEntities db)
        {
            ComponentContext = componentContext;
            Finder = finder;
            BackgroundJobClient = backgroundJobClient;
            DB = db;

        }

        private class RedirectingController : Controller
        {
            public new ActionResult RedirectToAction(string actionName, string controllerName, RouteValueDictionary routeValues)
            {
                return base.RedirectToAction(actionName, controllerName, routeValues);
            }
        }

        async Task<ActionResult> IWorkflowManager.AdvanceAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question, bool forceAdvance = false)
        {
            string nextOwnerId = "";
            var desc = await (this as IWorkflowManager).GetWorkflowDescription(wf);
            var currentActivity = desc.WebActionWorkflowActivities.FirstOrDefault(z => z.WorkflowActivityKey == wf.CurrentWorkflowActivityKey);

            //if question is null stays in current activity
            string nextActivityKey = "";
            WorkflowActivity nextActivity;
            if (question != null)
            {
                var chooser = ComponentContext.ResolveNamed<IActivityChooser>(currentActivity.NextActivityChooserTypeName);
                nextActivityKey = chooser.GetNextActivityKey(wf, question, currentActivity.NextActivityChooserConfig);
                nextActivity = desc.Activities.First(z => z.WorkflowActivityKey == nextActivityKey) ?? currentActivity;
            }
            else
            {
                nextActivity = currentActivity;
            }

            //TODO: Handle null case which says stay where you are.

            wf.CurrentWorkflowActivityKey = nextActivity.WorkflowActivityKey;


            //TODO: Updata other info like the owner, date
            //TODO: Add logic for handling groups of users.
                if (nextActivity is WebActionWorkflowActivity)
                {
                    if (wf.OwnerUserId != nextActivity.OwnerUserId || forceAdvance == true)
                    {
                        nextOwnerId = await GetNextOwnerAsync(nextActivity.OwnerUserId, wf,
                            nextActivity.WorkflowActivityKey);
                        wf.OwnerUserId = nextOwnerId;
                        var nextUser = await DB.AspNetUsers.FindAsync(wf.OwnerUserId);
                        var emailTemplate = await DB.EmailTemplates.FindAsync(nextActivity.EmailTemplateId);
                        var emailModel = new EmailViewModel
                        {
                            UserName = nextUser.UserName,
                            PDN = wf.UnliquidatedObligation.PegasysDocumentNumber
                        };
                        //TODO: What happens if it crashes?
                        //BackgroundJobClient.Enqueue<IBackgroundTasks>(
                           // bt => bt.Email("new owner", nextUser.Email, emailTemplate.EmailBody, emailModel));
                    }
                    wf.UnliquidatedObligation.Status = nextActivity.ActivityName;

                    if (nextActivity.DueIn != null)
                        wf.ExpectedDurationInSeconds = (long?)nextActivity.DueIn.Value.TotalSeconds;

                    if (question != null && question.Answer == "Valid")
                    {
                        wf.UnliquidatedObligation.Valid = true;
                    }
                    else if (question != null && question.Answer == "Invalid")
                    {
                        wf.UnliquidatedObligation.Valid = false;
                    }

                    //TODO: if owner changes, look at other ways of redirecting.

                    var next = (WebActionWorkflowActivity)nextActivity;
                    var c = new RedirectingController();

                    var routeValues = new RouteValueDictionary(next.RouteValueByName);
                    //routeValues[WorkflowIdRouteValueName] = wf.WorkflowId;
                    return c.RedirectToAction(next.ActionName, next.ControllerName, routeValues);
                }
                else
                {
                    //TODO: handle background hangfire.
                    throw new NotImplementedException();
                }
        }

        async Task<ActionResult> IWorkflowManager.RequestReassign(Workflow wf)
        {
            //TODO: Get programatically based on user's region
            var reassignGroupId = await GetNextOwnerAsync("ab9684e5-a277-41df-a268-f861416a3f0e", wf, "");
            wf.OwnerUserId = reassignGroupId;
            var c = new RedirectingController();
            var routeValues = new RouteValueDictionary(new Dictionary<string, object>());
            return await Task.FromResult(c.RedirectToAction("Index", "Ulo", routeValues));
        }

        async Task IWorkflowManager.SaveQuestion(Workflow wf, UnliqudatedObjectsWorkflowQuestion question)
        {
            var questionFromDB = await DB.UnliqudatedObjectsWorkflowQuestions.FirstOrDefaultAsync(q => q.WorkflowId == wf.WorkflowId && q.Pending == true);
            if (questionFromDB == null)
            {
                wf.UnliqudatedObjectsWorkflowQuestions.Add(question);
            }
            else
            {
                questionFromDB.Answer = questionFromDB.Answer;
                questionFromDB.Comments = question.Comments;
                questionFromDB.JustificationId = question.JustificationId;
                questionFromDB.Date = DateTime.Now;
                questionFromDB.UserId = question.UserId;
                questionFromDB.Pending = question.Pending;
            }
        }

        async Task<ActionResult> IWorkflowManager.Reassign(Workflow wf, string userId, string actionName)
        {
            //TODO: Get programatically based on user's region
            //TODO: split up into two for redirect and reassign.
            wf.OwnerUserId = userId;
            var c = new RedirectingController();
            var routeValues = new RouteValueDictionary(new Dictionary<string, object>());
            var nextUser = await DB.AspNetUsers.FindAsync(wf.OwnerUserId);
            var emailTemplate = await DB.EmailTemplates.FindAsync(1);
            var emailModel = new EmailViewModel
            {
                UserName = nextUser.UserName,
                PDN = wf.UnliquidatedObligation.PegasysDocumentNumber
            };
            //TODO: What happens if it crashes?
            BackgroundJobClient.Enqueue<IBackgroundTasks>(bt => bt.Email("new owner", nextUser.Email, emailTemplate.EmailBody, emailModel));

            return await Task.FromResult(c.RedirectToAction(actionName, "Ulo", routeValues));
        }



        private async Task<string> GetNextOwnerAsync(string proposedOwnerId, Workflow wf, string nextActivityKey)
        {
            //TODO: check if null, return proposedOwnserId
            var output = new ObjectParameter("nextOwnerId", typeof(string));
            //DB.Database.Log = s => Trace.WriteLine(s);
            DB.GetNextLevelOwnerId(proposedOwnerId, wf.WorkflowId, nextActivityKey, output);
            if (output.Value == DBNull.Value)
            {
                return await Task.FromResult(proposedOwnerId);
            }
            return await Task.FromResult(output.Value.ToString());

        }



        async Task<IWorkflowDescription> IWorkflowManager.GetWorkflowDescription(Workflow wf)
        {
            return await Finder.FindAsync(wf.WorkflowKey, wf.Version);
        }
    }
}