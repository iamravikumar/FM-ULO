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
    public interface IAdminDataLayer
    {
        IEnumerable<spGetLoadListNotArchived_Result> GetLoadListNotArchived();
        IEnumerable<spGetLoadListWithActiveDueDate_Result> GetLoadListWithActiveDueDate();
        IEnumerable<spGetLoadDetails_Result> GetLoadDetails(int iLoadID);
        DataSet GetAllAttachmentTypes();
        IEnumerable<spGetDefaultJustifications_Result> GetDefaultJustificationTypes();
        IEnumerable<spGetJustificationTypeByID_Result> GetJustificationTypeByID(int iID);
        DataSet GetPendingOItemsByUser(int iLoadID);
        DataSet GetUsersByRole(UserRoles role);
        DataSet GetPendingNAOItemsByOrg(int iLoadID);
        DataSet GetPendingOItems(int iLoadID);
        IEnumerable<spPendingOItemsByOrg_Result> GetPendingOItemsByOrg(int iLoadID);
        IEnumerable<spPendingDeobligateByOrg_Result> GetPendingDeobligateByOrg(int iLoadID);
        DataSet GetAllULOUsers();
        DataSet GetUserEmailByUserID(int iUserID);
        IEnumerable<string> GetOrgNameByOrgCode(string sOrgCode);
        IEnumerable<string> GetDocNumByItemID(int iItemID);


        int ArchiveLoad(int iLoadID);
        int UnarchiveLoad(int iLoadID);

        int SaveJustification(string sJustificationDescription, int iJustification, bool bDisplayAddOnField,
            string sAddOnDescription);
    }
}