using System.Collections.Generic;
using System.Linq;
using OpenItems.Data;

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
        public List<spGetOrgAndOrgCodeList_Result> GetAllOrganizations()
        {
            return Dal.GetOrgAndOrgCodeList().ToList();
        }

    }
}
