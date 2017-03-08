namespace GSA.OpenItems.Web
{
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Security;
    using System.Security.Principal;
    using GSA.BESS.Web.Classes;


    public partial class Logon : PageBase
    {

        private UsersBO UsersBO;
        public Logon()
        {
            UsersBO = new UsersBO(this.Dal, new EmailsBO(this.Dal));
        }
        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            var browser = Request.Browser;
            var s = "Browser Capabilities\n"
            + "Type = " + browser.Type + "\n"
            + "Name = " + browser.Browser + "\n"
            + "Version = " + browser.Version + "\n"
            + "Major Version = " + browser.MajorVersion + "\n"
            + "Minor Version = " + browser.MinorVersion + "\n"
            + "Platform = " + browser.Platform + "\n"
            + "Is Beta = " + browser.Beta + "\n"
            + "Is Crawler = " + browser.Crawler + "\n"
            + "Is AOL = " + browser.AOL + "\n"
            + "Is Win16 = " + browser.Win16 + "\n"
            + "Is Win32 = " + browser.Win32 + "\n"
            + "Supports Frames = " + browser.Frames + "\n"
            + "Supports Tables = " + browser.Tables + "\n"
            + "Supports Cookies = " + browser.Cookies + "\n"
            + "Supports VBScript = " + browser.VBScript + "\n"
            + "Supports JavaScript = " +
                browser.EcmaScriptVersion.ToString() + "\n"
            + "Supports Java Applets = " + browser.JavaApplets + "\n"
            + "Supports ActiveX Controls = " + browser.ActiveXControls
                  + "\n"
            + "Supports JavaScript Version = " +
                browser["JavaScriptVersion"] + "\n";

            //lblBrowserTypeAlert.Text = s;
            var conn = ConfigurationManager.ConnectionStrings["DefaultConn"].ConnectionString;

            var slblEnvironmentIsActive = ConfigurationManager.AppSettings["PasswordMenuIsVisible"];

            var sDatabaseServer1 = ConfigurationManager.AppSettings["DatabaseServer1"];
            // for db server name alias:
            var sDatabaseServer2 = ConfigurationManager.AppSettings["DatabaseServer2"];

            var sDatabaseServerUAT = ConfigurationManager.AppSettings["DatabaseServerUAT"];

            var sProdWebServerName = ConfigurationManager.AppSettings["ProdWebServerName"];
            var sProdWebServerAlias = ConfigurationManager.AppSettings["ProdWebServerAlias"];


            var sServerName = Request.ServerVariables["SERVER_NAME"];
            Session["WebServer"] = sServerName;

            if (System.Web.HttpContext.Current.Session["WebServer"] != null)
            {
                sServerName = System.Web.HttpContext.Current.Session["WebServer"].ToString();
                var sSiteName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;


                if (sServerName.ToLower() == sProdWebServerName || sServerName.ToLower() == sProdWebServerAlias)
                {
                    Session["ApplUrl"] = "https://" + sServerName + "" + sSiteName;
                }
                else
                {
                    Session["ApplUrl"] = "http://" + sServerName + "" + sSiteName;
                }

                txtDBServerName.Value = Session["ApplUrl"].ToString() + ", DB: " + Mid(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConn"].ToString(), 14, 53) + ", Browser: " + browser.Browser + ", Browser Type: " + browser.Type;
            }


            var sApplicationDisabled = ConfigurationManager.AppSettings["ApplicationDisabled"];

            if (sApplicationDisabled.ToLower().Trim() == "true")
            {
                tr_disabled.Attributes.Add("style", "display:");
                lblDisabledMessage.Text = ConfigurationManager.AppSettings["DisabledMessage"];
                btnSubmit.Visible = false;
                tr_login.Visible = false;
                tr_pswd.Visible = false;
                td_warning.Visible = false;
                td_forgot_pswd.Visible = false;
            }
            else
            {
                tr_disabled.Attributes.Add("style", "display:none");
                lblDisabledMessage.Text = "";
                btnSubmit.Visible = true;
                tr_login.Visible = true;
                tr_pswd.Visible = true;
                td_warning.Visible = true;
                td_forgot_pswd.Visible = true;
            }



            if (!IsPostBack)
            {

                Session["cn"] = conn;

                if (browser.Browser != "IE" && browser.Browser != "InternetExplorer")
                {
                    lblEnvironment.Visible = true;
                    lblEnvironment.Text = "<br /><br /><br />The 'ULO' application is compatible with the Internet Explorer only.<br><br>Do not use Chrome or any other non-IE based browsers.<br><br><font color='Blue'>Please copy and paste the link below into the IE address field and click 'Enter'.</font><br>" + Session["ApplUrl"].ToString();
                    btnSubmit.Visible = false;
                    tr_login.Visible = false;
                    tr_pswd.Visible = false;
                    lblEnvironment.Font.Size = 16;
                    //txtUsername.Enabled = false;
                    //txtPassword.Enabled = false;
                    td_forgot_pswd.Visible = false;
                }
                else
                {
                    if (slblEnvironmentIsActive.ToLower().Trim() == "true")
                    {

                        if (conn.IndexOf(sDatabaseServer1) == -1) // not production environment
                        {
                            lblEnvironment.Visible = true;
                        }
                        else
                        {
                            lblEnvironment.Visible = false;
                        }
                    }
                    else
                    {
                        lblEnvironment.Visible = false;
                    }
                }





                if (Request.QueryString["_ST"] != null)
                    ClientScript.RegisterStartupScript(Page.GetType(), "ST",
                    "<script language='javascript'>setTimeout(\"alert2('Your session has expired!!')\",10);</script>");
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            btnSubmit.Click += new EventHandler(btnSubmit_Click);
        }


        void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                string sMyIndentity = WindowsIdentity.GetCurrent().Name.ToString();
                string myPCName = System.Net.Dns.GetHostName();

                string strUsername = txtUsername.Text.Trim();
                string strPassword = txtPassword.Text.Trim();

                string adPath = System.Configuration.ConfigurationManager.AppSettings["adPath"];//Path to NCR LDAP directory server
                string sDomainValue = System.Configuration.ConfigurationManager.AppSettings["DomainValue"];//ENT

                LdapAuthentication adAuth = new LdapAuthentication(adPath);
                string sEmail = "";

                if (true == adAuth.IsAuthenticated(sDomainValue, txtUsername.Text.Trim(), txtPassword.Text.Trim(), this.Page, true, out sEmail))
                {
                    if (sEmail.Trim() != "" && sEmail != null && sEmail != "0")
                    {
                        string sUserID = "0";

                        //UsersBO.Authenticate(strUsername, strPassword, out sUserID);

                        UsersBO.AuthenticateAfterLDAP(sEmail.Trim(), strUsername, out sUserID);

                        FormsAuthentication.Initialize();
                        // Create the authentication ticket
                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                            1, // Ticket version
                            strUsername, // Username to be associated with this ticket
                            DateTime.Now, // Date/time issued
                            DateTime.Now.AddMinutes(HttpContext.Current.Session.Timeout), // Date/time to expire
                            false, // "true" for a persistent user cookie (could be a checkbox on form)
                            CurrentUserRoles, // User-data (the roles from this user record in our database)
                            FormsAuthentication.FormsCookiePath); // Path cookie is valid for

                        // Hash the cookie for transport over the wire
                        string hash = FormsAuthentication.Encrypt(ticket);
                        HttpCookie cookie = new HttpCookie(
                        FormsAuthentication.FormsCookieName, // Name of auth cookie (it's the name specified in web.config)
                        hash); // Hashed ticket

                        // Add the cookie to the list for outbound response
                        Response.Cookies.Add(cookie);

                        // Redirect to requested URL, or homepage if no previous page requested
                        string returnUrl = Request.QueryString["ReturnUrl"];
                        if (returnUrl == null) returnUrl = "~/Default.aspx";

                        // Don't call the FormsAuthentication.RedirectFromLoginPage here, since it could
                        // replace the custom authentication ticket we just added...
                        Session["User_ID"] = sUserID;
                        Session["FullName"] = CurrentUserName;
                        Session["UserEnmail"] = sEmail.Trim();
                        Response.Redirect(returnUrl);
                    }
                }
                else
                {
                    throw new AppError("Login error: LDAP Authentication did not succeed. Check user name and password and try again.");
                }

            }
            catch (AppError err)
            {
                AddError(err);
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (ErrorCount > 0)
                {
                    lblError.Text = GetErrors();
                }
            }
        }


        // ********************** Below is old ULO authentication  ******************* //
        //void btnSubmit_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        string strUsername = txtUsername.Text.Trim();
        //        string strPassword = txtPassword.Text.Trim();

        //        string sMyIndentity = WindowsIdentity.GetCurrent().Name.ToString();
        //        string myPCName = System.Net.Dns.GetHostName();

        //        string sUserID = "0";

        //        UsersBO.Authenticate(strUsername, strPassword, out sUserID);

        //        FormsAuthentication.Initialize();
        //        // Create the authentication ticket
        //        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
        //            1, // Ticket version
        //            strUsername, // Username to be associated with this ticket
        //            DateTime.Now, // Date/time issued
        //            DateTime.Now.AddMinutes(HttpContext.Current.Session.Timeout), // Date/time to expire
        //            false, // "true" for a persistent user cookie (could be a checkbox on form)
        //            CurrentUserRoles, // User-data (the roles from this user record in our database)
        //            FormsAuthentication.FormsCookiePath); // Path cookie is valid for

        //        // Hash the cookie for transport over the wire
        //        string hash = FormsAuthentication.Encrypt(ticket);
        //        HttpCookie cookie = new HttpCookie(
        //        FormsAuthentication.FormsCookieName, // Name of auth cookie (it's the name specified in web.config)
        //        hash); // Hashed ticket

        //        // Add the cookie to the list for outbound response
        //        Response.Cookies.Add(cookie);

        //        // Redirect to requested URL, or homepage if no previous page requested
        //        string returnUrl = Request.QueryString["ReturnUrl"];
        //        if (returnUrl == null) returnUrl = "~/Default.aspx";

        //        // Don't call the FormsAuthentication.RedirectFromLoginPage here, since it could
        //        // replace the custom authentication ticket we just added...
        //        Session["User_ID"] = sUserID;
        //        Response.Redirect(returnUrl);

        //    }
        //    catch (AppError err)
        //    {
        //        AddError(err);
        //    }
        //    catch (Exception ex)
        //    {
        //        AddError(ex);
        //    }
        //    finally
        //    {
        //        if (ErrorCount > 0)
        //        {
        //            lblError.Text = GetErrors();
        //        }
        //    }
        //}


        public static string Mid(string s, int start, int length)
        {
            return s.Substring(start - 1, length);
        }


    }
}