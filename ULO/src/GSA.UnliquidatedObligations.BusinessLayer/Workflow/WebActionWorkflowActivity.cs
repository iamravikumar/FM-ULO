using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    [DataContract(Name= "WebActionWorkflowActivity", Namespace = UloHelpers.WorkflowDescUrn)]
    public class WebActionWorkflowActivity : WorkflowActivity
    {
        [DataMember(Name = "ControllerName")]
        [JsonProperty("controllerName")]
        public string ControllerName { get; set; }

        [DataMember(Name= "ActionName")]
        [JsonProperty("actionName")]
        public string ActionName { get; set; }

        [JsonProperty("routeValueByName")]
        public IDictionary<string, object> RouteValueByName { get; set; }
        
        [DataMember(Name="QuestionChoices")]
        [JsonProperty("questionChoices")]
        public WorkflowQuestionChoices QuestionChoices { get; set; }
        
        //Just examples:
        //Concur goes forward
        //Nonconcur goes back
        
    }
}
