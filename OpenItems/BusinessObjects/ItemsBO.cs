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

        public DataSet GetItemsLinesToDeobligate(int iLoadID, string sOrganization)
        {
            return Dal.GetItemsLinesToDeobligate(iLoadID, sOrganization);
        }

    }
}