using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RevolutionaryStuff.Core;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    [DataContract(Name = "QuestionChoice", Namespace = UloHelpers.WorkflowDescUrn)]
    public class QuestionChoice
    {
        public static readonly QuestionChoice[] None = new QuestionChoice[0];

        [DataMember(Name= "Value")]
        [JsonProperty("value")]
        public string Value { get; set; }

        [DataMember(Name = "Text")]
        [JsonProperty("text")]
        public string Text { get; set; }

        [DataMember(Name = "JustificationKeys")]
        [JsonProperty("justificationKeys")]
        public List<string> JustificationKeys { get; set; }

        [DataMember(Name = "DocumentTypes")]
        [JsonProperty("documentTypes")]
        public List<string> DocumentTypes { get; set; }

        [DataMember(Name = "ExpectedDateAlwaysShow")]
        [JsonProperty("expectedDateAlwaysShow")]
        public bool ExpectedDateAlwaysShow { get; set; }

        [DataMember(Name = "MostRecentNonReassignmentAnswer")]
        [JsonProperty("mostRecentNonReassignmentAnswer")]
        public string MostRecentNonReassignmentAnswer { get; set; }

        public bool IsApplicable(string documentType, string mostRecentNonReassignmentAnswer)
            =>
                (
                    MostRecentNonReassignmentAnswer == null ||
                    MostRecentNonReassignmentAnswer == mostRecentNonReassignmentAnswer ||
                    CSV.ParseLine(MostRecentNonReassignmentAnswer).Contains(mostRecentNonReassignmentAnswer)
                ) &&
                (
                    DocumentTypes == null ||
                    DocumentTypes.Count == 0 ||
                    DocumentTypes.Contains(documentType) ||
                    DocumentTypes.Contains("*")
                );

        public override string ToString()
            => $"{this.GetType().Name} value=[{this.Value}] docTypes=[{CSV.FormatLine(DocumentTypes, false)}]";
    }
}
