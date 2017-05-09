using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.EnterpriseServices;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.Web.Models
{
    //TODO. May need to break these up a bit.
    public class UloWfQuestionsViewModel
    {
        public List<UloWfQuestionViewModel> Questions { get; set; }

        public UloWfQuestionsViewModel(List<UnliqudatedObjectsWorkflowQuestion> questions)
        {
            Questions = new List<UloWfQuestionViewModel>();
            foreach (var question in questions)
            {
                Questions.Add(new UloWfQuestionViewModel(question));
            }
        }
    }

    public class UloWfQuestionViewModel
    {
        public string Username { get; set; }
        public string Answer { get; set; }
        public string Justification { get; set; }
        public string Comments { get; set; }

        public UloWfQuestionViewModel(UnliqudatedObjectsWorkflowQuestion question)
        {
            Username = question.AspNetUser.UserName;
            Answer = question.Answer;
            Justification = JustificationChoices.Choices[(JustificationEnum) question.JustificationId].JustificationText;
            Comments = question.Comments;
        }
    }

    public class AdvanceViewModel
    {

        public List<QuestionChoicesViewModel> QuestionChoices { get; set; }

        public string QuestionLabel { get; set; }

        public string Answer { get; set; }

        public int JustificationId { get; set; }
        public string Comments { get; set; }

        public int WorkflowId { get; }

        public AdvanceViewModel()
        {
            
        }


        public AdvanceViewModel(WorkflowQuestionChoices workflowQuestionChoices, int workflowId)
        {
            
            QuestionLabel = workflowQuestionChoices.QuestionLabel;
            QuestionChoices = new List<QuestionChoicesViewModel>();
            foreach (var questionChoice in workflowQuestionChoices.Choices)
            {
                QuestionChoices.Add(new QuestionChoicesViewModel(questionChoice));
            }
            WorkflowId = workflowId;
        }
    }

    public class QuestionChoicesViewModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public List<Justification> Justifications { get; set; }

        public QuestionChoicesViewModel()
        {
            
        }

        public QuestionChoicesViewModel(QuestionChoice questionChoice)
        {
            Text = questionChoice.Text;
            Value = questionChoice.Value;
            Justifications = new List<Justification>();
            foreach (var questionChoiceJustificationsEnum in questionChoice.JustificationsEnums)
            {
                Justifications.Add(JustificationChoices.Choices[questionChoiceJustificationsEnum]);
            }
        }
    }

    public class DocumentsViewModel
    {
        public List<Document> Documents { get; set; }

        public DocumentsViewModel()
        {
            
        }

        public DocumentsViewModel(List<Document> documents)
        {
            Documents = documents;
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
        public WorkflowViewModel()
        {
           
        }
        public WorkflowViewModel(Workflow workflow, IWorkflowDescription workflowDecription)
        {
            Workflow = workflow;
            QuestionsViewModel =  new UloWfQuestionsViewModel(workflow.UnliqudatedObjectsWorkflowQuestions.ToList());
            WorkflowDescriptionViewModel = new WorkflowDescriptionViewModel(workflowDecription.WebActionWorkflowActivities.ToList(), workflow.CurrentWorkflowActivityKey);
            RequestForReassignmentsActive = workflow.RequestForReassignments.ToList().Count > 0 &&
                                                         Workflow.RequestForReassignments.FirstOrDefault() != null &&
                                                         Workflow.RequestForReassignments.First().IsActive;
            AdvanceViewModel = new AdvanceViewModel(WorkflowDescriptionViewModel.CurrentActivity.QuestionChoices, workflow.WorkflowId);

            DocumentsViewModel = new DocumentsViewModel(workflow.Documents.ToList());
        }
    }

    public class WorkflowDescriptionViewModel
    {
        public List<WebActionWorkflowActivity> Activites { get; set; }

        public WebActionWorkflowActivity CurrentActivity { get; set; }

        public WorkflowDescriptionViewModel()
        {
            
        }

        public WorkflowDescriptionViewModel(List<WebActionWorkflowActivity> activities , string currentActivityKey)
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
        public UloViewModel(UnliquidatedObligation ulo, Workflow workflow, IWorkflowDescription workflowDecription) { 
            CurretUnliquidatedObligation = ulo;
            WorkflowViewModel = new WorkflowViewModel(workflow, workflowDecription);
            
        }
    }

    public class EmailViewModel
    {
        public string UserName { get; set; }

        public string PDN { get; set; }
        public EmailViewModel()
        {
            
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