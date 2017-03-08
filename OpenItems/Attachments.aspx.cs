namespace GSA.OpenItems.Web
{
    using System;
    using System.IO;
    using System.Data;
    using System.Text;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;
    using GSA.OpenItems;

    public partial class Attachments : PageBase
    {
        private readonly Lookups LookupsBO;
        private readonly DocumentBO Document;
        public Attachments()
        {
            LookupsBO = new Lookups(this.Dal, new AdminBO(this.Dal));
            Document = new DocumentBO(this.Dal);
        }
        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {
                //first clear Message label
                lblMessage.Visible = false;
                lblMessage.Text = "";

                if (!IsPostBack)
                {
                    lblMessage.Visible = false;

                    if (Request.QueryString["doc"] != null && Request.QueryString["doc"] != "")
                        DocNumber = Request.QueryString["doc"];

                    lblDocNumber.Text = DocNumber;

                    var ds = Document.GetOrgListByDocNumber(LoadID, DocNumber);
                    CanBeUpdated = AttachmentsAvailableForUpdate(ds.Tables[0]);

                    InitReviewersInfo(ds.Tables[0], CanBeUpdated);

                    InitAttachmentsRecommendation();

                    //init lstDocTypes
                    lstDocTypes.DataSource = LookupsBO.GetDocTypesList();
                    lstDocTypes.DataValueField = "DocTypeCode";
                    lstDocTypes.DataTextField = "DocTypeName";
                    lstDocTypes.DataBind();

                    NewDocument = false;

                    if (CanBeUpdated && Request.QueryString["edit"] != null && Request.QueryString["edit"] != "")
                    {
                        txtDocID.Value = Request.QueryString["edit"];
                        //NewDocument = false;
                        EnableDocProperties(true);
                        InitDocProperties(Int32.Parse(txtDocID.Value));
                        btnDelete.Attributes.Add("onclick", "javascript:return delete_att();");
                    }
                    else
                        EnableDocProperties(false);

                }

                if (!CanBeUpdated)
                {
                    gvDocs.Columns[0].Visible = false;
                    ctrlUpload.Visible = false;
                    btnUpload.Visible = false;
                }

                if (!DisplaySendProperties)
                {
                    gvDocs.Columns[1].Visible = false;
                    gvDocs.Columns[2].Visible = false;
                    gvDocs.Columns[3].Visible = false;
                    gvDocs.Columns[4].Visible = false;
                    gvDocs.Columns[5].Visible = false;
                    lblEmailDateLabel.Visible = false;
                    lblEmailDate.Visible = false;
                }

                if (DisplaySendProperties && User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) && !LoadIsArchived)
                {
                    //only BD Admin has permission to send documents to Central Office:
                    btnSendCOEmail.Disabled = false;
                    btnSendCOEmail.Visible = true;
                    btnSendSMEEmail.Disabled = false;
                    btnSendSMEEmail.Visible = true;
                }
                else
                {
                    btnSendCOEmail.Disabled = true;
                    btnSendCOEmail.Visible = false;
                    btnSendSMEEmail.Disabled = true;
                    btnSendSMEEmail.Visible = false;
                }

                if (DisplaySendProperties && !User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                {
                    gvDocs.Columns[3].Visible = false;
                    gvDocs.Columns[4].Visible = false;
                }

                gvDocs.DataSource = AttachmentsDataTable;
                gvDocs.DataBind();

            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                {
                    lblMessage.Visible = true;
                    lblMessage.CssClass = "errorsum";
                    lblMessage.Text = GetErrors();
                }
            }

        }

        protected void Page_Init(object sender, System.EventArgs e)
        {
            btnUpload.Click += new EventHandler(btnUpload_Click);
            gvDocs.RowDataBound += new GridViewRowEventHandler(gvDocs_RowDataBound);
            btnSave.Click += new EventHandler(btnSave_Click);
            btnDelete.Click += new EventHandler(btnDelete_Click);
            btnCancel.Click += new EventHandler(btnCancel_Click);
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                NewDocument = false;
                EnableDocProperties(false);
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                {
                    lblMessage.Visible = true;
                    lblMessage.CssClass = "errorsum";
                    lblMessage.Text = GetErrors();
                }
            }
        }

        void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                var doc_id = Int32.Parse(txtDocID.Value);

                Document.DeleteAttachment(DocNumber, doc_id, CurrentUserID);

                History.InsertHistoryOnAttachmentActivity((int)HistoryActions.haDeleteAttachment, LoadID, CurrentUserOrganization, DocNumber,
                    txtComments.Text, txtLineNum.Text, doc_id, lstDocTypes.SelectedItem.Text, lblFileName.Text, CurrentUserID);

                AttachmentsDataTable = null;
                gvDocs.DataSource = AttachmentsDataTable;
                gvDocs.DataBind();

                lblFileName.Text = "";
                lblUploadDate.Text = "";
                lblUploadUser.Text = "";
                lblLineNum.Text = "";
                lblDocType.Text = "";
                lblComments.Text = "";
                NewDocument = false;
                EnableDocProperties(false);

                RefreshDocAdministrationList();
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                {
                    lblMessage.Visible = true;
                    lblMessage.CssClass = "errorsum";
                    lblMessage.Text = GetErrors();
                }
            }
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var doc_id = Int32.Parse(txtDocID.Value);
                //int doc_type_code = Int32.Parse(lstDocTypes.SelectedValue);
                var lines = txtLineNum.Text.Trim();
                var comments = txtComments.Text.Trim();
                var selected_types = "";
                var add_text = true;

                var selected = lstDocTypes.GetSelectedIndices();

                if (selected.Length == 0)
                    throw new Exception("Please select Document Type. This is required field.");

                var doc_type_code = new int[selected.Length];

                for (var i = 0; i < selected.Length; i++)
                {
                    doc_type_code[i] = Int32.Parse(lstDocTypes.Items[selected[i]].Value);
                    if (add_text)
                    {
                        if ((selected_types.Length + lstDocTypes.Items[selected[i]].Text.Length + 2) > 100)
                        {
                            selected_types = selected_types + "...  ";
                            add_text = false;
                        }
                        else
                            selected_types = selected_types + lstDocTypes.Items[selected[i]].Text + ", ";
                    }
                }

                Document.SaveAttachment(DocNumber, doc_id, LoadID, doc_type_code, lines, comments, CurrentUserID);

                if (selected_types.Length > 2)
                    selected_types = selected_types.Substring(0, selected_types.Length - 2);

                if (NewDocument)
                    History.InsertHistoryOnAttachmentActivity((int)HistoryActions.haUploadNewAttachment, LoadID, CurrentUserOrganization, DocNumber,
                        comments, lines, doc_id, selected_types, lblFileName.Text, CurrentUserID);
                else
                    History.InsertHistoryOnAttachmentActivity((int)HistoryActions.haUpdateAttacment, LoadID, CurrentUserOrganization, DocNumber,
                        comments, lines, doc_id, selected_types, lblFileName.Text, CurrentUserID);

                AttachmentsDataTable = null;
                gvDocs.DataSource = AttachmentsDataTable;
                gvDocs.DataBind();

                lblLineNum.Text = lines;
                lblDocType.Text = selected_types;
                lblComments.Text = comments;
                NewDocument = false;
                EnableDocProperties(false);

                //RefreshDocAdministrationList();
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                {
                    lblMessage.Visible = true;
                    lblMessage.CssClass = "errorsum";
                    lblMessage.Text = GetErrors();
                }
            }
        }

        private void RefreshDocAdministrationList()
        {
            if (ItemsViewSourcePath == "DocAdministration.aspx")
            {
                ItemsDataView = null;
                txtReloadOpener.Value = "y";
            }
        }

        void gvDocs_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //init controls for row
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var doc_id = (int)((DataRowView)e.Row.DataItem)[1];
                var row_id = e.Row.RowIndex;

                var lnkView = (HtmlAnchor)e.Row.FindControl("lnkView");
                if (lnkView != null)
                    lnkView.HRef = "javascript:view_doc(" + doc_id.ToString() + ");";

                var btnEdit = (HtmlInputImage)e.Row.FindControl("btnEdit");
                if (btnEdit != null)
                    btnEdit.Attributes.Add("onclick", "javascript:return edit_doc(" + doc_id.ToString() + ");");

                if (DisplaySendProperties)
                {
                    var chkEmail = (HtmlInputCheckBox)e.Row.FindControl("chkEmail");
                    if (chkEmail != null)
                    {
                        if (LoadIsArchived || !CanBeUpdated)
                            chkEmail.Disabled = true;
                        else
                        {
                            chkEmail.Attributes.Add("title", "t1_" + doc_id.ToString());
                            chkEmail.Attributes.Add("onclick", "javascript:include_in_email(this," + doc_id.ToString() + ",'t1_" + doc_id.ToString() + "');");
                            if ((bool)((DataRowView)e.Row.DataItem)[12])
                                chkEmail.Checked = true;
                        }
                    }

                    var chkRevEmail = (HtmlInputCheckBox)e.Row.FindControl("chkRevEmail");
                    if (chkRevEmail != null)
                    {
                        if (LoadIsArchived || !CanBeUpdated)
                            chkRevEmail.Disabled = true;
                        else
                        {
                            chkRevEmail.Attributes.Add("title", "t2_" + doc_id.ToString());
                            chkRevEmail.Attributes.Add("onclick", "javascript:include_rev_email(this," + doc_id.ToString() + ",'t2_" + doc_id.ToString() + "');");
                            if ((bool)((DataRowView)e.Row.DataItem)[13])
                                chkRevEmail.Checked = true;
                        }
                    }

                    var chkApproved = (HtmlInputCheckBox)e.Row.FindControl("chkApproved");
                    if (chkApproved != null)
                    {
                        if (LoadIsArchived || !User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                            chkApproved.Disabled = true;
                        else
                        {
                            var file_name = ((DataRowView)e.Row.DataItem)[2].ToString();
                            var doc_type_code = ((DataRowView)e.Row.DataItem)[6].ToString();
                            var doc_type_name = ((DataRowView)e.Row.DataItem)[7].ToString();
                            var upload_user = ((DataRowView)e.Row.DataItem)[17].ToString();
                            var org = ((DataRowView)e.Row.DataItem)[15].ToString();
                            chkApproved.Attributes.Add("onclick", "javascript:doc_approved(this," + doc_id.ToString() + "," + doc_type_code + ",'" + doc_type_name + "','" + file_name + "','" + upload_user + "','" + org + "');");

                        }
                        if (((DataRowView)e.Row.DataItem)[16] != DBNull.Value)
                            chkApproved.Checked = (Int16.Parse(((DataRowView)e.Row.DataItem)[16].ToString()) > 0) ? true : false;
                    }
                }

                var lblDocName = (Label)e.Row.FindControl("lblDocName");
                if (lblDocName != null)
                    lblDocName.Text = (string)((DataRowView)e.Row.DataItem)[2];

                e.Row.Attributes.Add("onmouseover", "javascript:return display_properties(" + row_id.ToString() + ");");
            }
        }

        void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                ApplicationAssert.CheckCondition(ctrlUpload.HasFile, "Please select document to attach.");

                var sFileName = Server.HtmlEncode(ctrlUpload.FileName);
                var sFilePath = ctrlUpload.PostedFile.FileName;
                var sFileExtension = Path.GetExtension("@" + sFilePath);
                //sFileExtension = sFileExtension.Replace(".", "");
                if (sFileExtension.ToLower() == ".docx" || sFileExtension.ToLower() == ".xlsx" || sFileExtension.ToLower() == ".docm" || sFileExtension.ToLower() == ".dotx" || sFileExtension.ToLower() == ".xlsm" || sFileExtension.ToLower() == ".xlsb")
                {
                    throw new Exception("The ULO application can not upload file with the'" + sFileExtension + "' extension. Please see the 'extention rules' above.");
                }

                if (sFileExtension.ToLower() == ".tiff" || sFileExtension.ToLower() == ".tif")
                {
                    throw new Exception("The ULO application can not upload file with the'" + sFileExtension + "' extension. Please see the 'extention rules' above.");
                }

                if (sFileExtension.ToLower() == ".zip")
                {
                    throw new Exception("The ULO application can not upload file with the'" + sFileExtension + "' extension.");
                }

                var file_length = (int)ctrlUpload.PostedFile.InputStream.Length;
                var file_data = new byte[file_length];
                var content_type = ctrlUpload.PostedFile.ContentType;

                ctrlUpload.PostedFile.InputStream.Read(file_data, 0, file_length);

                var intDocID = FilesBO.AddNewFile(sFileName, file_length, file_data, content_type, CurrentUserLogin);

                txtDocID.Value = intDocID.ToString();

                lblFileName.Text = sFileName;
                lblUploadDate.Text = String.Format("{0:MMM dd, yyyy}", DateTime.Now);
                lblUploadUser.Text = CurrentUserName + " (" + CurrentUserOrganization + ")";
                lblDocType.Text = "";
                lstDocTypes.ClearSelection();
                lblLineNum.Text = "";
                txtLineNum.Text = "";
                lblEmailDate.Text = "";
                lblComments.Text = "";
                txtComments.Text = "";

                NewDocument = true;
                EnableDocProperties(true);

            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                {
                    lblMessage.Visible = true;
                    lblMessage.CssClass = "errorsum";
                    lblMessage.Text = GetErrors();
                }
            }
        }

        private void InitPropertiesArray(DataTable dt)
        {
            /*
             * (0) - File Name
             * (1) - Upload Date
             * (2) - Upload User 
             * (3) - DocType Name
             * (4) - Line Num
             * (5) - Comments
             * (6) - Last sent Email date to CO
             * (7) - DocID
             */

            var sb = new StringBuilder();

            foreach (DataRow dr in dt.Rows)
            {
                sb.Append(dr["FileName"].ToString());
                sb.Append("|");
                sb.Append(String.Format("{0:MM/dd/yyyy}", (DateTime)dr["UploadDate"]));
                sb.Append("|");
                sb.Append(dr["UploadUser"].ToString());
                if (dr["Organization"].ToString() != "")
                    sb.Append(" (" + dr["Organization"].ToString() + ")");
                sb.Append("|");
                sb.Append(dr["DocTypeName"].ToString());
                sb.Append("|");
                sb.Append(dr["LinesInfo"].ToString());
                sb.Append("|");
                sb.Append(dr["Comment"].ToString());
                sb.Append("|");
                if (DisplaySendProperties && dr["LastEmailDate"] != DBNull.Value)
                    sb.Append(String.Format("{0:MM/dd/yyyy}", (DateTime)dr["LastEmailDate"]));
                else
                    sb.Append("");
                sb.Append("|");
                sb.Append(dr["DocID"].ToString());
                sb.Append("&&&");
            }

            txtPropertyArray.Value = sb.ToString();
        }

        private void InitReviewersInfo(DataTable dtDocumentOrgUsers, bool AvailableForUpdate)
        {
            //DataSet ds = DocumentBO.GetOrgListByDocNumber(LoadID, DocNumber);
            var sb = new StringBuilder();
            string str_record;
            bool bln_reviewer;
            foreach (DataRow dr in dtDocumentOrgUsers.Rows)
            {
                str_record = dr["ReviewerName"].ToString();
                if (str_record == "")
                {
                    str_record = "not assigned";
                    bln_reviewer = false;
                }
                else
                {
                    bln_reviewer = true;
                }
                str_record += " (";
                str_record += dr["Organization"].ToString();
                str_record += ")";

                if (sb.ToString().IndexOf(str_record) == -1)
                {
                    sb.Append(str_record);
                    sb.Append("&nbsp;&nbsp;&nbsp;");

                    if (AvailableForUpdate && bln_reviewer && DisplaySendProperties && !LoadIsArchived
                        && (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) || User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString())))
                    {
                        sb.Append("<input type='image' src='../images/ClosedMail.gif' alt='email' title='Send Email to ");
                        sb.Append(dr["ReviewerName"].ToString());
                        sb.Append("' onclick='javascript:return email_reviewer(");
                        sb.Append(dr["ReviewerUserID"]);
                        sb.Append(");' />");
                        sb.Append("<br />");
                    }
                    else
                    {
                        sb.Append("<br />");
                    }
                }
            }
            tdReviewer.InnerHtml = sb.ToString();
        }

        private void InitAttachmentsRecommendation()
        {
            if (!DisplaySendProperties)
            {
                tblInfo.Visible = false;
            }
            else
            {
                //display recommendations for sending attachments for 'Sample' Type load:
                var pegasys_doc_type = DocNumber.Substring(0, 2);
                var dt = Document.RecommendedAttachments(pegasys_doc_type);
                if (dt == null)
                {
                    lblPegasysDocTypeNote.Visible = true;
                    lblPegasysDocTypeNote.Text = "There is no attachments recommendation in the system for Document type " + pegasys_doc_type;
                }
                else
                {
                    var dr = dt.Rows[0];
                    if (dr["Notes"].ToString() == "")
                        lblPegasysDocTypeNote.Visible = false;
                    else
                    {
                        lblPegasysDocTypeNote.Visible = true;
                        lblPegasysDocTypeNote.Text = "(" + dr["Notes"].ToString() + ")";
                    }
                    lblPegasysDocType.Text = pegasys_doc_type;
                    lblRequiredForTotal.Text = dr["TotalDocTypeDesc"].ToString();
                    lblRequiredForUDO.Text = dr["DODocTypeDesc"].ToString();
                }
            }
        }

        private void InitDocProperties(int DocID)
        {
            var dr = AttachmentsDataTable.Select("DocID = " + DocID.ToString());
            lblFileName.Text = dr[0]["FileName"].ToString();
            //lblUploadDate.Text = String.Format("{0:MMM dd, yyyy}", (DateTime)dr[0]["UploadDate"]);
            lblUploadDate.Text = String.Format("{0:m/d/yyyy}", (DateTime)dr[0]["UploadDate"]);

            var user = dr[0]["UploadUser"].ToString();
            if (dr[0]["Organization"].ToString() != "")
                user = user + " (" + dr[0]["Organization"].ToString() + ")";
            lblUploadUser.Text = user;

            lblDocType.Text = dr[0]["DocTypeName"].ToString();
            lblLineNum.Text = dr[0]["LinesInfo"].ToString();
            txtLineNum.Text = dr[0]["LinesInfo"].ToString();
            lblComments.Text = dr[0]["Comment"].ToString();
            txtComments.Text = dr[0]["Comment"].ToString();

            if (DisplaySendProperties)
            {
                if (dr[0]["LastEmailDate"] != DBNull.Value)
                    lblEmailDate.Text = String.Format("{0:m/d/yyyy}", (DateTime)dr[0]["LastEmailDate"]);
                else
                    lblEmailDate.Text = "";
            }
            else
            {
                lblEmailDateLabel.Visible = false;
                lblEmailDate.Visible = false;
            }

            foreach (var r in dr)
            {
                if (r["DocTypeCode"].ToString() != "")
                    lstDocTypes.Items.FindByValue(r["DocTypeCode"].ToString()).Selected = true;
            }

        }


        private void EnableDocProperties(bool Enable)
        {
            lblDocType.Visible = !Enable;
            lstDocTypes.Visible = Enable;

            lblLineNum.Visible = !Enable;
            txtLineNum.Visible = Enable;

            lblComments.Visible = !Enable;
            txtComments.Visible = Enable;

            btnSave.Enabled = Enable;
            btnSave.Visible = Enable;
            btnDelete.Enabled = Enable;
            btnDelete.Visible = Enable;
            btnCancel.Enabled = Enable;
            btnCancel.Visible = Enable;

            txtDisplayFlag.Value = (Enable) ? "0" : "1";

            if (NewDocument)
            {
                btnDelete.Visible = false;
                btnDelete.Enabled = false;
            }
        }

        private DataTable AttachmentsDataTable
        {
            get
            {
                if (ViewState["ATTACHMENTS"] == null)
                {
                    var dt = Document.GetDocumentAttachments(Convert.ToInt32(DocNumber), LoadID.ToString()).Tables[0];
                    ViewState["ATTACHMENTS"] = dt;
                    //rebuild properties array for client-side functionality:
                    //prepare data array based on dataset - for client to display doc properties on_mouse_over
                    InitPropertiesArray(dt);
                }

                return (DataTable)ViewState["ATTACHMENTS"];
            }
            set
            {
                ViewState["ATTACHMENTS"] = value;
            }
        }

        private bool NewDocument
        {
            get
            {
                if (ViewState["NEW_DOC"] == null)
                    return false;
                else
                    return (bool)ViewState["NEW_DOC"];
            }
            set
            {
                ViewState["NEW_DOC"] = value;
            }
        }

        private bool CanBeUpdated
        {
            get
            {
                if (ViewState["UPDATE"] == null)
                    return false;
                else
                    return (bool)ViewState["UPDATE"];
            }
            set
            {
                ViewState["UPDATE"] = value;
            }
        }

        private bool DisplaySendProperties
        {
            get
            {
                if (ViewState["DISPLAY_SEND"] == null)
                {
                    InitLoadProperties();
                }
                return (bool)ViewState["DISPLAY_SEND"];
            }
            set
            {
                ViewState["DISPLAY_SEND"] = value;
            }
        }

        private bool LoadIsArchived
        {
            get
            {
                if (ViewState["IS_ARCHIVED"] == null)
                {
                    InitLoadProperties();
                }
                return (bool)ViewState["IS_ARCHIVED"];
            }
            set
            {
                ViewState["IS_ARCHIVED"] = value;
            }
        }

        private void InitLoadProperties()
        {
            //find current OpenItems Type:
            var ds = LoadList;
            if (ds == null)
            {
                throw new Exception("Your session has been expired. Please logout and login to the application again.");
            }
            var dr = ds.Tables[0].Select("LoadID = " + LoadID.ToString());
            var item_type = (int)dr[0]["OIType"];

            //if (item_type == (int)OpenItemType.tpSample)
            //    ViewState["DISPLAY_SEND"] = true;
            //else
            ViewState["DISPLAY_SEND"] = false;

            if (dr[0]["ArchiveDate"] == DBNull.Value)
                ViewState["IS_ARCHIVED"] = false;
            else
                ViewState["IS_ARCHIVED"] = true;
        }

        private bool AttachmentsAvailableForUpdate(DataTable dtDocumentOrgUsers)
        {
            if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                return true;

            var organizations = "|";
            var reviewers = "|";

            foreach (DataRow dr in dtDocumentOrgUsers.Rows)
            {
                organizations = organizations + dr["Organization"].ToString() + "|";
                reviewers = reviewers + dr["ReviewerUserID"].ToString() + "|";
            }

            if (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) && (organizations.IndexOf("|" + CurrentUserOrganization + "|") > -1))
                return true;

            if (User.IsInRole(((int)UserRoles.urReviewer).ToString()) && (reviewers.IndexOf("|" + CurrentUserID.ToString() + "|") > -1))
                return true;

            return false;
        }

    }
}