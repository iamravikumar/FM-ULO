using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.Web.Services
{
    interface IBackgroundTasks
    {
        void Email(string subject, string body, string recipient);
    }
}
