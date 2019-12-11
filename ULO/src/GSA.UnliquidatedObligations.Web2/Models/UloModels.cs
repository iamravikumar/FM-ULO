using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using Microsoft.AspNetCore.Mvc.Rendering;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class WorkflowListTab
    {
        public static readonly WorkflowListTab[] None = new WorkflowListTab[0];
        public string TabName { get; set; }
        public string TabKey { get; set; }
        public int ItemCount { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsAggregateTab { get; set; }
    }

#if true //ones copied from old ulo.  need to determine current usage
    public class WorkflowedUloListingItem
    {
        public int WorkflowId { get; set; }
        public int UloId { get; set; }
    }

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
        public DateTime CreatedAtUtc { get; set; }

        public UloWfQuestionViewModel()
        { }

        public UloWfQuestionViewModel(IDictionary<string, Justification> justificationByKey, UnliqudatedObjectsWorkflowQuestion question)
        {
            Username = question.User.UserName;
           // Username = question.UserId;
            Answer = question.Answer;
            JustificationKey = question.JustificationKey;
            Justification = justificationByKey.FindOrDefault(question.JustificationKey ?? "")?.Description;
            Comments = question.Comments;
            CreatedAtUtc = question.CreatedAtUtc;
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

        public string WorkflowRowVersionString { get; set; }

        public DateTime EditingBeganAtUtc { get; set; }

        public string PDN { get; set; }
        public bool? IsValid { get; set; }

        public AdvanceViewModel()
        {

        }

        public AdvanceViewModel(WorkflowQuestionChoices workflowQuestionChoices, UnliqudatedObjectsWorkflowQuestion question, Workflow workflow, DateTime? expectedDateForCompletion, bool expectedDateForCompletionEditable)
        {
            var workflowId = workflow.WorkflowId;
            WorkflowRowVersionString = workflow.WorkflowRowVersionString;
            EditingBeganAtUtc = DateTime.UtcNow;
            Answer = question != null ? question.Answer : "";
            Comments = question != null ? question.Comments : "";
            UnliqudatedWorkflowQuestionsId = question?.UnliqudatedWorkflowQuestionsId ?? 0;
            JustificationKey = question?.JustificationKey;
            ExpectedDateForCompletion = expectedDateForCompletion;
            ExpectedDateForCompletionEditable = expectedDateForCompletionEditable;
            var ulo = workflow.TargetUlo;
            PDN = ulo.PegasysDocumentNumber;
            IsValid = ulo.Valid;
            //            workflow.WorkflowRowVersionString;
            if (workflowQuestionChoices != null)
            {
                QuestionLabel = workflowQuestionChoices.QuestionLabel;
                foreach (var questionChoice in workflowQuestionChoices.WhereMostApplicable(workflow.TargetUlo.DocType, workflow.MostRecentNonReassignmentAnswer, workflow.MostRecentRealAnswer))
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
        //        public string MostRecentNonReassignmentAnswer { get; set; }
        //        public string MostRecentRealAnswer { get; set; }

        public QuestionChoicesViewModel()
        {

        }

        public QuestionChoicesViewModel(QuestionChoice questionChoice)
        {
            Text = questionChoice.Text;
            Value = questionChoice.Value;
            ExpectedDateAlwaysShow = questionChoice.ExpectedDateAlwaysShow;
            //          MostRecentNonReassignmentAnswer = questionChoice.MostRecentNonReassignmentAnswer;
            //        MostRecentRealAnswer = questionChoice.MostRecentRealAnswer;
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

        public ICollection<Document> UniqueMissingLineageDocuments;

        public int UniqueMissingLineageDocumentCount
            => (UniqueMissingLineageDocuments ?? Document.None).Count;

        public int WorkflowId { get; set; }

        public DocumentsViewModel()
        { }

        public DocumentsViewModel(IList<Document> documents, bool allowDocumentsEdit, string docType, ICollection<Document> uniqueMissingLineageDocuments, int workflowId)
        {
            Documents = documents;
            AllowDocumentsEdit = allowDocumentsEdit;
            DocType = docType;
            WorkflowId = workflowId;
            UniqueMissingLineageDocuments = uniqueMissingLineageDocuments;
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
        public bool WorkflowAssignedToCurrentUser { get; set; }
        public RequestForReassignment RequestForReassignment { get; set; }
        public bool IsRequestForReassignmentsActive
            => RequestForReassignment != null && RequestForReassignment.IsActive;

        public WorkflowViewModel()
        { }

        public WorkflowViewModel(Workflow workflow, bool workflowAssignedToCurrentUser, IWorkflowDescription workflowDescription, ICollection<Document> uniqueMissingLineageDocuments)
        {
            Requires.NonNull(workflow, nameof(workflow));

            Workflow = workflow;
            var questions = workflow.WorkflowUnliqudatedObjectsWorkflowQuestions.OrderBy(q => q.UnliqudatedWorkflowQuestionsId).ToList();
            WorkflowAssignedToCurrentUser = workflowAssignedToCurrentUser;
            bool allowDocumentEdits = workflowAssignedToCurrentUser;
            var expectedDateForCompletion = workflow.TargetUlo.ExpectedDateForCompletion;
            if (workflowDescription != null)
            {
                QuestionsViewModel = new UloWfQuestionsViewModel(workflowDescription.GetJustificationByKey(), questions.Where(q => !q.Pending).ToList());
                WorkflowDescriptionViewModel =
                    new WorkflowDescriptionViewModel(
                        workflow.WorkflowId,
                        workflowDescription.WebActionWorkflowActivities.ToList(),
                        workflow.CurrentWorkflowActivityKey);

                var pending = questions.Count > 0 && questions.Last().Pending ? questions.Last() : null;
                AdvanceViewModel = new AdvanceViewModel(WorkflowDescriptionViewModel.CurrentActivity.QuestionChoices, pending, workflow, expectedDateForCompletion, WorkflowDescriptionViewModel.CurrentActivity.ExpectedDateForCompletionEditable);
                allowDocumentEdits = workflowAssignedToCurrentUser && WorkflowDescriptionViewModel.CurrentActivity.AllowDocumentEdit;
            }
            //throw new NotImplementedException("old code in need for porting!");

            RequestForReassignment = Workflow.WorkflowRequestForReassignments.OrderByDescending(z => z.RequestForReassignmentID).FirstOrDefault();
            DocumentsViewModel = new DocumentsViewModel(workflow.WorkflowDocuments.ToList(), allowDocumentEdits, workflow.TargetUlo.DocType, uniqueMissingLineageDocuments, workflow.WorkflowId);

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
        public IList<GetUloSummariesByPdn_Result0> Others { get; set; }
        public bool BelongsToMyUnassignmentGroup { get; set; }

        public UloViewModel()
        { }

        public UloViewModel(UnliquidatedObligation ulo, Workflow workflow, IWorkflowDescription workflowDescription, bool workflowAsignedToCurrentUser, IList<GetUloSummariesByPdn_Result0> others, ICollection<Document> uniqueMissingLineageDocuments, bool belongs)
        {
            CurretUnliquidatedObligation = ulo;
            WorkflowViewModel = new WorkflowViewModel(workflow, workflowAsignedToCurrentUser, workflowDescription, uniqueMissingLineageDocuments);
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
        public IEnumerable<SelectListItem> ActivityNames { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public IEnumerable<SelectListItem> Reasons { get; set; }
        
        public bool IsReassignable { get; set; }
        public bool HasFilters { get; set; }

        public FilterViewModel()
        { }

        public FilterViewModel(IEnumerable<Workflow> workflows, IEnumerable<SelectListItem> docTypes, IEnumerable<SelectListItem> zones, IEnumerable<SelectListItem> regions, IEnumerable<string> baCodes, IEnumerable<string> activityNames, IEnumerable<string> statuses, IEnumerable<string> reasons, IEnumerable<SelectListItem> reviews, bool hasFilters)
        {
            Workflows = workflows;
            DocTypes = docTypes;
            Zones = zones;
            Regions = regions;
            BaCodes = baCodes.CreateSelectList();
            ActivityNames=activityNames.CreateSelectList();
            Statuses = statuses.CreateSelectList();
            Reasons = reasons.CreateSelectList();
            Reviews = reviews; 
            HasFilters = hasFilters;            
        }
    }

    public abstract class BaseEmailViewModel
    {
        public string UserName { get; private set; }

        public string SiteUrl { get; private set; }

        protected BaseEmailViewModel()
        {
            throw new NotImplementedException("old code in need for porting!");
            /*
            SiteUrl = Properties.Settings.Default.SiteUrl;
            */
        }

        protected BaseEmailViewModel(string userName)
            : this()
        {
            Requires.Text(userName, nameof(userName));
            UserName = userName;
        }

        protected BaseEmailViewModel(AspNetUser u)
            : this()
        {
            Requires.NonNull(u, nameof(u));
            UserName = u.UserName;
        }
    }

    public class EmailViewModel : BaseEmailViewModel
    {
        public string PDN { get; set; }

        public int UloId { get; set; }

        public int WorkflowId { get; set; }

        public EmailViewModel(AspNetUser u)
            : base(u)
        { }

        public EmailViewModel(string userName)
            : base(userName)
        { }
    }

    public class ItemsEmailViewModel<T> : BaseEmailViewModel
    {
        public ICollection<T> Items { get; private set; }

        public int ItemCount => Items.Count;

        public ItemsEmailViewModel(AspNetUser u, ICollection<T> items)
            : base(u)
        {
            Items = items ?? new List<T>();
        }
    }

    public class WorkflowsEmailViewModel : ItemsEmailViewModel<Workflow>
    {
        public string ReviewName { get; private set; }

        public bool MultipleReviewNames { get; private set; }

        public WorkflowsEmailViewModel(AspNetUser u, ICollection<Workflow> items)
            : base(u, items)
        {
            var reviewNames = items.Select(z => z.TargetUlo.Review.ReviewName).Distinct().ToList();
            ReviewName = reviewNames.FirstOrDefault();
            MultipleReviewNames = reviewNames.Count > 1;
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

    public class UploadFilesModel
    {
        public int ReviewId { get; set; }

        public IList<string> PegasysFilePathsList { get; set; } = new List<string>();

        public IList<string> RetaFileList { get; set; } = new List<string>();

        public IList<string> EasiFileList { get; set; } = new List<string>();

        public IList<string> One92FileList { get; set; } = new List<string>();

        public IList<string> ActiveCardholderFiles { get; set; } = new List<string>();

        public IList<string> PegasysOpenItemsCreditCards { get; set; } = new List<string>();

        public IList<string> CreditCardAliasCrosswalkFiles { get; set; } = new List<string>();

        public UploadFilesModel(int reviewId)
        {
            ReviewId = reviewId;
        }
    }


#endif
}
