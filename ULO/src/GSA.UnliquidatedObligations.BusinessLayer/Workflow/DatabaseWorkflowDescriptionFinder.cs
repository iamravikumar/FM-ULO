using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public class DatabaseWorkflowDescriptionFinder : IWorkflowDescriptionFinder
    {
        private readonly ULODBEntities DB;

        public DatabaseWorkflowDescriptionFinder(ULODBEntities db)
        {
            DB = db;
        }

        Task<IWorkflowDescription> IWorkflowDescriptionFinder.FindAsync(string workflowDefinitionKey, int minVersion)
        {
            var z =
                (
                from wd in DB.WorkflowDefinitions
                where wd.WorkflowKey == workflowDefinitionKey && wd.Version >= minVersion
                orderby wd.Version descending
                select wd
                ).FirstOrDefault();

            IWorkflowDescription ret = null;

            if (z != null)
            {
                ret = (IWorkflowDescription)WorkflowDescription.DeserializeFromXml(z.DescriptionXml);
            }

            return Task.FromResult(ret);
        }
    }
}
