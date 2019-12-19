using System.Collections.Generic;
using System.Net.Mail;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IEmailServer
    {
        void SendEmail(string subject, string body, string bodyHtml, IEnumerable<string> recipients, IEnumerable<Attachment> attachments);
    }
}
