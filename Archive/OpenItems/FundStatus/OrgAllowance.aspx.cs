namespace GSA.OpenItems.Web
{
    using System;
    using Data;

    public partial class FundStatus_OrgAllowance : PageBase
    {

        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {
                if (!Page.ClientScript.IsClientScriptBlockRegistered(Page.GetType(), "JS") &&
                    (Page.User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString()) || Page.User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceRO).ToString())
                        || Page.User.IsInRole(((int)UserRoles.urFSBDPowerReader).ToString())))
                {
                    Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "JS",
                        "<script language='javascript' src='include/OrgAllowance.js'></script>");
                    //Page.ClientScript.RegisterStartupScript(Page.GetType(), "JS_OnLoad",
                    //"<script FOR=window EVENT=onload language='javascript'>Page_OnLoad()</script>");
                }
                if (!IsPostBack)
                {
                    if (Page.User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString()) || Page.User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceRO).ToString())
                        || Page.User.IsInRole(((int)UserRoles.urFSBDPowerReader).ToString()))
                        InitControls();
                    else
                        DisablePage();
                }

            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                //if (Errors.Count > 0)
                    //lblError.Text = GetErrors();
            }
        }

        private void DisablePage()
        {
            tblCntrls.Visible = false;
            lblChartMsg.Visible = false;
            tblChart.Visible = false;
            tblBttns.Visible = false;

            //lblError.Visible = true;
            //lblError.Text = "You are not authorized to visit this page.";
        }



        private void InitControls()
        {
            RebuildAllowanceChart();
        }

        private void RebuildAllowanceChart()
        {
            /*
            FSAllowanceUI objUI = new FSAllowanceUI();
            objUI.ChartTable = tblChart;
            objUI.ReadOnly = !(Page.User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString()));
            objUI.DistributionViewType = CurrentDistributionViewType;
            objUI.FiscalYear = ddlFiscalYear.SelectedValue;
            objUI.Organization = SelectedOrganization;
            objUI.BusinessLine = SelectedBusinessLine;
            objUI.BusinessLineDesc = ddlBusinessLine.Items[ddlBusinessLine.SelectedIndex].Text;
            objUI.DrawDistributionTable(false);
            tblChart = objUI.ChartTable;
             */
        }

        private int CurrentDistributionViewType
        {
            get
            {
                if (ViewState["DISTR_VIEW"] == null)
                    ViewState["DISTR_VIEW"] = (int)FundAllowanceViewType.vtAllowanceTotal;

                return (int)ViewState["DISTR_VIEW"];
            }
            set { ViewState["DISTR_VIEW"] = value; }
        }

        private string SelectedBusinessLine
        {
            get
            {
                if (ViewState["BL"] == null)
                    ViewState["BL"] = 0;

                return (string)ViewState["BL"];
            }
            set { ViewState["BL"] = value; }
        }

        private string SelectedOrganization
        {
            get
            {
                if (ViewState["ORG"] == null)
                    ViewState["ORG"] = "";

                return (string)ViewState["ORG"];
            }
            set { ViewState["ORG"] = value; }
        }




    }
}