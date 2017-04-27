using System.Collections.Generic;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public class WorkflowQuestionChoices
    {
        [JsonProperty("label")]
        public string QuestionLabel { get; set; }
        [JsonProperty("choices")]
        public List<QuestionChoice> Choices { get; set; }
    }
}