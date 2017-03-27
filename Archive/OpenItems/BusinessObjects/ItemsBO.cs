using System.Collections.Generic;
using System.Linq;
using OpenItems.Data;

namespace GSA.OpenItems.Web
{
    using System.Data;
    using Data;

    /// <summary>
    /// Summary description for ItemsBO
    /// </summary>
    public class ItemsBO
    {
        private readonly IItemsDataLayer Dal;

        public ItemsBO(IItemsDataLayer dal)
        {
            Dal = dal;
        }

        public DataSet GetItemsList(int iLoadID, string sOrganization, int iUserID)
        {
            return Dal.GetOIList(iLoadID, sOrganization, iUserID);
        }

        public DataSet GetBA53ItemsList(int iLoadID, string sOrganization, int iUserID)
        {
            return Dal.GetBA53ItemsList(iLoadID, sOrganization, iUserID);

        }

        public DataSet SearchItems(int iLoadID, string sOrganization, string sDocNumber, string sProjNumber, string sBA, string sAwardNumber)
        {
            return Dal.SearchItems(iLoadID, sOrganization, sDocNumber, sProjNumber, sBA, sAwardNumber);
        }

        public List<spGetOILinesForDeobligation_Result> GetItemsLinesToDeobligate(int iLoadID, string sOrganization)
        {
            return Dal.GetItemsLinesToDeobligate(iLoadID, sOrganization).ToList();
        }

    }
}