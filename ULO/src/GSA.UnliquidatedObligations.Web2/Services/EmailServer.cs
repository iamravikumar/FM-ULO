using System.Collections.Generic;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class EmailServer : BaseLoggingDisposable, IEmailServer
    {
        private readonly SmtpClient EmailClient;

        public EmailServer(SmtpClient emailClient, ILogger<EmailServer> logger)
            : base(logger)
        {
            EmailClient = emailClient;
        }

        public void SendEmail(string subject, string body, string bodyHtml, IEnumerable<string> recipients, IEnumerable<Attachment> attachments)
        {
            Requires.NonNull(recipients, nameof(recipients));

            bodyHtml = StringHelpers.TrimOrNull(bodyHtml);

            var mail = new MailMessage();
            foreach (var recipient in recipients)
            {
                Requires.EmailAddress(recipient, nameof(recipient));
                mail.To.Add(new MailAddress(recipient));
            }
            mail.Subject = subject;
            mail.Body = bodyHtml ?? body;
            mail.IsBodyHtml = bodyHtml != null;
            if (attachments != null)
            {
                foreach (var a in attachments)
                {
                    mail.Attachments.Add(a);
                }
            }
            if (EmailClient.Host == null)
            {
                LogError("Email Server not properly configured.  Won't send {Subject} to {Recipients} with {AttachmentCount}", subject, recipients, mail.Attachments.Count);
            }
            else
            {
                EmailClient.Send(mail);
            }
        }
    }
}
