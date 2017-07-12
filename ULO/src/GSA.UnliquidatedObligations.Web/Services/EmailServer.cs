using System.Net.Mail;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class EmailServer : IEmailServer
    {
        private readonly SmtpClient EmailClient;

        public EmailServer(SmtpClient emailClient)
        {
            EmailClient = emailClient;
        }

        public void SendEmail(string subject, string body, string recipient)
        {
            var mail = new MailMessage();
            mail.To.Add(new MailAddress(recipient));
            mail.Subject = subject;
            mail.Body = body;

            EmailClient.Send(mail);
        }
    }
}