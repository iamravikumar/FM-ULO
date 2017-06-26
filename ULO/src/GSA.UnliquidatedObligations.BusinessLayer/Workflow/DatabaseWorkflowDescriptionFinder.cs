using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Utility.Caching;
using System.Collections.Generic;
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

        private readonly IDictionary<string, IWorkflowDescription> FinderCache = new Dictionary<string, IWorkflowDescription>();

        Task<IWorkflowDescription> IWorkflowDescriptionFinder.FindAsync(string workflowDefinitionKey, int minVersion)
        {
            IWorkflowDescription ret = null;
            var cacheKey = Cache.CreateKey(workflowDefinitionKey, minVersion);
            if (!FinderCache.TryGetValue(cacheKey, out ret))
            {
                var z =
                    (
                    from wd in DB.WorkflowDefinitions
                    where wd.WorkflowKey == workflowDefinitionKey && wd.Version >= minVersion
                    orderby wd.Version descending
                    select wd
                    ).FirstOrDefault();

                if (z != null)
                {
                    ret = (IWorkflowDescription)WorkflowDescription.DeserializeFromXml(z.DescriptionXml);
                }
                FinderCache[cacheKey] = ret;
            }
            return Task.FromResult(ret);
        }
    }
}
