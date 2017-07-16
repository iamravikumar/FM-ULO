using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ULODBEntities DB;
        protected readonly IComponentContext ComponentContext;
        protected readonly ICacher Cacher;

        public BaseController(ULODBEntities db, IComponentContext componentContext, ICacher cacher)
        {
            DB = db;
            ComponentContext = componentContext;
            Cacher = cacher;
            System.Web.HttpContext.Current.Items["ComponentContext"] = ComponentContext;
        }

        public string CurrentUserId
            => PortalHelpers.GetUserId(User?.Identity?.Name);

        public IEnumerable<GetMyGroups_Result> GetUserGroups(string userId=null)
            => Cacher.FindOrCreateValWithSimpleKey(
                Cache.CreateKey(nameof(GetUserGroups), userId??CurrentUserId),
                () => DB.GetMyGroups(userId??CurrentUserId).ToList().AsReadOnly(),
                UloHelpers.ShortCacheTimeout
                );

        protected IQueryable<T> ApplyBrowse<T>(IQueryable<T> q, string sortCol, string sortDir, int? page, int? pageSize, IDictionary<string, string> colMapper = null)
        {
            ViewBag.TotalItemCount = q.Count();
            q = ApplySort(q, sortCol, sortDir, colMapper);
            q = ApplyPagination(q, page, pageSize);
            return q;
        }

        protected IQueryable<T> ApplyBrowse<T>(IQueryable<T> q, string sortCol, Type enumType, string sortDir, int? page, int? pageSize, IDictionary<string, string> colMapper = null)
        {
            ViewBag.TotalItemCount = q.Count();
            q = q.OrderByField(sortCol, enumType, AspHelpers.IsSortDirAscending(sortDir));
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
            return q.Skip((p - 1) * s).Take(s);
        }
    }
}
