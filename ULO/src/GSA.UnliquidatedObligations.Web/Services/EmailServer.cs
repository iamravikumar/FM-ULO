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
            var mail = new MailMessage("system@test.com", recipient, subject, body);
            EmailClient.Send(mail);
        }
    }
}