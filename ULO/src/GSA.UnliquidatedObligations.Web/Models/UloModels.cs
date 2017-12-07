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
    public class UloWfQuestionsViewModel
    {
        public List<UloWfQuestionViewModel> Questions { get; set; }

        public UloWfQuestionsViewModel()
        { }

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

        public UloWfQuestionViewModel()
        { }

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

        [MaxLength(100)]
        public string Answer { get; set; }

        public string JustificationKey { get; set; }

        [MaxLength(4000)]
        public string Comments { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ExpectedDateForCompletion { get; set; }

        public bool ExpectedDateForCompletionEditable { get; set; }

        public string MostRecentJustificationKey { get; set; }

        public AdvanceViewModel()
        {

        }

        public AdvanceViewModel(WorkflowQuestionChoices workflowQuestionChoices, UnliqudatedObjectsWorkflowQuestion question, Workflow workflow, DateTime? expectedDateForCompletion, bool expectedDateForCompletionEditable, string mostRecentJustificationKey)
        {
            var workflowId = workflow.WorkflowId;
            Answer = question != null ? question.Answer : "";  
            Comments = question != null ? question.Comments : "";
            UnliqudatedWorkflowQuestionsId = question?.UnliqudatedWorkflowQuestionsId ?? 0;
            JustificationKey = question?.JustificationKey;
            ExpectedDateForCompletion = expectedDateForCompletion;
            ExpectedDateForCompletionEditable = expectedDateForCompletionEditable;
            if (workflowQuestionChoices != null)
            {
                QuestionLabel = workflowQuestionChoices.QuestionLabel;
                foreach (var questionChoice in workflowQuestionChoices.WhereMostApplicable(workflow.UnliquidatedObligation.DocType, workflow.MostRecentNonReassignmentAnswer))
                {
                    QuestionChoices.Add(new QuestionChoicesViewModel(questionChoice));
                }
            }
            MostRecentJustificationKey = mostRecentJustificationKey;
        }
    }

    public class QuestionChoicesViewModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public IList<string> JustificationKeys { get; set; } = new List<string>();
        public bool ExpectedDateAlwaysShow { get; set; }
        public string MostRecentNonReassignmentAnswer { get; set; }

        public QuestionChoicesViewModel()
        {

        }

        public QuestionChoicesViewModel(QuestionChoice questionChoice)
        {
            Text = questionChoice.Text;
            Value = questionChoice.Value;
            ExpectedDateAlwaysShow = questionChoice.ExpectedDateAlwaysShow;
            MostRecentNonReassignmentAnswer = questionChoice.MostRecentNonReassignmentAnswer;
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

        public DocumentsViewModel Subset(Document document)
            => new DocumentsViewModel
            {
                Documents = new List<Document> { document },
                AllowDocumentsEdit = this.AllowDocumentsEdit,
                DocType = this.DocType
            };
    }

    public class WorkflowViewModel
    {
        //TODO: set properties explicitly for Workflow
        public Workflow Workflow { get; set; }
        public UloWfQuestionsViewModel QuestionsViewModel { get; set; }
        public AdvanceViewModel AdvanceViewModel { get; set; }
        public WorkflowDescriptionViewModel WorkflowDescriptionViewModel { get; set; }
        public DocumentsViewModel DocumentsViewModel { get; set; }
        public bool WorkflowAssignedToCurrentUser { get; set; }
        public RequestForReassignment RequestForReassignment { get; set; }
        public bool IsRequestForReassignmentsActive
            => RequestForReassignment != null && RequestForReassignment.IsActive;

        public WorkflowViewModel()
        { }

        public WorkflowViewModel(Workflow workflow, bool workflowAssignedToCurrentUser, IWorkflowDescription workflowDescription=null)
        {
            Requires.NonNull(workflow, nameof(workflow));

            Workflow = workflow;
            var questions = workflow.UnliqudatedObjectsWorkflowQuestions.OrderBy(q=>q.UnliqudatedWorkflowQuestionsId).ToList();
            WorkflowAssignedToCurrentUser = workflowAssignedToCurrentUser;
            bool allowDocumentEdits = workflowAssignedToCurrentUser;
            var expectedDateForCompletion = workflow.UnliquidatedObligation.ExpectedDateForCompletion;
            if (workflowDescription != null)
            {
                QuestionsViewModel = new UloWfQuestionsViewModel(workflowDescription.GetJustificationByKey(), questions.Where(q=>!q.Pending).ToList());
                WorkflowDescriptionViewModel =
                    new WorkflowDescriptionViewModel(
                        workflow.WorkflowId,
                        workflowDescription.WebActionWorkflowActivities.ToList(),
                        workflow.CurrentWorkflowActivityKey);

                var pending = questions.Count > 0 && questions.Last().Pending ? questions.Last() : null;
                var mostRecentJustificationKey = QuestionsViewModel.Questions?.LastOrDefault(z=>!UnliqudatedObjectsWorkflowQuestion.CommonAnswers.IsAnyTypeOfReassignmentAnswer(z.Answer))?.JustificationKey;
                AdvanceViewModel = new AdvanceViewModel(WorkflowDescriptionViewModel.CurrentActivity.QuestionChoices, pending, workflow, expectedDateForCompletion, WorkflowDescriptionViewModel.CurrentActivity.ExpectedDateForCompletionEditable, mostRecentJustificationKey);
                allowDocumentEdits = workflowAssignedToCurrentUser && WorkflowDescriptionViewModel.CurrentActivity.AllowDocumentEdit;
            }
            RequestForReassignment = Workflow.GetReassignmentRequest();
            DocumentsViewModel = new DocumentsViewModel(workflow.Documents.ToList(), allowDocumentEdits, workflow.UnliquidatedObligation.DocType);
        }
    }

    public class WorkflowDescriptionViewModel
    {
        public int WorkflowId { get; set; }

        public IList<WebActionWorkflowActivity> Activites { get; set; }

        public WebActionWorkflowActivity CurrentActivity { get; set; }

        public WorkflowDescriptionViewModel()
        { }

        public WorkflowDescriptionViewModel(int workflowId, IList<WebActionWorkflowActivity> activities, string currentActivityKey)
        {
            WorkflowId = workflowId;
            Activites = new List<WebActionWorkflowActivity>(activities.OrderBy(a => a.SequenceNumber));
            CurrentActivity = activities.FirstOrDefault(a => a.WorkflowActivityKey == currentActivityKey);
        }
    }

    public class UloViewModel
    {
        public UnliquidatedObligation CurretUnliquidatedObligation { get; set; }
        public WorkflowViewModel WorkflowViewModel { get; set; }
        public IList<Workflow> OtherWorkflows { get; set; }
        public IList<GetUloSummariesByPdn_Result> Others { get; set; }
        public bool BelongsToMyUnassignmentGroup { get; set; }

        public UloViewModel()
        { }

        public UloViewModel(UnliquidatedObligation ulo, Workflow workflow, IWorkflowDescription workflowDescription, bool workflowAsignedToCurrentUser, IList<GetUloSummariesByPdn_Result> others, bool belongs)
        {
            CurretUnliquidatedObligation = ulo;
            WorkflowViewModel = new WorkflowViewModel(workflow, workflowAsignedToCurrentUser, workflowDescription);
            Others = others;
            BelongsToMyUnassignmentGroup = belongs;
        }
    }

    public class FilterViewModel
    {
        public IEnumerable<Workflow> Workflows { get; set; }
        public IEnumerable<SelectListItem> Reviews { get; set; }
        public IEnumerable<SelectListItem> DocTypes { get; set; }
        public IEnumerable<SelectListItem> Zones { get; set; }
        public IEnumerable<SelectListItem> Regions { get; set; }
        public IEnumerable<SelectListItem> BaCodes { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public IEnumerable<SelectListItem> Reasons { get; set; }
        public bool HasFilters { get; set; }

        public FilterViewModel()
        { }

        public FilterViewModel(IEnumerable<Workflow> workflows, IEnumerable<SelectListItem> docTypes, IEnumerable<SelectListItem> zones, IEnumerable<SelectListItem> regions, IEnumerable<string> baCodes, IEnumerable<string> activityNames, IEnumerable<string> statuses, IEnumerable<string> reasons, bool hasFilters)
        {
            Workflows = workflows;
            DocTypes = docTypes;
            Zones = zones;
            Regions = regions;
            BaCodes = baCodes.ConvertToSelectList();
            Statuses = statuses.ConvertToSelectList();
            Reasons = reasons.ConvertToSelectList();
            Reviews = PortalHelpers.CreateReviewSelectListItems();
            HasFilters = hasFilters;
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
        public FormAModel()
        { }

        public FormAModel(Workflow wf)
        {

        }
    }
}