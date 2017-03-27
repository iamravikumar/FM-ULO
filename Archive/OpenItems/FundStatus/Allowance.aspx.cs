

using System.Activities;
using System.Linq;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Configuration;
    using System.Data.SqlClient;
    using Data;

    public partial class Allowance : PageBase
    {

        private readonly FundAllowanceBO FundAllowance;
        private readonly FundStatusBO FundStatus;

        public Allowance()
        {
            FundAllowance = new FundAllowanceBO(this.Dal);
            FundStatus = new FundStatusBO(this.Dal);
        }
        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {
                if (!Page.ClientScript.IsClientScriptBlockRegistered(Page.GetType(), "JS"))
                {
                    Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "JS",
                        "<script language='javascript' src='include/Allowance.js'></script>");
                    //Page.ClientScript.RegisterStartupScript(Page.GetType(), "JS_OnLoad",
                    //"<script FOR=window EVENT=onload language='javascript'>Allowance_OnLoad()</script>");
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
                //    lblError.Text = GetErrors();
            }
        }

        private void DisablePage()
        {
            lblMessage.Text = "You are not authorized to visit this page.";
            lblMessage.CssClass = "lrgBldRedText";
            tblLabels.Visible = false;
            tblChart.Visible = false;
            tblButtons.Visible = false;
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            ddlFiscalYear.SelectedIndexChanged += new EventHandler(ddlFiscalYear_SelectedIndexChanged);
        }

        private decimal GetDecimal(string s)
        {
            s = s.Replace(",", "").Replace("$", "");
            return s == "" ? 0 : Convert.ToDecimal(s);
        }

        protected void btnSave_ServerClick(object sender, EventArgs e)
        {
            try
            {
                var rr = Result.Value.Split(new char[] { '|' });
                for (var i = 0; i < rr.Length; i++)
                {
                    var cc = rr[i].Split(new char[] { ';' });
                    var cnt = 1;
                    var m1 = DateTime.Parse(cc[2] + "/2000").Month;
                    var m2 = DateTime.Parse(cc[3] + "/2000").Month;
                    var s = m1.ToString().Length < 2 ? "0" + m1.ToString() : m1.ToString();
                    while (m1 != m2)
                    {
                        m1 = m1 % 12 + 1;
                        s += "," + (m1.ToString().Length < 2 ? "0" + m1.ToString() : m1.ToString());
                        cnt++;
                    }

                    var saveAllowanceReturn = FundAllowance.SaveFYAllowance(Convert.ToInt32(cc[0]),
                        ddlFiscalYear.SelectedValue, GetDecimal(cc[1]), s, cnt, CurrentUserID);

                    if (saveAllowanceReturn > 0)
                        History.InsertHistoryOnUpdateAllowance(
                            cc[0] == "0" ? (int)HistoryActions.haAddNewAllowanceAmount : (int)HistoryActions.haUpdateAllowance,
                            ddlFiscalYear.SelectedValue, cc[1], s, "", CurrentUserID);

                }
                var dt = FundStatus.GetOrganizationList().ToDataSet().Tables[0];
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    FSSummaryReport.DataObsoleteFlag(ddlFiscalYear.SelectedValue, dt.Rows[i]["ORG_CD"].ToString());
                }
                InitControls();
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

        void ddlFiscalYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var strFiscalYear = ddlFiscalYear.SelectedValue;
                //rebuild info table for selected fiscal year:
                RebuildAllowanceChart(strFiscalYear);
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

        private void RebuildAllowanceChart(string FiscalYear)
        {
            var detailedAllowance = FundAllowance.GetDetailedAllowanceForFiscalYear(FiscalYear);

            if (!detailedAllowance.Any())
                lblChartMsg.Text = "There is no allowed amount for selected fiscal year.";
            else
                lblChartMsg.Text = "Allowance amount for selected fiscal year:";

            /*
            FSAllowanceUI objUI = new FSAllowanceUI();
            objUI.DBTable = ds.Tables[0];
            objUI.ChartTable = tblChart;
            objUI.ReadOnly = !(Page.User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString()));
            objUI.DrawTableNCR(false);
            tblChart = objUI.ChartTable;
             */

            //get History:

            var ds = FundAllowance.GetAllowanceHistory(FiscalYear, HistoryActions.haAddNewAllowanceAmount,
                HistoryActions.haUpdateAllowance);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
                lblMessage.Text = String.Format("Last updated by {0} on {1}.",
                                    ds.Tables[0].Rows[0]["UpdateUserName"].ToString(),
                                    String.Format("{0:MMMM dd, yyyy}", ds.Tables[0].Rows[0]["ActionDate"]));
        }

        private void InitControls()
        {
            if (Page.User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString()))
                btnSave.AddOnClick("return save_changes()");
            else
                tblButtons.Visible = false;

            //init fiscal year list:   
            ddlFiscalYear.DataSource = FundAllowance.GetAvailableFiscalYearList(Server.MapPath("~"));
            ddlFiscalYear.DataTextField = "Text";
            ddlFiscalYear.DataValueField = "Value";
            ddlFiscalYear.DataBind();
            ddlFiscalYear.SelectedValue = DateTime.Now.Year.ToString();
            ddlFiscalYear_SelectedIndexChanged(null, null);
        }
    }
}
