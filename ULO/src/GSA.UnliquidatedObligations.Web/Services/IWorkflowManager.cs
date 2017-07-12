using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IWorkflowManager
    {
        Task<ActionResult> AdvanceAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question, bool forceAdvance = false, bool ignoreActionResult=false);
        Task<ActionResult> RequestReassignAsync(Workflow wf);
        Task<ActionResult> ReassignAsync(Workflow wf, string userId, string actionName);
        Task SaveQuestionAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question);
        Task<IWorkflowDescription> GetWorkflowDescriptionAsync(Workflow wf);
    }
}
