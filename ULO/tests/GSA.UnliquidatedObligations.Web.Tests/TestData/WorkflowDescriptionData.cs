using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class WorkflowDescriptionData
    {
        public static string GenerateData(string currenWorkflowActivityKey)
        {
            var nextActivityConfig = JsonConvert.SerializeObject(new FieldComparisonActivityChooser.MySettings
            {
                Expressions = new List<FieldComparisonActivityChooser.Expression>
                    {
                        new FieldComparisonActivityChooser.Expression
                        {
                            Code = "wf.CurrentWorkflowActivityKey == \"B2\" && wfQuestion.Answer == \"NotConcur\"",
                            WorkflowActivityKey = "B1"
                        },
                        new FieldComparisonActivityChooser.Expression
                        {
                            Code = "wf.CurrentWorkflowActivityKey == \"B1\" && wfQuestion.Answer == \"Approve\"",
                            WorkflowActivityKey = "B2"
                        },

                        new FieldComparisonActivityChooser.Expression
                        {
                            Code = "wf.CurrentWorkflowActivityKey == \"B2\" && wfQuestion.Answer == \"Concur\"",
                            WorkflowActivityKey = "B3"
                        }
                    }

            });

            var yesJustificationEnums = new List<JustificationEnum>()
            {
                JustificationEnum.ContractNotComplete,
                JustificationEnum.ServicePeriodNotExpired,
                JustificationEnum.ContractorFiledClaim,
                JustificationEnum.WatingOnRelease,
                JustificationEnum.NoRecentActivity,
                JustificationEnum.ValidRecurringContract,
                JustificationEnum.Other
            };
            var noJustificationEnums = new List<JustificationEnum>()
            {
                JustificationEnum.ItemInvalid,
                JustificationEnum.InvalidRecurringContract,
                JustificationEnum.Other
            };

            List<WebActionWorkflowActivity> wfDActivities = new List<WebActionWorkflowActivity>()
            {
                new WebActionWorkflowActivity
                {
                    ActionName = "Index",
                    ActivityName = "Region Review",
                    SequenceNumber = 1,
                    ControllerName = "Ulo",
                    NextActivityChooserConfig = nextActivityConfig,
                    NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                    WorkflowActivityKey = "B1",
                    OwnerUserId = "00188258-4467-484f-8c59-8e0da3e991f1",
                    RouteValueByName = new Dictionary<string, object>(),
                    EmailTemplateId = 1,
                    QuestionChoices = new WorkflowQuestionChoices
                    {
                        QuestionLabel = "Do you Approve",
                        Choices = new List<QuestionChoice>()
                        {
                            new QuestionChoice()
                            {
                                Value = "Yes",
                                Text = "Yes",
                                JustificationsEnums = yesJustificationEnums
                            },
                            new QuestionChoice()
                            {
                                Value = "No",
                                Text = "No",
                                JustificationsEnums = noJustificationEnums
                            },
                        }
                    }
                },
                 new WebActionWorkflowActivity
                {
                    ActionName = "Index",
                    ActivityName = "Region Approval",
                    SequenceNumber = 2,
                    ControllerName = "Ulo",
                    NextActivityChooserConfig = nextActivityConfig,
                    NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                    WorkflowActivityKey = currenWorkflowActivityKey,
                    OwnerUserId = "9a9c50c5-ae82-40be-89dc-e9676cf731fb",
                    RouteValueByName = new Dictionary<string, object>(),
                    EmailTemplateId = 1,
                     QuestionChoices = new WorkflowQuestionChoices
                    {
                        QuestionLabel = "Do you Concur",
                        Choices = new List<QuestionChoice>()
                        {
                            new QuestionChoice()
                            {
                                Value = "Concur",
                                Text = "Concur",
                                JustificationsEnums = yesJustificationEnums
                            },
                            new QuestionChoice()
                            {
                                Value = "Don't concur",
                                Text = "Don't Concur",
                                JustificationsEnums = noJustificationEnums
                            },
                        }
                    }
                },
                    new WebActionWorkflowActivity
                {
                    ActionName = "Index",
                    ActivityName = "CO Review",
                    SequenceNumber = 3,
                    ControllerName = "Ulo",
                    NextActivityChooserConfig = nextActivityConfig,
                    NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                    WorkflowActivityKey = "B3",
                    OwnerUserId = "f2860baf-a555-4834-baf3-62b929d1b6b1",
                    RouteValueByName = new Dictionary<string, object>(),
                    EmailTemplateId = 1,
                     QuestionChoices = new WorkflowQuestionChoices
                    {
                        QuestionLabel = "Do you Concur",
                        Choices = new List<QuestionChoice>()
                        {
                            new QuestionChoice()
                            {
                                Value = "Concur",
                                Text = "Concur",
                                JustificationsEnums = yesJustificationEnums
                            },
                            new QuestionChoice()
                            {
                                Value = "Don't concur",
                                Text = "Don't Concur",
                                JustificationsEnums = noJustificationEnums
                            },
                        }
                    }
                }
            };
            var d = new WorkflowDescription
            {
                WebActionWorkflowActivities = wfDActivities
            };

            return JsonConvert.SerializeObject(d);


        }
    }
}