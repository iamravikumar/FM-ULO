using System;
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
        public List<UnliqudatedObjectsWorkflowQuestion> Questions { get; set; }

        public UloWfQuestionsViewModel(List<UnliqudatedObjectsWorkflowQuestion> questions)
        {
            Questions = questions;
        }
    }

    public class AdvanceViewModel
    {

        public WorkflowQuestionChoices WorkflowQuestionChoices { get; set; }

        public string Answer { get; set; }

        //TODO, will eventually be from database
        public Dictionary<string, string> JustificationChoices { get; }

        public string Justification { get; set; }
        public string Comments { get; set; }

        public int WorkflowId { get; }

        public AdvanceViewModel()
        {
            
        }

        public AdvanceViewModel(WorkflowQuestionChoices workflowQuestionChoices, int workflowId)
        {
            WorkflowQuestionChoices = workflowQuestionChoices;
            JustificationChoices = new Dictionary<string, string>()
            {
                {"The Contract is not complete and work is on-going", "The Contract is not complete and work is on-going"},
                {"The service period has not Expired", "The service period has not Expired"},
                {"Contractor has filed claim against GSA", "Contractor has filed claim against GSA" },
                {"Waiting on release of claims from vendor", "Waiting on release of claims from vendor" },
                {"There has been no recent activity but a notice to proceed will be issued", "There has been no recent activity but a notice to proceed will be issued" },
                {"Item is invalid and Contracting Officer is working on modification to deobligate the balance", "Item is invalid and Contracting Officer is working on modification to deobligate the balance" },
                {"Invalid - Recurring Contract - $ not needed - will adjust accordingly" , "Invalid - Recurring Contract - $ not needed - will adjust accordingly" },
                {"Valid - Recurring Contract - $ needed", "Valid - Recurring Contract - $ needed" },
                {"Other", "Other" }
            };
            WorkflowId = workflowId;
        }
    }

    public class WorkflowViewModel
    {
        //TODO: set properties explicitly for Workflow
        public Workflow Workflow { get; set; }
        public UloWfQuestionsViewModel QuestionsViewModel { get; set; }
        public AdvanceViewModel AdvanceViewModel { get; set; }
        public WorkflowDescriptionViewModel WorkflowDescriptionViewModel { get; set; }

        public WorkflowViewModel()
        {
           
        }
        public WorkflowViewModel(Workflow workflow, IWorkflowDescription workflowDecription)
        {
            Workflow = workflow;
            QuestionsViewModel =  new UloWfQuestionsViewModel(workflow.UnliqudatedObjectsWorkflowQuestions.ToList());
            WorkflowDescriptionViewModel = new WorkflowDescriptionViewModel(workflowDecription.WebActionWorkflowActivities.ToList(), workflow.CurrentWorkflowActivityKey);
            AdvanceViewModel = new AdvanceViewModel(WorkflowDescriptionViewModel.CurrentActivity.QuestionChoices, workflow.WorkflowId);
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


        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public decimal? UDOShouldBe { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public decimal? DOShouldBe {get; set; }

        public UloViewModel()
        { }


        public UloViewModel(UnliquidatedObligation ulo, Workflow workflow, IWorkflowDescription workflowDecription) { 
            UDOShouldBe = ulo.UDOShouldBe;
            DOShouldBe = ulo.DOShouldBe;
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