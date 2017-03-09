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
    public interface IItemsDataLayer
    {
        DataSet GetOIList(int iLoadID, string sOrganization, int iUserID);
        DataSet GetBA53ItemsList(int iLoadID, string sOrganization, int iUserID);

        DataSet SearchItems(int iLoadID, string sOrganization, string sDocNumber, string sProjNumber, string sBA,
            string sAwardNumber);

        DataSet GetItemsLinesToDeobligate(int iLoadID, string sOrganization);
    }
}