using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Authorization;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly IReportRunner ReportRunner;
        private readonly IOptions<Config> ConfigOptions;
        private readonly IRecurringJobManager RJM;
        private readonly IBackgroundJobClient BackgroundJobClient;

        public static class ActionNames
        {
            public const string ListReports = "Index";
            public const string ConfigureReport = "ConfigureReport";
            public const string ExecuteReport = "ExecuteReport";
        }

        public const string ReportParameterNamePrefix = "rp__";

        public static class ReportFrequencies
        {
            public const string Synchronous = "synchronous";
            public const string EmailMeOnce = "emailMeOnce";
            public const string Recurring = "recurring";
        }

        public class Config
        {
            public const string ConfigSectionName = "ReportsControllerConfig";
            public int ReportEmailTemplateId { get; set; }
            public string ReportRecipientEmailDomains { get; set; }
        }

        public ReportsController(IReportRunner reportRunner, IOptions<Config> configOptions, UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger logger, IRecurringJobManager rjm, IBackgroundJobClient backgroundJobClient)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            ReportRunner = reportRunner;
            ConfigOptions = configOptions;
            RJM = rjm;
            BackgroundJobClient = backgroundJobClient;
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
            var report = (await PortalHelpers.GetReportsAsync(name)).FirstOrDefault();
            if (report == null) return NotFound();
            return View(new ConfigureReportModel(PortalHelpers, report) { CurrentUserEmail = CurrentUser.Email });
        }

        [ActionName(ActionNames.ExecuteReport)]
        [Route("Reports/{name}/Execute")]
        [HttpPost]
        public async Task<ActionResult> ExecuteReport(string name, string frequency)
        {
            var report = (await PortalHelpers.GetReportsAsync(name)).FirstOrDefault();
            if (report == null) return NotFound();
            var paramValueByParamName = new Dictionary<string, string>();
            foreach (string k in Request.Form.Keys)
            {
                if (!k.StartsWith(ReportParameterNamePrefix)) continue;
                paramValueByParamName[k.RightOf(ReportParameterNamePrefix)] = Request.Form[k].FirstOrDefault();
            }
            var config = ConfigOptions.Value;
            var template = PortalHelpers.GetEmailTemplate(config.ReportEmailTemplateId);
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
                        Response.Headers.Add("Content-Disposition", cd.ToString());
                        return File(res.ContentStream, res.ContentType.MediaType);
                    }
                case ReportFrequencies.EmailMeOnce:
                    {
                        var recipients = new[] { string.Format(config.ReportRecipientEmailDomains, User.Identity.Name) };
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
                        var domains = CSV.ParseLine(config.ReportRecipientEmailDomains).ToCaseInsensitiveSet();
                        var recipients = Request.Form["recipients"].FirstOrDefault().Split(';', ',', '\t', ' ', '\r', '\n').Select(z => z.TrimOrNull()).WhereNotNull().Where(z => domains.Contains(z.RightOf("@"))).ToArray();
                        var now = DateTime.UtcNow;
                        var time = DateTime.Parse(Request.Form["time"]);
                        var recurringJobId = $"{User.Identity.Name}.Report.{report.Name}.{now.ToYYYYMMDD()}.{now.ToHHMMSS()}";
                        var o = new ReportEmailViewModel(User.Identity.Name)
                        {
                            JobId = recurringJobId,
                            Report = report
                        };
                        var job = Hangfire.Common.Job.FromExpression<IBackgroundTasks>(b => b.EmailReport(recipients, template.EmailSubject, template.EmailBody, template.EmailHtmlBody, o, name, paramValueByParamName));
                        var cron = Hangfire.Cron.Daily(time.Hour, time.Minute);
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
