using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Configuration;
    using Data;

    public partial class FundsReport : PageBase
    {

        private readonly UsersBO Users;
        public FundsReport()
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
                        "<script language='javascript' src='include/FundsReport.js'></script>");
                    //Page.ClientScript.RegisterStartupScript(Page.GetType(), "JS_OnLoad",
                    //    "<script FOR=window EVENT=onload language='javascript'>Page_OnLoad()</script>");
                }

                if (!IsPostBack)
                {
                    ctrlCriteria.ScreenType = (int)FundsStatusScreenType.stFundStatusReport;
                    ctrlCriteria.InitControls();

                    if (Request.QueryString["back"] != null && Request.QueryString["back"] == "y")
                    {
                        //back from Funds Review screen - display previous report:
                        ctrlCriteria.DisplayPrevSelectedValues(FundsStatusSelectedValues);

                        BuildReport();
                    }
                    else if (Request.QueryString["fy"] != null && Request.QueryString["fy"] != "")
                    {
                        ctrlCriteria.FiscalYear = Request.QueryString["fy"];
                        ctrlCriteria.BookMonth = Request.QueryString["bm"];

                        ctrlCriteria.BusinessLine = Request.QueryString["bl"];
                        ctrlCriteria.Organization = Request.QueryString["org"];

                        //ctrlCriteria.AllowReturnBack("SummaryReport.aspx?back=y");                                                
                        FundsStatusSelectedValues = ctrlCriteria.SaveCurrentSelectedValues();

                        BuildReport();
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

        protected void Page_Init(object sender, EventArgs e)
        {
            ctrlCriteria.Submit += new EventHandler(ctrlCriteria_Submit);
        }

        void ctrlCriteria_Submit(object sender, EventArgs e)
        {
            try
            {
                FundsStatusSelectedValues = ctrlCriteria.SaveCurrentSelectedValues();

                BuildReport();
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

        private void BuildReport()
        {
            lblState.Text = "";

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
                lblState.Text = String.Format("Fund Status Report for Organization {0}, Fiscal Year {1} is not available at this moment. <p />The report data is updated by automated process every {2} minutes. <br />You can request the report immediately (it might take a few minutes). Thank you.", ctrlCriteria.Organization, ctrlCriteria.FiscalYear, Settings.Default.RebuildReportIntervalInMinutes);
            }
            else if (status == (int)FundStatusReportStateStatus.rsObsolete)
            {
                divReportState.Visible = true;
                lblState.Text = String.Format("Fund Status Report for Organization {0}, Fiscal Year {1} is obsolete. <p />The report data is updated by automated process every {2} minutes. <br />You can request the report immediately (it might take a few minutes). Thank you.", ctrlCriteria.Organization, ctrlCriteria.FiscalYear, Settings.Default.RebuildReportIntervalInMinutes);
            }
            else
            {
                divReportState.Visible = false;
                lblState.Text = "";
            }

            if (status == (int)FundStatusReportStateStatus.rsObsolete || status == (int)FundStatusReportStateStatus.rsValid)
            {
                //draw the table tblData:
                var drawing_class = new FSReportUI();
                drawing_class.FiscalYear = ctrlCriteria.FiscalYear;
                drawing_class.BookMonth = ctrlCriteria.BookMonth;
                drawing_class.Organization = ctrlCriteria.Organization;
                drawing_class.BusinessLine = ctrlCriteria.BusinessLine;
                drawing_class.TableToDraw = tblData;
                drawing_class.BuildHTMLTable();
                tblData = drawing_class.TableToDraw;
            }
        }
    }
}