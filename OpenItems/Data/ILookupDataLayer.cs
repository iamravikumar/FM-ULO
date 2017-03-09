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
    public interface ILookupDataLayer
    {
        IEnumerable<spGetOpenItemsTypes_Result> GetOpenItemsTypes();
        IEnumerable<spGetDataSourceTypes_Result> GetDataSourceTypes();
        IEnumerable<spGetBA53AccrualTypes_Result> GetBA53AccrualTypes();
        IEnumerable<spGetBA53AccrualTypeActions_Result> GetBA53AccrualTypeActions(int iAccrualTypeCode);
        IEnumerable<spGetLoadList_Result> GetLoadList();
        IEnumerable<string> GetOrganizationsList();
        IEnumerable<spGetJustifications_Result> GetJustifications();
        IEnumerable<spGetDefaultJustifications_Result> GetDefaultJustifications();
        IEnumerable<spGetActiveCodeList_Result> GetActiveCodeList();
        IEnumerable<spGetCodeList_Result> GetCodeList();
        IEnumerable<spGetValidationValues_Result> GetValidationValues();
        IEnumerable<spGetContactsRoles_Result> GetContactsRoles();
        IEnumerable<spGetWholeOrgList_Result> GetWholeOrgList();
    }
}