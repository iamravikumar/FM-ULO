using RevolutionaryStuff.Core;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    [DataContract(Name = "WorkflowDescription", Namespace = UloHelpers.WorkflowDescUrn)]
    public class WorkflowDescription : IWorkflowDescription
    {
        private static readonly DataContractSerializer Serializer = new DataContractSerializer(typeof(WorkflowDescription), new [] {typeof(WebActionWorkflowActivity), typeof(WorkflowQuestionChoices), typeof(QuestionChoice) });

        public virtual string ToXml()
            => Serializer.WriteObjectToString(this);

        internal static WorkflowDescription DeserializeFromXml(string xml)
            => Serializer.ReadObjectFromString<WorkflowDescription>(xml);

        public IEnumerable<WorkflowActivity> Activities
        {
            get
            {
                foreach (var wf in WebActionWorkflowActivities)
                {
                    yield return wf;
                }
            }
        }

        [DataMember(Name = "InitialActivityKey")]
        public string InitialActivityKey { get; set; }

        [DataMember(Name= "WebActionActivities")]
        public ICollection<WebActionWorkflowActivity> WebActionWorkflowActivities { get; set; }

        [DataMember(Name = "Justifications")]
        public ICollection<Justification> Justifications { get; set; }

        [DataMember(Name = "ResassignmentJustificationKeys")]
        public ICollection<string> ResassignmentJustificationKeys { get; set; }
    }
}
