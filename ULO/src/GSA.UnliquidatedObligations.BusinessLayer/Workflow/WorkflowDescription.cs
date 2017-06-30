using System.Collections.Generic;
using System.Runtime.Serialization;
using GSA.UnliquidatedObligations.BusinessLayer.Helpers;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    [DataContract(Name = "WorkflowDescription", Namespace = UloHelpers.WorkflowDescUrn)]
    public class WorkflowDescription : IWorkflowDescription
    {
        private static readonly DataContractSerializer Serializer = new DataContractSerializer(typeof(WorkflowDescription), new [] {typeof(WebActionWorkflowActivity), typeof(WorkflowQuestionChoices), typeof(QuestionChoice) });

        public virtual string ToXml()
        {
            return Serializer.WriteObject(this);
        }

        public static WorkflowDescription DeserializeFromXml(string xml)
        {
            return (WorkflowDescription)Serializer.ReadObject(xml);
        }

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
    }
}
