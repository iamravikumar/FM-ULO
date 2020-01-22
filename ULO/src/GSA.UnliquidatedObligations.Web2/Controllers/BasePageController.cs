using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RevolutionaryStuff.AspNetCore;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using Serilog;
using Serilog.Core;
using Serilog.Core.Enrichers;
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
        protected readonly ILogger Logger;
        protected readonly ICacher Cacher;
        protected readonly UloDbContext DB;
        protected readonly UserHelpers UserHelpers;

        protected BasePageController(UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger logger)
        {
            Requires.NonNull(logger, nameof(logger));

            DB = db;

            Cacher = cacher;

            PortalHelpers = portalHelpers;
            UserHelpers = userHelpers;

            Logger = logger.ForContext(new ILogEventEnricher[]
            {
                new PropertyEnricher(typeof(Type).Name, this.GetType().Name),
            });

            Logger.Debug("Page request to {Controller}", this.GetType().Name);
        }

        public string CurrentUserId
            => UserHelpers.CurrentUserId;

        public string CurrentUserName
            => UserHelpers.CurrentUserName;  
        
        public bool UseOldGetEligibleReviewersAlgorithm => PortalHelpers.UseOldGetEligibleReviewersAlgorithm;

        protected AspNetUser CurrentUser
        {
            get
            {
                if (CurrentUser_p == null)
                {
                    CurrentUser_p = DB.AspNetUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
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

        protected IQueryable<T> ApplyBrowse<T>(IQueryable<T> q, string sortCol, Type enumType, string sortDir, int? page, int? pageSize, IDictionary<string, string> colMapper = null)
        {
            SetTotalItemCount(q);
            bool isAscending = AspHelpers.IsSortDirAscending(sortDir);
            ViewBag.SortCol = sortCol;
            ViewBag.SortDir = sortDir;
            q = q.OrderByField(sortCol, enumType, isAscending);
            q = ApplyPagination(q, page, pageSize);
            return q;
        }

        protected IQueryable<T> ApplyBrowse<T>(IQueryable<T> q, string sortCol, IEnumerable<string> orderedValues, string sortDir, int? page, int? pageSize, IDictionary<string, string> colMapper = null)
        {
            SetTotalItemCount(q);
            bool isAscending = AspHelpers.IsSortDirAscending(sortDir);
            ViewBag.SortCol = sortCol;
            ViewBag.SortDir = sortDir;
            q = q.OrderByField(sortCol, orderedValues, isAscending);
            q = ApplyPagination(q, page, pageSize);
            return q;
        }

        protected void LogStaleWorkflowError(Workflow wf, string workflowRowVersionString, DateTime? editingBeganAtUtc)
        {
            Log.Error(
                "Workflow {workflowId} is stale. Trying to edit {staleWorkflowRowVersion} when {currentWorkflowRowVersion} is the most recent.  Record was in the wind for {inprogressTimespan}.",
                wf.WorkflowId, workflowRowVersionString, wf.WorkflowRowVersionString, DateTime.UtcNow.Subtract(editingBeganAtUtc.GetValueOrDefault(DateTime.MinValue))
                );
        }

        protected string GetStaleWorkflowErrorMessage(Workflow wf, string workflowRowVersionString, DateTime? editingBeganAtUtc)
           => string.Format(PortalHelpers.StaleWorkflowErrorMessageTemplate, workflowRowVersionString, editingBeganAtUtc);

        protected JsonResult CreateJsonError(Exception ex)
          => Json(new ExceptionError(ex));

        protected void AddPageAlert(string toastMessage, bool autoDismiss = false, PageAlert.AlertTypes pageAlertType = PageAlert.AlertTypes.Info, bool nextRequest = false)
           => AddPageAlert(new PageAlert(toastMessage, autoDismiss, pageAlertType), nextRequest);

        protected void AddPageAlert(PageAlert pa, bool nextRequest = false)
        {
            if (pa == null || string.IsNullOrEmpty(pa.Message)) return;
            IList<PageAlert> pageAlerts;
            if (nextRequest)
            {
                pageAlerts = PageAlert.GetPageAlerts(TempData);
            }
            else
            {
                pageAlerts = (IList<PageAlert>)ViewData[PageAlert.PageAlertsKey];
            }
            pageAlerts = pageAlerts ?? new List<PageAlert>();
            pageAlerts.Add(pa);
            if (nextRequest)
            {
                PageAlert.SetPageAlerts(TempData, pageAlerts);
            }
        }

        public IEnumerable<GetMyGroups_Result0> GetUserGroups(string userId = null)
            => Cacher.FindOrCreateValue(
                Cache.CreateKey(nameof(GetUserGroups), userId ?? CurrentUserId),
                () => DB.GetMyGroupsAsync(userId ?? CurrentUserId).ExecuteSynchronously().ToList().AsReadOnly(),
                PortalHelpers.ShortCacheTimeout
                );
    }
}
