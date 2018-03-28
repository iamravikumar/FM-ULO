using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ULODBEntities DB;
        protected readonly IComponentContext ComponentContext;
        protected readonly ICacher Cacher;
        internal readonly ILogger Log;

        public static class WorkflowStalenessMagicFieldNames
        {
            public const string WorkflowRowVersionString = "StalenessWorkflowRowVersionString";
            public const string EditingBeganAtUtc = "StalenessEditingBeganAtUtc";
            public const string WorkflowId = "StalenessWorkflowId";
        }

        protected async Task CheckStalenessFromFormAsync(Workflow wf=null)
        {
            int workflowId = Parse.ParseInt32(Request.Form[WorkflowStalenessMagicFieldNames.WorkflowId]);
            DateTime editingBeganAtUtc = DateTime.Parse(Request.Form[WorkflowStalenessMagicFieldNames.EditingBeganAtUtc]);
            string workflowRowVersionString = Request.Form[WorkflowStalenessMagicFieldNames.WorkflowRowVersionString];
            if (wf == null || wf.WorkflowId != workflowId)
            {
                wf = await DB.Workflows.FindAsync(workflowId);
            }
            if (wf == null) throw new FileNotFoundException();
            if (wf.WorkflowRowVersionString != workflowRowVersionString)
            {
                LogStaleWorkflowError(wf, workflowRowVersionString, editingBeganAtUtc);
                var staleMessage = GetStaleWorkflowErrorMessage(wf, workflowRowVersionString, editingBeganAtUtc);
                throw new Exception(staleMessage);
            }
        }

        protected BaseController(ULODBEntities db, IComponentContext componentContext, ICacher cacher, ILogger log)
        {
            DB = db;
            ComponentContext = componentContext;
            Cacher = cacher;
            Log = log.ForContext(GetType());
            var ctx = System.Web.HttpContext.Current;
            if (ctx != null)
            {
                ctx.Items["ComponentContext"] = ComponentContext;
                var u = ctx.Request?.Url;
                if (u != null)
                {
                    Log.Debug("Page request to {Controller} with {RequestUrl}", this.GetType().Name, u);
                }
            }
        }

        protected JsonResult CreateJsonError(Exception ex)
            => Json(new ExceptionError(ex));

        protected void LogStaleWorkflowError(Workflow wf, string workflowRowVersionString, DateTime ?editingBeganAtUtc)
        {
            Log.Error(
                "Workflow {workflowId} is stale. Trying to edit {staleWorkflowRowVersion} when {currentWorkflowRowVersion} is the most recent.  Record was in the wind for {inprogressTimespan}.",
                wf.WorkflowId, workflowRowVersionString, wf.WorkflowRowVersionString, DateTime.UtcNow.Subtract(editingBeganAtUtc.GetValueOrDefault(DateTime.MinValue))
                );
        }

        protected string GetStaleWorkflowErrorMessage(Workflow wf, string workflowRowVersionString, DateTime? editingBeganAtUtc)
            => string.Format(Properties.Settings.Default.StaleWorkflowErrorMessageTemplate, workflowRowVersionString, editingBeganAtUtc);

        protected void OnlySupportedInDevelopmentEnvironment()
        {
            if (!PortalHelpers.UseDevAuthentication) throw new NotSupportedException();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewData["PageAlerts"] = TempData["PageAlerts"];
            base.OnActionExecuting(filterContext);
        }

        protected void AddPageAlert(string toastMessage, bool autoDismiss = false, PageAlert.AlertTypes pageAlertType = PageAlert.AlertTypes.Info, bool nextRequest = false)
            => AddPageAlert(new PageAlert(toastMessage, autoDismiss, pageAlertType), nextRequest);

        protected void AddPageAlert(PageAlert pa, bool nextRequest = false)
        {
            if (pa == null || string.IsNullOrEmpty(pa.Message)) return;
            var d = nextRequest ? (IDictionary<string,object>) TempData : (IDictionary<string, object>) ViewData;
            var pageAlerts = d["PageAlerts"] as IList<PageAlert>;
            if (pageAlerts == null)
            {
                d["PageAlerts"] = pageAlerts = new List<PageAlert>();
            }
            pageAlerts.Add(pa);
        }

        protected ActionResult RedirectToHome()
            => RedirectToAction(UloController.ActionNames.Home, UloController.Name);

        protected virtual ActionResult RedirectToIndex()
            => RedirectToAction("Index");

        public string CurrentUserId
            => PortalHelpers.GetUserId(User?.Identity?.Name);

        protected AspNetUser CurrentUser
        {
            get
            {
                if (CurrentUser_p == null)
                {
                    CurrentUser_p = DB.AspNetUsers.FirstOrDefault(u => u.UserName == this.User.Identity.Name);
                }
                return CurrentUser_p;
            }
        }
        private AspNetUser CurrentUser_p;

        public IEnumerable<GetMyGroups_Result> GetUserGroups(string userId=null)
            => Cacher.FindOrCreateValWithSimpleKey(
                Cache.CreateKey(nameof(GetUserGroups), userId??CurrentUserId),
                () => DB.GetMyGroups(userId??CurrentUserId).ToList().AsReadOnly(),
                UloHelpers.ShortCacheTimeout
                );

        protected IDictionary<int, string> PopulateDocumentTypeNameByDocumentTypeIdInViewBag()
        {
            var documentTypeNameByDocumentTypeId = Cacher.FindOrCreateValWithSimpleKey(
                Cache.CreateKey(typeof(DocumentsController), "documentTypeNameByDocumentTypeId"),
                () => DB.DocumentTypes.ToDictionary(z => z.DocumentTypeId, z => z.Name).AsReadOnly(),
                UloHelpers.MediumCacheTimeout);
            ViewBag.DocumentTypeNameByDocumentTypeId = documentTypeNameByDocumentTypeId;
            return documentTypeNameByDocumentTypeId;
        }

        protected IQueryable<T> ApplyBrowse<T>(IQueryable<T> q, string sortCol, string sortDir, int? page, int? pageSize, IDictionary<string, string> colMapper = null)
        {
            var cnt = q.Count();
            ViewBag.TotalItemCount = cnt;
            if (cnt == 0)
            {
                q = (new T[0]).AsQueryable();
            }
            else
            {
                q = ApplySort(q, sortCol, sortDir, colMapper);
                q = ApplyPagination(q, page, pageSize);
            }
            return q;
        }

        protected IQueryable<T> ApplyBrowse<T>(IQueryable<T> q, string sortCol, Type enumType, string sortDir, int? page, int? pageSize, IDictionary<string, string> colMapper = null)
        {
            ViewBag.TotalItemCount = q.Count();
            bool isAscending = AspHelpers.IsSortDirAscending(sortDir);
            ViewBag.SortCol = sortCol;
            ViewBag.SortDir = sortDir;
            q = q.OrderByField(sortCol, enumType, isAscending);
            q = ApplyPagination(q, page, pageSize);
            return q;
        }

        protected IQueryable<T> ApplyBrowse<T>(IQueryable<T> q, string sortCol, IEnumerable<string> orderedValues, string sortDir, int? page, int? pageSize, IDictionary<string, string> colMapper = null)
        {
            ViewBag.TotalItemCount = q.Count();
            bool isAscending = AspHelpers.IsSortDirAscending(sortDir);
            ViewBag.SortCol = sortCol;
            ViewBag.SortDir = sortDir;
            q = q.OrderByField(sortCol, orderedValues, isAscending);
            q = ApplyPagination(q, page, pageSize);
            return q;
        }

        private static readonly IDictionary<string, string> NoMappings = new Dictionary<string, string>().AsReadOnly();

        protected IQueryable<T> ApplySort<T>(IQueryable<T> q, string sortCol, string sortDir, IDictionary<string, string> colMapper = null)
        {
            var m = new Dictionary<string, string>(colMapper ?? NoMappings);
            if (!m.ContainsKey("CreatedAt"))
            {
                m["CreatedAt"] = "CreatedAtUtc";
            }
            sortCol = StringHelpers.TrimOrNull(sortCol);
            bool isAscending = AspHelpers.IsSortDirAscending(sortDir);
            ViewBag.SortCol = sortCol;
            ViewBag.SortDir = sortDir;
            if (sortCol != null)
            {
                sortCol = m.FindOrMissing(sortCol, sortCol);
                q = q.OrderByField(sortCol, isAscending);
            }
            return q;
        }

        protected IQueryable<T> ApplyPagination<T>(IQueryable<T> q, int? page = null, int? pageSize = null)
        {
            var rowsPerPageString = Request.Cookies["rowsPerPage"];
            int rowsPerPage;
            if (!int.TryParse(rowsPerPageString?.Value, out rowsPerPage))
            {
                rowsPerPage = 10;
            }
            var s = rowsPerPage;
            var p = page.GetValueOrDefault();
            if (p < 1) p = 1;
            ViewBag.PaginationSupported = true;
            ViewBag.PageNum = p;
            ViewBag.PageSize = s;
            return q.Skip((p - 1) * s).Take(s).ToList().AsQueryable();
        }
    }
}
