using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSA.UnliquidatedObligations.BusinessLayer.Helpers;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    [DataContract(Name = "WorkflowDescription", Namespace = UloHelpers.WorkflowDescUrn)]
    internal class WorkflowDescription : IWorkflowDescription
    {

        private static readonly DataContractSerializer Serializer = new DataContractSerializer(typeof(WorkflowDescription), new [] {typeof(WebActionWorkflowActivity), typeof(WorkflowQuestionChoices), typeof(QuestionChoice) });
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public virtual string ToXml()
        {
            return Serializer.WriteObject(this);
        }

        public static WorkflowDescription Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<WorkflowDescription>(json);
        }

        public static WorkflowDescription DeserializeFromXml(string xml)
        {
            return (WorkflowDescription)Serializer.ReadObject(xml);
        }

        [JsonIgnore]
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
        [JsonProperty("webActionActivities")]
        public ICollection<WebActionWorkflowActivity> WebActionWorkflowActivities { get; set; }


    }
}
