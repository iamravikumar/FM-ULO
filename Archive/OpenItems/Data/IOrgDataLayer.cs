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
    public interface IOrgDataLayer
    {
        IEnumerable<spGetOrgAndOrgCodeList_Result> GetOrgAndOrgCodeList();
    }
}