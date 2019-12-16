using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Data.Reporting;
using GSA.UnliquidatedObligations.Web.Authorization;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using Serilog;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanExecuteReports)]
    public class ReportsController : BasePageController
    {
        public const string Name = "Reports";

        public static class ActionNames
        {
            public const string ListReports = "Index";
            public const string ConfigureReport = "ConfigureReport";
            public const string ExecuteReport = "ExecuteReport";
        }

        public ReportsController(UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        { }

        [ActionName(ActionNames.ListReports)]
        [Route("Reports")]
        public async Task<ActionResult> Index(string sortCol, string sortDir, int? page, int? pageSize)
        {
            var reports = await GetAllReportsAsync();

            reports = ApplyBrowse(reports, sortCol, sortDir, page, pageSize);
            return View(reports);
        }

        [ActionName(ActionNames.ConfigureReport)]
        [Route("Reports/{name}")]
        [HttpGet]
        public async Task<ActionResult> ConfigureReport(string name)
        {
            var report = (await GetAllReportsAsync()).FirstOrDefault(r => r.Name == name);
            if (report == null) return NotFound();
            return View(new ConfigureReportModel(PortalHelpers, report));
        }

        [ActionName(ActionNames.ExecuteReport)]
        [Route("Reports/{name}/Execute")]
        [HttpPost]
        public async Task<ActionResult> ExecuteReport(string name)
        {
            var report = (await GetAllReportsAsync()).FirstOrDefault(r => r.Name == name);
            if (report == null) return NotFound();
            using (var conn = new SqlConnection(PortalHelpers.DefaultUloConnectionString))
            {
                Log.Information("Executing report {ReportName} with {SprocSchema}.{SprocName}", name, report.SprocSchema, report.SprocName);
                conn.Open();
                using (var cmd = new SqlCommand($"[{report.SprocSchema}].[{report.SprocName}]", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 60 * 60
                })
                {
                    foreach (var pd in report.Parameters)
                    {
                        var sval = pd.IsHardcoded ? pd.HardCodedValue : Request.Form[pd.SqlParameterName].FirstOrDefault();
                        var oval = Convert.ChangeType(sval, pd.ClrType);
                        cmd.Parameters.Add(new SqlParameter("@" + pd.SqlParameterName, oval));
                    }
                    var ds = cmd.ExecuteReadDataSet(Logger);
                    var st = new MemoryStream();
                    ds.ToSpreadSheet(st);
                    st.Position = 0;
                    var now = DateTime.UtcNow;
                    string filename = report.FilenamePattern == null ?
                        report.Name :
                        string.Format(
                            report.FilenamePattern,
                            now,
                            now.ToLocalTime(),
                            now.ToTimeZone(PortalHelpers.DisplayTimeZone));
                    filename += MimeType.Application.SpreadSheet.Xlsx.PrimaryFileExtension;
                    var cd = new System.Net.Mime.ContentDisposition
                    {
                        FileName = filename,
                        Inline = false,
                    };
                    Response.Headers.Add("Content-Disposition", cd.ToString());
                    return File(st, MimeType.Application.SpreadSheet.Xlsx);
                }
            }
        }

        private Task<IQueryable<ReportDescription>> GetAllReportsAsync()
            => Task.FromResult(Cacher.FindOrCreateValue(
                nameof(GetAllReportsAsync),
                () => DB.ReportDefinitions.Where(rd => rd.IsActive == true).Select(rd => rd.Description).WhereNotNull().ToList().AsReadOnly(),
                PortalHelpers.MediumCacheTimeout).AsQueryable()
                );
    }
}
