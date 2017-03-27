using System.Collections.Generic;
using System.Linq;
using OpenItems.Data;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web.UI.WebControls;
    using Data;

    public class OrgUsersErrEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; }
    }

    public partial class Controls_OrgUsers : System.Web.UI.UserControl
    {

        public delegate void OrgUsersErrEventHandler(object sender, OrgUsersErrEventArgs e);
        public event OrgUsersErrEventHandler OnError;

        //TODO: Create UserControl Base class that defines these. Not able to use constructor.
        private static readonly IDataLayer Dal = new DataLayer(new zoneFinder(), new ULOContext());
        private readonly AssignBO Assign = new AssignBO(Dal, new ItemBO(Dal));

        protected void Page_Init(object sender, EventArgs e)
        {
            ddlOrganizations.SelectedIndexChanged += new EventHandler(ddlOrganizations_SelectedIndexChanged);
            ddlUsers.SelectedIndexChanged += new EventHandler(ddlUsers_SelectedIndexChanged);
        }

        void ddlUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var selected = ddlUsers.SelectedValue;

                if (selected == "0")
                {
                    //get the whole list of organizations:
                    ddlOrganizations.DataSource = OrgList;
                    ddlOrganizations.DataValueField = "OrgCode";
                    ddlOrganizations.DataTextField = "OrganizationDesc";
                    ddlOrganizations.DataBind();
                }
                else
                {
                    //get organizations list according user's organization
                    //only in case if selected organization = "0"
                    //in other case we already have initiated list for organizations:

                    if (ddlOrganizations.SelectedValue == "0")
                    {
                        var userOrg = ((DataRow)UsersListTable.Select("UserID = " + selected)[0])["Organization"].ToString();

                        var col = new ListItemCollection();
                        var it = new ListItem("", "0");
                        col.Add(it);

                        var orgsList = OrgList.Where(o => o.Organization == userOrg);
                        foreach (var org in orgsList)
                        {
                            it = new ListItem(org.OrganizationDesc, org.Organization);
                            col.Add(it);
                        }

                        ddlOrganizations.DataSource = col;
                        ddlOrganizations.DataValueField = "Value";
                        ddlOrganizations.DataTextField = "Text";
                        ddlOrganizations.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                //erase event OnError:
                if (this.OnError != null)
                {
                    var arg = new OrgUsersErrEventArgs();
                    arg.ErrorMessage = ex.Message;
                    this.OnError(this, arg);
                }
            }
        }

        void ddlOrganizations_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ddlOrganizations.SelectedValue == "0")
                {
                    //get the whole list of users:
                    ddlUsers.DataSource = UsersListTable;
                    ddlUsers.DataValueField = "UserID";
                    ddlUsers.DataTextField = "UserName";
                    ddlUsers.DataBind();
                }
                else
                {
                    var selected_org = ddlOrganizations.SelectedItem.Text;
                    var selected = selected_org.Substring(0, selected_org.IndexOf(":") - 1);

                    //get users related to the selected organization only.
                    //if we already have selected user related to the selected organization,
                    //then select it.
                    var find_selected = false;
                    var selectedUser = ddlUsers.SelectedValue;
                    if (selected == ((DataRow)UsersListTable.Select("UserID = " + selectedUser)[0])["Organization"].ToString())
                        find_selected = true;

                    var col = new ListItemCollection();
                    var it = new ListItem("", "0");
                    col.Add(it);

                    var drows = UsersListTable.Select("Organization = '" + selected + "'");
                    foreach (var dr in drows)
                    {
                        it = new ListItem(dr["UserName"].ToString(), ((int)dr["UserID"]).ToString());
                        col.Add(it);
                    }

                    ddlUsers.DataSource = col;
                    ddlUsers.DataValueField = "Value";
                    ddlUsers.DataTextField = "Text";
                    ddlUsers.DataBind();

                    if (find_selected)
                        ddlUsers.SelectedValue = selectedUser;
                }
            }
            catch (Exception ex)
            {
                //erase event OnError:
                if (this.OnError != null)
                {
                    var arg = new OrgUsersErrEventArgs();
                    arg.ErrorMessage = ex.Message;
                    this.OnError(this, arg);
                }
            }
        }

        public void InitLists()
        {
            var orgList = Assign.GetOrganizationsList();
            orgList.Insert(0, new spGetReassignOrgList_Result {OrgCode = "0", OrganizationDesc = "", Organization = "0"});
            //var dtOrg = dsOrg.Tables[0];
           

            OrgList = orgList;
            ddlOrganizations.DataSource = orgList;
            ddlOrganizations.DataValueField = "OrgCode";
            ddlOrganizations.DataTextField = "OrganizationDesc";
            ddlOrganizations.DataBind();

            var dsUsers = Assign.GetUsersOrganizationsList();
            var dtUsers = dsUsers.Tables[0];
            var drUsers = dtUsers.NewRow();
            drUsers["UserID"] = 0;
            drUsers["UserName"] = "";
            drUsers["Organization"] = "0";
            dtUsers.Rows.InsertAt(drUsers, 0);

            UsersListTable = dtUsers;
            ddlUsers.DataSource = dtUsers;
            ddlUsers.DataValueField = "UserID";
            ddlUsers.DataTextField = "UserName";
            ddlUsers.DataBind();
        }

        private List<spGetReassignOrgList_Result> OrgList
        {
            get
            {
                if (ViewState["OrgList"] != null)
                    return (List<spGetReassignOrgList_Result>)ViewState["OrgList"];
                else
                    return null;
            }
            set
            {
                ViewState["OrgList"] = value;
            }
        }

        private DataTable UsersListTable
        {
            get
            {
                if (ViewState["UsersListTable"] != null)
                    return (DataTable)ViewState["UsersListTable"];
                else
                    return null;
            }
            set
            {
                ViewState["UsersListTable"] = value;
            }
        }

        public string SelectedOrgCode
        {
            get
            {
                return ddlOrganizations.SelectedValue;
            }
            set
            {
                if (value != null && value != "")
                {

                    ddlOrganizations.SelectedValue = value;
                    ddlOrganizations_SelectedIndexChanged(null, null);
                }
            }
        }

        public int SelectedUserID
        {
            get
            {
                return Int32.Parse(ddlUsers.SelectedValue);
            }
            set
            {
                if (value != null && value != 0)
                {
                    ddlUsers.SelectedValue = value.ToString();
                }
            }
        }

        public string OrganizationLabel
        {
            set { lblOrganization.Text = value; }
        }

        public string UsersLabel
        {
            set { lblUsers.Text = value; }
        }


    }
}