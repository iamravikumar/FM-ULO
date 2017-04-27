using System.Collections.Generic;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public class QuestionChoice
    {
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("justifications")]
        public List<JustificationEnum> JustificationsEnums { get; set; }
    }
}