using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Hangfire;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanExecuteReports)]
    public class ReportsController : BaseController
    {
        public const string Name = "Reports";
        private readonly IReportRunner ReportRunner;
        private readonly IRecurringJobManager RJM;
        private readonly IBackgroundJobClient BackgroundJobClient;
        public const string ReportParameterNamePrefix = "rp__";

        public static class ReportFrequencies
        {
            public const string Synchronous = "synchronous";
            public const string EmailMeOnce = "emailMeOnce";
            public const string Recurring = "recurring";
        }

        public static class ActionNames
        {
            public const string ListReports = "Index";
            public const string ConfigureReport = "ConfigureReport";
            public const string ExecuteReport = "ExecuteReport";
        }

        public ReportsController(ULODBEntities db, IComponentContext componentContext, ICacher cacher, Serilog.ILogger logger, IReportRunner reportRunner, IRecurringJobManager rjm, IBackgroundJobClient backgroundJobClient)
            : base(db, componentContext, cacher, logger)
        {
            this.ReportRunner = reportRunner;
            this.RJM = rjm;
            this.BackgroundJobClient = backgroundJobClient;
        }

        [ActionName(ActionNames.ListReports)]
        [Route("Reports")]
        public async Task<ActionResult> Index(string sortCol, string sortDir, int? page, int? pageSize)
        {
            var reports = await PortalHelpers.GetReportsAsync();

            reports = ApplyBrowse(reports, sortCol, sortDir, page, pageSize);
            return View(reports);
        }

        [ActionName(ActionNames.ConfigureReport)]
        [Route("Reports/{name}")]
        [HttpGet]
        public async Task<ActionResult> ConfigureReport(string name)
        {
            var report = (await PortalHelpers.GetReportsAsync()).FirstOrDefault(r => r.Name == name);
            if (report == null) return HttpNotFound();
            return View(new ConfigureReportModel(DB, report) { CurrentUserEmail = CurrentUser.Email });
        }

        [ActionName(ActionNames.ExecuteReport)]
        [Route("Reports/{name}/Execute")]
        [HttpPost]
        public async Task<ActionResult> ExecuteReport(string name, string frequency)
        {
            var report = (await PortalHelpers.GetReportsAsync(name)).FirstOrDefault();
            if (report == null) return HttpNotFound();
            var paramValueByParamName = new Dictionary<string, string>();
            foreach (string k in Request.Form.Keys)
            {
                if (!k.StartsWith(ReportParameterNamePrefix)) continue;
                paramValueByParamName[k.RightOf(ReportParameterNamePrefix)] = Request.Form[k];
            }
            var template = PortalHelpers.GetEmailTemplate(Properties.Settings.Default.ReportEmailTemplateId);
            switch (frequency)
            {
                case "":
                case null:
                case ReportFrequencies.Synchronous:
                    {
                        var res = await ReportRunner.ExecuteAsync(name, paramValueByParamName);
                        var cd = new System.Net.Mime.ContentDisposition
                        {
                            FileName = res.Name,
                            Inline = false,
                        };
                        Response.AppendHeader("Content-Disposition", cd.ToString());
                        return File(res.ContentStream, res.ContentType.MediaType);
                    }
                case ReportFrequencies.EmailMeOnce:
                    {
                        var recipients = new[] { CurrentUser.Email };
                        var o = new ReportEmailViewModel(User.Identity.Name)
                        {
                            Report = report
                        };
                        var jobId = BackgroundJobClient.Enqueue<IBackgroundTasks>(b => b.EmailReport(recipients, template.EmailSubject, template.EmailBody, template.EmailHtmlBody, o, name, paramValueByParamName));
                        this.AddPageAlert(new PageAlert($"Scheduled immediate execution of report \"{report.Title}\" for email to {recipients.FirstOrDefault()} with jobId={jobId}", false), true);
                        return RedirectToAction(ActionNames.ListReports);
                    }
                case ReportFrequencies.Recurring:
                    {
                        var domains = CSV.ParseLine(Properties.Settings.Default.ReportRecipientEmailDomains).ToCaseInsensitiveSet();
                        var recipients = Request.Form["recipients"].Split(';', ',', '\t', ' ', '\r', '\n').Select(z => z.TrimOrNull()).WhereNotNull().Where(z => domains.Contains(z.RightOf("@"))).ToArray();
                        var now = DateTime.UtcNow;
                        var recurringJobId = $"{User.Identity.Name}.Report.{report.Name}.{now.ToYYYYMMDD()}.{now.ToHHMMSS()}";
                        var o = new ReportEmailViewModel(User.Identity.Name)
                        {
                            RecurringJobId = recurringJobId,
                            Report = report,
                            UserNote = Request.Form["userNote"]
                        };
                        var job = Hangfire.Common.Job.FromExpression<IBackgroundTasks>(b => b.EmailReport(recipients, template.EmailSubject, template.EmailBody, template.EmailHtmlBody, o, name, paramValueByParamName));
                        var cron = Request.Form["cron"].TrimOrNull();
                        if (cron == null)
                        {
                            var time = DateTime.Parse(Request.Form["time"]);
                            cron = Hangfire.Cron.Daily(time.Hour, time.Minute);
                        } 
                        RJM.AddOrUpdate(recurringJobId, job, cron);
                        this.AddPageAlert(new PageAlert($"Scheduled recurring report \"{report.Title}\" for email to {recipients.Length} recipients with recurringJobId={recurringJobId}", false), true);
                        return RedirectToAction(ActionNames.ListReports);
                    }
                default:
                    throw new RevolutionaryStuff.Core.UnexpectedSwitchValueException(frequency);
            }
        }
    }
}
