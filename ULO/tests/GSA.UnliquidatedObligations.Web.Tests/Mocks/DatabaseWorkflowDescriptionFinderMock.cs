using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.Web.Tests.Mocks
{
    class DatabaseWorkflowDescriptionFinderMock : IWorkflowDescriptionFinder
    {
        public Task<IWorkflowDescription> FindAsync(string workflowDefinitionKey, int minVersion = 0)
        {
            var nextActivityConfig = JsonConvert.SerializeObject(new FieldComparisonActivityChooser.MySettings
            {
                Expressions = new List<FieldComparisonActivityChooser.Expression>
                    {
                        new FieldComparisonActivityChooser.Expression
                        {
                            Code = "true",
                            WorkflowActivityKey = "A2"
                        }
                    }

            });

            List<WorkflowActivity> wfDActivities = new List<WorkflowActivity>()
                {
                    new WebActionWorkflowActivity
                    {
                        ActionName = "Ulo",
                        ControllerName = "Ulo",
                        NextActivityChooserConfig = nextActivityConfig,
                        NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                        WorkflowActivityKey = "4a41abad-bac3-47fb-a8cf-5d667439d7c3",
                        OwnerUserId = "f2860baf-a555-4834-baf3-62b929d1b6b1"
                    },
                    new WebActionWorkflowActivity
                    {
                        ActionName = "Index",
                        ControllerName = "Ulo",
                        NextActivityChooserConfig = "",
                        NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                        WorkflowActivityKey = "A2",
                        OwnerUserId = "8a59d021-b45f-4c2e-bc0f-3b59938e47b0",
                        RouteValueByName = new Dictionary<string, object>()

                    }
                };
            var d = new WorkflowDescriptionMock
            {
                Activities = wfDActivities
            };
            return Task.FromResult((IWorkflowDescription)d);
        }
    }
}
