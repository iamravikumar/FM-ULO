using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for IDataLayer
/// </summary>
///
namespace Data
{
    public interface IDataLayer : IAdminDataLayer, IAssignDataLayer, IDocumentDataLayer, IEmailDataLayer, IFundAllowanceDataLayer, IFundStatusDataLayer, IItemDataLayer, IItemsDataLayer, ILineNumDataLayer, IOrgDataLayer, IReportDataLayer, IUploadServiceDataLayer, IUsersDataLayer, ILookupDataLayer
    { }
}