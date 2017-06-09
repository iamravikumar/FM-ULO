using System.Collections.Generic;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    [DataContract(Name = "QuestionChoice", Namespace = UloHelpers.WorkflowDescUrn)]
    public class QuestionChoice
    {
        [DataMember(Name= "Value")]
        [JsonProperty("value")]
        public string Value { get; set; }
        [DataMember(Name = "Text")]
        [JsonProperty("text")]
        public string Text { get; set; }
        [DataMember(Name = "Justifications")]
        [JsonProperty("justifications")]
        public List<JustificationEnum> JustificationsEnums { get; set; }
    }
}