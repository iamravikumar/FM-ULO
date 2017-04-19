using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.EnterpriseServices;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
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

        public bool? Valid { get; set; }

        public string Justification { get; set; }

        public int WorkflowId { get; set; }

        public AdvanceViewModel()
        {
            
        }

        public AdvanceViewModel(Workflow workflow)
        {
            Valid = null;
            Justification = "";
            WorkflowId = workflow.WorkflowId;
        }
    }

    public class WorkflowViewModel
    {
        //TODO: set properties explicitly for Workflow
        public Workflow Workflow { get; set; }
        public UloWfQuestionsViewModel QuestionsViewModel { get; set; }
        public AdvanceViewModel AdvanceViewModel { get; set; }
        public WorkflowViewModel()
        {
           
        }
        public WorkflowViewModel(Workflow workflow)
        {
            Workflow = workflow;
            QuestionsViewModel =  new UloWfQuestionsViewModel(workflow.UnliqudatedObjectsWorkflowQuestions.ToList());
            AdvanceViewModel = new AdvanceViewModel(workflow);

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


        public UloViewModel(UnliquidatedObligation ulo, Workflow workflow) { 
            UDOShouldBe = ulo.UDOShouldBe;
            DOShouldBe = ulo.DOShouldBe;
            CurretUnliquidatedObligation = ulo;
            WorkflowViewModel = new WorkflowViewModel(workflow);
            
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