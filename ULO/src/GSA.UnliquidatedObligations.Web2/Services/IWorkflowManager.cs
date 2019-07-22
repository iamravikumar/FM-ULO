using System.Collections.Generic;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IWorkflowManager
    {
        Task<IActionResult> AdvanceAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question, IList<string> submitterGroupNames = null, bool forceAdvance = false, bool ignoreActionResult = false, bool sendNotifications = true);
        Task<IActionResult> RequestReassignAsync(Workflow wf);
        Task<IActionResult> ReassignAsync(Workflow wf, string userId, string actionName);
        Task<IWorkflowDescription> GetWorkflowDescriptionAsync(Workflow wf);
    }
}
