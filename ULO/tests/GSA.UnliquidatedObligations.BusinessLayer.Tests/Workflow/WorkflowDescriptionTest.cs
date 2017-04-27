using System;
using System.Collections.Generic;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.BusinessLayer.Tests.Workflow
{
    [TestClass]
    public class WorkflowDescriptionTest
    {


        [TestMethod]
        public void Deserialize_returns_correct_results()
        {

            //var nextActivityConfig = JsonConvert.SerializeObject(new FieldComparisonActivityChooser.MySettings
            //{
            //    Expressions = new List<FieldComparisonActivityChooser.Expression>
            //        {
            //            new FieldComparisonActivityChooser.Expression
            //            {
            //                Code = "wfQuestion.Valid == true",
            //                WorkflowActivityKey = "B1"
            //            },
            //            new FieldComparisonActivityChooser.Expression
            //            {
            //                Code = "wfQuestion.Valid == false",
            //                WorkflowActivityKey = "B2"
            //            }
            //        }

            //});

            //List<WebActionWorkflowActivity> wfDActivities = new List<WebActionWorkflowActivity>()
            //{
            //    new WebActionWorkflowActivity
            //    {
            //        ActionName = "Ulo",
            //        ControllerName = "Ulo",
            //        NextActivityChooserConfig = nextActivityConfig,
            //        NextActivityChooserTypeName = "FieldComparisonActivityChooser",
            //        WorkflowActivityKey = "4a41abad-bac3-47fb-a8cf-5d667439d7c3",
            //        OwnerUserId = "f2860baf-a555-4834-baf3-62b929d1b6b1",
            //        EmailTemplateId = 1,
            //        QuestionChoices = new WorkflowQuestionChoices
            //        {
            //            QuestionLabel = "Valid",
            //            Choices = new Dictionary<string, string>
            //            {
            //                { "Is Valid", "Yes"},
            //                {"Not Valid", "No" }
            //            }
            //        }
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
            //        EmailTemplateId = 1,
            //        QuestionChoices = new WorkflowQuestionChoices
            //        {
            //            QuestionLabel = "Concur",
            //            Choices = new Dictionary<string, string>
            //            {
            //                { "Concur", "Yes"},
            //                { "Don't Concur", "No" }
            //            }
            //        }
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
            //        EmailTemplateId = 1,
            //        QuestionChoices = new WorkflowQuestionChoices
            //        {
            //            QuestionLabel = "Do you Approve",
            //            Choices = new Dictionary<string, string>
            //            {
            //                { "Approve", "Yes"},
            //                { "Disapprove", "No" } 
            //            }
            //        }
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
            //};

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
                                Text = "Yes",
                                Value = "Approve",
                                JustificationsEnums = yesJustificationEnums
                            },
                            new QuestionChoice()
                            {
                                Text = "No",
                                Value = "Disapprove",
                                JustificationsEnums = noJustificationEnums
                            }
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
                    WorkflowActivityKey = "B2",
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
                                Text = "Yes",
                                Value = "Concur",
                                JustificationsEnums = yesJustificationEnums
                            },
                            new QuestionChoice()
                            {
                                Text = "No",
                                Value = "Don'Concur",
                                JustificationsEnums = noJustificationEnums
                            }
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
                                Text = "Yes",
                                Value = "Concur",
                                JustificationsEnums = yesJustificationEnums
                            },
                            new QuestionChoice()
                            {
                                Text = "No",
                                Value = "Don'Concur",
                                JustificationsEnums = noJustificationEnums
                            }
                        }
                    }
                }
            };
            var d = new WorkflowDescription
            {
                WebActionWorkflowActivities = wfDActivities
            };

            var serialized = JsonConvert.SerializeObject(d);
            var deserialized = WorkflowDescription.Deserialize(serialized);
            Assert.IsInstanceOfType(deserialized, typeof(WorkflowDescription));

        }
    }
}
