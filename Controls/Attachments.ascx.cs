using System.ComponentModel;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using GSA.OpenItems;
    using Data;

    public partial class Controls_Attachments : System.Web.UI.UserControl
    {
        bool bShowEmailIcon = false;
        bool bShowAddAttBtn = false;
        //TODO: Create UserControl Base class that defines these. Not able to use constructor.
        private static readonly IDataLayer Dal = new DataLayer(new zoneFinder());
        private readonly AdminBO Admin = new AdminBO(Dal);
        private DocumentBO Document = new DocumentBO(Dal);

        protected void Page_Load(object sender, EventArgs e)
        {

            try
            {
                lblAttachErr.Visible = false;
                lblAttachErr.Text = "";
                if (!IsPostBack)
                {
                    btnAttachments.Visible = true;
                }

                InitAttachmentsList();
            }
            catch (Exception ex)
            {
                lblAttachErr.Visible = true;
                lblAttachErr.Text = ex.Message;
            }
        }

        private void InitAttachmentsList()
        {
            btnAttachments.Attributes.Add("onclick", "javascript:return view_att();ClearErrMsg();");



            var _page = new PageBase();
            var ds = Document.GetDocumentAttachments(Convert.ToInt32(_page.DocNumber), _page.LoadID.ToString());
            if (ds == null || ds.Tables[0].Rows.Count == 0)
            {
                if (bShowEmailIcon == true)
                {
                    if (Page.User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) == true || Page.User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) == true)
                    {
                        hdnUserIsAdmin.Value = "1";
                        btnEmail.Visible = true;
                        //btnEmail.Attributes.Add("title", "");
                    }
                    else
                    {
                        hdnUserIsAdmin.Value = "0";
                        btnEmail.Visible = false;
                    }
                }
                else
                {
                    hdnUserIsAdmin.Value = "0";
                    btnEmail.Visible = false;
                }
                return;
            }

            var count = ds.Tables[0].Rows.Count;
            lblAttCount.Visible = true;
            //lblAttCount.Text = " (" + count.ToString() + "). ";

            var doc_id = 0;
            var sUploadDate = "";
            var iCountDisplayedAtt = 0;

            for (int row_id = 0, tbl_row = 0; row_id < count && tbl_row < 3; row_id++) // olga's - only 3 docs
                                                                                       //for (int row_id = 0, tbl_row = 0; row_id < count; row_id++)
            {
                if (doc_id != (int)ds.Tables[0].Rows[row_id]["DocID"])
                {
                    // String.Format("{0:M/d/yyyy}", dt);  
                    doc_id = (int)ds.Tables[0].Rows[row_id]["DocID"];
                    sUploadDate = ds.Tables[0].Rows[row_id]["UploadDate"].ToString();
                    var dUploadDate = Convert.ToDateTime(sUploadDate);
                    sUploadDate = dUploadDate.ToString("d");
                    var td = new HtmlTableCell();
                    var tr = new HtmlTableRow();

                    var sb = new StringBuilder();
                    //sb.Append(sUploadDate + " - ");
                    sb.Append("<a href='javascript:view_doc(");

                    sb.Append(ds.Tables[0].Rows[row_id]["DocID"].ToString());
                    sb.Append(");' ><img src='../images/btn_view_file.gif' alt='' title='view file'  style='border:0;vertical-align:middle;'/>");

                    //sb.Append(ds.Tables[0].Rows[row_id]["DocID"].ToString() +");' >");
                    //sb.Append("<img src='../images/btn_view_file.gif' alt='' height='18px' style='border:0;'/>");

                    sb.Append("<span class='regBldGreyText'>");
                    sb.Append(sUploadDate + " - " + ds.Tables[0].Rows[row_id]["FileName"].ToString());
                    //sb.Append(ds.Tables[0].Rows[row_id]["FileName"].ToString() + " (" + sUploadDate + ")");
                    sb.Append("</span></a>");
                    td.InnerHtml = sb.ToString();
                    td.Attributes.Add("style", "vertical-align:top");
                    tr.Attributes.Add("style", "vertical-align:top");
                    tr.Cells.Add(td);
                    tr.Attributes.Add("class", "regBldGreyText");

                    tblAttLinks.Rows.Add(tr);
                    tbl_row++;
                    iCountDisplayedAtt = iCountDisplayedAtt + 1;
                }

            }

            if (count > 3)
            {
                lblAttCount.Text = " (" + iCountDisplayedAtt + " of " + count.ToString() + ") ";
            }
            else
            {
                lblAttCount.Text = " (" + count.ToString() + ") ";
            }

            if (count == 0 && bShowEmailIcon == true)
            {
                if (Page.User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) == true || Page.User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) == true)
                {
                    hdnUserIsAdmin.Value = "1";
                    btnEmail.Visible = true;
                }
                else
                {
                    hdnUserIsAdmin.Value = "0";
                    btnEmail.Visible = false;
                }
            }
            else
            {
                hdnUserIsAdmin.Value = "0";
                btnEmail.Visible = false;
            }

            if (bShowAddAttBtn == true)
            {
                btnAttachments.Visible = true;
            }
            else
            {
                if (count == 0)
                {
                    btnAttachments.Visible = false;
                    lblAttCount.Text = "(0)";
                }

                if (count > 3)
                {
                    btnAttachments.Visible = true;
                }
                else
                {
                    btnAttachments.Visible = true;
                }
            }

        }


        public void ShowEmailIcon(bool bShow, string sReviewerEmail, string sDocNum)
        {
            hdnReviewerEmail.Value = sReviewerEmail;
            hdnDocNum.Value = sDocNum;

            if (bShow == true)
            {
                bShowEmailIcon = true;
            }
            else
            {
                bShowEmailIcon = false;
            }
        }

        public void ShowAddAttBtn(bool bShow)
        {

            if (bShow == true)
            {
                bShowAddAttBtn = true;
            }
            else
            {
                bShowAddAttBtn = false;
            }
        }





        protected void btnEmail_ServerClick(object sender, ImageClickEventArgs e)
        {


            var iCnt = 0;
            var sSentTo = "";
            var sMessageHeader = "*** This is an auto-generated email. Please do not reply or forward to ***<br><br>";

            //if (hdnSendEmail.Value == "1")
            //{
            //    string s = "Sent";
            //}
            // send email to reviewer
            var sBody = sMessageHeader + "<p>Your validation is incomplete. Please upload supporting documentation in the ULO Database for document # " + hdnDocNum.Value;
            var sSbj = "Your validation for document # " + hdnDocNum.Value + " is incomplete";

            try
            {
                if (hdnSendEmailAfterConfirm.Value == "1")
                {

                    if (hdnReviewerEmail.Value.Trim() != "")
                    {
                        Admin.SendCustomEmail(hdnReviewerEmail.Value, sBody, sSbj, out sSentTo, out iCnt);
                        lblAttachErr.Visible = true;
                        lblAttachErr.Text = "Message has been sent to: " + hdnReviewerEmail.Value;
                    }
                    else
                    {
                        lblAttachErr.Visible = true;
                        lblAttachErr.Text = "Can not sent email. Invalid Email address.";
                        btnEmail.Visible = true;
                    }
                }
                else
                {
                    btnEmail.Visible = true;
                }



            }
            catch (Exception ex)
            {
                lblAttachErr.Visible = true;
                lblAttachErr.Text = "SMTP Error: " + ex.Message + " Try again later.";
                btnEmail.Visible = true;
            }

        }
    }
}