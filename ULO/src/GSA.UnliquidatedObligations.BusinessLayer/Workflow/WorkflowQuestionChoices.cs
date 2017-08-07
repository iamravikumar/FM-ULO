using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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

        public IEnumerable<QuestionChoice> WhereApplicable(string docType)
            => Choices.Where(z => z.IsApplicable(docType));
    }
}