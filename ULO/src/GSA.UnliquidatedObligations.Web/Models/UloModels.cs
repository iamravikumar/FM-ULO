using System;
using System.Collections.Generic;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.Web.Models
{
    //TODO. May need to break these up a bit.
    public class UloWfQuestionsViewModel
    {
        public List<UloWfQuestionViewModel> Questions { get; set; }

        public UloWfQuestionsViewModel(IDictionary<string, Justification> justificationByKey, List<UnliqudatedObjectsWorkflowQuestion> questions)
        {
            Questions = new List<UloWfQuestionViewModel>();
            foreach (var question in questions)
            {
                Questions.Add(new UloWfQuestionViewModel(justificationByKey, question));
            }
        }
    }

    public class UloWfQuestionViewModel
    {
        public string Username { get; set; }
        public string Answer { get; set; }
        public string JustificationKey { get; set; }
        public string Justification { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedDate { get; set; }

        public UloWfQuestionViewModel(IDictionary<string, Justification> justificationByKey, UnliqudatedObjectsWorkflowQuestion question)
        {
            Username = question.AspNetUser.UserName;
            Answer = question.Answer;
            JustificationKey = question.JustificationKey;
            Justification = justificationByKey.FindOrDefault(question.JustificationKey??"")?.Description;
            Comments = question.Comments;
            CreatedDate = question.CreatedAtUtc.Date;
        }
    }

    public class AdvanceViewModel
    {
        public IList<QuestionChoicesViewModel> QuestionChoices { get; set; } = new List<QuestionChoicesViewModel>();

        public int UnliqudatedWorkflowQuestionsId { get; set; }

        public string QuestionLabel { get; set; }

        //[Required(ErrorMessage = "Answer is required")]
        public string Answer { get; set; }

        public string JustificationKey { get; set; }

        public string Comments { get; set; }

        public int WorkflowId { get; }

        public bool JustificationNeeded { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ExpectedDateForCompletion { get; set; }

        public bool ExpectedDateForCompletionEditable { get; set; }

        public bool ExpectedDateForCompletionNeeded { get; set; }

        public AdvanceViewModel()
        {

        }

        public AdvanceViewModel(WorkflowQuestionChoices workflowQuestionChoices, UnliqudatedObjectsWorkflowQuestion question, Workflow workflow, bool justificationNeeded = true, DateTime? expectedDateForCompletion = null, bool expectedDateForCompletionEditable = true, bool expectedDateForCompletionNeeded = true)
        {
            var workflowId = workflow.WorkflowId;
            Answer = question != null ? question.Answer : "";  
            Comments = question != null ? question.Comments : "";
            UnliqudatedWorkflowQuestionsId = question?.UnliqudatedWorkflowQuestionsId ?? 0;
            JustificationKey = question?.JustificationKey;
            WorkflowId = workflowId;
            JustificationNeeded = justificationNeeded;
            ExpectedDateForCompletion = expectedDateForCompletion;
            ExpectedDateForCompletionEditable = expectedDateForCompletionEditable;
            ExpectedDateForCompletionNeeded = expectedDateForCompletionNeeded;
            if (workflowQuestionChoices != null)
            {
                QuestionLabel = workflowQuestionChoices.QuestionLabel;
                foreach (var questionChoice in workflowQuestionChoices.WhereApplicable(workflow.UnliquidatedObligation.DocType))
                {
                    QuestionChoices.Add(new QuestionChoicesViewModel(questionChoice));
                }
            }
        }
    }

    public class QuestionChoicesViewModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public IList<string> JustificationKeys { get; set; } = new List<string>();
        public bool ExpectedDateAlwaysShow { get; set; }

        public QuestionChoicesViewModel()
        {

        }

        public QuestionChoicesViewModel(QuestionChoice questionChoice)
        {
            Text = questionChoice.Text;
            Value = questionChoice.Value;
            ExpectedDateAlwaysShow = questionChoice.ExpectedDateAlwaysShow;
            if (questionChoice.JustificationKeys != null)
            {
                foreach (var justificationKey in questionChoice.JustificationKeys)
                {
                    JustificationKeys.Add(justificationKey);
                }
            }
        }
    }

    public class DocumentsViewModel
    {
        public IList<Document> Documents { get; set; }

        public bool AllowDocumentsEdit { get; set; }

        public string DocType { get; set; }

        public DocumentsViewModel()
        { }

        public DocumentsViewModel(IList<Document> documents, bool allowDocumentsEdit, string docType)
        {
            Documents = documents;
            AllowDocumentsEdit = allowDocumentsEdit;
            DocType = docType;
        }
    }

    public class WorkflowViewModel
    {
        //TODO: set properties explicitly for Workflow
        public Workflow Workflow { get; set; }
        public UloWfQuestionsViewModel QuestionsViewModel { get; set; }
        public AdvanceViewModel AdvanceViewModel { get; set; }
        public WorkflowDescriptionViewModel WorkflowDescriptionViewModel { get; set; }
        public DocumentsViewModel DocumentsViewModel { get; set; }
        public bool RequestForReassignmentsActive { get; set; }
        public bool WorkflowAssignedToCurrentUser { get; set; }
        public WorkflowViewModel()
        {

        }
        public WorkflowViewModel(Workflow workflow, bool workflowAssignedToCurrentUser, IWorkflowDescription workflowDescription=null)
        {
            Requires.NonNull(workflow, nameof(workflow));

            Workflow = workflow;
            var questions = workflow.UnliqudatedObjectsWorkflowQuestions.Where(q => q.Pending == false).ToList();
            WorkflowAssignedToCurrentUser = workflowAssignedToCurrentUser;
            bool allowDocumentEdits = workflowAssignedToCurrentUser;
            var expectedDateForCompletion = workflow.UnliquidatedObligation.ExpectedDateForCompletion;
            if (workflowDescription != null)
            {
                QuestionsViewModel = new UloWfQuestionsViewModel(workflowDescription.GetJustificationByKey(), questions);
                WorkflowDescriptionViewModel =
                    new WorkflowDescriptionViewModel(workflowDescription.WebActionWorkflowActivities.ToList(),
                        workflow.CurrentWorkflowActivityKey);

                var unliqudatedObjectsWorkflowQuestionPending = workflow.UnliqudatedObjectsWorkflowQuestions.FirstOrDefault(q => q.Pending == true);
                if (unliqudatedObjectsWorkflowQuestionPending == null && questions.Count > 0)
                {
                    AdvanceViewModel = new AdvanceViewModel(WorkflowDescriptionViewModel.CurrentActivity.QuestionChoices, questions[questions.Count - 1], workflow, WorkflowDescriptionViewModel.CurrentActivity.JustificationNeeded, expectedDateForCompletion, WorkflowDescriptionViewModel.CurrentActivity.ExpectedDateForCompletionEditable, WorkflowDescriptionViewModel.CurrentActivity.ExpectedDateForCompletionNeeded);
                }
                else
                {
                    AdvanceViewModel = new AdvanceViewModel(WorkflowDescriptionViewModel.CurrentActivity.QuestionChoices, unliqudatedObjectsWorkflowQuestionPending, workflow, WorkflowDescriptionViewModel.CurrentActivity.JustificationNeeded, expectedDateForCompletion, WorkflowDescriptionViewModel.CurrentActivity.ExpectedDateForCompletionEditable, WorkflowDescriptionViewModel.CurrentActivity.ExpectedDateForCompletionNeeded);
                }
                allowDocumentEdits = workflowAssignedToCurrentUser && WorkflowDescriptionViewModel.CurrentActivity.AllowDocumentEdit;
            }

            RequestForReassignmentsActive = workflow.RequestForReassignments.ToList().Count > 0 &&
                                                         Workflow.RequestForReassignments.FirstOrDefault() != null &&
                                                         Workflow.RequestForReassignments.First().IsActive;


            DocumentsViewModel = new DocumentsViewModel(workflow.Documents.ToList(), allowDocumentEdits, workflow.UnliquidatedObligation.DocType);
        }
    }

    public class WorkflowDescriptionViewModel
    {
        public List<WebActionWorkflowActivity> Activites { get; set; }

        public WebActionWorkflowActivity CurrentActivity { get; set; }

        public WorkflowDescriptionViewModel()
        {

        }

        public WorkflowDescriptionViewModel(List<WebActionWorkflowActivity> activities, string currentActivityKey)
        {
            Activites = new List<WebActionWorkflowActivity>(activities.OrderBy(a => a.SequenceNumber));
            CurrentActivity = activities.FirstOrDefault(a => a.WorkflowActivityKey == currentActivityKey);
        }
    }

    public class UloViewModel
    {
        //TODO: set properties explicitly for UnliquidatedObligation
        public UnliquidatedObligation CurretUnliquidatedObligation { get; set; }
        public WorkflowViewModel WorkflowViewModel { get; set; }
        public UloViewModel()
        { }
        public UloViewModel(UnliquidatedObligation ulo, Workflow workflow, IWorkflowDescription workflowDescription, bool workflowAsignedToCurrentUser)
        {
            CurretUnliquidatedObligation = ulo;
            WorkflowViewModel = new WorkflowViewModel(workflow, workflowAsignedToCurrentUser, workflowDescription);

        }
    }

    public class FilterViewModel
    {
        public IEnumerable<Workflow> Workflows { get; set; }
        public IEnumerable<SelectListItem> DocTypes { get; set; }
        public IEnumerable<SelectListItem> Zones { get; set; }
        public IEnumerable<SelectListItem> Regions { get; set; }
        public IEnumerable<SelectListItem> BaCodes { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public FilterViewModel(IEnumerable<Workflow> workflows, IEnumerable<string> docTypes, IEnumerable<SelectListItem> zones, IEnumerable<SelectListItem> regions, IEnumerable<string> baCodes, IEnumerable<string> statuses)
        {
            Workflows = workflows;
            DocTypes = docTypes.ConvertToSelectList();
            Zones = zones;
            Regions = regions;
            BaCodes = baCodes.ConvertToSelectList();
            Statuses = statuses.ConvertToSelectList();
        }
    }

    public class EmailViewModel
    {
        public string UserName { get; set; }

        public string PDN { get; set; }

        public int UloId { get; set; }

        public string SiteUrl { get; set; }

        public int WorkflowId { get; set; }
        public EmailViewModel()
        {
            SiteUrl = Properties.Settings.Default.SiteUrl;
        }
    }

    public class FormAModel
    {
        public string Field0Value { get; set; }

        public FormAModel()
        { }

        public FormAModel(Workflow wf)
        {

        }
    }
}