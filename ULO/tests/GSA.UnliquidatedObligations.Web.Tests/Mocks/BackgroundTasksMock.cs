using GSA.UnliquidatedObligations.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.Web.Tests.Mocks
{
    class BackgroundTasksMock : IBackgroundTasks
    {
        public void Email(string subject, string body, string recipient)
        {
           
        }
    }
}
