namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;
    using Controls;
    using Data;

    public partial class FundsEntryData : PageBase
    {
        private readonly FundStatusBO FundStatus;
        private readonly UsersBO Users;

        public FundsEntryData()
        {
            FundStatus = new FundStatusBO(this.Dal);
            Users = new UsersBO(this.Dal, new EmailsBO(this.Dal));
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.ClientScript.IsClientScriptBlockRegistered(Page.GetType(), "JS"))
                    Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "JS",
                        "<script language='javascript' src='include/FundsEntryData.js'></script>");

                if (!IsPostBack)
                    InitProperties();                    
                else
                    lblError.Visible = false;

                DrawTable();
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
                lblError.Visible = true;
            }            
        }

        private void InitProperties()
        {
            if (!(User.IsInRole(((int)UserRoles.urFSBDAnalystFundsCoordinator).ToString()) || User.IsInRole(((int)UserRoles.urFSBDPowerReader).ToString()) ||
                    User.IsInRole(((int)UserRoles.urFSOrgAdminWR).ToString()) || User.IsInRole(((int)UserRoles.urFSOrgAdminRO).ToString())))
                throw new Exception("You are not authorized to visit this page.");

            Organization = Request.QueryString["org"];
            BusinessLineCode = Request.QueryString["bl"];
            FiscalYear = Request.QueryString["fy"];
            BookMonth = Request.QueryString["bm"];
            ReportGroupCode = Int32.Parse(Request.QueryString["gcd"]);
            AdjustmentsAdditional = Request.QueryString["adj"];

            txtBookMonth.Value = BookMonth;
            lblOrganization.Text = Organization;
            lblMonth.Text = String.Format("{0:MMMM}", DateTime.Parse(BookMonth + "/" + FiscalYear));
            lblFiscalYear.Text = FiscalYear;
            var dst = FSDataServices.GetFSReportConfiguration();
            if (dst != null && dst.Tables[0] != null && dst.Tables[0].Rows.Count > 0)
            {
                var dr_col = dst.Tables[0].Select("GROUP_CD=" + ReportGroupCode.ToString());
                if (dr_col.Length > 0)
                    lblReportGroup.Text = dr_col[0]["Name"].ToString();                    
            }
            lblReportGroupCode.Text = ReportGroupCode.ToString();
            var data_type = 0;
            if (!Int32.TryParse(Request.QueryString["t"], out data_type))
                throw new Exception("Error. Missing parameters.");
            DataEntryType = data_type;

            switch (DataEntryType)
            {
                case (int)FundsStatusUserEntryType.ueOneTimeAdjustment:
                    lblTitle.Text = "One Time Adjustments";
                    break;
                case (int)FundsStatusUserEntryType.ueOverUnderAccrued:
                    lblTitle.Text = "Over/Under Accrued";
                    break;
                case (int)FundsStatusUserEntryType.ueExpectedByYearEnd:
                    lblTitle.Text = "Expected by Year End";
                    break;
                default:
                    throw new Exception("Error. Missing parameters.");
            }

            //DisplayLastUpdateInfo
            DataSet ds = null;
            switch (DataEntryType)
            {
                case (int)FundsStatusUserEntryType.ueOneTimeAdjustment:
                    ds = History.GetFundStatusDataUpdateHistory((int)HistoryActions.haUpdateOneTimeAdjustment, Organization, FiscalYear, BookMonth, ReportGroupCode, BusinessLineCode);
                    break;
                case (int)FundsStatusUserEntryType.ueOverUnderAccrued:
                    ds = History.GetFundStatusDataUpdateHistory((int)HistoryActions.haUpdateOverUnderAccrued, Organization, FiscalYear, BookMonth, ReportGroupCode, BusinessLineCode);
                    break;
                case (int)FundsStatusUserEntryType.ueExpectedByYearEnd:
                    ds = History.GetFundStatusDataUpdateHistory((int)HistoryActions.haUpdateExpectedByYearEnd, Organization, FiscalYear, BookMonth, ReportGroupCode, BusinessLineCode);
                    break;
            }
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                var user_name = ds.Tables[0].Rows[0]["UpdateUserName"].ToString();
                var update_date = String.Format("{0:MMMM dd, yyyy}", ds.Tables[0].Rows[0]["ActionDate"]);
                lblMessage.Text = String.Format("Last updated by {0} on {1}.", user_name, update_date);
            }
        }
        
