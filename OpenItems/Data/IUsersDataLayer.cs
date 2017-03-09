using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using GSA.OpenItems;

/// <summary>
/// Summary description for IDataLayer
/// </summary>
///
namespace Data
{
    public interface IUsersDataLayer
    {
        DataSet GetUserByUserEmail(string sEmail);
        DataSet GetULOUserByUserEmail(string sUserEmail);
        DataSet GetNCRUserByUserEmail(string sUserEmail);
        DataSet GetUserByUserID(int iUserID);
        DataSet SearchPersonnel(string sFirstName, string sLastName);
        DataSet GetUsersByRole(UserRoles role);
        DataSet GetAllActiveInactiveUsers();
        DataSet GetAllNCRUsers();
        DataSet GetUserRoleForFSOrg(int iCurrentUserID, string sBusinessLineCode, string sOrganization);

        void SaveUser(int iUserID, string sEmail, string sPassword, string sRoleCode, int iActive, string sFirstName,
            string sLastName, string sMiddleInitial, string sOrganization, string sPhone, string sDefaultApplication, out int iID);
    }
}