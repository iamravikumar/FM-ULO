namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web;
    using System.Text;
    using System.Web.UI.WebControls;

    public partial class SearchPerson : PageBase
    {
        const string SEARCH_CONTACT = "contact";
        const string SEARCH_EMAIL = "email";

        private readonly Lookups LookupsBO;
        private readonly UsersBO Users;

        public SearchPerson()
        {
            LookupsBO = new Lookups(this.Dal, new AdminBO(this.Dal));
            Users = new UsersBO(this.Dal, new EmailsBO(this.Dal));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(hdnShow.Value=="0")
            {
                txtLastName.AddDisplayNone();
                txtFirstName.AddDisplayNone();
                btnSearch.AddDisplayNone();
                note.AddDisplayNone();
                td_fn.AddDisplayNone();
                td_ln.AddDisplayNone();
                grid_row.AddDisplayNone();
            }
            else
            {
                txtLastName.AddDisplay();
                txtFirstName.AddDisplay();
                btnSearch.AddDisplay();
                note.AddDisplay();
                td_fn.AddDisplay();
                td_ln.AddDisplay();
                grid_row.AddDisplay();
            }

            lblInfo.Text = "To add the contact please double click on row below. Otherwise click 'Cancel'.";

            try
            {
                RegisterClientSessionScript();

                if (!IsPostBack)
                {
                    if (Request.QueryString["mode"] != null)
                        SearchMode = Request.QueryString["mode"];

                    if (SearchMode == SEARCH_CONTACT)
                    {
                        if (Request.QueryString["item"] != null)
                            txtItem.Value = Request.QueryString["item"];
                        if (Request.QueryString["doc"] != null)
                            txtDoc.Value = Request.QueryString["doc"];
                        if (Request.QueryString["org"] != null)
                            txtOrg.Value = Request.QueryString["org"];

                        var dv = LookupsBO.GetContactsRoleList();
                        ddlRoles.DataSource = dv;
                        ddlRoles.DataValueField = "RoleCode";
                        ddlRoles.DataTextField = "RoleDescription";
                        ddlRoles.DataBind();
                    }
                    if (SearchMode == SEARCH_EMAIL)
                    {
                        lblRole.Visible = false;
                        ddlRoles.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                lblInfo.CssClass = "errorsum";
                lblInfo.Text = ex.Message;
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
            btnSearch.Click += new EventHandler(btnSearch_Click);
            gvPersons.RowDataBound += new GridViewRowEventHandler(gvPersons_RowDataBound);
        }

        void gvPersons_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (SearchMode == SEARCH_CONTACT)
                {
                    string phone;
                    var name = (string)((DataRowView)e.Row.DataItem)[0];
                    name = name.Replace("'", "");
                    var id = (int)((DataRowView)e.Row.DataItem)[2];
                    if (((DataRowView)e.Row.DataItem)[3] != DBNull.Value)
                        phone = ((string)((DataRowView)e.Row.DataItem)[3]).Replace("'", "");
                    else
                        phone = "";

                    e.Row.AddOnDblClick("javascript:on_select(" + id.ToString() + ",'" + name + "','" + phone + "');");
                }
                if (SearchMode == SEARCH_EMAIL)
                {
                    if (((DataRowView)e.Row.DataItem)[4] != DBNull.Value)                                            
                        e.Row.AddOnDblClick("javascript:on_email_select('" + ((DataRowView)e.Row.DataItem)[4].ToString() + "');");                    
                }
            }
        }

        void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                lblInfo.CssClass = "brown_text";
                lblNoteTop.Text = "Please enter last name and/or first name and then click on 'Search' button";
                var strFirst = txtFirstName.Text.Trim();
                var strLast = txtLastName.Text.Trim();
                if (strFirst == "" && strLast == "")
                {
                    throw new Exception("Please initialize your search criteria (at least one first letter).");
                }

                var ds = Users.SearchPersonnel(strFirst, strLast);
                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    lblInfo.CssClass = "errorsum";
                    lblInfo.Text = "No records found for the selection criteria";
                }
                else
                {
                    gvPersons.DataSource = ds.Tables[0].DefaultView;
                    gvPersons.DataBind();
                    lblInfo.CssClass = "brown_text";
                    lblInfo.Text = "To add the contact please double click on row below. Otherwise click 'Cancel'.";

                    //if (SearchMode == SEARCH_CONTACT)
                    //    //lblInfo.Text = "To add contact please double click on row below. Otherwise click 'Cancel'.";
                    //else
                    //    //lblInfo.Text = "Please enter last name or first name and then double click on 'Search' button.";
                }
            }
            catch (Exception ex)
            {
                lblInfo.CssClass = "errorsum";

                lblInfo.Text = ex.Message;
            }
        }

        private string SearchMode
        {
            get
            {
                if (ViewState["MODE"] == null || ViewState["MODE"] == "")
                    ViewState["MODE"] = SEARCH_CONTACT;

                return (string)ViewState["MODE"];                
            }
            set
            {
                ViewState["MODE"] = value;
            }
        }
    }
}