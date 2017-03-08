namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Configuration;
    using Data;


    /// <summary>
    /// Summary description for LineNumBO
    /// </summary>
    public class LineNumBO
    {

        private readonly ItemBO Item;
        private readonly ILineNumDataLayer Dal;
        public LineNumBO(ILineNumDataLayer dal, ItemBO item)
        {
            Dal = dal;
            Item = item;
        }

        public LineNum GetLineNum(int iOItemID, int iLineNum)
        {

            return (new LineNum(iOItemID, iLineNum));

        }

        public void UpdateDetails(int iOItemID, int iLoadID, int iLineN, string sULOOrgCode, int iValid, string sComments, int iJustification,
            string sJustificationAddOn, string sJustificationOther, string sCode, string sCodeComment, string sRWA, int iUpdateUser)
        {
            /*****************************************************************/
            //1. - update Line properties
            /*****************************************************************/
            var objLine = new LineNum(iOItemID, iLineN);
            var reviewer = objLine.ReviewerUserID;
            var line_prev_validation = objLine.Valid;
            objLine.Valid = iValid;
            objLine.Comments = sComments;
            objLine.JustificationCode = iJustification;
            objLine.JustificationAddOn = sJustificationAddOn;
            objLine.JustificationOther = sJustificationOther;
            objLine.Code = sCode;
            objLine.CodeComments = sCodeComment;
            objLine.RWA = sRWA;
            objLine.Save(iUpdateUser);

            iJustification = objLine.JustificationCode;

            //get Open Item details
            var objItem = new OpenItem(iOItemID, iLoadID, sULOOrgCode, reviewer);
            var document_valid = objItem.ValidCode;
            var item_status = objItem.StatusCode;
            var item_new_status = objItem.StatusCode;
            var item_orig_orgcode = objItem.OriginalOrgCode;
            var doc_number = objItem.DocNumber;

            //2. - insert History on update Line Properties
            // OR
            //3. - insert History on update Line Validation if it has been changed
            // (insert only one record to the History due to not duplicate records)
            if (line_prev_validation != iValid)
                History.InsertHistoryOnLineUpdate((int)HistoryActions.haLineValidationUpdate, iLoadID, iOItemID, item_orig_orgcode, sULOOrgCode,
                    doc_number, iLineN, item_status, iValid, sCode, sCodeComment, iJustification, sJustificationAddOn, sComments, reviewer, iUpdateUser);
            else
                History.InsertHistoryOnLineUpdate((int)HistoryActions.haLinePropertiesUpdate, iLoadID, iOItemID, item_orig_orgcode, sULOOrgCode,
                    doc_number, iLineN, item_status, line_prev_validation, sCode, sCodeComment, iJustification, sJustificationAddOn, sComments, reviewer, iUpdateUser);

            /*****************************************************************/
            //4. - update Item Status if necessary
            /*****************************************************************/
            item_new_status = Item.CalculateItemStatus(iOItemID, sULOOrgCode, reviewer);

            if (item_status != item_new_status)
            {
                Item.UpdateItemStatus(iOItemID, iLoadID, sULOOrgCode, reviewer, item_new_status);

                //5. - insert History on update Item Status
                History.InsertHistoryOnItemUpdate((int)HistoryActions.haItemPropertiesUpdate, iLoadID, iOItemID, item_orig_orgcode, sULOOrgCode,
                    doc_number, item_new_status, document_valid, null, reviewer, iUpdateUser);
            }

        }

        public DateTime CertifyDeobligation(int iOItemID, int iItemLNum)
        {
            var dtCertDate = Dal.CertifyDeobligation(iOItemID, iItemLNum);
            return (DateTime)Utility.GetNotNullValue(dtCertDate, "DateTime");
          
        }

        public bool LineOnReassignRequest(int iOItemID, int iLineNum, string sULOOrgCode, int iReviewerUserID)
        {
            return (bool)Dal.LineOnReassignRequest(iOItemID, iLineNum, sULOOrgCode, iReviewerUserID);
        }

        public bool AvailableForUpdate(bool bItemIsArchived, bool bIsFeedbackLoad, string sItemOrganization, string sLineOrganization,
                        bool bIsLineOnReassignRequest, int iItemReviewer, object oUser, string sUserOrganization, int UserID)
        {
            /* check following conditions:
             1) if the load is archived;
             * if it's feedback load;
             * if the item or this particular line is not waiting for reassign decision;

             2) if the user that requested the item has permission for update.

             * Short description of user's update permission:
             * Budget Division Administrator (RoleCode 99) 
             *          - has permission for all items in this load.
             * Organization (Service Center) Administrator (RoleCode 98)
             *          - has permission for all items with ULOOrgCode = related organization.
             * Reviewer (RoleCode 97)
             *          - has permission for all items in his own workload (where ReviewerUserID = current reviewer UserID)                 
             */

            if (bItemIsArchived) return false;

            if (bIsFeedbackLoad) return false;

            if (iItemReviewer == 0)
            {
                return false;
            }

            if (bIsLineOnReassignRequest) return false;

            if (((System.Security.Principal.IPrincipal)oUser).IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString())) return true;

            if (((System.Security.Principal.IPrincipal)oUser).IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) && sLineOrganization == sUserOrganization)
                return true;

            if (((System.Security.Principal.IPrincipal)oUser).IsInRole(((int)UserRoles.urReviewer).ToString())
                && iItemReviewer == UserID && sLineOrganization == sUserOrganization)
                return true;

            return false;

        }
    }
}