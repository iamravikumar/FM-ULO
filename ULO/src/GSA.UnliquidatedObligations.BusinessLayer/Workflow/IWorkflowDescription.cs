using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public interface IWorkflowDescription
    {
        ICollection<WorkflowActivity> Activities { get; set; }
    }
}
