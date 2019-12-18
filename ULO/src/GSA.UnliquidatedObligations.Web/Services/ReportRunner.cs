using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using GSA.UnliquidatedObligations.Utility;
using System.Web.Mvc;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class ReportRunner : IReportRunner
    {
        private readonly ILogger Logger;

        public ReportRunner(ILogger logger)
        {
            this.Logger = logger;
        }

        async Task<System.Net.Mail.Attachment> IReportRunner.ExecuteAsync(string reportName, IDictionary<string, string> paramValueByParamName)
        {
            var report = (await PortalHelpers.GetReportsAsync(reportName)).FirstOrDefault();
            using (var conn = new SqlConnection(PortalHelpers.DefaultUloConnectionString))
            {
                Log.Information("Executing report {ReportName} with {SprocSchema}.{SprocName}", reportName, report.SprocSchema, report.SprocName);
                await conn.OpenAsync();
                using (var cmd = new SqlCommand($"[{report.SprocSchema}].[{report.SprocName}]", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 60 * 60
                })
                {
                    foreach (var pd in report.Parameters)
                    {
                        var sval = pd.IsHardcoded ? pd.HardCodedValue : paramValueByParamName[pd.SqlParameterName];
                        var oval = Convert.ChangeType(sval, pd.ClrType);
                        cmd.Parameters.Add(new SqlParameter("@" + pd.SqlParameterName, oval));
                    }
                    var ds = cmd.ExecuteReadDataSet();
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
    }
}
