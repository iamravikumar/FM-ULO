using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public interface IWorkflowDescriptionFinder
    {
        Task<IWorkflowDescription> FindAsync(string workflowDefinitionKey, int minVersion = 0);

    }
}
