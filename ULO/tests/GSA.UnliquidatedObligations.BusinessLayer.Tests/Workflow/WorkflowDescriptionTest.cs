using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            var allJustificationEnumsArr = (JustificationEnum[])Enum.GetValues(typeof(JustificationEnum));

            var allJustificationEnumsList = new List<JustificationEnum>();
            allJustificationEnumsList.AddRange(allJustificationEnumsArr);
            allJustificationEnumsList =
                 allJustificationEnumsList.Where(
                    j => j != JustificationEnum.ReassignVaction && j != JustificationEnum.ReassignNeedHelp).ToList();


            var nextActivityConfig = JsonConvert.SerializeObject(new FieldComparisonActivityChooser.MySettings
            {
                Expressions = new List<FieldComparisonActivityChooser.Expression>
                    {
                        new FieldComparisonActivityChooser.Expression
                        {
                            Code = "wf.CurrentWorkflowActivityKey == \"B1\"",
                            WorkflowActivityKey = "B2"
                        },
                        new FieldComparisonActivityChooser.Expression
                        {
                            Code = "wf.CurrentWorkflowActivityKey == \"B2\" && wfQuestion.Answer == \"Disapprove\"",
                            WorkflowActivityKey = "B1"
                        },
                        new FieldComparisonActivityChooser.Expression
                        {
                            Code = "wf.CurrentWorkflowActivityKey == \"B2\" && wfQuestion.Answer == \"Approve\"",
                            WorkflowActivityKey = "B3"
                        },
                        new FieldComparisonActivityChooser.Expression
                        {
                            Code = "wf.CurrentWorkflowActivityKey == \"B3\" && wfQuestion.Answer == \"Not Concur\"",
                            WorkflowActivityKey = "B2"
                        },
                        new FieldComparisonActivityChooser.Expression
                        {
                            Code = "wf.CurrentWorkflowActivityKey == \"B3\" && wfQuestion.Answer == \"Concur\"",
                            WorkflowActivityKey = "B4"
                        },
                        new FieldComparisonActivityChooser.Expression
                        {
                            Code = "wf.CurrentWorkflowActivityKey == \"B4\" && wfQuestion.Answer == \"Not Concur\"",
                            WorkflowActivityKey = "B3"
                        },
                         new FieldComparisonActivityChooser.Expression
                        {
                            Code = "wf.CurrentWorkflowActivityKey == \"B4\" && wfQuestion.Answer == \"Concur\"",
                            WorkflowActivityKey = "B5"
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
                    DueIn = new TimeSpan(20, 0, 0, 0),
                    NextActivityChooserConfig = nextActivityConfig,
                    NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                    WorkflowActivityKey = "B1",
                    OwnerUserName = "RegionReviewers",
                    JustificationNeeded = true,
                    AllowDocumentEdit = true,
                    RouteValueByName = new Dictionary<string, object>(),
                    EmailTemplateId = 1,
                    QuestionChoices = new WorkflowQuestionChoices
                    {
                        QuestionLabel = "Is this Valid?",
                        DefaultJustificationEnums = null,
                        Choices = new List<QuestionChoice>()
                        {
                            new QuestionChoice()
                            {
                                Text = "Yes",
                                Value = "Valid",
                                JustificationsEnums = yesJustificationEnums
                            },
                            new QuestionChoice()
                            {
                                Text = "No",
                                Value = "Invalid",
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
                    OwnerUserName = "RegionApprovers",
                    JustificationNeeded = true,
                    AllowDocumentEdit = true,
                    RouteValueByName = new Dictionary<string, object>(),
                    EmailTemplateId = 1,
                    QuestionChoices = new WorkflowQuestionChoices
                    {
                       QuestionLabel = "Do you Approve?",
                       DefaultJustificationEnums = allJustificationEnumsList,
                       Choices = new List<QuestionChoice>()
                        {
                            new QuestionChoice()
                            {
                                Text = "Yes",
                                Value = "Approve"
                            },
                            new QuestionChoice()
                            {
                                Text = "No",
                                Value = "Disapprove"
                            }
                        }
                    }
                },
                new WebActionWorkflowActivity
                {
                    ActionName = "Index",
                    ActivityName = "CO Review 1",
                    SequenceNumber = 3,
                    ControllerName = "Ulo",
                    NextActivityChooserConfig = nextActivityConfig,
                    NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                    WorkflowActivityKey = "B3",
                    OwnerUserName = "CO Reviewers",
                    AllowDocumentEdit = false,
                    JustificationNeeded = false,
                    RouteValueByName = new Dictionary<string, object>(),
                    EmailTemplateId = 1,
                    QuestionChoices = new WorkflowQuestionChoices
                    {
                       QuestionLabel = "Do you Concur",
                       DefaultJustificationEnums = allJustificationEnumsList,
                       Choices = new List<QuestionChoice>()
                        {
                            new QuestionChoice()
                            {
                                Text = "Yes",
                                Value = "Concur"
                            },
                            new QuestionChoice()
                            {
                                Text = "No",
                                Value = "Not Concur"
                            }
                        }
                    }
                },
                new WebActionWorkflowActivity
                {
                    ActionName = "Index",
                    ActivityName = "CO Review 2",
                    SequenceNumber = 4,
                    ControllerName = "Ulo",
                    NextActivityChooserConfig = nextActivityConfig,
                    NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                    WorkflowActivityKey = "B4",
                    OwnerUserName = "CO Reviewers",
                    AllowDocumentEdit = false,
                    JustificationNeeded = false,
                    RouteValueByName = new Dictionary<string, object>(),
                    EmailTemplateId = 1,
                    QuestionChoices = new WorkflowQuestionChoices
                    {
                       QuestionLabel = "Do you Concur",
                       DefaultJustificationEnums = allJustificationEnumsList,
                       Choices = new List<QuestionChoice>()
                        {
                            new QuestionChoice()
                            {
                                Text = "Yes",
                                Value = "Concur"
                            },
                            new QuestionChoice()
                            {
                                Text = "No",
                                Value = "Not Concur"
                            }
                        }
                    }
                },
                new WebActionWorkflowActivity
                {
                    ActionName = "Index",
                    ActivityName = "Complete",
                    SequenceNumber = 5,
                    ControllerName = "Ulo",
                    NextActivityChooserConfig = nextActivityConfig,
                    NextActivityChooserTypeName = "FieldComparisonActivityChooser",
                    WorkflowActivityKey = "B5",
                    OwnerUserName = "RegionApprovers",
                    JustificationNeeded = false,
                    AllowDocumentEdit = false,
                    RouteValueByName = new Dictionary<string, object>(),
                    EmailTemplateId = 1,
                    QuestionChoices = new WorkflowQuestionChoices
                    {
                       QuestionLabel = "Do you Concur",
                       DefaultJustificationEnums = allJustificationEnumsList,
                       Choices = new List<QuestionChoice>()
                        {
                            new QuestionChoice()
                            {
                                Text = "Yes",
                                Value = "Concur"
                            },
                            new QuestionChoice()
                            {
                                Text = "No",
                                Value = "Not Concur"
                            }
                        }
                    }
                }
            };

            var d = new WorkflowDescription
            {
                WebActionWorkflowActivities = wfDActivities
            };

            var serialized = d.ToXml();
            var deserialized = WorkflowDescription.DeserializeFromXml(serialized);
            var xml = d.ToXml();
            Trace.WriteLine(xml);
            Assert.IsInstanceOfType(deserialized, typeof(WorkflowDescription));

        }
    }
}
