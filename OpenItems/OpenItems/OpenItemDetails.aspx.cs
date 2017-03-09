using System.Linq;
using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Text;
    using System.Configuration;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;
    using System.Drawing;

    public partial class OpenItemDetails : PageBase, System.Web.UI.ICallbackEventHandler
    {
        protected String returnValue;

        private readonly Lookups Lookups;
        private readonly AdminBO Admin;
        private readonly ItemBO Item;
        private readonly LineNumBO LineNum;
        public OpenItemDetails()
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
                //lblError.Text = "";

                if (!IsPostBack)
                {
                    ApplicationAssert.CheckCondition(
                        (Request.QueryString["id"] != null && Request.QueryString["org"] != null && Request.QueryString["user"] != null),
                        "Missing parameters. Please reload the page.");

                    if (Request.QueryString["load"] != null && Request.QueryString["load"] != "")
                    {
                        var _load = 0;
                        if (Int32.TryParse(Request.QueryString["load"], out _load))
                            LoadID = _load;
                    }

                    OItemID = Int32.Parse(Request.QueryString["id"]);
                    OrgCode = Request.QueryString["org"];
                    CurrentItemReviewerID = Int32.Parse(Request.QueryString["user"]);

                    ItemLinesDataView = null;
                    txtItemID.Value = OItemID.ToString();

                    var item = Item.GetOpenItem(OItemID, LoadID, OrgCode, CurrentItemReviewerID);
                    BlnForUpdate = Item.AvailableForUpdate(item.IsArchived, (item.ParentLoadID > 0), item.Organization, item.ReviewerUserID, User, CurrentUserOrganization, CurrentUserID);


                    LoadItemDetails(item);
                    LoadFeedbackTable(item);
                    BindGridData(item);

                    //txtComment.Attributes.Add(onchange", "javascript:onchange_comment();");
                    if (item.ReviewerUserID == 0)
                    {
                        //lblError.Text = lblError.Text + "<br>Item is not assigned yet, therefore can't be edited. If you have admin rights, please return to the Open Items list page and assign the item.";
                    }
                }



                var cbReference = Page.ClientScript.GetCallbackEventReference(this, "arg", "ReceiveServerData", "context");
                String callbackScript;
                callbackScript = "function CallServer(arg, context)" + "{ " + cbReference + ";}";
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "CallServer", callbackScript, true);

                WireClientEvents();


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

        public void RaiseCallbackEvent(String eventArgument)
        {
            try
            {
                var arrParams = eventArgument.Split(new char[] { '|' });

                var str_reviewer = arrParams[0];
                var str_udo = arrParams[1];
                var str_do = arrParams[2];
                var str_comm = arrParams[3];
                var str_date = arrParams[4];

                DateTime comp_date;
                if (str_date == "")
                    comp_date = DateTime.MinValue;
                else
                    comp_date = DateTime.Parse(str_date);

                SaveItemProperties(comp_date, str_udo, str_do, str_comm, str_reviewer);
            }
            catch (Exception ex)
            {
                returnValue = ex.Message;
            }
            returnValue = "TRUE";
        }

        public String GetCallbackResult()
        {
            return returnValue;
        }


        private void WireClientEvents()
        {
            var sb = new StringBuilder();
            sb.Append("<script type='text/javascript' >");
            sb.Append("function on_cancel()");
            sb.Append("{");
            sb.Append("window.location.href = '");
            sb.Append(ItemsViewSourcePath);
            sb.Append("?back=y';");
            sb.Append("}");
            sb.Append("</script>");
            Page.RegisterClientScriptBlock("on_cancel", sb.ToString());
        }

        protected void Page_Init(object sender, System.EventArgs e)
        {
            gvLineNums.RowDataBound += new GridViewRowEventHandler(gvLineNums_RowDataBound);
            gvLineNums.Sorting += new GridViewSortEventHandler(gvLineNums_Sorting);
            btnSave.Click += new EventHandler(btnSave_Click);
            rpFeedback.ItemDataBound += new RepeaterItemEventHandler(rpFeedback_ItemDataBound);
        }

        void gvLineNums_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            try
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    var ulo_org_code = (string)((DataRowView)e.Row.DataItem)[12];
                    var line_num = (int)((DataRowView)e.Row.DataItem)[1];
                    var line_organization = (string)((DataRowView)e.Row.DataItem)[17];
                    var reviewer_id = (int)((DataRowView)e.Row.DataItem)[13];

                    var chk_reassign = (HtmlInputCheckBox)e.Row.FindControl("chk_reassign");
                    if (chk_reassign != null)
                    {
                        if (BlnForUpdate && LineNum.AvailableForUpdate(false, false, lblOrganization.Text, line_organization,
                            LineNum.LineOnReassignRequest(OItemID, line_num, ulo_org_code, reviewer_id),
                            reviewer_id, User, CurrentUserOrganization, CurrentUserID))
                        {
                            chk_reassign.AddOnClick("javascript:line_selected(this," + line_num.ToString() + ");");

                            btnReviewer.Disabled = false;
                            btnReviewer.Visible = true;
                            lblReassignLines.Visible = true;
                        }
                        else
                            chk_reassign.Disabled = true;
                    }

                    for (var i = 2; i < e.Row.Cells.Count; i++)
                    {
                        //if (LoadID != 11) // this needs to be corrected ??????
                        //{
                        //    e.Row.Cells[i].AddOnClick("javascript:get_line_details('LineNumDetails.aspx?id=" + OItemID.ToString() + "&num=" + line_num.ToString() + "');");
                        //}
                        //else
                        //{
                        //    e.Row.Cells[i].AddOnClick("javascript:get_line_details('OIBA53Review.aspx?id=" + OItemID.ToString() + "&num=" + line_num.ToString() + "');");
                        //}
                        e.Row.Cells[i].AddOnClick("javascript:get_line_details('LineNumDetails.aspx?id=" + OItemID.ToString() + "&num=" + line_num.ToString() + "');");

                    }
                }
            }
            catch (Exception ex) { AddError(ex); }
        }

        void rpFeedback_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                var dr = (DataRowView)e.Item.DataItem;
                if (dr != null)
                {

                    var load_id = ((int)dr["LoadID"]).ToString();
                    var lblFeedback = (Label)e.Item.FindControl("lblFeedback");
                    lblFeedback.Text = String.Format("Feedback file from Central Office received on {0:MMM dd, yyyy}.     Review round - {1}", (DateTime)dr["LoadDate"], (int)dr["ReviewRound"]);
                    var lblFdbCOComment = (Label)e.Item.FindControl("lblFdbCOComment");
                    lblFdbCOComment.Text = (string)Utility.GetNotNullValue(dr["FeedbackComments"], "String");

                    var tblFeedback = (HtmlTable)e.Item.FindControl("tblFeedback");


                    if ((DateTime)Utility.GetNotNullValue(dr["ArchiveDate"], "DateTime") == DateTime.MinValue &&
                        (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) ||
                        User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) && CurrentUserOrganization == lblOrganization.Text ||
                        User.IsInRole(((int)UserRoles.urReviewer).ToString()) && CurrentItemReviewerID == CurrentUserID))
                    {
                        tblFeedback.Style.Add("border", "solid 2px red");

                        //set value for editing item properties:
                        FeedbackForUpdate = true;

                        //get active fields:
                        var ddlFdbValid = (DropDownList)e.Item.FindControl("ddlFdbValid");
                        ddlFdbValid.Visible = true;
                        ddlFdbValid.DataSource = Lookups.GetValidationValuesList();
                        ddlFdbValid.DataValueField = "Valid";
                        ddlFdbValid.DataTextField = "ValidValueDescription";
                        ddlFdbValid.DataBind();
                        ddlFdbValid.SelectedValue = String.Format("{0}", (int)dr["Valid"]);
                        var txtFdbResponse = (TextBox)e.Item.FindControl("txtFdbResponse");
                        txtFdbResponse.Visible = true;
                        txtFdbResponse.Text = (string)Utility.GetNotNullValue(dr["Response"], "String");
                        //txtFdbResponse.BackColor = Color.Yellow;
                        //ddlFdbValid.BackColor = Color.Yellow;

                        var btnSaveFdb = (HtmlInputButton)e.Item.FindControl("btnSaveFdb");
                        btnSaveFdb.Visible = true;
                        btnSaveFdb.AddOnClick("javascript:save_feedback(this," + load_id + ",'" + Settings.Default.FeedbackLoad_DO_UDO_Required + "');");

                        if (Settings.Default.FeedbackLoad_DO_UDO_Display)
                        {
                            var txtFdbUDO = (TextBox)e.Item.FindControl("txtFdbUDO");
                            txtFdbUDO.Enabled = true;
                            txtFdbUDO.Text = Utility.DisplayMoneyFormat(dr["UDOShouldBe"]);
                            var txtFdbDO = (TextBox)e.Item.FindControl("txtFdbDO");
                            txtFdbDO.Enabled = true;
                            txtFdbDO.Text = Utility.DisplayMoneyFormat(dr["DOShouldBe"]);

                            //txtFdbUDO.BackColor = Color.Yellow;
                            //txtFdbDO.BackColor = Color.Yellow;

                            var revFdbUDO = (RegularExpressionValidator)e.Item.FindControl("revFdbUDO");
                            var revFdbDO = (RegularExpressionValidator)e.Item.FindControl("revFdbDO");
                            if (Settings.Default.FeedbackLoad_DO_UDO_Required)
                            {
                                revFdbUDO.ValidationExpression = "[$]?(\\d+([\\,]{1}\\d{3})*)+([\\.]\\d{1,2})?";
                                revFdbDO.ValidationExpression = "[$]?(\\d+([\\,]{1}\\d{3})*)+([\\.]\\d{1,2})?";
                            }
                            else
                            {
                                revFdbUDO.ValidationExpression = "[$]?(\\d*([\\,]{1}\\d{3})*)*([\\.]\\d{1,2})?";
                                revFdbDO.ValidationExpression = "[$]?(\\d*([\\,]{1}\\d{3})*)*([\\.]\\d{1,2})?";
                            }
                        }
                        else
                        {
                            var trDO_UDO = (HtmlTableRow)e.Item.FindControl("trDO_UDO");
                            trDO_UDO.Visible = false;
                        }

                        lblFeedback.ForeColor = Color.Yellow;
                        lblFdbCOComment.ForeColor = Color.Red;
                    }
                    else
                    {
                        //disable fields, for info only:
                        var lblFdbValid = (Label)e.Item.FindControl("lblFdbValid");
                        lblFdbValid.Visible = true;
                        lblFdbValid.Text = dr["ValidValueDescription"].ToString();
                        var lblFdbResponse = (Label)e.Item.FindControl("lblFdbResponse");
                        lblFdbResponse.Visible = true;
                        lblFdbResponse.Text = (string)Utility.GetNotNullValue(dr["Response"], "String");

                        if (Settings.Default.FeedbackLoad_DO_UDO_Display)
                        {
                            var txtFdbUDO = (TextBox)e.Item.FindControl("txtFdbUDO");
                            txtFdbUDO.Enabled = false;
                            txtFdbUDO.Text = Utility.DisplayMoneyFormat(dr["UDOShouldBe"]);
                            var txtFdbDO = (TextBox)e.Item.FindControl("txtFdbDO");
                            txtFdbDO.Enabled = false;
                            txtFdbDO.Text = Utility.DisplayMoneyFormat(dr["DOShouldBe"]);
                        }
                        else
                        {
                            var trDO_UDO = (HtmlTableRow)e.Item.FindControl("trDO_UDO");
                            trDO_UDO.Visible = false;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                AddError(ex);
            }
        }

        void btnFdb_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(ItemsViewSourcePath + "?back=y");
            }
            catch (Exception ex) { AddError(ex); }
        }


        private bool FeedbackForUpdate
        {
            get
            {
                if (ViewState["FEEDBACK_UPDATE"] == null)
                    return false;
                else
                    return (bool)ViewState["FEEDBACK_UPDATE"];
            }
            set
            {
                ViewState["FEEDBACK_UPDATE"] = value;
            }
        }

        private void SaveItemProperties(DateTime dtCompDate, string strUDO, string strDO, string strComments, string strReviewer)
        {
            var comment = strComments.Trim();
            var reviewer = Int32.Parse(strReviewer);

            if (Item.UpdateItemProperties(OItemID, LoadID, OrgCode, strUDO, strDO, dtCompDate, comment, reviewer, CurrentUserID))
                ItemsDataView = null;
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var str_msg = "";
                if (dtCompletionDate.isEmpty)
                    str_msg = str_msg + "- 'Expected Completion Date'<br />";
                if (txtUDO.Text.Trim() == "$" || txtUDO.Text.Trim().Length == 0)
                    str_msg = str_msg + "- 'UDO Should Be'<br />";
                if (txtDO.Text.Trim() == "$" || txtDO.Text.Trim().Length == 0)
                    str_msg = str_msg + "- 'DO Should Be'<br />";

                if (str_msg.Length > 0)
                {
                    str_msg = "The following fields are required:<br />" + str_msg + "Please enter the value(s).";
                    throw new Exception(str_msg);
                }

                var comp_date = dtCompletionDate.Date;
                SaveItemProperties(comp_date, txtUDO.Text, txtDO.Text, txtComment.Text, txtReviewerUserID.Value);

                //ItemsDataView = null;
                Response.Redirect(ItemsViewSourcePath + "?back=y");
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

        void gvLineNums_Sorting(object sender, GridViewSortEventArgs e)
        {
            var expSort = ItemLinesSortExp;
            if ((expSort == e.SortExpression) || (expSort == e.SortExpression + " ASC"))
            {
                expSort = e.SortExpression + " DESC";
            }
            else
            {
                expSort = e.SortExpression + " ASC";
            }
            ItemLinesSortExp = expSort;

            BindGridData(null);

        }

        private void LoadFeedbackTable(OpenItem item)
        {
            if (item.ParentLoadID == 0)
                rpFeedback.Visible = false;
            else
            {
                //load Feedback details:
                var feedbackRecords = Item.GetFeedbackRecords(OItemID);
                if (feedbackRecords.Any())
                {
                    rpFeedback.Visible = false;
                    return;
                }
                rpFeedback.DataSource = feedbackRecords;
                rpFeedback.DataBind();
                var iRoundCount = rpFeedback.Items.Count;

                //foreach (RepeaterItem repeatItem in rpFeedback.Items)
                //{    // if condition to add HeaderTemplate Dynamically only Once    
                //    if (repeatItem.ItemIndex == 1)
                //    {
                //       //lblFeedback
                //    }
                //}
            }
        }

        private void LoadItemDetails(OpenItem item)
        {
            //bool blnForUpdate = Item.AvailableForUpdate(item.IsArchived, (item.ParentLoadID > 0), item.Organization, item.ReviewerUserID, User, CurrentUserOrganization, CurrentUserID);

            lblReviewer.Text = item.Reviewer;
            txtReviewerUserID.Value = item.ReviewerUserID.ToString();
            txtReviewerEmail.Value = Admin.GetEmailByUserID(Convert.ToInt32(txtReviewerUserID.Value));

            lblDocNumber.Text = item.DocNumber;
            DocNumber = item.DocNumber;

            txtItemStatusCode.Value = item.StatusCode.ToString();


            lblEstAccrual.Text = item.EstimatedAccrual;
            lblTypeOfBalance.Text = item.TypeOfBalance;
            lblCategory.Text = item.Category;
            lblACCR.Text = Utility.DisplayMoneyFormat(item.ACCR);
            lblAcctPeriod.Text = item.AcctPeriod;
            lblAwardNum.Text = item.AwardNumber;
            lblDO.Text = Utility.DisplayMoneyFormat(item.DO);
            lblEarliest.Text = Utility.DisplayDateFormat(item.Earliest, "MM/yyyy");
            lblFirstReview.Text = item.ReviewRoundDesc;
            lblHOLDBACKS.Text = Utility.DisplayMoneyFormat(item.HOLDBACKS);
            lblJustification.Text = item.Justification;
            lblLatest.Text = Utility.DisplayDateFormat(item.Latest, "MM/yyyy");
            lblOrganization.Text = item.Organization;
            lblOrgCode.Text = item.ULOOrgCode;
            lblPENDPYMT.Text = Utility.DisplayMoneyFormat(item.PEND_PYMT);
            lblPYMTS_CONF.Text = Utility.DisplayMoneyFormat(item.PYMT_CONF);
            lblTitleField.Text = item.TitleField;
            lblTotalLine.Text = Utility.DisplayMoneyFormat(item.TotalLine);
            lblUDO.Text = Utility.DisplayMoneyFormat(item.UDO);

            lblLastPaid.Text = Utility.DisplayDateFormat(item.LastPaid, "MMM dd, yyyy");
            lblExpDate.Text = Utility.DisplayDateFormat(item.ExpirationDate, "MMM dd, yyyy");

            lblDueDate.Text = Utility.DisplayDateFormat(item.DueDate, "MMM dd, yyyy");
            if (item.DueDate <= DateTime.Now && !item.IsArchived)
                lblDueDate.CssClass = "regBldRedText";

            if (item.IsArchived)
                lblStatus.Text = item.Status + " (Archived)";
            else
                lblStatus.Text = item.Status;

            if (item.ParentLoadID > 0)
            {
                lblValid.Text = item.ValidDescription + " (1st review)";
                lblValid.CssClass = "regBldGreyText";
            }
            else
                lblValid.Text = item.ValidDescription;

            if (item.JustificationRequired)
                lblJustfReq.Text = "Yes";
            else
                lblJustfReq.Text = "No";

            txtComment.Text = item.Comments;
            txtDO.Text = item.DOShouldBe;
            txtUDO.Text = item.UDOShouldBe;

            dtCompletionDate.MinDate = DateTime.Parse("01/01/1990");
            dtCompletionDate.MaxDate = DateTime.Now.AddYears(10);
            dtCompletionDate.VoidEmpty = false;
            if (item.ExpectedCompletionDate > DateTime.MinValue)
            {
                dtCompletionDate.Date = item.ExpectedCompletionDate;
                if (item.ExpectedCompletionDate < DateTime.Now)
                    dtCompletionDate.DateCSSClass = "regBldRedText";
            }

            ctrlContacts.ContrOfficerByCO = item.ContrOfficerByCO;


            if (item.ItemInfo.Length > 0)
            {
                txtItemInfo.Text = item.ItemInfo;
                txtItemInfo.Enabled = false;

                tblItemInfo.Visible = true;
            }
            else
            {
                tblItemInfo.Visible = false;
            }


            /*
            //save current item properties on client:
            StringBuilder sb = new StringBuilder();
            sb.Append("<script type='text/javascript' >");                
            sb.Append("var init_udo = '" + txtUDO.Text + "';");
            sb.Append("var init_do = '" + txtDO.Text + "';");
            sb.Append("</script>");
            ClientScript.RegisterClientScriptBlock(this.GetType(), "item_details", sb.ToString());
            */

            if (!BlnForUpdate)// not updates here
            {
                txtComment.Enabled = false;
                txtDO.Enabled = false;
                txtUDO.Enabled = false;
                dtCompletionDate.FreezeMode = true;
                dtCompletionDate.VoidEdit = true;

                btnSave.Enabled = false;
                btnSave.Visible = false;

                btnReviewer.Disabled = true;
                btnReviewer.Visible = false;
                btnReviewer0.Disabled = true;
                btnReviewer0.Visible = false;
                lblReassignLines.Visible = false;

                ctrlContacts.Enabled = false;
                ctrlAttachments.ShowAddAttBtn(false);
                ctrlAttachments.ShowEmailIcon(false, txtReviewerEmail.Value, lblDocNumber.Text);
            }
            else // here we can update the data
            {
                ctrlContacts.Enabled = true;
                ctrlAttachments.ShowAddAttBtn(true);

                if (lblValid.Text.ToLower().Trim() == "valid" || lblValid.Text.ToLower().Trim() == "invalid")
                {
                    ctrlAttachments.ShowEmailIcon(true, txtReviewerEmail.Value, lblDocNumber.Text);
                }
                else
                {
                    ctrlAttachments.ShowEmailIcon(false, txtReviewerEmail.Value, lblDocNumber.Text);
                }

                if (item.StatusCode == (int)OpenItemStatus.stReassignRequest)
                {
                    btnReviewer.Disabled = true;
                    btnReviewer.Visible = false;
                    lblReassignLines.Visible = false;
                }

                if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                    txtReassignTargetPage.Value = "Reroute";
                else if (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                    txtReassignTargetPage.Value = "Reassign";
                else
                    txtReassignTargetPage.Value = "ReassignRequest";

            }

        }

        private void BindGridData(OpenItem item)
        {
            if (ItemLinesDataView == null)
            {
                if (item == null)
                    item = new OpenItem(OItemID, LoadID, OrgCode, CurrentItemReviewerID);

                // ***SM
                //ItemLinesDataView = item.GetItemLines(false);
                ItemLinesDataView = item.GetItemLinesByReviewerID(CurrentItemReviewerID);
            }

            ItemLinesDataView.Sort = ItemLinesSortExp;

            gvLineNums.DataSource = ItemLinesDataView;
            gvLineNums.DataBind();

            //bool blnForUpdate = Item.AvailableForUpdate(item.IsArchived, (item.ParentLoadID > 0), item.Organization, item.ReviewerUserID, User, CurrentUserOrganization, CurrentUserID);
            //if (!blnForUpdate)
            if (!BlnForUpdate)
                gvLineNums.Columns[1].Visible = false;
        }

        private bool BlnForUpdate
        {
            get
            {
                if (ViewState["ITEM_FOR_UPDATE"] == null)
                {
                    var item = new OpenItem(OItemID, LoadID, OrgCode, CurrentItemReviewerID);
                    ViewState["ITEM_FOR_UPDATE"] = Item.AvailableForUpdate(item.IsArchived, (item.ParentLoadID > 0), item.Organization, item.ReviewerUserID, User, CurrentUserOrganization, CurrentUserID);
                }
                return (bool)ViewState["ITEM_FOR_UPDATE"];
            }
            set
            {
                ViewState["ITEM_FOR_UPDATE"] = value;
            }
        }

    }
}