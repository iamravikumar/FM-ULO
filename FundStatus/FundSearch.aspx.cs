namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Configuration;
    using Data;

    public partial class FundSearch : PageBase
    {
        private readonly FundStatusBO FundStatus;
        public FundSearch()
        {
            FundStatus = new FundStatusBO(this.Dal);
        }
        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {
                if (!Page.ClientScript.IsClientScriptBlockRegistered(Page.GetType(), "JS"))
                    Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "JS",
                        "<script language='javascript' src='include/FundSearch.js'></script>");

                if (!IsPostBack)
                {
                    btns.Visible = mgRezult.Visible = false;
                    SearchCriteria.FillControls();

                    if (Request.QueryString["vm"] != null)
                    {
                        SearchCriteria.ViewMode = Convert.ToInt32(Request.QueryString["vm"]);
                        if (Request.QueryString["bl"] != null)
                            SearchCriteria.BusinessLine = Request.QueryString["bl"];
                        if (Request.QueryString["org"] != null)
                        {
                            if (!(SearchCriteria.BusinessLine != "" && Request.QueryString["org"] == ""))
                                SearchCriteria.Organization = Request.QueryString["org"];
                        }
                        if (Request.QueryString["fy"] != null)
                            SearchCriteria.FiscalYear = Request.QueryString["fy"];
                        if (Request.QueryString["ba"] != null)
                            SearchCriteria.BudgetActivity = Request.QueryString["ba"];
                        if (Request.QueryString["bm"] != null)
                            SearchCriteria.BookMonth = Request.QueryString["bm"];
                        if (Request.QueryString["gcd"] != null)
                            SearchCriteria.GroupCD = Request.QueryString["gcd"];
                        if (Request.QueryString["sf"] != null)
                            SearchCriteria.SummaryFunction = Request.QueryString["sf"];
                        if (Request.QueryString["oc"] != null)
                            SearchCriteria.ObjClassCode = Request.QueryString["oc"];

                        FundsSearchSelectedValues = SearchCriteria.Criteria;
                        //btnBack.Visible = true;
                        ShowResults();
                    }
                }
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }

        protected void Page_Init(object sender, System.EventArgs e)
        {
            SearchCriteria.Submit += new EventHandler(ctrlCriteria_Submit);
        }

        private string GetOrganizationsByBL(string BusinessLine)
        {
            var ds = FundStatus.GetOrganizationList();
            var org = "";
            var dr_col = ds.Tables[0].Select(String.Format("BL_CD='{0}'", BusinessLine));
            foreach (var dr in dr_col)
                org += dr["ORG_CD"].ToString() + ",";

            return org;
        }

        private string ClearDoubles(string SourceStrArray)
        {
            var arr = SourceStrArray.Split(new char[] { ',' });
            var result = "";
            foreach (var s in arr)
            {
                result = result.Replace(s + ",", "");
                result += s + ",";
            }
            if (result.Length > 1)
                result = result.Substring(0, result.Length - 1);
            return result;
        }

        protected void ShowResults()
        {
            try
            {
                btns.Visible = mgRezult.Visible = true;
                SearchCriteria.Visible = false;
                var records = 0;
                decimal total_amount = 0;
                object business_line = (SearchCriteria.BusinessLine != "") ? SearchCriteria.BusinessLine : null;
                object org_code = (SearchCriteria.Organization != "") ? SearchCriteria.Organization : null;
                org_code = ClearDoubles(org_code.ToString());
                object book_month = (SearchCriteria.BookMonth != "") ? SearchCriteria.BookMonth : null;
                object group_cd = (SearchCriteria.GroupCD != "") ? SearchCriteria.GroupCD : null;
                object sum_func = (SearchCriteria.SummaryFunction != "") ? SearchCriteria.SummaryFunction : null;
                object oc_code = (SearchCriteria.ObjClassCode != "") ? SearchCriteria.ObjClassCode : null;
                object CostElem = (SearchCriteria.CostElement != "") ? SearchCriteria.CostElement : null;
                object doc_num = (SearchCriteria.DocNumber != "") ? SearchCriteria.DocNumber : null;

                var Message = "<font style='color:Navy'>Search Criteria:</font><font style='color:black'><br /> ";
                if (SearchCriteria.ViewMode == (int)FundsReviewViewMode.fvObligations)
                    Message += "Obligations,";
                else if (SearchCriteria.ViewMode == (int)FundsReviewViewMode.fvIncome)
                    Message += "Income,";
                else
                    Message += "One Time Adjustments,";

                Message += " Fiscal Year: " + SearchCriteria.FiscalYear;
                if (book_month != null)
                    Message += ", Book Month: " + SearchCriteria.BookMonthNames.ToString().Replace(",", ", ");
                Message += "<br />Budget Activity: " + SearchCriteria.BudgetActivity;
                if (business_line != null)
                    Message += ", Business Line: " + SearchCriteria.BusinessLineName;
                if (org_code != null)
                    Message += "<br />Organizations: " + org_code.ToString().Replace(",", ", ");
                if (doc_num != null)
                    Message += "<br />Document: " + doc_num;
                if (group_cd != null)
                    Message += "<br/>Function Report Group: " + SearchCriteria.GroupCDName;
                if (sum_func != null)
                    Message += "<br/>Summary Functions: " + sum_func.ToString().Replace(",", ", ");
                if (oc_code != null)
                    Message += "<br/>OCC Functions: " + oc_code.ToString().Replace(",", ", ");
                if (CostElem != null)
                    Message += "<br/>Cost Elements: " + CostElem.ToString().Replace(",", ", ");
                Message += "</font><br/>";

                var ds = FundStatus.GetSearchResults(SearchCriteria.ViewMode, SearchCriteria.FiscalYear, SearchCriteria.BudgetActivity, out records, out total_amount,
                    org_code, book_month, group_cd, sum_func, oc_code, CostElem, doc_num, Int32.Parse(ConfigurationManager.AppSettings["QueryResultMaxRecords"]), false);

                if (ds == null || records == 0)
                {
                    Message += "No records found.";
                }
                else
                {
                    if (records > Int32.Parse(ConfigurationManager.AppSettings["QueryResultMaxRecords"]))
                        Message += String.Format("{0:0,0} records found.  Total Amount {1:$0,0}. <br />Only first {2:0,0} records are displayed. Please narrow your search criteria or request results by email.",
                            records, total_amount, ConfigurationManager.AppSettings["QueryResultMaxRecords"]);
                    else if (records > 9)
                        Message += String.Format("{0:0,0} records found. Total Amount {1:$0,0}.", records, total_amount);
                    else
                        Message += String.Format("{0} records found. Total Amount {1:$0,0}.", records, total_amount);

                    var dt = ds.Tables[0].Clone();
                    dt.Columns["Amount"].DataType = typeof(string);
                    foreach (DataRow r in ds.Tables[0].Rows)
                    {
                        dt.ImportRow(r);
                    }
                    dt.Columns["Amount"].ColumnName = "$ Amount";
                    foreach (DataRow r in dt.Rows)
                    {
                        if (r["book month"].ToString() != "")
                            r["book month"] = String.Format("{0:MMMM}", DateTime.Parse(r["book month"].ToString() + "/2000"));

                        r["$ Amount"] = String.Format("{0:0,0}", Convert.ToInt64(Convert.ToDecimal(r["$ Amount"])));
                        if (r["$ Amount"].ToString() == "00")
                            r["$ Amount"] = "0";
                    }
                    mgRezult.Table = dt;

                }
                lblMessages.Text = Message;
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                //"Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding."
                if (ex.Number == -2)
                {
                    //give a message to the user:
                    throw new Exception("Search results contain too many records or the server is not responding. You can try again or narrow your seach criteria. Thank you.");
                }
                else
                    throw ex;
            }
        }

        void ctrlCriteria_Submit(object sender, EventArgs e)
        {
            try
            {
                SearchCriteria.FillControls();
                //save selected criteria values:
                FundsSearchSelectedValues = SearchCriteria.Criteria;
                ShowResults();
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }

        protected void btnNewSearch_Click(object sender, EventArgs e)
        {
            btns.Visible = mgRezult.Visible = false;
            SearchCriteria.Visible = true;
            SearchCriteria.FillControls();
        }

    }
}