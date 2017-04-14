using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IEmailServer
    {
      void SendEmail(string subject, string body, string recipient);
    }
}