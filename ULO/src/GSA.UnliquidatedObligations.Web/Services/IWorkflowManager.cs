using System.Threading.Tasks;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IWorkflowManager
    {
        Task<ActionResult> AdvanceAsync(BusinessLayer.Data.Workflow wf, BusinessLayer.Data.UnliqudatedObjectsWorkflowQuestion question);
    }
}
