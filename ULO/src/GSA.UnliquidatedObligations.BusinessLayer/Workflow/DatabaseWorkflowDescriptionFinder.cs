using System.Collections.Generic;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public class DatabaseWorkflowDescriptionFinder : IWorkflowDescriptionFinder
    {
        private readonly ULODBEntities DB;

        public DatabaseWorkflowDescriptionFinder(ULODBEntities db)
        {
            DB = db;
        }

        Task<IWorkflowDescription> IWorkflowDescriptionFinder.FindAsync(string workflowDefinitionKey, int minVersion)
        {
            var z = 
                (
                from wd in DB.WorkflowDefinitions
                where wd.WorkflowKey == workflowDefinitionKey && wd.Version >= minVersion
                orderby wd.Version descending
                select wd
                ).FirstOrDefault();

            //TODO: This is just for testing:
            
            if (z != null)
            {
                var d = (IWorkflowDescription) WorkflowDescription.Deserialize(z.DescriptionJson);
                //TODO: This is just for testing:
                //var nextActivityConfig = JsonConvert.SerializeObject(new FieldComparisonActivityChooser.MySettings
                //{
                //    Expressions = new List<FieldComparisonActivityChooser.Expression>
                //    {
                //        new FieldComparisonActivityChooser.Expression
                //        {
                //            Code = "wfQuestion.Valid == true",
                //            WorkflowActivityKey = "A2"
                //        },
                //        new FieldComparisonActivityChooser.Expression
                //        {
                //            Code = "wfQuestion.Valid == false",
                //            WorkflowActivityKey = "A3"
                //        }
                //    }

                //});

                //var nextActivityConfig = JsonConvert.SerializeObject(new FieldComparisonActivityChooser.MySettings
                //{
                //    Expressions = new List<FieldComparisonActivityChooser.Expression>
                //    {
                //        new FieldComparisonActivityChooser.Expression
                //        {
                //            Code = "wfQuestion.Valid == true",
                //            WorkflowActivityKey = "B1"
                //        },
                //        new FieldComparisonActivityChooser.Expression
                //        {
                //            Code = "wfQuestion.Valid == false",
                //            WorkflowActivityKey = "B2"
                //        }
                //    }

                //});

                //List<WorkflowActivity> wfDActivities = new List<WorkflowActivity>()
                //{
                //         new WebActionWorkflowActivity
                //        {
                //            ActionName = "Index",
                //            ControllerName = "Ulo",
                //            NextActivityChooserConfig = nextActivityConfig,
                //            NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                //            WorkflowActivityKey = "9a9c50c5-ae82-40be-89dc-e9676cf731fb",
                //            OwnerUserId = "ff9f2cfe-5523-4844-bc97-f705f40de2f5",
                //            RouteValueByName = new Dictionary<string, object>(),
                //            EmailTemplateId = 1
                //        },
                //        new WebActionWorkflowActivity
                //        {
                //            ActionName = "Index",
                //            ControllerName = "Ulo",
                //            NextActivityChooserConfig = nextActivityConfig,
                //            NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                //            WorkflowActivityKey = "B1",
                //            OwnerUserId = "9a9c50c5-ae82-40be-89dc-e9676cf731fb",
                //            RouteValueByName = new Dictionary<string, object>(),
                //            EmailTemplateId = 1
                //        },
                //         new WebActionWorkflowActivity
                //        {
                //            ActionName = "Index",
                //            ControllerName = "Ulo",
                //            NextActivityChooserConfig = "",
                //            NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                //            WorkflowActivityKey = "B2",
                //            OwnerUserId = "00188258-4467-484f-8c59-8e0da3e991f1",
                //            RouteValueByName = new Dictionary<string, object>(),
                //            EmailTemplateId = 1
                //        },

                //};

                //List<WorkflowActivity> wfDActivities =  new List<WorkflowActivity>()
                //{
                //    new WebActionWorkflowActivity
                //    {
                //        ActionName = "Ulo",
                //        ControllerName = "Ulo",
                //        NextActivityChooserConfig = nextActivityConfig,
                //        NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                //        WorkflowActivityKey = "4a41abad-bac3-47fb-a8cf-5d667439d7c3",     
                //        OwnerUserId = "f2860baf-a555-4834-baf3-62b929d1b6b1",
                //        EmailTemplateId = 1
                //    },
                //    new WebActionWorkflowActivity
                //    {
                //        ActionName = "Index",
                //        ControllerName = "Ulo",
                //        NextActivityChooserConfig = "",
                //        NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                //        WorkflowActivityKey = "A2",
                //        OwnerUserId = "8a59d021-b45f-4c2e-bc0f-3b59938e47b0",
                //        RouteValueByName = new Dictionary<string, object>(),
                //        EmailTemplateId = 1
                //    },
                //     new WebActionWorkflowActivity
                //    {
                //        ActionName = "Index",
                //        ControllerName = "Ulo",
                //        NextActivityChooserConfig = "",
                //        NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                //        WorkflowActivityKey = "A3",
                //        OwnerUserId = "00fcab74-9b2a-43f7-b77d-686fc3064dd0",
                //        RouteValueByName = new Dictionary<string, object>(),
                //        EmailTemplateId = 1
                //    },
                //    new WebActionWorkflowActivity
                //    {
                //        ActionName = "Index",
                //        ControllerName = "Ulo",
                //        NextActivityChooserConfig = "",
                //        NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                //        WorkflowActivityKey = "B1",
                //        OwnerUserId = "9a9c50c5-ae82-40be-89dc-e9676cf731fb",
                //        RouteValueByName = new Dictionary<string, object>(),
                //        EmailTemplateId = 1
                //    }
                ////};
                //var d = new WorkflowDescription
                //{
                //    Activities = wfDActivities
                //};
                return Task.FromResult((IWorkflowDescription)d);
            }

            return Task.FromResult((IWorkflowDescription)null);
        }

    }
}
