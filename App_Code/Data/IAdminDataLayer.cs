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
    public interface IAdminDataLayer
    {
        DataSet GetLoadListNotArchived();
        DataSet GetLoadListWithActiveDueDate();
        DataSet GetLoadDetails(int iLoadID);
        DataSet GetAllAttachmentTypes();
        DataSet GetDefaultJustificationTypes();
        DataSet GetJustificationTypeByID(int iID);
        DataSet GetPendingOItemsByUser(int iLoadID);
        DataSet GetUsersByRole(UserRoles role);
        DataSet GetPendingNAOItemsByOrg(int iLoadID);
        DataSet GetPendingOItems(int iLoadID);
        DataSet GetPendingOItemsByOrg(int iLoadID);
        DataSet GetPendingDeobligateByOrg(int iLoadID);
        DataSet GetAllULOUsers();
        DataSet GetUserEmailByUserID(int iUserID);
        DataSet GetOrgNameByOrgCode(string sOrgCode);
        DataSet GetDocNumByItemID(int iItemID);


        void ArchiveLoad(string sLoadID);
        void UnarchiveLoad(string sLoadID);

        void SaveJustification(string sJustificationDescription, int iJustification, bool bDisplayAddOnField,
            string sAddOnDescription, out int iID);
    }
}