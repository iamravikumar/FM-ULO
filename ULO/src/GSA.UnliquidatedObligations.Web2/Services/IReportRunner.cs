using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IReportRunner
    {
        Task<Attachment> ExecuteAsync(string reportName, IDictionary<string, string> paramValueByParamName);
    }
}
