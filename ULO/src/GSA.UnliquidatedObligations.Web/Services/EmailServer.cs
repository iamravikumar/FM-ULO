using RevolutionaryStuff.Core;
using Serilog;
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

        public void SendEmail(string subject, string body, string bodyHtml, string recipient)
        {
            Requires.EmailAddress(recipient, nameof(recipient));

            bodyHtml = StringHelpers.TrimOrNull(bodyHtml);

            var mail = new MailMessage();
            mail.To.Add(new MailAddress(recipient));
            mail.Subject = subject;
            mail.Body = bodyHtml ?? body;
            mail.IsBodyHtml = bodyHtml != null;
            if (EmailClient.Host == null)
            {
                Log.Error("Email Server not properly configured.  Wont send {Subject} to {Recipient}", subject, recipient);
            }
            else
            {
                EmailClient.Send(mail);
            }
        }
    }
}