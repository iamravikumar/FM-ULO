using System.Linq;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Text;
    using System.Web.UI.WebControls;

    public partial class LineNumDetails : PageBase
    {
        DataTable dtCodeList;

        private readonly Lookups Lookups;
        private readonly AdminBO Admin;
        private readonly ItemBO Item;
        private readonly LineNumBO LineNum;
        public LineNumDetails()
        {
            Admin = new AdminBO(this.Dal);
            Lookups = new Lookups(this.Dal, Admin);
            Item = new ItemBO(this.Dal);
            LineNum = new LineNumBO(this.Dal, Item);
        }

        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    ApplicationAssert.CheckCondition((Request.QueryString["id"] != null && Request.QueryString["num"] != null), "Missing parameters. Please reload the page.");

                    OItemID = Int32.Parse(Request.QueryString["id"]);
                    base.LineNum = Int32.Parse(Request.QueryString["num"]);

                    if (LoadID == 0)
                    {
                        if (Request.QueryString["load_id"] != null)
                        {
                            LoadID = Int32.Parse(Request.QueryString["load_id"]);
                        }
                        else
                        {
                            LoadID = 0;
                        }
                    }

                    var line_item = LineNum.GetLineNum(OItemID, base.LineNum);
                    if (LoadID == 0 || OItemID == 0)
                    {
                        throw new Exception("Your session has beed expired. Please logout from the application and login again.");
                    }
                    var open_item = Item.GetOpenItem(OItemID, LoadID, line_item.ULOOrgCode, line_item.ReviewerUserID);

                    InitControls(open_item.IsArchived);

                    bool bln_update;
                    bool line_on_reassign;
                    line_on_reassign = LineNum.LineOnReassignRequest(OItemID, base.LineNum, line_item.ULOOrgCode, line_item.ReviewerUserID);
                    if (line_on_reassign)
                        lblHeaderLabel.Text = lblHeaderLabel.Text + " unavailable for update, waiting for reassignment.";
                    else if (open_item.Organization != line_item.Organization)
                        lblHeaderLabel.Text = lblHeaderLabel.Text + " (belong " + line_item.Organization + ")"; //?

                    bln_update = LineNum.AvailableForUpdate(open_item.IsArchived, (open_item.ParentLoadID > 0),
                        open_item.Organization, line_item.Organization, line_on_reassign, open_item.ReviewerUserID, User, CurrentUserOrganization, CurrentUserID);

                    //if (OrgCode != line_item.ULOOrgCode)
                    //     bln_update = false;
                    //else
                    //    bln_update = Item.AvailableForUpdate(open_item.IsArchived, (open_item.ParentLoadID > 0), open_item.Organization, open_item.ReviewerUserID, User, CurrentUserOrganization, CurrentUserID);

                    if (Errors.Count == 0)
                    {
                        LoadOpenItemDetails(open_item, bln_update);
                        LoadLineNumDetails(line_item, bln_update, open_item.IsArchived);
                    }


                    if (!bln_update)
                    {
                        btnSave.Enabled = false;
                        btnSave.Visible = false;
                        ctrlContacts.Enabled = false;
                        ctrlAttachments.ShowEmailIcon(false, txtReviewerEmail.Value, lblDocNumber.Text);
                        ctrlAttachments.ShowAddAttBtn(false);
                    }
                    else
                    {
                        ctrlAttachments.ShowAddAttBtn(true);
                        if (lblValid.Text.ToLower().Trim() == "valid" || lblValid.Text.ToLower().Trim() == "invalid")
                        {
                            ctrlAttachments.ShowEmailIcon(true, txtReviewerEmail.Value, lblDocNumber.Text);
                        }
                        else
                        {
                            ctrlAttachments.ShowEmailIcon(false, txtReviewerEmail.Value, lblDocNumber.Text);
                        }
                        ctrlContacts.Enabled = true;
                    }

                    if (open_item.ReviewerUserID == 0)
                    {
                        //lblError.Text = lblError.Text + "<br>Item is not assigned yet, therefore can't be edited. If you have admin rights, please return to the Open Items list page and assign the item.";
                    }
                }
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                //if (Errors.Count > 0)
                //    lblError.Text = GetErrors();
            }
        }

        protected void Page_Init(object sender, System.EventArgs e)
        {
            btnSave.Click += new EventHandler(btnSave_Click);
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var str_error = "";

                var item_id = Int32.Parse(txtItemID.Value);
                var line_num = Int32.Parse(lblLineNum.Text);
                var ulo_org_code = lblOrgCode.Text;
                var comments = txtComment.Text.Trim();
                var rwa = txtRWA.Text.Trim();

                var valid = Int32.Parse(ddlValid.SelectedValue);
                if (valid == 0)
                    str_error = "Please select 'Valid' value.<br/>";

                var justification = Int32.Parse(rblJustification.SelectedValue);
                var just_addon = "";
                var just_other = "";
                if (justification == 6)
                    just_other = txtJustOther.Text.Trim();
                else
                    just_addon = txtAddJustification.Text.Trim();
                if (justification == 0)
                    str_error = str_error + "Please select 'Justification' value.<br/>";
                if (justification == 6 && just_other.Length == 0)
                {
                    rblJustification.SelectedIndex = 0;
                    str_error = str_error + "Please enter your text for 'Justification' value.<br/>";
                }

                var code = ddlCode.SelectedValue;
                var code_comment = txtCodeComment.Text.Trim();
                if (code == "")
                    str_error = str_error + "Please select 'Code' value.<br/>";

                if (str_error.Length > 0)
                {
                    str_error = str_error + "This is required field.";
                    lblMsgValidation.Visible = true;
                    lblMsgValidation.Text = str_error;
                    return;
                }

                LineNum.UpdateDetails(item_id, LoadID, line_num, ulo_org_code, valid, comments, justification, just_addon, just_other, code, code_comment, rwa, CurrentUserID);

                //for update Items grid view:
                ItemsDataView = null;

                //Response.Redirect("OpenItemDetails.aspx?id=" + item_id.ToString() + "&org=" + ulo_org_code + "&user=" + CurrentItemReviewerID.ToString());
                Response.Redirect("OpenItemDetails.aspx?id=" + OItemID.ToString() + "&org=" + OrgCode + "&user=" + CurrentItemReviewerID.ToString());
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                //if (Errors.Count > 0)
                //    lblError.Text = GetErrors();
            }
        }

        private void InitControls(bool ItemIsArchived)
        {
            ddlValid.DataSource = Lookups.GetValidationValuesList();
            ddlValid.DataValueField = "Valid";
            ddlValid.DataTextField = "ValidValueDescription";
            ddlValid.DataBind();

            var dtJust = Lookups.GetJustificationDefaultList();
            //ddlJustification.DataSource = dtJust;
            //ddlJustification.DataValueField = "Justification";
            //ddlJustification.DataTextField = "JustificationDescription";
            //ddlJustification.DataBind();

            rblJustification.DataSource = dtJust;
            rblJustification.DataValueField = "Justification";
            rblJustification.DataTextField = "JustificationDescription";
            rblJustification.DataBind();
            rblJustification.Items.RemoveAt(0);


            var arrDisplayAddOn = "";
            var arrAddOnDescList = "";
            var sAddOnDesc = "";
            foreach (DataRow dr in dtJust.Rows)
            {
                arrDisplayAddOn = arrDisplayAddOn + dr["DisplayAddOnField"].ToString() + "|";

                if (dr["AddOnDescription"].ToString().Trim() == "")
                {
                    sAddOnDesc = "None";
                }
                else
                {
                    sAddOnDesc = dr["AddOnDescription"].ToString();
                }
                arrAddOnDescList = arrAddOnDescList + sAddOnDesc + "|";
            }
            txtDisplayAddOn.Value = arrDisplayAddOn;
            txtAddOnDescList.Value = arrAddOnDescList;

            if (ItemIsArchived)
                dtCodeList = Lookups.GetActiveAndExpiredCodeList();
            else
                dtCodeList = Lookups.GetActiveCodeList();
            ddlCode.DataSource = dtCodeList;
            ddlCode.DataValueField = "Code";
            ddlCode.DataTextField = "CodeDefinition";
            ddlCode.DataBind();

            var arrCodeList = "";
            var arrCodeDescList = "";
            var arrCodeValidation = "";
            foreach (DataRow dr in dtCodeList.Rows)
            {
                arrCodeList = arrCodeList + dr["Code"].ToString() + "|";
                arrCodeDescList = arrCodeDescList + dr["CodeDefinition"].ToString() + "|";
                arrCodeValidation = arrCodeValidation + dr["Valid"].ToString() + "|";
            }
            txtCodeList.Value = arrCodeList;
            txtCodeDescList.Value = arrCodeDescList;
            txtCodeValidation.Value = arrCodeValidation;

            //wire client events: 
            btnSave.Attributes["onclick"] = "javascript:return on_save();";
            btnCancel.Attributes["onclick"] = "javascript:window.location.href='OpenItemDetails.aspx?id=" + OItemID.ToString() + "&org=" + OrgCode + "&user=" + CurrentItemReviewerID.ToString() + "';";
            ddlValid.Attributes["onchange"] = "javascript:return on_valid_select(this);";
            ddlCode.Attributes["onchange"] = "javascript:return on_code_select(this);";
            //ddlJustification.Attributes["onchange"] = "javascript:return on_justification_select(this);";
            var j_id = 0;
            foreach (ListItem li in rblJustification.Items)
            {
                j_id = j_id + 1;
                //j_id = rblJustification.SelectedItem.Value;
                //li.AddOnClick("javascript:rbl_display_Just_explanation('" + li.Value + "')");
                li.AddOnClick("javascript:rbl_display_Just_explanation('" + j_id + "')");
            }
            //rblJustification.SelectedItem.Value

        }

        private void LoadOpenItemDetails(OpenItem open_item, bool EnableForUpdate)
        {
            txtItemID.Value = open_item.OItemID.ToString();
            lblDocNumber.Text = open_item.DocNumber;

            //moved to the Line properties:
            //lblReviewer.Text = open_item.Reviewer;                

            //lblOrganization.Text = open_item.Organization;
            //lblOrgCode.Text = open_item.ULOOrgCode;

            //if (open_item.IsArchived)
            //    lblStatus.Text = open_item.Status + " (Archived)";
            //else
            //    lblStatus.Text = open_item.Status;

            lblDueDate.Text = Utility.DisplayDateFormat(open_item.DueDate, "MMM dd, yyyy");
            if (open_item.DueDate <= DateTime.Now && !open_item.IsArchived)
                lblDueDate.CssClass = "regBldRedText";

            if (open_item.JustificationRequired)
                lblJustfReq.Text = "Yes";
            else
                lblJustfReq.Text = "No";
            lblAwardNum.Text = open_item.AwardNumber;
            lblTotalLineItem.Text = Utility.DisplayMoneyFormat(open_item.TotalLine);
            lblTitleField.Text = open_item.TitleField;
            lblEarliest.Text = Utility.DisplayDateFormat(open_item.Earliest, "MM/yyyy");
            lblLatest.Text = Utility.DisplayDateFormat(open_item.Latest, "MM/yyyy");
            lblAcctPeriod.Text = open_item.AcctPeriod;
            lblLastPaid.Text = Utility.DisplayDateFormat(open_item.LastPaid, "MMM dd, yyyy");
            lblExpDate.Text = Utility.DisplayDateFormat(open_item.ExpirationDate, "MMM dd, yyyy");
            lblCompletionDate.Text = Utility.DisplayDateFormat(open_item.ExpectedCompletionDate, "MMM dd, yyyy");
            if (open_item.ExpectedCompletionDate > DateTime.MinValue && open_item.ExpectedCompletionDate < DateTime.Now)
                lblCompletionDate.CssClass = "regBldRedText";
            lblUDO.Text = Utility.DisplayMoneyFormat(open_item.UDO);
            lblUDOShouldBe.Text = open_item.UDOShouldBe;
            lblDO.Text = Utility.DisplayMoneyFormat(open_item.DO);
            lblDOShouldBe.Text = open_item.DOShouldBe;
            lblACCR.Text = Utility.DisplayMoneyFormat(open_item.ACCR);
            lblPENDPYMT.Text = Utility.DisplayMoneyFormat(open_item.PEND_PYMT);
            lblPYMTS_CONF.Text = Utility.DisplayMoneyFormat(open_item.PYMT_CONF);
            lblHOLDBACKS.Text = Utility.DisplayMoneyFormat(open_item.HOLDBACKS);

            ctrlContacts.ContrOfficerByCO = open_item.ContrOfficerByCO;
            if (EnableForUpdate)
            {
                ctrlContacts.Enabled = true;
                ctrlAttachments.ShowAddAttBtn(false);
            }
            else
            {
                ctrlContacts.Enabled = false;
                ctrlAttachments.ShowAddAttBtn(true);
            }
        }

        private void LoadLineNumDetails(LineNum line_item, bool EnableForUpdate, bool IsArchived)
        {
            lblLineNum.Text = line_item.LineNumID.ToString();

            lblReviewer.Text = line_item.Reviewer;
            txtReviewerEmail.Value = Admin.GetEmailByUserID(line_item.ReviewerUserID);

            lblOrganization.Text = line_item.Organization;
            lblOrgCode.Text = line_item.ULOOrgCode;

            if (IsArchived)
                lblStatus.Text = line_item.Status + " (Archived)";
            else
                lblStatus.Text = line_item.StatusDescription;

            //display Line ULOOrgCode only if it's different from Open Item ULOOrgCode:
            /*
            if (lblOrgCode.Text != line_item.ULOOrgCode)
            {
                lblLineOrgCode.Text = String.Format(" ({0})", line_item.ULOOrgCode);
                lblLineOrgCode.Visible = true;
            }
            else
                lblLineOrgCode.Visible = false;
             */

            lblValid.Text = line_item.ValidDescription;
            if (line_item.Valid == (int)OpenItemValidation.vlInvalid)
            {
                lblDeobligatedDate.Visible = true;
                lblDeobligatedInfo.Visible = true;
                lblDeobligatedDate.Text = Utility.DisplayDateFormat(line_item.DeobligatedDate, "MMM dd, yyyy");
            }
            else
            {
                lblDeobligatedInfo.Visible = false;
                lblDeobligatedDate.Visible = false;
            }
            lblTotalLineLine.Text = Utility.DisplayMoneyFormat(line_item.TotalLine);
            lblProjNum.Text = line_item.ProjectNumber;
            lblBA.Text = line_item.BA;
            txtRWA.Text = line_item.RWA;
            lblAGRE_BLDG.Text = line_item.AGRE_BLDG;
            lblBuilding.Text = line_item.Building;
            lblVendCD.Text = line_item.VendorCD;
            lblVendName.Text = line_item.VendorName;
            lblACTG_PD.Text = Utility.DisplayDateFormat(line_item.ACTG_PD, "MMM dd, yyyy");
            lblLastPaidLine.Text = Utility.DisplayDateFormat(line_item.LastPaid, "MMM dd, yyyy");
            lblBBFY.Text = line_item.BBFY.ToString();
            lblEBFY.Text = line_item.EBFY.ToString();
            lblUDOLine.Text = Utility.DisplayMoneyFormat(line_item.UDO);
            lblFC.Text = line_item.FC;
            lblOC.Text = line_item.OC.ToString();
            lblCE.Text = line_item.CE.ToString();
            lblACCRLine.Text = Utility.DisplayMoneyFormat(line_item.ACCR);
            lblPENDPYMTLine.Text = Utility.DisplayMoneyFormat(line_item.PEND_PYMT);
            lblPYMTS_CONFLine.Text = Utility.DisplayMoneyFormat(line_item.PYMTS_CONF);
            lblHOLDBACKSLine.Text = Utility.DisplayMoneyFormat(line_item.HOLDBACKS);

            txtComment.Text = line_item.Comments;

            if (line_item.LineInfo.Length > 0)
            {
                txtLineInfo.Text = line_item.LineInfo;
                txtLineInfo.Enabled = false;

                tblLineInfo.Visible = true;
            }
            else
            {
                tblLineInfo.Visible = false;
            }


            ddlCode.SelectedValue = line_item.Code;
            txtCodeComment.Text = line_item.CodeComments;
            if (line_item.Code != "")
            {
                var drCode = dtCodeList.Select("Code = '" + line_item.Code + "'");
                if (drCode.Length > 0)
                    lblCode.Text = drCode[0]["Code"].ToString();
            }

            ddlValid.SelectedValue = line_item.Valid.ToString();

            //display Justification Value:
            var justifications = Lookups.GetJustificationValues();
            var justification = justifications.Where(j => j.Justification == line_item.JustificationCode);
            if (justification.Any())
            {
                var firstJustification = justification.First();
                if (firstJustification.InDefaultList)
                {
                    rblJustification.SelectedValue = line_item.JustificationCode.ToString();
                    if (firstJustification.DisplayAddOnField)
                    {
                        lblJustificationExplanation.Text = (string)Utility.GetNotNullValue(firstJustification.AddOnDescription, "String");
                        txtAddJustification.Text = line_item.JustificationAddOn;
                    }
                    else
                    {
                        lblJustificationExplanation.AddVisibilityHidden();
                        txtAddJustification.AddVisibilityHidden();
                        lblJustAddInfo.AddVisibilityHidden();
                    }
                    txtJustOther.AddVisibilityHidden();
                    lblJustOther.AddVisibilityHidden();
                }
                else
                {
                    rblJustification.SelectedValue = "6";
                    lblJustificationExplanation.AddVisibilityHidden();
                    txtAddJustification.AddVisibilityHidden();
                    txtJustOther.Text = (string)Utility.GetNotNullValue(firstJustification.JustificationDescription, "String");
                }
            }
            else
            {
                lblJustificationExplanation.AddVisibilityHidden();
                txtAddJustification.AddVisibilityHidden();
                lblJustAddInfo.AddVisibilityHidden();
                txtJustOther.AddVisibilityHidden();
                lblJustOther.AddVisibilityHidden();
            }

            var sb = new StringBuilder();
            sb.Append("<script type='text/javascript' >");
            sb.Append("var original_selected_just = '");
            sb.Append(rblJustification.SelectedValue);
            sb.Append("';");
            sb.Append("var original_add_on = '");
            sb.Append(txtAddJustification.Text);
            sb.Append("';");
            sb.Append("</script>");
            Page.RegisterStartupScript("orig_just", sb.ToString());


            if (!EnableForUpdate)
            {
                txtRWA.Enabled = false;
                txtComment.Enabled = false;
                ddlValid.Enabled = false;
                ddlCode.Enabled = false;
                txtCodeComment.Enabled = false;
                rblJustification.Enabled = false;
                txtAddJustification.Enabled = false;
                txtJustOther.Enabled = false;
            }
            else
            {
                //check if there are values for UDO & DO Should Be fields:
                var str_msg = "";
                if (lblCompletionDate.Text == "")
                    str_msg = str_msg + "Expected Completion Date.<br />";
                if (lblUDOShouldBe.Text.Trim() == "$" || lblUDOShouldBe.Text.Trim().Length == 0)
                    str_msg = str_msg + "UDO Should Be.<br />";
                if (lblDOShouldBe.Text.Trim() == "$" || lblDOShouldBe.Text.Trim().Length == 0)
                    str_msg = str_msg + "DO Should Be.<br />";

                if (str_msg.Length > 0)
                {
                    str_msg = "The following fields are required:<br />" + str_msg + "Please fill these fields before Line validation. <br/>";

                    lblMsgValidation.Visible = true;
                    lblMsgValidation.Text = str_msg;
                    ddlValid.Enabled = false;
                    ddlCode.Enabled = false;
                    txtCodeComment.Enabled = false;
                    rblJustification.Enabled = false;
                    txtAddJustification.Enabled = false;
                    txtJustOther.Enabled = false;
                }
            }
        }
    }
}