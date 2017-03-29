using Newtonsoft.Json;
using System.Collections.Generic;

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

        [JsonProperty("activities")]
        public ICollection<WorkflowActivity> Activities { get; set; }
    }
}
