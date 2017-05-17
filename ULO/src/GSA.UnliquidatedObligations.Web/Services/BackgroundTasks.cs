using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RazorEngine;
using RazorEngine.Templating;
using System.Linq;
namespace GSA.UnliquidatedObligations.Web.Services
{
    public class BackgroundTasks : IBackgroundTasks
    {
        private readonly IEmailServer EmailServer;
        public BackgroundTasks(IEmailServer emailServer)
        {
            EmailServer = emailServer;
        }

        public void Email(string subject, string recipient, string template, object model)
        {
            var compiledEmailBody = Engine.Razor.RunCompile(template, "email", null, model);
            EmailServer.SendEmail(subject, compiledEmailBody, recipient);
        }
    }
}
