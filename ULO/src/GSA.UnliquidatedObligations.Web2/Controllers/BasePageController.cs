using System;
using System.Collections.Generic;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using RASP = RevolutionaryStuff.AspNetCore;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public abstract class BasePageController : RASP.Controllers.BasePageController
    {
        public static class WorkflowStalenessMagicFieldNames
        {
            public const string WorkflowRowVersionString = "StalenessWorkflowRowVersionString";
            public const string EditingBeganAtUtc = "StalenessEditingBeganAtUtc";
            public const string WorkflowId = "StalenessWorkflowId";
        }

        protected readonly PortalHelpers PortalHelpers;
        protected readonly ICacher Cacher;
        protected readonly UloDbContext DB;
        protected readonly UserHelpers UserHelpers;

        static BasePageController()
        {
            ApplySortDefaultMappings["CreatedAt"] = "CreatedAtUtc";
        }

        protected BasePageController(UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger logger)
            : base(logger)
        {
            DB = db;
            Cacher = cacher;
            PortalHelpers = portalHelpers;
            UserHelpers = userHelpers;
            DB.CurrentUserId = userHelpers.CurrentUserId;
            LogDebug("Page request to {Controller}", this.GetType().Name);
        }

        public string CurrentUserId
            => UserHelpers.CurrentUserId;

        public string CurrentUserName
            => UserHelpers.CurrentUserName;  
        
        protected AspNetUser CurrentUser
        {
            get
            {
                if (CurrentUser_p == null)
                {
                    CurrentUser_p = DB.AspNetUsers
                        .Include(u=>u.ChildUserUserUsers)
                        .AsNoTracking()
                        .FirstOrDefault(u => u.UserName == User.Identity.Name);
                }
                return CurrentUser_p;
            }
        }
        private AspNetUser CurrentUser_p;

        protected void OnlySupportedInDevelopmentEnvironment()
        {
            if (!PortalHelpers.UseDevAuthentication)
            {
                throw new NotSupportedException($"You are trying to make a call that is only allowed when {PortalHelpers}.{PortalHelpers.UseDevAuthentication}=true");
            } 
        }

        protected IDictionary<int, string> PopulateDocumentTypeNameByDocumentTypeIdInViewBag()
        {
            var documentTypeNameByDocumentTypeId = Cacher.FindOrCreateValue(
                Cache.CreateKey(nameof(PopulateDocumentTypeNameByDocumentTypeIdInViewBag)),
                () => DB.DocumentTypes.AsNoTracking().ToDictionary(z => z.DocumentTypeId, z => z.Name).AsReadOnly(),
                PortalHelpers.MediumCacheTimeout);
            ViewBag.DocumentTypeNameByDocumentTypeId = documentTypeNameByDocumentTypeId;
            return documentTypeNameByDocumentTypeId;
        }

        protected void LogStaleWorkflowError(Workflow wf, string workflowRowVersionString, DateTime? editingBeganAtUtc)
            => LogError(
                "Workflow {workflowId} is stale. Trying to edit {staleWorkflowRowVersion} when {currentWorkflowRowVersion} is the most recent.  Record was in the wind for {inprogressTimespan}.",
                wf.WorkflowId, workflowRowVersionString, wf.WorkflowRowVersionString, DateTime.UtcNow.Subtract(editingBeganAtUtc.GetValueOrDefault(DateTime.MinValue))
                );

        protected string GetStaleWorkflowErrorMessage(Workflow wf, string workflowRowVersionString, DateTime? editingBeganAtUtc)
           => string.Format(PortalHelpers.StaleWorkflowErrorMessageTemplate, workflowRowVersionString, editingBeganAtUtc);

        protected JsonResult CreateJsonError(Exception ex)
          => Json(new ExceptionError(ex));

        public IEnumerable<GetMyGroups_Result0> GetUserGroups(string userId = null)
            => Cacher.FindOrCreateValue(
                Cache.CreateKey(nameof(GetUserGroups), userId ?? CurrentUserId),
                () => DB.GetMyGroupsAsync(userId ?? CurrentUserId).ExecuteSynchronously().ToList().AsReadOnly(),
                PortalHelpers.ShortCacheTimeout
                );
    }
}
