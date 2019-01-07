using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    [DataContract(Name= "WebActionWorkflowActivity", Namespace = UloHelpers.WorkflowDescUrn)]
    public class WebActionWorkflowActivity : WorkflowActivity
    {
        [DataMember(Name = "ControllerName")]
        public string ControllerName { get; set; }

        [DataMember(Name= "ActionName")]
        public string ActionName { get; set; }

        [DataMember(Name = "RouteValueByName")]
        public IDictionary<string, object> RouteValueByName { get; set; }
        
        [DataMember(Name="QuestionChoices")]
        public WorkflowQuestionChoices QuestionChoices { get; set; }
    }
}
