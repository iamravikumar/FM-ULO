using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IWorkflowManager
    {
        Task<ActionResult> AdvanceAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question, IList<string> submitterGroupNames=null, bool forceAdvance = false, bool ignoreActionResult=false, bool sendNotifications=true);
        Task<ActionResult> RequestReassignAsync(Workflow wf);
        Task<ActionResult> ReassignAsync(Workflow wf, string userId, string actionName);
        Task<IWorkflowDescription> GetWorkflowDescriptionAsync(Workflow wf);
    }
}
