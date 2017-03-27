using System.Linq;
using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Xml;
    using System.Configuration;

    public partial class HTTPServer : PageBase
    {
        private readonly AdminBO Admin;
        private readonly AssignBO Assign;
        private readonly DocumentBO Document;
        private readonly EmailsBO Emails;
        private readonly ItemBO Item;
        private readonly LineNumBO LineNum;
        private readonly UsersBO Users;
        private readonly Lookups Lookups;
        public HTTPServer()
        {
            Admin = new AdminBO(this.Dal);
            Item = new ItemBO(this.Dal);
            Assign = new AssignBO(this.Dal, Item);
            Document = new DocumentBO(this.Dal);
            Emails = new EmailsBO(this.Dal);
            LineNum = new LineNumBO(this.Dal, Item);
            Users = new UsersBO(this.Dal, Emails);
            Lookups = new Lookups(this.Dal, Admin);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            var str_result = "";
            var _page = new PageBase();
            var user = _page.CurrentUserID;
            var intLoadID = _page.LoadID;

            var obj_xml = new XmlDataDocument();

            try
            {
                obj_xml.Load(Request.InputStream);                

                if (obj_xml.DocumentElement.Name == "verify_assignment") //action code 4
                {
                    var intOItemID = 0;
                    var intReviewerUserID = 0;
                    var strOrgCode = "";
                    var email_request = 0;

                    var node = obj_xml.DocumentElement.FirstChild;

                    var item_id = node.Attributes.GetNamedItem("item_id").Value;
                    if (item_id.Length > 0)
                        intOItemID = Int32.Parse(item_id);
                    strOrgCode = node.Attributes.GetNamedItem("org_code").Value;
                    var reviewer_id = node.Attributes.GetNamedItem("reviewer_id").Value;
                    if (reviewer_id.Length > 0)
                        intReviewerUserID = Int32.Parse(reviewer_id);

                    Assign.VerifyAssignment(intOItemID, intLoadID, strOrgCode, intReviewerUserID);

                    // action code 4
                    if (Settings.Default.SendEmailOnAssignVerification)
                        email_request = Emails.InsertEmailRequest(user, (int)HistoryActions.haAssignmentVerification, false);

                    History.InsertHistoryOnReasssignment((int)HistoryActions.haAssignmentVerification, intOItemID, intLoadID, strOrgCode, "", 0, intReviewerUserID, "", user, email_request);

                    SendEmailOnReassigRequest(intOItemID, strOrgCode, "", "", "", intReviewerUserID, 0, "", "4");


                    //we made update of data displayed in the items grid, so we will need to reload it:
                    _page.ItemsDataView = null;
                }

                if (obj_xml.DocumentElement.Name == "verify_reroute") //action 10 
                {                    
                    var intRequestID = 0;
                    var strPrevOrganization = "";
                    var intPrevReviewerUserID = 0;
                    var intNewReviewerUserID = 0;
                    var email_request = 0;

                    var node = obj_xml.DocumentElement.FirstChild;
                    
                    var request_id = node.Attributes.GetNamedItem("request_id").Value;
                    if (request_id.Length > 0)
                        intRequestID = Int32.Parse(request_id);
                    var user_id = node.Attributes.GetNamedItem("prev_user").Value;
                    if (user_id.Length > 0)
                        intPrevReviewerUserID = Int32.Parse(user_id);
                    strPrevOrganization = node.Attributes.GetNamedItem("prev_org").Value;
                    var new_user_id = node.Attributes.GetNamedItem("new_reviewer").Value;
                    if (new_user_id.Length > 0)
                        intNewReviewerUserID = Int32.Parse(new_user_id);

                    var reroute_result = Assign.VerifyReroute(intRequestID);
                    /*
                    return code 0 - Item was not rerouted (missed case?)
                    **************************************************************
                    LineNum = 0  --> all lines belong to the item (same ULOOrgCode + ReviewerUserID) were rerouted:
                    return code 1 - new item (ItemID + NewULOOrgCode + NewReviewerUserID) already exist, lines from original item joined to the new item,
				                    original item has been deleted (only if original and new items are not the same)
                    return code 2 - new item (ItemID + NewULOOrgCode + NewReviewerUserID) doesn't exist, so original item has been updated to be the new one
                    **************************************************************
                    reroute specific LineNum, there are other Lines in the original item that should not be rerouted --> split item:
                    -------------------------
                    return code 3 - new item (ItemID + NewULOOrgCode + NewReviewerUserID) already exist, requested Line from original item joined to the new item
                    return code 4 - new item (ItemID + NewULOOrgCode + NewReviewerUserID) doesn't exist, requested Line from original item become new item 
				                    (insert new record into tblOIOrganization)
                    ***************************************************************
                    reroute specific LineNum, there are NO other Lines in the original item that should not be rerouted --> original item should be deleted or updated:
                    -------------------------
                    return code 5 - new item (ItemID + NewULOOrgCode + NewReviewerUserID) already exist, requested Line from original item joined to the new item,
				                    original item has been deleted (only if original and new items are not the same)
                    return code 6 - new item (ItemID + NewULOOrgCode + NewReviewerUserID) doesn't exist, original item has been updated to be the new one
                    */
                    if (reroute_result > 0 && reroute_result < 7)
                    {
                        // action code 10
                        if (Settings.Default.SendEmailOnRerouteAssign)  // 10
                            email_request = Emails.InsertEmailRequest(user, (int)HistoryActions.haRerouteAssignment, true);

                        History.InsertHistoryOnRerouteVerification(intRequestID, strPrevOrganization, intPrevReviewerUserID, user, email_request);

                        SendEmailOnReassigRequest(0, "", "", strPrevOrganization, "", intPrevReviewerUserID, intNewReviewerUserID, "", "10"); //////////

                        //update of data displayed in the items grid has been made, so we will need to reload it:
                        _page.ItemsDataView = null;
                    }
                    else
                    {
                        //error on reassign
                        str_result = "<error><err_msg msg='There is a problem to reroute this item. \nPlease contact System Administrator.' /></error>";
                    }
                }




                if (obj_xml.DocumentElement.Name == "add_contact")
                {
                    var strItem = "";
                    var intOItemID = 0;
                    var strDocNumber = "";
                    var strOrgCode = "";
                    var intPersonnelID = 0;
                    var strRoleDesc = "";
                    var strContactName = "";

                    var node = obj_xml.DocumentElement.FirstChild;

                    var pid = node.Attributes.GetNamedItem("pid").Value;
                    if (pid.Length > 0)
                        intPersonnelID = Int32.Parse(pid);
                    strItem = node.Attributes.GetNamedItem("item").Value;
                    if (strItem.Length > 0)
                        intOItemID = Int32.Parse(strItem);
                    strDocNumber = node.Attributes.GetNamedItem("doc").Value;
                    strOrgCode = node.Attributes.GetNamedItem("org").Value;
                    strRoleDesc = node.Attributes.GetNamedItem("role").Value;
                    strContactName = node.Attributes.GetNamedItem("name").Value;

                    var intContactUserID = Item.AddDocumentContact(strDocNumber, strOrgCode, intPersonnelID, strRoleDesc);

                    History.InsertHistoryOnAddItemContact(strDocNumber, intOItemID, intLoadID, strOrgCode, intContactUserID, strContactName, strRoleDesc, user);
                }

                if (obj_xml.DocumentElement.Name == "delete_contact")
                {
                    var strDocNumber = "";
                    var strOrgCode = "";
                    var strItem = "";
                    var intOItemID = 0;
                    var strContactName = "";
                    var strRoleDesc = "";

                    var node = obj_xml.DocumentElement.FirstChild;

                    strDocNumber = node.Attributes.GetNamedItem("doc").Value;
                    strOrgCode = node.Attributes.GetNamedItem("org").Value;
                    strItem = node.Attributes.GetNamedItem("item").Value;
                    if (strItem.Length > 0)
                        intOItemID = Int32.Parse(strItem);
                    strRoleDesc = node.Attributes.GetNamedItem("role").Value;
                    strContactName = node.Attributes.GetNamedItem("name").Value;

                    string strFirstName;
                    string strLastName;

                    strLastName = strContactName.Substring(0, strContactName.IndexOf(","));
                    strFirstName = strContactName.Substring(strContactName.IndexOf(",") + 2);

                    var intContactUserID = Item.DeleteDocContact(strDocNumber, strOrgCode, strRoleDesc, strFirstName, strLastName);

                    History.InsertHistoryOnDeleteItemContact(strDocNumber, intOItemID, intLoadID, strOrgCode, intContactUserID, strContactName, strRoleDesc, user);
                }

                if (obj_xml.DocumentElement.Name == "certify_deobl")
                {
                    var node = obj_xml.DocumentElement.FirstChild;

                    var intOItemID = Int32.Parse(node.Attributes.GetNamedItem("item_id").Value);
                    var intItemLN = Int32.Parse(node.Attributes.GetNamedItem("line_num").Value);
                    var strOrgCode = node.Attributes.GetNamedItem("org").Value;
                    var cert_date = LineNum.CertifyDeobligation(intOItemID, intItemLN);
                    if (cert_date != DateTime.MinValue)
                        str_result = "<result_date>" + cert_date.ToString("MMM dd, yyyy") + "</result_date>";
                    else
                        str_result = "<result_date></result_date>";

                    //insert History:
                    History.InsertHistoryOnLineDeobligation(intOItemID, intLoadID, intItemLN, strOrgCode, cert_date, user);
                    
                    //we made update of data displayed in the items grid, so we will need to reload it:
                    _page.ItemsDataView = null;
                }

                if (obj_xml.DocumentElement.Name == "reassign_request")  //5
                {                    
                    var node = obj_xml.DocumentElement.FirstChild;

                    var strItemID = node.Attributes.GetNamedItem("item").Value;
                    var strLines = node.Attributes.GetNamedItem("lines").Value;
                    var strOrgCode = node.Attributes.GetNamedItem("org_code").Value;
                    var strOrgNewValue = node.Attributes.GetNamedItem("org_new_value").Value;                                        
                    var strUserId = node.Attributes.GetNamedItem("user_id").Value;
                    var strComments = node.Attributes.GetNamedItem("com").Value;
                    //string strDocNum = node.Attributes.GetNamedItem("doc_num").Value;

                    var strResponsibleOrganization = Lookups.GetOrganizationByOrgCode(strOrgCode);

                    var strNewOrganization = "";
                    var strNewOrgCode = "";
                    if (strOrgNewValue.Length > 0)
                    {
                        strNewOrganization = strOrgNewValue.Substring(0, strOrgNewValue.IndexOf(":") - 1);
                        strNewOrgCode = strOrgNewValue.Substring(strOrgNewValue.IndexOf(":") + 2);
                    }
                    var intItemID = Int32.Parse(strItemID);
                    var intUserID = 0;
                    if (strUserId.Length > 0)
                        intUserID = Int32.Parse(strUserId);
                    if (strLines == "")
                        strLines = "0";

                    // ***SM
                    var intRequestID = Assign.RequestReassignItem(intItemID, strOrgCode, strLines, user, strResponsibleOrganization, 
                        strNewOrganization, strNewOrgCode, intUserID, strComments);

                    if (intRequestID == 0)
                    {
                        str_result = "<error><err_msg msg='This Open Item has been already requested for reroute. \nPlease refresh your items view.' /></error>";
                    }
                    else
                    {
                        var email_request = 0;
                        if (Settings.Default.SendEmailOnRerouteRequest)
                            email_request = Emails.InsertEmailRequest(user, (int)HistoryActions.haRerouteRequest, true);

                        History.InsertHistoryOnRerouteRequest(intItemID, strLines, intLoadID, strOrgCode, intRequestID, strOrgNewValue,
                            user, intUserID, strComments, user, email_request);
                    }

                    //refresh Items GridResults on the next reload:
                    _page.ItemsDataView = null;


                    SendEmailOnReassigRequest(intItemID, strOrgCode, strNewOrgCode, strResponsibleOrganization, strNewOrganization, user, intUserID, strComments,"5");
                }

                if (obj_xml.DocumentElement.Name == "reassign_reroute") //3
                {
                    var node = obj_xml.DocumentElement.FirstChild;

                    var strItemID = node.Attributes.GetNamedItem("item").Value;
                    var strOrgCode = node.Attributes.GetNamedItem("org_code").Value;
                    var strLines = node.Attributes.GetNamedItem("lines").Value;
                    var strOrgNewValue = node.Attributes.GetNamedItem("org_new_value").Value;
                    var strAction = node.Attributes.GetNamedItem("action").Value;
                    var strPrevUserId = node.Attributes.GetNamedItem("prev_user_id").Value;
                    var strUserId = node.Attributes.GetNamedItem("user_id").Value;
                    var strComments = node.Attributes.GetNamedItem("com").Value;
                    var strRequestId = node.Attributes.GetNamedItem("request").Value;
                    var strResponsibleOrganization = "";

                    var strNewOrganization = "";
                    var strNewOrgCode = "";
                    var intUserID = 0;
                    var intPreviousReviewerID = 0;
                    var email_request = 0;
                    var intRequestID = 0;

                    if (strUserId.Length > 0)
                        intUserID = Int32.Parse(strUserId);
                    if (strPrevUserId.Length > 0)
                        intPreviousReviewerID = Int32.Parse(strPrevUserId);
                    
                    if (strOrgNewValue.Length > 0)
                    {
                        strNewOrganization = strOrgNewValue.Substring(0, strOrgNewValue.IndexOf(":") - 1);
                        strNewOrgCode = strOrgNewValue.Substring(strOrgNewValue.IndexOf(":") + 2);
                    }
                    var intActionCode = Int32.Parse(strAction);
                    var intItemID = Int32.Parse(strItemID);

                    if (strRequestId != "")
                        intRequestID = Int32.Parse(strRequestId);

                    if (intActionCode == (int)HistoryActions.haReviewerAssignment)
                    {
                        //in the case of reviewer reassignment within the same organization,
                        //instead of this:
                        //AssignBO.ReassignItem(intItemID, strOrgCode, intPreviousReviewerID, intUserID, strComments);
                        // we will use direct reroute for item (or specific lines) within the same organization:
                        strResponsibleOrganization = Lookups.GetOrganizationByOrgCode(strOrgCode);

                        /*****************************************************************************************************/
                        //ADD LOG FOR WATCHING RECORDS UPDATE IN THE tblOIOrganization !!!!!!!!!!!
                        /*****************************************************************************************************/
                        var dt = Assign.RerouteItemDirectly(intItemID, strLines, intPreviousReviewerID, strOrgCode, strResponsibleOrganization, strOrgCode, intUserID, strComments);


                        //if we don't have previous Reviewer - it is first time Reviewer Assignment - 
                        //we don't need to send immidiately notification email;
                        //if we do have previous Reviewer - it is Re-Assignment within the same organization,
                        //we should send notification email to user immidiately:
                        if (intPreviousReviewerID == 0)
                        {
                            if (Settings.Default.SendEmailOnReviewerAssignment)
                                email_request = Emails.InsertEmailRequest(user, (int)HistoryActions.haReviewerAssignment, false);
                            // action code 3
                            History.InsertHistoryOnReasssignment((int)HistoryActions.haReviewerAssignment, intItemID, intLoadID, strOrgCode, strLines,
                                intPreviousReviewerID, intUserID, strComments, user, email_request);
                        }
                        else
                        {
                            if (Settings.Default.SendEmailOnReviewerReassign)
                                email_request = Emails.InsertEmailRequest(user, (int)HistoryActions.haReviewerReassignment, true);

                            History.InsertHistoryOnReasssignment((int)HistoryActions.haReviewerReassignment, intItemID, intLoadID, strOrgCode, strLines,
                                intPreviousReviewerID, intUserID, strComments, user, email_request);
                        }

                        //in the case of reroute request by Reviewer, the item has been already reassigned (code above),
                        //we need to cancel reroute request in the tblReassignRequest table:
                        if (intRequestID != 0)
                            Assign.CancelRerouteRequest(intRequestID);

                    }
                    else
                        if (intActionCode == (int)HistoryActions.haRerouteRequest)
                        {
                            //in the case of reroute request by Reviewer, 
                            //we need to cancel previous reroute request in the tblReassignRequest table,
                            //and then to create new reroute request for BD:
                            if (intRequestID != 0)
                                Assign.CancelRerouteRequest(intRequestID);

                            intRequestID = Assign.RequestReassignItem(intItemID, strOrgCode, strLines, intPreviousReviewerID, OIConstants.OpenItemsGridFilter_BDResponsibility,
                                                                    strNewOrganization, strNewOrgCode, intUserID, strComments);

                            if (intRequestID == 0)
                            {
                                str_result = "<error><err_msg msg='This Open Item has been already requested for reroute. \nPlease refresh your items view.' /></error>";
                            }
                            else
                            {
                                //send email only if current logged on user is not "BD Admin"
                                if (!User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) &&
                                    Settings.Default.SendEmailOnRerouteRequest)
                                    email_request = Emails.InsertEmailRequest(user, (int)HistoryActions.haRerouteRequest, true);

                                History.InsertHistoryOnRerouteRequest(intItemID, strLines, intLoadID, strOrgCode, intRequestID, strOrgNewValue,
                                    intPreviousReviewerID, intUserID, strComments, user, email_request);
                            }
                        }

                    SendEmailOnReassigRequest(intItemID, strOrgCode, strNewOrgCode, strResponsibleOrganization, strNewOrganization, user, intUserID, strComments,"4");

                    
                    //refresh Items GridResults on the next reload:
                    _page.ItemsDataView = null;

                }

                if (obj_xml.DocumentElement.Name == "group_reroute") //action 16
                {
                    var node = obj_xml.DocumentElement.FirstChild;
                    var strLoadID = "0";

                    var strItemsArray = node.Attributes.GetNamedItem("items").Value;
                    var strOrgNewValue = node.Attributes.GetNamedItem("org_new_value").Value;
                    var strUserID = node.Attributes.GetNamedItem("user").Value;
                    var strComments = node.Attributes.GetNamedItem("com").Value;

                    var strNewOrganization = "";
                    var strNewOrgCode = "";                    
                    var intUserID = 0;                    
                    var email_request = 0;

                    if (strUserID.Length > 0)
                        intUserID = Int32.Parse(strUserID);

                    if (strOrgNewValue.Length > 0)
                    {
                        strNewOrganization = strOrgNewValue.Substring(0, strOrgNewValue.IndexOf(":") - 1);
                        strNewOrgCode = strOrgNewValue.Substring(strOrgNewValue.IndexOf(":") + 2);
                    }                    

                    if (strItemsArray.Substring(strItemsArray.Length - 1) == ",")
                        strItemsArray = strItemsArray.Substring(0, strItemsArray.Length - 1);

                    var arr = strItemsArray.Split(new char[] { ',' });
                    string item_id;
                    string line_num;
                    DataTable dt;
                    LineNum line_obj = null;

                    foreach (var arr_item in arr)
                    {
                        if (arr_item != "")
                        {
                            item_id = arr_item.Substring(0, arr_item.IndexOf("_")).Trim();
                            line_num = arr_item.Substring(arr_item.IndexOf("_") + 1).Trim();

                            line_obj = LineNum.GetLineNum(Int32.Parse(item_id), Int32.Parse(line_num));

                            if (intLoadID == 0)
                            {
                                strLoadID = Admin.GetLoadIDByItemID(line_obj.OItemID);
                                if (strLoadID != null && strLoadID.Trim() != "" && strLoadID != "0")
                                {
                                    intLoadID = Convert.ToInt32(strLoadID);
                                }
                            }
                            
                            if (strNewOrganization == "" || line_obj.Organization == strNewOrganization)
                            {
                                //reassignment within the same organization: // corrected by Foram
                                dt = Assign.RerouteItemDirectly(line_obj.OItemID, line_num, line_obj.ReviewerUserID, line_obj.ULOOrgCode, line_obj.Organization, strNewOrgCode, intUserID, strComments);

                                // //action code 16
                                History.InsertHistoryOnReasssignment((int)HistoryActions.haReviewerReassignment, line_obj.OItemID, intLoadID, line_obj.ULOOrgCode, line_num,
                                     line_obj.ReviewerUserID, intUserID, strComments, user, 0);
                            }
                            else
                            {
                                //reroute to an other organization:
                                dt = Assign.RerouteItemDirectly(line_obj.OItemID, line_num, line_obj.ReviewerUserID, line_obj.ULOOrgCode, strNewOrganization, strNewOrgCode, intUserID, strComments);

                                History.InsertHistoryOnRerouteReassign(line_obj.OItemID, dt, line_num, intLoadID, line_obj.DocNumber, line_obj.ULOOrgCode,
                                    line_obj.Organization, line_obj.ReviewerUserID, strNewOrgCode, strOrgNewValue, intUserID, strComments, user, 0);
                            }
                        }
                    }
                    if (strNewOrganization == "" && line_obj != null)
                    {
                        strNewOrganization = line_obj.Organization;
                        strNewOrgCode = line_obj.ULOOrgCode;
                    }

                    //insert history for group assignment - for email details,
                    //in the case of group assignment one email only will be sent to the user
                    if (Settings.Default.SendEmailOnGroupAssign)
                    {
                        // action code 19
                        email_request = Emails.InsertEmailRequest(user, (int)HistoryActions.haGroupAssignment, true);
                        //SendEmailOnReassigRequest(intOItemID, strCurrentOrgCode, strNewOrgCode, strCurrentOrganization, strNewOrganization, user, intUserID, strComments, "19");

                    }

                    History.InsertHistoryOnGroupAssign(intLoadID, intUserID, strNewOrganization, strNewOrgCode, strComments, user, email_request);

                    SendEmailOnReassigRequest(0, line_obj.ULOOrgCode, strNewOrgCode, line_obj.Organization, strNewOrganization, user, intUserID, strComments, "16");

                    
                    //refresh Items GridResults on the next reload:
                    _page.ItemsDataView = null;                    
                }

                if (obj_xml.DocumentElement.Name == "reroute") //10
                {
                    var node = obj_xml.DocumentElement.FirstChild;

                    var strRequestID = node.Attributes.GetNamedItem("request_id").Value;
                    var strOItemID = node.Attributes.GetNamedItem("item_id").Value;
                    var strLines = node.Attributes.GetNamedItem("lines").Value;
                    var strDocNum = node.Attributes.GetNamedItem("doc_num").Value;
                    var strPrevOrganization = node.Attributes.GetNamedItem("prev_org").Value;
                    var strPrevReviewer = node.Attributes.GetNamedItem("prev_user").Value;
                    var strOrgNewValue = node.Attributes.GetNamedItem("org_new_value").Value;
                    var strUserID = node.Attributes.GetNamedItem("user").Value;
                    var strComments = node.Attributes.GetNamedItem("com").Value;
                    
                    var strNewOrganization = "";
                    var strNewOrgCode = "";
                    var strCurrentOrganization = "";
                    var strCurrentOrgCode = "";
                    var intOItemID = 0;
                    var intUserID = 0;
                    var intRequestID = 0;
                    var intPrevUserID = 0;
                    var email_request = 0;

                    if (strOItemID.Length > 0)
                        intOItemID = Int32.Parse(strOItemID);
                    if (strRequestID.Length > 0)
                        intRequestID = Int32.Parse(strRequestID);
                    if (strUserID.Length > 0)
                        intUserID = Int32.Parse(strUserID);
                    if (strPrevReviewer.Length > 0)
                        intPrevUserID = Int32.Parse(strPrevReviewer);
                    
                    if (strPrevOrganization.Length > 0)
                    {
                        strCurrentOrganization = strPrevOrganization.Substring(0, strPrevOrganization.IndexOf(":") - 1);
                        strCurrentOrgCode = strPrevOrganization.Substring(strPrevOrganization.IndexOf(":") + 2);
                    }
                    if (strOrgNewValue.Length > 0)
                    {
                        strNewOrganization = strOrgNewValue.Substring(0, strOrgNewValue.IndexOf(":") - 1);
                        strNewOrgCode = strOrgNewValue.Substring(strOrgNewValue.IndexOf(":") + 2);
                    }

                    if (intRequestID > 0)
                    {
                        //reroute item by request:
                        var RetCode = Assign.RerouteItem(intRequestID, strNewOrganization, strNewOrgCode, intUserID, strComments);

                        if (Settings.Default.SendEmailOnRerouteAssign)
                            email_request = Emails.InsertEmailRequest(user, (int)HistoryActions.haRerouteAssignment, true);

                        History.InsertHistoryOnRerouteReassign(intRequestID, strPrevOrganization, intPrevUserID,
                            strOrgNewValue, intUserID, strComments, user, email_request);
                    }
                    else
                    {

                        //reroute item directly to user, the action permitted to BD Admin only:
                        var dt = Assign.RerouteItemDirectly(intOItemID, strLines, intPrevUserID, strCurrentOrgCode, strNewOrganization, strNewOrgCode, intUserID, strComments);

                        if (Settings.Default.SendEmailOnRerouteAssign)
                            email_request = Emails.InsertEmailRequest(user, (int)HistoryActions.haRerouteAssignment, true);

                        History.InsertHistoryOnRerouteReassign(intOItemID, dt, strLines, intLoadID, strDocNum, strCurrentOrgCode, 
                            strPrevOrganization, intPrevUserID, strNewOrgCode, strOrgNewValue, intUserID, strComments, user, email_request);



                        //refresh Items GridResults on the next reload:
                        _page.ItemsDataView = null;
                    }

                    SendEmailOnReassigRequest(intOItemID, strCurrentOrgCode, strNewOrgCode, strCurrentOrganization, strNewOrganization, user, intUserID, strComments, "10");

                }

                if (obj_xml.DocumentElement.Name == "select_doc_to_email")
                {
                    var node = obj_xml.DocumentElement.FirstChild;

                    bool bln_selected;
                    int doc_id;

                    var selected_value = node.Attributes.GetNamedItem("selected").Value;
                    var selected_doc = node.Attributes.GetNamedItem("doc").Value;

                    if (selected_value != "" && selected_doc != "")
                    {
                        bln_selected = Boolean.Parse(selected_value);
                        doc_id = Int32.Parse(selected_doc);

                        Document.SelectSendAttachment(_page.DocNumber, doc_id, _page.LoadID, bln_selected);

                        if (Settings.Default.InsertHistory_OnSelectAttachmentToCO)
                        {
                            var doc = new Document(doc_id);
                            var file_name = doc.FileName;
                            var doc_type = String.Join(",", doc.DocumentTypeName);
                            History.InsertHistoryOnSelectAttachmentForEmail((int)HistoryActions.haSelectAttachmentForCOEmail, _page.DocNumber, _page.LoadID, bln_selected, doc_id, doc_type, file_name, _page.CurrentUserOrganization, user);
                        }
                    }
                }

                if (obj_xml.DocumentElement.Name == "select_doc_revision_email")
                {
                    var node = obj_xml.DocumentElement.FirstChild;

                    bool bln_selected;
                    int doc_id;

                    var selected_value = node.Attributes.GetNamedItem("selected").Value;
                    var selected_doc = node.Attributes.GetNamedItem("doc").Value;

                    if (selected_value != "" && selected_doc != "")
                    {
                        bln_selected = Boolean.Parse(selected_value);
                        doc_id = Int32.Parse(selected_doc);

                        Document.SelectAttachmentForRevision(_page.DocNumber, doc_id, _page.LoadID, bln_selected);

                        if (Settings.Default.InsertHistory_OnSelectAttachmentToSME)
                        {
                            var doc = new Document(doc_id);
                            var file_name = doc.FileName;
                            var doc_type = String.Join(",", doc.DocumentTypeName);
                            History.InsertHistoryOnSelectAttachmentForEmail((int)HistoryActions.haSelectAttachmentForSMEEmail, _page.DocNumber, _page.LoadID, bln_selected, doc_id, doc_type, file_name, _page.CurrentUserOrganization, user);
                        }
                    }
                }

                if (obj_xml.DocumentElement.Name == "select_doc_approved")
                {
                    var node = obj_xml.DocumentElement.FirstChild;

                    Int16 selected;
                    int doc_id;
                    int doc_type_code;

                    var selected_value = node.Attributes.GetNamedItem("selected").Value;
                    var selected_doc = node.Attributes.GetNamedItem("doc").Value;
                    var doc_type = node.Attributes.GetNamedItem("doc_type").Value;
                    var doc_type_name = node.Attributes.GetNamedItem("doc_type_name").Value;
                    var file_name = node.Attributes.GetNamedItem("file_name").Value;
                    var upload_user_email = node.Attributes.GetNamedItem("user").Value;
                    var upload_organization = node.Attributes.GetNamedItem("org").Value;

                    if (selected_value != "" && selected_doc != "" && doc_type != "")
                    {
                        selected = (Boolean.Parse(selected_value)) ? (Int16)DocRevisionStatus.dsApproved : (Int16)DocRevisionStatus.dsNotApproved;
                        doc_id = Int32.Parse(selected_doc);
                        doc_type_code = Int32.Parse(doc_type);
                        
                        Document.UpdateDocRevision(_page.LoadID, _page.DocNumber, doc_id, doc_type_code, selected);                                                
                        History.InsertHistoryOnDocumentRevision(_page.DocNumber, _page.LoadID, doc_id, file_name, doc_type_name, selected, upload_user_email, upload_organization, user);
                    }
                }

                if (obj_xml.DocumentElement.Name == "save_feedback")
                {
                    var intItemID = 0;
                    var intReviewer = 0;
                    var intLoad = 0;
                    var intValid = 0;
                    decimal decDO = 0;
                    decimal decUDO = 0;
                    var error_msg = "";

                    var node = obj_xml.DocumentElement.FirstChild;
                    
                    var item_id = node.Attributes.GetNamedItem("item_id").Value;
                    if (item_id.Length > 0)
                        intItemID = Int32.Parse(item_id);
                    var reviewer_id = node.Attributes.GetNamedItem("reviewer_id").Value;
                    if (reviewer_id.Length > 0)
                        intReviewer = Int32.Parse(reviewer_id);
                    var load_id = node.Attributes.GetNamedItem("load_id").Value;
                    if (load_id.Length > 0)
                        intLoad = Int32.Parse(load_id);
                    var valid = node.Attributes.GetNamedItem("valid").Value;
                    if (valid.Length > 0)
                        intValid = Int32.Parse(valid);
                    var udo_should_be = node.Attributes.GetNamedItem("udo").Value;
                    var do_should_be = node.Attributes.GetNamedItem("do").Value;
                    if (intValid == 0)
                        error_msg = "Please select the validation value. This is a required field. ";
                    try
                    {
                        if (udo_should_be.Length > 0 && udo_should_be != "$")
                            decUDO = Utility.GetDecimalFromDisplayedMoney(udo_should_be);
                        
                        if (do_should_be.Length > 0 && do_should_be != "$")
                            decDO = Utility.GetDecimalFromDisplayedMoney(do_should_be);
                    }
                    catch (Exception exp) 
                    {
                        error_msg = error_msg + "Please verify that you have entered the correct money value.";
                    }
                    if (error_msg != "")
                        str_result = "<error><err_msg msg='" + error_msg + "' /></error>";
                    else
                    {
                        var response = node.Attributes.GetNamedItem("response").Value;
                        var doc_number = node.Attributes.GetNamedItem("doc_num").Value;
                        var org_code = node.Attributes.GetNamedItem("org_code").Value;

                        Item.SaveFeedbackResponse(intItemID, doc_number, intLoad, intValid, response, decUDO, decDO);

                        //update item status:
                        Item.UpdateItemStatus(intItemID, intLoad, org_code, intReviewer, (int)OpenItemStatus.stClosed);

                        History.InsertHistoryOnFeedbackResponse(doc_number, intLoad, intItemID, org_code, intValid, response, decUDO, decDO, (int)OpenItemStatus.stClosed, intReviewer, user);

                        //refresh Items GridResults on the next reload:
                        _page.ItemsDataView = null;
                    }
                }
                
                if (str_result == "")
                    str_result = "<ok/>";
            }
            catch (Exception ex)
            {
                str_result = "<error><err_msg msg='" + ex.Message + "' /></error>";
            }

            Response.ContentType = "text/xml";
            Response.Write(str_result);
            Response.Flush();

        }

        private void SendEmailOnReassigRequest(int intItemID,
                                                      string sOldOrgCode, string sNewOrgCode,
                                                      string sOldOrganization, string sNewOrganization,
                                                      int iOldReviewerID, int iNewReviewerID,
                                                      string sComments, string ActionType)
        {
            //
            if (Settings.Default.SendEmailOnAllTypeOfAssignments)
            {
                var sOldReviewerName = "";
                var sNewReviewerName = "";
                var sMessageText = "";
                var sDocNum = "";
                var sSentToAdmins = "";
                var sSentTo = "";
                var sSubject = "";
                var iCnt = 0;
                var sNewReviewerEmail = "";

                    // here we need to send email message to all BA's on Reassign/Reroute request ***SM

                    // get Old User name by ID
                    if (iOldReviewerID != 0)
                    {
                        var ds = Users.GetUserByUserID(iOldReviewerID);
                        sOldReviewerName = ds.Tables[0].Rows[0]["FirstName"].ToString() + " " + ds.Tables[0].Rows[0]["LastName"].ToString();

                    }

                    // get New User name by ID
                    //if (ActionType == "5")
                    //{
                    if (iNewReviewerID != 0)
                    {
                        var ds = Users.GetUserByUserID(iNewReviewerID);
                        sNewReviewerName = ds.Tables[0].Rows[0]["FirstName"].ToString() + " " + ds.Tables[0].Rows[0]["LastName"].ToString();
                        sNewReviewerEmail = ds.Tables[0].Rows[0]["Email"].ToString();
                    }
                    //}

                    // get doc number  by OItemID

                    if (intItemID != 0)
                    {
                        var items = Item.GetDocNumByItemID(intItemID);
                        sDocNum = items.First();
                    }

                    var sMessageHeader = "<font color='red'>*** This is an auto-generated email. Please do not reply or forward to ***</font><br><br>";

                    if (ActionType == "5")
                    {

                        sSubject = "Re-Assign/Re-Route Request for the ULO document " + sDocNum;
                        sMessageText = sMessageHeader + "New reassign/reroute reguest has been placed for the ULO document '" + sDocNum + "'.<br><br>";

                        sMessageText = sMessageText + "Original Reviewer: " + sOldReviewerName + ".<br>";
                        sMessageText = sMessageText + "Original Organization: " + sOldOrganization + " : " + sOldOrgCode + ".<br><br>";

                        sMessageText = sMessageText + "Suggested  Reviewer: " + sNewReviewerName + ".<br>";
                        sMessageText = sMessageText + "Suggested  Organization: " + sNewOrganization + " : " + sNewOrgCode + ".<br><br>";

                        sMessageText = sMessageText + "Reroute/Reassign Comments:<br><br>";
                        sMessageText = sMessageText + sComments + "'.<br><br>";
                        sMessageText = sMessageText + "To confirm or modify rerouting, please login to <a href='http://dotnetweb.pbsncr.gsa.gov/OpenItems/ReviewOpenItems.aspx' >ULO Application</a>.<p>";
                        sMessageText = sMessageText + "For more information, go to the <a href='http://dotnetweb.pbsncr.gsa.gov/OpenItems/docs/UserManual.doc'>ULO (Open Items) User Manual.</a><p>";
                        sMessageText = sMessageText + "<font color='red'>Attention</font>: Please use the Internet Explorer (IE) browser instead of the Chrome browser.<p>";
                        sMessageText = sMessageText + "Thank you.<p><p>";
                        sMessageText = sMessageText + "(Ref # " + ActionType + ")";

                        Admin.SendCustomEmailToBDAdmins(sMessageText, sSubject, out sSentToAdmins, out iCnt);
                    }
                    else if (ActionType == "4" || ActionType == "3" || ActionType == "10")
                    {
                        if (ActionType == "4" || ActionType == "3")
                        {
                            sSubject = "New Open Items have been assigned to you.";
                            sMessageText = sMessageHeader + "New Open Items have been assigned to you.<br><br>";
                        }
                        else //10
                        {
                            sSubject = "New Open Items have been assigned to you by Budget Division Administrator as a result of reroute request.";
                            sMessageText = sMessageHeader + "New Open Items have been assigned to you by Budget Division Administrator as a result of reroute request.<br><br>";
                        }

                        sMessageText = sMessageText + "Reroute/Reassign Comments:<br><br>";
                        sMessageText = sMessageText + sComments + "'.<br><br>";
                        sMessageText = sMessageText + "Please login and review your workload in <a href='http://dotnetweb.pbsncr.gsa.gov/OpenItems/ReviewOpenItems.aspx' >ULO Application.</a><p>";
                        sMessageText = sMessageText + "For more information, go to the <a href='http://dotnetweb.pbsncr.gsa.gov/OpenItems/docs/UserManual.doc'>ULO (Open Items) User Manual.</a><p>";
                        sMessageText = sMessageText + "<font color='red'>Attention</font>: Please use the Internet Explorer (IE) browser instead of the Chrome browser.<p>";
                        sMessageText = sMessageText + "Thank you.<p><p>";
                        sMessageText = sMessageText + "(Ref # " + ActionType + ")";

                        Admin.SendCustomEmail(sNewReviewerEmail, sMessageText, sSubject, out sSentTo, out iCnt);

                    }
                    else if (ActionType == "16" || ActionType == "19")
                    {
                        sSubject = "New Open Items for validation have been assigned to you";
                        sMessageText = sMessageHeader + "New Open Items for validation have been assigned to you.<br><br>";
                        sMessageText = sMessageText + "Reroute/Reassign Comments:<br><br>";
                        sMessageText = sMessageText + sComments + "'.<br><br>";
                        sMessageText = sMessageText + "Please login and review your workload in <a href='http://dotnetweb.pbsncr.gsa.gov/OpenItems/ReviewOpenItems.aspx' >ULO Application.</a><p>";
                        sMessageText = sMessageText + "For more information, go to the <a href='http://dotnetweb.pbsncr.gsa.gov/OpenItems/docs/UserManual.doc'>ULO (Open Items) User Manual.</a><p>";
                        sMessageText = sMessageText + "<font color='red'>Attention</font>: Please use the Internet Explorer (IE) browser instead of the Chrome browser.<p>";
                        sMessageText = sMessageText + "Thank you.<p><p>";
                        sMessageText = sMessageText + "(Ref # " + ActionType + ")";

                        Admin.SendCustomEmail(sNewReviewerEmail, sMessageText, sSubject, out sSentTo, out iCnt);
                    }



            }
        }
    }
    }


