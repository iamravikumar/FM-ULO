using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using System;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class WorkflowManager : IWorkflowManager
    {
        public const string WorkflowIdRouteValueName = "workflowId";

        //private readonly IServiceProvider ServiceProvider;
        private readonly IComponentContext ComponentContext;
        private readonly IWorkflowDescriptionFinder Finder;

        //public WorkflowManager(IServiceProvider serviceProvider, IWorkflowDescriptionFinder finder)
        //{
        //    ServiceProvider = serviceProvider;
        //    Finder = finder;
        //}

        //TODO: Make sure to run by Jason.  No longer using ServiceProvider
        public WorkflowManager(IComponentContext componentContext, IWorkflowDescriptionFinder finder)
        {
            ComponentContext = componentContext;
            Finder = finder;
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
            //TODO: Service Provider question
            //TODO: May possibly need to see how to register Icompone
            //var chooser = (IActivityChooser)ServiceProvider.GetService(currentActivity.NextActivityChooserType);
            var chooser = ComponentContext.ResolveNamed<IActivityChooser>(currentActivity.NextActivityChooserTypeName);
            var nextActivityKey = chooser.GetNextActivityKey(wf, currentActivity.NextActivityChooserConfig);

            //TODO: Handle null case which says stay where you are.
            var nextActivity = desc.Activities.First(z => z.WorkflowActivityKey == nextActivityKey) ?? currentActivity;
            wf.CurrentWorkflowActivityKey = nextActivity.WorkflowActivityKey;

            //TODO: Updata other info like the owner, date.
            if (nextActivity is WebActionWorkflowActivity)
            {
                //TODO: if owner changes, look at other ways of redirecting.
                var next = (WebActionWorkflowActivity)nextActivity;
                var c = new RedirectingController();
                var routeValues = new RouteValueDictionary(next.RouteValueByName);
                routeValues[WorkflowIdRouteValueName] = wf.WorkflowId;
                return c.RedirectToAction(next.ControllerName, next.ActionName, routeValues);
            }
            else
            {
                //TODO: handle background hangfire.
                throw new NotImplementedException();
            }
        }
    }
}