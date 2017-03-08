using System;

public partial class ReviewOpenItems : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.Redirect("~/OpenItems/ReviewOpenItems.aspx");
    }
}
