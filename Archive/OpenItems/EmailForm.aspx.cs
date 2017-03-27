using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Configuration;
    using System.Text;
    using System.Web;
    using System.Web.UI.HtmlControls;
    using GSA.OpenItems;
    using Data;

    public partial class EmailForm : PageBase
    {
        const string MODE_BY_USER_ID = "uid";
        const string MODE_SAMPLE_REVIEWER = "r";
        const string MODE_SAMPLE_CO = "co";
        const string MODE_SAMPLE_SME = "sme";

        private readonly DocumentBO Document;
        private readonly EmailsBO Email;
        private readonly UsersBO Users;
        public EmailForm()
        {
            Document = new DocumentBO(this.Dal);
            Email = new EmailsBO(this.Dal);
            Users = new UsersBO(this.Dal, Email);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                lblMessage.Text = "";
                RegisterClientSessionScript();

                if (!IsPostBack)
                {
                    EmailMode = MODE_BY_USER_ID;
                    var recipient_userid = 0;
                    if (Request.QueryString["mode"] != null && Request.QueryString["mode"] != "")
                        EmailMode = Request.QueryString["mode"];
                    if (Request.QueryString["uid"] != null && Request.QueryString["uid"] != "")
                        recipient_userid = Int32.Parse(Request.QueryString["uid"]);

                    InitFields(recipient_userid);
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
            sBuilder.Append(" try { ");
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
            sBuilder.Append(" } catch(e) {} ");
            sBuilder.Append("</script>");

            if (Page.User.Identity.Name.Trim().Length > 0)
                ClientScript.RegisterStartupScript(sBuilder.GetType(), "Timeout", sBuilder.ToString());
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            btnSendEmail.Click += new EventHandler(btnSendEmail_Click);
        }

        void btnSendEmail_Click(object sender, EventArgs e)
        {
                //send Email:
                string _to;
                var _cc = txtCc.Text;
                var _subject = txtSubject.Text;
                var _body = txtBody.Text;

                var _page = new PageBase();

                if (EmailMode == MODE_SAMPLE_CO)
                {
                    _to = txtTo.Text;
                    Email.SendEmail(_to, _cc, _subject, _body, DocsArray, _page.CurrentUserLogin);
                    if (DocsArray.Length > 0)
                        Document.UpdateSentAttachments(_page.DocNumber, DocsArray, _page.LoadID);
                    //insert History log
                    History.InsertHistoryOnSendDocumentsByEmail((int)HistoryActions.haSendEmailToCO, _page.DocNumber, _page.LoadID, DocsArray, _to, _subject, _body, _page.CurrentUserOrganization, _page.CurrentUserID);
                }
                else if (EmailMode == MODE_SAMPLE_SME)
                {
                    _to = ddlTo.SelectedItem.Value;
                    Email.SendEmail(_to, _cc, _subject, _body, DocsArray, _page.CurrentUserLogin);
                    if (DocsArray.Length > 0)
                        Document.UpdateSentAttForRevision(_page.DocNumber, DocsArray, _page.LoadID);
                    //insert History log
                    History.InsertHistoryOnSendDocumentsByEmail((int)HistoryActions.haSendEmailToSME, _page.DocNumber, _page.LoadID, DocsArray, _to, _subject, _body, _page.CurrentUserOrganization, _page.CurrentUserID);
                }
                else
                {
                    _to = txtTo.Text;
                    Email.SendEmail(_to, _cc, _subject, _body, _page.CurrentUserLogin);
                    //insert History log ? - not sure we need to log the simple email to reviewer...
                }

                //close window:
                var sb = new StringBuilder();
                if (EmailMode == MODE_SAMPLE_CO || EmailMode == MODE_SAMPLE_SME)
                {
                    sb.Append("<script type ='text/javascript' >");
                    sb.Append("opener.window_close();");                     
                    sb.Append("self.close();");
                    sb.Append("</script>");
                }
                else
                {
                    sb.Append("<script type ='text/javascript' >");
                    sb.Append("self.close();");
                    sb.Append("</script>");
                }
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "script", sb.ToString());                

        }

        private void InitFields(int RecipientUserID)
        {

                switch (EmailMode)
                {
                    case MODE_BY_USER_ID :
                        txtTo.Text = GetEmailAddress(RecipientUserID);
                        trAttachments.Visible = false;
                        trCheckbox.Visible = false;
                        break;

                    case MODE_SAMPLE_CO:
                        txtTo.Text = Settings.Default.Email_COEmailAddress;
                        txtSubject.Text = BuildCustomString(Settings.Default.Email_COSubject);
                        InitAttachmentsList();
                        break;

                    case MODE_SAMPLE_SME:
                        btnTo.Visible = false;
                        txtTo.Visible = false;
                        lblTo.Visible = true;
                        lblTo.Text = "Select SME:";
                        ddlTo.Visible = true;
                        ddlTo.DataSource = Users.GetUsersByRole(((int)UserRoles.urSME).ToString());
                        ddlTo.DataValueField = "Email";
                        ddlTo.DataTextField = "FullName";
                        ddlTo.DataBind();
                        txtSubject.Text = BuildCustomString(Settings.Default.Email_SMESubject);
                        InitAttachmentsList();
                        break;

                    case MODE_SAMPLE_REVIEWER:
                        txtTo.Text = GetEmailAddress(RecipientUserID);
                        var file_name = Server.MapPath("~") + Settings.Default.Email_ToReviewerTextFile;
                        txtSubject.Text = BuildCustomString(Utility.GetFileText(file_name, "SUBJECT"));
                        txtBody.Text = BuildCustomString(Utility.GetFileText(file_name, "BODY"));
                        trAttachments.Visible = false;
                        trCheckbox.Visible = false;
                        break;
                }

        }

        private void InitAttachmentsList()
        {
            // init attachments list by Email mode - for Central Office or for SME revision:

                var _page = new PageBase();
                var ds = Document.GetDocumentAttachments(Convert.ToInt32(_page.DocNumber), _page.LoadID.ToString());
                string field_to_search;
                if (EmailMode == MODE_SAMPLE_CO)
                    field_to_search = "IncludeInEmail";
                else if (EmailMode == MODE_SAMPLE_SME)
                    field_to_search = "IncludeRevisionEmail";
                else
                    return;
                var docs_array = ",";
                var doc_in_arr = "";
                var sb = new StringBuilder();
                var tr = new HtmlTableRow();
                HtmlTableCell td;
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    tblAtt.Rows.Add(tr);
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        doc_in_arr = dr["DocID"].ToString() + ",";
                        if ((bool)dr[field_to_search] && docs_array.IndexOf("," + doc_in_arr) == -1)
                        {
                            //add new file:
                            docs_array = docs_array + doc_in_arr;

                            tr = new HtmlTableRow();
                            tblAtt.Rows.Add(tr);
                            td = new HtmlTableCell();
                            td.AddBldGreyText();
                            td.InnerText = dr["FileName"].ToString();
                            tr.Cells.Add(td);

                            sb.Append("File: ");
                            sb.Append(dr["FileName"].ToString());
                            sb.AppendLine();
                            sb.Append("Documents included: ");
                            var document = new Document((int)dr["DocID"]);
                            sb.Append(String.Join(",", document.DocumentTypeName));                            
                            sb.AppendLine();
                            sb.Append("Associated Lines (ItemLNum) : ");
                            sb.Append(dr["LinesInfo"].ToString());
                            sb.AppendLine();
                            sb.Append("Comment: ");
                            sb.Append(dr["Comment"].ToString());
                            sb.AppendLine();                            
                        }
                    }

                    if (docs_array.Length > 1)
                        docs_array = docs_array.Substring(1, docs_array.Length - 2);
                    else
                        docs_array = "";

                    DocsArray = docs_array;
                    txtComments.Value = sb.ToString();
                }
                else
                {
                    trCheckbox.Visible = false;
                    DocsArray = "";
                }                                

        }

        private string BuildCustomString(string OriginalString)
        {
                var _page = new PageBase();
                var ds = _page.LoadList;
                var dr = ds.Tables[0].Select("LoadID = " + _page.LoadID.ToString());
                var load_date = String.Format("{0:MMM dd, yyyy}", dr[0]["LoadDate"]);

                var subject = OriginalString;
                subject = subject.Replace("[p_LoadDate]", load_date);
                subject = subject.Replace("[p_DocNumber]", _page.DocNumber);

                return subject;
        }

        private string GetEmailAddress(int UserID)
        {
            var email = "";

                var ds = Users.GetUserByUserID(UserID);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                    email = ds.Tables[0].Rows[0]["Email"].ToString();                

            return email;
        }

        private string DocsArray
        {
            get
            {
                if (ViewState["DOCS"] != null)
                    return (string)ViewState["DOCS"];
                else
                    return "";
            }
            set
            {
                ViewState["DOCS"] = value;
            }
        }

        private string EmailMode
        {
            get
            {
                if (ViewState["MODE"] != null)
                    return (string)ViewState["MODE"];
                else
                    return MODE_BY_USER_ID;
            }
            set
            {
                ViewState["MODE"] = value;
            }
        }
    }
}