namespace GSA.OpenItems.Web
{
    using System;

    public partial class Logoff : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.Security.FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
        }
    }
}