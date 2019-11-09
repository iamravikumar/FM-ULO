using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class WorkflowManager : IWorkflowManager
    {
        public const string WorkflowIdRouteValueName = "workflowId";
        private readonly IOptions<Config> ConfigOptions;
        private readonly PortalHelpers PortalHelpers;
        private readonly UserHelpers UserHelpers;
        private readonly IServiceProvider ServiceProvider;
        private readonly IWorkflowDescriptionFinder Finder;
        private readonly IBackgroundJobClient BackgroundJobClient;
        private readonly UloDbContext DB;
        private readonly ICacher Cacher;
        private readonly Serilog.ILogger Log;

        public class Config
        {
            public const string ConfigSectionName = "WorkflowManagerConfig";
            public string ReassignGroupUserName { get; set; }
            public int ManualReassignmentEmailTemplateId { get; set; }
        }

        public WorkflowManager(IOptions<Config> configOptions, PortalHelpers portalHelpers, UserHelpers userHelpers, IServiceProvider serviceProvider, IWorkflowDescriptionFinder finder, IBackgroundJobClient backgroundJobClient, UloDbContext db, Serilog.ILogger log, ICacher cacher)
        {
            ConfigOptions = configOptions;
            PortalHelpers = portalHelpers;
            UserHelpers = userHelpers;
            ServiceProvider = serviceProvider;
            Finder = finder;
            BackgroundJobClient = backgroundJobClient;
            DB = db;
            Log = log.ForContext<WorkflowManager>();
            Cacher = cacher;
        }
        private class RedirectingController : Controller
        {
            public ActionResult RedirectToAction(string actionName, string controllerName, RouteValueDictionary routeValues)
            {
                return base.RedirectToAction(actionName, controllerName, (object) routeValues);
            }
        }

        private async Task<string> GetNextOwnerUserIdAsync(Workflow wf, string proposedOwnerUserName, string nextActivityKey = null)
        {
            Requires.NonNull(wf, nameof(wf));
            nextActivityKey = nextActivityKey ?? wf.CurrentWorkflowActivityKey;

            string proposedOwnerUserId = null;
            if (proposedOwnerUserName != null)
            {
                proposedOwnerUserId = UserHelpers.GetUserId(proposedOwnerUserName);
                if (proposedOwnerUserId == null) throw new Exception($"Could not find user=[{proposedOwnerUserName}]");
            }

            var res = await DB.GetNextLevelOwnerIdAsync(proposedOwnerUserId, wf.WorkflowId, nextActivityKey);
            var nextOwnerId = res.GetOutputParameterVal<string>("nextOwnerId");

            Log.Information($"DB.GetNextLevelOwnerId('{proposedOwnerUserId}', {wf.WorkflowId}, '{nextActivityKey}')=>{nextOwnerId}");

            return nextOwnerId;
        }

        private Task NotifyNewAssigneeAsync(Workflow wf, string userId, int emailTemplateId)
        {
            var u = Cacher.FindOrCreateValue(
                $"{nameof(NotifyNewAssigneeAsync)}.{nameof(userId)}={userId}",
                () => DB.AspNetUsers.Select(z => new { UserId = z.Id, UserType = z.UserType, UserName = z.UserName, Email = z.Email }).FirstOrDefault(z => z.UserId == userId),
                PortalHelpers.MediumCacheTimeout);
            if (u == null)
            {
                Log.Error($"Cannot find {userId}", userId);
            }
            else if (u.UserType == AspNetUser.UserTypes.Person && RegexHelpers.Common.EmailAddress.IsMatch(u.Email))
            {
                var emailTemplate = Cacher.FindOrCreateValue(
                    $"{nameof(NotifyNewAssigneeAsync)}.{nameof(emailTemplateId)}={emailTemplateId}",
                    () => DB.EmailTemplates.Select(z => new { EmailBody = z.EmailBody, EmailSubject = z.EmailSubject, EmailTemplateId = z.EmailTemplateId, EmailHtmlBody = z.EmailHtmlBody }).FirstOrDefault(z => z.EmailTemplateId == emailTemplateId),
                    PortalHelpers.MediumCacheTimeout);
                if (emailTemplate == null)
                {
                    Log.Error($"Cannot find emailTemplateId={emailTemplateId}");
                }
                else
                {
                    var emailModel = new EmailViewModel(u.UserName)
                    {
                        PDN = wf.TargetUlo.PegasysDocumentNumber,
                        WorkflowId = wf.WorkflowId,
                        UloId = wf.TargetUlo.UloId
                    };
                    BackgroundJobClient.Enqueue<IBackgroundTasks>(bt => bt.Email(u.Email, emailTemplate.EmailSubject, emailTemplate.EmailBody, emailTemplate.EmailHtmlBody, emailModel));
                }
            }
            else
            {
                Log.Information($"Will not send email to {u}");
            }
            return Task.CompletedTask;
        }

        async Task<IActionResult> IWorkflowManager.AdvanceAsync(Workflow wf, UnliqudatedObjectsWorkflowQuestion question, IList<string> submitterGroupNames, bool forceAdvance, bool ignoreActionResult, bool sendNotifications)
        {
            Requires.NonNull(wf, nameof(wf));

            string nextOwnerId = "";
            var desc = await (this as IWorkflowManager).GetWorkflowDescriptionAsync(wf);
            var currentActivity = desc.WebActionWorkflowActivities.FirstOrDefault(z => z.WorkflowActivityKey == wf.CurrentWorkflowActivityKey);

            //if question is null stays in current activity
            string nextActivityKey = "";

            try
            {
                BusinessLayer.Workflow.WorkflowActivity nextActivity;
                if (question != null)
                {
                    var t = Type.GetType(currentActivity.NextActivityChooserTypeName);
                    var chooser = (IActivityChooser) ServiceProvider.GetService(t);
                    nextActivityKey = chooser.GetNextActivityKey(wf, question, currentActivity.NextActivityChooserConfig, submitterGroupNames) ?? wf.CurrentWorkflowActivityKey;
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
                    if ((wf.CurrentWorkflowActivityKey != currentActivity.WorkflowActivityKey && wf.OwnerUser.UserName != nextActivity.OwnerUserName) || forceAdvance == true)
                    {
                        nextOwnerId = await GetNextOwnerUserIdAsync(wf, null, nextActivity.WorkflowActivityKey);
                        wf.OwnerUserId = nextOwnerId;

                        if (sendNotifications)
                        {
                            await NotifyNewAssigneeAsync(wf, nextOwnerId, nextActivity.EmailTemplateId);
                        }
                    }
                    wf.TargetUlo.Status = nextActivity.ActivityName;

                    if (nextActivity.DueIn != null)
                    {
                        wf.ExpectedDurationInSeconds = (long?)nextActivity.DueIn.Value.TotalSeconds;
                    }

                    if (question != null && question.IsValid)
                    {
                        wf.TargetUlo.Valid = true;
                    }
                    else if (question != null && question.IsInvalid)
                    {
                        wf.TargetUlo.Valid = false;
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

        async Task<IActionResult> IWorkflowManager.RequestReassignAsync(Workflow wf)
        {
            Requires.NonNull(wf, nameof(wf));

            var config = ConfigOptions.Value;

            //TODO: Get programatically based on user's region
            var reassignGroupId = await GetNextOwnerUserIdAsync(wf, config.ReassignGroupUserName);
            wf.OwnerUserId = reassignGroupId ?? UserHelpers.GetUserId(config.ReassignGroupUserName);
            var c = new RedirectingController();
            var routeValues = new RouteValueDictionary(new Dictionary<string, object>());
            return c.RedirectToAction(UloController.ActionNames.Index, UloController.Name, routeValues);
        }

        async Task<IActionResult> IWorkflowManager.ReassignAsync(Workflow wf, string userId, string actionName)
        {
            Requires.NonNull(wf, nameof(wf));
            Requires.Text(userId, nameof(userId));

            var config = ConfigOptions.Value;

            wf.OwnerUserId = userId;
            var routeValues = new RouteValueDictionary(new Dictionary<string, object>());

            await NotifyNewAssigneeAsync(wf, userId, config.ManualReassignmentEmailTemplateId);

            var c = new RedirectingController();
            return c.RedirectToAction(actionName, UloController.Name, routeValues);
        }

        Task<IWorkflowDescription> IWorkflowManager.GetWorkflowDescriptionAsync(Workflow wf)
            => Cacher.FindOrCreateValueAsync(
                Cache.CreateKey(wf.WorkflowKey, wf.Version),
                () => Finder.FindAsync(wf.WorkflowKey, wf.Version),
                PortalHelpers.MediumCacheTimeout
                );
    }
}
