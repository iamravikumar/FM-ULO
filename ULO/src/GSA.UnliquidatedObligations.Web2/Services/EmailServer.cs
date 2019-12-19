using RevolutionaryStuff.Core;
using Serilog;
using System.Collections.Generic;
using System.Net.Mail;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class EmailServer : IEmailServer
    {
        private readonly SmtpClient EmailClient;
        private readonly ILogger Log;

        public EmailServer(SmtpClient emailClient, ILogger log)
        {
            EmailClient = emailClient;
            Log = log.ForContext<EmailServer>();
        }

        public void SendEmail(string subject, string body, string bodyHtml, IEnumerable<string> recipients, IEnumerable<System.Net.Mail.Attachment> attachments)
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
                Log.Error("Email Server not properly configured.  Won't send {Subject} to {Recipients} with {AttachmentCount}", subject, recipients, mail.Attachments.Count);
            }
            else
            {
                EmailClient.Send(mail);
            }
        }
    }
}
