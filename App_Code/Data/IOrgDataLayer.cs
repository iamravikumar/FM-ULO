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
    public interface IOrgDataLayer
    {
        DataSet GetOrgAndOrgCodeList();
    }
}