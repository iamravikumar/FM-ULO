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
    public interface IItemsDataLayer
    {
        //TODO: Change after db is fixed
        DataSet GetOIList(int iLoadID, string sOrganization, int iUserID);
        //TODO: Change after db is fixed
        DataSet GetBA53ItemsList(int iLoadID, string sOrganization, int iUserID);
        //TODO: Change after db is fixed
        DataSet SearchItems(int iLoadID, string sOrganization, string sDocNumber, string sProjNumber, string sBA,
            string sAwardNumber);
        IEnumerable<spGetOILinesForDeobligation_Result> GetItemsLinesToDeobligate(int iLoadID, string sOrganization);
    }
}