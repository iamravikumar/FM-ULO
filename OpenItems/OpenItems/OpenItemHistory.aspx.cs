namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Text;
    using System.Web.UI.WebControls;

    public partial class OpenItemHistory : PageBase
    {
        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    if (Request.QueryString["att"] == "yes")
                        lblMainTitle.Text = "Attachments History";

                    InitControls(Request.QueryString["doc"]);
                }
                else
                {
                    if (lblMessage.CssClass == "errorsum")
                    {
                        //'erase' error message
                        lblMessage.Visible = false;
                        lblMessage.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.CssClass = "errorsum";
                lblMessage.Text = "Error: " + ex.Message;
            }
        }

        private void InitControls(string DocNumber)
        {
            if (DocNumber == null || DocNumber == "")
            {
                var item = new OpenItem(OItemID, LoadID, OrgCode, CurrentItemReviewerID);

                //lblDocNumber.Text = item.DocNumber;
                //lblReviewer.Text = item.Reviewer;
                DocNumber = item.DocNumber;
            }

            lblDocNumber.Text = DocNumber;

            var dsHistory = History.GetDocumentHistory(DocNumber);

            dlLoads.DataSource = dsHistory.Tables[0];
            dlLoads.DataBind();
        }

        protected void Page_Init(object sender, System.EventArgs e)
        {
            dlLoads.ItemCreated += new DataListItemEventHandler(dlLoads_ItemCreated);
        }

        void dlLoads_ItemCreated(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var lblLoadInfo = (Label)e.Item.FindControl("lblLoadInfo");

                var dr = (DataRowView)e.Item.DataItem;

                var load_id = (int)dr["LoadID"];
                var doc_num = (string)dr["DocNumber"];
                // int item_id = (int)dr["OItemID"];

                var sb = new StringBuilder();
                sb.Append("Review Name - '");
                sb.Append(((string)dr["Description"]).ToString());
                sb.Append("'.   Data loaded on ");
                sb.Append(((DateTime)dr["LoadDate"]).ToString("MMM dd, yyyy"));
                sb.Append(".   Review round - ");
                sb.Append(((int)dr["ReviewRound"]).ToString());
                sb.Append(".  Data source - ");
                sb.Append((string)dr["DataSourceDescription"]);
                sb.Append(".  ");
                if (dr["ArchiveDate"] != DBNull.Value)
                {
                    sb.Append(" <br />The Review has been archived on ");
                    sb.Append(((DateTime)dr["ArchiveDate"]).ToString("MMM dd, yyyy"));
                    sb.Append(".  ");
                }
                lblLoadInfo.Text = sb.ToString();
                DataSet ds;
                if (Request.QueryString["att"] == "yes")
                    ds = History.GetAttachmentsHistoryPerLoad(doc_num, load_id);
                else
                    ds = History.GetDocHistoryPerLoad(doc_num, load_id);
                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    var lblLoadMsg = (Label)e.Item.FindControl("lblLoadMsg");
                    lblLoadMsg.Text = "No records found.";
                }
                else
                {
                    var gvHistory = (GridView)e.Item.FindControl("gvHistory");
                    gvHistory.RowDataBound += new GridViewRowEventHandler(gvHistory_RowDataBound);

                    gvHistory.DataSource = ds.Tables[0];
                    gvHistory.DataBind();
                }
            }
        }


        void gvHistory_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            var dr = (DataRowView)e.Row.DataItem;

            var lblAction = (Label)e.Row.FindControl("lblAction");
            if (lblAction != null)
            {
                var action_desc = (string)dr["ActionDescription"];
                if (action_desc.Substring(0, 4) == "Line")
                    action_desc = "Line " + ((int)Utility.GetNotNullValue(dr["ItemLNum"], "Int")).ToString() + action_desc.Substring(4);

                if ((int)dr["ActionCode"] == (int)HistoryActions.haRerouteAssignment && (int)Utility.GetNotNullValue(dr["ItemLNum"], "Int") != 0)
                    action_desc = "Line " + ((int)dr["ItemLNum"]).ToString() + " " + action_desc;

                lblAction.Text = action_desc;
            }
            //else
            //{
            //    lblAction.Text = "Reviewer clicked on 'Save' button";
            //}

            var lblValid = (Label)e.Row.FindControl("lblValid");
            if (lblValid != null)
            {
                string valid;
                if ((int)dr["ActionCode"] == (int)HistoryActions.haDocumentRevisionBySME)
                {
                    if ((int)Utility.GetNotNullValue(dr["Valid"], "Int") == (int)DocRevisionStatus.dsApproved)
                        valid = "Approved";
                    else
                        valid = "Not Approved";
                }
                else
                    valid = dr["ValidValueDescription"].ToString();

                lblValid.Text = valid;
            }

            var lblInfo = (Label)e.Row.FindControl("lblInfo");
            if (lblInfo != null)
            {
                var text_info = "";
                StringBuilder sb;
                string prev_reviewer;
                string new_reviewer;
                string contact_role;
                string contact_name;
                string lines;
                switch ((int)dr["ActionCode"])
                {
                    case (int)HistoryActions.haUploadNewAttachment:
                    case (int)HistoryActions.haUpdateAttacment:
                    case (int)HistoryActions.haDeleteAttachment:
                        sb = new StringBuilder();
                        sb.Append("Document: ");
                        sb.Append("<a href='javascript:view_doc(");
                        sb.Append(dr["CustomFieldNumeric"].ToString());
                        sb.Append(");' ><img src='../images/btn_view_file.gif' alt='' title='view file' style='border:0;vertical-align:middle'/><span class='regBlueText'>");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField01"], "String"));
                        sb.Append("</span></a>");
                        sb.Append("<br />");
                        sb.Append("Document Types: ");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField02"], "String"));
                        text_info = sb.ToString();
                        break;
                    case (int)HistoryActions.haDocumentRevisionBySME:
                        sb = new StringBuilder();
                        sb.Append("Document: ");
                        sb.Append("<a href='javascript:view_doc(");
                        sb.Append(dr["CustomFieldNumeric"].ToString());
                        sb.Append(");' ><img src='../images/btn_view_file.gif' alt='' title='view file'  style='border:0;vertical-align:middle'/><span class='regBlueText'>");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField01"], "String"));
                        sb.Append("</span></a>");
                        sb.Append("<br />");
                        sb.Append("Document Type: ");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField02"], "String"));
                        text_info = sb.ToString();
                        break;
                    case (int)HistoryActions.haSelectAttachmentForCOEmail:
                    case (int)HistoryActions.haSelectAttachmentForSMEEmail:
                        sb = new StringBuilder();
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField03"], "String"));
                        sb.Append(" Document: ");
                        sb.Append("<a href='javascript:view_doc(");
                        sb.Append(dr["CustomFieldNumeric"].ToString());
                        sb.Append(");' ><img src='../images/btn_view_file.gif' alt='' title='view file' style='border:0;vertical-align:middle'/><span class='regBlueText'>");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField01"], "String"));
                        sb.Append("</span></a>");
                        sb.Append("<br />");
                        sb.Append("Document Types: ");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField02"], "String"));
                        text_info = sb.ToString();
                        break;
                    case (int)HistoryActions.haSendEmailToCO:
                    case (int)HistoryActions.haSendEmailToSME:
                        sb = new StringBuilder();
                        sb.Append("Email has been sent to: ");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField01"], "String"));
                        sb.Append("<br />");
                        sb.Append("Subject: ");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField02"], "String"));
                        text_info = sb.ToString();
                        break;
                    case (int)HistoryActions.haCreateItemContact:
                    case (int)HistoryActions.haDeleteItemContact:
                        contact_name = (string)Utility.GetNotNullValue(dr["CustomField02"], "String");
                        contact_role = (string)Utility.GetNotNullValue(dr["CustomField01"], "String");
                        text_info = contact_role + " : " + contact_name;
                        break;
                    case (int)HistoryActions.haAssignmentVerification:
                        new_reviewer = (string)Utility.GetNotNullValue(dr["NewReviewer"], "String");
                        text_info = "Verified assignment to " + new_reviewer;
                        break;
                    case (int)HistoryActions.haReviewerAssignment:
                        new_reviewer = (string)Utility.GetNotNullValue(dr["NewReviewer"], "String");
                        text_info = "Reassigned to " + new_reviewer;
                        break;
                    case (int)HistoryActions.haReviewerReassignment:
                        prev_reviewer = (string)Utility.GetNotNullValue(dr["ReviewerName"], "String");
                        new_reviewer = (string)Utility.GetNotNullValue(dr["NewReviewer"], "String");
                        if (prev_reviewer != "" || new_reviewer != "")
                        {
                            sb = new StringBuilder();
                            sb.Append("Reassignment");
                            if (prev_reviewer != "")
                            {
                                sb.Append(" from ");
                                sb.Append(prev_reviewer);
                            }
                            if (new_reviewer != "")
                            {
                                sb.Append(" to ");
                                sb.Append(new_reviewer);
                            }
                            lines = (string)Utility.GetNotNullValue(dr["CustomField03"], "String");
                            if (lines != "all" && lines != "0" && lines != "")
                            {
                                sb.Append(". Lines: ");
                                sb.Append(lines);
                            }
                            text_info = sb.ToString();
                        }
                        break;
                    case (int)HistoryActions.haRerouteRequest:
                        prev_reviewer = (string)Utility.GetNotNullValue(dr["ReviewerName"], "String");
                        new_reviewer = (string)Utility.GetNotNullValue(dr["NewReviewer"], "String");
                        sb = new StringBuilder();
                        sb.Append("Suggested reroute from ");
                        sb.Append(prev_reviewer);
                        sb.Append(" (");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField01"], "String"));
                        sb.Append(") to ");
                        sb.Append(new_reviewer);
                        sb.Append(" (");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField02"], "String"));
                        sb.Append(")");
                        lines = (string)Utility.GetNotNullValue(dr["CustomField03"], "String");
                        if (lines != "all" && lines != "0" && lines != "")
                        {
                            sb.Append(". Lines: ");
                            sb.Append(lines);
                        }
                        text_info = sb.ToString();
                        break;
                    case (int)HistoryActions.haRerouteAssignment:
                        prev_reviewer = (string)Utility.GetNotNullValue(dr["ReviewerName"], "String");
                        new_reviewer = (string)Utility.GetNotNullValue(dr["NewReviewer"], "String");
                        sb = new StringBuilder();
                        sb.Append("Confirmed reroute from ");
                        sb.Append(prev_reviewer);
                        sb.Append(" (");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField01"], "String"));
                        sb.Append(") to ");
                        sb.Append(new_reviewer);
                        sb.Append(" (");
                        sb.Append((string)Utility.GetNotNullValue(dr["CustomField03"], "String"));
                        sb.Append(")");
                        text_info = sb.ToString();
                        break;
                        //case (int)HistoryActions.haFeedbackResponse:
                        //    text_info = (string)Utility.GetNotNullValue(dr["CustomField01"], "String");
                        //    if (text_info != "")
                        //        text_info = text_info + ", ";
                        //    text_info = text_info + (string)Utility.GetNotNullValue(dr["CustomField02"], "String");
                        //    break;
                }
                lblInfo.Text = text_info;
                if (lblInfo.Text.Trim() == "")
                {
                    lblInfo.Text = "Reviewer clicked on 'Save' button.";
                }

            }
        }
    }
}