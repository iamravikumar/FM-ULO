using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public abstract class WorkflowActivity
    {
        [JsonProperty("key")]
        public string WorkflowActivityKey { get; set; }

        [JsonProperty("name")]
        public string ActivityName { get; set; }

        [JsonProperty("sequenceNumber")]
        public int SequenceNumber { get; set; }

        [JsonProperty("OwnerUserId")]
        public string OwnerUserId { get; set; }

        [JsonIgnore]
        public Type NextActivityChooserType
        {
            get { return Type.GetType(NextActivityChooserTypeName); }
        }

        [JsonProperty("nextActivityChooserType")]
        public string NextActivityChooserTypeName { get; set; }

        [JsonProperty("nextActivityChooserConfig")]
        public string NextActivityChooserConfig { get; set; }

        [JsonProperty("emailTemplateId")]
        public int EmailTemplateId { get; set; }

        
    }
}
