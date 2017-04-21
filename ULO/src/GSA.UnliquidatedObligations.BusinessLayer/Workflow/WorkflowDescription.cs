using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{

    internal class WorkflowDescription : IWorkflowDescription
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static WorkflowDescription Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<WorkflowDescription>(json);
        }

        public async Task<WebActionWorkflowActivity> GetWebActivityById(string workflowActivityKey)
        {
            return await WebActionWorkflowActivities
                    .FirstOrDefaultAsync(waf => waf.WorkflowActivityKey == workflowActivityKey);
        }

        [JsonIgnore]
        public IEnumerable<WorkflowActivity> Activities
        {
            get
            {
                foreach (var wf in WebActionWorkflowActivities)
                {
                    yield return wf;
                }
            }
        }

        [JsonProperty("webActionActivities")]
        public ICollection<WebActionWorkflowActivity> WebActionWorkflowActivities { get; set; }
    }
}
