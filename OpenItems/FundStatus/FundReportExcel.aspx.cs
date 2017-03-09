using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web.UI;
    using System.Configuration;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using Data;

    public partial class FundReportExcel : PageBase
    {

        public int p_iHeaderLines = 0;		//will be inserted to Excel.xml,
        public string s_PageBrakes = "";	//which included into sheet

        public Label lblMsg;
        public HtmlTable Sheet;

        const string HT_KEY_BL = "BL";
        const string HT_KEY_BL_NAME = "BLName";
        const string HT_KEY_ORG = "Org";
        const string HT_KEY_YEAR = "Year";
        const string HT_KEY_BOOK_MONTH = "BookMonth";
        const string HT_KEY_BA = "BA";
        const string HT_KEY_VIEW = "View";
        const string HT_KEY_GROUP_CD = "Group";
        const string HT_KEY_SUM_FUNC = "SumFunc";
        const string HT_KEY_OC = "OC";
        const string HT_KEY_CE = "CE";
        const string HT_KEY_DOC_NUM = "DocNum";

        const string HT_KEY_FILTER = "SUM_FUNC_FILTER";
        const string HT_KEY_FILTER_SUM_FUNC = "REPORT_SUM_FUNCTION";
        const string HT_KEY_FILTER_OCCODE = "REPORT_OC_CODE";

        const string CELL_ALIGN_LEFT = "left";
        const string CELL_ALIGN_RIGHT = "right";
        const string CELL_ALIGN_MIDDLE = "middle";

        FSSummaryUI draw;

        private readonly FundAllowanceBO FundAllowance;
        private readonly FundStatusBO FundStatus;
        private readonly UsersBO Users;
        public FundReportExcel()
        {
            FundAllowance = new FundAllowanceBO(this.Dal);
            FundStatus = new FundStatusBO(this.Dal);
            Users = new UsersBO(this.Dal, new EmailsBO(this.Dal));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                lblMsg = (Label)Master.FindControl("lblMsg");
                Sheet = (HtmlTable)Master.FindControl("Sheet");
                if (!IsPostBack)
                {
                    var type = Request.QueryString["type"];


                    //take the selected values from Session variable - Hashtable according Screen Type.

                    var int_screen_type = Int32.Parse(type);

                    switch (int_screen_type)
                    {
                        case (int)FundsStatusScreenType.stFundsReview:
                            BuildFundsReviewTable();
                            break;

                        case (int)FundsStatusScreenType.stFundsSearch:
                            BuildFundsSearchTable();
                            break;

                        case (int)FundsStatusScreenType.stFundStatusReport:
                            BuildStatusReportTable();
                            break;

                        //case (int)FundsStatusScreenType.stFundsAllowance:
                        //    break;

                        case (int)FundsStatusScreenType.stFundSummaryReport:
                            BuildSummaryProjectionReportTable();
                            break;

                        case (int)FundsStatusScreenType.stAllowance:
                            BuildAllowanceReportTable();
                            break;

                        case (int)FundsStatusScreenType.stOrgAllowance:
                            BuildOrgAllowanceReportTable();
                            break;

                        case (int)FundsStatusScreenType.stFundsEntryData:
                            FundsEntryDataReport();
                            break;
                    }
                }

                FinalizeOutput();
            }
            catch (Exception ex)
            {
                lblMsg.Text = ex.Message;
            }
        }

        static string[] Month = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        private void FundsEntryDataReport()
        {
            var Organization = Request.QueryString["org"];
            var FiscalYear = Request.QueryString["fy"];
            var BookMonth = Request.QueryString["bm"];
            var ReportGroupCode = Int32.Parse(Request.QueryString["gcd"]);
            var BusinessLineCode = Request.QueryString["bl"];
            var AdjustmentsAdditional = Request.QueryString["adj"];

            //check if user's role allows to get this report:
            if (!(User.IsInRole(((int)UserRoles.urFSBDAnalystFundsCoordinator).ToString()) || User.IsInRole(((int)UserRoles.urFSBDPowerReader).ToString()) ||
                    User.IsInRole(((int)UserRoles.urFSOrgAdminWR).ToString()) || User.IsInRole(((int)UserRoles.urFSOrgAdminRO).ToString())))
            {
                if (Users.UserAuthorizedForFSReports((new PageBase()).CurrentUserID, BusinessLineCode, Organization) <= 0)
                    throw new Exception("You are not authorized to review this data.");
            }

            HtmlTableRow tr;
            HtmlTableCell td;
            var DataEntryType = 0;
            if (!Int32.TryParse(Request.QueryString["t"], out DataEntryType))
                throw new Exception("Error. Missing parameters.");
            var sTitle = "";
            switch (DataEntryType)
            {
                case (int)FundsStatusUserEntryType.ueOneTimeAdjustment:
                    sTitle = "One Time Adjustments";
                    break;
                case (int)FundsStatusUserEntryType.ueOverUnderAccrued:
                    sTitle = "Over/Under Accrued";
                    break;
                case (int)FundsStatusUserEntryType.ueExpectedByYearEnd:
                    sTitle = "Expected by Year End";
                    break;
                default:
                    throw new Exception("Error. Missing parameters.");
            }
            tr = new HtmlTableRow();
            tr.Cells.AddCellLeft(sTitle, "title", 9);
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            tr.Cells.AddCell("", "", 9);
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            tr.Cells.AddCell(Organization, "title2", 2);
            tr.Cells.AddCell(String.Format("{0:MMMM}", DateTime.Parse(BookMonth + "/" + FiscalYear)), "title2", 2);
            tr.Cells.AddCellLeft("Fiscal Year: " + FiscalYear, "title2", 5);
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            //td = null;
            var dst = FSDataServices.GetFSReportConfiguration();
            if (dst != null && dst.Tables[0] != null && dst.Tables[0].Rows.Count > 0)
            {
                var dr_col = dst.Tables[0].Select("GROUP_CD=" + ReportGroupCode.ToString());
                if (dr_col.Length > 0)
                    tr.Cells.AddCell(dr_col[0]["Name"].ToString(), "title2", 2);
                else
                    tr.Cells.AddCell("", "title2", 2);
            }
            else
                tr.Cells.AddCell("", "title2", 2);
            //tr.Cells.Add(td);
            tr.Cells.AddCell("", "title2", 2);
            tr.Cells.AddCell("", "title2", 5);
            Sheet.Rows.Add(tr);

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
                default:
                    throw new Exception("Error. Missing parameters.");
            }
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                var user_name = ds.Tables[0].Rows[0]["UpdateUserName"].ToString();
                var update_date = String.Format("{0:MMMM dd, yyyy}", ds.Tables[0].Rows[0]["ActionDate"]);
                tr = new HtmlTableRow();
                tr.Cells.AddCell(String.Format("Last updated by {0} on {1}.", user_name, update_date), "reportCaption", 9);
                Sheet.Rows.Add(tr);
            }

            decimal total_amount = 0;
            var _add_awards = (AdjustmentsAdditional == "award") ? true : false;
            var _add_training = (AdjustmentsAdditional == "training") ? true : false;
            var _add_travel = (AdjustmentsAdditional == "travel") ? true : false;
            int entry_id;
            string doc_number;
            decimal amount;
            string explanation;
            string entry_month;

            tr = new HtmlTableRow();
            tr.Cells.AddCell("BookMonth", "reportHeaderBlue", 2);
            tr.Cells.AddCell("DocNumber", "reportHeaderBlue", 2);
            tr.Cells.AddCell("$ Amount", "reportHeaderBlue", 1);
            tr.Cells.AddCell("Explanation", "reportHeaderBlue", 4);
            Sheet.Rows.Add(tr);

            ds = FSDataServices.GetUserEntryData(DataEntryType, FiscalYear, BookMonth, Organization, BusinessLineCode, ReportGroupCode, _add_awards, _add_training, _add_travel, false);
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
                tr.Cells.AddCell(entry_month, "extRow", 2);
                tr.Cells.AddCell(doc_number, "extRow", 2);
                tr.Cells.AddCell((Convert.ToInt64(amount)).ToString(), "ExtNumber", 1);
                tr.Cells.AddCell(explanation, "extRow", 4);
                Sheet.Rows.Add(tr);
                if (entry_id < 0)   //expanded table
                {
                    var dt = FundStatus.GetUserEntryDataList(DataEntryType, FiscalYear, BookMonth, Organization, BusinessLineCode, ReportGroupCode, entry_id).Tables[0];
                    dt.Columns["Amount"].ColumnName = "$ Amount";
                    if (dt == null || dt.Columns.Count == 0)
                        continue;
                    tr = new HtmlTableRow();
                    tr.Cells.AddCell("", "", 1);
                    for (var i = 0; i < dt.Columns.Count; i++)
                    {
                        tr.Cells.AddCell(dt.Columns[i].ToString(), "reportHeaderGreen", 1);
                    }
                    Sheet.Rows.Add(tr);
                    foreach (DataRow r in dt.Rows)
                    {
                        tr = new HtmlTableRow();
                        tr.Cells.AddCell("", "", 1);
                        r["Month"] = Month[Convert.ToInt32(r["Month"]) - 1];
                        r["$ Amount"] = String.Format("{0:0,0}", Convert.ToInt64(Convert.ToDecimal(r["$ Amount"])));
                        if (r["$ Amount"].ToString() == "00")
                            r["$ Amount"] = "0";
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            if (dt.Columns[i].ColumnName == "$ Amount")
                                tr.Cells.AddCell((Convert.ToInt64(r["$ Amount"])).ToString(), "IntNumber", "");
                            else
                                tr.Cells.AddCell(r[i].ToString(), "intRow", 1);
                        }
                        Sheet.Rows.Add(tr);
                    }
                }
            }
            Sheet.Rows[3].Cells[2].InnerText = "Amount: " + (total_amount == 0 ? "0" : String.Format("{0:$0,0}", Convert.ToInt64(total_amount)));
            p_iHeaderLines = 5;
        }

        private void BuildAllowanceReportTable()
        {
            //check if user's role allows to get this report:            
            if (!(User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString()) || User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceRO).ToString()) ||
                    User.IsInRole(((int)UserRoles.urFSBDPowerReader).ToString())))
                throw new Exception("You are not authorized to review this data.");

            var FY = Request.QueryString["FY"];
            var dt = FundAllowance.GetDetailedAllowanceForFiscalYear(FY).ToDataSet().Tables[0];

            /*
            FSAllowanceUI objUI = new FSAllowanceUI();
            objUI.DBTable = dt;
            objUI.ChartTable = Sheet;
            objUI.DrawTableNCR(true);
            Sheet = objUI.ChartTable;
            */

            var tr = new HtmlTableRow();
            HtmlTableCell td;
            if (dt.Rows.Count == 0)
                td = CreateCell("There is no allowed amount for fiscal year " + FY, "title2", CELL_ALIGN_LEFT);
            else
                td = CreateCell("Allowance amount for fiscal year " + FY, "title2", CELL_ALIGN_LEFT);
            td.ColSpan = 15;
            tr.Cells.Add(td);
            Sheet.Rows.Insert(0, tr);
            tr = new HtmlTableRow();
            td = CreateCell("", "", CELL_ALIGN_LEFT);
            td.ColSpan = 15;
            tr.Cells.Add(td);
            Sheet.Rows.Insert(1, tr);
            p_iHeaderLines = 3;
        }

        private void BuildOrgAllowanceReportTable()
        {
            //check if user's role allows to get this report:            
            if (!(User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString()) || User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceRO).ToString()) ||
                    User.IsInRole(((int)UserRoles.urFSBDPowerReader).ToString())))
                throw new Exception("You are not authorized to review this data.");

            var FY = Request.QueryString["FY"];
            var Org = Request.QueryString["org"];
            var DistributionType = (int)FundAllowanceViewType.vtAllowanceTotal;
            if (Org != null && Org != "")
            {
                DistributionType = (int)FundAllowanceViewType.vtAllowanceForOrganization;
                if (Org == "_")
                    Org = Session["ORG_list"].ToString().Replace(",", ", ");
            }
            /*
            FSAllowanceUI objUI = new FSAllowanceUI();
            objUI.ChartTable = Sheet;
            objUI.DistributionViewType = DistributionType;
            objUI.FiscalYear = FY;
            objUI.Organization = Org;
            objUI.DrawDistributionTable(true);
            Sheet = objUI.ChartTable;
            */

            var tr = new HtmlTableRow();
            HtmlTableCell td;
            if (Org != null && Org != "")
                td = CreateCell("Allowance Distribution by Function Code for fiscal year " + FY + " for " + Org, "title2", CELL_ALIGN_LEFT);
            else
                td = CreateCell("Fund Allowance Over NCR Organizations for fiscal year " + FY, "title2", CELL_ALIGN_LEFT);
            td.ColSpan = 26;
            tr.Cells.Add(td);
            Sheet.Rows.Insert(0, tr);
            tr = new HtmlTableRow();
            td = CreateCell("", "", CELL_ALIGN_LEFT);
            td.ColSpan = 26;
            tr.Cells.Add(td);
            Sheet.Rows.Insert(1, tr);
            p_iHeaderLines = 3;
        }

        private void BuildFundsSearchTable()
        {
            //get Query parameters:
            var ht = (new PageBase()).FundsSearchSelectedValues;
            var business_line = (string)ht[HT_KEY_BL];
            var organization = (string)ht[HT_KEY_ORG];
            var fiscal_year = (string)ht[HT_KEY_YEAR];
            var BudgetActivity = (string)ht[HT_KEY_BA];
            var book_month = (string)ht[HT_KEY_BOOK_MONTH];
            var group_cd = (string)ht[HT_KEY_GROUP_CD];
            var summary_function = (string)ht[HT_KEY_SUM_FUNC];
            var oc_code = (string)ht[HT_KEY_OC];
            var CostElem = (string)ht[HT_KEY_CE];
            var doc_number = (string)ht[HT_KEY_DOC_NUM];
            var obl_income_view = Int32.Parse((string)ht[HT_KEY_VIEW]);
            var title_text = "";
            switch (obl_income_view)
            {
                case (int)FundsReviewViewMode.fvObligations:
                    title_text = "Obligations";
                    break;
                case (int)FundsReviewViewMode.fvIncome:
                    title_text = "Income";
                    break;
                case (int)FundsReviewViewMode.fvOneTimeAdjustments:
                    title_text = "One Time Adjustments";
                    break;
            }

            //start Excel headers:
            var tr = new HtmlTableRow();
            tr.Cells.AddCellLeft("Funds Search Results", "title", 14);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCellLeft(title_text, "title2", 14);
            Sheet.Rows.Add(tr);

            //display query parameters in Excel:
            if (business_line != "")
            {
                var ds_t = FundStatus.GetBusinessLineList().ToDataSet();
                if (ds_t != null && ds_t.Tables[0] != null && ds_t.Tables[0].Rows.Count > 0)
                {
                    var dr_col = ds_t.Tables[0].Select("BL_CD='" + business_line + "'");
                    if (dr_col.Length > 0)
                    {
                        var bl_name = dr_col[0]["BLDesc"].ToString();
                        tr = new HtmlTableRow();
                        tr.Cells.AddCellLeft("Business Line:", "reportCaption2", 2);
                        tr.Cells.AddCellLeft(bl_name, "title2", 12);
                        Sheet.Rows.Add(tr);
                    }
                }
            }
            tr = new HtmlTableRow();
            tr.Cells.AddCellLeft("Organization:", "reportCaption2", 2);
            tr.Cells.AddCellLeft(organization, "title2", 12);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCellLeft("Fiscal Year:", "reportCaption2", 2);
            tr.Cells.AddCellLeft(fiscal_year, "title2", 12);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCellLeft("Book Month:", "reportCaption2", 2);
            if (book_month == "")
                tr.Cells.AddCellLeft("All Available", "title2", 12);
            else
            {
                var s = "";
                var ss = book_month.Split(new char[] { ',' });
                foreach (var m in ss)
                {
                    s += String.Format("{0:MMMM}", DateTime.Parse(m + "/" + fiscal_year)) + ", ";
                }
                s = s.Substring(0, s.Length - 2);
                tr.Cells.AddCellLeft(s, "title2", 12);
            }
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCellLeft("Budget Activity:", "reportCaption2", 2);
            tr.Cells.AddCellLeft(BudgetActivity, "title2", 12);
            Sheet.Rows.Add(tr);

            if (group_cd != null && group_cd != "")
            {
                var ds_g = FundStatus.GetReportFunctionGroupList();
                if (ds_g != null && ds_g.Tables[0] != null && ds_g.Tables[0].Rows.Count > 0)
                {
                    var dr_col = ds_g.Tables[0].Select("GROUP_CD=" + group_cd);
                    if (dr_col.Length > 0)
                    {
                        var group_name = dr_col[0]["Name"].ToString();
                        tr = new HtmlTableRow();
                        tr.Cells.AddCellLeft("Filter by Function Report Group:", "reportCaption2", 2);
                        tr.Cells.AddCellLeft(group_name, "tableBold", 12);
                        Sheet.Rows.Add(tr);
                    }
                }
            }
            if (summary_function != null && summary_function != "")
            {
                tr = new HtmlTableRow();
                tr.Cells.AddCellLeft("Filter by Functions:", "reportCaption2", 2);
                tr.Cells.AddCellLeft(summary_function, "tableBold", 12);
                Sheet.Rows.Add(tr);
            }
            if (oc_code != null && oc_code != "")
            {
                tr = new HtmlTableRow();
                tr.Cells.AddCellLeft("Filter by OC Code:", "reportCaption2", 2);
                tr.Cells.AddCellLeft(oc_code, "tableBold", 12);
                Sheet.Rows.Add(tr);
            }
            if (CostElem != null && CostElem != "")
            {
                tr = new HtmlTableRow();
                tr.Cells.AddCellLeft("Filter by Cost Elements:", "reportCaption2", 2);
                tr.Cells.AddCellLeft(CostElem, "tableBold", 12);
                Sheet.Rows.Add(tr);
            }
            if (doc_number != "")
            {
                tr = new HtmlTableRow();
                tr.Cells.AddCellLeft("DocNumber like:", "reportCaption2", 2);
                tr.Cells.AddCellLeft(doc_number, "tableBold", 12);
                Sheet.Rows.Add(tr);
            }

            tr = new HtmlTableRow();
            tr.Cells.AddCell("", "", 14);
            Sheet.Rows.Add(tr);
            p_iHeaderLines = Sheet.Rows.Count + 1;

            //get the data:
            var records = 0;
            decimal total_amount = 0;
            object obj_org_code = (organization != "") ? organization : null;
            object obj_book_month = (book_month != "") ? book_month : null;
            object obj_sum_func = (summary_function != "") ? summary_function : null;
            object obj_oc_code = (oc_code != "") ? oc_code : null;
            object obj_CE = (CostElem != "") ? CostElem : null;
            object obj_doc_num = (doc_number != "") ? doc_number : null;
            object obj_group_cd = (group_cd != "") ? group_cd : null;

            var ds = FundStatus.GetSearchResults(obl_income_view, fiscal_year, BudgetActivity, out records, out total_amount,
                obj_org_code, obj_book_month, obj_group_cd, obj_sum_func, obj_oc_code, obj_CE, obj_doc_num, Settings.Default.QueryResultMaxRecords, false);

            if (ds == null || records == 0)
            {
                //no records:
                tr = new HtmlTableRow();
                tr.Cells.AddCellLeft("No records found", "reportCaption", 14);
                Sheet.Rows.Add(tr);
                p_iHeaderLines = 0;
            }
            else
            {
                var dt = ds.Tables[0].Clone();
                dt.Columns["Amount"].DataType = typeof(int);
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    dt.ImportRow(r);
                }
                dt.Columns["Amount"].ColumnName = "$ Amount";
                foreach (DataRow r in dt.Rows)
                {
                    if (r["book month"].ToString() != "")
                        r["book month"] = String.Format("{0:MMMM}", DateTime.Parse(r["book month"].ToString() + "/2000"));
                    r["$ Amount"] = Convert.ToInt32(r["$ Amount"]);
                }
                BuildGeneralExcelOutput(dt);
            }
        }

        private void BuildFundsReviewTable()
        {

            //ctrlCriteria.ExpandedByMonthView - always Expanded view in Excel:
            var expanded_view = true;

            //get Query parameters:
            /************************************/
            var ht = (new PageBase()).FundsReviewSelectedValues;
            var organization = (string)ht[HT_KEY_ORG];
            var business_line_code = (string)ht[HT_KEY_BL];
            var business_line = (string)ht[HT_KEY_BL_NAME];
            var fiscal_year = (string)ht[HT_KEY_YEAR];
            var book_month = (string)ht[HT_KEY_BOOK_MONTH];
            var obl_income_view = Int32.Parse((string)ht[HT_KEY_VIEW]);
            var title_text = "";
            switch (obl_income_view)
            {
                case (int)FundsReviewViewMode.fvObligations:
                    title_text = "Obligations";
                    break;
                case (int)FundsReviewViewMode.fvIncome:
                    title_text = "Income";
                    break;
                case (int)FundsReviewViewMode.fvOneTimeAdjustments:
                    title_text = "One Time Adjustments";
                    break;
            }

            //start Excel headers:
            /************************************/
            var tr = new HtmlTableRow();
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            tr.Cells.AddCell("Funds Review", "title", CELL_ALIGN_LEFT);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCell(title_text, "title2", CELL_ALIGN_LEFT);
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            Sheet.Rows.Add(tr);

            //get the data:
            /************************************/
            var last_book_month_included = (book_month == "00") ? "09" : book_month;
            var dsTotal = FSTotalsReport.GetSummaryState(fiscal_year, organization, business_line_code, last_book_month_included, obl_income_view, false);
            var ds = FSTotalsReport.GetSummaryState(fiscal_year, organization, business_line_code, last_book_month_included, obl_income_view, true);

            //display query parameters in Excel:
            /************************************/
            if (business_line_code != "")
            {
                tr = new HtmlTableRow();
                tr.Cells.AddCell("Business Line:", "reportCaption2", CELL_ALIGN_LEFT);
                tr.Cells.AddCell(business_line, "title2", CELL_ALIGN_RIGHT);
                Sheet.Rows.Add(tr);
            }
            tr = new HtmlTableRow();
            tr.Cells.AddCell("Organization:", "reportCaption2", CELL_ALIGN_LEFT);
            tr.Cells.AddCell(organization, "title2", CELL_ALIGN_RIGHT);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCell("Fiscal Year:", "reportCaption2", CELL_ALIGN_LEFT);
            tr.Cells.AddCell(fiscal_year, "title2", CELL_ALIGN_RIGHT);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCell("Book Month:", "reportCaption2", CELL_ALIGN_LEFT);
            if (book_month == "00")
                tr.Cells.AddCell("All Available", "title2", CELL_ALIGN_RIGHT);
            else
                tr.Cells.AddCell(String.Format("{0:MMMM}", DateTime.Parse(book_month + "/" + fiscal_year)), "title2", CELL_ALIGN_RIGHT);
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            Sheet.Rows.Add(tr);

            if (dsTotal == null || dsTotal.Tables[0].Rows.Count == 0)
            {
                //no records:
                /************************************/
                tr = new HtmlTableRow();
                tr.Cells.AddCell("No records found", "reportCaption2", CELL_ALIGN_LEFT);
                Sheet.Rows.Add(tr);
            }
            else
            {
                //draw the table Sheet that will be displayed in Excel:
                /******************************************************/
                var drawing_class = new FSReviewUI();
                drawing_class.TotalsDataTable = dsTotal.Tables[0];
                drawing_class.SourceDataTable = (ds == null) ? null : ds.Tables[0];
                drawing_class.TableToDraw = Sheet;
                drawing_class.MonthlyView = true;
                if (obl_income_view == (int)FundsReviewViewMode.fvIncome)
                    drawing_class.DisplayColumnObjClassCode = false;
                drawing_class.BookMonth = book_month;
                drawing_class.BuildReportForExcel = true;
                drawing_class.BuildTheTable();
                Sheet = drawing_class.TableToDraw;
            }
        }

        private void BuildSummaryProjectionReportTable()
        {
            //start Excel headers:
            /************************************/
            var tr = new HtmlTableRow();
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            tr.Cells.AddCell("Fund Summary Projection Report", "title", CELL_ALIGN_LEFT);
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            Sheet.Rows.Add(tr);

            //get Query parameters:
            /************************************/
            var ht = (new PageBase()).FundsSummaryReportSelectedValues;
            var organization = (string)ht[HT_KEY_ORG];
            var fiscal_year = (string)ht[HT_KEY_YEAR];
            var business_line = (string)ht[HT_KEY_BL_NAME];
            var business_line_id = (string)ht[HT_KEY_BL];

            //check if user's role allows to get this report:
            if (!(User.IsInRole(((int)UserRoles.urFSBDPowerReader).ToString()) || User.IsInRole(((int)UserRoles.urFSBDAnalystFundsCoordinator).ToString()) ||
                    User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceRO).ToString()) || User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString())))
            {
                if (Users.UserAuthorizedForFSReports((new PageBase()).CurrentUserID, business_line_id, organization) <= 0)
                    throw new Exception("You are not authorized to review this data.");
            }

            //display query parameters in Excel:
            /************************************/
            tr = new HtmlTableRow();
            tr.Cells.AddCell("Business Line:", "reportCaption2", CELL_ALIGN_LEFT);
            tr.Cells.AddCell(business_line, "title2", CELL_ALIGN_RIGHT);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCell("Organization:", "reportCaption2", CELL_ALIGN_LEFT);
            tr.Cells.AddCell(organization, "title2", CELL_ALIGN_RIGHT);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCell("Fiscal Year:", "reportCaption2", CELL_ALIGN_LEFT);
            tr.Cells.AddCell(fiscal_year, "title2", CELL_ALIGN_RIGHT);
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            Sheet.Rows.Add(tr);

            //draw the table Sheet that will be displayed in Excel:                                               
            draw = new FSSummaryUI();
            draw.FiscalYear = fiscal_year;
            draw.Organization = organization;
            draw.BusinessLine = business_line_id;

            CreateReportView((int)FundStatusSummaryReportView.svObligations, "Obligations", "Fund Status Summary Report - Obligations Info");
            CreateReportView((int)FundStatusSummaryReportView.svObligationsMonthly, "Obligations - Monthly View", "Fund Status Summary Report - Obligations Monthly Amounts Info");
            CreateReportView((int)FundStatusSummaryReportView.svIncome, "Income", "Fund Status Summary Report - Income Info");
            CreateReportView((int)FundStatusSummaryReportView.svIncomeMonthly, "Income - Monthly View", "Fund Status Summary Report - Income Monthly Amounts Info");
            CreateReportView((int)FundStatusSummaryReportView.svAllowance, "Allowance", "Fund Status Summary Report - Allowance Info");
            CreateReportView((int)FundStatusSummaryReportView.svYearEndProjection, "Year End Projection", "Fund Status Summary Report - Year End Projection Info");
            CreateReportView((int)FundStatusSummaryReportView.svProjectionVariance, "Projection Variance", "Fund Status Summary Report - Projection Variance Info");
            CreateReportView((int)FundStatusSummaryReportView.svProjectedBalance, "Projected Balance", "Fund Status Summary Report - Projected Balance Info");
        }

        private void CreateReportView(int ReportViewType, string ReportHeader, string LabelText)
        {
            draw.SummaryReportViewType = ReportViewType;

            var tr = new HtmlTableRow();
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCell(LabelText, "reportCaption2", CELL_ALIGN_LEFT);
            Sheet.Rows.Add(tr);

            draw.TableToDraw = Sheet;
            draw.ReportHeader = ReportHeader;
            draw.BuildHTMLTable();
            Sheet = draw.TableToDraw;
        }

        private void BuildStatusReportTable()
        {
            //start Excel headers:
            /************************************/
            var tr = new HtmlTableRow();
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            tr.Cells.AddCell("Fund Status Report", "title", CELL_ALIGN_LEFT);
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            Sheet.Rows.Add(tr);

            //get Query parameters:
            /************************************/
            var ht = (new PageBase()).FundsStatusSelectedValues;
            var organization = (string)ht[HT_KEY_ORG];
            var fiscal_year = (string)ht[HT_KEY_YEAR];
            var book_month = (string)ht[HT_KEY_BOOK_MONTH];
            var business_line = (string)ht[HT_KEY_BL_NAME];
            var business_line_id = (string)ht[HT_KEY_BL];

            //check if user's role allows to get this report:
            if (!(User.IsInRole(((int)UserRoles.urFSBDPowerReader).ToString()) || User.IsInRole(((int)UserRoles.urFSBDAnalystFundsCoordinator).ToString()) ||
                    User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceRO).ToString()) || User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString())))
            {
                if (Users.UserAuthorizedForFSReports((new PageBase()).CurrentUserID, business_line_id, organization) <= 0)
                    throw new Exception("You are not authorized to review this data.");
            }

            //display query parameters in Excel:
            /************************************/
            tr = new HtmlTableRow();
            tr.Cells.AddCell("Business Line:", "reportCaption2", CELL_ALIGN_LEFT);
            tr.Cells.AddCell(business_line, "title2", CELL_ALIGN_RIGHT);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCell("Organization:", "reportCaption2", CELL_ALIGN_LEFT);
            tr.Cells.AddCell(organization, "title2", CELL_ALIGN_RIGHT);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCell("Fiscal Year:", "reportCaption2", CELL_ALIGN_LEFT);
            tr.Cells.AddCell(fiscal_year, "title2", CELL_ALIGN_RIGHT);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCell("Book Month:", "reportCaption2", CELL_ALIGN_LEFT);
            tr.Cells.AddCell(String.Format("{0:MMMM}", DateTime.Parse(book_month + "/" + fiscal_year)), "title2", CELL_ALIGN_RIGHT);
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            Sheet.Rows.Add(tr);

            //draw the table Sheet that will be displayed in Excel:
            var drawing_class = new FSReportUI();
            drawing_class.FiscalYear = fiscal_year;
            drawing_class.BookMonth = book_month;
            drawing_class.BusinessLine = business_line_id;
            drawing_class.Organization = organization;
            drawing_class.TableToDraw = Sheet;
            drawing_class.BuildReportForExcel = true;
            drawing_class.BuildHTMLTable();
            Sheet = drawing_class.TableToDraw;
        }

        private HtmlTableCell CreateCellLeft(string InnerText, string CssClass, int cSpan)
        {
            var HCell = new HtmlTableCell();
            HCell.InnerText = InnerText;
            if (CssClass != "")
                HCell.Attributes["class"] = CssClass;
            HCell.Align = "left";
            if (cSpan > 1)
                HCell.ColSpan = cSpan;
            return HCell;
        }

        private HtmlTableCell CreateCell(string InnerText, string CssClass, int cSpan)
        {
            var HCell = new HtmlTableCell();
            HCell.InnerText = InnerText;
            if (CssClass != "")
                HCell.Attributes["class"] = CssClass;
            HCell.Align = "center";
            if (cSpan > 1)
                HCell.ColSpan = cSpan;
            return HCell;
        }

        private HtmlTableCell CreateCell(string InnerText, string CssClass, string CellAlign)
        {
            var objCell = new HtmlTableCell();
            objCell.InnerText = InnerText;
            objCell.Attributes["class"] = CssClass;
            if (CellAlign != "")
                objCell.Align = CellAlign;
            return objCell;
        }

        private void BuildGeneralExcelOutput(DataTable dt)
        {
            HtmlTableCell objCell;
            var objRow = new HtmlTableRow();            //add header first

            for (var i = 0; i < dt.Columns.Count; i++)
                objRow.Cells.AddCell(dt.Columns[i].ToString(), "tableCaption", "");

            Sheet.Rows.Add(objRow);

            var ExcelLine = 1;
            foreach (DataRow row in dt.Rows)
            {
                objRow = new HtmlTableRow();

                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    objCell = new HtmlTableCell();
                    //objCell.Style.Add("background-color", ExcelLine % 2 > 0 ? "#CCFFCC" : "#CCFFFF");
                    /*
                    if (i == (dt.Columns.Count - 1) && dt.Columns[i].DataType.Name == "String")
                    {
                        //only for the last column (usually it will include 'Comments'):
                        //check if the length of string more than 250 char - devide into 2 columns:
                        string str_comm = row[dt.Columns[i].ToString()].ToString();
                        if (str_comm.Length > 250)
                        {
                            objCell.InnerText = str_comm.Substring(0, 250);
                            objCell.Attributes["class"] = "textLeft";
                            objRow.Cells.Add(objCell);

                            objCell = new HtmlTableCell();
                            objCell.InnerText = str_comm.Substring(250);
                        }
                        else
                            objCell.InnerText = str_comm;
                    }
                    else
                     */

                    objCell.InnerText = row[dt.Columns[i].ToString()].ToString();

                    switch (dt.Columns[i].DataType.Name)
                    {
                        case "Int32":
                            objCell.Attributes["class"] = "WholeNumber";
                            break;
                        case "DateTime":
                            objCell.Attributes["class"] = "DateDigits";
                            break;
                        case "Decimal":
                            var col = dt.Columns[i].ColumnName;
                            if (col == "TotalLine" || col == "UDO" || col == "UDOShouldBe" ||
                                col == "DO" || col == "DOShouldBe" || col == "COM" || col == "ACCR" ||
                                col == "PENDPYMT" || col == "TotalOrder" || col == "PYMTS_TRAN" || col == "PYMTS_CONF" ||
                                col == "HOLDBACKS")
                                objCell.Attributes["class"] = "Money";
                            else
                                objCell.Attributes["class"] = "Number";
                            break;
                        case "String":
                        default:
                            objCell.Attributes["class"] = "textLeft";
                            break;
                    }
                    objRow.Cells.Add(objCell);
                }
                ExcelLine++;
                Sheet.Rows.Add(objRow);
            }
        }

        private void FinalizeOutput()
        {

            //Translate Form in HTML format and send it to client Excel
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";

            Response.ContentType = "application/vnd.ms-excel";
            this.EnableViewState = false;
            var Stringer = new System.IO.StringWriter();
            var Htmler = new HtmlTextWriter(Stringer);
            this.RenderControl(Htmler);
            var s = Stringer.ToString();
            Response.Write(s);

            /*
            //save file on the server for archiving if needed 
            string FILE_NAME = this.Page.Request.PhysicalApplicationPath + "Excel.xls";
            if (File.Exists(FILE_NAME))
                File.Delete(FILE_NAME);
            StreamWriter sw = File.CreateText(FILE_NAME);
            sw.Write(s);
            sw.Close();
            */
            Response.End();
        }

    }

}