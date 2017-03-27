using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Text;
    using System.Configuration;
    using System.Collections;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;
    using System.Drawing;

    public partial class OIBA53Review : PageBase
    {
        int iStateID = 0;
        int iReasonForDelayID = 0;
        protected String returnValue;

        private readonly Lookups LookupsBO;
        private readonly AdminBO Admin;
        private readonly ItemBO Item;
        private readonly LineNumBO LineNumBO;

        public OIBA53Review()
        {
            Admin = new AdminBO(this.Dal);
            LookupsBO = new Lookups(this.Dal, Admin);
            Item = new ItemBO(this.Dal);
            LineNumBO = new LineNumBO(this.Dal, Item);
        }

        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {

            Response.CacheControl = "no-cache";
            Response.AddHeader("Pragma", "no-cache");
            Response.Expires = -1;
            //lblOrgCode.AddDisplay();

            //AlignDollarTextFields();
            //ViewState["ActionType"] = "";

            try
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

                if (!ClientScript.IsClientScriptBlockRegistered(Page.GetType(), "JS"))
                {
                    ClientScript.RegisterClientScriptBlock(Page.GetType(), "JS",
                       "<script language='javascript' src='../include/Misc.js'></script>");
                    //ClientScript.RegisterStartupScript(Page.GetType(), "JS_OnLoad",
                    //"<script FOR=window EVENT=onload language='javascript'>Resize()</script>");
                }

                ddgAccrualType.OnChangeHandler += new EventHandler(ddgAccrualType_changed);
                ddgReasonCode.OnChangeHandler += new EventHandler(ddgReasonCode_changed);
                ddgCurrFY.OnChangeHandler += new EventHandler(ddgCurrFY_changed);
                ddgState.OnChangeHandler += new EventHandler(ddgState_changed);
                //ddgReasonForDelay.OnChangeHandler += new EventHandler(ddgReasonForDelay_changed);


                ddgAccrualType.ShowExpandButton = true;
                ddgActionType.ShowExpandButton = true;
                ddgReasonCode.ShowExpandButton = true;
                ddgTeamCode.ShowExpandButton = true;
                ddgCurrFY.ShowExpandButton = true;
                ddgState.ShowExpandButton = true;
                ddgReasonForDelay.ShowExpandButton = true;
                // lblState.Text = "State:";
                //LimitTextBoxesLength(Page.Controls);



                if (IsPostBack == false)
                {
                    tr_other_reason_for_delay.AddDisplayNone();


                    LoadBA53Page();

                    TextBoxesLength();

                    if (Errors.Count == 0)
                    {
                        InitControls();
                    }
                }

                //lblAccrTypeAction.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                //lblAccrTypeAction.Text = lblActionTypeID.Text;
                var s1 = hdActionTypeValue.Value;
                lblAccrTypeAction.Text = s1; //
                if (lblAccrTypeAction.Text.Trim().ToLower() == "- select -")
                {
                    lblAccrTypeAction.Text = "Select from the combo below";
                }

                if (lblAccrTypeAction.Text.Trim() == "")
                {
                    lblAccrTypeAction.Text = "n/a";
                }

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

        private void InitControls()
        {
            calReportDate.FreezeMode = false;

            if (!BlnForUpdate) //hide email icon
            {
                ctrlAttachments.ShowEmailIcon(false, txtReviewerEmail.Value, lblDocNumber.Text);
                ctrlAttachments.ShowAddAttBtn(false);
                ctrlContacts.Enabled = false;
                btnReviewer.Visible = false;
                lblReassignLines.Visible = false;
            }
            else
            {
                ctrlAttachments.ShowAddAttBtn(true);

                if (lblValid.Text.ToLower().Trim() == "valid" || lblValid.Text.ToLower().Trim() == "invalid" && ddgAccrualType.SelectedIndex != 0)
                {
                    ctrlAttachments.ShowEmailIcon(true, txtReviewerEmail.Value, lblDocNumber.Text);
                }
                else
                {
                    ctrlAttachments.ShowEmailIcon(false, txtReviewerEmail.Value, lblDocNumber.Text);
                }
                ctrlContacts.Enabled = true;
            }

        }

        private void LoadBA53Page()
        {
            try
            {
                ApplicationAssert.CheckCondition((Request.QueryString["id"] != null), "Missing parameters. Please reload the page.");

                OItemID = Int32.Parse(Request.QueryString["id"]);
                txtItemID.Value = OItemID.ToString();
                OrgCode = Request.QueryString["org"];
                CurrentItemReviewerID = Int32.Parse(Request.QueryString["user"]);
                LineNum = 1;
                txtLineNo.Value = LineNum.ToString();
                //calFiscalYearEndDate.FreezeMode = true;

                var line_item = new LineBA53(OItemID, LineNum);

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

                if (LoadID == 0 || OItemID == 0)
                {
                    throw new Exception("Your session has beed expired. Please logout from the application and login again.");
                }
                var open_item = Item.GetOpenItem(OItemID, LoadID, line_item.ULOOrgCode, line_item.ReviewerUserID);

                bool bln_update;
                bool line_on_reassign;

                line_on_reassign = LineNumBO.LineOnReassignRequest(OItemID, LineNum, line_item.ULOOrgCode, line_item.ReviewerUserID);
                if (line_on_reassign)
                {
                    lblHeaderLabel.Text = lblHeaderLabel.Text + " unavailable for update, waiting for reassignment.";
                    btnSave.Enabled = false;
                    btnRecalculate.Enabled = false;
                    DisableOrClearControls(true, false);
                }
                else if (open_item.Organization != line_item.Organization)
                    lblHeaderLabel.Text = lblHeaderLabel.Text + " (belong " + line_item.Organization + ")"; //?

                bln_update = LineNumBO.AvailableForUpdate(open_item.IsArchived, (open_item.ParentLoadID > 0),
                    open_item.Organization, line_item.Organization, line_on_reassign, open_item.ReviewerUserID, User, CurrentUserOrganization, CurrentUserID);

                DocNumber = open_item.DocNumber;


                LoadOpenItemDetails(open_item, bln_update);
                LoadFeedbackTable(open_item);
                LoadLineNumDetails(line_item, bln_update, open_item.IsArchived);
                if (open_item.ReviewerUserID == 0)
                {
                    //lblError.Text = lblError.Text + "<br>Item is not assigned yet, therefore can't be edited. If you have admin rights, please return to the Open Items list page and assign the item.";
                }
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
        }

        private void GetDefaultView()
        {
            try
            {
                div_calc_methodology.AddDisplayNone();
                div_projected_lease_info.AddDisplayNone();
                div_breakout_part_year_establ_accr.AddDisplayNone();
                div_breakout_part_yaer_cost.AddDisplayNone();
                div_breakout_part_year_RET.AddDisplayNone();
                div_Reviewer_Reason_Code.AddDisplayNone();
                tbl_btns.AddDisplayNone();

                var dt = LookupsBO.GetBA53AccrualTypes();
                ddgAccrualType.ShowHeader = false;
                ddgAccrualType.Table = dt;
                ddgAccrualType.KeepInViewState = true;


                //if (ddgAccrualType.ItemID == "2" || ddgAccrualType.ItemID == "3")
                //{
                //    // fill in ddgActionType 
                //    DataTable dt_act = Lookups.GetBA53AccrualTypeActions(Convert.ToInt16(ddgAccrualType.ItemID));
                //    ddgActionType.ShowHeader = false;
                //    ddgActionType.Table = dt_act;
                //    ddgActionType.KeepInViewState = true;

                //}

                ddgReasonCode.FillDropDownGridAllColumnsVisibleButID("spGetReviewerReasonCodes", "ReasonCodeID", "ReasonCodeDesc", "", false);
                ddgTeamCode.FillDropDownGridAllColumnsVisibleButID("spGetTeamCodes", "TeamCodeID", "TeamCode", "", false);

                td_state_1.AddDisplayNone();
                td_state_2.AddDisplayNone();

                td_action_1.AddDisplayNone(); ////
                td_action_2.AddDisplayNone();////

                div_tr_reason_for_delay.AddDisplayNone();

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

        private void GetViewByAcctType(string sType)
        {
            try
            {
                td_action_1.AddDisplayNone(); ////
                td_action_2.AddDisplayNone();////

                lblAccrTypeLabel.Text = "Accrual Type:";
                lblAccrualTypeActionLabel.Text = "Accrual Type Action:";
                lblAccrualType.Text = ddgAccrualType.Table.Rows[ddgAccrualType.SelectedIndex][1].ToString();

                //if (ddgActionType.SelectedIndex != -1)
                //{
                //    lblAccrTypeAction.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                //    //lblActionTypeID.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                //}
                //else
                //{
                //    lblAccrTypeAction.Text = "";
                //    //lblActionTypeID.Text = "";
                //}


                if (ddgActionType.SelectedIndex != -1)
                {
                    lblAccrTypeAction.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                    //lblActionTypeID.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][2].ToString();
                }
                else
                {
                    ddgActionType.SelectedIndex = 0;
                    //lblActionTypeID.Text = "";
                }

                hdActionTypeValue.Value = lblAccrTypeAction.Text;

                div_Reviewer_Reason_Code.AddDisplay();
                tbl_btns.AddDisplay();
                SetValid(ddgReasonCode.ItemID);
                td_state_1.AddDisplayNone();
                td_state_2.AddDisplayNone();
                div_tr_reason_for_delay.AddDisplayNone();
                lblAccrTypeAction.Text = "n/a";
                //ddgActionType.SelectedIndex = 0;
                //ddgReasonCode.FillDropDownGridAllColumnsVisibleButID("spGetReviewerReasonCodes", "ReasonCodeID", "ReasonCodeDesc","",false); 
                //ReloadReasonCodeByRbSupportCalc();

                switch (sType)
                {
                    case "1": //CPI
                        ddgActionType.SelectedIndex = 0;
                        lblFY_1_EscalationExpenseDiff.SetReadOnly();
                        lblFY_1_EscalationExpenseDiff.ToolTip = "";
                        lblFY_1_EscalationExpensePercIncrease.SetReadOnly();
                        div_calc_methodology.AddDisplay();//
                        tr_lease_eff_date.AddDisplay();//
                        tr_proj_no.AddDisplayNone();//
                        tr_annual_rent.AddDisplay();//
                        tr_lease_exp_date.AddDisplayNone();//
                        tr_lease_ann_date.AddDisplayNone();//
                        tr_eff_date_action.AddDisplayNone();// calendar control at cacl. meth section
                        tr_new_proj_ann_rent.AddDisplayNone();//

                        div_projected_lease_info.AddDisplayNone();//

                        div_breakout_part_year_establ_accr.AddDisplayNone();//

                        div_breakout_part_yaer_cost.AddDisplay();//
                        tr_FY2.AddDisplay();//
                        tr_FY1.AddDisplay();//
                        tr_FY.AddDisplay();//
                        tr_curr_proj_ann_rent.AddDisplayNone();//
                        tr_lbl_new_proj_ann_rent.AddDisplayNone();//
                        tr_old_proj_ann_rent.AddDisplayNone();//
                        tr_txt_new_proj_ann_rent.AddDisplayNone();//
                        tr_curr_audit_end_date.AddDisplayNone();//
                        tr_f_year_end_date.AddDisplay();//

                        div_breakout_part_year_RET.AddDisplayNone();//
                        lblPartYearCostTitle.Text = "Breakout of Part Year Cost:";
                        //lblFY_1_EscalationExpenseDiff.ToolTip = "Diff. between " + Left(lblFY_1_CPIEscExp.Text, 4) + " Escalation Expenses and " + Left(lblFY_2_CPIEscExp.Text, 4) + " Escalation Expenses";
                        //lblFY_EscalationExpenseDiff.ToolTip = "Diff. between " + Left(lblFYCPIEscProj.Text, 4) + " Escalation Expenses and " + Left(lblFY_1_CPIEscExp.Text, 4) + " Escalation Expenses";

                        break;
                    case "2": //EXPANSION
                        if (ddgActionType.SelectedIndex == 0)
                        {
                            lblAccrTypeAction.Text = "Not selected";
                            //lblActionTypeID.Text = "Not selected";
                        }
                        else
                        {
                            if (ddgActionType.SelectedIndex != -1)
                            {
                                lblAccrTypeAction.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                                //lblActionTypeID.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                            }
                        }

                        td_action_1.AddDisplay();
                        td_action_2.AddDisplay();
                        var dt_exp = LookupsBO.GetBA53AccrualTypeActions(2);
                        ddgActionType.ShowHeader = false;
                        ddgActionType.Table = dt_exp;
                        ddgActionType.KeepInViewState = true;

                        div_calc_methodology.AddDisplay();//
                        tr_lease_eff_date.AddDisplay();//
                        tr_proj_no.AddDisplay();//
                        tr_annual_rent.AddDisplay();//
                        tr_lease_exp_date.AddDisplayNone();//
                        tr_lease_ann_date.AddDisplayNone();//
                        tr_eff_date_action.AddDisplayNone();//
                        tr_new_proj_ann_rent.AddDisplayNone();//
                        tr_proj_no.AddEvenTD();

                        div_projected_lease_info.AddDisplayNone();//

                        div_breakout_part_year_establ_accr.AddDisplayNone();//

                        div_breakout_part_yaer_cost.AddDisplay();//
                        tr_FY2.AddDisplayNone();//
                        tr_FY1.AddDisplayNone();//
                        tr_FY.AddDisplayNone();//
                        tr_curr_proj_ann_rent.AddDisplayNone();//
                        tr_lbl_new_proj_ann_rent.AddDisplayNone();//
                        tr_old_proj_ann_rent.AddDisplayNone();//
                        tr_txt_new_proj_ann_rent.AddDisplayNone();//
                        tr_curr_audit_end_date.AddDisplayNone();//
                        tr_f_year_end_date.AddDisplay();//
                        div_breakout_part_year_RET.AddDisplayNone();//
                        lblPartYearCostTitle.Text = "Breakout of Part Year Cost:";
                        break;
                    case "3": //HOLDOVER
                        if (ddgActionType.SelectedIndex == 0)
                        {
                            lblAccrTypeAction.Text = "Not selected";
                            //lblActionTypeID.Text = "Not selected";
                        }
                        else
                        {
                            if (ddgActionType.SelectedIndex != -1)
                            {
                                lblAccrTypeAction.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][2].ToString();
                                //lblActionTypeID.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                            }
                        }

                        td_action_1.AddDisplay();
                        td_action_2.AddDisplay();
                        var dt_hold = LookupsBO.GetBA53AccrualTypeActions(3);
                        ddgActionType.ShowHeader = false;
                        ddgActionType.Table = dt_hold;
                        ddgActionType.KeepInViewState = true;

                        div_calc_methodology.AddDisplay();//
                        tr_lease_eff_date.AddDisplay();//
                        tr_proj_no.AddDisplayNone();//
                        tr_annual_rent.AddDisplay();//
                        tr_lease_exp_date.AddDisplay();//
                        tr_lease_ann_date.AddDisplayNone();//
                        tr_eff_date_action.AddDisplayNone();//
                        tr_new_proj_ann_rent.AddDisplayNone();//

                        div_projected_lease_info.AddDisplay();//

                        div_breakout_part_year_establ_accr.AddDisplay();//
                        tr_curr_proj_ann_rent_est.AddDisplay();//
                        tr_lbl_new_proj_ann_rent_est.AddDisplay();//
                        tr_old_proj_ann_rent_est.AddDisplayNone();//
                        tr_txt_new_proj_ann_rent_est.AddDisplayNone();//

                        div_breakout_part_yaer_cost.AddDisplay();//
                        tr_FY2.AddDisplayNone();//
                        tr_FY1.AddDisplayNone();//
                        tr_FY.AddDisplayNone();//
                        tr_curr_proj_ann_rent.AddDisplay();//

                        tr_lbl_new_proj_ann_rent.AddDisplay();//
                        tr_txt_new_proj_ann_rent.AddDisplayNone();//
                        tr_old_proj_ann_rent.AddDisplayNone();//
                        tr_curr_audit_end_date.AddDisplay();//
                        tr_f_year_end_date.AddDisplayNone();//
                        div_breakout_part_year_RET.AddDisplayNone();//
                        lblPartYearCostTitle.Text = "Breakout of Part Year Cost:";
                        lblAccr1.Visible = true;
                        lblAccr2.Visible = true;
                        lblAccr1.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                        lblAccr2.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                        break;
                    case "4":  //RET
                        ddgActionType.SelectedIndex = 0;
                        td_state_1.AddDisplay();
                        td_state_2.AddDisplay();
                        ddgState.FillDropDownGridAllColumnsVisibleButID("spGetStates", "State_ID", "State", "", "@TemplateName", "RET", false);
                        ddgState.DropDownGridSelectedItem(iStateID.ToString(), ddgState.Table, true);

                        div_tr_reason_for_delay.AddDisplay();
                        ddgReasonForDelay.FillDropDownGridAllColumnsVisibleButID("spGetReasonForDelay", "ID", "ReasonForDelay", "", false);
                        ddgReasonForDelay.DropDownGridSelectedItem(iReasonForDelayID.ToString(), ddgReasonForDelay.Table, true);

                        if (ddgReasonForDelay.Table.Rows[ddgReasonForDelay.SelectedIndex][1].ToString().ToLower().Trim() == "other")
                        {
                            tr_other_reason_for_delay.AddDisplay();
                        }
                        else
                        {
                            tr_other_reason_for_delay.AddDisplayNone();
                        }

                        div_calc_methodology.AddDisplay();//
                        tr_lease_eff_date.AddDisplay();//
                        tr_proj_no.AddDisplayNone();//
                        tr_annual_rent.AddDisplay();//
                        tr_lease_exp_date.AddDisplayNone();//
                        tr_lease_ann_date.AddDisplayNone();//
                        tr_eff_date_action.AddDisplayNone();//
                        tr_new_proj_ann_rent.AddDisplayNone();//

                        div_projected_lease_info.AddDisplayNone();//

                        div_breakout_part_year_establ_accr.AddDisplayNone();//

                        div_breakout_part_yaer_cost.AddDisplayNone();//

                        div_breakout_part_year_RET.AddDisplay();//
                        lblFYRETEscExp.Text = GetFiscalYear(DateTime.Now, 0).ToString() + " RET Escalation Expense:";
                        lblFY1RETEscExp.Text = GetFiscalYear(DateTime.Now, 1).ToString() + " RET Escalation Expense:";
                        lblFY2_RETEscPtojTitile.Text = GetFiscalYear(DateTime.Now, 2).ToString() + " RET Escalation Projection:";
                        lblFY2_RETEscPtojRevTitile.Text = GetFiscalYear(DateTime.Now, 3).ToString() + " RET Escalation Projection (Revised):";

                        var dt = new DataTable();
                        dt.Columns.Add("Year", typeof(string));
                        ddgCurrFY.InsertToDropDownGrid("- select-", true, dt);
                        var iCurrFY = Convert.ToInt32(GetFiscalYear(DateTime.Now, 0));

                        for (var i = iCurrFY - 10; i < iCurrFY + 3; i++)
                        {
                            ddgCurrFY.InsertToDropDownGrid(i.ToString(), true, dt);
                        }

                        GetRetDatePeriods(ddgCurrFY.SelectedIndex, ddgCurrFY.ItemID);
                        switch (ddgState.ItemID)
                        {
                            case "0":// - select -
                                ddgState.SelectedIndex = 0;
                                break;
                            case "1": //DC
                                tr_RET_lbl1stHalfTaxInvFY2.AddDisplay();
                                tr_RET_1stHalfTaxInvFY1.AddDisplay();
                                break;
                            case "2": //MD
                                tr_RET_lbl1stHalfTaxInvFY2.AddDisplayNone(); //txt1stHalfFY2_TotalTaxBill & txt1stHalfFY2_NoMonthsReimb not visible
                                tr_RET_1stHalfTaxInvFY1.AddDisplayNone();// txt1stHalfFY1_TotalTaxBill & txt1stHalfFY1_NoMonthsReimb  not visible
                                break;
                            case "3": //VA
                                tr_RET_lbl1stHalfTaxInvFY2.AddDisplayNone(); //txt1stHalfFY2_TotalTaxBill & txt1stHalfFY2_NoMonthsReimb not visible
                                tr_RET_1stHalfTaxInvFY1.AddDisplayNone(); // txt1stHalfFY1_TotalTaxBill & txt1stHalfFY1_NoMonthsReimb  not visible
                                break;
                            default:
                                break;
                        }

                        break;
                    case "5": //STEP RENT
                        ddgActionType.SelectedIndex = 0;
                        div_calc_methodology.AddDisplay();//
                        tr_lease_eff_date.AddDisplayNone();//
                        tr_proj_no.AddDisplay();//
                        tr_annual_rent.AddDisplayNone();//
                        tr_lease_exp_date.AddDisplayNone();//
                        tr_lease_ann_date.AddDisplayNone();//
                        tr_eff_date_action.AddDisplay();//
                        tr_new_proj_ann_rent.AddDisplay();//

                        div_projected_lease_info.AddDisplayNone();//

                        div_breakout_part_year_establ_accr.AddDisplay();//
                        tr_curr_proj_ann_rent_est.AddDisplayNone();//
                        tr_lbl_new_proj_ann_rent_est.AddDisplayNone();//
                        tr_old_proj_ann_rent_est.AddDisplay();//
                        tr_txt_new_proj_ann_rent_est.AddDisplay();//

                        div_breakout_part_yaer_cost.AddDisplay();//
                        tr_FY2.AddDisplayNone();//
                        tr_FY1.AddDisplayNone();//
                        tr_FY.AddDisplayNone();//
                        tr_curr_proj_ann_rent.AddDisplayNone();//

                        tr_lbl_new_proj_ann_rent.AddDisplayNone();//
                        tr_txt_new_proj_ann_rent.AddDisplay();//

                        tr_old_proj_ann_rent.AddDisplay();//
                        tr_txt_new_proj_ann_rent.AddDisplay();//
                        tr_curr_audit_end_date.AddDisplayNone();//
                        div_breakout_part_year_RET.AddDisplayNone();//
                        lblPartYearCostTitle.Text = "Breakout of Part Year Cost - Revised";
                        //txtAnnualIncrOfAction.CssClass = "textbox_as_label";
                        txtAnnualIncrOfAction.ReadOnly = true;
                        txtAnnualIncrOfAction.BorderStyle = BorderStyle.None;
                        break;

                    case "6": //BID - the same as RET *
                        ddgActionType.SelectedIndex = 0;
                        td_state_1.AddDisplay();
                        td_state_2.AddDisplay();
                        ddgState.FillDropDownGridAllColumnsVisibleButID("spGetStates", "State_ID", "State", "", "@TemplateName", "BID", false);
                        ddgState.DropDownGridSelectedItem(iStateID.ToString(), ddgState.Table, true);


                        div_tr_reason_for_delay.AddDisplay();
                        ddgReasonForDelay.FillDropDownGridAllColumnsVisibleButID("spGetReasonForDelay", "ID", "ReasonForDelay", "", false);
                        ddgReasonForDelay.DropDownGridSelectedItem(iReasonForDelayID.ToString(), ddgReasonForDelay.Table, true);

                        if (ddgReasonForDelay.Table.Rows[ddgReasonForDelay.SelectedIndex][1].ToString().ToLower().Trim() == "other")
                        {
                            tr_other_reason_for_delay.AddDisplay();
                        }
                        else
                        {
                            tr_other_reason_for_delay.AddDisplayNone();
                        }

                        div_calc_methodology.AddDisplay();//
                        tr_lease_eff_date.AddDisplay();//
                        tr_proj_no.AddDisplayNone();//
                        tr_annual_rent.AddDisplay();//
                        tr_lease_exp_date.AddDisplayNone();//
                        tr_lease_ann_date.AddDisplayNone();//
                        tr_eff_date_action.AddDisplayNone();//
                        tr_new_proj_ann_rent.AddDisplayNone();//

                        div_projected_lease_info.AddDisplayNone();//

                        div_breakout_part_year_establ_accr.AddDisplayNone();//

                        div_breakout_part_yaer_cost.AddDisplayNone();//

                        div_breakout_part_year_RET.AddDisplay();//
                        lblFYRETEscExp.Text = GetFiscalYear(DateTime.Now, 0).ToString() + " RET Escalation Expense:";
                        lblFY1RETEscExp.Text = GetFiscalYear(DateTime.Now, 1).ToString() + " RET Escalation Expense:";
                        lblFY2_RETEscPtojTitile.Text = GetFiscalYear(DateTime.Now, 2).ToString() + " RET Escalation Projection:";
                        lblFY2_RETEscPtojRevTitile.Text = GetFiscalYear(DateTime.Now, 3).ToString() + " RET Escalation Projection (Revised):";

                        var dt1 = new DataTable();
                        dt1.Columns.Add("Year", typeof(string));
                        ddgCurrFY.InsertToDropDownGrid("- select-", true, dt1);
                        var iCurrFY_ = Convert.ToInt32(GetFiscalYear(DateTime.Now, 0));

                        for (var i = iCurrFY_ - 10; i < iCurrFY_ + 3; i++)
                        {
                            ddgCurrFY.InsertToDropDownGrid(i.ToString(), true, dt1);
                        }

                        GetRetDatePeriods(ddgCurrFY.SelectedIndex, ddgCurrFY.ItemID);

                        switch (ddgState.ItemID)
                        {
                            case "0":// - select -
                                ddgState.SelectedIndex = 0;
                                break;
                            case "1": //DC
                                tr_RET_lbl1stHalfTaxInvFY2.AddDisplay();
                                tr_RET_1stHalfTaxInvFY1.AddDisplay();
                                break;
                            case "2": //MD
                                tr_RET_lbl1stHalfTaxInvFY2.AddDisplayNone(); //txt1stHalfFY2_TotalTaxBill & txt1stHalfFY2_NoMonthsReimb not visible
                                tr_RET_1stHalfTaxInvFY1.AddDisplayNone();// txt1stHalfFY1_TotalTaxBill & txt1stHalfFY1_NoMonthsReimb  not visible
                                break;
                            case "3": //VA
                                tr_RET_lbl1stHalfTaxInvFY2.AddDisplayNone(); //txt1stHalfFY2_TotalTaxBill & txt1stHalfFY2_NoMonthsReimb not visible
                                tr_RET_1stHalfTaxInvFY1.AddDisplayNone(); // txt1stHalfFY1_TotalTaxBill & txt1stHalfFY1_NoMonthsReimb  not visible
                                break;
                            default:
                                break;
                        }
                        break;

                    case "7": //CLAIM
                        ddgActionType.SelectedIndex = 0;
                        lblFY_1_EscalationExpenseDiff.SetReadOnly();
                        lblFY_1_EscalationExpensePercIncrease.SetReadOnly();
                        div_calc_methodology.AddDisplay();//
                        tr_lease_eff_date.AddDisplay();//
                        tr_proj_no.AddDisplayNone();//
                        tr_annual_rent.AddDisplay();//
                        tr_lease_exp_date.AddDisplayNone();//
                        tr_lease_ann_date.AddDisplayNone();//
                        tr_eff_date_action.AddDisplayNone();//
                        tr_new_proj_ann_rent.AddDisplayNone();//

                        div_projected_lease_info.AddDisplayNone();//

                        div_breakout_part_year_establ_accr.AddDisplayNone();//

                        div_breakout_part_yaer_cost.AddDisplayNone();//
                        tr_FY2.AddDisplayNone();//
                        tr_FY1.AddDisplayNone();//
                        tr_FY.AddDisplayNone();//
                        tr_curr_proj_ann_rent.AddDisplayNone();//
                        tr_lbl_new_proj_ann_rent.AddDisplayNone();//
                        tr_old_proj_ann_rent.AddDisplayNone();//
                        tr_txt_new_proj_ann_rent.AddDisplayNone();//
                        tr_curr_audit_end_date.AddDisplayNone();//
                        tr_f_year_end_date.AddDisplayNone();//

                        div_breakout_part_year_RET.AddDisplayNone();//
                        lblPartYearCostTitle.Text = "Breakout of Part Year Cost:";
                        break;

                    default:
                        break;
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

        private void AlignDollarTextFields()
        {
            var controlList = new ArrayList();
            AddControls(Page.Controls, controlList);
            foreach (String str in controlList)
            {
                ////Response.Write(str + "<br/>");
                //if (str.IndexOf("txt") != -1)
                //{
                //    lblError.Text = lblError.Text + "<br/>" + str + "<br/>";
                //    TextBox t = new TextBox();
                //    t.ID = str;
                //    t.CssClass = "regTextAlignRight";

                //}
                //TextBox t = new TextBox();
                //if (t.ID = str)
                //{
                //}


                //if (ctr.GetType().ToString() == "System.Web.UI.WebControls.TextBox")
                //{
                //    if (((TextBox)(ctr)).Text.IndexOf("$") != -1)
                //    {
                //        ((TextBox)(ctr)).CssClass = "regTextAlignRight";
                //    }

                //}

            }


        }

        private void AddControls(ControlCollection page, ArrayList controlList)
        {
            foreach (Control c in page)
            {
                if (c.ID != null)
                {
                    controlList.Add(c.ID);
                }


                if (c.HasControls())
                {
                    AddControls(c.Controls, controlList);

                    //if (c.GetType().ToString() == "System.Web.UI.WebControls.TextBox")
                    //{
                    //    if (((TextBox)(c)).Text.IndexOf("$") != -1)
                    //    {
                    //        ((TextBox)(c)).CssClass = "regTextAlignRight";
                    //    }

                    //}
                }

            }
        }

        private void TextBoxesLength()
        {
            var controlList = new ArrayList();
            LimitTextBoxesLength(Page.Controls, controlList);
        }


        private void LimitTextBoxesLength(ControlCollection page, ArrayList controlList)
        {
            foreach (Control c in page)
            {
                if (c.ID != null)
                {
                    if (c.GetType().ToString() == "System.Web.UI.WebControls.TextBox")
                    {
                        if (((TextBox)(c)).MaxLength != 30 && ((TextBox)(c)).MaxLength != 3 && ((TextBox)(c)).MaxLength != 200 && ((TextBox)(c)).MaxLength != 15 && ((TextBox)(c)).MaxLength != 100)
                        {
                            ((TextBox)(c)).MaxLength = 12;// 999 999 999
                        }

                    }
                    controlList.Add(c.ID);
                }

            }

        }

        private void DisableOrClearControls(bool bDisable, bool bClear)
        {
            var controlList = new ArrayList();
            AddControls(Page.Controls, controlList, bDisable, bClear);
        }


        private void AddControls(ControlCollection page, ArrayList controlList, bool bDisable, bool bClear)
        {
            foreach (Control c in page)
            {
                if (c.ID != null)
                {

                    if (c.GetType().ToString() == "System.Web.UI.WebControls.TextBox")
                    {
                        if (((TextBox)(c)).ID != "txtLineNo")
                        {
                            if (bClear == true)
                            {
                                ((TextBox)(c)).Text = "";
                            }
                        }

                        if (bDisable == true)
                        {
                            ((TextBox)(c)).BorderStyle = BorderStyle.None;
                            ((TextBox)(c)).ReadOnly = true;
                        }

                        if (((TextBox)(c)).Text.IndexOf("$") != -1)
                        {
                            ((TextBox)(c)).CssClass = "regTextAlignRight";
                        }
                        else
                        {
                            ((TextBox)(c)).CssClass = "regText";
                        }
                    }




                    if (c.GetType().ToString() == "System.Web.UI.WebControls.RadioButtonList")
                    {
                        if (bClear == true)
                        {
                            ((RadioButtonList)(c)).SelectedIndex = -1;
                        }

                        if (bDisable == true)
                        {
                            ((RadioButtonList)(c)).Enabled = false;
                        }
                    }

                    if (c.GetType().ToString() == "Controls.GetDate")
                    {
                        if (bClear == true)
                        {
                            ((Controls.GetDate)(c)).Clear();
                        }

                        if (bDisable == true)
                        {
                            ((Controls.GetDate)(c)).FreezeMode = true;
                            ((Controls.GetDate)(c)).VoidEdit = true;
                        }
                    }

                    if (c.GetType().ToString() == "Controls.DropDownGrid")
                    {
                        if (bClear == true)
                        {
                            ((Controls.DropDownGrid)(c)).SelectedIndex = 0;
                        }

                        if (bDisable == true)
                        {
                            ((Controls.DropDownGrid)(c)).EnableDropDownGrid(false);
                        }
                    }


                    controlList.Add(c.ID);
                }

                if (c.HasControls())
                {
                    AddControls(c.Controls, controlList, bDisable, bClear);
                }
            }
            foreach (Control c in tblCalcMeth.Rows)
            {

                if (c.GetType().ToString() == "System.Web.UI.WebControls.Label")
                {
                    if (bClear == true)
                    {
                        ((Label)(c)).Text = "";
                    }
                }
            }

            foreach (Control c in tblProjNewLeaseInf.Rows)
            {

                if (c.GetType().ToString() == "System.Web.UI.WebControls.Label")
                {
                    if (bClear == true)
                    {
                        ((Label)(c)).Text = "";
                    }
                }
            }

            foreach (Control c in tblreakoutOfPartYearCostEst.Rows)
            {

                if (c.GetType().ToString() == "System.Web.UI.WebControls.Label")
                {
                    if (bClear == true)
                    {
                        ((Label)(c)).Text = "";
                    }
                }
            }
            foreach (Control c in tblPartYearCostTitle.Rows)
            {

                if (c.GetType().ToString() == "System.Web.UI.WebControls.Label")
                {
                    if (bClear == true)
                    {
                        ((Label)(c)).Text = "";
                    }
                }
            }
            foreach (Control c in tblRETDetails.Rows)
            {

                if (c.GetType().ToString() == "System.Web.UI.WebControls.Label")
                {
                    if (bClear == true)
                    {
                        ((Label)(c)).Text = "";
                    }
                }
            }
            //tblreakoutOfPartYearCostEst  tblPartYearCostTitle  tblRETDetails
        }



        private void LoadOpenItemDetails(OpenItem open_item, bool EnableForUpdate)
        {
            try
            {
                var cc = tr_new_proj_ann_rent.Controls;
                lblOrgCode.Text = open_item.ULOOrgCode;

                txtItemID.Value = open_item.OItemID.ToString(); // hidden
                lblDocNumber.Text = open_item.DocNumber;
                txtReviewerUserID.Value = open_item.ReviewerUserID.ToString();

                if (open_item.IsArchived)
                {
                    lblStatus.Text = open_item.Status + " (Archived)";
                }

                else
                {
                    lblStatus.Text = open_item.Status;
                }

                lblDueDate.Text = Utility.DisplayDateFormat(open_item.DueDate, "MMM dd, yyyy");

                if (open_item.DueDate <= DateTime.Now && !open_item.IsArchived)
                    lblDueDate.CssClass = "regBldRedText";

                //lblAccrualType.Text = ddlAccrualType.SelectedItem.Value;
                ctrlContacts.ContrOfficerByCO = open_item.ContrOfficerByCO;


                if (!BlnForUpdate)
                {
                    txtRemarks.Enabled = false;
                    btnSave.Enabled = false;
                    btnSave.Visible = false;
                    btnReviewer0.Disabled = true;
                    btnReviewer0.Visible = false;
                    ctrlContacts.Enabled = false;
                    DisableOrClearControls(true, false);
                    ctrlContacts.Enabled = false;
                    ctrlAttachments.ShowAddAttBtn(false);
                }
                else
                {
                    ctrlAttachments.ShowAddAttBtn(true);
                    ctrlContacts.Enabled = true;

                    if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                        txtReassignTargetPage.Value = "Reroute";
                    else if (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                        txtReassignTargetPage.Value = "Reassign";
                    else
                        txtReassignTargetPage.Value = "ReassignRequest";
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

        private void LoadFeedbackTable(OpenItem item)
        {
            if (item.ParentLoadID == 0)
                rpFeedback.Visible = false;
            else
            {
                // //load Feedback details:
                // DataSet ds = Item.GetFeedbackRecords(OItemID);
                // if (ds == null || ds.Tables[0].Rows.Count == 0)
                // {
                //     rpFeedback.Visible = false;
                //     return;
                // }
                //// rpFeedback.
                // rpFeedback.DataSource = ds.Tables[0].DefaultView;
                // rpFeedback.DataBind();


                // foreach (RepeaterItem repeatItem in rpFeedback.Items)
                // {    // if condition to add HeaderTemplate Dynamically only Once    
                //     if (repeatItem.ItemIndex == 0)    
                //     {
                //         RepeaterItem headerItem = new RepeaterItem(repeatItem.ItemIndex, ListItemType.Header);
                //         HtmlGenericControl hTag = new HtmlGenericControl("h4");       
                //         hTag.InnerHtml = "Test";       
                //         repeatItem.Controls.Add(hTag);   
                //     }
                // }
                //lblError.Text = "The follow-up review functionality is not available for the BA53 items";
                DisableOrClearControls(true, false);
            }
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
                        User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) && CurrentUserOrganization == lblOrganization.Value ||
                        User.IsInRole(((int)UserRoles.urReviewer).ToString()) && CurrentItemReviewerID == CurrentUserID))
                    {
                        tblFeedback.Style.Add("border", "solid 2px red");

                        //set value for editing item properties:
                        FeedbackForUpdate = true;

                        //get active fields:
                        var ddlFdbValid = (DropDownList)e.Item.FindControl("ddlFdbValid");
                        ddlFdbValid.Visible = true;
                        ddlFdbValid.DataSource = LookupsBO.GetValidationValuesList();
                        ddlFdbValid.DataValueField = "Valid";
                        ddlFdbValid.DataTextField = "ValidValueDescription";
                        ddlFdbValid.DataBind();
                        ddlFdbValid.SelectedValue = String.Format("{0}", (int)dr["Valid"]);
                        var txtFdbResponse = (TextBox)e.Item.FindControl("txtFdbResponse");
                        txtFdbResponse.Visible = true;
                        txtFdbResponse.Text = (string)Utility.GetNotNullValue(dr["Response"], "String");
                        txtFdbResponse.BackColor = Color.Yellow;
                        ddlFdbValid.BackColor = Color.Yellow;

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

        private void SaveItemProperties(DateTime dtCompDate, string strUDO, string strDO, string strComments, string strReviewer)
        {
            var comment = strComments.Trim();
            var reviewer = Int32.Parse(strReviewer);

            if (Item.UpdateItemProperties(OItemID, LoadID, OrgCode, strUDO, strDO, dtCompDate, comment, reviewer, CurrentUserID))
                ItemsDataView = null;
        }

        public String GetCallbackResult()
        {
            return returnValue;
        }

        private void LoadLineNumDetails(LineBA53 line_item, bool EnableForUpdate, bool IsArchived)
        {
            try
            {
                GetDefaultView();
                ddgReasonCode.SelectedIndex = line_item.ReviewerReasonCode;
                //ddgTeamCode.SelectedIndex = line_item.TeamCode;
                ddgTeamCode.DropDownGridSelectedItemByValue(line_item.TeamCode, ddgTeamCode.Table, true);
                ddgAccrualType.SelectedIndex = line_item.AccrualType;

                if (ddgAccrualType.ItemID == "2" || ddgAccrualType.ItemID == "3")
                {
                    // fill in ddgActionType 
                    var dt_act = LookupsBO.GetBA53AccrualTypeActions(Convert.ToInt16(ddgAccrualType.ItemID));
                    ddgActionType.ShowHeader = false;
                    ddgActionType.Table = dt_act;
                    ddgActionType.KeepInViewState = true;

                    ddgActionType.DropDownGridSelectedItemByValue(line_item.AccrTypeActionDesc, dt_act, true);

                }
                else
                {
                    ddgActionType.SelectedIndex = 0;
                }

                //ddgActionType.SelectedIndex = line_item.AccrualTypeAction;
                //ddgActionType.DropDownGridSelectedItem(lblAccrTypeAction.Text,

                iStateID = line_item.StateID;
                iReasonForDelayID = line_item.ReasonForDelayID;
                SetValid(ddgReasonCode.ItemID);
                txtReviewerEmail.Value = Admin.GetEmailByUserID(Convert.ToInt32(line_item.ReviewerUserID.ToString()));


                if (line_item.AccrualType == 0)
                {
                    //GetDefaultView();
                    lblAccrTypeLabel.Text = "Accrual Type:";
                }
                else
                {
                    lblAccrTypeLabel.Text = "Accrual Type:";
                    GetViewByAcctType(line_item.AccrualType.ToString());
                }

                //if (line_item.AccrualTypeAction == 0)
                //{
                //    //GetDefaultView();
                //    lblAccrualTypeActionLabel.Text = "Accrual Type Action:";
                //}
                //else
                //{
                //    lblAccrTypeLabel.Text = "Accrual Type Action:";
                //    GetViewByAcctType(line_item.AccrualTypeAction.ToString());
                //}

                lblLineNumber.Text = line_item.LineNumID.ToString();

                lblReviewer.Text = line_item.Reviewer + " (" + line_item.Organization + ")";
                lblOrganization.Value = line_item.Organization;
                //txtTeamCode.Text = line_item.TeamCode;
                //lblServiceCenter.Text = line_item.ServiceCenter.ToString();
                lblLeaseNum.Text = line_item.LeaseNumber;
                lblTypeOfLease.Text = line_item.TypeOfLease;
                lblACCTLNUM.Text = line_item.AcctLNumber.ToString(); ;
                lblBA.Text = line_item.BA.ToString(); ;
                lblACCRUAL.Text = Utility.DisplayMoneyFormat(line_item.Arrcual); ;
                //lblAudAmtDO.Text = Utility.DisplayMoneyFormat(line_item.AuditedAmtDO);
                lblBuilding.Text = line_item.Building;
                lblTitle.Text = line_item.Title.ToString();
                lblLineNumber.Text = line_item.LineNumID.ToString();
                lblFC.Text = line_item.FC.ToString();
                lblPENDPYMT.Text = Utility.DisplayMoneyFormat(line_item.PendPymt);
                lblVendorName.Text = line_item.VendName.ToString();
                lblBBFY.Text = line_item.BBFY.ToString();
                lblOC.Text = line_item.OC.ToString();

                // this is the calculated value: lblACCRUAL.Text + lblPENDPYMT.Text
                line_item.TotalDO = line_item.Arrcual + line_item.PendPymt;
                lblTotalDO.Text = Utility.DisplayMoneyFormat(line_item.TotalDO);
                txtTotalDOShouldBe.Text = Utility.DisplayMoneyFormat(line_item.TotalDOShouldBe);
                lblVendorNumber.Text = line_item.VendNum;
                lblEBFY.Text = line_item.EBFY.ToString();
                // lblACTG_PD is unvisible
                lblACTG_PD.Text = Utility.DisplayDateFormat(line_item.ACTG_PD, "MMM dd, yyyy");
                lblCE.Text = line_item.CE.ToString();
                lblUDO.Text = Utility.DisplayMoneyFormat(line_item.UDO);

                if (line_item.FileContainsSupport == true) //Yes
                {
                    rbSupportCalc.SelectedIndex = 0;
                }
                else //No
                {
                    rbSupportCalc.SelectedIndex = 1;
                    //ReloadReasonCodeByRbSupportCalc();
                }

                if (line_item.PriorYearHistoryAttached == true)
                {
                    rbPmtHistory.SelectedIndex = 0;
                }
                else
                {
                    rbPmtHistory.SelectedIndex = 1;
                }

                txtRemarks.Text = line_item.Remarks;

                txtTotalDOShouldBe.Text = Utility.DisplayMoneyFormat(line_item.TotalDOShouldBe);
                //txtTeamCode.Text = line_item.TeamCode.ToString();
                // SM on 01/26/2012 changed per Fatimo request -  Team Code is the same as Sort Code
                lblSortCode.Text = line_item.TeamCode.ToString();  // line_item.SortCode.ToString();
                txtREABudgetAnalyst.Text = line_item.Signature1.ToString();
                txtCFOBudgetAnalyst.Text = line_item.Signature2.ToString();
                DisplayDate(calREABudgetAnalystDate, line_item.SignatureDate1);
                DisplayDate(calCFOBudgetAnalystDate, line_item.SignatureDate2);
                DisplayDate(calReportDate, line_item.ReportDate);


                // ------- Calculation methodology section -----------------------
                //Current Lease info
                lblLeaseNum.Text = line_item.LeaseNumber.ToString();
                DisplayDate(calEffDateOfAction5, line_item.EffectiveDateOfAction5);
                txtProjProjectNo.Text = line_item.ProjectNumber.ToString();
                DisplayDate(calLeasAnnDate, line_item.LeaseAnniversaryDate);
                txtNewProjAnnualRent.Text = Utility.DisplayMoneyFormat(line_item.NewProjectedAnnualRent);
                DisplayDate(calLeaseEffDate, line_item.LeaseEffectiveDate);
                txtAnnualRent.Text = Utility.DisplayMoneyFormat(line_item.AnnualRent);
                //lblCurrProjAnnRentEst.Text = txtAnnualRent.Text;
                txtRSF.Text = DisplayDecimalFormat(line_item.RSF).ToString();
                DisplayDate(calLeaseExpDate, line_item.LeaseExpirationDate);

                // Projected lease info
                txtPtojNewLeaseNo.Text = line_item.ProjectedLeaseNo.ToString();
                txtProjProjectNo.Text = line_item.ProjectedProjectNo.ToString();
                txtProjProspNo.Text = line_item.ProjectedProspectusNo.ToString();
                DisplayDate(calProjLeaseEffDate, line_item.ProjectedsLeaseEffectiveDate);
                txtProjAnnualRent.Text = Utility.DisplayMoneyFormat(line_item.ProjectedAnnualRent);
                //lblNewProjAnnRent_est.Text = txtProjAnnualRent.Text;
                txtProjRSF.Text = DisplayDecimalFormat(line_item.ProjectedRSF).ToString();
                // ---------------------------------------------------------------


                // ------ Breakout of Part Year Cost of Established Accrual Section ---------------
                lblCurrProjAnnRentEst.Text = Utility.DisplayMoneyFormat(line_item.EstablishedCurrentProjAnnualRent);
                txtAnnualRent.Text = Utility.DisplayMoneyFormat(line_item.AnnualRent);
                txtOldProjAnnRent_est.Text = Utility.DisplayMoneyFormat(line_item.EstablishedOldProjAnnualRent);
                txtProjAnnualRent.Text = Utility.DisplayMoneyFormat(line_item.EstablishedNewProjAnnualRent);
                lblNewProjAnnRent_est.Text = txtProjAnnualRent.Text;
                lblMonthlyIncreaseEst.Text = Utility.DisplayMoneyFormat(line_item.EstablishedMonthlyIncrease);// calculated value   
                DisplayDate(calEffDateOfActionEst, line_item.EstablishedEffectiveDateOfAction);
                lblTotalMonthsEst.Text = line_item.EstablishedTotalMonths.ToString();               // calculated value   
                DisplayDate(calFiscalYearEndDateEst, line_item.EstablishedFYEndDate);
                lblTotalMonthsCatchUpEst.Text = Utility.DisplayMoneyFormat(line_item.EstablishedTotalMonthsCatchUp); // calculated value   
                txtNoDaysProratedMonthEst.Text = line_item.EstablishedNumberDaysProratedMonth.ToString();

                lblDaysProratedEst.Text = line_item.EstablishedDaysProrated.ToString();             // calculated value   
                lblTotalDayCatchUpEst.Text = Utility.DisplayMoneyFormat(line_item.EstablishedTotalDayCatchUp);       // calculated value   
                lblTotalCatchUpEst.Text = Utility.DisplayMoneyFormat(line_item.EstablishedTotalCatchUp);             // calculated value   

                if (lblDaysProratedEst.Text == "0")
                {
                    lblDaysProratedEst.Text = "";
                }

                if (lblTotalDayCatchUpEst.Text == "$00.00")
                {
                    lblTotalDayCatchUpEst.Text = "";
                }

                if (lblTotalCatchUpEst.Text == "$00.00")
                {
                    lblTotalCatchUpEst.Text = "";
                }

                // ---------------------------------------------------------------------------------------------------

                //---- Breakout of Part Year Cost / Breakout of Part Year Cost Revised section -----------------------
                txtCurrProjNo.Text = line_item.ProjectNumber;
                DisplayDate(calFiscalYearEndDate, line_item.FYEndDate);
                txtFY_2_CPIEscExp.Text = Utility.DisplayMoneyFormat(line_item.FY_2_CPI_EscalationExpense);
                txtFY_1_CPIEscExp.Text = Utility.DisplayMoneyFormat(line_item.FY_1_CPI_EscalationExpense);
                txtFYCPIEscProj.Text = Utility.DisplayMoneyFormat(line_item.FY_CPI_EscalationProjection);
                lblCurrProjAnnRent.Text = Utility.DisplayMoneyFormat(line_item.CurrProjectedAnnualRent);
                txtOldProjAnnRent.Text = Utility.DisplayMoneyFormat(line_item.OldProjectedAnnualRent);

                if (ddgAccrualType.ItemID == "3") // holdover
                {
                    lblCurrProjAnnRentEst.Text = Utility.DisplayMoneyFormat(txtAnnualRent.Text);
                    lblNewProjAnnRent.Text = lblCurrProjAnnRentEst.Text;
                    lblNewProjAnnRent_est.Text = Utility.DisplayMoneyFormat(txtProjAnnualRent.Text);
                    lblAnnIncrOfActionEst.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblNewProjAnnRent_est.Text, "Decimal")) - Convert.ToDecimal(FormatNull(lblCurrProjAnnRentEst.Text, "Decimal")));

                    lblCurrProjAnnRent.Text = Utility.DisplayMoneyFormat(txtAnnualRent.Text);
                    lblNewProjAnnRent.Text = Utility.DisplayMoneyFormat(txtProjAnnualRent.Text);
                    txtAnnualIncrOfAction.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblNewProjAnnRent.Text, "Decimal")) - Convert.ToDecimal(FormatNull(lblCurrProjAnnRent.Text, "Decimal")));
                    txtAnnualIncrOfAction.ReadOnly = true;
                    //txtAnnualIncrOfAction.CssClass = "textbox_as_label";
                    txtAnnualIncrOfAction.ReadOnly = true;
                    txtAnnualIncrOfAction.BorderStyle = BorderStyle.None;
                }
                else if (ddgAccrualType.ItemID == "5") // STEP RENT
                {
                    txtNewProjAnnRent_est.Text = Utility.DisplayMoneyFormat(line_item.EstablishedNewProjAnnualRent);
                    txtOldProjAnnRent_est.Text = Utility.DisplayMoneyFormat(line_item.EstablishedOldProjAnnualRent);
                    lblAnnIncrOfActionEst.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txtNewProjAnnRent_est.Text, "Decimal")) - Convert.ToDecimal(FormatNull(txtOldProjAnnRent_est.Text, "Decimal")));

                    txtNewProjAnnRent.Text = Utility.DisplayMoneyFormat(line_item.RevisedNewProjAnnualRent); // revised section
                    txtOldProjAnnRent.Text = Utility.DisplayMoneyFormat(line_item.OldProjectedAnnualRent);
                    txtAnnualIncrOfAction.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txtNewProjAnnRent.Text, "Decimal")) - Convert.ToDecimal(FormatNull(txtOldProjAnnRent.Text, "Decimal")));
                    txtAnnualIncrOfAction.ReadOnly = true;
                    //txtAnnualIncrOfAction.CssClass = "textbox_as_label";
                    txtAnnualIncrOfAction.ReadOnly = true;
                    txtAnnualIncrOfAction.BorderStyle = BorderStyle.None;
                }
                else
                {
                    txtAnnualIncrOfAction.Text = Utility.DisplayMoneyFormat(line_item.AnnualIncreaseOfAction);
                    lblAnnIncrOfActionEst.Text = Utility.DisplayMoneyFormat(line_item.EstablishedAnnualIncreaseOfAction);
                }



                lblFY_1_EscalationExpenseDiff.Text = Utility.DisplayMoneyFormat(line_item.FY_1_EscalationExpenseDiff);
                lblFY_1_EscalationExpensePercIncrease.Text = line_item.FY_1_EscalationExpensePercIncrease.ToString() + "%";
                // -----------------------------------------------------------------------------------------------

                // ------ values below come from database, the same values can be recalculated using Recalculate(line_item) void -----

                DisplayDate(calEffDateOfAction, line_item.EffectiveDateOfAction);
                DisplayDate(calCurrAuditEndDate, line_item.CurrAuditEndDate);
                lblFY_EscalationExpenseDiff.Text = Utility.DisplayMoneyFormat(line_item.FY_EscalationExpenseDiff);
                lblFY_EscalationExpensePercIncrease.Text = line_item.FY_EscalationExpensePercIncrease.ToString() + "%";
                lblMonthlyIncrease.Text = Utility.DisplayMoneyFormat(line_item.MonthlyIncrease);
                lblTotalMonths.Text = line_item.TotalMonths.ToString();
                lblTotalMonthsCatchUp3_5.Text = Utility.DisplayMoneyFormat(line_item.TotalMonthsCatchUp);
                lblTotalMonthsCatchUp1_2.Text = Utility.DisplayMoneyFormat(line_item.TotalMonthsCatchUp);
                txtNoDaysProratedMonth.Text = line_item.NumberDaysProratedMonth.ToString();

                lblDatesProrated.Text = line_item.DaysProrated.ToString();
                if (lblDatesProrated.Text == "0")
                {
                    lblDatesProrated.Text = "";
                }

                lblTotalDayCatchUp.Text = Utility.DisplayMoneyFormat(line_item.TotalDayCatchUp);
                if (lblTotalDayCatchUp.Text == "$00.00")
                {
                    lblTotalDayCatchUp.Text = "";
                }

                lblTotalCatchUp.Text = Utility.DisplayMoneyFormat(line_item.TotalCatchUp);
                if (lblTotalCatchUp.Text == "$00.00")
                {
                    lblTotalCatchUp.Text = "";
                }
                //txtAnnualIncrOfAction.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txtNewProjAnnRent.Text, "Decimal")) - Convert.ToDecimal(FormatNull(txtOldProjAnnRent.Text, "Decimal")));
                // --------------------------------------------------------------------------------------------------------------------

                //-----  Breakout of Part Year Cost section FY and FY+1, for accr type "RET Details"(4) ONLY ----
                // --- FY ---
                //lblFYXX
                txt1stHalfTaxInvFY_DatePeriod.Text = line_item.FY_FirstHalfTaxInvoiceDatePeriod.ToString();  // calculated value for new item
                txt1stHalfFY_TotalTaxBill.Text = Utility.DisplayMoneyFormat(line_item.FY_FirstHalfTaxInvoiceTotalTaxBill);
                txt1stHalfFY_NoMonthsReimb.Text = line_item.FY_FirstHalfTaxInvoiceNoOfMonReimb.ToString();
                lbl1stHalfFY_ReimbAmt.Text = Utility.DisplayMoneyFormat(line_item.FY_FirstHalfTaxInvoiceReimbAmt);

                txt2ndHalfTaxInvFY_DatePeriod.Text = line_item.FY_SecondHalfTaxInvoiceDatePeriod.ToString();
                txt2ndHalfFY_TotalTaxBill.Text = Utility.DisplayMoneyFormat(line_item.FY_SecondHalfTaxInvoiceTotalTaxBill);
                txt2ndHalfFY_NoMonthsReimb.Text = line_item.FY_SecondHalfTaxInvoiceNoOfMonReimb.ToString();
                lbl2ndHalfFY_ReimbAmt.Text = Utility.DisplayMoneyFormat(line_item.FY_SecondHalfTaxInvoiceReimbAmt);

                txt1stHalfTaxInvFY1_DatePeriod.Text = line_item.FY1_FirstHalfTaxInvoiceDatePeriod.ToString();
                txt1stHalfFY1_TotalTaxBill.Text = Utility.DisplayMoneyFormat(line_item.FY1_FirstHalfTaxInvoiceTotalTaxBill);
                txt1stHalfFY1_NoMonthsReimb.Text = line_item.FY1_FirstHalfTaxInvoiceNoOfMonReimb.ToString();
                lbl1stHalfFY1_ReimbAmt.Text = Utility.DisplayMoneyFormat(line_item.FY1_FirstHalfTaxInvoiceReimbAmt);

                lblFY_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(line_item.FY_TaxBillReceiptTotal);
                lblFY_TaxBillReceipt_NoMonthReimb.Text = line_item.FY_TaxBillReceiptNoOfMonReimb.ToString();
                lblFY_TaxBillReceipt_Reimb.Text = Utility.DisplayMoneyFormat(line_item.FY_TaxBillReceiptReimbAmt);

                txtFY_TaxBaseYear.Text = Utility.DisplayMoneyFormat(line_item.FY_TaxBaseYearReimb);
                if (Convert.ToDecimal(Utility.GetDecimalFromDisplayedMoney(txtFY_TaxBaseYear.Text)) < 0)
                {
                    txtFY_TaxBaseYear.CssClass = "regRedText";
                }

                lblFY_NetAmountReimb.Text = Utility.DisplayMoneyFormat(line_item.FY_NetAmountReimb);

                txtFY_PercGovOccupReimb.Text = Utility.DisplayDecimalFormat(line_item.FY_PercOfGovOccupancyReimb);

                lblFY_AmtDueLessorReimb.Text = Utility.DisplayMoneyFormat(line_item.FY_AmountDueLessorReimb);

                // --- FY+1 ---
                txt1stHalfTaxInvFY1_DatePeriod_.Text = line_item.FY1_FirstHalfTaxInvoiceDatePeriod.ToString();
                txt1stHalfFY1_TotalTaxBill_.Text = Utility.DisplayMoneyFormat(line_item.FY1_FirstHalfTaxInvoiceTotalTaxBill);
                txt1stHalfFY1_NoMonthsReimb_.Text = line_item.FY1_FirstHalfTaxInvoiceNoOfMonReimb.ToString();
                lbl1stHalfFY1_ReimbAmt_.Text = Utility.DisplayMoneyFormat(line_item.FY1_FirstHalfTaxInvoiceReimbAmt);

                txt2ndHalfTaxInvFY1_DatePeriod.Text = line_item.FY1_SecondHalfTaxInvoiceDatePeriod.ToString();
                txt2ndHalfFY1_TotalTaxBill.Text = Utility.DisplayMoneyFormat(line_item.FY1_SecondHalfTaxInvoiceTotalTaxBill);
                txt2ndHalfFY1_NoMonthsReimb.Text = line_item.FY1_SecondHalfTaxInvoiceNoOfMonReimb.ToString();
                lbl2ndHalfFY1_ReimbAmt.Text = Utility.DisplayMoneyFormat(line_item.FY1_SecondHalfTaxInvoiceReimbAmt);

                txt1stHalfTaxInvFY2_DatePeriod.Text = line_item.FY2_FirstHalfTaxInvoiceDatePeriod.ToString();
                txt1stHalfFY2_TotalTaxBill.Text = Utility.DisplayMoneyFormat(line_item.FY2_FirstHalfTaxInvoiceTotalTaxBill);
                txt1stHalfFY2_NoMonthsReimb.Text = line_item.FY2_FirstHalfTaxInvoiceNoOfMonReimb.ToString();
                lbl1stHalfFY2_ReimbAmt.Text = Utility.DisplayMoneyFormat(line_item.FY2_FirstHalfTaxInvoiceReimbAmt);

                lblFY1_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(line_item.FY1_TaxBillReceiptTotal);
                lblFY1_TaxBillReceipt_NoMonthReimb.Text = line_item.FY1_TaxBillReceiptNoOfMonReimb.ToString();
                lblFY1_TaxBillReceipt_Reimb.Text = Utility.DisplayMoneyFormat(line_item.FY1_TaxBillReceiptReimbAmt);

                txtFY1_TaxBaseYear.Text = Utility.DisplayMoneyFormat(line_item.FY1_TaxBaseYearReimb);
                if (Convert.ToDecimal(Utility.GetDecimalFromDisplayedMoney(txtFY1_TaxBaseYear.Text)) < 0)
                {
                    txtFY1_TaxBaseYear.CssClass = "regRedText";
                }
                lblFY1_NetAmountReimb.Text = Utility.DisplayMoneyFormat(line_item.FY1_NetAmountReimb);
                txtFY1_PercGovOccup.Text = Utility.DisplayDecimalFormat(line_item.FY1_PercOfGovOccupancyReimb);
                lblFY1_AmtDueLessor.Text = Utility.DisplayMoneyFormat(line_item.FY1_AmountDueLessorReimb);
                //lblFY_AmtDueLessor.Text = Utility.DisplayMoneyFormat(line_item.FY1_AmountDueLessorReimb);

                // --- FY+2 ---
                lblFY2_RETEscProjNetIncrease.Text = Utility.DisplayMoneyFormat(line_item.FY2_RETEscalationProjectionNetIncr);
                txtFY2_RETEscProjPercIncrease.Text = line_item.FY2_RETEscalationProjectionPercIncr.ToString();
                txtFY2_RETEscProjAccrual.Text = Utility.DisplayMoneyFormat(line_item.FY2_RETEscalationProjectionAccr);

                lblFY2_RETEscProjNetIncreaseRev.Text = Utility.DisplayMoneyFormat(line_item.FY2_RETEscalationProjectionRevNetIncr);
                txtFY2_RETEscProjPercIncreaseRev.Text = line_item.FY2_RETEscalationProjectionRevPercIncr.ToString();
                txtFY2_RETEscProjAccrualRev.Text = Utility.DisplayMoneyFormat(line_item.FY2_RETEscalationProjectionRevAccr);
                // ---------------------------------------------------------------------------------------------------

                if (calEffDateOfAction.isEmpty == true)
                {
                    lblFY_2_CPIEscExp.Text = "[FY-2] CPI Escalation Expense";
                    lblFY_1_CPIEscExp.Text = "[FY-1] CPI Escalation Expense:";
                    lblFYCPIEscProj.Text = "[Current FY] CPI Escalation Projection:";
                }
                else
                {
                    lblFY_2_CPIEscExp.Text = GetFiscalYear(calEffDateOfAction.Date, -2) + " Escalation Expense:";
                    lblFY_1_CPIEscExp.Text = GetFiscalYear(calEffDateOfAction.Date, -1) + " Escalation Expense:";
                    lblFYCPIEscProj.Text = GetFiscalYear(calEffDateOfAction.Date, 0) + " Escalation Projection:";
                }


                if (calEffDateOfAction.isEmpty == true)
                {
                    lblFY_YY.Text = "FY";
                }
                else
                {
                    lblFY_YY.Text = "FY" + Right(calEffDateOfAction.Date.Year.ToString(), 2);
                }


                if (calEffDateOfActionEst.isEmpty == true)
                {
                    lblFY_YY_Est.Text = "FY";
                }
                else
                {
                    lblFY_YY_Est.Text = "FY" + Right(calEffDateOfActionEst.Date.Year.ToString(), 2);
                }

                lblFY1.Text = line_item.FY_YY_est.ToString();
                ddgCurrFY.DropDownGridSelectedItem(line_item.FY_YY.ToString(), ddgCurrFY.Table, true);

                if (ddgAccrualType.ItemID == "4") //RET
                {
                    GetRetDatePeriods(ddgCurrFY.SelectedIndex, ddgCurrFY.ItemID);
                }

                if (ddgAccrualType.ItemID == "6") //BID
                {
                }

                if (ddgActionType.SelectedIndex != -1)
                {
                    lblAccr1.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                    lblAccr2.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                }
                txtOtherReasonForDelay.Text = line_item.OtherReasonForDelay;

                //Recalculate(line_item);

                //line_item.IsArchived

                if (IsArchived == true)
                {
                    DisableOrClearControls(true, false);
                    btnRecalculate.Visible = false;
                }

                GetRetDatePeriods(ddgCurrFY.SelectedIndex, ddgCurrFY.ItemID);

                //lblStatus.Text = open_item.Status + " (Archived)";

                if (!BlnForUpdate)
                {
                    btnSave.Enabled = false;
                    btnSave.Visible = false;
                    btnRecalculate.Visible = false;
                    btnReviewer0.Disabled = true;
                    btnReviewer0.Visible = false;
                    ctrlContacts.Enabled = false;
                    ctrlAttachments.ShowAddAttBtn(false);
                    DisableOrClearControls(true, false);
                }
                else
                {
                    ctrlContacts.Enabled = true;
                    ctrlAttachments.ShowAddAttBtn(true);
                }

                lblFY_1_EscalationExpenseDiff.ToolTip = "Diff. between " + Left(lblFY_1_CPIEscExp.Text, 4) + " Escalation Expenses and " + Left(lblFY_2_CPIEscExp.Text, 4) + " Escalation Expenses";
                lblFY_EscalationExpenseDiff.ToolTip = "Diff. between " + Left(lblFYCPIEscProj.Text, 4) + " Escalation Expenses and " + Left(lblFY_1_CPIEscExp.Text, 4) + " Escalation Expenses";

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


        public int GetFiscalYear(DateTime d, int iShift)
        {
            var month = d.Month;
            var year = d.Year;

            if (month > 9)
            {
                return year + 1 + iShift;
            }
            else
            {
                return year + iShift;
            }

        }

        public DateTime FiscalYearEndDate()
        {
            // get the current month and year 
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;
            // if month is October or later, the FY ends 9/30 next year
            // if else,  it ends 9-30 of this year
            return month > 9 ? new DateTime(year + 1, 9, 30) : new DateTime(year, 9, 30);
        }

        public DateTime FiscalYearStartDate()
        {
            // get the current month and year 
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;
            // if month is october or later, the FY started 10-1 of this year
            // if else, it started 10-1 of last year
            return month > 9 ? new DateTime(year, 10, 1) : new DateTime(year - 1, 10, 1);
        }


        protected void DisplayDate(Controls.GetDate cal, DateTime d)
        {
            if (d != DateTime.MinValue)
            {
                cal.Date = d;
            }
            else
            {
                //cal.Date = DateTime.MinValue;
            }
            cal.MinDate = DateTime.Parse("01/01/1990");
            cal.MaxDate = DateTime.Now.AddYears(10);
            cal.VoidEmpty = false;
        }

        protected void PaintButtons()
        {
            if (!BlnForUpdate) // get controls disabled/hidden
            {
                txtRemarks.Enabled = false;
                btnSave.Enabled = false;
                btnSave.Visible = false;
                btnReviewer0.Disabled = true;
                btnReviewer0.Visible = false;
                ctrlContacts.Enabled = false;
                ctrlAttachments.ShowAddAttBtn(false);
            }
            else // get controls enabled/shown
            {
                ctrlContacts.Enabled = true;
                ctrlAttachments.ShowAddAttBtn(true);


                if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                    txtReassignTargetPage.Value = "Reroute";
                else if (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                    txtReassignTargetPage.Value = "Reassign";
                else
                    txtReassignTargetPage.Value = "ReassignRequest";
            }
        }

        protected void ddgAccrualType_changed(object sender, EventArgs e)
        {
            var sAccrTypeCode = ddgAccrualType.ItemID;
            GetViewByAcctType(sAccrTypeCode);
            //lblAccrualType.Text = "";
            //lblAccr1.Text = ddgAccrualType.
            //lblAccr2.Text = 
            PaintButtons();
            ddgActionType.SelectedIndex = 0;
            if (ddgAccrualType.ItemID == "2" || ddgAccrualType.ItemID == "3")
            {
                lblAccrTypeAction.Text = "Not selected";
                //lblActionTypeID.Text = "Not selected";
            }
            else
            {
                lblAccrTypeAction.Text = "n/a";
                // lblActionTypeID.Text = "n/a";
            }

        }

        protected void ddgReasonCode_changed(object sender, EventArgs e)
        {
        }


        //protected void ddgReasonCode_changed(object sender, EventArgs e)
        //{
        //    SetValid(ddgReasonCode.ItemID);
        //    PaintButtons();

        //}

        protected void ddgCurrFY_changed(object sender, EventArgs e)
        {
            GetRetDatePeriods(ddgCurrFY.SelectedIndex, ddgCurrFY.ItemID);
            PaintButtons();
        }


        //protected void ddgReasonForDelay_changed(object sender, EventArgs e)
        //{
        //    if (ddgReasonForDelay.Table.Rows[ddgReasonForDelay.SelectedIndex][1].ToString().ToLower().Trim() == "other")
        //    {
        //        tr_other_reason_for_delay.AddDisplay();
        //    }
        //    else
        //    {
        //        tr_other_reason_for_delay.AddDisplayNone();
        //    }
        //    PaintButtons();
        //}

        protected void ddgState_changed(object sender, EventArgs e)
        {
            //GetRetDatePeriods(ddgCurrFY.SelectedIndex, ddgCurrFY.ItemID);
            var line_item = new LineBA53(Convert.ToInt32(txtItemID.Value), Convert.ToInt32(txtLineNo.Value));
            switch (ddgState.ItemID)
            {
                case "0":// - select -
                    ddgState.SelectedIndex = 0;
                    break;
                case "1": //DC
                    tr_RET_lbl1stHalfTaxInvFY2.AddDisplay();
                    tr_RET_1stHalfTaxInvFY1.AddDisplay();
                    // txt1stHalfTaxInvFY1_DatePeriod_.ReadOnly=true;
                    //txt1stHalfFY1_TotalTaxBill_.ReadOnly=true;
                    //txt1stHalfFY1_NoMonthsReimb_.ReadOnly = true;

                    //txt1stHalfTaxInvFY1_DatePeriod_.CssClass = "disabled_textbox";
                    //txt1stHalfFY1_TotalTaxBill_.CssClass = "disabled_textbox";
                    //txt1stHalfFY1_NoMonthsReimb_.CssClass = "disabled_textbox";

                    Recalculate(line_item);
                    break;
                case "2": //MD
                    tr_RET_lbl1stHalfTaxInvFY2.AddDisplayNone(); //txt1stHalfFY2_TotalTaxBill & txt1stHalfFY2_NoMonthsReimb not visible
                    tr_RET_1stHalfTaxInvFY1.AddDisplayNone();// txt1stHalfFY1_TotalTaxBill & txt1stHalfFY1_NoMonthsReimb  not visible
                    Recalculate(line_item);
                    break;
                case "3": //VA
                    tr_RET_lbl1stHalfTaxInvFY2.AddDisplayNone(); //txt1stHalfFY2_TotalTaxBill & txt1stHalfFY2_NoMonthsReimb not visible
                    tr_RET_1stHalfTaxInvFY1.AddDisplayNone(); // txt1stHalfFY1_TotalTaxBill & txt1stHalfFY1_NoMonthsReimb  not visible
                    Recalculate(line_item);
                    break;
                default:

                    break;
            }
            PaintButtons();
        }

        protected void GetRetDatePeriods(int iIndex, string sYear)
        {

            if (iIndex == 0)
            {
                lblFY1.Text = "";
                lblFYRETEscExp.Text = "FY?? RET Escalation Expense:";
                lblFY1RETEscExp.Text = "FY?? RET Escalation Expense:";
                lbl1stHalfTaxInvFY.Text = "1st Half of Tax Invoice FY??:";
                lbl12ndHalfTaxInvFY.Text = "2nd Half of Tax Invoice FY??:";
                lbl1stHalfTaxInvFY1.Text = "1st Half of Tax Invoice FY??:";
                lbl1stHalfTaxInvFY1_.Text = "1st Half of Tax Invoice FY??:";
                lbl12ndHalfTaxInvFY1.Text = "2nd Half of Tax Invoice FY??:";
                lbl1stHalfTaxInvFY2.Text = "1st Half of Tax Invoice FY??:";

                lblFY2_RETEscPtojTitile.Text = "FY?? RET Escalation Projection:";
                lblFY2_RETEscPtojRevTitile.Text = "FY?? RET Escalation Projection (Revised):";
            }
            else
            {
                if (ddgCurrFY.ItemID != "")
                {
                    lblFY1.Text = Convert.ToString(Convert.ToInt32(ddgCurrFY.ItemID) + 1);
                    lblFYRETEscExp.Text = ddgCurrFY.ItemID + " RET Escalation Expense:";
                }
                lblFY1RETEscExp.Text = lblFY1.Text + " RET Escalation Expense:";

                lbl1stHalfTaxInvFY.Text = "1st Half of Tax Invoice " + ddgCurrFY.ItemID + ":";
                lbl12ndHalfTaxInvFY.Text = "2nd Half of Tax Invoice " + ddgCurrFY.ItemID + ":";
                lbl1stHalfTaxInvFY1.Text = "1st Half of Tax Invoice " + lblFY1.Text + ":";

                lbl1stHalfTaxInvFY1_.Text = "1st Half of Tax Invoice " + lblFY1.Text + ":";
                lbl12ndHalfTaxInvFY1.Text = "2nd Half of Tax Invoice " + lblFY1.Text + ":";

                if (lblFY1.Text != "")
                {
                    lbl1stHalfTaxInvFY2.Text = "1st Half of Tax Invoice " + Convert.ToString(Convert.ToInt16(lblFY1.Text) + 1) + ":";
                    lblFY2_RETEscPtojTitile.Text = Convert.ToString(Convert.ToInt16(lblFY1.Text) + 1) + " RET Escalation Projection:";
                    lblFY2_RETEscPtojRevTitile.Text = Convert.ToString(Convert.ToInt16(lblFY1.Text) + 1) + " RET Escalation Projection (Revised):";
                }
            }
        }


        private void SetValid(string sReasonCode)
        {
            try
            {
                switch (ddgReasonCode.ItemID)
                {
                    case "0":// - select -
                        lblValid.Text = "";
                        break;
                    case "1": //Valid
                        lblValid.Text = "Valid";
                        break;
                    default:
                        lblValid.Text = "Invalid";
                        break;
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

        private void Recalculate(LineBA53 line_item)
        {
            try
            {
                if (!BlnForUpdate)
                {
                    txtRemarks.Enabled = false;
                    btnSave.Enabled = false;
                    btnSave.Visible = false;
                    btnReviewer0.Disabled = true;
                    btnReviewer0.Visible = false;
                    ctrlContacts.Enabled = false;
                    DisableOrClearControls(true, false);
                    ctrlContacts.Enabled = false;
                    ctrlAttachments.ShowAddAttBtn(false);
                }
                else
                {
                    ctrlContacts.Enabled = true;
                    ctrlAttachments.ShowAddAttBtn(true);
                    //ctrlContacts.LoadContacts();

                    if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                        txtReassignTargetPage.Value = "Reroute";
                    else if (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                        txtReassignTargetPage.Value = "Reassign";
                    else
                        txtReassignTargetPage.Value = "ReassignRequest";
                }

                lblSortCode.Text = ddgTeamCode.Table.Rows[ddgTeamCode.SelectedIndex][1].ToString();

                lblTotalDO.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblACCRUAL.Text) + Utility.GetDecimalFromDisplayedMoney(lblACCRUAL.Text));
                txtNewProjAnnRent_est.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtNewProjAnnRent_est.Text));
                txtOldProjAnnRent_est.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtOldProjAnnRent_est.Text));
                txtNewProjAnnRent.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtNewProjAnnRent.Text));
                txtNewProjAnnualRent.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtNewProjAnnualRent.Text));
                txtOldProjAnnRent.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtOldProjAnnRent.Text));
                txtAnnualRent.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtAnnualRent.Text));
                txtProjAnnualRent.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtProjAnnualRent.Text));
                txtProjRSF.Text = Utility.DisplayDecimalFormat(txtProjRSF.Text);
                txtFY_2_CPIEscExp.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtFY_2_CPIEscExp.Text));
                txtFY_1_CPIEscExp.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtFY_1_CPIEscExp.Text));
                txtFYCPIEscProj.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtFYCPIEscProj.Text));
                txtAnnualIncrOfAction.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtAnnualIncrOfAction.Text));
                txt1stHalfFY_TotalTaxBill.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txt1stHalfFY_TotalTaxBill.Text));
                txt2ndHalfFY_TotalTaxBill.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txt2ndHalfFY_TotalTaxBill.Text));
                txt1stHalfFY_TotalTaxBill.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txt1stHalfFY_TotalTaxBill.Text));
                txt2ndHalfFY_TotalTaxBill.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txt2ndHalfFY_TotalTaxBill.Text));
                txt1stHalfFY1_TotalTaxBill.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txt1stHalfFY1_TotalTaxBill.Text));
                txtFY_TaxBaseYear.Text = Utility.DisplayDecimalFormat(txtFY_TaxBaseYear.Text);
                txtFY_PercGovOccupReimb.Text = Utility.DisplayDecimalFormat(txtFY_PercGovOccupReimb.Text);
                txt1stHalfFY1_TotalTaxBill_.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txt1stHalfFY1_TotalTaxBill_.Text));
                txt2ndHalfFY1_TotalTaxBill.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txt2ndHalfFY1_TotalTaxBill.Text));
                txt1stHalfFY2_TotalTaxBill.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txt1stHalfFY2_TotalTaxBill.Text));
                txtFY1_TaxBaseYear.Text = Utility.DisplayMoneyFormat(txtFY1_TaxBaseYear.Text);
                txtFY1_PercGovOccup.Text = Utility.DisplayDecimalFormat(txtFY1_PercGovOccup.Text);
                txtFY2_RETEscProjPercIncrease.Text = Utility.DisplayDecimalFormat(txtFY2_RETEscProjPercIncrease.Text);
                txtFY2_RETEscProjAccrual.Text = Utility.DisplayMoneyFormat(Utility.DisplayDecimalFormat(txtFY2_RETEscProjAccrual.Text));
                txt1stHalfTaxInvFY1_DatePeriod_.Text = txt1stHalfTaxInvFY1_DatePeriod.Text;
                txt1stHalfFY1_TotalTaxBill_.Text = txt1stHalfFY1_TotalTaxBill.Text;
                txt1stHalfFY1_NoMonthsReimb_.Text = txt1stHalfFY1_NoMonthsReimb.Text;


                switch (ddgAccrualType.ItemID)
                {
                    case "1": //CPI

                        // calculated value($) 
                        line_item.FY_1_EscalationExpenseDiff = Convert.ToDecimal(FormatNull(txtFY_1_CPIEscExp.Text, "Decimal")) - Convert.ToDecimal(FormatNull(txtFY_2_CPIEscExp.Text, "Decimal"));
                        lblFY_1_EscalationExpenseDiff.Text = Utility.DisplayMoneyFormat(line_item.FY_1_EscalationExpenseDiff);

                        // calculated value (%)
                        line_item.FY_2_CPI_EscalationExpense = Convert.ToDecimal(FormatNull(txtFY_2_CPIEscExp.Text, "Decimal"));
                        if (line_item.FY_2_CPI_EscalationExpense != 0)
                        {
                            lblFY_1_EscalationExpensePercIncrease.Text = (line_item.FY_1_EscalationExpenseDiff * 100 / line_item.FY_2_CPI_EscalationExpense).ToString("#,##0.00") + "%";
                        }

                        // calculated value($)
                        line_item.FY_EscalationExpenseDiff = Convert.ToDecimal(FormatNull(txtFYCPIEscProj.Text, "Decimal")) - Convert.ToDecimal(FormatNull(txtFY_1_CPIEscExp.Text, "Decimal"));
                        lblFY_EscalationExpenseDiff.Text = Utility.DisplayMoneyFormat(line_item.FY_EscalationExpenseDiff);

                        // calculated value (%)
                        line_item.FY_1_CPI_EscalationExpense = Convert.ToDecimal(FormatNull(txtFY_1_CPIEscExp.Text, "Decimal"));
                        if (line_item.FY_1_CPI_EscalationExpense != 0)
                        {
                            lblFY_EscalationExpensePercIncrease.Text = (line_item.FY_EscalationExpenseDiff * 100 / line_item.FY_1_CPI_EscalationExpense).ToString("#,##0.00") + "%";
                        }

                        // calculated value ($)
                        var dMonthlyIncrease = Convert.ToDecimal(Utility.GetDecimalFromDisplayedMoney(txtAnnualIncrOfAction.Text)) / 12;
                        lblMonthlyIncrease.Text = "$" + dMonthlyIncrease.ToString("#,##0.00");

                        //DisplayDate(calEffDateOfAction, line_item.EffectiveDateOfAction);

                        //DateTime d1 = calEffDateOfAction.Date;
                        //string sY1 = d1.Year.ToString();
                        //string sD2 = "09/30/" + sY1;
                        //calFiscalYearEndDate.Date = Convert.ToDateTime(sD2);

                        calFiscalYearEndDate.Date = Convert.ToDateTime("09/30/" + calFiscalYearEndDate.Date.Year);


                        //DisplayDate(calCurrAuditEndDate, line_item.CurrAuditEndDate);
                        // DisplayDate(calFiscalYearEndDate, FiscalYearEndDate());  // pre-filled

                        // calculated value(number)
                        if (calFiscalYearEndDate.isEmpty == true || calEffDateOfAction.isEmpty == true)
                        {
                            lblTotalMonths.Text = "0";
                            lblTotalMonths.ToolTip = "Effective Date of Action and Fiscal Year End Date should not be null";
                        }
                        else
                        {
                            //int monthsApart = 12 * (calFiscalYearEndDate.Date.Year - calEffDateOfAction.Date.Year) + calFiscalYearEndDate.Date.AddDays(1).Month - calEffDateOfAction.Date.Month;
                            //lblTotalMonths.Text = monthsApart.ToString();

                            var dtStart = calEffDateOfAction.Date;
                            var dtEnd = calFiscalYearEndDate.Date.AddDays(1);
                            var span = dtEnd.Subtract(dtStart);
                            var iDayDiff = span.Days;
                            var monthsApart = iDayDiff / 30;
                            lblTotalMonths.Text = monthsApart.ToString();
                            lblTotalMonths.ToolTip = "";
                        }

                        //  calculated value calculated value  (for STEP RENT and HOLDOVER) 
                        lblTotalMonthsCatchUp3_5.Text = "$" + Convert.ToString(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncrease.Text) * Utility.GetDecimalFromDisplayedMoney(lblTotalMonths.Text)); // calculated value 

                        // calculated value "$"
                        lblTotalMonthsCatchUp1_2.Text = "$" + Convert.ToString(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncrease.Text) * Utility.GetDecimalFromDisplayedMoney(lblTotalMonths.Text));
                        lblTotalMonthsCatchUp1_2.Text = Utility.DisplayMoneyFormat(lblTotalMonthsCatchUp1_2.Text);

                        // calculated value (num)
                        //txtNoDaysProratedMonth.Text = System.DateTime.DaysInMonth(calEffDateOfAction.Date.Year, calEffDateOfAction.Date.Month).ToString();

                        // calculated value (num)
                        if (calEffDateOfAction.isEmpty == false && calEffDateOfAction.Date.Day > 1)
                        {
                            lblDatesProrated.Text = Convert.ToString(Convert.ToInt32(txtNoDaysProratedMonth.Text) - calEffDateOfAction.Date.Day + 1);
                        }
                        else
                        {
                            lblDatesProrated.Text = "";
                        }

                        // calculated value ($)
                        if (calEffDateOfAction.isEmpty == true || calEffDateOfAction.Date.Day <= 1)
                        {
                            lblTotalDayCatchUp.Text = "";
                        }
                        else
                        {
                            //int i = Convert.ToInt32(lblDatesProrated.Text) / Convert.ToInt32(txtNoDaysProratedMonth.Text);
                            if (txtNoDaysProratedMonth.Text != "0" && txtNoDaysProratedMonth.Text.Trim() != "" && lblDatesProrated.Text.Trim() != "")
                            {
                                lblTotalDayCatchUp.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncrease.Text) * Convert.ToInt32(lblDatesProrated.Text) / Convert.ToInt32(txtNoDaysProratedMonth.Text));
                            }
                            else
                            {
                                //lblError.Text = "Please enter value for the 'Number of Days in Prorated Month' field.";
                            }
                        }

                        //pre-filled value (FYXX)
                        //string sYear = GetFiscalYear(DateTime.Now, 0).ToString();
                        //lblFY_YY.Text = "FY" + Right(sYear.Substring(sYear.Length - 2),2);

                        // calculated value ($)
                        if (lblDatesProrated.Text == "0" || lblDatesProrated.Text.Trim() == "")
                        {
                            lblTotalCatchUp.Text = lblTotalMonthsCatchUp1_2.Text;
                            // lblTotalCatchUp.Text =  "";
                        }
                        else
                        {
                            lblTotalCatchUp.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblTotalDayCatchUp.Text) + Utility.GetDecimalFromDisplayedMoney(lblTotalMonthsCatchUp1_2.Text));
                        }

                        if (calEffDateOfAction.isEmpty == true)
                        {
                            lblFY_YY.Text = "FY";
                            lblFY_2_CPIEscExp.Text = "[FY-2] CPI Escalation Expense";
                            lblFY_1_CPIEscExp.Text = "[FY-1] CPI Escalation Expense:";
                            lblFYCPIEscProj.Text = "[Current FY] CPI Escalation Projection:";
                        }
                        else
                        {
                            lblFY_YY.Text = "FY" + Right(calEffDateOfAction.Date.Year.ToString(), 2);
                            lblFY_2_CPIEscExp.Text = GetFiscalYear(calEffDateOfAction.Date, -2) + " Escalation Expense:";
                            lblFY_1_CPIEscExp.Text = GetFiscalYear(calEffDateOfAction.Date, -1) + " Escalation Expense:";
                            lblFYCPIEscProj.Text = GetFiscalYear(calEffDateOfAction.Date, 0) + " Escalation Projection:";
                        }


                        break;
                    case "2": //EXPANSION


                        // calculated value ($)
                        dMonthlyIncrease = Convert.ToDecimal(Utility.GetDecimalFromDisplayedMoney(txtAnnualIncrOfAction.Text)) / 12;
                        lblMonthlyIncrease.Text = "$" + dMonthlyIncrease.ToString("#,##0.00");

                        //DisplayDate(calEffDateOfAction, line_item.EffectiveDateOfAction);
                        //DisplayDate(calCurrAuditEndDate, line_item.CurrAuditEndDate);

                        //d1 = calEffDateOfAction.Date;
                        //sY1 = d1.Year.ToString();
                        //sD2 = "09/30/" + sY1;
                        //calFiscalYearEndDate.Date = Convert.ToDateTime(sD2);

                        calFiscalYearEndDate.Date = Convert.ToDateTime("09/30/" + calFiscalYearEndDate.Date.Year);


                        // DisplayDate(calFiscalYearEndDate, FiscalYearEndDate());  // pre-filled

                        // calculated value(number)
                        if (calFiscalYearEndDate.isEmpty == true || calEffDateOfAction.isEmpty == true)
                        {
                            lblTotalMonths.Text = "0";
                            lblTotalMonths.ToolTip = "Effective Date of Action and Fiscal Year End Date should not be null";
                        }
                        else
                        {

                            //int monthsApart = 12 * (calFiscalYearEndDate.Date.Year - calEffDateOfAction.Date.Year) + calFiscalYearEndDate.Date.AddDays(1).Month - calEffDateOfAction.Date.Month;
                            var dtStart = calEffDateOfAction.Date;
                            var dtEnd = calFiscalYearEndDate.Date.AddDays(1);
                            var span = dtEnd.Subtract(dtStart);
                            var iDayDiff = span.Days;
                            var monthsApart = iDayDiff / 30;
                            lblTotalMonths.Text = monthsApart.ToString();
                            lblTotalMonths.ToolTip = "";

                        }


                        //  calculated value calculated value  (for STEP RENT and HOLDOVER) 
                        lblTotalMonthsCatchUp3_5.Text = "$" + Convert.ToString(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncrease.Text) * Utility.GetDecimalFromDisplayedMoney(lblTotalMonths.Text)); // calculated value 

                        // calculated value "$"
                        lblTotalMonthsCatchUp1_2.Text = "$" + Convert.ToString(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncrease.Text) * Utility.GetDecimalFromDisplayedMoney(lblTotalMonths.Text));
                        lblTotalMonthsCatchUp1_2.Text = Utility.DisplayMoneyFormat(lblTotalMonthsCatchUp1_2.Text);

                        // calculated value (num)
                        //txtNoDaysProratedMonth.Text = System.DateTime.DaysInMonth(calEffDateOfAction.Date.Year, calEffDateOfAction.Date.Month).ToString();

                        // calculated value (num)
                        if (calEffDateOfAction.isEmpty == false && calEffDateOfAction.Date.Day > 1)
                        {
                            lblDatesProrated.Text = Convert.ToString(Convert.ToInt32(txtNoDaysProratedMonth.Text) - calEffDateOfAction.Date.Day + 1);
                        }
                        else
                        {
                            lblDatesProrated.Text = "";
                        }

                        // calculated value ($)
                        if (calEffDateOfAction.isEmpty == true || calEffDateOfAction.Date.Day <= 1)
                        {
                            lblTotalDayCatchUp.Text = "";
                        }
                        else
                        {
                            if (lblMonthlyIncrease.Text != "0" && lblMonthlyIncrease.Text != "0.00" && lblMonthlyIncrease.Text.Trim() != "" && lblDatesProrated.Text != "0" && lblDatesProrated.Text != "0.00" && lblDatesProrated.Text.Trim() != "")
                            {
                                lblTotalDayCatchUp.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncrease.Text) / Convert.ToInt32(txtNoDaysProratedMonth.Text) * Convert.ToInt32(lblDatesProrated.Text));
                            }
                        }

                        //pre-filled value (FYXX)
                        //sYear = GetFiscalYear(DateTime.Now, 0).ToString();
                        //lblFY_YY.Text = "FY" + sYear.Substring(sYear.Length - 2);

                        // calculated value ($)
                        if (lblDatesProrated.Text == "0" || lblDatesProrated.Text == "")
                        {
                            lblTotalCatchUp.Text = lblTotalMonthsCatchUp1_2.Text;
                        }
                        else
                        {
                            lblTotalCatchUp.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblTotalDayCatchUp.Text) + Utility.GetDecimalFromDisplayedMoney(lblTotalMonthsCatchUp1_2.Text));
                        }
                        if (calEffDateOfAction.isEmpty == true)
                        {
                            lblFY_YY.Text = "FY";
                        }
                        else
                        {
                            lblFY_YY.Text = "FY" + Right(calEffDateOfAction.Date.Year.ToString(), 2);
                        }
                        break;
                    case "3": //HOLDOVER
                        ////////  Breakout of Part Year Cost of Established Accrual section /////////////////////////////
                        lblCurrProjAnnRentEst.Text = txtAnnualRent.Text;
                        lblAccr1.Text = ddgActionType.Table.Rows[ddgActionType.SelectedIndex][1].ToString();
                        lblNewProjAnnRent_est.Text = txtProjAnnualRent.Text;
                        lblAnnIncrOfActionEst.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblNewProjAnnRent_est.Text, "Decimal")) - Convert.ToDecimal(FormatNull(lblCurrProjAnnRentEst.Text, "Decimal")));
                        lblMonthlyIncreaseEst.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblAnnIncrOfActionEst.Text) / 12);

                        //d1 = calEffDateOfAction.Date;
                        //sY1 = d1.Year.ToString();
                        //sD2 = "09/30/" + sY1;
                        //calFiscalYearEndDate.Date = Convert.ToDateTime(sD2);

                        calFiscalYearEndDate.Date = Convert.ToDateTime("09/30/" + calFiscalYearEndDate.Date.Year);

                        if (calFiscalYearEndDateEst.isEmpty == true || calEffDateOfActionEst.isEmpty == true)
                        {
                            lblTotalMonthsEst.Text = "";
                            lblTotalMonthsEst.ToolTip = "Effective Date of Action and Fiscal Year End Date should not be null";
                        }
                        else
                        {
                            //int monthsApart = 12 * (calFiscalYearEndDateEst.Date.Year - calEffDateOfActionEst.Date.Year) + calFiscalYearEndDateEst.Date.AddDays(1).Month - calEffDateOfActionEst.Date.Month;
                            //lblTotalMonthsEst.Text = monthsApart.ToString();
                            //lblTotalMonthsEst.ToolTip = "";


                            var dtStart = calEffDateOfActionEst.Date;
                            var dtEnd = calFiscalYearEndDateEst.Date.AddDays(1);
                            var span = dtEnd.Subtract(dtStart);
                            var iDayDiff = span.Days;
                            var monthsApart = iDayDiff / 30;
                            lblTotalMonthsEst.Text = monthsApart.ToString();
                            lblTotalMonthsEst.ToolTip = "";

                        }

                        // calculated value ($)
                        lblTotalMonthsCatchUpEst.Text = "$" + Convert.ToString(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncreaseEst.Text) * Utility.GetDecimalFromDisplayedMoney(lblTotalMonthsEst.Text)); // calculated value 

                        // calculated value (num)
                        txtNoDaysProratedMonthEst.Text = System.DateTime.DaysInMonth(calEffDateOfActionEst.Date.Year, calEffDateOfActionEst.Date.Month).ToString();

                        // calculated value (num)
                        if (calEffDateOfActionEst.isEmpty == false && calEffDateOfActionEst.Date.Day > 1)
                        {
                            lblDaysProratedEst.Text = Convert.ToString(Convert.ToInt32(txtNoDaysProratedMonthEst.Text) - calEffDateOfActionEst.Date.Day + 1);
                        }
                        else
                        {
                            lblDaysProratedEst.Text = "";
                        }

                        // calculated value ($)
                        if (calEffDateOfActionEst.isEmpty == true || calEffDateOfActionEst.Date.Day <= 1)
                        {
                            lblTotalDayCatchUpEst.Text = "";
                        }
                        else
                        {
                            if (lblMonthlyIncreaseEst.Text != "0" && lblMonthlyIncreaseEst.Text != "0.00" && lblMonthlyIncreaseEst.Text.Trim() != "" && lblDaysProratedEst.Text != "0" && lblDaysProratedEst.Text != "0.00" && lblDaysProratedEst.Text.Trim() != "")
                            {
                                lblTotalDayCatchUpEst.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncreaseEst.Text) / Convert.ToInt32(txtNoDaysProratedMonthEst.Text) * Convert.ToInt32(lblDaysProratedEst.Text));
                            }
                        }


                        if (calEffDateOfActionEst.isEmpty == true)
                        {
                            lblFY_YY_Est.Text = "FY";
                        }
                        else
                        {
                            lblFY_YY_Est.Text = "FY" + Right(calEffDateOfActionEst.Date.Year.ToString(), 2);
                        }

                        // calculated value ($)
                        if (txtNoDaysProratedMonthEst.Text == "0" || txtNoDaysProratedMonthEst.Text.Trim() == "")
                        {
                            lblTotalCatchUpEst.Text = lblTotalMonthsCatchUpEst.Text;
                        }
                        else
                        {
                            lblTotalCatchUpEst.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblTotalDayCatchUpEst.Text) + Utility.GetDecimalFromDisplayedMoney(lblTotalMonthsCatchUpEst.Text));
                        }
                        //--------------------  End of section (established)  ---------------------/

                        ////////  Breakout of Part Year Cost section ///////////////////////////////
                        lblCurrProjAnnRent.Text = txtAnnualRent.Text;
                        lblAccr2.Text = lblAccrTypeAction.Text;
                        lblNewProjAnnRent.Text = txtProjAnnualRent.Text;
                        txtAnnualIncrOfAction.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblNewProjAnnRent.Text, "Decimal")) - Convert.ToDecimal(FormatNull(lblCurrProjAnnRent.Text, "Decimal")));
                        txtAnnualIncrOfAction.ReadOnly = true;
                        txtAnnualIncrOfAction.ToolTip = "Calculated value";
                        lblMonthlyIncrease.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(txtAnnualIncrOfAction.Text) / 12);

                        if (calCurrAuditEndDate.isEmpty == true || calEffDateOfAction.isEmpty == true)
                        {
                            lblTotalMonths.Text = "";
                            lblTotalMonths.ToolTip = "Effective Date of Action and Fiscal Year End Date should not be null";
                        }
                        else
                        {
                            //int monthsApart = 12 * (calCurrAuditEndDate.Date.Year - calEffDateOfAction.Date.Year) + calCurrAuditEndDate.Date.AddDays(1).Month - calEffDateOfAction.Date.Month;
                            //lblTotalMonths.Text = monthsApart.ToString();
                            //lblTotalMonths.ToolTip = "";

                            var dtStart = calEffDateOfAction.Date;
                            var dtEnd = calCurrAuditEndDate.Date.AddDays(1);
                            var span = dtEnd.Subtract(dtStart);
                            var iDayDiff = span.Days;
                            var monthsApart = iDayDiff / 30;
                            lblTotalMonths.Text = monthsApart.ToString();
                            lblTotalMonths.ToolTip = "";
                        }

                        // calculated value ($)
                        lblTotalMonthsCatchUp3_5.Text = "$" + Convert.ToString(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncrease.Text) * Utility.GetDecimalFromDisplayedMoney(lblTotalMonths.Text));

                        // calculated value (num)
                        txtNoDaysProratedMonth.Text = System.DateTime.DaysInMonth(calEffDateOfAction.Date.Year, calEffDateOfAction.Date.Month).ToString();

                        //calculated value (num)
                        if (calCurrAuditEndDate.isEmpty == false && calEffDateOfAction.Date.Day > 1)
                        {
                            lblDatesProrated.Text = Convert.ToString(Convert.ToInt32(txtNoDaysProratedMonth.Text) - calEffDateOfAction.Date.Day + 1);
                        }
                        else
                        {
                            lblDatesProrated.Text = "";
                            lblTotalDayCatchUp.Text = "";
                        }

                        // calculated value ($)
                        if (calEffDateOfAction.isEmpty == true || calEffDateOfAction.Date.Day <= 1)
                        {
                            lblTotalDayCatchUp.Text = "";
                        }
                        else
                        {
                            //if (txtNoDaysProratedMonth.Text != "0" && txtNoDaysProratedMonth.Text != "0.00" && txtNoDaysProratedMonth.Text.Trim() != "" && lblDatesProrated.Text != "0" && lblDatesProrated.Text != "0.00" && lblDatesProrated.Text.Trim() != "")
                            if (txtNoDaysProratedMonth.Text != "0" && txtNoDaysProratedMonth.Text != "0.00" && txtNoDaysProratedMonth.Text.Trim() != "" && lblDatesProrated.Text != "0" && lblDatesProrated.Text != "0.00" && lblDatesProrated.Text.Trim() != "")
                            {
                                lblTotalDayCatchUp.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncrease.Text) / Convert.ToInt32(txtNoDaysProratedMonth.Text) * Convert.ToInt32(lblDatesProrated.Text));
                            }
                            else
                            {
                                //lblError.Text = "Please enter value for the 'Number of Days in Prorated Month' field.";
                            }
                        }

                        if (calEffDateOfAction.isEmpty == true)
                        {
                            lblFY_YY.Text = "FY";
                        }
                        else
                        {
                            lblFY_YY.Text = "FY" + Right(calEffDateOfAction.Date.Year.ToString(), 2);
                        }

                        //lblTotalCatchUp.Text = "";

                        // calculated value ($)
                        if (txtNoDaysProratedMonth.Text == "0" || txtNoDaysProratedMonth.Text.Trim() == "")
                        {
                            lblTotalCatchUp.Text = lblTotalMonthsCatchUp3_5.Text;
                        }
                        else
                        {
                            lblTotalCatchUp.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblTotalDayCatchUp.Text) + Utility.GetDecimalFromDisplayedMoney(lblTotalMonthsCatchUp3_5.Text));
                        }

                        //calFiscalYearEndDateEst

                        //d1 = calEffDateOfActionEst.Date;
                        //sY1 = d1.Year.ToString();
                        //sD2 = "09/30/" + sY1;
                        //calFiscalYearEndDateEst.Date = Convert.ToDateTime(sD2);

                        //d1 = calEffDateOfAction5.Date;
                        //sY1 = d1.Year.ToString();
                        //sD2 = "09/30/" + sY1;
                        //calCurrAuditEndDate.Date = Convert.ToDateTime(sD2);

                        calFiscalYearEndDateEst.Date = Convert.ToDateTime("09/30/" + calFiscalYearEndDateEst.Date.Year);
                        //calCurrAuditEndDate.Date = Convert.ToDateTime("09/30/" + calCurrAuditEndDate.Date.Year);


                        //--------------------  End of section  ---------------------/

                        break;
                    case "4":  //RET

                        //-----  FY RET Escalation Expense section  ---------
                        lbl1stHalfFY_ReimbAmt.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY_TotalTaxBill.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt1stHalfFY_NoMonthsReimb.Text, "Int")));
                        lbl2ndHalfFY_ReimbAmt.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt2ndHalfFY_TotalTaxBill.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt2ndHalfFY_NoMonthsReimb.Text, "Int")));
                        lbl1stHalfFY1_ReimbAmt.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt1stHalfFY1_NoMonthsReimb.Text, "Int")));

                        ///***
                        if (ddgState.ItemID == "0")
                        {
                            throw new Exception("Please select State");
                        }

                        if (ddgState.ItemID == "1")
                        {
                            lblFY_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill.Text, "Decimal")));
                            lblFY_TaxBillReceipt_NoMonthReimb.Text = Convert.ToString(Convert.ToDecimal(FormatNull(txt1stHalfFY_NoMonthsReimb.Text, "Int")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY_NoMonthsReimb.Text, "Int")) + Convert.ToDecimal(FormatNull(txt1stHalfFY1_NoMonthsReimb.Text, "Int")));
                            lblFY_TaxBillReceipt_Reimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lbl1stHalfFY_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl2ndHalfFY_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl1stHalfFY1_ReimbAmt.Text, "Decimal")));

                        }
                        else
                        {
                            lblFY_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY_TotalTaxBill.Text, "Decimal")));
                            lblFY_TaxBillReceipt_NoMonthReimb.Text = Convert.ToString(Convert.ToDecimal(FormatNull(txt1stHalfFY_NoMonthsReimb.Text, "Int")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY_NoMonthsReimb.Text, "Int")));
                            lblFY_TaxBillReceipt_Reimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lbl1stHalfFY_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl2ndHalfFY_ReimbAmt.Text, "Decimal")));

                        }

                        if (txtFY_TaxBaseYear.Text.Trim() == "")
                        {
                            txtFY_TaxBaseYear.Text = "0";
                        }
                        else
                        {
                            txtFY_TaxBaseYear.Text = Utility.DisplayMoneyFormat(txtFY_TaxBaseYear.Text);
                            if (Convert.ToDecimal(Utility.GetDecimalFromDisplayedMoney(txtFY_TaxBaseYear.Text)) < 0)
                            {
                                txtFY_TaxBaseYear.CssClass = "regRedText";
                            }
                        }

                        lblFY_NetAmountReimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY_TaxBillReceipt_Reimb.Text, "Decimal")) - Convert.ToDecimal(FormatNull(txtFY_TaxBaseYear.Text, "Decimal")));

                        if (txtFY_PercGovOccupReimb.Text.Trim() == "")
                        {
                            txtFY_PercGovOccupReimb.Text = "0";
                        }

                        lblFY_AmtDueLessorReimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY_NetAmountReimb.Text, "Decimal")) * Convert.ToDecimal(FormatNull(txtFY_PercGovOccupReimb.Text, "Decimal")) / 100);
                        //-----  End of FY section  ---------

                        //-----  FY+1 RET Escalation Expense section   ---------
                        lbl1stHalfFY1_ReimbAmt_.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill_.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt1stHalfFY1_NoMonthsReimb_.Text, "Int")));
                        lbl2ndHalfFY1_ReimbAmt.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt2ndHalfFY1_TotalTaxBill.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt2ndHalfFY1_NoMonthsReimb.Text, "Int")));
                        lbl1stHalfFY2_ReimbAmt.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY2_TotalTaxBill.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt1stHalfFY2_NoMonthsReimb.Text, "Int")));

                        ///***

                        if (ddgState.ItemID == "1")
                        {
                            lblFY1_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill_.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY1_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt1stHalfFY2_TotalTaxBill.Text, "Decimal")));
                            lblFY1_TaxBillReceipt_NoMonthReimb.Text = Convert.ToString(Convert.ToDecimal(FormatNull(txt1stHalfFY1_NoMonthsReimb_.Text, "Int")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY1_NoMonthsReimb.Text, "Int")) + Convert.ToDecimal(FormatNull(txt1stHalfFY2_NoMonthsReimb.Text, "Int")));
                            lblFY1_TaxBillReceipt_Reimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lbl1stHalfFY1_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl2ndHalfFY1_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl1stHalfFY2_ReimbAmt.Text, "Decimal")));

                        }
                        else
                        {
                            lblFY1_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill_.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY1_TotalTaxBill.Text, "Decimal")));
                            lblFY1_TaxBillReceipt_NoMonthReimb.Text = Convert.ToString(Convert.ToDecimal(FormatNull(txt1stHalfFY1_NoMonthsReimb_.Text, "Int")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY1_NoMonthsReimb.Text, "Int")));
                            lblFY1_TaxBillReceipt_Reimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lbl1stHalfFY1_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl2ndHalfFY1_ReimbAmt.Text, "Decimal")));

                        }

                        // txt1stHalfFY1_NoMonthsReimb_ + txt2ndHalfFY1_NoMonthsReimb + txt1stHalfFY2_NoMonthsReimb

                        if (txtFY1_TaxBaseYear.Text.Trim() == "")
                        {
                            txtFY1_TaxBaseYear.Text = "0";
                        }
                        else
                        {
                            txtFY1_TaxBaseYear.Text = Utility.DisplayMoneyFormat(txtFY1_TaxBaseYear.Text);
                            if (Convert.ToDecimal(Utility.GetDecimalFromDisplayedMoney(txtFY1_TaxBaseYear.Text)) < 0)
                            {
                                txtFY1_TaxBaseYear.CssClass = "regRedText";
                            }
                        }

                        lblFY1_NetAmountReimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY1_TaxBillReceipt_Reimb.Text, "Decimal")) - Convert.ToDecimal(FormatNull(txtFY1_TaxBaseYear.Text, "Decimal")));

                        if (txtFY1_PercGovOccup.Text.Trim() == "")
                        {
                            txtFY1_PercGovOccup.Text = "0";
                        }

                        lblFY1_AmtDueLessor.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY1_NetAmountReimb.Text, "Decimal")) * Convert.ToDecimal(FormatNull(txtFY1_PercGovOccup.Text, "Decimal")) / 100);
                        // ------ Escalation Projection Values  -----//
                        if (txtFY2_RETEscProjAccrual.Text.Trim() == "")
                        {
                            txtFY2_RETEscProjAccrual.Text = "0";
                        }

                        if (txtFY2_RETEscProjPercIncrease.Text.Trim() == "")
                        {
                            txtFY2_RETEscProjPercIncrease.Text = "0";
                        }


                        //lblFY2_RETEscProjNetIncrease.Text = Utility.DisplayMoneyFormat((Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal")) - Convert.ToDecimal(FormatNull(lblFY_AmtDueLessorReimb.Text, "Decimal"))) + Convert.ToDecimal(FormatNull(txtFY2_RETEscProjAccrual.Text, "Decimal")));
                        lblFY2_RETEscProjNetIncrease.Text = Utility.DisplayMoneyFormat((Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal")) - Convert.ToDecimal(FormatNull(lblFY_AmtDueLessorReimb.Text, "Decimal"))));



                        if (lblFY1_AmtDueLessor.Text != "0" && lblFY1_AmtDueLessor.Text != "0.00" && lblFY1_AmtDueLessor.Text != "$0.00" && lblFY1_AmtDueLessor.Text != "$00.00" && lblFY1_AmtDueLessor.Text.Trim() != "")
                        {
                            txtFY2_RETEscProjPercIncrease.Text = Utility.DisplayDecimalFormat(Convert.ToDecimal(FormatNull(lblFY2_RETEscProjNetIncrease.Text, "Decimal")) / Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal")) * 100);
                        }

                        txtFY2_RETEscProjAccrual.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY2_RETEscProjNetIncrease.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal")));


                        //-----  End of FY+1 section  ---------

                        break;
                    case "5": //STEP RENT



                        ////////  Breakout of Part Year Cost of Established Accrual section /////////////
                        //lblAccr1.Text = lblAccrualType.Text;
                        lblAnnIncrOfActionEst.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txtNewProjAnnRent_est.Text, "Decimal")) - Convert.ToDecimal(FormatNull(txtOldProjAnnRent_est.Text, "Decimal")));


                        lblMonthlyIncreaseEst.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblAnnIncrOfActionEst.Text) / 12);// 

                        if (calFiscalYearEndDateEst.isEmpty == true || calEffDateOfActionEst.isEmpty == true)
                        {
                            lblTotalMonthsEst.Text = "";
                            lblTotalMonthsEst.ToolTip = "Effective Date of Action and Fiscal Year End Date should not be null";
                        }
                        else
                        {
                            //int monthsApart = 12 * (calFiscalYearEndDateEst.Date.Year - calEffDateOfActionEst.Date.Year) + calFiscalYearEndDateEst.Date.AddDays(1).Month - calEffDateOfActionEst.Date.Month;
                            //lblTotalMonthsEst.Text = monthsApart.ToString();
                            //lblTotalMonthsEst.ToolTip = "";


                            var dtStart = calEffDateOfActionEst.Date;
                            var dtEnd = calFiscalYearEndDateEst.Date.AddDays(1);
                            var span = dtEnd.Subtract(dtStart);
                            var iDayDiff = span.Days;
                            var monthsApart = iDayDiff / 30;
                            lblTotalMonthsEst.Text = monthsApart.ToString();
                            lblTotalMonthsEst.ToolTip = "";
                        }

                        //d1 = calEffDateOfAction.Date;
                        //sY1 = d1.Year.ToString();
                        //sD2 = "09/30/" + sY1;
                        //calFiscalYearEndDate.Date = Convert.ToDateTime(sD2);

                        calFiscalYearEndDate.Date = Convert.ToDateTime("09/30/" + calFiscalYearEndDate.Date.Year);


                        // calculated value ($)
                        lblTotalMonthsCatchUpEst.Text = "$" + Convert.ToString(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncreaseEst.Text) * Utility.GetDecimalFromDisplayedMoney(lblTotalMonthsEst.Text)); // calculated value 

                        // calculated value (num)
                        //txtNoDaysProratedMonthEst.Text = System.DateTime.DaysInMonth(calEffDateOfActionEst.Date.Year, calEffDateOfActionEst.Date.Month).ToString();

                        // calculated value (num)
                        if (calEffDateOfActionEst.isEmpty == false && calEffDateOfActionEst.Date.Day > 1)
                        {
                            lblDaysProratedEst.Text = Convert.ToString(Convert.ToInt32(txtNoDaysProratedMonthEst.Text) - calEffDateOfActionEst.Date.Day + 1);
                        }
                        else
                        {
                            lblDaysProratedEst.Text = "";
                        }

                        // calculated value ($)

                        lblTotalDayCatchUpEst.Text = "";

                        // calculated value ($)
                        if (calEffDateOfActionEst.isEmpty == true || calEffDateOfActionEst.Date.Day <= 1)
                        {
                            lblTotalDayCatchUpEst.Text = "";
                        }
                        else
                        {
                            if (txtNoDaysProratedMonthEst.Text != "0" && txtNoDaysProratedMonthEst.Text != "0.00" && txtNoDaysProratedMonthEst.Text.Trim() != "" && lblDaysProratedEst.Text != "0" && lblDaysProratedEst.Text != "0.00" && lblDaysProratedEst.Text.Trim() != "")
                            {
                                lblTotalDayCatchUpEst.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncreaseEst.Text) / Convert.ToInt32(txtNoDaysProratedMonthEst.Text) * Convert.ToInt32(lblDaysProratedEst.Text));
                            }
                            else
                            {
                                //lblError.Text = "Please enter value for the 'Number of Days in Prorated Month' field.";
                            }
                        }


                        if (txtNoDaysProratedMonthEst.Text == "0" || txtNoDaysProratedMonthEst.Text.Trim() == "")
                        {
                            lblTotalCatchUpEst.Text = lblTotalMonthsCatchUpEst.Text;
                        }
                        else
                        {
                            lblTotalCatchUpEst.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblTotalDayCatchUpEst.Text) + Utility.GetDecimalFromDisplayedMoney(lblTotalMonthsCatchUpEst.Text));
                        }


                        if (calEffDateOfActionEst.isEmpty == true)
                        {
                            lblFY_YY_Est.Text = "FY";
                        }
                        else
                        {
                            lblFY_YY_Est.Text = "FY" + Right(calEffDateOfActionEst.Date.Year.ToString(), 2);
                        }


                        //--------------------  End of section (established)  ---------------------/

                        ////////  Breakout of Part Year Cost section ///////////////////////////////
                        //lblAccr2.Text = lblAccrualType.Text;
                        txtAnnualIncrOfAction.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txtNewProjAnnRent.Text, "Decimal")) - Convert.ToDecimal(FormatNull(txtOldProjAnnRent.Text, "Decimal")));
                        txtAnnualIncrOfAction.ReadOnly = true;
                        txtAnnualIncrOfAction.ToolTip = "Calculated value";
                        //lblAnnualIncrOfAction.Text = txtAnnualIncrOfAction.Text;
                        lblMonthlyIncrease.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(txtAnnualIncrOfAction.Text) / 12);

                        if (calFiscalYearEndDate.isEmpty == true || calEffDateOfAction.isEmpty == true)
                        {
                            lblTotalMonths.Text = "0";
                            lblTotalMonths.ToolTip = "Effective Date of Action and Fiscal Year End Date should not be null";
                        }
                        else
                        {
                            //int monthsApart = 12 * (calFiscalYearEndDate.Date.Year - calEffDateOfAction.Date.Year) + calFiscalYearEndDate.Date.AddDays(1).Month - calEffDateOfAction.Date.Month;
                            //lblTotalMonths.Text = monthsApart.ToString();
                            //lblTotalMonths.ToolTip = "";

                            var dtStart = calEffDateOfAction.Date;
                            var dtEnd = calFiscalYearEndDate.Date.AddDays(1);
                            var span = dtEnd.Subtract(dtStart);
                            var iDayDiff = span.Days;
                            var monthsApart = iDayDiff / 30;
                            lblTotalMonths.Text = monthsApart.ToString();
                            lblTotalMonths.ToolTip = "";
                        }

                        // calculated value ($)
                        lblTotalMonthsCatchUp3_5.Text = "$" + Convert.ToString(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncrease.Text) * Utility.GetDecimalFromDisplayedMoney(lblTotalMonths.Text));

                        // calculated value (num)
                        //txtNoDaysProratedMonth.Text = System.DateTime.DaysInMonth(calEffDateOfAction.Date.Year, calEffDateOfAction.Date.Month).ToString();

                        // calculated value (num)
                        if (calEffDateOfAction.isEmpty == false && calEffDateOfAction.Date.Day > 1)
                        {
                            lblDatesProrated.Text = Convert.ToString(Convert.ToInt32(txtNoDaysProratedMonth.Text) - calEffDateOfAction.Date.Day + 1);
                        }
                        else
                        {
                            lblDatesProrated.Text = "";
                        }

                        // calculated value ($)
                        if (calEffDateOfAction.isEmpty == true || calEffDateOfAction.Date.Day <= 1)
                        {
                            lblTotalDayCatchUp.Text = "";
                        }
                        else
                        {
                            if (txtNoDaysProratedMonth.Text != "0" && txtNoDaysProratedMonth.Text != "0.00" && txtNoDaysProratedMonth.Text.Trim() != "" && lblDatesProrated.Text != "0" && lblDatesProrated.Text != "0.00" && lblDatesProrated.Text.Trim() != "")
                            {
                                lblTotalDayCatchUp.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblMonthlyIncrease.Text) / Convert.ToInt32(txtNoDaysProratedMonth.Text) * Convert.ToInt32(lblDatesProrated.Text));
                            }
                            else
                            {
                                //lblError.Text = "Please enter value for the 'Number of Days in Prorated Month' field.";
                            }
                        }

                        if (calEffDateOfAction.isEmpty == true)
                        {
                            lblFY_YY.Text = "FY";
                        }
                        else
                        {
                            lblFY_YY.Text = "FY" + Right(calEffDateOfAction.Date.Year.ToString(), 2);
                        }


                        // calculated value ($)
                        if (txtNoDaysProratedMonth.Text == "0" || txtNoDaysProratedMonth.Text.Trim() == "")
                        {
                            lblTotalCatchUp.Text = lblTotalMonthsCatchUp3_5.Text;
                        }
                        else
                        {
                            lblTotalCatchUp.Text = Utility.DisplayMoneyFormat(Utility.GetDecimalFromDisplayedMoney(lblTotalDayCatchUp.Text) + Utility.GetDecimalFromDisplayedMoney(lblTotalMonthsCatchUp3_5.Text));
                        }
                        //--------------------  End of section  ---------------------/

                        //calCurrAuditEndDate.Date = Convert.ToDateTime("09/30/" + calCurrAuditEndDate.Date.Year);
                        calFiscalYearEndDateEst.Date = Convert.ToDateTime("09/30/" + calFiscalYearEndDateEst.Date.Year);


                        break;
                    case "6":  //BID - the same as RET .

                        //-----  FY RET Escalation Expense section  ---------
                        lbl1stHalfFY_ReimbAmt.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY_TotalTaxBill.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt1stHalfFY_NoMonthsReimb.Text, "Int")));
                        lbl2ndHalfFY_ReimbAmt.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt2ndHalfFY_TotalTaxBill.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt2ndHalfFY_NoMonthsReimb.Text, "Int")));
                        lbl1stHalfFY1_ReimbAmt.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt1stHalfFY1_NoMonthsReimb.Text, "Int")));

                        //lblFY_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill.Text, "Decimal")));

                        if (ddgState.ItemID == "0")
                        {
                            throw new Exception("Please select State");
                        }

                        if (ddgState.ItemID == "1") // DC
                        {
                            lblFY_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill.Text, "Decimal")));

                            lblFY_TaxBillReceipt_NoMonthReimb.Text = Convert.ToString(Convert.ToDecimal(FormatNull(txt1stHalfFY_NoMonthsReimb.Text, "Int")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY_NoMonthsReimb.Text, "Int")) + Convert.ToDecimal(FormatNull(txt1stHalfFY1_NoMonthsReimb.Text, "Int")));

                            lblFY_TaxBillReceipt_Reimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lbl1stHalfFY_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl2ndHalfFY_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl1stHalfFY1_ReimbAmt.Text, "Decimal")));

                        }
                        else // 2 - MD, 3 - VA
                        {
                            lblFY_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY_TotalTaxBill.Text, "Decimal")));
                            lblFY_TaxBillReceipt_NoMonthReimb.Text = Convert.ToString(Convert.ToDecimal(FormatNull(txt1stHalfFY_NoMonthsReimb.Text, "Int")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY_NoMonthsReimb.Text, "Int")));
                            lblFY_TaxBillReceipt_Reimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lbl1stHalfFY_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl2ndHalfFY_ReimbAmt.Text, "Decimal")));

                        }


                        if (txtFY_TaxBaseYear.Text.Trim() == "")
                        {
                            txtFY_TaxBaseYear.Text = "0";
                        }
                        else
                        {
                            txtFY_TaxBaseYear.Text = Utility.DisplayMoneyFormat(txtFY_TaxBaseYear.Text);
                            if (Convert.ToDecimal(Utility.GetDecimalFromDisplayedMoney(txtFY_TaxBaseYear.Text)) < 0)
                            {
                                txtFY_TaxBaseYear.CssClass = "regRedText";
                            }
                        }

                        lblFY_NetAmountReimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY_TaxBillReceipt_Reimb.Text, "Decimal")) - Convert.ToDecimal(FormatNull(txtFY_TaxBaseYear.Text, "Decimal")));

                        if (txtFY_PercGovOccupReimb.Text.Trim() == "")
                        {
                            txtFY_PercGovOccupReimb.Text = "0";
                        }

                        lblFY_AmtDueLessorReimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY_NetAmountReimb.Text, "Decimal")) * Convert.ToDecimal(FormatNull(txtFY_PercGovOccupReimb.Text, "Decimal")) / 100);

                        //-----  End of FY section  ---------

                        //-----  FY+1 RET Escalation Expense section   ---------
                        lbl1stHalfFY1_ReimbAmt_.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill_.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt1stHalfFY1_NoMonthsReimb_.Text, "Int")));
                        lbl2ndHalfFY1_ReimbAmt.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt2ndHalfFY1_TotalTaxBill.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt2ndHalfFY1_NoMonthsReimb.Text, "Int")));
                        lbl1stHalfFY2_ReimbAmt.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY2_TotalTaxBill.Text, "Decimal")) / 6 * Convert.ToInt16(FormatNull(txt1stHalfFY2_NoMonthsReimb.Text, "Int")));

                        lblFY1_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY1_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt1stHalfFY2_TotalTaxBill.Text, "Decimal")));

                        /// ***
                        if (ddgState.ItemID == "1")
                        {
                            lblFY1_TaxBillReceipt_NoMonthReimb.Text = Convert.ToString(Convert.ToDecimal(FormatNull(txt1stHalfFY1_NoMonthsReimb_.Text, "Int")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY1_NoMonthsReimb.Text, "Int")) + Convert.ToDecimal(FormatNull(txt1stHalfFY2_NoMonthsReimb.Text, "Int")));
                            lblFY1_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY1_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt1stHalfFY2_TotalTaxBill.Text, "Decimal")));
                            lblFY1_TaxBillReceipt_Reimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lbl1stHalfFY1_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl2ndHalfFY1_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl1stHalfFY2_ReimbAmt.Text, "Decimal")));

                        }
                        else
                        {
                            lblFY1_TaxBillReceipt_NoMonthReimb.Text = Convert.ToString(Convert.ToDecimal(FormatNull(txt1stHalfFY1_NoMonthsReimb_.Text, "Int")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY1_NoMonthsReimb.Text, "Int")));
                            lblFY1_TaxBillReceipt_Total.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill.Text, "Decimal")) + Convert.ToDecimal(FormatNull(txt2ndHalfFY1_TotalTaxBill.Text, "Decimal")));
                            lblFY1_TaxBillReceipt_Reimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lbl1stHalfFY1_ReimbAmt.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lbl2ndHalfFY1_ReimbAmt.Text, "Decimal")));

                        }


                        if (txtFY1_TaxBaseYear.Text.Trim() == "")
                        {
                            txtFY1_TaxBaseYear.Text = "0";
                        }
                        else
                        {
                            txtFY1_TaxBaseYear.Text = Utility.DisplayMoneyFormat(txtFY1_TaxBaseYear.Text);
                            if (Convert.ToDecimal(Utility.GetDecimalFromDisplayedMoney(txtFY1_TaxBaseYear.Text)) < 0)
                            {
                                txtFY1_TaxBaseYear.CssClass = "regRedText";
                            }
                        }

                        lblFY1_NetAmountReimb.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY1_TaxBillReceipt_Reimb.Text, "Decimal")) - Convert.ToDecimal(FormatNull(txtFY1_TaxBaseYear.Text, "Decimal")));

                        if (txtFY1_PercGovOccup.Text.Trim() == "")
                        {
                            txtFY1_PercGovOccup.Text = "0";
                        }

                        lblFY1_AmtDueLessor.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY1_NetAmountReimb.Text, "Decimal")) * Convert.ToDecimal(FormatNull(txtFY1_PercGovOccup.Text, "Decimal")) / 100);
                        // ------ Escalation Projection Values  -----//
                        if (txtFY2_RETEscProjAccrual.Text.Trim() == "")
                        {
                            txtFY2_RETEscProjAccrual.Text = "0";
                        }

                        if (txtFY2_RETEscProjPercIncrease.Text.Trim() == "")
                        {
                            txtFY2_RETEscProjPercIncrease.Text = "0";
                        }

                        // lblFY2_RETEscProjNetIncrease.Text = Utility.DisplayMoneyFormat((Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal")) - Convert.ToDecimal(FormatNull(lblFY_AmtDueLessorReimb.Text, "Decimal"))) + Convert.ToDecimal(FormatNull(txtFY2_RETEscProjAccrual.Text, "Decimal")));
                        lblFY2_RETEscProjNetIncrease.Text = Utility.DisplayMoneyFormat((Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal")) - Convert.ToDecimal(FormatNull(lblFY_AmtDueLessorReimb.Text, "Decimal"))));



                        if (lblFY1_AmtDueLessor.Text != "0" && lblFY1_AmtDueLessor.Text != "0.00" && lblFY1_AmtDueLessor.Text != "$0.00" && lblFY1_AmtDueLessor.Text != "$00.00" && lblFY1_AmtDueLessor.Text.Trim() != "")
                        {
                            txtFY2_RETEscProjPercIncrease.Text = Utility.DisplayDecimalFormat(Convert.ToDecimal(FormatNull(lblFY2_RETEscProjNetIncrease.Text, "Decimal")) / Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal")) * 100);
                        }

                        txtFY2_RETEscProjAccrual.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY2_RETEscProjNetIncrease.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal")));


                        //lblFY2_RETEscProjNetIncrease.Text = Utility.DisplayMoneyFormat((Convert.ToDecimal(FormatNull(txtFY2_RETEscProjAccrual.Text, "Decimal")) * Convert.ToDecimal(FormatNull(txtFY2_RETEscProjPercIncrease.Text, "Decimal"))) + Convert.ToDecimal(FormatNull(txtFY2_RETEscProjAccrual.Text, "Decimal")));
                        //lblFY2_RETEscProjNetIncreaseRev.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal")) - Convert.ToDecimal(FormatNull(lblFY_AmtDueLessorReimb.Text, "Decimal")));
                        //if (lblFY1_AmtDueLessor.Text != "0" && lblFY1_AmtDueLessor.Text != "0.00" && lblFY1_AmtDueLessor.Text.Trim() != "")
                        //{
                        //    txtFY2_RETEscProjPercIncreaseRev.Text = Utility.DisplayDecimalFormat(Convert.ToDecimal(FormatNull(lblFY2_RETEscProjNetIncreaseRev.Text, "Decimal")) / Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal")));
                        //}


                        //txtFY2_RETEscProjAccrualRev.Text = Utility.DisplayMoneyFormat(Convert.ToDecimal(FormatNull(lblFY2_RETEscProjNetIncreaseRev.Text, "Decimal")) + Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal")));

                        //-----  End of FY+1 section  ---------

                        break;
                    case "7":  //CLAIM

                        break;

                    default:
                        break;

                }

                PaintButtons();


                ClientScript.RegisterStartupScript(typeof(Page), "JumpScroll", "jumpScroll()", true);
                //ClientScript.RegisterClientScriptBlock(Page.GetType(), "JS",
                //"<script language='javascript' src='../include/Misc.js'></script>");
            }
            catch (Exception ex)
            {
                //lblError.Text = ex.Message; ;
            }

        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            //here we are saving some item properties in tblOIMain and tblOIOrganization (incl. status)
            SaveItemProperties(DateTime.Now, "", txtTotalDOShouldBe.Text, txtRemarks.Text, txtReviewerUserID.Value);

            var iValid = 0;

            var line_item = new LineBA53(Convert.ToInt32(txtItemID.Value), Convert.ToInt32(txtLineNo.Value));

            if (lblValid.Text.ToLower().Trim() == "valid")
            {
                iValid = 2;
            }
            else
            {
                iValid = 1;
            }

            // here we are saving line (1) properties to tblOIDetails (valid, userID, comments), tblDocRWA, tblHistory
            LineNumBO.UpdateDetails(OItemID, LoadID, 1, OrgCode, iValid, txtRemarks.Text, 1, "", "", "", "", "", CurrentUserID);

            // here we are saving item properties in tblOILease
            Recalculate(line_item);


            var sError = "";

            try
            {
                sError = ValidateDropDownGrid(ddgAccrualType, "Accrual Type", sError);
                sError = ValidateDropDownGrid(ddgReasonCode, "Reason Code", sError);
                sError = ValidateDropDownGrid(ddgTeamCode, "Team Code", sError);
                sError = ValidateTextBox(txtRSF, "RSF", sError);
                //sError = ValidateTextBox(txtTeamCode, "Team Code", sError);



                if (ddgReasonCode.ItemID == "1") // valid
                {
                    sError = ValidateTextBox(txtRemarks, "'Remarks'", sError);
                }
                sError = ValidateCalendarControl(calReportDate, "Est. Compl. Lease Date", sError);

                var objBA53 = new LineBA53(Convert.ToInt32(txtItemID.Value), Convert.ToInt32(lblLineNumber.Text));

                objBA53.TotalDO = Convert.ToDecimal(FormatNull(lblTotalDO.Text, "Decimal"));

                if (rbPmtHistory.SelectedIndex == 0)
                    objBA53.PriorYearHistoryAttached = true;
                else
                    objBA53.PriorYearHistoryAttached = false;

                if (rbSupportCalc.SelectedIndex == 0)
                    objBA53.FileContainsSupport = true;
                else
                    objBA53.FileContainsSupport = false;

                objBA53.AccrualType = Convert.ToInt32(FormatNull(ddgAccrualType.ItemID, "Int"));
                objBA53.AccrualTypeAction = Convert.ToInt32(FormatNull(ddgActionType.ItemID, "Int"));
                //objBA53.AccrualTypeAction = Convert.ToInt32(FormatNull(lblActionTypeID.Text, "Int"));

                objBA53.ReviewerReasonCode = Convert.ToInt32(FormatNull(ddgReasonCode.ItemID, "Int"));
                //if(ddgReasonCode.ItemID=="1")
                objBA53.Remarks = txtRemarks.Text;

                objBA53.TotalDOShouldBe = Convert.ToDecimal(FormatNull(txtTotalDOShouldBe.Text, "Decimal"));
                objBA53.TeamCode = ddgTeamCode.Table.Rows[ddgTeamCode.SelectedIndex][1].ToString();
                objBA53.SortCode = objBA53.TeamCode;
                objBA53.Signature1 = txtREABudgetAnalyst.Text;
                objBA53.Signature2 = txtCFOBudgetAnalyst.Text;
                objBA53.SignatureDate1 = calREABudgetAnalystDate.Date;
                objBA53.SignatureDate2 = calCFOBudgetAnalystDate.Date;
                objBA53.ReportDate = calReportDate.Date;

                switch (ddgAccrualType.ItemID)
                {
                    case "1": //CPI

                        sError = ValidateTextBox(txtAnnualRent, "Annual Rent", sError);
                        sError = ValidateTextBox(txtFY_2_CPIEscExp, lblFY_2_CPIEscExp.Text, sError);
                        sError = ValidateTextBox(txtFY_1_CPIEscExp, lblFY_1_CPIEscExp.Text, sError);
                        sError = ValidateTextBox(txtFYCPIEscProj, lblFYCPIEscProj.Text, sError);
                        sError = ValidateTextBox(txtAnnualIncrOfAction, "Annual Increase of Action", sError);
                        sError = ValidateTextBox(txtNoDaysProratedMonth, "Number of Days in Prorated Month", sError);
                        sError = ValidateCalendarControl(calEffDateOfAction, "Effective Date of Action", sError);
                        sError = ValidateCalendarControl(calFiscalYearEndDate, "Fiscal Year End Date", sError);
                        //sError = ValidateTextBox(txtTeamCode, "Team Code", sError);
                        //sError = ValidateCalendarControl(calREABudgetAnalystDate, "REA Budge tAnalyst Signature Date Date", sError);
                        //sError = ValidateCalendarControl(calCFOBudgetAnalystDate, "CFO Budget Analyst Signature Date", sError);

                        if (sError != "")
                        {
                            throw new Exception(sError);
                        }

                        objBA53.LeaseNumber = lblLeaseNum.Text;
                        //objBA53.LeaseNumber = txtText.Text;
                        objBA53.LeaseEffectiveDate = calLeaseEffDate.Date;
                        objBA53.AnnualRent = Convert.ToDecimal(FormatNull(txtAnnualRent.Text, "Decimal"));
                        objBA53.RSF = Convert.ToDecimal(txtRSF.Text);
                        objBA53.FY_2_CPI_EscalationExpense = Convert.ToDecimal(FormatNull(txtFY_2_CPIEscExp.Text, "Decimal"));
                        objBA53.FY_1_CPI_EscalationExpense = Convert.ToDecimal(FormatNull(txtFY_1_CPIEscExp.Text, "Decimal"));
                        objBA53.FY_1_EscalationExpenseDiff = Convert.ToDecimal(FormatNull(lblFY_1_EscalationExpenseDiff.Text, "Decimal"));
                        lblFY_1_EscalationExpensePercIncrease.Text = lblFY_1_EscalationExpensePercIncrease.Text.Replace("%", "");
                        objBA53.FY_1_EscalationExpensePercIncrease = Convert.ToDecimal(FormatNull(lblFY_1_EscalationExpensePercIncrease.Text, "Decimal"));

                        objBA53.FY_CPI_EscalationProjection = Convert.ToDecimal(FormatNull(txtFYCPIEscProj.Text, "Decimal"));
                        objBA53.FY_EscalationExpenseDiff = Convert.ToDecimal(FormatNull(lblFY_EscalationExpenseDiff.Text, "Decimal"));

                        lblFY_EscalationExpensePercIncrease.Text = lblFY_EscalationExpensePercIncrease.Text.Replace("%", "");
                        objBA53.FY_EscalationExpensePercIncrease = Convert.ToDecimal(FormatNull(lblFY_EscalationExpensePercIncrease.Text, "Decimal"));

                        objBA53.AnnualIncreaseOfAction = Convert.ToDecimal(FormatNull(txtAnnualIncrOfAction.Text, "Decimal"));
                        objBA53.MonthlyIncrease = Convert.ToDecimal(FormatNull(lblMonthlyIncrease.Text, "Decimal"));

                        objBA53.EffectiveDateOfAction = calEffDateOfAction.Date;
                        objBA53.TotalMonths = Convert.ToInt32(FormatNull(lblTotalMonths.Text, "Int"));

                        objBA53.FYEndDate = calFiscalYearEndDate.Date;
                        objBA53.TotalMonthsCatchUp = Convert.ToDecimal(FormatNull(lblTotalMonthsCatchUp1_2.Text, "Decimal"));

                        objBA53.NumberDaysProratedMonth = Convert.ToInt32(FormatNull(txtNoDaysProratedMonth.Text, "Int"));

                        if (lblDatesProrated.Text.Trim() == "")
                        {
                            objBA53.DaysProrated = 0;
                        }
                        else
                        {
                            objBA53.DaysProrated = Convert.ToInt32(lblDatesProrated.Text);
                        }

                        if (lblTotalDayCatchUp.Text.Trim() == "")
                        {
                            objBA53.TotalDayCatchUp = 0;
                        }
                        else
                        {
                            objBA53.TotalDayCatchUp = Convert.ToDecimal(FormatNull(lblTotalDayCatchUp.Text, "Decimal"));
                        }

                        if (lblTotalCatchUp.Text.Trim() == "")
                        {
                            objBA53.TotalCatchUp = 0;
                        }
                        else
                        {
                            objBA53.TotalCatchUp = Convert.ToDecimal(FormatNull(lblTotalCatchUp.Text, "Decimal"));
                        }

                        objBA53.FY_YY = Convert.ToInt32(FormatNull(lblFY_YY.Text.Replace("FY", ""), "Int"));
                        objBA53.SaveCPI(CurrentUserID);

                        break;
                    case "2": //EXPANSION
                        sError = ValidateDropDownGrid(ddgActionType, "Accrual Type Action", sError);
                        sError = ValidateTextBox(txtAnnualRent, "'Annual Rent", sError);
                        sError = ValidateTextBox(txtCurrProjNo, "Project Number", sError);
                        sError = ValidateTextBox(txtNewProjAnnualRent, "New Projected Annual Rent", sError);
                        sError = ValidateTextBox(txtAnnualIncrOfAction, "Annual Increase of Action", sError);
                        sError = ValidateCalendarControl(calLeaseEffDate, "Lease Effective Date", sError);
                        sError = ValidateCalendarControl(calEffDateOfAction, "Effective Date of Action", sError);
                        sError = ValidateCalendarControl(calFiscalYearEndDate, "Fiscal Year End Date", sError);
                        sError = ValidateTextBox(txtNoDaysProratedMonth, "Number of Days in Prorated Month", sError);

                        if (sError != "")
                        {
                            throw new Exception(sError);
                        }

                        objBA53.LeaseNumber = lblLeaseNum.Text;
                        objBA53.ProjectNumber = txtCurrProjNo.Text;
                        objBA53.NewProjectedAnnualRent = Convert.ToDecimal(FormatNull(txtNewProjAnnualRent.Text, "Decimal"));
                        objBA53.LeaseEffectiveDate = calLeaseEffDate.Date;
                        objBA53.AnnualRent = Convert.ToDecimal(FormatNull(txtAnnualRent.Text, "Decimal"));
                        objBA53.RSF = Convert.ToDecimal(txtRSF.Text);
                        objBA53.AnnualIncreaseOfAction = Convert.ToDecimal(FormatNull(txtAnnualIncrOfAction.Text, "Decimal"));
                        objBA53.MonthlyIncrease = Convert.ToDecimal(FormatNull(lblMonthlyIncrease.Text, "Decimal"));
                        objBA53.EffectiveDateOfAction = calEffDateOfAction.Date;
                        objBA53.TotalMonths = Convert.ToInt32(FormatNull(lblTotalMonths.Text, "Int"));
                        objBA53.FYEndDate = calFiscalYearEndDate.Date;
                        objBA53.TotalMonthsCatchUp = Convert.ToDecimal(FormatNull(lblTotalMonthsCatchUp1_2.Text, "Decimal"));
                        objBA53.NumberDaysProratedMonth = Convert.ToInt32(FormatNull(txtNoDaysProratedMonth.Text, "Int"));

                        //objBA53.DaysProrated = Convert.ToInt32(lblDatesProrated.Text);

                        //objBA53.TotalDayCatchUp = Convert.ToDecimal(FormatNull(lblTotalDayCatchUp.Text, "Decimal"));

                        //objBA53.TotalCatchUp = Convert.ToDecimal(FormatNull(lblTotalCatchUp.Text, "Decimal"));

                        if (lblDatesProrated.Text.Trim() == "")
                        {
                            objBA53.DaysProrated = 0;
                        }
                        else
                        {
                            objBA53.DaysProrated = Convert.ToInt32(lblDatesProrated.Text);
                        }

                        if (lblTotalDayCatchUp.Text.Trim() == "")
                        {
                            objBA53.TotalDayCatchUp = 0;
                        }
                        else
                        {
                            objBA53.TotalDayCatchUp = Convert.ToDecimal(FormatNull(lblTotalDayCatchUp.Text, "Decimal"));
                        }

                        if (lblTotalCatchUp.Text.Trim() == "")
                        {
                            objBA53.TotalCatchUp = 0;
                        }
                        else
                        {
                            objBA53.TotalCatchUp = Convert.ToDecimal(FormatNull(lblTotalCatchUp.Text, "Decimal"));
                        }
                        objBA53.FY_YY = Convert.ToInt32(FormatNull(lblFY_YY.Text.Replace("FY", ""), "Int"));
                        objBA53.SaveExpension(CurrentUserID);
                        break;
                    case "3": //HOLDOVER

                        //Current Lease Information section controls validation
                        sError = ValidateDropDownGrid(ddgActionType, "Accrual Type Action", sError);
                        sError = ValidateTextBox(txtAnnualRent, "Annual Rent", sError);
                        sError = ValidateCalendarControl(calLeaseEffDate, "Lease Effective Date", sError);
                        sError = ValidateCalendarControl(calLeaseExpDate, "Lease Expiration Date", sError);

                        //Projected New Lease Information section controls validation
                        sError = ValidateTextBox(txtPtojNewLeaseNo, "Projected New Lease Number", sError);
                        sError = ValidateTextBox(txtProjProjectNo, "Projected New Project Number", sError);
                        sError = ValidateTextBox(txtProjProspNo, "Prospectus Number:", sError);
                        sError = ValidateCalendarControl(calProjLeaseEffDate, "Projected New Lease Effective Date", sError);
                        sError = ValidateTextBox(txtProjAnnualRent, "Projected New Annual Rent: ", sError);
                        sError = ValidateTextBox(txtProjRSF, "Projected RSF", sError);

                        //Breakout of Part Year Cost of Established Accrual section controls validation
                        sError = ValidateTextBox(txtNoDaysProratedMonthEst, "Number of Days in Prorated Month (Established Accrual section)", sError);
                        sError = ValidateCalendarControl(calEffDateOfActionEst, "Effective Date of Action (Established Accrual section)", sError);
                        sError = ValidateCalendarControl(calFiscalYearEndDateEst, "Fiscal Year End Date (Established Accrual section)", sError);

                        //Breakout of Part Year Cost section controls validation
                        sError = ValidateTextBox(txtNoDaysProratedMonth, "Number of Days in Prorated Month (Breakout of Part Year Cost section)", sError);
                        sError = ValidateCalendarControl(calEffDateOfAction, "Effective Date of Action (Breakout of Part Year Cost section)", sError);
                        sError = ValidateCalendarControl(calCurrAuditEndDate, "Current Audit End Date", sError);

                        if (sError != "")
                        {
                            throw new Exception(sError);
                        }

                        if (sError != "")
                        {
                            throw new Exception(sError);
                        }

                        // Calculation Methodology - current lease information
                        objBA53.LeaseNumber = lblLeaseNum.Text;
                        objBA53.LeaseEffectiveDate = calLeaseEffDate.Date;
                        objBA53.LeaseExpirationDate = calLeaseExpDate.Date;
                        objBA53.AnnualRent = Convert.ToDecimal(FormatNull(txtAnnualRent.Text, "Decimal"));
                        objBA53.RSF = Convert.ToDecimal(txtRSF.Text);
                        // Calculation Methodology - projected new lease information
                        objBA53.ProjectedLeaseNo = txtPtojNewLeaseNo.Text;
                        objBA53.ProjectedProjectNo = txtProjProjectNo.Text;
                        objBA53.ProjectedProspectusNo = txtProjProspNo.Text;
                        objBA53.ProjectedsLeaseEffectiveDate = calProjLeaseEffDate.Date;
                        objBA53.ProjectedAnnualRent = Convert.ToDecimal(FormatNull(txtProjAnnualRent.Text, "Decimal"));
                        objBA53.ProjectedRSF = Convert.ToDecimal(txtProjRSF.Text);
                        //Breakout of Part Year Cost of Established Accrual
                        objBA53.EstablishedCurrentProjAnnualRent = Convert.ToDecimal(FormatNull(lblCurrProjAnnRentEst.Text, "Decimal"));
                        objBA53.EstablishedNewProjAnnualRent = Convert.ToDecimal(FormatNull(txtProjAnnualRent.Text, "Decimal"));
                        objBA53.EstablishedAnnualIncreaseOfAction = Convert.ToDecimal(FormatNull(lblAnnIncrOfActionEst.Text, "Decimal"));
                        objBA53.EstablishedMonthlyIncrease = Convert.ToDecimal(FormatNull(lblMonthlyIncreaseEst.Text, "Decimal"));
                        objBA53.EstablishedEffectiveDateOfAction = calEffDateOfActionEst.Date;
                        objBA53.EstablishedTotalMonths = Convert.ToInt32(FormatNull(lblTotalMonthsEst.Text, "Int"));
                        objBA53.EstablishedFYEndDate = calFiscalYearEndDateEst.Date;
                        objBA53.EstablishedTotalMonthsCatchUp = Convert.ToDecimal(FormatNull(lblTotalMonthsCatchUpEst.Text, "Decimal"));
                        objBA53.EstablishedNumberDaysProratedMonth = Convert.ToInt32(FormatNull(txtNoDaysProratedMonthEst.Text, "Int"));

                        //objBA53.EstablishedDaysProrated = Convert.ToInt32(FormatNull(lblDaysProratedEst.Text, "Int"));
                        //objBA53.EstablishedTotalDayCatchUp = Convert.ToDecimal(FormatNull(lblTotalDayCatchUpEst.Text, "Decimal"));
                        //objBA53.EstablishedTotalCatchUp = Convert.ToDecimal(FormatNull(lblTotalCatchUpEst.Text, "Decimal"));

                        if (lblDaysProratedEst.Text.Trim() == "")
                        {
                            objBA53.EstablishedDaysProrated = 0;
                        }
                        else
                        {
                            if (lblDaysProratedEst.Text != "")
                            {
                                objBA53.EstablishedDaysProrated = Convert.ToInt32(lblDaysProratedEst.Text);
                            }
                            else
                            {
                                objBA53.EstablishedDaysProrated = 0;
                            }
                        }

                        if (lblTotalDayCatchUpEst.Text.Trim() == "")
                        {
                            objBA53.EstablishedTotalDayCatchUp = 0;
                        }
                        else
                        {
                            objBA53.EstablishedTotalDayCatchUp = Convert.ToDecimal(FormatNull(lblTotalDayCatchUpEst.Text, "Decimal"));
                        }

                        if (lblTotalCatchUpEst.Text.Trim() == "")
                        {
                            objBA53.EstablishedTotalCatchUp = 0;
                        }
                        else
                        {
                            objBA53.EstablishedTotalCatchUp = Convert.ToDecimal(FormatNull(lblTotalCatchUpEst.Text, "Decimal"));
                        }



                        objBA53.FY_YY_est = Convert.ToInt32(FormatNull(lblFY_YY_Est.Text.Replace("FY", ""), "Int"));
                        //Breakout of Part Year Cost
                        objBA53.CurrProjectedAnnualRent = Convert.ToDecimal(FormatNull(lblCurrProjAnnRent.Text, "Decimal"));// !!!!!!!
                        objBA53.NewProjectedAnnualRent = Convert.ToDecimal(FormatNull(lblNewProjAnnRent.Text, "Decimal"));
                        objBA53.AnnualIncreaseOfAction = Convert.ToDecimal(FormatNull(txtAnnualIncrOfAction.Text, "Decimal"));
                        objBA53.MonthlyIncrease = Convert.ToDecimal(FormatNull(lblMonthlyIncrease.Text, "Decimal"));
                        objBA53.EffectiveDateOfAction = calEffDateOfAction.Date;
                        objBA53.TotalMonths = Convert.ToInt32(FormatNull(lblTotalMonths.Text, "Int"));
                        objBA53.CurrAuditEndDate = calCurrAuditEndDate.Date;
                        objBA53.TotalMonthsCatchUp = Convert.ToDecimal(FormatNull(lblTotalMonthsCatchUp3_5.Text, "Decimal"));
                        objBA53.NumberDaysProratedMonth = Convert.ToInt32(FormatNull(txtNoDaysProratedMonth.Text, "Int"));

                        //objBA53.DaysProrated = Convert.ToInt32(FormatNull(lblDatesProrated.Text, "Int"));
                        //objBA53.TotalDayCatchUp = Convert.ToDecimal(FormatNull(lblTotalDayCatchUp.Text, "Decimal"));
                        //objBA53.TotalCatchUp = Convert.ToDecimal(FormatNull(lblTotalCatchUp.Text, "Decimal"));

                        if (lblDatesProrated.Text.Trim() == "")
                        {
                            objBA53.DaysProrated = 0;
                        }
                        else
                        {
                            objBA53.DaysProrated = Convert.ToInt32(lblDatesProrated.Text);
                        }

                        if (lblTotalDayCatchUp.Text.Trim() == "")
                        {
                            objBA53.TotalDayCatchUp = 0;
                        }
                        else
                        {
                            objBA53.TotalDayCatchUp = Convert.ToDecimal(FormatNull(lblTotalDayCatchUp.Text, "Decimal"));
                        }

                        if (lblTotalCatchUp.Text.Trim() == "")
                        {
                            objBA53.TotalCatchUp = 0;
                        }
                        else
                        {
                            objBA53.TotalCatchUp = Convert.ToDecimal(FormatNull(lblTotalCatchUp.Text, "Decimal"));
                        }

                        objBA53.FY_YY = Convert.ToInt32(FormatNull(lblFY_YY.Text.Replace("FY", ""), "Int"));
                        objBA53.SaveHoldover(CurrentUserID);
                        break;
                    case "4":  //RET
                        // validation for "Accrual Type", "Reason Code", "Lease Number" and "RSF" fields already done on the top of the "Save" event

                        var sErrorYear = ValidateDropDownGrid_1(ddgCurrFY, "Current Year");

                        if (sErrorYear != "")
                        {
                            throw new Exception(sErrorYear);
                        }

                        sError = ValidateDropDownGrid(ddgState, "State", sError);

                        //sError = ValidateTextBox(txtTeamCode, "Team Code", sError);


                        //'Calculation Methodology' section controls validation
                        sError = ValidateTextBox(txtAnnualRent, "Annual Rent", sError);
                        sError = ValidateCalendarControl(calLeaseEffDate, "Lease Effective Date", sError);

                        //'Breakout of Part Year Cost' section controls validation
                        //Validate controls in Current Year section

                        sError = ValidateTextBox(txt1stHalfTaxInvFY_DatePeriod, "Invoice Date for " + lbl1stHalfTaxInvFY.Text, sError);
                        sError = ValidateTextBox(txt1stHalfFY_TotalTaxBill, "Total of tax Bill for " + lbl1stHalfTaxInvFY.Text, sError);
                        sError = ValidateTextBox(txt1stHalfFY_NoMonthsReimb, "# of mos reimb for " + lbl1stHalfTaxInvFY.Text, sError);

                        sError = ValidateTextBox(txt2ndHalfTaxInvFY_DatePeriod, "Invoice Date for " + lbl12ndHalfTaxInvFY.Text, sError);
                        sError = ValidateTextBox(txt2ndHalfFY_TotalTaxBill, "Total of tax Bill for " + lbl12ndHalfTaxInvFY.Text, sError);
                        sError = ValidateTextBox(txt2ndHalfFY_NoMonthsReimb, "# of mos reimb for " + lbl12ndHalfTaxInvFY.Text, sError);



                        if (ddgState.ItemID == "1") // only for DC
                        {
                            sError = ValidateTextBox(txt1stHalfTaxInvFY1_DatePeriod, "Invoice Date for " + lbl1stHalfTaxInvFY1.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY1_TotalTaxBill, "Total of tax Bill for " + lbl1stHalfTaxInvFY1.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY1_NoMonthsReimb, "# of mos reimb for " + lbl1stHalfTaxInvFY1.Text, sError);
                        }

                        sError = ValidateTextBox(txtFY_TaxBaseYear, ddgCurrFY.ItemID + " Tax Base Year", sError);
                        sError = ValidateTextBox(txtFY_PercGovOccupReimb, ddgCurrFY.ItemID + " % Of Government Occupancy", sError);


                        if (ddgState.ItemID == "3" || ddgState.ItemID == "3") // only for VA and MD
                        {
                            //Validate controls in 'Current Year + 1' section 
                            sError = ValidateTextBox(txt1stHalfTaxInvFY1_DatePeriod_, "Invoice Date for " + lbl1stHalfTaxInvFY1_.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY1_TotalTaxBill_, "Total of tax Bill for " + lbl1stHalfTaxInvFY1_.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY1_NoMonthsReimb_, "# of mos reimb for " + lbl1stHalfTaxInvFY1_.Text, sError);
                        }

                        sError = ValidateTextBox(txt2ndHalfTaxInvFY1_DatePeriod, "Invoice Date for " + lbl12ndHalfTaxInvFY1.Text, sError);
                        sError = ValidateTextBox(txt2ndHalfFY1_TotalTaxBill, "Total of tax Bill for " + lbl12ndHalfTaxInvFY1.Text, sError);
                        sError = ValidateTextBox(txt2ndHalfFY1_NoMonthsReimb, "# of mos reimb for " + lbl12ndHalfTaxInvFY1.Text, sError);



                        if (ddgState.ItemID == "1") // only for DC
                        {
                            sError = ValidateTextBox(txt1stHalfTaxInvFY2_DatePeriod, "Invoice Date for " + lbl1stHalfTaxInvFY2.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY2_TotalTaxBill, "Total of tax Bill for " + lbl1stHalfTaxInvFY2.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY2_NoMonthsReimb, "# of mos reimb for " + lbl1stHalfTaxInvFY2.Text, sError);
                        }

                        sError = ValidateTextBox(txtFY_TaxBaseYear, ddgCurrFY.ItemID + " Tax Base Year", sError);
                        sError = ValidateTextBox(txtFY_PercGovOccupReimb, ddgCurrFY.ItemID + " % Of Government Occupancy", sError);

                        //Escalation Projection fields validation
                        //sError = ValidateTextBox(txtFY2_RETEscProjPercIncrease, ddgCurrFY.ItemID + "% Increase for " + lblFY2_RETEscPtojTitile.Text, sError);
                        //sError = ValidateTextBox(txtFY2_RETEscProjAccrual, ddgCurrFY.ItemID + "Accrual for " + lblFY2_RETEscPtojTitile.Text, sError);
                        //sError = ValidateTextBox(txtFY2_RETEscProjPercIncreaseRev, ddgCurrFY.ItemID + "% Increase for " + lblFY2_RETEscPtojRevTitile.Text, sError);

                        if (ddgReasonForDelay.Table.Rows[ddgReasonForDelay.SelectedIndex][1].ToString().ToLower().Trim() == "other")
                        {
                            tr_other_reason_for_delay.AddDisplay();
                            sError = ValidateTextBox(txtOtherReasonForDelay, " description value for the Reason for Delay", sError);
                        }



                        if (sError != "")
                        {
                            throw new Exception(sError);
                        }
                        objBA53.ReasonForDelayID = Convert.ToInt16(ddgReasonForDelay.Table.Rows[ddgReasonForDelay.SelectedIndex][0].ToString());
                        objBA53.StateID = Convert.ToInt16(ddgState.Table.Rows[ddgState.SelectedIndex][0].ToString());

                        // Calculation Methodology
                        objBA53.LeaseNumber = lblLeaseNum.Text;
                        objBA53.LeaseEffectiveDate = calLeaseEffDate.Date;
                        objBA53.AnnualRent = Convert.ToDecimal(FormatNull(txtAnnualRent.Text, "Decimal"));
                        objBA53.RSF = Convert.ToDecimal(txtRSF.Text);

                        //Breakout of Part Year Cost for the FY
                        objBA53.FY_FirstHalfTaxInvoiceDatePeriod = txt1stHalfTaxInvFY_DatePeriod.Text;
                        objBA53.FY_FirstHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt1stHalfFY_TotalTaxBill.Text, "Decimal"));
                        objBA53.FY_FirstHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt1stHalfFY_NoMonthsReimb.Text, "Int"));
                        objBA53.FY_FirstHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl1stHalfFY_ReimbAmt.Text, "Decimal")); //calculated

                        objBA53.FY_SecondHalfTaxInvoiceDatePeriod = txt2ndHalfTaxInvFY_DatePeriod.Text;
                        objBA53.FY_SecondHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt2ndHalfFY_TotalTaxBill.Text, "Decimal"));
                        objBA53.FY_SecondHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt2ndHalfFY_NoMonthsReimb.Text, "Int"));
                        objBA53.FY_SecondHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl2ndHalfFY_ReimbAmt.Text, "Decimal")); //calculated

                        // --- see marked the same way below ---
                        objBA53.FY1_FirstHalfTaxInvoiceDatePeriod = txt1stHalfTaxInvFY1_DatePeriod.Text;
                        objBA53.FY1_FirstHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill.Text, "Decimal"));
                        objBA53.FY1_FirstHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt1stHalfFY1_NoMonthsReimb.Text, "Int"));
                        objBA53.FY1_FirstHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl1stHalfFY1_ReimbAmt.Text, "Decimal")); //calculated
                        // ---

                        objBA53.FY_TaxBillReceiptTotal = Convert.ToDecimal(FormatNull(lblFY_TaxBillReceipt_Total.Text, "Decimal"));//calculated
                        objBA53.FY_TaxBillReceiptNoOfMonReimb = Convert.ToInt32(FormatNull(lblFY_TaxBillReceipt_NoMonthReimb.Text, "Int"));//calculated
                        objBA53.FY_TaxBillReceiptReimbAmt = Convert.ToDecimal(FormatNull(lblFY_TaxBillReceipt_Reimb.Text, "Decimal"));//calculated
                        objBA53.FY_TaxBaseYearReimb = Convert.ToDecimal(FormatNull(txtFY_TaxBaseYear.Text, "Decimal"));
                        objBA53.FY_NetAmountReimb = Convert.ToDecimal(FormatNull(lblFY_NetAmountReimb.Text, "Decimal"));//calculated
                        objBA53.FY_PercOfGovOccupancyReimb = Convert.ToDecimal(FormatNull(txtFY_PercGovOccupReimb.Text, "Decimal"));
                        objBA53.FY_AmountDueLessorReimb = Convert.ToDecimal(FormatNull(lblFY_AmtDueLessorReimb.Text, "Decimal"));//calculated

                        //---  see marked the same way above ---
                        //Breakout of Part Year Cost for the FY+1
                        //objBA53.FY1_FirstHalfTaxInvoiceDatePeriod = txt1stHalfTaxInvFY1_DatePeriod_.Text; //calculated 
                        //objBA53.FY1_FirstHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill_.Text, "Decimal"));
                        //objBA53.FY1_FirstHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt1stHalfFY1_NoMonthsReimb_.Text, "Int"));
                        //objBA53.FY1_FirstHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl1stHalfFY1_ReimbAmt_.Text, "Decimal")); //calculated
                        //---

                        objBA53.FY1_SecondHalfTaxInvoiceDatePeriod = txt2ndHalfTaxInvFY1_DatePeriod.Text; //calculated
                        objBA53.FY1_SecondHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt2ndHalfFY1_TotalTaxBill.Text, "Decimal"));
                        objBA53.FY1_SecondHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt2ndHalfFY1_NoMonthsReimb.Text, "Int"));
                        objBA53.FY1_SecondHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl2ndHalfFY1_ReimbAmt.Text, "Decimal")); //calculated

                        objBA53.FY2_FirstHalfTaxInvoiceDatePeriod = txt1stHalfTaxInvFY1_DatePeriod.Text; //calculated
                        objBA53.FY2_FirstHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt1stHalfFY2_TotalTaxBill.Text, "Decimal"));
                        objBA53.FY2_FirstHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt1stHalfFY2_NoMonthsReimb.Text, "Int"));
                        objBA53.FY2_FirstHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl1stHalfFY2_ReimbAmt.Text, "Decimal")); //calculated

                        objBA53.FY1_TaxBillReceiptTotal = Convert.ToDecimal(FormatNull(lblFY1_TaxBillReceipt_Total.Text, "Decimal"));//calculated
                        objBA53.FY1_TaxBillReceiptNoOfMonReimb = Convert.ToInt32(FormatNull(lblFY1_TaxBillReceipt_NoMonthReimb.Text, "Int"));//calculated
                        objBA53.FY1_TaxBillReceiptReimbAmt = Convert.ToDecimal(FormatNull(lblFY1_TaxBillReceipt_Reimb.Text, "Decimal"));//calculated
                        objBA53.FY1_TaxBaseYearReimb = Convert.ToDecimal(FormatNull(txtFY1_TaxBaseYear.Text, "Decimal"));
                        objBA53.FY1_NetAmountReimb = Convert.ToDecimal(FormatNull(lblFY1_NetAmountReimb.Text, "Decimal"));//calculated
                        objBA53.FY1_PercOfGovOccupancyReimb = Convert.ToDecimal(FormatNull(txtFY1_PercGovOccup.Text, "Decimal"));
                        objBA53.FY1_AmountDueLessorReimb = Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal"));//calculated

                        //FY+2 Escalation Projection
                        objBA53.FY2_RETEscalationProjectionNetIncr = Convert.ToDecimal(FormatNull(lblFY2_RETEscProjNetIncrease.Text, "Decimal"));//calculated
                        objBA53.FY2_RETEscalationProjectionPercIncr = Convert.ToDecimal(FormatNull(txtFY2_RETEscProjPercIncrease.Text, "Decimal"));
                        objBA53.FY2_RETEscalationProjectionAccr = Convert.ToDecimal(FormatNull(txtFY2_RETEscProjAccrual.Text, "Decimal"));

                        //FY+2 Escalation Projection(revised)
                        objBA53.FY2_RETEscalationProjectionRevNetIncr = Convert.ToDecimal(FormatNull(lblFY2_RETEscProjNetIncreaseRev.Text, "Decimal"));//calculated
                        objBA53.FY2_RETEscalationProjectionRevPercIncr = Convert.ToDecimal(FormatNull(txtFY2_RETEscProjPercIncreaseRev.Text, "Decimal"));//calculated
                        objBA53.FY2_RETEscalationProjectionRevAccr = Convert.ToDecimal(FormatNull(txtFY2_RETEscProjAccrualRev.Text, "Decimal"));//calculated
                        if (ddgCurrFY.SelectedIndex != 0)
                        {
                            objBA53.FY_YY = Convert.ToInt32(ddgCurrFY.ItemID.ToString());
                        }
                        else
                        {
                            objBA53.FY_YY = 0;
                        }
                        if (lblFY1.Text == "")
                        {
                        }
                        else
                        {
                            objBA53.FY_YY_est = Convert.ToInt32(lblFY1.Text);
                        }

                        objBA53.OtherReasonForDelay = txtOtherReasonForDelay.Text;

                        //Convert.ToInt32(FormatNull(lblFY1.Text.Replace("FY", ""), "Int"));//calculated

                        objBA53.SaveRET(CurrentUserID);
                        break;

                    case "5": //STEP RENT

                        //Validate controls in Calculation Methodology section
                        sError = ValidateCalendarControl(calEffDateOfAction5, "Effective Date of Action (Calculation Methodology section)", sError);
                        sError = ValidateTextBox(txtCurrProjNo, "Project Number", sError);
                        sError = ValidateTextBox(txtNewProjAnnualRent, "New Project Annual Rent (Calculation Methodology section)", sError);

                        //Validate controls in Established Accrual section
                        sError = ValidateTextBox(txtOldProjAnnRent_est, "Old Project Annual Rent (Established Accrual section)", sError);
                        sError = ValidateTextBox(txtNewProjAnnRent_est, "New Project Annual Rent (Established Accrual section)", sError);
                        sError = ValidateCalendarControl(calEffDateOfActionEst, "Effective Date of Action (Established Accrual section)", sError);
                        sError = ValidateCalendarControl(calFiscalYearEndDateEst, "Fiscal Year End Date (Established Accrual section)", sError);
                        sError = ValidateTextBox(txtNoDaysProratedMonthEst, "Number of Days in Prorated Month (Established Accrual section)", sError);

                        //Validate controls in Breakout of Part Year Cost - Revised section
                        sError = ValidateTextBox(txtOldProjAnnRent, "Old Project Annual Rent (Revised section)", sError);
                        sError = ValidateTextBox(txtNewProjAnnRent, "New Project Annual Rent (Revised section)", sError);
                        sError = ValidateTextBox(txtAnnualIncrOfAction, "Annual Increase of Action (Revised section)", sError);
                        sError = ValidateCalendarControl(calEffDateOfAction, "Effective Date of Action (Revised section))", sError);
                        //sError = ValidateCalendarControl(calCurrAuditEndDate, "Current Audit End Date (Revised section)", sError);
                        sError = ValidateCalendarControl(calFiscalYearEndDate, "Fiscal Year End Date (Revised section)", sError);
                        sError = ValidateTextBox(txtNoDaysProratedMonth, "Number of Days in Prorated Month (Revised section)", sError);

                        if (sError != "")
                        {
                            throw new Exception(sError);
                        }

                        // Calculation Methodology
                        objBA53.LeaseNumber = lblLeaseNum.Text;
                        objBA53.ProjectNumber = txtCurrProjNo.Text;
                        objBA53.EffectiveDateOfAction5 = calEffDateOfAction5.Date;
                        var s = txtNewProjAnnualRent.Text;
                        objBA53.NewProjectedAnnualRent = Convert.ToDecimal(FormatNull(txtNewProjAnnualRent.Text, "Decimal"));
                        //txtNewProjAnnRent
                        //txtNewProjAnnualRent
                        objBA53.RSF = Convert.ToDecimal(txtRSF.Text);

                        // Breakout of Part Year Cost of Established Accrual
                        objBA53.EstablishedOldProjAnnualRent = Convert.ToDecimal(FormatNull(txtOldProjAnnRent_est.Text, "Decimal"));
                        objBA53.EstablishedNewProjAnnualRent = Convert.ToDecimal(FormatNull(txtNewProjAnnRent_est.Text, "Decimal"));
                        objBA53.EstablishedAnnualIncreaseOfAction = Convert.ToDecimal(FormatNull(lblAnnIncrOfActionEst.Text, "Decimal"));//calculated
                        objBA53.EstablishedMonthlyIncrease = Convert.ToDecimal(FormatNull(lblMonthlyIncreaseEst.Text, "Decimal"));//calculated
                        objBA53.EstablishedEffectiveDateOfAction = calEffDateOfActionEst.Date;
                        objBA53.EstablishedTotalMonths = Convert.ToInt32(FormatNull(lblTotalMonthsEst.Text, "Decimal"));//calculated;
                        objBA53.EstablishedFYEndDate = calFiscalYearEndDateEst.Date;
                        objBA53.EstablishedTotalMonthsCatchUp = Convert.ToDecimal(FormatNull(lblTotalMonthsCatchUpEst.Text, "Decimal"));//calculated
                        objBA53.EstablishedNumberDaysProratedMonth = Convert.ToInt32(FormatNull(txtNoDaysProratedMonthEst.Text, "Decimal"));

                        //objBA53.EstablishedDaysProrated = Convert.ToInt32(FormatNull(lblDaysProratedEst.Text, "Decimal"));//calculated
                        //objBA53.EstablishedTotalDayCatchUp = Convert.ToDecimal(FormatNull(lblTotalDayCatchUpEst.Text, "Decimal"));//calculated
                        //objBA53.EstablishedTotalCatchUp = Convert.ToDecimal(FormatNull(lblTotalCatchUpEst.Text, "Decimal"));

                        if (lblDaysProratedEst.Text.Trim() == "")
                        {
                            objBA53.EstablishedDaysProrated = 0;
                        }
                        else
                        {
                            objBA53.EstablishedDaysProrated = Convert.ToInt32(lblDatesProrated.Text);
                        }

                        if (lblTotalDayCatchUpEst.Text.Trim() == "")
                        {
                            objBA53.EstablishedTotalDayCatchUp = 0;
                        }
                        else
                        {
                            objBA53.EstablishedTotalDayCatchUp = Convert.ToDecimal(FormatNull(lblTotalDayCatchUp.Text, "Decimal"));
                        }

                        if (lblTotalCatchUpEst.Text.Trim() == "")
                        {
                            objBA53.EstablishedTotalCatchUp = 0;
                        }
                        else
                        {
                            objBA53.EstablishedTotalCatchUp = Convert.ToDecimal(FormatNull(lblTotalCatchUpEst.Text, "Decimal"));
                        }

                        objBA53.FY_YY_est = Convert.ToInt32(FormatNull(lblFY_YY_Est.Text.Replace("FY", ""), "Int"));//calculated

                        //Breakout of Part Year Cost
                        objBA53.OldProjectedAnnualRent = Convert.ToDecimal(FormatNull(txtOldProjAnnRent.Text, "Decimal"));// !!!!!!!
                        //objBA53.NewProjectedAnnualRent = Convert.ToDecimal(FormatNull(txtNewProjAnnRent.Text, "Decimal"));
                        objBA53.AnnualIncreaseOfAction = Convert.ToDecimal(FormatNull(txtAnnualIncrOfAction.Text, "Decimal"));
                        objBA53.MonthlyIncrease = Convert.ToDecimal(FormatNull(lblMonthlyIncrease.Text, "Decimal"));
                        objBA53.EffectiveDateOfAction = calEffDateOfAction.Date;
                        objBA53.TotalMonths = Convert.ToInt32(FormatNull(lblTotalMonths.Text, "Int"));
                        objBA53.CurrAuditEndDate = calCurrAuditEndDate.Date;
                        objBA53.TotalMonthsCatchUp = Convert.ToDecimal(FormatNull(lblTotalMonthsCatchUp3_5.Text, "Decimal"));
                        objBA53.NumberDaysProratedMonth = Convert.ToInt32(FormatNull(txtNoDaysProratedMonth.Text, "Int"));

                        //objBA53.DaysProrated = Convert.ToInt32(FormatNull(lblDatesProrated.Text, "Int"));
                        //objBA53.TotalDayCatchUp = Convert.ToDecimal(FormatNull(lblTotalDayCatchUp.Text, "Decimal"));
                        //objBA53.TotalCatchUp = Convert.ToDecimal(FormatNull(lblTotalCatchUp.Text, "Decimal"));

                        if (lblDatesProrated.Text.Trim() == "")
                        {
                            objBA53.DaysProrated = 0;
                        }
                        else
                        {
                            objBA53.DaysProrated = Convert.ToInt32(lblDatesProrated.Text);
                        }

                        if (lblTotalDayCatchUp.Text.Trim() == "")
                        {
                            objBA53.TotalDayCatchUp = 0;
                        }
                        else
                        {
                            objBA53.TotalDayCatchUp = Convert.ToDecimal(FormatNull(lblTotalDayCatchUp.Text, "Decimal"));
                        }

                        if (lblTotalCatchUp.Text.Trim() == "")
                        {
                            objBA53.TotalCatchUp = 0;
                        }
                        else
                        {
                            objBA53.TotalCatchUp = Convert.ToDecimal(FormatNull(lblTotalCatchUp.Text, "Decimal"));
                        }


                        objBA53.FY_YY = Convert.ToInt32(FormatNull(lblFY_YY.Text.Replace("FY", ""), "Int"));

                        objBA53.FYEndDate = calFiscalYearEndDate.Date;
                        objBA53.RevisedNewProjAnnualRent = Convert.ToDecimal(FormatNull(txtNewProjAnnRent.Text, "Decimal"));

                        // here to save BID we use the same fonction as we used to save RENT
                        objBA53.SaveStepRent(CurrentUserID);
                        break;
                    case "6": //BID (same as RET)*

                        sErrorYear = ValidateDropDownGrid_1(ddgCurrFY, "Current Year");

                        if (sErrorYear != "")
                        {
                            throw new Exception(sErrorYear);
                        }


                        sError = ValidateDropDownGrid(ddgState, "State", sError);

                        // sError = ValidateTextBox(txtTeamCode, "Team Code", sError);

                        //'Calculation Methodology' section controls validation
                        sError = ValidateTextBox(txtAnnualRent, "Annual Rent", sError);
                        sError = ValidateCalendarControl(calLeaseEffDate, "Lease Effective Date", sError);

                        //'Breakout of Part Year Cost' section controls validation
                        //Validate controls in Current Year section
                        sError = ValidateTextBox(txt1stHalfTaxInvFY_DatePeriod, "Invoice Date for " + lbl1stHalfTaxInvFY.Text, sError);
                        sError = ValidateTextBox(txt1stHalfFY_TotalTaxBill, "Total of tax Bill for " + lbl1stHalfTaxInvFY.Text, sError);
                        sError = ValidateTextBox(txt1stHalfFY_NoMonthsReimb, "# of mos reimb for " + lbl1stHalfTaxInvFY.Text, sError);

                        sError = ValidateTextBox(txt2ndHalfTaxInvFY_DatePeriod, "Invoice Date for " + lbl12ndHalfTaxInvFY.Text, sError);
                        sError = ValidateTextBox(txt2ndHalfFY_TotalTaxBill, "Total of tax Bill for " + lbl12ndHalfTaxInvFY.Text, sError);
                        sError = ValidateTextBox(txt2ndHalfFY_NoMonthsReimb, "# of mos reimb for " + lbl12ndHalfTaxInvFY.Text, sError);



                        if (ddgState.ItemID == "1") // only for DC
                        {
                            sError = ValidateTextBox(txt1stHalfTaxInvFY1_DatePeriod, "Invoice Date for " + lbl1stHalfTaxInvFY1.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY1_TotalTaxBill, "Total of tax Bill for " + lbl1stHalfTaxInvFY1.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY1_NoMonthsReimb, "# of mos reimb for " + lbl1stHalfTaxInvFY1.Text, sError);
                        }

                        sError = ValidateTextBox(txtFY_TaxBaseYear, ddgCurrFY.ItemID + " Tax Base Year", sError);
                        sError = ValidateTextBox(txtFY_PercGovOccupReimb, ddgCurrFY.ItemID + " % Of Government Occupancy", sError);

                        if (ddgState.ItemID == "3") // only for VA
                        {
                            //Validate controls in 'Current Year + 1' section 
                            sError = ValidateTextBox(txt1stHalfTaxInvFY1_DatePeriod_, "Invoice Date for " + lbl1stHalfTaxInvFY1_.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY1_TotalTaxBill_, "Total of tax Bill for " + lbl1stHalfTaxInvFY1_.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY1_NoMonthsReimb_, "# of mos reimb for " + lbl1stHalfTaxInvFY1_.Text, sError);
                        }

                        sError = ValidateTextBox(txt2ndHalfTaxInvFY1_DatePeriod, "Invoice Date for " + lbl12ndHalfTaxInvFY1.Text, sError);
                        sError = ValidateTextBox(txt2ndHalfFY1_TotalTaxBill, "Total of tax Bill for " + lbl12ndHalfTaxInvFY1.Text, sError);
                        sError = ValidateTextBox(txt2ndHalfFY1_NoMonthsReimb, "# of mos reimb for " + lbl12ndHalfTaxInvFY1.Text, sError);



                        if (ddgState.ItemID == "1") // only for DC
                        {
                            sError = ValidateTextBox(txt1stHalfTaxInvFY2_DatePeriod, "Invoice Date for " + lbl1stHalfTaxInvFY2.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY2_TotalTaxBill, "Total of tax Bill for " + lbl1stHalfTaxInvFY2.Text, sError);
                            sError = ValidateTextBox(txt1stHalfFY2_NoMonthsReimb, "# of mos reimb for " + lbl1stHalfTaxInvFY2.Text, sError);
                        }

                        sError = ValidateTextBox(txtFY_TaxBaseYear, ddgCurrFY.ItemID + " Tax Base Year", sError);
                        sError = ValidateTextBox(txtFY_PercGovOccupReimb, ddgCurrFY.ItemID + " % Of Government Occupancy", sError);

                        //Escalation Projection fields validation
                        //sError = ValidateTextBox(txtFY2_RETEscProjPercIncrease, ddgCurrFY.ItemID + "% Increase for " + lblFY2_RETEscPtojTitile.Text, sError);
                        //sError = ValidateTextBox(txtFY2_RETEscProjAccrual, ddgCurrFY.ItemID + "Accrual for " + lblFY2_RETEscPtojTitile.Text, sError);
                        //sError = ValidateTextBox(txtFY2_RETEscProjPercIncreaseRev, ddgCurrFY.ItemID + "% Increase for " + lblFY2_RETEscPtojRevTitile.Text, sError);

                        if (ddgReasonForDelay.Table.Rows[ddgReasonForDelay.SelectedIndex][1].ToString().ToLower().Trim() == "other")
                        {
                            tr_other_reason_for_delay.AddDisplay();
                            sError = ValidateTextBox(txtOtherReasonForDelay, ddgCurrFY.ItemID + " description value for the Reason for Delay", sError);
                        }


                        if (sError != "")
                        {
                            throw new Exception(sError);
                        }

                        objBA53.ReasonForDelayID = Convert.ToInt16(ddgReasonForDelay.Table.Rows[ddgReasonForDelay.SelectedIndex][0].ToString());
                        objBA53.StateID = Convert.ToInt16(ddgState.Table.Rows[ddgState.SelectedIndex][0].ToString());


                        // Calculation Methodology
                        objBA53.LeaseNumber = lblLeaseNum.Text;
                        objBA53.LeaseEffectiveDate = calLeaseEffDate.Date;
                        objBA53.AnnualRent = Convert.ToDecimal(FormatNull(txtAnnualRent.Text, "Decimal"));
                        objBA53.RSF = Convert.ToDecimal(txtRSF.Text);

                        //Breakout of Part Year Cost for the FY
                        objBA53.FY_FirstHalfTaxInvoiceDatePeriod = txt1stHalfTaxInvFY_DatePeriod.Text;
                        objBA53.FY_FirstHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt1stHalfFY_TotalTaxBill.Text, "Decimal"));
                        objBA53.FY_FirstHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt1stHalfFY_NoMonthsReimb.Text, "Int"));
                        objBA53.FY_FirstHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl1stHalfFY_ReimbAmt.Text, "Decimal")); //calculated

                        objBA53.FY_SecondHalfTaxInvoiceDatePeriod = txt2ndHalfTaxInvFY_DatePeriod.Text;
                        objBA53.FY_SecondHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt2ndHalfFY_TotalTaxBill.Text, "Decimal"));
                        objBA53.FY_SecondHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt2ndHalfFY_NoMonthsReimb.Text, "Int"));
                        objBA53.FY_SecondHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl2ndHalfFY_ReimbAmt.Text, "Decimal")); //calculated

                        // --- see marked the same way below ---
                        objBA53.FY1_FirstHalfTaxInvoiceDatePeriod = txt1stHalfTaxInvFY1_DatePeriod.Text;
                        objBA53.FY1_FirstHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill.Text, "Decimal"));
                        objBA53.FY1_FirstHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt1stHalfFY1_NoMonthsReimb.Text, "Int"));
                        objBA53.FY1_FirstHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl1stHalfFY1_ReimbAmt.Text, "Decimal")); //calculated
                        // ---

                        objBA53.FY_TaxBillReceiptTotal = Convert.ToDecimal(FormatNull(lblFY_TaxBillReceipt_Total.Text, "Decimal"));//calculated
                        objBA53.FY_TaxBillReceiptNoOfMonReimb = Convert.ToInt32(FormatNull(lblFY_TaxBillReceipt_NoMonthReimb.Text, "Int"));//calculated
                        objBA53.FY_TaxBillReceiptReimbAmt = Convert.ToDecimal(FormatNull(lblFY_TaxBillReceipt_Reimb.Text, "Decimal"));//calculated
                        objBA53.FY_TaxBaseYearReimb = Convert.ToDecimal(FormatNull(txtFY_TaxBaseYear.Text, "Decimal"));
                        objBA53.FY_NetAmountReimb = Convert.ToDecimal(FormatNull(lblFY_NetAmountReimb.Text, "Decimal"));//calculated
                        objBA53.FY_PercOfGovOccupancyReimb = Convert.ToDecimal(FormatNull(txtFY_PercGovOccupReimb.Text, "Decimal"));
                        objBA53.FY_AmountDueLessorReimb = Convert.ToDecimal(FormatNull(lblFY_AmtDueLessorReimb.Text, "Decimal"));//calculated

                        //---  see marked the same way above ---
                        //Breakout of Part Year Cost for the FY+1
                        //objBA53.FY1_FirstHalfTaxInvoiceDatePeriod = txt1stHalfTaxInvFY1_DatePeriod_.Text; //calculated 
                        //objBA53.FY1_FirstHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt1stHalfFY1_TotalTaxBill_.Text, "Decimal"));
                        //objBA53.FY1_FirstHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt1stHalfFY1_NoMonthsReimb_.Text, "Int"));
                        //objBA53.FY1_FirstHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl1stHalfFY1_ReimbAmt_.Text, "Decimal")); //calculated
                        //---

                        objBA53.FY1_SecondHalfTaxInvoiceDatePeriod = txt2ndHalfTaxInvFY1_DatePeriod.Text; //calculated
                        objBA53.FY1_SecondHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt2ndHalfFY1_TotalTaxBill.Text, "Decimal"));
                        objBA53.FY1_SecondHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt2ndHalfFY1_NoMonthsReimb.Text, "Int"));
                        objBA53.FY1_SecondHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl2ndHalfFY1_ReimbAmt.Text, "Decimal")); //calculated

                        objBA53.FY2_FirstHalfTaxInvoiceDatePeriod = txt1stHalfTaxInvFY1_DatePeriod.Text; //calculated
                        objBA53.FY2_FirstHalfTaxInvoiceTotalTaxBill = Convert.ToDecimal(FormatNull(txt1stHalfFY2_TotalTaxBill.Text, "Decimal"));
                        objBA53.FY2_FirstHalfTaxInvoiceNoOfMonReimb = Convert.ToInt32(FormatNull(txt1stHalfFY2_NoMonthsReimb.Text, "Int"));
                        objBA53.FY2_FirstHalfTaxInvoiceReimbAmt = Convert.ToDecimal(FormatNull(lbl1stHalfFY2_ReimbAmt.Text, "Decimal")); //calculated

                        objBA53.FY1_TaxBillReceiptTotal = Convert.ToDecimal(FormatNull(lblFY1_TaxBillReceipt_Total.Text, "Decimal"));//calculated
                        objBA53.FY1_TaxBillReceiptNoOfMonReimb = Convert.ToInt32(FormatNull(lblFY1_TaxBillReceipt_NoMonthReimb.Text, "Int"));//calculated
                        objBA53.FY1_TaxBillReceiptReimbAmt = Convert.ToDecimal(FormatNull(lblFY1_TaxBillReceipt_Reimb.Text, "Decimal"));//calculated
                        objBA53.FY1_TaxBaseYearReimb = Convert.ToDecimal(FormatNull(txtFY1_TaxBaseYear.Text, "Decimal"));
                        objBA53.FY1_NetAmountReimb = Convert.ToDecimal(FormatNull(lblFY1_NetAmountReimb.Text, "Decimal"));//calculated
                        objBA53.FY1_PercOfGovOccupancyReimb = Convert.ToDecimal(FormatNull(txtFY1_PercGovOccup.Text, "Decimal"));
                        objBA53.FY1_AmountDueLessorReimb = Convert.ToDecimal(FormatNull(lblFY1_AmtDueLessor.Text, "Decimal"));//calculated

                        //FY+2 Escalation Projection
                        objBA53.FY2_RETEscalationProjectionNetIncr = Convert.ToDecimal(FormatNull(lblFY2_RETEscProjNetIncrease.Text, "Decimal"));//calculated
                        objBA53.FY2_RETEscalationProjectionPercIncr = Convert.ToDecimal(FormatNull(txtFY2_RETEscProjPercIncrease.Text, "Decimal"));
                        objBA53.FY2_RETEscalationProjectionAccr = Convert.ToDecimal(FormatNull(txtFY2_RETEscProjAccrual.Text, "Decimal"));

                        //FY+2 Escalation Projection(revised)
                        objBA53.FY2_RETEscalationProjectionRevNetIncr = Convert.ToDecimal(FormatNull(lblFY2_RETEscProjNetIncreaseRev.Text, "Decimal"));//calculated
                        objBA53.FY2_RETEscalationProjectionRevPercIncr = Convert.ToDecimal(FormatNull(txtFY2_RETEscProjPercIncreaseRev.Text, "Decimal"));//calculated
                        objBA53.FY2_RETEscalationProjectionRevAccr = Convert.ToDecimal(FormatNull(txtFY2_RETEscProjAccrualRev.Text, "Decimal"));//calculated

                        if (ddgCurrFY.SelectedIndex != 0)
                        {
                            objBA53.FY_YY = Convert.ToInt32(ddgCurrFY.ItemID.ToString());
                        }
                        else
                        {
                            objBA53.FY_YY = 0;
                        }
                        if (lblFY1.Text == "")
                        {
                        }
                        else
                        {
                            objBA53.FY_YY_est = Convert.ToInt32(lblFY1.Text);
                        }

                        objBA53.OtherReasonForDelay = txtOtherReasonForDelay.Text;

                        //Convert.ToInt32(FormatNull(lblFY1.Text.Replace("FY", ""), "Int"));//calculated

                        objBA53.SaveRET(CurrentUserID);
                        break;

                    case "7":  //CLAIM

                        sError = ValidateCalendarControl(calLeaseEffDate, "Lease Effective Date", sError);
                        sError = ValidateTextBox(txtAnnualRent, "'Annual Rent", sError);

                        if (sError != "")
                        {
                            throw new Exception(sError);
                        }

                        objBA53.LeaseNumber = lblLeaseNum.Text;
                        objBA53.LeaseEffectiveDate = calLeaseEffDate.Date;
                        objBA53.AnnualRent = Convert.ToDecimal(FormatNull(txtAnnualRent.Text, "Decimal"));
                        objBA53.RSF = Convert.ToDecimal(txtRSF.Text);
                        objBA53.SaveCLAIM(CurrentUserID);

                        break;
                    default:
                        if (sError != "")
                        {
                            throw new Exception(sError);
                        }
                        break;

                }

                Response.Redirect(ItemsViewSourcePath + "?back=y");
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                {
                    //lblError.Text = GetErrors();
                    ClientScript.RegisterStartupScript(typeof(Page), "JumpScrollUp", "jumpScrollUp()", true);
                }
            }

        }



        private string ValidateDropDownGrid(Controls.DropDownGrid oDDG, string sFieldName, string sError)
        {

            if (oDDG.SelectedIndex == 0)
            {
                if (sError == "")
                {
                    sError = "Please enter '" + sFieldName + "'";
                }
                else
                {
                    sError = sError + "<br>Please enter '" + sFieldName + "'";
                }
                //oDDG.TblCSSClass = "tblGrid_Yellow";
                //oDDG.ItemCSSClass = "ArtYellow";
                //TblBorderClass
            }
            else
            {
                oDDG.TblCSSClass = "tblGrid";
                oDDG.ItemCSSClass = "Art";
            }
            return sError;
        }

        private string ValidateDropDownGrid_1(Controls.DropDownGrid oDDG, string sFieldName)
        {
            var sError = "";

            if (oDDG.SelectedIndex == 0)
            {
                sError = "Please enter '" + sFieldName + "'";
                //oDDG.TblCSSClass = "tblGrid_Yellow";
                //oDDG.ItemCSSClass = "ArtYellow";
            }
            else
            {
                oDDG.TblCSSClass = "tblGrid";
                oDDG.ItemCSSClass = "Art";
            }
            return sError;
        }
        private string ValidateTextBox(TextBox oTextBox, string sFieldName, string sError)
        {
            if (oTextBox.Text.Trim() == "")
            {
                if (sError == "")
                {
                    sError = "Please enter the " + sFieldName;
                }
                else
                {
                    sError = sError + "<br>Please enter the " + sFieldName;
                }
                //oTextBox.AddStyle("background-color:Yellow");
            }
            else
            {
                oTextBox.AddStyle("background-color:white");
            }
            return sError;
        }

        private string ValidateCalendarControl(Controls.GetDate oCalendarControl, string sFieldName, string sError)
        {
            if (oCalendarControl.isEmpty == true)
            {
                if (sError == "")
                {
                    sError = "Please enter '" + sFieldName + "'";
                }
                else
                {
                    sError = sError + "<br>Please enter '" + sFieldName + "'";
                }
                //oCalendarControl.DateCSSClass = "dateYellow";
            }
            else
            {
                oCalendarControl.DateCSSClass = "date";
            }

            return sError;
        }







        private string FormatNull(string sValue, string sDataType)
        {
            if (sValue != "" && sValue != null)
            {
                sValue = sValue.Replace("$", "");
                return sValue;
            }
            else
            {
                switch (sDataType)
                {
                    case "String":
                        return "";
                    case "Int":
                        return "0";
                    case "Decimal":
                        return "0";
                    case "DateTime":
                        return DateTime.MinValue.ToShortDateString();
                    default:
                        return (string)"";

                }
            }
        }

        public static string DisplayDecimalFormat(object InputValue)
        {
            var ret_value = "0";
            if (InputValue != DBNull.Value)
                ret_value = String.Format("{0:0,0.00}", InputValue);
            //ret_value = String.Format("{0,0.00}", InputValue);

            return ret_value;
        }
        protected void btnRecalculate_Click(object sender, EventArgs e)
        {
            var line_item = new LineBA53(Convert.ToInt32(txtItemID.Value), Convert.ToInt32(txtLineNo.Value));
            Recalculate(line_item);
        }

        public static string Right(string param, int length)
        {
            //start at the index based on the lenght of the sting minus
            //the specified lenght and assign it a variable
            var result = param.Substring(param.Length - length, length);
            //return the result of the operation
            return result;
        }
        public static string Left(string param, int length)
        {
            //we start at 0 since we want to get the characters starting from the
            //left and with the specified lenght and assign it to a variable
            var result = param.Substring(0, length);
            //return the result of the operation
            return result;
        }

        public static string Mid(string param, int startIndex, int length)
        {
            //start at the specified index in the string ang get N number of
            //characters depending on the lenght and assign it to a variable
            var result = param.Substring(startIndex, length);
            //return the result of the operation
            return result;
        }





        public static string AddLeadingZero(string param)
        {
            var result = "";
            if (param.Length == 1)
            {
                result = "0" + param;
            }
            else
            {
                result = param;
            }
            return result;
        }

        protected void rbSupportCalc_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReloadReasonCodeByRbSupportCalc();
            PaintButtons();
        }

        protected void ReloadReasonCodeByRbSupportCalc()
        {
            ddgReasonCode.ClearTable();

            // check if  file contain support for methodology for calculation of accrual
            if (rbSupportCalc.SelectedIndex == 0) // Yes (here we load all vald and invalid reasons)
            {
                ddgReasonCode.FillDropDownGridAllColumnsVisibleButID("spGetReviewerReasonCodes", "ReasonCodeID", "ReasonCodeDesc", "", false);
                lblValid.Text = "";
            }
            else  //No (here we load only invalid reasons)
            {
                ddgReasonCode.FillDropDownGridAllColumnsVisibleButID("spGetInvalidReviewerReasonCodes", "ReasonCodeID", "ReasonCodeDesc", "", false);
                lblValid.Text = "Invalid";
            }
            ddgReasonCode.SelectedIndex = 0;

        }
        protected void btnCancel_ServerClick(object sender, EventArgs e)
        {
            Response.Redirect("ReviewOpenItems.aspx");
        }
    }
}