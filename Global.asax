<%@ Application Language="C#" %>

<script runat="server">
    
    void Application_Start(object sender, EventArgs e) 
    {
        // Code that runs on application startup

    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

    }
        
    void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }

    protected void Application_AuthenticateRequest(Object sender, EventArgs e)
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

    protected void Application_Error(Object sender, EventArgs e)
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
       
</script>


