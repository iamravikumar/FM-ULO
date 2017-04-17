using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RazorEngine;
using RazorEngine.Templating;
using System.Linq;
namespace GSA.UnliquidatedObligations.Web.Services
{
    public class BackgroundTasks : IBackgroundTasks
    {
        private readonly IEmailServer EmailServer;
        private ULODBEntities DB;
        public BackgroundTasks(IEmailServer emailServer, ULODBEntities db)
        {
            EmailServer = emailServer;
            DB = db;
        }

        public void Email(string subject, string recipient, string template, object model)
        {
            
            var compiledEmailBody = Engine.Razor.RunCompile(template, "email", null, model);
            EmailServer.SendEmail(subject, compiledEmailBody, recipient);
        }
    }
}
