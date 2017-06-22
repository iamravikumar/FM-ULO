using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public interface IWorkflowDescription
    {
        IEnumerable<WorkflowActivity> Activities { get; }
        ICollection<WebActionWorkflowActivity> WebActionWorkflowActivities { get; set; }
  
    }
}
