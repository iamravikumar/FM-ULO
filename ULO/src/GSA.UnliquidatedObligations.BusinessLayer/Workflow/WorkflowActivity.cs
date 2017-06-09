using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{

    [DataContract(Name= "WorkflowActivity", Namespace = UloHelpers.WorkflowDescUrn)]
    public abstract class WorkflowActivity
    {
        [DataMember(Name="Key")]
        [JsonProperty("key")]
        public string WorkflowActivityKey { get; set; }

        [DataMember(Name = "Name")]
        [JsonProperty("name")]
        public string ActivityName { get; set; }

        [DataMember(Name = "SequenceNumber")]
        [JsonProperty("sequenceNumber")]
        public int SequenceNumber { get; set; }

        [DataMember(Name = "OwnerUserId")]
        [JsonProperty("OwnerUserId")]
        public string OwnerUserId { get; set; }

        [JsonIgnore]
        public Type NextActivityChooserType
        {
            get { return Type.GetType(NextActivityChooserTypeName); }
        }

        [DataMember(Name = "NextActivityChooserType")]
        [JsonProperty("nextActivityChooserType")]
        public string NextActivityChooserTypeName { get; set; }

        [DataMember(Name = "NextActivityChooserConfig")]
        [JsonProperty("nextActivityChooserConfig")]
        public string NextActivityChooserConfig { get; set; }

        [DataMember(Name = "EmailTemplateId")]
        [JsonProperty("emailTemplateId")]
        public int EmailTemplateId { get; set; }

        
    }
}
