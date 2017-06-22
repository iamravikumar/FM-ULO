using System.Collections.Generic;
using System.Runtime.Serialization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    [DataContract(Name="WorkflowChoices", Namespace= UloHelpers.WorkflowDescUrn)]
    public class WorkflowQuestionChoices
    {
        [DataMember(Name = "Label")]
        [JsonProperty("label")]
        public string QuestionLabel { get; set; }

        [DataMember(Name = "Choices")]
        [JsonProperty("choices")]
        public List<QuestionChoice> Choices { get; set; }

        [DataMember(Name ="DefaultJustificationEnums")]
        [JsonProperty("defaultJustificationEnums")]
        public List<JustificationEnum> DefaultJustificationEnums { get; set; }
    }
}