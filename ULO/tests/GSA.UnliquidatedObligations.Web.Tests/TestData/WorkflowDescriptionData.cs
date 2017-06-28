using System.Collections.Generic;
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
                    OwnerUserName = "RegionReviewers",
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
                    OwnerUserName = "RegionApprovers",
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
                    OwnerUserName = "CO Reviewers",
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