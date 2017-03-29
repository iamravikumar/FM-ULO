using Newtonsoft.Json;
using System;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public abstract class WorkflowActivity
    {
        [JsonProperty("key")]
        public string WorkflowActivityKey { get; set; }

        [JsonIgnore]
        public Type NextActivityChooserType
        {
            get { return Type.GetType(NextActivityChooserTypeName); }
        }

        [JsonProperty("nextActivityChooserType")]
        public string NextActivityChooserTypeName { get; set; }

        [JsonProperty("nextActivityChooserConfig")]
        public string NextActivityChooserConfig { get; set; }
    }
}
