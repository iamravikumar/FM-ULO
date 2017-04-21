using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using System;
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
            var desc = await Finder.FindAsync(wf.WorkflowKey, wf.Version);
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
                    wf.OwnerUserId = nextActivity.OwnerUserId;
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

        async Task<WebActionWorkflowActivity> IWorkflowManager.GetCurrentWebActivity(Workflow wf)
        {
            return
                await Finder.FindAsync(wf.WorkflowKey, wf.Version)
                    .Result.GetWebActivityById(wf.CurrentWorkflowActivityKey);
        }
    }
}