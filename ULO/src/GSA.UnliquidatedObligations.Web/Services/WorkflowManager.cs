using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using Hangfire;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class WorkflowManager : IWorkflowManager
    {
        public const string WorkflowIdRouteValueName = "workflowId";

        private readonly IComponentContext ComponentContext;
        private readonly IWorkflowDescriptionFinder Finder;
        private readonly IBackgroundJobClient BackgroundJobClient;
        private readonly ULODBEntities DB;
        private readonly ICacher Cacher;
        private readonly Serilog.ILogger Log;

        public WorkflowManager(IComponentContext componentContext, IWorkflowDescriptionFinder finder, IBackgroundJobClient backgroundJobClient, ULODBEntities db, Serilog.ILogger log, ICacher cacher)
        {
            ComponentContext = componentContext;
            Finder = finder;
            BackgroundJobClient = backgroundJobClient;
            DB = db;
            Log = log.ForContext<WorkflowManager>();
            Cacher = cacher;
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

            try
            {
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
                        nextOwnerId = await GetNextOwnerUserIdAsync(nextActivity.OwnerUserName, wf, nextActivity.WorkflowActivityKey, nextActivity.OwnerProhibitedPreviousActivityNames);
                        wf.OwnerUserId = nextOwnerId;

                        await NotifyNewAssigneeAsync(wf, nextOwnerId, nextActivity.EmailTemplateId);
                    }
                    wf.UnliquidatedObligation.Status = nextActivity.ActivityName;

                    if (nextActivity.DueIn != null)
                        wf.ExpectedDurationInSeconds = (long?)nextActivity.DueIn.Value.TotalSeconds;

                    if (question != null && question.IsValid)
                    {
                        wf.UnliquidatedObligation.Valid = true;
                    }
                    else if (question != null && question.IsInvalid)
                    {
                        wf.UnliquidatedObligation.Valid = false;
                    }
                    else
                    {
                        wf.UnliquidatedObligation.Valid = null;
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
                    throw new NotImplementedException();
                }
            }
            finally
            {
                Log.Information("Workflow {WorkflowId} with {WorkflowKey} changing from activity {OldWorkflowActivityKey} to activity {NewWorkflowActivityKey} owned By {NewOwnerId}", wf.WorkflowId, wf.WorkflowKey, currentActivity, wf.CurrentWorkflowActivityKey, nextOwnerId);
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
            return await Task.FromResult(c.RedirectToAction(UloController.ActionNames.Index, UloController.Name, routeValues));
        }

        private Task NotifyNewAssigneeAsync(Workflow wf, string userId, int emailTemplateId)
        {
            var u = Cacher.FindOrCreateValWithSimpleKey(
                userId,
                () => DB.AspNetUsers.Select(z => new { UserId = z.Id, UserType = z.UserType, UserName = z.UserName, Email = z.Email }).FirstOrDefault(z => z.UserId == userId),
                UloHelpers.MediumCacheTimeout);
            if (u == null)
            {
                Log.Error($"Cannot find {userId}", userId);
            }
            else if (u.UserType==AspNetUser.UserTypes.Person && RegexHelpers.Common.EmailAddress.IsMatch(u.Email))
            {
                var emailTemplate = Cacher.FindOrCreateValWithSimpleKey(
                    emailTemplateId,
                    () => DB.EmailTemplates.Select(z=>new { EmailBody = z.EmailBody, EmailSubject = z.EmailSubject, EmailTemplateId = z.EmailTemplateId }).FirstOrDefault(z => z.EmailTemplateId == emailTemplateId),
                    UloHelpers.MediumCacheTimeout);
                if (emailTemplate == null)
                {
                    Log.Error($"Cannot find emailTemplateId={emailTemplateId}");
                }
                else
                {
                    var emailModel = new EmailViewModel
                    {
                        UserName = u.UserName,
                        PDN = wf.UnliquidatedObligation.PegasysDocumentNumber,
                        WorkflowId = wf.WorkflowId,
                        UloId = wf.UnliquidatedObligation.UloId
                    };
                    BackgroundJobClient.Enqueue<IBackgroundTasks>(bt => bt.Email(emailTemplate.EmailSubject, u.Email, emailTemplate.EmailBody, emailModel));
                }
            }
            else
            {
                Log.Information($"Will not send email to {u}");
            }
            return Task.CompletedTask;
        }

        async Task<ActionResult> IWorkflowManager.ReassignAsync(Workflow wf, string userId, string actionName)
        {
            Requires.NonNull(wf, nameof(wf));
            Requires.Text(userId, nameof(userId));

            wf.OwnerUserId = userId;
            var routeValues = new RouteValueDictionary(new Dictionary<string, object>());

            await NotifyNewAssigneeAsync(wf, userId, Properties.Settings.Default.ManualReassignmentEmailTemplateId);

            var c = new RedirectingController();
            return await Task.FromResult(c.RedirectToAction(actionName, UloController.Name, routeValues));
        }

        private async Task<string> GetNextOwnerUserIdAsync(string proposedOwnerUserName, Workflow wf, string nextActivityKey, IEnumerable<string> ownerProhibitedPreviousActivityNames=null)
        {
            Requires.Text(proposedOwnerUserName, nameof(proposedOwnerUserName));
            Requires.NonNull(wf, nameof(wf));

            var u = Cacher.FindOrCreateValWithSimpleKey(
                proposedOwnerUserName, 
                () => DB.AspNetUsers.Select(z=>new {UserId=z.Id, UserType=z.UserType, UserName=z.UserName}).FirstOrDefault(z => z.UserName == proposedOwnerUserName),
                UloHelpers.MediumCacheTimeout
                );

            if (u.UserType == AspNetUser.UserTypes.Person)
            {
                return await Task.FromResult(u.UserType);
            }
            else
            {
                if (proposedOwnerUserName != Properties.Settings.Default.ReassignGroupUserName)
                {
                    Requires.Text(nextActivityKey, nameof(nextActivityKey));
                }

                var output = new ObjectParameter("nextOwnerId", typeof(string));
                //DB.Database.Log = s => Trace.WriteLine(s);
                DB.GetNextLevelOwnerId(u.UserId, wf.WorkflowId, nextActivityKey, CSV.FormatLine(ownerProhibitedPreviousActivityNames ?? Empty.StringArray, false), output);
                if (output.Value == DBNull.Value)
                {
                    return await Task.FromResult(u.UserId);
                }
                return await Task.FromResult(output.Value.ToString());
            }
        }

        Task<IWorkflowDescription> IWorkflowManager.GetWorkflowDescriptionAsync(Workflow wf)
            => Cacher.FindOrCreateValWithSimpleKeyAsync(
                Cache.CreateKey(wf.WorkflowKey, wf.Version),
                () => Finder.FindAsync(wf.WorkflowKey, wf.Version),
                UloHelpers.MediumCacheTimeout
                );        
    }
}