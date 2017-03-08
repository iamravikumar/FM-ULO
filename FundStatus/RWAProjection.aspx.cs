namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using Data;

    public partial class FundStatus_RWAProjection : PageBase
{

        private readonly UsersBO Users;
        public FundStatus_RWAProjection()
        {
            Users = new UsersBO(this.Dal, new EmailsBO(this.Dal));
        }
        protected override void PageLoadEvent(object sender, System.EventArgs e)
    {
        try
        {
            if (!Page.ClientScript.IsClientScriptBlockRegistered(Page.GetType(), "JS"))
            {
                Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "JS",
                    "<script language='javascript' src='include/RWAProjection.js'></script>");
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "JS_OnLoad",
                    "<script FOR=window EVENT=onload language='javascript'>Page_OnLoad()</script>");
            }
            if (!IsPostBack)
            {
                if (!(User.IsInRole(((int)UserRoles.urFSBDAnalystFundsCoordinator).ToString()) || User.IsInRole(((int)UserRoles.urFSBDPowerReader).ToString()) ||
                    User.IsInRole(((int)UserRoles.urFSOrgAdminWR).ToString()) || User.IsInRole(((int)UserRoles.urFSOrgAdminRO).ToString())))
                    throw new Exception("You are not authorized to visit this page.");               

                var org = Request.QueryString["org"];
                var business_line = Request.QueryString["bl"];
                var fiscal_year = Request.QueryString["fy"];
                var book_month = Request.QueryString["bm"];
                //int business_line_code = Int32.Parse(business_line);

                var read_only = true;
                if (User.IsInRole(((int)UserRoles.urFSBDAnalystFundsCoordinator).ToString()) && org != "")
                    read_only = false;
                else
                {
                    var user_role = Users.UserAuthorizedForFSReports(CurrentUserID, business_line, org);
                    if (user_role <= 0)
                        throw new Exception("You are not authorized to visit this page.");
                    else
                        if (user_role == (int)UserRoles.urFSOrgAdminWR && org != "")
                            read_only = false;
                }
                
                var label_text = "Distribution of RWA Income - Over/Under";
                if (org != "")
                {
                    label_text += "<br />by Function Code for Organization " + org;
                }
                else if (business_line != "0")
                {
                    label_text += "<br />by Organization for Buisness Line " + business_line;    //replace to the BL name
                }
                label_text += "<br />starting " + String.Format("{0:MMMM}", DateTime.Parse(book_month + "/" + fiscal_year));
                label_text += " FY " + fiscal_year;
                lblMain.Text = label_text;

                var ds = FSDataServices.GetPaidDaysReference(fiscal_year, book_month);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    label_text = String.Format("PAID DAYS: {0}.  <br />DAYS IN YEAR: {1}.", ds.Tables[0].Rows[0]["CummDays"].ToString(), ds.Tables[0].Rows[0]["DaysInYear"].ToString());
                    lblPaidDays.Text = label_text;
                    lblD.Value = ds.Tables[0].Rows[0]["CummDays"].ToString();
                    lblDD.Value = ds.Tables[0].Rows[0]["DaysInYear"].ToString();
                }

                DataSet dsIncome;
                DataSet dsProjection;                
                if (org != "")
                {
                    dsIncome = FSSummaryReport.GetSummaryState(fiscal_year, org, "");
                    dsProjection = FSDataServices.GetRWAProjection(fiscal_year, org, "");                    
                }
                else
                {
                    dsIncome = FSSummaryReport.GetSummaryState(fiscal_year, "", business_line);
                    dsProjection = FSDataServices.GetRWAProjection(fiscal_year, "", business_line);
                }


                         

                //build Distribution Table:
                var obj = new RWAProjectionUI();
                obj.FiscalYear = fiscal_year;
                obj.Organization = org;
                obj.BusinessLine = business_line;
                obj.BookMonth = book_month;
                obj.IncomeTable = dsIncome.Tables[0];
                obj.ProjectionTable = dsProjection.Tables[0];
                obj.ChartTable = tblData;
                obj.ReadOnly = read_only;
                obj.DrawDistributionTable();
                tblData = obj.ChartTable;

                if (read_only)
                {
                    btnApply.Disabled = true;
                }

                //get History:.
                ds = History.GetFundStatusDataUpdateHistory((int)HistoryActions.haUpdateRWADistribution, org, fiscal_year, "", 0, business_line);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    lblMessage.Text = String.Format("Last updated by {0} on {1}.",
                                        ds.Tables[0].Rows[0]["UpdateUserName"].ToString(),
                                        String.Format("{0:MMMM dd, yyyy}", ds.Tables[0].Rows[0]["ActionDate"])); 
                }
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
    
}
}