#region PrivateProperties

        private int DataEntryType
        {
            get { return (int)ViewState["DATA_TYPE"]; }
            set { ViewState["DATA_TYPE"] = value; }
        }

        private string FiscalYear
        {
            get { return (string)ViewState["FISCAL_YEAR"]; }
            set { ViewState["FISCAL_YEAR"] = value; }
        }

        private string BookMonth
        {
            get { return (string)ViewState["BOOK_MONTH"]; }
            set { ViewState["BOOK_MONTH"] = value; }
        }

        private string Organization
        {
            get { return (string)ViewState["ORG"]; }
            set { ViewState["ORG"] = value; }
        }

        private int ReportGroupCode
        {
            get { return (int)ViewState["GROUP_CD"]; }
            set { ViewState["GROUP_CD"] = value; }
        }

        private string BusinessLineCode
        {
            get { return (string)ViewState["BL_CODE"]; }
            set { ViewState["BL_CODE"] = value; }
        }

        private string AdjustmentsAdditional
        {
            get { return (string)ViewState["ADJ_ADD"]; }
            set { ViewState["ADJ_ADD"] = value; }
        }

        private int CurrentUserID
        {
            get 
            {
                if (ViewState["CURRENT_USER"] == null)
                    ViewState["CURRENT_USER"] = (new PageBase()).CurrentUserID;
                
                return (int)ViewState["CURRENT_USER"]; 
            }
        }

