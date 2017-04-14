using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class BackgroundTasks : IBackgroundTasks
    {
        private readonly IEmailServer EmailServer;
        private ULODBEntities DB;
        public BackgroundTasks(IEmailServer emailServer)
        {
            EmailServer = emailServer;
        }

        public void Email(string subject, string body, string recipient)
        {

            EmailServer.SendEmail(subject, body, recipient);
        }
    }
}