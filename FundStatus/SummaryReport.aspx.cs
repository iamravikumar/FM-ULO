namespace GSA.OpenItems.Web
{

    using System;
    using System.Configuration;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;
    using Data;

    public partial class FundStatus_SummaryReport : PageBase
    {
        FSSummaryUI draw;
        private readonly UsersBO Users;
        public FundStatus_SummaryReport()
        {
            Users = new UsersBO(this.Dal, new EmailsBO(this.Dal));
        }

        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    ctrlCriteria.ScreenType = (int)FundsStatusScreenType.stFundSummaryReport;
                    ctrlCriteria.InitControls();

                    if (Request.QueryString["back"] != null && Request.QueryString["back"] == "y")
                    {
                        //back from Funds Review screen - display previous report:
                        ctrlCriteria.DisplayPrevSelectedValues(FundsSummaryReportSelectedValues);

                        BuildReport();
                    }
                }
                divData.Attributes["style"] = "display:none;";
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

        protected void Page_Init(object sender, EventArgs e)
        {
            ctrlCriteria.Submit += new EventHandler(ctrlCriteria_Submit);
        }

        void ctrlCriteria_Submit(object sender, EventArgs e)
        {
            try
            {
                FundsSummaryReportSelectedValues = ctrlCriteria.SaveCurrentSelectedValues();

                BuildReport();
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

        private void BuildReport()
        {
            lblMessages.Text = "";
            lblState.Text = "";
            //divData.Attributes["style"] = "display:none;";

            if (!(User.IsInRole(((int)UserRoles.urFSBDPowerReader).ToString()) || User.IsInRole(((int)UserRoles.urFSBDAnalystFundsCoordinator).ToString()) ||
                    User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceRO).ToString()) || User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString())))
            {
                if (Users.UserAuthorizedForFSReports(CurrentUserID, ctrlCriteria.BusinessLine, ctrlCriteria.Organization) <= 0)
                    throw new Exception("You are not authorized to see current report. Please select Business Line or Organization that you are allowed to review. Thank you.");
            }

            //first check if the report data is valid:
            var status = FSSummaryReport.GetReportStateStatus(ctrlCriteria.FiscalYear, ctrlCriteria.Organization, ctrlCriteria.BusinessLine);
            if (status == (int)FundStatusReportStateStatus.rsDoesNotExist)
            {
                divReportState.Visible = true;
                lblState.Text = String.Format("Fund Status Report for Organization {0}, Fiscal Year {1} is not available at this moment. <p />The report data is updated by automated process every {2} minutes. <br />You can request the report immediately (it might take a few minutes). Thank you.", ctrlCriteria.Organization, ctrlCriteria.FiscalYear, ConfigurationManager.AppSettings["RebuildReportIntervalInMinutes"]);
            }
            else if (status == (int)FundStatusReportStateStatus.rsObsolete)
            {
                divReportState.Visible = true;
                lblState.Text = String.Format("Fund Status Report for Organization {0}, Fiscal Year {1} is obsolete. <p />The report data is updated by automated process every {2} minutes. <br />You can request the report immediately (it might take a few minutes). Thank you.", ctrlCriteria.Organization, ctrlCriteria.FiscalYear, ConfigurationManager.AppSettings["RebuildReportIntervalInMinutes"]);
            }
            else
            {
                divReportState.Visible = false;
                lblState.Text = "";
            }

            if (status == (int)FundStatusReportStateStatus.rsObsolete || status == (int)FundStatusReportStateStatus.rsValid)
            {
                if (status == (int)FundStatusReportStateStatus.rsValid)
                    lblMessages.Text = "TIP: click on the month header to enter Fund Status Report.";

                draw = new FSSummaryUI();
                draw.FiscalYear = ctrlCriteria.FiscalYear;
                draw.Organization = ctrlCriteria.Organization;
                draw.BusinessLine = ctrlCriteria.BusinessLine;

                CreateReportView((int)FundStatusSummaryReportView.svObligations, "Obligations", "Show/Hide Fund Status Summary Report - Obligations Info");
                CreateReportView((int)FundStatusSummaryReportView.svObligationsMonthly, "Obligations - Monthly View", "Show/Hide Fund Status Summary Report - Obligations Monthly Amounts Info");
                CreateReportView((int)FundStatusSummaryReportView.svIncome, "Income", "Show/Hide Fund Status Summary Report - Income Info");
                CreateReportView((int)FundStatusSummaryReportView.svIncomeMonthly, "Income - Monthly View", "Show/Hide Fund Status Summary Report - Income Monthly Amounts Info");
                CreateReportView((int)FundStatusSummaryReportView.svAllowance, "Allowance", "Show/Hide Fund Status Summary Report - Allowance Info");
                CreateReportView((int)FundStatusSummaryReportView.svYearEndProjection, "Year End Projection", "Show/Hide Fund Status Summary Report - Year End Projection Info");
                CreateReportView((int)FundStatusSummaryReportView.svProjectionVariance, "Projection Variance", "Show/Hide Fund Status Summary Report - Projection Variance Info");
                CreateReportView((int)FundStatusSummaryReportView.svProjectedBalance, "Projected Balance", "Show/Hide Fund Status Summary Report - Projected Balance Info");

                divData.Attributes["style"] = "display:inline;";

                /*
                StringBuilder sb = new StringBuilder();
                sb.Append("<script language='javascript'> ");
                sb.Append("on_long_load(); ");
                sb.Append("</script>");
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "load_back", sb.ToString());
                 */
            }
            else
                draw = null;
        }

        private void CreateReportView(int ReportViewType, string ReportHeader, string LabelText)
        {
            draw.SummaryReportViewType = ReportViewType;
            var lbl = (Label)FindControl(String.Format("lblShowTable_{0}", ReportViewType));
            lbl.Text = LabelText;
            var tbl = (HtmlTable)FindControl(String.Format("tblData_{0}", ReportViewType));
            draw.TableToDraw = tbl;
            draw.ReportHeader = ReportHeader;
            draw.BuildHTMLTable();
            tbl = draw.TableToDraw;
            //tbl.Style.Add("overflowX", "auto");
        }
    }
}