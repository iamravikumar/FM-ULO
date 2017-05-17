using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
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

        async Task<ActionResult> IWorkflowManager.AdvanceAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question)
        {
            var desc = await (this as IWorkflowManager).GetWorkflowDescription(wf);
            var currentActivity = desc.WebActionWorkflowActivities.FirstOrDefault(z => z.WorkflowActivityKey == wf.CurrentWorkflowActivityKey);
            var chooser = ComponentContext.ResolveNamed<IActivityChooser>(currentActivity.NextActivityChooserTypeName);
            var nextActivityKey = chooser.GetNextActivityKey(wf, question, currentActivity.NextActivityChooserConfig);

            //TODO: Handle null case which says stay where you are.
            var nextActivity = desc.Activities.First(z => z.WorkflowActivityKey == nextActivityKey) ?? currentActivity;
            wf.CurrentWorkflowActivityKey = nextActivity.WorkflowActivityKey;


            //TODO: Updata other info like the owner, date
            //TODO: Add logic for handling groups of users.
            if (nextActivity is WebActionWorkflowActivity)
            {
                if (wf.OwnerUserId != nextActivity.OwnerUserId)
                {
                    wf.OwnerUserId = await GetNextOwnerAsync(nextActivity.OwnerUserId, wf, nextActivity.WorkflowActivityKey);
                    var nextUser = await DB.AspNetUsers.FindAsync(wf.OwnerUserId);
                    var emailTemplate = await DB.EmailTemplates.FindAsync(nextActivity.EmailTemplateId);
                    var emailModel = new EmailViewModel
                    {
                        UserName = nextUser.UserName,
                        PDN = wf.UnliquidatedObligation.PegasusDocumentNumber
                    };
                    //TODO: What happens if it crashes?
                    BackgroundJobClient.Enqueue<IBackgroundTasks>(bt => bt.Email("new owner", nextUser.Email, emailTemplate.EmailBody, emailModel));
                }
                wf.UnliquidatedObligation.Status = nextActivity.ActivityName;
                wf.UnliqudatedObjectsWorkflowQuestions.Add(question);
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
            var reassignGroupId =  await GetNextOwnerAsync("ab9684e5-a277-41df-a268-f861416a3f0e", wf, "");
            wf.OwnerUserId = reassignGroupId;
            var c = new RedirectingController();
            var routeValues = new RouteValueDictionary(new Dictionary<string, object>());
            return await Task.FromResult(c.RedirectToAction("Index", "Ulo", routeValues));
        }

        async Task<ActionResult> IWorkflowManager.Reassign(Workflow wf, string userId, string actionName)
        {
            //TODO: Get programatically based on user's region
            wf.OwnerUserId = userId;
            var c = new RedirectingController();
            var routeValues = new RouteValueDictionary(new Dictionary<string, object>());
            var nextUser = await DB.AspNetUsers.FindAsync(wf.OwnerUserId); 
            var emailTemplate = await DB.EmailTemplates.FindAsync(1);
                    var emailModel = new EmailViewModel
                    {
                        UserName = nextUser.UserName,
                        PDN = wf.UnliquidatedObligation.PegasusDocumentNumber
                    };
                    //TODO: What happens if it crashes?
                    BackgroundJobClient.Enqueue<IBackgroundTasks>(bt => bt.Email("new owner", nextUser.Email, emailTemplate.EmailBody, emailModel));
        
            return await Task.FromResult(c.RedirectToAction(actionName, "Ulo", routeValues));
        }

        private async Task<string> GetNextOwnerAsync(string proposedOwnerId, Workflow wf, string nextActivityKey)
        {
            //TODO: check if null, return proposedOwnserId
            var output = new ObjectParameter("nextOwnerId", typeof(string));
            DB.GetNextLevelOwnerId(proposedOwnerId, wf.WorkflowId, nextActivityKey, output);
            return await Task.FromResult(output.Value.ToString());
        }



        async Task<IWorkflowDescription> IWorkflowManager.GetWorkflowDescription(Workflow wf)
        {
            return await Finder.FindAsync(wf.WorkflowKey, wf.Version);
        }
    }
}