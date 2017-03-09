namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Configuration;
    using System.Data.SqlClient;
    using Data;

    public partial class AllowanceDistribution : PageBase
    {

        private FundAllowanceBO FundAllowance;

        public AllowanceDistribution()
        {
            FundAllowance = new FundAllowanceBO(this.Dal);
        }

        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {
                if (!Page.ClientScript.IsClientScriptBlockRegistered(Page.GetType(), "JS") && Page.User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString()))
                {
                    Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "JS",
                        "<script language='javascript' src='include/AllowanceDistribution.js'></script>");
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "JS_OnLoad",
                        "<script FOR=window EVENT=onload language='javascript'>Page_OnLoad()</script>");
                }
                if (!IsPostBack)
                {
                    if (Page.User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString()))
                        InitControls();
                    else
                        DisablePage();
                }
                else
                    lblError.Visible = false;
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                {
                    lblError.Text = GetErrors();
                    lblError.Visible = true;
                }
            }
        }

        private void InitControls()
        {
            var org = Request.QueryString["org"];
            var business_line = Request.QueryString["bl"];
            var business_line_code = Request.QueryString["blc"];
            var fiscal_year = Request.QueryString["fy"];
            var book_month = Request.QueryString["bm"];
            var type = Request.QueryString["t"];

            var label_text = "Change Distribution ";
            var distribution_type = Int32.Parse(type);
            HistoryActions HistoryType;
            if (distribution_type == (int)FundAllowanceViewType.vtAllowanceForBusinessLine)
            {
                label_text += "by Organization for " + business_line;
                HistoryType = HistoryActions.haUpdateFS_OrgAllowanceDistribution;
            }
            else if (distribution_type == (int)FundAllowanceViewType.vtAllowanceForOrganization)
            {
                label_text += "by Function Code for " + org;
                HistoryType = HistoryActions.haUpdateFS_FuncAllowanceDistribution;
            }
            else
            {
                label_text += "over NCR by Business Line";
                HistoryType = HistoryActions.haUpdateFS_OrgAllowanceDistribution;
            }
            label_text += "<br />starting " + String.Format("{0:MMMM}", DateTime.Parse(book_month + "/" + fiscal_year));
            label_text += " FY " + fiscal_year;
            lblMain.Text = label_text;

            var amount = FundAllowance.GetMonthlyAmount(fiscal_year, book_month, business_line_code, org, distribution_type);
            lblAllowance.Text = String.Format("{0:$0,0}", amount);

            //build Distribution Table:
            /*
            FSAllowanceUI fs = new FSAllowanceUI();
            fs.FiscalYear = fiscal_year;
            fs.BookMonth = book_month;
            fs.Organization = org;
            fs.BusinessLine = business_line_code;
            fs.BusinessLineDesc = business_line;
            fs.DistributionViewType = distribution_type;
            fs.AllowanceAmountToDistribute = amount;
            fs.ChartTable = tblData;
            fs.ReDistributeTable();
            tblData = fs.ChartTable;
             */
            //get History:
            var ds = FundAllowance.GetFundStatusUpdateHistory(HistoryType, fiscal_year, book_month, org);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
                lblMessage.Text = String.Format("Last updated by {0} on {1}.",
                                    ds.Tables[0].Rows[0]["UpdateUserName"].ToString(),
                                    String.Format("{0:MMMM dd, yyyy}", ds.Tables[0].Rows[0]["ActionDate"]));


        }

        private void DisablePage()
        {
            lblMain.Visible = false;
            lblAllText.Visible = false;
            lblAllowance.Visible = false;
            lblMessage.Visible = false;
            tblData.Visible = false;
            tblButtons.Visible = false;

            lblError.Visible = true;
            lblError.Text = "You are not authorized to visit this page.";
        }
    }
}