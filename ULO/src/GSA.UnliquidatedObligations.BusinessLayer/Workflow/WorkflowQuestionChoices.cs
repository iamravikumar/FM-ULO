using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public class WorkflowQuestionChoices
    {
        [JsonProperty("label")]
        public string QuestionLabel { get; set; }
        [JsonProperty("choices")]
        public Dictionary<string, string> Choices { get; set; }
    }
}