namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Configuration;
    using Data;
    using global::OpenItems.Properties;

    /// <summary>
    /// Summary description for UsersBO
    /// </summary>
    public class UsersBO
    {
        private readonly IUsersDataLayer Dal;
        private readonly EmailsBO Email;
        public UsersBO(IUsersDataLayer dal, EmailsBO email)
        {
            Dal = dal;
            Email = email;
        }
        // this is new ULO authentication process - after LDAP implemented this authentication process is in use
        public void AuthenticateAfterLDAP(string sEmail, string sUserName, out string sUserID)
        {
            //var ds = GetUserByUserEmail(sEmail);
            //// Check whether the user exists or not
            ////ApplicationAssert.CheckCondition(ds.Tables[0].Rows.Count > 0, "You don't have the ULO account . Please contact your ULO administrator.");
            //if (ds.Tables[0].Rows.Count == 0)
            //{
            //    var strMessage = "The ENT Active Directory returned the following GSA email address: '" + sEmail + "'. There are no active users in the OpenItems database associated with this email address.";
            //    var sSysAdminEmail = Settings.Default.SysAdminEmail;
            //    var sULOSystemEmail = Settings.Default.MailSenderAddress;
            //    if (Settings.Default.SendEmailToSysAdmin)
            //    {
            //        Email.SendEmail(sSysAdminEmail, "", "ULO Authentication Error", strMessage, sULOSystemEmail);
            //    }
            //    throw new Exception("You don't have an account with the ULO system. Please contact your ULO administrator.");
            //}


            //var intUserID = (int)ds.Tables[0].Rows[0]["UserID"];

            //if (intUserID == 0 || intUserID == null)
            //{
                sUserID = "0";
            //}
            //else
            //{
            //    sUserID = intUserID.ToString();
            //}

            //var strUserFullName = (string)ds.Tables[0].Rows[0]["FirstName"] + " " + (string)ds.Tables[0].Rows[0]["LastName"];
            //var strUserOrganization = (string)ds.Tables[0].Rows[0]["Organization"];
            //var strRoles = (string)ds.Tables[0].Rows[0]["RoleCode"];
            //var bSysAdmin = (bool)ds.Tables[0].Rows[0]["SysAdmin"];
            //var sSysAdminRoleCode = Settings.Default.SysAdminRoleCode;

            //if (bSysAdmin == true)
            //{
                //strRoles = strRoles + "|" + sSysAdminRoleCode;
            //}
            //var defaultApplication = Int32.Parse(ds.Tables[0].Rows[0]["DefaultApplication"].ToString());



            var _pageBase = new PageBase();
            _pageBase.CurrentUserID = 1;
            _pageBase.CurrentUserName = "test test";
            _pageBase.CurrentUserLogin = sUserName;
            _pageBase.CurrentUserRoles = "|" + Settings.Default.SysAdminRoleCode;
            _pageBase.CurrentUserOrganization = "";
            _pageBase.CurrentUserDefaultApp = 0;
        }

        // this is old ULO authentication process - after LDAP implemented this authentication process is not in use


        public DataSet GetUserByUserEmail(string sEmail)
        {
            return Dal.GetUserByUserEmail(sEmail);
        }

        public DataSet GetULOUserByUserEmail(string sUserEmail)
        {
            return Dal.GetULOUserByUserEmail(sUserEmail);
        }

        public DataSet GetNCRUserByUserEmail(string sUserEmail)
        {
            return Dal.GetNCRUserByUserEmail(sUserEmail);
        }

        public DataSet GetUserByUserID(int iUserID)
        {
            return Dal.GetUserByUserID(iUserID);
        }

        public DataSet SearchPersonnel(string sFirstName, string sLastName)
        {
            return Dal.SearchPersonnel(sFirstName, sLastName);
        }

        public DataSet GetUsersByRole(string sRole)
        {
            var role = (UserRoles)Enum.Parse(typeof(UserRoles), sRole);
            return Dal.GetUsersByRole(role);
        }

        public DataSet GetAllULOUsers() // active and inactive
        {
            return Dal.GetAllActiveInactiveUsers();
        }

        public DataSet GetAllNCRUsers()
        {
            return Dal.GetAllNCRUsers();
        }

        public int UserAuthorizedForFSReports(int iCurrentUserID, string sBusinessLineCode, string sOrganization)
        {
            if (sBusinessLineCode == "" && sOrganization == "")
                return -1;

            var ds = Dal.GetUserRoleForFSOrg(iCurrentUserID, sBusinessLineCode, sOrganization);

            if (ds == null || ds.Tables[0] == null)
                return -1;

            if (ds.Tables[0].Rows.Count == 0)
                return 0;

            //request for rights for specific Organization:
            if (sOrganization != "")
                return (int)ds.Tables[0].Rows[0]["RoleCode"];

            //else : (BusinessLineCode not equal "")
            //request for rights for Business Line:
            else
            {
                var min_role_code = -1;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (dr["RoleCode"] == DBNull.Value)
                        min_role_code = 0;
                    else
                        if (min_role_code == -1 || min_role_code > (int)dr["RoleCode"])
                        min_role_code = (int)dr["RoleCode"];

                }
                return min_role_code;
            }
        }

        public void SaveUser(int iUserID, string sEmail, string sPassword, string sRoleCode, int iActive,
            string sFirstName, string sLastName, string sMiddleInitial, string sOrganization, string sPhone, string sDefaultApplication, out int iID)
        {
            Dal.SaveUser(iUserID, sEmail, sPassword, sRoleCode, iActive, sFirstName, sLastName, sMiddleInitial, sOrganization, sPhone, sDefaultApplication, out iID);
        }

    }
}