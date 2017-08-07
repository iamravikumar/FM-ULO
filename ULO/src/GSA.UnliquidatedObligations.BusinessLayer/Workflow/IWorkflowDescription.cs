using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public interface IWorkflowDescription
    {
        string InitialActivityKey { get; }
        IEnumerable<WorkflowActivity> Activities { get; }
        ICollection<WebActionWorkflowActivity> WebActionWorkflowActivities { get; set; }
        ICollection<Justification> Justifications { get; }
        ICollection<string> ResassignmentJustificationKeys { get; }
    }
}
