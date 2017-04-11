using System;
using System.Collections.Generic;
using System.Dynamic;
using System.EnterpriseServices;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.Web.Models
{
    //TODO. May need to break these up a bit.
    public class ULOWfQuestionsViewModel
    {
        public string Person { get; set; }
        public bool? Valid { get; set; }

        public string Justification { get; set; }

        public int WorkflowId { get; set; }

        public ULOWfQuestionsViewModel(UnliqudatedObjectsWorkflowQuestion question)
        {
            Person = question.AspNetUser.UserName;
            Valid = question.Valid;
            Justification = question.Justification;
            WorkflowId = question.WorkflowId;
        }
    }

    public class WorkflowViewModel
    {
        //TODO: set properties explicitly for Workflow
        public Workflow Workflow { get; set; }
        public List<ULOWfQuestionsViewModel> QuestionsVmList;

        public WorkflowViewModel(Workflow workflow)
        {
            Workflow = workflow;
            QuestionsVmList =  new List<ULOWfQuestionsViewModel>();
            foreach (var question in workflow.UnliqudatedObjectsWorkflowQuestions)
            {
                QuestionsVmList.Add(new ULOWfQuestionsViewModel(question));
            }
           
        }
    }

    public class UloViewModel
    {
        //TODO: set properties explicitly for UnliquidatedObligation
        public UnliquidatedObligation CurretUnliquidatedObligation;
        public List<WorkflowViewModel> WorkflowVMsList;

        
        public string UDOShouldBe { get; set; }
        public string DOShouldBe {get; set; }

        public UloViewModel()
        { }


        public UloViewModel(UnliquidatedObligation ulo)
        {
            CurretUnliquidatedObligation = ulo;
            WorkflowVMsList = new List<WorkflowViewModel>();
            foreach (var workflow in ulo.Workflows)
            {
                WorkflowVMsList.Add(new WorkflowViewModel(workflow));
            }
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