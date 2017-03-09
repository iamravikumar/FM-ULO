namespace GSA.OpenItems.Web
{
    using System;
    using System.Text;
    using System.Web;

    public partial class ReassignRequest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                RegisterClientSessionScript();

                if (!IsPostBack)
                {
                    if (Request.QueryString["item"] == null || Request.QueryString["item"] == "" ||
                        Request.QueryString["doc"] == null || Request.QueryString["doc"] == "" ||
                        Request.QueryString["org"] == null || Request.QueryString["org"] == "")
                    {
                        throw new AppError("Missing parameters. Please reload the page.");
                    }

                    var _item_id = Request.QueryString["item"];
                    var _doc_num = Request.QueryString["doc"];
                    var _org_code = Request.QueryString["org"];
                    string _lines;
                    if (Request.QueryString["lines"] == null || Request.QueryString["lines"] == "")
                        _lines = "0";
                    else
                        _lines = Request.QueryString["lines"];

                    InitControls(_item_id, _lines, _doc_num, _org_code);

                }
                else
                {
                    if (lblMessage.CssClass == "errorsum")
                    {
                        //'erase' error message
                        lblMessage.Visible = false;
                        lblMessage.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.CssClass = "errorsum";
                lblMessage.Text = "Error: " + ex.Message;
            }
        }

        private void RegisterClientSessionScript()
        {
            var sBuilder = new StringBuilder();

            sBuilder.Append("<script language='javascript'>");
            sBuilder.Append(" var open_items_app=true; ");
            sBuilder.Append(" var bln_close = false; ");
            sBuilder.Append("if (opener){if (opener.open_items_app){bln_close = true;}}");
            sBuilder.Append("if (bln_close){");
            sBuilder.Append("window.setTimeout(\"window.close();\", ");
            sBuilder.Append((HttpContext.Current.Session.Timeout * 60000).ToString());
            sBuilder.Append(", \"JavaScript\");");
            sBuilder.Append("}");
            sBuilder.Append("else{");
            sBuilder.Append("window.setTimeout(\"window.alert('Your session has expired !');window.location='");
            sBuilder.Append(Request.ApplicationPath);
            sBuilder.Append("/Default.aspx';\", ");
            sBuilder.Append((HttpContext.Current.Session.Timeout * 60000).ToString());
            sBuilder.Append(", \"JavaScript\");");
            sBuilder.Append("}");
            sBuilder.Append("</script>");

            if (Page.User.Identity.Name.Trim().Length > 0)
                ClientScript.RegisterStartupScript(sBuilder.GetType(), "Timeout", sBuilder.ToString());
        }


        protected void Page_Init(object sender, EventArgs e)
        {
            ctrlOrgUsers.OnError += new Controls_OrgUsers.OrgUsersErrEventHandler(ctrlOrgUsers_OnError);
        }

        void ctrlOrgUsers_OnError(object sender, OrgUsersErrEventArgs e)
        {
            lblMessage.Visible = true;
            lblMessage.CssClass = "errorsum";
            lblMessage.Text = "Error in application: " + e.ErrorMessage;
        }

        private void InitControls(string ItemID, string Lines, string DocNumber, string OrgCode)
        {

            lblDocNumber.Text = DocNumber;
            lblOrgCode.Text = OrgCode;
            txtItemID.Value = ItemID;
            if (Lines != "0")
                lblLines.Text = Lines;

            var OItemID = Int32.Parse(ItemID);

            ctrlOrgUsers.OrganizationLabel = "Suggested Organization:";
            ctrlOrgUsers.UsersLabel = "Suggested Reviewer:";
            ctrlOrgUsers.InitLists();
            ctrlOrgUsers.SelectedOrgCode = OrgCode;
            ctrlOrgUsers.SelectedUserID = (new PageBase()).CurrentUserID;

        }


    }
}