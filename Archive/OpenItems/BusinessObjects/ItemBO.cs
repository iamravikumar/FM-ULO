using System.Collections.Generic;
using System.Linq;
using OpenItems.Data;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using Data;

    /// <summary>
    /// Summary description for ItemBO
    /// </summary>
    public class ItemBO
    {
        private readonly IItemDataLayer Dal;
        public ItemBO(IItemDataLayer dal)
        {
            Dal = dal;
        }
        public OpenItem GetOpenItem(int OItemID, int LoadID, string ULOOrgCode, int ReviewerUserID)
        {

            return (new OpenItem(OItemID, LoadID, ULOOrgCode, ReviewerUserID));

        }

        public DataSet GetItemContactList(string sDocNumber, string ULOOrgCode)
        {
            return Dal.GetItemContacts(sDocNumber);
        }
        public List<string> GetDocNumByItemID(int iOItemID)
        {
            return Dal.GetDocNumByItemID(iOItemID).ToList();
        }

        public List<spGetOIFeedbackDetails_Result> GetFeedbackRecords(int iOItemID)
        {
            return Dal.GetOIFeedbackDetails(iOItemID).ToList();
        }

        public void SaveFeedbackResponse(int iOItemID, string sDocNumber, int iLoadID, int iValid, string sResponse,
            decimal dUDOShouldBe, decimal dDOShouldBe)
        {
            Dal.UpdateFeedback(iOItemID, sDocNumber, iLoadID, iValid, sResponse, dUDOShouldBe, dDOShouldBe);

        }

        public int AddDocumentContact(string sDocNumber, string ULOOrgCode, int iPersonnelID, string sRoleDesc)
        {
            return Dal.AddDocumentContact(sDocNumber, iPersonnelID, sRoleDesc);
        }

        public int DeleteDocContact(string sDocNumber, string ULOOrgCode, string sRoleDesc, string sFirstName, string sLastName)
        {
            return Dal.DeleteDocumentContact(sDocNumber, sRoleDesc, sFirstName, sLastName);
        }

        public int CalculateItemStatus(int iOItemID, string sULOOrgCode, int iReviewerUserID)
        {
            return Dal.CalculateItemStatus(iOItemID, sULOOrgCode, iReviewerUserID);
        }

        public void UpdateItemStatus(int iOItemID, int iLoadID, string sULOOrgCode, int iReviewerUserID, int iStatusCode)
        {
            Dal.UpdateItemStatus(iOItemID, iLoadID,sULOOrgCode, iReviewerUserID, iStatusCode);
        }

        public bool UpdateItemProperties(int iOItemID, int iLoadID, string sULOOrgCode, string sUDOShouldBe, string sDOShouldBe,
            DateTime dtExpCompDate, string sComments, int iReviewerUserID, int iUpdateUser)
        {
            var update_grid_view_flag = false;

            var objItem = new OpenItem(iOItemID, iLoadID, sULOOrgCode, iReviewerUserID);
            //int load_id = objItem.LoadID;
            var status = objItem.StatusCode;
            var valid = objItem.ValidCode;
            var orig_orgcode = objItem.OriginalOrgCode;
            var doc_number = objItem.DocNumber;

            //first, check if changes have been made :
            //------------------------------------------
            if ((sDOShouldBe.Trim().Length > 0 && sUDOShouldBe.Trim() != "$" && sUDOShouldBe != objItem.UDOShouldBe) ||
                (sDOShouldBe.Trim().Length > 0 && sDOShouldBe.Trim() != "$" && sDOShouldBe != objItem.DOShouldBe) ||
                (dtExpCompDate != DateTime.MinValue && dtExpCompDate != objItem.ExpectedCompletionDate) ||
                objItem.Comments.Trim() != sComments.Trim())
            {
                Dal.UpdateItemProperties(iOItemID, sULOOrgCode, sUDOShouldBe, sDOShouldBe, dtExpCompDate, sComments);

                var new_status = CalculateItemStatus(iOItemID, sULOOrgCode, iReviewerUserID);

                if (new_status == (int)OpenItemStatus.stNewItem || new_status == (int)OpenItemStatus.stAssigned)
                {
                    new_status = (int)OpenItemStatus.stInProcess;
                }
                //if item status should be changed, update items grid view:                    
                if (new_status != status)
                {
                    UpdateItemStatus(iOItemID, iLoadID, sULOOrgCode, iReviewerUserID, status);
                    update_grid_view_flag = true;
                }

                //insert History for Item properties update:
                History.InsertHistoryOnItemUpdate((int)HistoryActions.haItemPropertiesUpdate, iLoadID, iOItemID, orig_orgcode,
                    sULOOrgCode, doc_number, status, valid, sComments, iReviewerUserID, iUpdateUser);
            }

            return update_grid_view_flag;

        }

        public DataSet GetULOOrganizationsByItemLines(int OItemID)
        {
            return GetLinesOrgCodes(OItemID, "");

        }

        public DataSet GetULOOrganizationsByItemLines(int iOItemID, string sLines)
        {

            return GetLinesOrgCodes(iOItemID, sLines);

        }

        private DataSet GetLinesOrgCodes(int iOItemID, string sLines)
        {
            return Dal.GetLinesOrgCodes(iOItemID, sLines);
        }

        public bool AvailableForUpdate(bool ItemIsArchived, bool IsFeedbackLoad, string ItemOrganization, int ItemReviewer, object User, string UserOrganization, int UserID)
        {

            /* check following conditions:
             1) if the load is archived;
             2) if the user that requested the item has permission for update.

             * Short description of user's update permission:
             * Budget Division Administrator (RoleCode 99) 
             *          - has permission for all items in this load.
             * Organization (Service Center) Administrator (RoleCode 98)
             *          - has permission for all items with ULOOrgCode = related organization.
             * Reviewer (RoleCode 97)
             *          - has permission for all items in his own workload (where ReviewerUserID = current reviewer UserID)                 
             */

            if (ItemIsArchived)
            {
                return false;
            }

            if (IsFeedbackLoad)
            {
                return false;
            }

            if (ItemReviewer == 0)
            {
                return false;
            }

            if (((System.Security.Principal.IPrincipal)User).IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
            {
                return true;
            }

            if (((System.Security.Principal.IPrincipal)User).IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) && ItemOrganization == UserOrganization)
            {
                return true;
            }

            if (((System.Security.Principal.IPrincipal)User).IsInRole(((int)UserRoles.urReviewer).ToString()) && ItemReviewer == UserID)
            {
                return true;
            }

            return false;

        }
    }
}