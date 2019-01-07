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
        public string MostRecentNonReassignmentAnswerCsv
        {
            get => MostRecentNonReassignmentAnswerCsv_p;
            set
            {
                MostRecentNonReassignmentAnswerCsv_p = value;
                MostRecentNonReassignmentAnswers = CSV.ParseLine(value) ?? Empty.StringArray;
            }
        }
        private string MostRecentNonReassignmentAnswerCsv_p;

        private ICollection<string> MostRecentNonReassignmentAnswers;

        [DataMember(Name = "MostRecentRealAnswer")]
        [JsonProperty("mostRecentRealAnswer")]
        public string MostRecentRealAnswerCsv
        {
            get => MostRecentRealAnswerCsv_p;
            set
            {
                MostRecentRealAnswerCsv_p = value;
                MostRecentRealAnswers = CSV.ParseLine(value) ?? Empty.StringArray;
            }
        }
        private string MostRecentRealAnswerCsv_p;

        private ICollection<string> MostRecentRealAnswers;

        public bool IsApplicable(string documentType, string mostRecentNonReassignmentAnswer, string mostRecentRealAnswer)
            =>
                (
                    MostRecentNonReassignmentAnswers==null ||
                    MostRecentNonReassignmentAnswers.Count == 0 ||
                    MostRecentNonReassignmentAnswers.Contains(mostRecentNonReassignmentAnswer)
                ) &&
                (
                    MostRecentRealAnswers == null ||
                    MostRecentRealAnswers.Count == 0 ||
                    MostRecentRealAnswers.Contains(mostRecentRealAnswer)
                ) &&
                (
                    DocumentTypes == null ||
                    DocumentTypes.Count == 0 ||
                    DocumentTypes.Contains(documentType) ||
                    DocumentTypes.Contains("*")
                );

        public override string ToString()
            => $"{this.GetType().Name} value=[{this.Value}] docTypes=[{CSV.FormatLine(DocumentTypes, false)}] mostRecentNonReassignmentAnswers=[{MostRecentNonReassignmentAnswerCsv}] mostRecentRealAnswers=[{MostRecentRealAnswerCsv}]";
    }
}
