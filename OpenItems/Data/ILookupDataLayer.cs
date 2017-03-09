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
    public interface ILookupDataLayer
    {
        DataSet GetOpenItemsTypes();
        DataSet GetDataSourceTypes();
        DataSet GetBA53AccrualTypes();
        DataSet GetBA53AccrualTypeActions(int iAccrualTypeCode);
        DataSet GetLoadList();
        DataSet GetOrganizationsList();
        DataSet GetJustifications();
        DataSet GetDefaultJustifications();
        DataSet GetActiveCodeList();
        DataSet GetCodeList();
        DataSet GetValidationValues();
        DataSet GetContactsRoles();
        DataSet GetWholeOrgList();
    }
}