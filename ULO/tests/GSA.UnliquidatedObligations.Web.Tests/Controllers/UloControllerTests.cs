﻿using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.Caching;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Tests.Controllers
{
    [TestClass]
    public class UloControllerTests : ControllerTests
    {
        private UloController UloController;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            UloController = new UloController(WorkflowManager, ApplicationUserManager, DbContext, ComponentContext, Cache.DataCacher)
            {
                ControllerContext = ControllerContext
            };
        }

        [TestMethod]
        public async Task Details_returns_view_with_correct_model()
        {
            var view = await UloController.Details(UloId, WorkflowId) as ViewResult;
            var returnedModel = (UloViewModel)view.Model;
            Assert.AreEqual(returnedModel.CurretUnliquidatedObligation.UloId, UloId);
            Assert.AreEqual(returnedModel.WorkflowViewModel.Workflow.WorkflowId, WorkflowId);
        }

        [TestMethod]
        public async Task Details_returns_view_with_correct_Ulo_information()
        {
            var view = await UloController.Details(UloId, WorkflowId) as ViewResult;
            var returnedModel = (UloViewModel)view.Model;
            Assert.AreEqual(returnedModel.CurretUnliquidatedObligation.UloId, UloId);
        }

        [TestMethod]
        public async Task Details_returns_view_with_correct_workflow_information()
        {
            var view = await UloController.Details(UloId, WorkflowId) as ViewResult;
            var returnedModel = (UloViewModel)view.Model;
            Assert.AreEqual(returnedModel.CurretUnliquidatedObligation.UloId, UloId);
            Assert.AreEqual(returnedModel.WorkflowViewModel.Workflow.WorkflowId, WorkflowId);
            Assert.AreEqual(returnedModel.WorkflowViewModel.WorkflowDescriptionViewModel.CurrentActivity.WorkflowActivityKey, CurrentWorkflowActivityKey);
            Assert.AreEqual(returnedModel.WorkflowViewModel.WorkflowDescriptionViewModel.Activites.Count, 3);

            var currentViewModelActivity = returnedModel.WorkflowViewModel.WorkflowDescriptionViewModel.CurrentActivity;
            var advanceViewModel = returnedModel.WorkflowViewModel.AdvanceViewModel;

            Assert.AreEqual(currentViewModelActivity.WorkflowActivityKey, CurrentWorkflowActivityKey);
            Assert.AreEqual(advanceViewModel.QuestionLabel, "Do you Concur");
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

            var expectedChoices = new List<QuestionChoice>()
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
            };

            //TODO: create custom commparer to compare whole object
            //CollectionAssert.AreEquivalent(advanceViewModel.WorkflowQuestionChoices.Choices, expectedChoices, IEqualityComparer<QuestionChoice>);
            Assert.AreEqual(advanceViewModel.QuestionChoices.Count, expectedChoices.Count);
        }

        
        [TestMethod]
        public async Task Details_returns_view_with_correct_Document_information()
        {
            base.ControllerContext.HttpContext.H
            var view = await UloController.Details(UloId, WorkflowId) as ViewResult;
            var returnedModel = (UloViewModel)view.Model;
            Assert.AreEqual(returnedModel.WorkflowViewModel.DocumentsViewModel.Documents.Count, 10);
        }

        [TestMethod]
        public async Task RegionWorkflows_returns_correct_workflows()
        {
            var view = await UloController.RegionWorkflows(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null) as ViewResult;
            var returnedModel = (IEnumerable<Workflow>) view.Model;
            Assert.AreEqual(returnedModel.Count(), 7);
        }
    }
}
