using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class WorkflowManager : IWorkflowManager
    {
        public const string WorkflowIdRouteValueName = "workflowId";

        private readonly IServiceProvider ServiceProvider;
        private readonly IWorkflowDescriptionFinder Finder;
        private readonly IRedirectToAction RedirectToAction;

        public WorkflowManager(IServiceProvider serviceProvider, IWorkflowDescriptionFinder finder, IRedirectToAction redirectToAction)
        {
            ServiceProvider = serviceProvider;
            Finder = finder;
            RedirectToAction = redirectToAction;
        }

        private class RedirectingController : Controller
        {
            public new ActionResult RedirectToAction(string actionName, string controllerName, RouteValueDictionary routeValues)
            {
                return base.RedirectToAction(actionName, controllerName, routeValues);
            }
        }

        async Task<ActionResult> IWorkflowManager.AdvanceAsync(Workflow wf)
        {
            var desc = await Finder.FindAsync(wf.WorkflowKey, wf.Version);
            var currentActivity = desc.Activities.FirstOrDefault(z => z.WorkflowActivityKey == wf.WorkflowKey);
            var chooser = (IActivityChooser)ServiceProvider.GetService(currentActivity.NextActivityChooserType);
            var nextActivityKey = chooser.GetNextActivityKey(wf, currentActivity.NextActivityChooserConfig);
            var nextActivity = desc.Activities.First(z => z.WorkflowActivityKey == nextActivityKey) ?? currentActivity;
            wf.CurrentWorkflowActivityKey = nextActivity.WorkflowActivityKey;
            if (nextActivity is WebActionWorkflowActivity)
            {
                var next = (WebActionWorkflowActivity)nextActivity;
                var c = new RedirectingController();
                var routeValues = new RouteValueDictionary(next.RouteValueByName);
                routeValues[WorkflowIdRouteValueName] = wf.WorkflowId;
                return c.RedirectToAction(next.ControllerName, next.ActionName, routeValues);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}