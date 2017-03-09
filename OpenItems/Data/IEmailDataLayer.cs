
using System.Data;
using GSA.OpenItems;

/// <summary>
/// Summary description for IDataLayer
/// </summary>
///
namespace Data
{
    public interface IEmailDataLayer
    {
        DataSet GetUserByUsername(string sUsername);
        int InsertEmailRequest(int iCurrentUserID , int iHistoryAction, bool bSendNow);
        int ActivateAssignEmailRequest(int iCurrentUserID, int iSelectedLoad);
    }
}