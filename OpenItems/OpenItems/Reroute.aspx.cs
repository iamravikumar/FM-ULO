namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Text;
    using System.Web;

    public partial class Reroute : PageBase
    {
        private readonly AssignBO Assign;
        private readonly ItemBO Item;
        public Reroute()
        {
            Assign = new AssignBO(this.Dal, Item);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                RegisterClientSessionScript();

                if (!IsPostBack)
                {
                    if (Request.QueryString["request"] != null && Request.QueryString["request"] != "")
                    {
                        InitControlsByRequest(Int32.Parse(Request.QueryString["request"]));
                    }
                    else if (Request.QueryString["group"] != null && Request.QueryString["group"] != "")
                    {
                        InitControlsForGroupAssign(Request.QueryString["group"]);
                    }
                    else if (Request.QueryString["item"] != null && Request.QueryString["item"] != "" &&
                        Request.QueryString["org"] != null && Request.QueryString["org"] != "" &&
                        Request.QueryString["user"] != null && Request.QueryString["user"] != "")
                    {
                        var lines = "";
                        if (Request.QueryString["lines"] != null && Request.QueryString["lines"] != "")
                            lines = Request.QueryString["lines"];
                        //if (lines == "" || lines == "0")
                        //    lines = "all";

                        InitControlsByItem(Int32.Parse(Request.QueryString["item"]), lines, Request.QueryString["org"], Request.QueryString["user"]);
                    }
                    else
                        throw new Exception("Missing parameters. Please reload the page.");

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
                lblMessage.Text = "Error in application: " + ex.Message;
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

        private void InitControlsForGroupAssign(string ItemsArray)
        {
            tblLabels.Visible = false;
            lblGroupAssign.Visible = true;
            txtGroupAssign.Value = ItemsArray;

            ctrlOrgUsers.OrganizationLabel = "Select Organization:";
            ctrlOrgUsers.UsersLabel = "Select Reviewer:";
            ctrlOrgUsers.InitLists();
        }

        private void InitControlsByItem(int OItemID, string Lines, string ULOOrgCode, string PrevReviewer)
        {

            txtItemID.Value = OItemID.ToString();

            if (Lines == "" || Lines == "0")
            {
                //init by item properties:
                lblLines.Text = "all";

                int reviewer_id;
                if (PrevReviewer == "" || PrevReviewer == "0")
                    reviewer_id = 0;
                else
                    reviewer_id = Int32.Parse(PrevReviewer);
                var item = new OpenItem(OItemID, (new PageBase()).LoadID, ULOOrgCode, reviewer_id);

                txtPrevOrganization.Value = item.Organization + " : " + item.ULOOrgCode;
                txtPrevReviewer.Value = item.ReviewerUserID.ToString();
                lblDocNumber.Text = item.DocNumber;

                lblPrevOrganization.Text = item.Organization;
                lblPrevOrgCode.Text = item.ULOOrgCode;
                lblPrevReviewerName.Text = item.Reviewer;
            }
            else
            {
                //init by lines properties:
                lblLines.Text = Lines;

                var line_num = 0;
                Int32.TryParse(Lines, out line_num);
                if (line_num == 0)
                {
                    //we have multiline selection:
                    lblDocNumber.Text = Request.QueryString["doc"];

                    var str_org = "";
                    var str_names = "";
                    var ds = Item.GetULOOrganizationsByItemLines(OItemID, Lines);

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if (str_org.IndexOf((string)dr["Organization"]) == -1)
                            str_org = str_org + (string)dr["Organization"] + ", ";

                        if (str_names.IndexOf((string)dr["ReviewerName"]) == -1)
                            str_names = str_names + (string)dr["ReviewerName"] + "<br/>";
                    }

                    lblPrevOrganization.Text = str_org.Substring(0, str_org.Length - 2);
                    lblPrevOrgCode.Style.Add("display", "none");
                    lblPrevReviewerName.Text = str_names;
                }
                else
                {
                    //one line selected
                    var line = new LineNum(OItemID, line_num);

                    txtPrevOrganization.Value = line.Organization + " : " + line.ULOOrgCode;
                    txtPrevReviewer.Value = line.ReviewerUserID.ToString();
                    lblDocNumber.Text = line.DocNumber;

                    lblPrevOrganization.Text = line.Organization;
                    lblPrevOrgCode.Text = line.ULOOrgCode;
                    lblPrevReviewerName.Text = line.Reviewer;
                }
            }

            ctrlOrgUsers.OrganizationLabel = "Select Organization:";
            ctrlOrgUsers.UsersLabel = "Select Reviewer:";
            ctrlOrgUsers.InitLists();
        }

        private void InitControlsByRequest(int RequestID)
        {
            var dr = Assign.GetRerouteRequestProperties(RequestID);
            if (dr == null)
                throw new ApplicationException("There is a problem to locate current request. Please reload the page.");

            txtRequestID.Value = RequestID.ToString();
            txtPrevOrganization.Value = (string)Utility.GetNotNullValue(dr["ResponsibleOrg"], "String") + " : " + (string)Utility.GetNotNullValue(dr["ULOOrgCode"], "String");
            txtPrevReviewer.Value = ((int)Utility.GetNotNullValue(dr["ReviewerUserID"], "Int")).ToString();

            lblDocNumber.Text = (string)dr["DocNumber"];
            var lines = (int)dr["ItemLNum"];
            if (lines == 0)
                lblLines.Text = "all";
            else
                lblLines.Text = lines.ToString();
            lblPrevOrganization.Text = (string)Utility.GetNotNullValue(dr["ResponsibleOrg"], "String");
            lblPrevOrgCode.Text = (string)Utility.GetNotNullValue(dr["ULOOrgCode"], "String");
            lblPrevReviewerName.Text = (string)Utility.GetNotNullValue(dr["ReviewerName"], "String");
            txtComment.Value = (string)Utility.GetNotNullValue(dr["Comments"], "String");

            var intSelectedUserID = (int)Utility.GetNotNullValue(dr["RerouteUserID"], "Int");
            var strSelectedOrg = (string)Utility.GetNotNullValue(dr["RerouteOrgCode"], "String");

            ctrlOrgUsers.OrganizationLabel = "Suggested Organization:";
            ctrlOrgUsers.UsersLabel = "Suggested Reviewer:";
            ctrlOrgUsers.InitLists();

            ctrlOrgUsers.SelectedOrgCode = strSelectedOrg;
            ctrlOrgUsers.SelectedUserID = intSelectedUserID;
        }
    }
}