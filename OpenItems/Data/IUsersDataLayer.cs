using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using GSA.OpenItems;
using OpenItems.Data;

/// <summary>
/// Summary description for IDataLayer
/// </summary>
///
namespace Data
{
    public interface IUsersDataLayer
    {
        //TODO: Change after db is fixed
        DataSet GetUserByUserEmail(string sEmail);
        
        //TODO: Change after db is fixed
        DataSet GetULOUserByUserEmail(string sUserEmail);
        
        //TODO: Change after db is fixed
        DataSet GetNCRUserByUserEmail(string sUserEmail);

        //TODO: Change after db is fixed
        DataSet GetUserByUserID(int iUserID);

        //TODO: Change after db is fixed
        DataSet SearchPersonnel(string sFirstName, string sLastName);

        //TODO: Change after db is fixed
        DataSet GetUsersByRole(UserRoles role);

        IEnumerable<spGetAllActiveInactiveUsers_Result> GetAllActiveInactiveUsers();

        //TODO: Change after db is fixed
        DataSet GetAllNCRUsers();
        DataSet GetUserRoleForFSOrg(int iCurrentUserID, string sBusinessLineCode, string sOrganization);

        int SaveUser(int iUserID, string sEmail, string sPassword, string sRoleCode, int iActive, string sFirstName,
            string sLastName, string sMiddleInitial, string sOrganization, string sPhone, string sDefaultApplication);
    }
}