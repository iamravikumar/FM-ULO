using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Configuration;

    public partial class Controls_Menu : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

                if (Session["FullName"] != null)
                {
                    lblUserFullName.Text= "Welcome, " + Session["FullName"].ToString() + "!"; 

                    if (Session["UserEnmail"] != null)
                    {
                        lblUserFullName.ToolTip =  Session["UserEnmail"].ToString();
                    }

                }

                hlAdmin.Visible = false;
                sAdmin.Visible = false;
                sPassword.Visible = true;

                hlTabApp1.NavigateUrl = Settings.Default.OpenItemsDefaultPage;
                hlTabApp2.NavigateUrl = Settings.Default.FundStatusDefaultPage;
                var sAdminPageRightsForBDAdmin = Settings.Default.AdminPageRightsForBDAdmin;
                var sAdminPageRightsForOrgAdmin = Settings.Default.AdminPageRightsForOrgAdmin;
                var sPasswordMenuIsVisible = Settings.Default.PasswordMenuIsVisible;

                if (Page.User.IsInRole(((int)UserRoles.urProjectManager).ToString()))
                {
                    // ???
                }

                if (Page.User.IsInRole(((int)UserRoles.urSysAdmin).ToString()))
                {
                    ActivateOI_OrgAdminMenu();
                    ActivateOI_BDAdminMenu();
                    ActivateFS_OrgAdminMenu();
                    ActivateFS_BDAdminMenu();
                    Activate_SysAdminMenu();
                    Activate_AdminMenu();
                }

                if (Page.User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                {
                    ActivateOI_OrgAdminMenu();
                    if (sAdminPageRightsForOrgAdmin)
                    {
                        Activate_AdminMenu();
                    }
                }

                if (Page.User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                {
                    ActivateOI_OrgAdminMenu();
                    ActivateOI_BDAdminMenu();

                    if (sAdminPageRightsForBDAdmin)
                    {
                        Activate_AdminMenu();
                    }
                }

                if (Page.User.IsInRole(((int)UserRoles.urFSOrgAdminWR).ToString()) || Page.User.IsInRole(((int)UserRoles.urFSOrgAdminRO).ToString())
                    || Page.User.IsInRole(((int)UserRoles.urFSBDAnalystFundsCoordinator).ToString()))
                    ActivateFS_OrgAdminMenu();

                if (Page.User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceWR).ToString()) || Page.User.IsInRole(((int)UserRoles.urFSBDAdminAllowanceRO).ToString())
                    || Page.User.IsInRole(((int)UserRoles.urFSBDPowerReader).ToString()))
                {
                    ActivateFS_OrgAdminMenu();
                    ActivateFS_BDAdminMenu();
                }

                if (Page.Request.Url.ToString().IndexOf("FundStatus") > -1)
                {
                    div_app1.Visible = false;
                    hlTabApp1.Attributes["class"] = "";
                    div_app2.Visible = true;
                    hlTabApp2.Attributes["class"] = "selected";
                }
                else
                {
                    div_app1.Visible = true;
                    hlTabApp1.Attributes["class"] = "selected";
                    div_app2.Visible = false;
                    hlTabApp2.Attributes["class"] = "";
                }

                if (sPasswordMenuIsVisible)
                {
                    nlPassword.Visible = true;
                    sPassword.Visible = true;
                }
                else
                {
                    nlPassword.Visible = false;
                    sPassword.Visible = false;
                }

                nlPassword.Visible = false;
                sPassword.Visible = false;


        }


        private void ActivateOI_OrgAdminMenu()
        {
            hlAssignment.Visible = true;
            sAssignment.Visible = true;
            //hlDeobligation.Visible = true;
            //sDeobligation.Visible = true;
            hlReports.Visible = true;
            sReports.Visible = true;
            
        }

        private void ActivateOI_BDAdminMenu()
        {
            //hlAdmin.Visible = true;
            //sAdmin.Visible = true;
            //hlAdmin.Visible = true;
            //sAdmin.Visible = true;
        }

        private void Activate_AdminMenu()
        {
            hlAdmin.Visible = true;
            sAdmin.Visible = true;
        }

        private void Activate_SysAdminMenu()
        {
            hlUploadData.Visible = true;
            sUploadData.Visible = true;
            hlAdmin.Visible = true;
            sAdmin.Visible = true;
            
        }

        private void ActivateFS_OrgAdminMenu()
        {
            //hlFundsSearch.Visible = true;
            //sFundsSearch.Visible = true;
            hlFundsStatus.Visible = true;
            sFundsStatus.Visible = true;
            hlSummaryReport.Visible = true;
            sSummaryReport.Visible = true;
        }

        private void ActivateFS_BDAdminMenu()
        {
            hlAllowance.Visible = false;
            sAllowance.Visible = false;
            hlAllowanceOrg.Visible = false;
            sAllowanceOrg.Visible = false;            
        }
    }
}