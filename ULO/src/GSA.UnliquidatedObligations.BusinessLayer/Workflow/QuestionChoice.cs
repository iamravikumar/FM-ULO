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

        [DataMember(Name = "JustificationKeys")]
        public List<string> JustificationKeys { get; set; }

        [DataMember(Name = "DocumentTypes")]
        [JsonProperty("documentTypes")]
        public List<string> DocumentTypes { get; set; }

        public bool IsApplicable(string documentType)
            =>
                DocumentTypes == null ||
                DocumentTypes.Count == 0 ||
                DocumentTypes.Contains(documentType) ||
                DocumentTypes.Contains("*");
    }
}
