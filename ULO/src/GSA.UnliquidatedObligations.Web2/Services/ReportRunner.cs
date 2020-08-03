using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data.Reporting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class ReportRunner : BaseLoggingDisposable, IReportRunner
    {
        private readonly PortalHelpers PortalHelpers;
        private readonly IOptions<Config> ConfigOptions;

        public class Config
        {
            public const string ConfigSectionName = "ReportRunnerConfig";

            public int TimeoutInSeconds { get; set; } = 60*60;
        }

        public ReportRunner(ILogger<ReportRunner> logger, PortalHelpers portalHelpers, IOptions<Config> configOptions)
            : base(logger)
        {
            PortalHelpers = portalHelpers;
            ConfigOptions = configOptions;
        }

        async Task<System.Net.Mail.Attachment> IReportRunner.ExecuteAsync(string reportName, IDictionary<string, string> paramValueByParamName)
        {
            ReportDescription report = null;
            try
            {
                report = (await PortalHelpers.GetReportsAsync(reportName)).FirstOrDefault();
                using (var conn = new SqlConnection(PortalHelpers.DefaultUloConnectionString))
                {
                    LogInformation("Executing report {ReportName} with {SprocSchema}.{SprocName}", reportName, report.SprocSchema, report.SprocName);
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand($"[{report.SprocSchema}].[{report.SprocName}]", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandTimeout = ConfigOptions.Value.TimeoutInSeconds
                    })
                    {
                        foreach (var pd in report.Parameters)
                        {
                            var sval = pd.IsHardcoded ? pd.HardCodedValue : paramValueByParamName[pd.SqlParameterName];
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
                        return new System.Net.Mail.Attachment(st, filename, MimeType.Application.SpreadSheet.Xlsx);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "error running report {name}", report.Name);
                throw;
            }
        }
    }
}
