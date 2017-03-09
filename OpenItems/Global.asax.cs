using System;
using System.Web;
using System.Web.Security;

namespace OpenItems
{
    public class Global : HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            // Extract the forms authentication cookie
            var cookieName = FormsAuthentication.FormsCookieName;
            var authCookie = HttpContext.Current.Request.Cookies[cookieName];

            if (null == authCookie)
            {
                // There is no authentication cookie.
                return;
            }

            FormsAuthenticationTicket authTicket = null;

            authTicket = FormsAuthentication.Decrypt(authCookie.Value);

            if (null == authTicket)
            {
                // Cookie failed to decrypt.
                return;
            }

            // When the ticket was created, the UserData property was assigned a
            // pipe delimited string of role names from the database.
            var roles = authTicket.UserData.Split(new char[] { '|' });

            // Create an Identity object
            var id = new FormsIdentity(authTicket);

            // Attach the new principal object to the current HttpContext object
            // This principal will flow throughout the request.
            HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(id, roles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            //get reference to the source of the exception chain
            var ex = Server.GetLastError().GetBaseException();

            //log the details of the exception and page state to the Log file        
            GSA.OpenItems.Utility.WriteLogEntry("DATE AND TIME: " + DateTime.Now.ToString());
            GSA.OpenItems.Utility.WriteLogEntry("MESSAGE: " + ex.Message);
            GSA.OpenItems.Utility.WriteLogEntry("SOURCE: " + ex.Source);
            GSA.OpenItems.Utility.WriteLogEntry("FORM: " + Request.Form.ToString());
            GSA.OpenItems.Utility.WriteLogEntry("QUERYSTRING: " + Request.QueryString.ToString());
            GSA.OpenItems.Utility.WriteLogEntry("TARGETSITE: " + ex.TargetSite);
            GSA.OpenItems.Utility.WriteLogEntry("STACKTRACE: " + ex.StackTrace);
            GSA.OpenItems.Utility.WriteLogEntry("");

            //send an email notification to administrator
            new GSA.OpenItems.EmailUtility().SendAlertToSysAdmin("ULO/FundStatus application alert",
                String.Format("An error has occurred in ULO/FundStatus application. Form: {0}. Source: {1}. Error: {2}", Request.Form.ToString(), ex.Source, ex.Message));
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}