using Autofac;
using Hangfire;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Models;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace GSA.UnliquidatedObligations.Web.Services
{
    /// <remarks>
    /// Caching in here is present because when we're being called from Background.AssignWorkFlows thousands of times,
    /// we need to prevent unnecesary database calls and slow EntityFramework in memory Find commands.
    /// The caching is local only, as opposed to being injected as we're not worried about being called from different user threads, 
    /// just background ones, AND we're caching EF objects, so at most, this would have had to have been injected with caller scope
    /// </remarks>
    public class WorkflowManager : IWorkflowManager
    {
        public const string WorkflowIdRouteValueName = "workflowId";

        private readonly IComponentContext ComponentContext;
        private readonly IWorkflowDescriptionFinder Finder;
        private readonly IBackgroundJobClient BackgroundJobClient;
        protected readonly ULODBEntities DB;
        private readonly ICacher Cacher = new BasicCacher();

        public WorkflowManager(IComponentContext componentContext, IWorkflowDescriptionFinder finder, IBackgroundJobClient backgroundJobClient, ULODBEntities db)
        {
            ComponentContext = componentContext;
            Finder = finder;
            BackgroundJobClient = backgroundJobClient;
            DB = db;

        }

        private class RedirectingController : Controller
        {
            public new ActionResult RedirectToAction(string actionName, string controllerName, RouteValueDictionary routeValues)
            {
                return base.RedirectToAction(actionName, controllerName, routeValues);
            }
        }

        async Task<ActionResult> IWorkflowManager.AdvanceAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question, bool forceAdvance, bool ignoreActionResult)
        {
            Requires.NonNull(wf, nameof(wf));

            string nextOwnerId = "";
            var desc = await (this as IWorkflowManager).GetWorkflowDescriptionAsync(wf);
            var currentActivity = desc.WebActionWorkflowActivities.FirstOrDefault(z => z.WorkflowActivityKey == wf.CurrentWorkflowActivityKey);

            //if question is null stays in current activity
            string nextActivityKey = "";
            WorkflowActivity nextActivity;
            if (question != null)
            {
                var chooser = ComponentContext.ResolveNamed<IActivityChooser>(currentActivity.NextActivityChooserTypeName);
                nextActivityKey = chooser.GetNextActivityKey(wf, question, currentActivity.NextActivityChooserConfig) ?? wf.CurrentWorkflowActivityKey;
                nextActivity = desc.Activities.First(z => z.WorkflowActivityKey == nextActivityKey) ?? currentActivity;
            }
            else
            {
                nextActivity = currentActivity;
            }

            //TODO: Handle null case which says stay where you are.

            wf.CurrentWorkflowActivityKey = nextActivity.WorkflowActivityKey;


            //TODO: Updata other info like the owner, date
            //TODO: Add logic for handling groups of users.
            if (nextActivity is WebActionWorkflowActivity)
            {
                if ((wf.CurrentWorkflowActivityKey != currentActivity.WorkflowActivityKey && wf.AspNetUser.UserName != nextActivity.OwnerUserName) || forceAdvance == true)
                {
                    nextOwnerId = await GetNextOwnerUserIdAsync(nextActivity.OwnerUserName, wf,
                        nextActivity.WorkflowActivityKey);
                    wf.OwnerUserId = nextOwnerId;
                    var nextUser = Cacher.FindOrCreateValWithSimpleKey(
                        wf.OwnerUserId,
                        () => DB.AspNetUsers.Find(wf.OwnerUserId));
                    var emailTemplate = Cacher.FindOrCreateValWithSimpleKey(
                        nextActivity.EmailTemplateId,
                        () => DB.EmailTemplates.Find(nextActivity.EmailTemplateId));
                    var emailModel = new EmailViewModel
                    {
                        UserName = nextUser.UserName,
                        PDN = wf.UnliquidatedObligation.PegasysDocumentNumber
                    };
                    //TODO: What happens if it crashes?
                    //BackgroundJobClient.Enqueue<IBackgroundTasks>(
                    // bt => bt.Email("new owner", nextUser.Email, emailTemplate.EmailBody, emailModel));
                }
                wf.UnliquidatedObligation.Status = nextActivity.ActivityName;

                if (nextActivity.DueIn != null)
                    wf.ExpectedDurationInSeconds = (long?)nextActivity.DueIn.Value.TotalSeconds;

                if (question != null && question.Answer == "Valid")
                {
                    wf.UnliquidatedObligation.Valid = true;
                }
                else if (question != null && question.Answer == "Invalid")
                {
                    wf.UnliquidatedObligation.Valid = false;
                }

                if (ignoreActionResult)
                {
                    return null;
                }

                //TODO: if owner changes, look at other ways of redirecting.

                var next = (WebActionWorkflowActivity)nextActivity;
                var c = new RedirectingController();

                var routeValues = new RouteValueDictionary(next.RouteValueByName);
                //routeValues[WorkflowIdRouteValueName] = wf.WorkflowId;
                return c.RedirectToAction(next.ActionName, next.ControllerName, routeValues);
            }
            else
            {
                //TODO: handle background hangfire.
                throw new NotImplementedException();
            }
        }

        async Task<ActionResult> IWorkflowManager.RequestReassignAsync(Workflow wf)
        {
            Requires.NonNull(wf, nameof(wf));

            //TODO: Get programatically based on user's region
            var reassignGroupId = await GetNextOwnerUserIdAsync(Properties.Settings.Default.ReassignGroupUserName, wf, "");
            wf.OwnerUserId = reassignGroupId;
            var c = new RedirectingController();
            var routeValues = new RouteValueDictionary(new Dictionary<string, object>());
            return await Task.FromResult(c.RedirectToAction("Index", "Ulo", routeValues));
        }

        async Task IWorkflowManager.SaveQuestionAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question)
        {
            Requires.NonNull(wf, nameof(wf));
            Requires.NonNull(question, nameof(question));

            var questionFromDB = await DB.UnliqudatedObjectsWorkflowQuestions.FirstOrDefaultAsync(q => q.WorkflowId == wf.WorkflowId && q.Pending == true);
            if (questionFromDB == null)
            {
                wf.UnliqudatedObjectsWorkflowQuestions.Add(question);
            }
            else
            {
                questionFromDB.Answer = questionFromDB.Answer;
                questionFromDB.Comments = question.Comments;
                questionFromDB.JustificationId = question.JustificationId;
                questionFromDB.UserId = question.UserId;
                questionFromDB.Pending = question.Pending;
                questionFromDB.CreatedAtUtc = question.CreatedAtUtc;
            }
        }

        async Task<ActionResult> IWorkflowManager.ReassignAsync(Workflow wf, string userId, string actionName)
        {
            Requires.NonNull(wf, nameof(wf));
            Requires.Text(userId, nameof(userId));

            //TODO: Get programatically based on user's region
            //TODO: split up into two for redirect and reassign.
            wf.OwnerUserId = userId;
            var c = new RedirectingController();
            var routeValues = new RouteValueDictionary(new Dictionary<string, object>());
            var nextUser = await DB.AspNetUsers.FindAsync(wf.OwnerUserId);
            var emailTemplate = await DB.EmailTemplates.FindAsync(1);
            var emailModel = new EmailViewModel
            {
                UserName = nextUser.UserName,
                PDN = wf.UnliquidatedObligation.PegasysDocumentNumber
            };
            //TODO: What happens if it crashes?
            BackgroundJobClient.Enqueue<IBackgroundTasks>(bt => bt.Email("new owner", nextUser.Email, emailTemplate.EmailBody, emailModel));

            return await Task.FromResult(c.RedirectToAction(actionName, "Ulo", routeValues));
        }

        private async Task<string> GetNextOwnerUserIdAsync(string proposedOwnerUserName, Workflow wf, string nextActivityKey)
        {
            Requires.Text(proposedOwnerUserName, nameof(proposedOwnerUserName));
            Requires.NonNull(wf, nameof(wf));
            if (proposedOwnerUserName != Properties.Settings.Default.ReassignGroupUserName)
            {
                Requires.Text(nextActivityKey, nameof(nextActivityKey));
            }

            var u = Cacher.FindOrCreateValWithSimpleKey(proposedOwnerUserName, () => DB.AspNetUsers.FirstOrDefault(z => z.UserName == proposedOwnerUserName));
            //TODO: check if null, return proposedOwnserId
            var output = new ObjectParameter("nextOwnerId", typeof(string));
            //DB.Database.Log = s => Trace.WriteLine(s);
            DB.GetNextLevelOwnerId(u.Id, wf.WorkflowId, nextActivityKey, output);
            if (output.Value == DBNull.Value)
            {
                return await Task.FromResult(u.Id);
            }
            return await Task.FromResult(output.Value.ToString());
        }

        Task<IWorkflowDescription> IWorkflowManager.GetWorkflowDescriptionAsync(Workflow wf)
            => Cacher.FindOrCreateValWithSimpleKeyAsync(
                Cache.CreateKey(wf.WorkflowKey, wf.Version),
                () => Finder.FindAsync(wf.WorkflowKey, wf.Version)
                );        
    }
}