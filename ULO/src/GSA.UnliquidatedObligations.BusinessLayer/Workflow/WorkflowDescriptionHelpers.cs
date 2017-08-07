using RevolutionaryStuff.Core;
using System.Collections.Generic;
using System.Linq;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public static class WorkflowDescriptionHelpers
    {
        public static IList<Justification> GetJustifications(this IWorkflowDescription workflowDescription, IEnumerable<string> justificationKeys)
        {
            var justifications = new List<Justification>();
            if (justificationKeys != null)
            {
                var d = workflowDescription.Justifications.ToDictionary(j => j.Key);
                foreach (var k in justificationKeys)
                {
                    var justification = d.FindOrDefault(k);
                    if (justification != null)
                    {
                        justifications.Add(justification);
                    }
                }
            }
            return justifications;
        }

        public static IDictionary<string, Justification> GetJustificationByKey(this IWorkflowDescription workflowDescription)
            => workflowDescription.Justifications.ToDictionaryOnConflictKeepLast(j => j.Key, j => j);

        public static IList<Justification> GetResassignmentJustifications(this IWorkflowDescription workflowDescription)
            => workflowDescription.GetJustifications(workflowDescription.ResassignmentJustificationKeys);
    }
}
