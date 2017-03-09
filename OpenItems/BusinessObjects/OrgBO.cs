namespace GSA.OpenItems.Web
{
    using System.Data;
    using Data;


    public class OrgBO
    {
        private readonly IOrgDataLayer Dal;
        public OrgBO(IOrgDataLayer dal)
        {
            Dal = dal;
        }
        public DataSet GetAllOrganizations()
        {
            return Dal.GetOrgAndOrgCodeList();
        }

    }
}
