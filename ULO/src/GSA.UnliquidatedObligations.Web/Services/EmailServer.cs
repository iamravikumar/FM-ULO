using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

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