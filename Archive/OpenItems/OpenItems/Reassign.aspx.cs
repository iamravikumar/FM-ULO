namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Text;
    using System.Web;
    using System.Web.UI.WebControls;

    public partial class Reassign : PageBase
    {
        private readonly AdminBO Admin;
        private readonly AssignBO Assign;
        public readonly ItemBO Item;
        public readonly Lookups Lookups;
        public Reassign()
        {
            Admin = new AdminBO(this.Dal);
            Item = new ItemBO(this.Dal);
            Assign = new AssignBO(this.Dal, Item);
            Lookups = new Lookups(this.Dal, Admin);
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
                        //InitControlsForGroupAssign(Request.QueryString["group"]);
                        //SM_
                        if (Request.QueryString["org_code"] != null && Request.QueryString["org_code"] != "")
                        {
                            // txtOrgCode.Value = Request.QueryString["org_code"].ToString();
                            InitControlsForGroupAssign(Request.QueryString["group"], Request.QueryString["org_code"].ToString());
                        }
                    }
                    else if (Request.QueryString["item"] != null && Request.QueryString["item"] != "" &&
                        Request.QueryString["org"] != null && Request.QueryString["org"] != "" &&
                        Request.QueryString["user"] != null && Request.QueryString["user"] != "")
                    {
                        var lines = "";
                        if (Request.QueryString["lines"] != null && Request.QueryString["lines"] != "")
                            lines = Request.QueryString["lines"];

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

        private void InitControlsForGroupAssign(string ItemsArray, string sOrgCode)
        {
            //group assignment is allowed for reassign only, so disable all reroute controls:
            lblRerouteLabel.Visible = false;
            lblRerouteMsg.Visible = false;
            ctrlOrgUsers.Visible = false;
            tblRerouteCtrls.Visible = false;
            //SM_
            lblOrgCode.Visible = true;

            //init:
            tblLabels.Visible = false;
            lblGroupAssign.Visible = true;
            txtGroupAssign.Value = ItemsArray;
            var sOrgName = Admin.GetOrganizationNameByOrgCode(sOrgCode);
            txtOrgCode.Value = sOrgName + " : " + sOrgCode;

            var ds = Assign.GetUsersOrganizationsList();
            var drows = ds.Tables[0].Select("Organization = '" + (new PageBase()).CurrentUserOrganization + "'");
            var col = new ListItemCollection();
            foreach (var dr in drows)
            {
                var it = new ListItem(dr["UserName"].ToString(), dr["UserID"].ToString());
                col.Add(it);
            }
            ddlAssign.DataSource = col;
            ddlAssign.DataValueField = "Value";
            ddlAssign.DataTextField = "Text";
            ddlAssign.DataBind();
            //lblOrgCode.Text = 
        }

        private void InitControlsByRequest(int RequestID)
        {
            var dr = Assign.GetRerouteRequestProperties(RequestID);
            if (dr == null)
                throw new ApplicationException("There is a problem to locate current request. Please reload the page.");

            txtRequestID.Value = RequestID.ToString();
            txtCurrentReviewer.Value = ((int)Utility.GetNotNullValue(dr["ReviewerUserID"], "Int")).ToString();

            lblDocNumber.Text = (string)dr["DocNumber"];
            var lines = (int)dr["ItemLNum"];
            txtLines.Value = lines.ToString();
            if (lines == 0)
                lblLines.Text = "all";
            else
                lblLines.Text = lines.ToString();

            var intItemID = (int)dr["OItemID"];
            txtItemID.Value = intItemID.ToString();
            var strOrgCode = (string)Utility.GetNotNullValue(dr["ULOOrgCode"], "String");
            lblOrgCode.Text = strOrgCode;
            lblPrevReviewer.Text = (string)Utility.GetNotNullValue(dr["ReviewerName"], "String");

            var intSelectedUserID = (int)Utility.GetNotNullValue(dr["RerouteUserID"], "Int");
            var strSelectedOrg = (string)Utility.GetNotNullValue(dr["RerouteOrgCode"], "String");

            if ((string)Utility.GetNotNullValue(dr["Responsibility"], "String") == Lookups.GetOrganizationByOrgCode(strOrgCode))
            {
                //if reroute requested for the same Organization, init fields for Assign:
                txtCommentAssign.Value = (string)Utility.GetNotNullValue(dr["Comments"], "String");
                InitAssignUsersList(intItemID, strOrgCode, intSelectedUserID);
                InitUsersOrganizationsList("", 0);
            }
            else
            {
                //if reroute requested for the other Organization, init fields for Reroute Request to BD:
                txtCommentReroute.Value = (string)Utility.GetNotNullValue(dr["Comments"], "String");
                InitAssignUsersList(intItemID, strOrgCode, 0);
                InitUsersOrganizationsList(strSelectedOrg, intSelectedUserID);
            }
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

                txtCurrentReviewer.Value = item.ReviewerUserID.ToString();
                lblPrevReviewer.Text = item.Reviewer;
                lblDocNumber.Text = item.DocNumber;
                lblOrgCode.Text = item.ULOOrgCode;

                InitAssignUsersList(OItemID, ULOOrgCode, 0);
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
                        if (str_org.IndexOf((string)dr["ULOOrgCode"]) == -1)
                            str_org = str_org + (string)dr["ULOOrgCode"] + ", ";

                        if (str_names.IndexOf((string)dr["ReviewerName"]) == -1)
                            str_names = str_names + (string)dr["ReviewerName"] + "<br/>";
                    }

                    lblOrgCode.Text = str_org.Substring(0, str_org.Length - 2);
                    lblPrevReviewer.Text = str_names;

                    InitAssignUsersList(OItemID, ULOOrgCode, 0);
                }
                else
                {
                    //one line selected
                    var line = new LineNum(OItemID, line_num);

                    txtCurrentReviewer.Value = line.ReviewerUserID.ToString();
                    lblPrevReviewer.Text = line.Reviewer;
                    lblDocNumber.Text = line.DocNumber;
                    lblOrgCode.Text = line.ULOOrgCode;

                    InitAssignUsersList(OItemID, line.ULOOrgCode, 0);
                }
            }

            //init with default settings:
            InitUsersOrganizationsList("", 0);

        }

        private void InitAssignUsersList(int OItemID, string ULOOrgCode, int SelectUserID)
        {
            var dsAssign = Assign.GetAssignUsers(OItemID, ULOOrgCode);
            if (dsAssign == null || dsAssign.Tables[0].Rows.Count == 0)
            {
                lblMessage.Visible = true;
                lblMessage.CssClass = "errorsum";
                lblMessage.Text = "There is no reviewer found for the current OrgCode: " + ULOOrgCode + ". Please contact System Administrator.";
                btnAssign.Disabled = true;
            }
            else
            {
                //init ddlAssign:
                var dt = dsAssign.Tables[0];
                var dr = dt.NewRow();
                dr["UserID"] = 0;
                dr["UserName"] = "";
                dt.Rows.InsertAt(dr, 0);
                ddlAssign.DataSource = dt;
                ddlAssign.DataValueField = "UserID";
                ddlAssign.DataTextField = "UserName";
                ddlAssign.DataBind();

                if (SelectUserID != 0 && dsAssign.Tables[0].Rows.Count > 0)
                    ddlAssign.SelectedValue = SelectUserID.ToString();
            }
        }

        private void InitUsersOrganizationsList(string SuggestedOrgCode, int SuggestedReviewerID)
        {
            ctrlOrgUsers.OrganizationLabel = "Suggested Organization:";
            ctrlOrgUsers.UsersLabel = "Suggested Reviewer:";
            ctrlOrgUsers.InitLists();
            if (SuggestedOrgCode != "")
                ctrlOrgUsers.SelectedOrgCode = SuggestedOrgCode;
            if (SuggestedReviewerID != 0)
                ctrlOrgUsers.SelectedUserID = SuggestedReviewerID;
        }
    }
}