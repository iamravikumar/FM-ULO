using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Data.Reporting;
using GSA.UnliquidatedObligations.Utility;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanExecuteReports)]
    public class ReportsController : BaseController
    {
        public const string Name = "Reports";

        public static class ActionNames
        {
            public const string ListReports = "Index";
            public const string ConfigureReport = "ConfigureReport";
            public const string ExecuteReport = "ExecuteReport";
        }

        public ReportsController(ULODBEntities db, IComponentContext componentContext)
            : base(db, componentContext)
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
        [Route("Reports/{name}/Configure")]
        [HttpGet]
        public async Task<ActionResult> ConfigureReport(string name)
        {
            var report = (await GetAllReportsAsync()).FirstOrDefault(r => r.Name == name);
            if (report == null) return HttpNotFound();
            return View(report);
        }

        [ActionName(ActionNames.ExecuteReport)]
        [Route("Reports/{name}/Execute")]
        [HttpPost]
        public async Task<ActionResult> ExecuteReport(string name)
        {
            var report = (await GetAllReportsAsync()).FirstOrDefault(r => r.Name == name);
            if (report == null) return HttpNotFound();
            using (var conn = new SqlConnection(PortalHelpers.DefaultUloConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand($"[{report.SprocSchema}].[{report.SprocName}]", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 60 * 60
                })
                {
                    foreach (var pd in report.Parameters)
                    {
                        var sval = pd.IsHardcoded ? pd.HardCodedValue : Request.Form[pd.SqlParameterName];
                        var oval = Convert.ChangeType(sval, pd.ClrType);
                        cmd.Parameters.Add(new SqlParameter("@"+pd.SqlParameterName, oval));
                    }
                    var ds = cmd.ExecuteReadDataSet();
                    var st = new MemoryStream();
                    ds.ToSpreadSheet(st);
                    st.Position = 0;
                    var cd = new System.Net.Mime.ContentDisposition
                    {
                        FileName = report.Name + MimeType.Application.SpreadSheet.Xlsx.PrimaryFileExtension,
                        Inline = false,                         
                    };
                    Response.AppendHeader("Content-Disposition", cd.ToString());
                    return File(st, MimeType.Application.SpreadSheet.Xlsx);
                }
            }
        }

        private Task<IQueryable<ReportDescription>> GetAllReportsAsync()
        {
            return Task.FromResult(
                DB.ReportDefinitions.Where(rd => rd.IsActive == true).ConvertAll(rd => rd.Description).ToList().AsQueryable()
            );
        }
    }
}