#endregion PrivateProperties

        static string[] Month = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        private void DrawTable()
        {
            decimal total_amount = 0;
            var read_only = true;
            if (User.IsInRole(((int)UserRoles.urFSBDAnalystFundsCoordinator).ToString()) && Organization != "")
                read_only = false;
            else if (!User.IsInRole(((int)UserRoles.urFSBDAnalystFundsCoordinator).ToString()))
            {
                var user_role = Users.UserAuthorizedForFSReports(CurrentUserID, BusinessLineCode, Organization);
                if (user_role <= 0)
                    throw new Exception("You are not authorized to visit this page.");
                else
                    if (user_role == (int)UserRoles.urFSOrgAdminWR && Organization != "")
                        read_only = false;
            }

            var _add_awards = (AdjustmentsAdditional == "award") ? true : false;
            var _add_training = (AdjustmentsAdditional == "training") ? true : false;
            var _add_travel = (AdjustmentsAdditional == "travel") ? true : false;

            //get the data:
            var ds = FSDataServices.GetUserEntryData(DataEntryType, FiscalYear, BookMonth, Organization, BusinessLineCode, ReportGroupCode, _add_awards, _add_training, _add_travel, false);

            HtmlTableRow tr = null;
            HtmlTableCell td = null;
            int entry_id;
            string doc_number;
            decimal amount;
            string explanation;
            string entry_month;

            //remove first column if there is no permission for edit or delete:
            if (read_only)
            {
                tblData.Rows[0].Cells[0].InnerText = "BookMonth";
            }
            //tblData.Rows[0].Cells[0].AddDisplayNone();                

            var iRowCount = 0;
            ds.Tables[0].Columns["Amount"].ColumnName = "$ Amount";
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                entry_id = (int)dr["EntryID"];
                entry_month = String.Format("{0:MMMM}", DateTime.Parse(dr["BookMonth"].ToString() + "/" + dr["FiscalYear"].ToString()));
                doc_number = (string)Utility.GetNotNullValue(dr["DocNumber"], "String");
                amount = (decimal)dr["$ Amount"];
                total_amount += amount;
                explanation = (string)Utility.GetNotNullValue(dr["Explanation"], "String");

                tr = new HtmlTableRow();
                tr.AddCssClass((iRowCount++) % 2 > 0 ? "tRowAlt" : "tRow");
                if (entry_id >= 0 && !read_only && dr["BookMonth"].ToString() == BookMonth)
                {
                    td = new HtmlTableCell();
                    var btnEdit = new HtmlInputImage();
                    btnEdit.ID = "edit_ctrl_" + entry_id.ToString();
                    btnEdit.Src = "../images/note.gif";
                    btnEdit.Alt = "Edit record";
                    btnEdit.AddOnClick("return edit_row(this," + entry_id.ToString() + "," + DataEntryType.ToString() + ");");
                    td.Controls.Add(btnEdit);
                    tr.Cells.Add(td);

                    td = new HtmlTableCell();
                    var btnDelete = new HtmlInputImage();
                    btnDelete.ID = "delete_ctrl_" + entry_id.ToString();
                    btnDelete.Src = "../images/btn_contact_delete.gif";
                    btnDelete.Alt = "Delete record";
                    btnDelete.AddOnClick("return delete_row(this," + entry_id.ToString() + "," + DataEntryType.ToString() + ");");
                    td.Controls.Add(btnDelete);
                    tr.Cells.Add(td);
                }
                else
                {
                    td = new HtmlTableCell();
                    td.ColSpan = 2;
                    td.InnerText = entry_month;
                    tr.Cells.Add(td);
                }

                td = new HtmlTableCell();
                if (entry_id < 0)
                {
                    td.InnerHtml = "<font color='green'>Show Details</font>";
                    td.AddOnClick("Expand(this)");
                    td.AddTitle("Expand");
                    td.Style.Add("cursor", "pointer");
                }
                else
                    td.InnerHtml = doc_number != "" ? doc_number : "&nbsp;";
                tr.Cells.Add(td);

                td = new HtmlTableCell();
                td.InnerText = amount == 0 ? "0" : String.Format("{0:0,0}", Convert.ToInt64(amount));
                td.Align = "right";
                tr.Cells.Add(td);

                td = new HtmlTableCell();
                td.InnerHtml = explanation != "" ? explanation : "&nbsp;";
                tr.Cells.Add(td);

                tblData.Rows.Add(tr);
                if (entry_id < 0)   //expanded table
                {
                    tr = new HtmlTableRow();
                    tr.Style.Add("display", "none");
                    td = new HtmlTableCell();
                    tr.Cells.Add(td);
                    td = new HtmlTableCell();
                    td.ColSpan = 4;

                    var mg = new MultiGrid();
                    mg = (MultiGrid)Page.LoadControl("..\\Controls\\MultiGrid.ascx");
                    td.Controls.Add(mg);
                    tr.Cells.Add(td);
                    tblData.Rows.Add(tr);

                    var dtO = FundStatus.GetUserEntryDataList(DataEntryType, FiscalYear, BookMonth, Organization, BusinessLineCode, ReportGroupCode, entry_id).Tables[0];
                    var dt = dtO.Clone();
                    dt.Columns["Amount"].DataType = typeof(string);
                    foreach (DataRow r in dtO.Rows)
                    {
                        dt.ImportRow(r);
                    }
                    dt.Columns["Amount"].ColumnName = "$ Amount";
                    foreach (DataRow r in dt.Rows)
                    {
                        r["Month"] = Month[Convert.ToInt32(r["Month"]) - 1];
                        r["$ Amount"] = String.Format("{0:0,0}", Convert.ToInt64(Convert.ToDecimal(r["$ Amount"])));
                        if (r["$ Amount"].ToString() == "00")
                            r["$ Amount"] = "0";
                    }
                    mg.Table = dt;
                    mg.TblCSSClass = "eTbl";
                    mg.TblBorderClass = "eBorder";
                    mg.HeaderCSSClass = "reportHeaderGreen";
                    mg.ItemCSSClass = "eRow";
                    mg.Height = Unit.Pixel(dt.Rows.Count * 18 + 19); 
                         //make it not scrollable by setting full height
                }
            }
            //add row for insert record - if applicable
            if (!read_only)
            {
                HtmlInputText txt;

                tr = new HtmlTableRow();
                tr.AddCssClass((iRowCount++) % 2 > 0 ? "tRowAlt" : "tRow");

                td = new HtmlTableCell();
                var btnCancel = new HtmlInputImage();
                btnCancel.ID = "cancel_ctrl_new";
                btnCancel.Src = "../images/back.gif";
                btnCancel.Alt = "Cancel changes";
                btnCancel.AddWidth(12);
                btnCancel.AddHeight(12);
                btnCancel.AddOnClick("return cancel_edit(this);");
                td.Controls.Add(btnCancel);
                tr.Cells.Add(td);

                td = new HtmlTableCell();
                var btnEdit = new HtmlInputImage();
                btnEdit.ID = "save_ctrl_new";
                btnEdit.Src = "../images/save.gif";
                btnEdit.Alt = "Save new record";
                btnEdit.AddWidth(12);
                btnEdit.AddHeight(12);
                btnEdit.AddOnClick("return save_row(this,0," + DataEntryType.ToString() + ");");
                td.Controls.Add(btnEdit);
                tr.Cells.Add(td);

                td = new HtmlTableCell();
                td.AddStyle("padding-left:5px;padding-right:5px");
                txt = new HtmlInputText();
                txt.AddStyle("width:100%;height:15px;text-align:center;");
                td.Controls.Add(txt);
                tr.Cells.Add(td);

                td = new HtmlTableCell();
                td.AddStyle("padding-left:5px;padding-right:5px");
                txt = new HtmlInputText();
                txt.AddStyle("width:100%;height:15px;text-align:center;");
                txt.AddOnMouseOut("extractNumber(this,0,true,9999999999);");
                txt.AddOnKeyUp("extractNumber(this,0,true,9999999999);");
                txt.AddKeyPressBlockNonNumbers();
                td.Controls.Add(txt);
                tr.Cells.Add(td);

                td = new HtmlTableCell();
                td.AddStyle("padding-left:5px;padding-right:5px");
                txt = new HtmlInputText();
                txt.AddStyle("width:100%;height:15px;text-align:center;");
                td.Controls.Add(txt);
                tr.Cells.Add(td);

                tblData.Rows.Add(tr);
            }
            lblAmount.Text = total_amount == 0 ? "0" : String.Format("{0:$0,0}", Convert.ToInt64(total_amount));
        }
    }
}