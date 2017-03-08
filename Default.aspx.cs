namespace GSA.OpenItems.Web
{
    using System;
    using System.Configuration;

    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if ((new PageBase()).CurrentUserDefaultApp == (int)ULOApplications.appOpenItems)
                Response.Redirect(ConfigurationManager.AppSettings["OpenItemsDefaultPage"]);
            else
                Response.Redirect(ConfigurationManager.AppSettings["FundStatusDefaultPage"]);
            //Response.Redirect("ServerNotAvailableErr.htm");
        }
    }
}