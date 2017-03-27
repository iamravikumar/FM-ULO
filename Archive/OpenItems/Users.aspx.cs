namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Configuration;
    using System.Web.UI.WebControls;
    using System.Data.SqlClient;
    using System.Text.RegularExpressions;


    public partial class Users : PageBase 
    {
        private readonly AdminBO Admin;
        private readonly OrgBO Org;
        private readonly UsersBO UsersBO;
        public Users()
        {
            Admin = new AdminBO(this.Dal);
            Org = new OrgBO(this.Dal);
            UsersBO = new UsersBO(this.Dal, new EmailsBO(this.Dal));
        }
        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            tr_password.Visible = false;
            //mgUserRole.ItemCSSClass = "ArtYellow";
            //mgUserRole.TblCSSClass = "tblGridYellow";

            //txtPassword.Text = "";
            if (IsPostBack == false)
            {

                //btnSave.Attributes["onclick"] = "javascript:return confirm('Are you sure you want to save the record and send email to the user with login info?');";
                txtFirstName.SetReadOnly();
                txtLastName.SetReadOnly();
                txtMI.SetReadOnly();
                txtEmail.SetReadOnly();
                //ToolTip="read-only field
                txtFirstName.ToolTip = "read-only field";
                txtLastName.ToolTip = "read-only field";
                txtMI.ToolTip = "read-only field";
                txtEmail.ToolTip = "read-only field";
                
            }

            lblError.Text = "";
            lblError.CssClass = "error";
            //int iID = 0;

            if (IsPostBack == false)
            {
                ddlDefAppl.Enabled = false;
                try
                {
                    // load all NCS users
                    FillCombos("user_new");
                    ddlUsers.Items.Insert(0, "- select -");

                    // load User Roles
                    FillCombos("role");

                    // load Organizations
                    FillCombos("org");
                    ddlOrg.Items.Insert(0, "- select -");

                    if (Request.QueryString["mode"] == "pswd")
                    {
                        td_rb.Visible = false;
                        lnkHelp.Visible = false;
                        var iUserID = Convert.ToInt32(Session["User_ID"]);
                        if (iUserID != 0)
                        {
                            var ds = UsersBO.GetUserByUserID(iUserID);

                            hUserID.Value = ds.Tables[0].Rows[0]["UserID"].ToString();
                            txtEmail.Text = ds.Tables[0].Rows[0]["Email"].ToString();
                            txtFirstName.Text = ds.Tables[0].Rows[0]["FirstName"].ToString();
                            txtLastName.Text = ds.Tables[0].Rows[0]["LastName"].ToString();
                            txtMI.Text = ds.Tables[0].Rows[0]["MiddleInitial"].ToString();
                            txtPhone.Text = ds.Tables[0].Rows[0]["Phone"].ToString();
                            txtPassword.Text = ds.Tables[0].Rows[0]["Password"].ToString().Trim();
                            hPassword.Value = txtPassword.Text;
                            var sActive = ds.Tables[0].Rows[0]["Active"].ToString();
                            if (sActive.Trim().ToLower() == "true")
                            {
                                sActive = "Active";
                            }
                            else
                            {
                                sActive = "Inactive";
                            }
                            DisplayComboSelectedItem(ddlActive, sActive, "t");
                            DisplayComboSelectedItem(ddlDefAppl, ds.Tables[0].Rows[0]["DefaultApplication"].ToString(), "v");

                            var sRoles = ds.Tables[0].Rows[0]["RoleCode"].ToString();
                            var arrRoles = sRoles.Split(new char[] { '|' });
                            var sRoleCode = "";

                            for (var j = 0; j < mgUserRole.Table.Rows.Count; j++)
                            {
                                sRoleCode = mgUserRole.Table.Rows[j][0].ToString();

                                foreach (var s in arrRoles)
                                {
                                    if (sRoleCode == s)
                                    {
                                        mgUserRole.SelectedIndex = j;
                                    }
                                }
                            }

                            DisplayComboSelectedItem(ddlOrg, ds.Tables[0].Rows[0]["Organization"].ToString(), "v");

                            tr_user.Visible = false;
                            txtFirstName.Enabled = false;
                            txtLastName.Enabled = false;
                            txtMI.Enabled = false;
                            txtPhone.Enabled = false;
                            ddlOrg.Enabled = false;
                            //mgUserRole.EnableMultiGrid(false);
                            tr_user_role.AddDisplayNone();
                            ddlActive.Enabled = false;
                            ddlDefAppl.Enabled = false;
                            pswd_help.Visible = false;
                            phone_help.Visible = false;
                            tr_note.Visible = false;
                            btnReset.Visible = false;
                            lblTitle.Text = "Change Password";
                            txtPassword.CssClass = "regBldBlueTextRedBorder";
                            lblError.Text = DisplayMessage("Please enter your new password and click 'Save'", false);

                            //string sUR = "";

                            //foreach (int i in mgUserRole.SelectedIndexes)
                            //{
                            //    sUR = mgUserRole.Table.Rows[i][0].ToString();
                            //    if (sUR == "95" || sUR == "96" || sUR == "97" || sUR == "98" || sUR == "99" || sUR == "100")
                            //    {
                            //        //bNeedPassword = true;
                            //        break;

                            //    }
                            //    else
                            //    {
                            //        //bNeedPassword = false;
                            //        txtPassword.Text = "n/a";
                            //        //txtPassword.Enabled = false;
                            //    }
                            //}
                        }
                        else
                        {
                            throw new Exception ("ULO System can not change your password. Please call your administrator");
                        }

                        //Session["User_ID"]
                        //GetUserByUserID

                    }
                }
                catch (Exception ex)
                {
                    lblError.Text = DisplayMessage("Error: " + ex.Message, true);
                }
            }



        }

        protected void FillCombos(string sComboName)
        {
            var iID = 0;
            ddlUsers.Enabled = true;

            switch (sComboName)
            {
                case "user_new": // add new NCR user 
                    var ds = UsersBO.GetAllNCRUsers();
                    ddlUsers.Items.Clear();
                    ddlUsers.DataSource = ds;
                    ddlUsers.DataTextField = ds.Tables[0].Columns["FullName"].ToString();
                    ddlUsers.DataValueField = ds.Tables[0].Columns["Email"].ToString();
                    ddlUsers.DataBind();
                    break;
                case "user_old":  // edit existing ULO User
                    var users = UsersBO.GetAllULOUsers(); //active and inactive
                    ddlUsers.Items.Clear();
                    ddlUsers.DataSource = users;
                    ddlUsers.DataTextField = "FullName";
                    ddlUsers.DataValueField = "Email";
                    ddlUsers.DataBind();
                    break;
                case "user_new2": // add new non-NCR user manually
                    ddlUsers.Enabled = false;
                    break;
                case "role":
                    mgUserRole.FillMultiGrid("spGetAllUserRolesByApp", "RoleCode", "@app", "ULO", false, out iID);// only for ULO
                   // mgUserRole.FillMultiGrid("spGetAllUserRoles", "RoleCode", "", "", false, out iID); // For ULO and FundStatus apps
                    //spGetAllUserRolesByApp
                    mgUserRole.ShowHeader = false;
                    mgUserRole.MultiChoice = true;
                    break;
                case "org":
                    var orgs = Org.GetAllOrganizations();
                    ddlOrg.Items.Clear();
                    ddlOrg.DataSource = orgs;
                    ddlOrg.DataTextField = "OrgAndOrgCode";
                    ddlOrg.DataValueField = "Organization";
                    ddlOrg.DataBind();
                    break;
                default:
                    break;
            }
        }

        protected void FillControls(int iMode)
        {
            try
            {
                txtFirstName.CssClass = "disabled_textbox";
                txtLastName.CssClass = "disabled_textbox";
                txtMI.CssClass = "disabled_textbox";
                txtEmail.CssClass = "disabled_textbox";
                

                // load User Roles
                FillCombos("role");

                // load Organizations
                FillCombos("org");
                ddlOrg.Items.Insert(0, "- select -");

               
                if (iMode == 0) // new NCR user
                {
                    // load all NCS users from the Personnal DB
                    FillCombos("user_new");
                    ddlUsers.Items.Insert(0, "- select -");
                    txtPassword.Text = "pswd";
                }
                else if (iMode == 2) // edit user
                {
                    // load all ULO users 
                    FillCombos("user_old");
                    ddlUsers.Items.Insert(0, "- select -");
                }
                else //("1") new non-NCR user
                {
                    FillCombos("user_new2");
                    txtFirstName.CssClass = "enabled_textbox";
                    txtLastName.CssClass = "enabled_textbox";
                    txtMI.CssClass = "enabled_textbox";
                    txtEmail.CssClass = "enabled_textbox";
                    txtPassword.Text = "pswd";
                }

                

                hUserID.Value = "0";
            }
            catch (Exception ex)
            {
                lblError.Text = DisplayMessage("Error: " + ex.Message, true);
            }
        }

        protected void ddlUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            var bNeedPassword = true;
            btnSave.Enabled = true;

            mgUserRole.SelectedIndex = -1;

            try
            {
                if (ddlUsers.SelectedIndex != 0)
                {
                    var sEmail = ddlUsers.SelectedItem.Value.ToString().Trim();

                    if (rbListNewOld.SelectedIndex == 0) // new user
                    {
                        hUserID.Value = "0";

                        // here we need to check if this user already exists in the Database

                        var dsULO = UsersBO.GetULOUserByUserEmail(sEmail);

                        if (dsULO.Tables[0].Rows.Count != 0) //user already exists in the Database
                        {
                            btnSave.Enabled = false;
                            throw new Exception("This user already exists in the ULO Database. Please select 'Edit Existing User' option and find this user again.");
                        }
                        else // new user
                        {
                            var ds = UsersBO.GetNCRUserByUserEmail(sEmail);

                            if (ds.Tables[0].Rows.Count != 0)
                            {
                                txtEmail.Text = sEmail;
                                txtFirstName.Text = ds.Tables[0].Rows[0]["FirstName"].ToString();
                                txtLastName.Text = ds.Tables[0].Rows[0]["LastName"].ToString();
                                txtMI.Text = ds.Tables[0].Rows[0]["MiddleInitial"].ToString();
                                txtPhone.Text = ds.Tables[0].Rows[0]["Phone"].ToString();
                                ddlActive.SelectedIndex = 0;
                                ddlDefAppl.SelectedIndex = 0;
                                txtPassword.Text = "pswd";
                            }
                            else
                            {
                                lblError.Text = "No records found in the Personnel Database";
                            }
                        }
                    }
                    else //existing user
                    {
                        var ds = UsersBO.GetULOUserByUserEmail(sEmail);

                        if (ds.Tables[0].Rows.Count != 0)
                        {
                            hUserID.Value = ds.Tables[0].Rows[0]["UserID"].ToString();
                            txtEmail.Text = sEmail;
                            txtFirstName.Text = ds.Tables[0].Rows[0]["FirstName"].ToString();
                            txtLastName.Text = ds.Tables[0].Rows[0]["LastName"].ToString();
                            txtMI.Text = ds.Tables[0].Rows[0]["MiddleInitial"].ToString();
                            txtPhone.Text = ds.Tables[0].Rows[0]["Phone"].ToString();
                            txtPassword.Text = ds.Tables[0].Rows[0]["Password"].ToString().Trim();
                            hPassword.Value = txtPassword.Text;
                            var sActive = ds.Tables[0].Rows[0]["Active"].ToString();
                            if (sActive.Trim().ToLower() == "true")
                            {
                                sActive = "Active";
                            }
                            else
                            {
                                sActive = "Inactive";
                            }
                            DisplayComboSelectedItem(ddlActive, sActive, "t");
                            DisplayComboSelectedItem(ddlDefAppl, ds.Tables[0].Rows[0]["DefaultApplication"].ToString(),"v");

                            var sRoles = ds.Tables[0].Rows[0]["RoleCode"].ToString();
                            var arrRoles = sRoles.Split(new char[] { '|' });
                            var sRoleCode = "";

                            for (var j = 0; j < mgUserRole.Table.Rows.Count; j++)
                            {
                                sRoleCode = mgUserRole.Table.Rows[j][0].ToString();

                                foreach (var s in arrRoles)
                                {
                                    if (sRoleCode == s)
                                    {
                                        mgUserRole.SelectedIndex = j;
                                    }                                
                                }
                            }     
                            
                            DisplayComboSelectedItem(ddlOrg, ds.Tables[0].Rows[0]["Organization"].ToString(), "v");

                            var sUR = "";

                            foreach (var i in mgUserRole.SelectedIndexes)
                            {
                                sUR = mgUserRole.Table.Rows[i][0].ToString();
                                if (sUR == "95" || sUR == "96" || sUR == "97" || sUR == "98" || sUR == "99" || sUR == "100")
                                {
                                    bNeedPassword = true;
                                    break;

                                }
                                else
                                {
                                    bNeedPassword = false;
                                    txtPassword.Text = "n/a";
                                    //txtPassword.Enabled = false;
                                }
                            }

                            
                        }
                        else
                        {
                            lblError.Text = "No records found in the Personnel Database";
                        }
                    }
                }
                else
                {
                    ResetFields();
                }
            }
            catch (Exception ex)
            {
                lblError.Text = DisplayMessage("Error: " + ex.Message, true);
            }

        }
        protected void ResetFields()
        {
            ddlUsers.SelectedIndex = 0;
            txtEmail.Text = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtMI.Text = "";
            txtPhone.Text = "";
            ddlActive.SelectedIndex = 0;
            ddlDefAppl.SelectedIndex = 0;
            ddlOrg.SelectedIndex = 0;
            mgUserRole.SelectedIndex = -1;
            btnSave.Enabled = true;
            lblError.Text = "";

        }

        private string DisplayComboSelectedItem(DropDownList MyComboBox, string s, string sValueOrText)
        {
            try
            {
                if (s == null || s.Trim() == "" || s.Trim() == "0")
                {
                    MyComboBox.Items.Insert(0, "");
                }
                else
                {
                    MyComboBox.SelectedIndex = -1;
                    if (sValueOrText == "v") // value
                    {
                        MyComboBox.Items.FindByValue(s).Selected = true;
                    }
                    else // "t" (text)
                    {
                        MyComboBox.Items.FindByText(s).Selected = true;
                    }
                }
                return s;
            }
            catch (Exception ex)
            {
                MyComboBox.Items.Clear();
                throw (ex);
            }

        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ResetFields();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var iID = 0;
            var iCnt = 0;
            var sSentToUser = "";
            var sSentToAdmins = "";
            var sError = "";
                       
            var sUserRoles = "";
            var sUserDesc = "";
            var sUserSatus = "";
            var sPassword = "";

                try
                {
                
                    var returnNum = UsersBO.SaveUser(Convert.ToInt32(hUserID.Value), txtEmail.Text, sPassword, sUserRoles, Convert.ToInt32(ddlActive.SelectedItem.Value), txtFirstName.Text, txtLastName.Text, txtMI.Text, ddlOrg.SelectedItem.Value, txtPhone.Text, ddlDefAppl.SelectedItem.Value);
                    //AdminBO.SendCustomEmail(txtEmail.Text, "Body", "Sbjt", out sSentTo, out iCnt);

                    if (returnNum > 0)
                    {
                        lblError.Text = DisplayMessage("Record saved.", false);
                    }

                    var sMessageText = "";
                    var sSubject = "";

                    var sMessageTextForBA = "";
                    var sSubjectForBA = "";
                    var sUserAcctDetails = "";

                    var sEmail = txtEmail.Text.Trim();

                    //string sLogin = sEmail.Remove(sEmail.IndexOf("@"));

                    var sMessageHeader = "<font color='red'>*** This is an auto-generated email. Please do not reply or forward to ***</font><br><br>";

                    //string sGenericPswdNote = "You have generic password. Please login into the ULO application, click the 'Change Password' link and replace your generic password with a new one.";

                    sUserAcctDetails = "<br><br>User Role(s): " + sUserDesc + "<br>Organization: " + ddlOrg.SelectedItem.Value.ToString() + "<br>User Status: " + sUserSatus ;
                   
                    var sUserAcctDetailsBA = "<br><br>User Role(s): " + sUserDesc + "<br>Organization: " + ddlOrg.SelectedItem.Value.ToString() + "<br>User Status: " + sUserSatus;

                    if (btnReset.Visible == true) // admin mode - updating existing user or creating a new user
                    {
                        if (rbListNewOld.SelectedValue == "2") //update existing user
                        {
                            sMessageText = sMessageHeader + "Your ULO account has been updated on " + DateTime.Now.ToString() + ".<br>See the account details below." + sUserAcctDetails + "<br><br>http://dotnetweb.pbsncr.gsa.gov/OpenItems";
                            sMessageTextForBA = sMessageHeader + "The ULO account has been updated for " + txtFirstName.Text + " " + txtLastName.Text + " on " + DateTime.Now.ToString() + ".<br>See your account details below." + sUserAcctDetailsBA + "<br><br>http://dotnetweb.pbsncr.gsa.gov/OpenItems";

                            sSubject = "Your ULO account has been changed";
                            sSubjectForBA = "The ULO reviewer account has been changed";
                        }
                        else // creating a new user
                        {
                            sMessageText = sMessageHeader + "Your ULO account has been created on " + DateTime.Now.ToString() + ".<br>See your account details below." + sUserAcctDetails;
                            sMessageTextForBA = sMessageHeader + "New ULO account has been created for " + txtFirstName.Text + " " + txtLastName.Text + " on " + DateTime.Now.ToString() + ".<br>See the account details below." + sUserAcctDetailsBA;

                            sSubject = "Your ULO account has been created";
                            sSubjectForBA = "New ULO account has been created";
                        }

                        if (sUserSatus == "Inactive")
                        {
                            sSubject = "Your ULO account has been disabled";
                            sSubjectForBA = "The ULO reviewer account has been disabled";

                            sMessageText = sMessageHeader + "Your ULO account has been disabled on " + DateTime.Now.ToString();
                            sMessageTextForBA = sMessageHeader + "The ULO account has been disabled for " + txtFirstName.Text + " " + txtLastName.Text + " on " + DateTime.Now.ToString();

                        }
                    }
                    else // paswd mode -  changing just new password for ULO User who logged-in
                    {
                        //if (txtPassword.Text.Trim() == "")
                        //{
                        //    throw new Exception("Please enter your password");
                        //}
                        //else
                        //{
                        //    sMessageText = sMessageHeader + "Your ULO application password has been changed on " + DateTime.Now.ToString() + ".<br>Your new password is: " + txtPassword.Text + "<br><br>http://dotnetweb.pbsncr.gsa.gov/OpenItems";
                        //    sSubject = "Your ULO password has been changed";
                        //}
                    }


                    Admin.SendCustomEmail(txtEmail.Text, sMessageText, sSubject, out sSentToUser, out iCnt);

                    if (btnReset.Visible == true) // admin mode
                    {
                        Admin.SendCustomEmailToBDAdmins(sMessageTextForBA, sSubjectForBA, out sSentToAdmins, out iCnt);
                    }

                    lblError.Text = lblError.Text + ". " + DisplayMessage("Email message has been sent to the following ULO User(s):<br>" + sSentToUser + ", " + sSentToAdmins, false);
                }
                catch (Exception ex)
                {
                    lblError.Text = DisplayMessage(ex.Message, true);
                }


        }
        protected void rbListNewOld_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetFields();

            if (rbListNewOld.SelectedIndex == 0) // new user
            {
                FillControls(0);
                txtFirstName.SetReadOnly();
                txtLastName.SetReadOnly();
                txtMI.SetReadOnly();
                txtEmail.SetReadOnly();
                txtFirstName.ToolTip = "read-only field";
                txtLastName.ToolTip = "read-only field";
                txtMI.ToolTip = "optional field";
                txtEmail.ToolTip = "read-only field";
                tr_user.AddDisplay();
                //tr_user.Visible = true;
            }
            else if (rbListNewOld.SelectedIndex == 2) //existing user
            {
                FillControls(2);
                txtFirstName.SetReadOnly();
                txtLastName.SetReadOnly();
                txtMI.SetReadOnly();
                txtEmail.SetReadOnly();
                txtFirstName.ToolTip = "read-only field";
                txtLastName.ToolTip = "read-only field";
                txtMI.ToolTip = "read-only field";
                txtEmail.ToolTip = "read-only field";
                tr_user.AddDisplay();
                //tr_user.Visible = true;
            }
            else // "1" new non-NCR user
            {
                FillControls(1);
                txtFirstName.SetReadOnly(false);
                txtLastName.SetReadOnly(false);
                txtMI.SetReadOnly(false);
                txtEmail.SetReadOnly(false);
                txtFirstName.ToolTip = "required value";
                txtLastName.ToolTip = "required value";
                txtMI.ToolTip = "required value";
                txtEmail.ToolTip = "required value";
                tr_user.AddDisplayNone();
                //tr_user.Visible = false;
            }
            ddlUsers.SelectedIndex = -1;
            ddlOrg.SelectedIndex = -1;
            mgUserRole.SelectedIndex = -1;
        }
        protected string DisplayMessage(string sMessage, bool bError)
        {

            if (bError == true)
            {
                lblError.CssClass = "error";
            }
            else
            {
                lblError.CssClass = "blue_message";
            }
            return sMessage;
        }

        private string ValidateTextBox(TextBox oTextBox, string sFieldName, string sError)
        {
            if (oTextBox.Text.Trim() == "")
            {
                if (sError == "")
                {
                    sError = sFieldName ;
                }
                else
                {
                    sError = sError + ", " + sFieldName + "'";
                }
                //oTextBox.AddStyle("background-color:Yellow");
            }
            else
            {
                //oTextBox.AddStyle("background-color:white");
            }
            return sError;
        }

        public static bool IsValidEmailAddress(string sEmail)
        {
            if (sEmail == null)
            {
                return false;
            }

            var nFirstAT = sEmail.IndexOf('@');
            var nLastAT = sEmail.LastIndexOf('@');

            if ((nFirstAT > 0) && (nLastAT == nFirstAT) &&
            (nFirstAT < (sEmail.Length - 1)))
            {
                // address is ok regarding the single @ sign
               // return (Regex.IsMatch(sEmail, @"(\w+)@(\w+)\.(\w+)"));
                return (Regex.IsMatch(sEmail, @"(\w+)@gsa.gov"));
            }
            else
            {
                return false;
            }
        }

 



}
}
