using System.Threading.Tasks;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IWorkflowManager
    {
        Task<ActionResult> AdvanceAsync(BusinessLayer.Data.Workflow wf, BusinessLayer.Data.UnliqudatedObjectsWorkflowQuestion question);
        Task<ActionResult> RequestReassign(BusinessLayer.Data.Workflow wf);
        Task<ActionResult> Reassign(BusinessLayer.Data.Workflow wf, string userId);

        Task<IWorkflowDescription> GetWorkflowDescription(BusinessLayer.Data.Workflow wf);
    }
}
