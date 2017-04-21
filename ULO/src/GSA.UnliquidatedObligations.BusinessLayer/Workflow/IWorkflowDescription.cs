using System.Collections.Generic;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public interface IWorkflowDescription
    {
        IEnumerable<WorkflowActivity> Activities { get; }
        ICollection<WebActionWorkflowActivity> WebActionWorkflowActivities { get; set; }

        Task<WebActionWorkflowActivity> GetWebActivityById(string workflowActivityKey);
    }
}
