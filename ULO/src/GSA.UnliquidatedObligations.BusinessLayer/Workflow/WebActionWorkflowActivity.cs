using Newtonsoft.Json;
using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public class WebActionWorkflowActivity : WorkflowActivity
    {
        [JsonProperty("controllerName")]
        public string ControllerName { get; set; }

        [JsonProperty("actionName")]
        public string ActionName { get; set; }

        [JsonProperty("routeValueByName")]
        public IDictionary<string, object> RouteValueByName { get; set; }
        //TODO Add question choices
        [JsonProperty("questionChoices")]
        public WorkflowQuestionChoices QuestionChoices { get; set; }
        
        //Just examples:
        //Concur goes forward
        //Nonconcur goes back
        
    }
}
