using System.Linq;

namespace GSA.OpenItems.Web
{
    using Data;
    using System;
    using System.Web.UI.WebControls;

    public partial class SendULOEmail : PageBase
    {
        string sErrorStatus = "0";

        private readonly AdminBO Admin;
        private readonly UsersBO Users;
        public SendULOEmail() {
            Admin = new AdminBO(this.Dal);
            Users = new UsersBO(this.Dal, new EmailsBO(this.Dal));
        }

        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            


            lblError.Text = "";
            lblError.CssClass = "error";
            //int iID = 0;

            hMessageHeader.Value = "<p><font face='Verdana' color='red' size='3'>*** This is an auto-generated email. Please do not reply or forward to ***</font></p><font face='Verdana' color='#333333' size='3'><p>";

            if (IsPostBack == false)
            {
                btnSendDueDateReminder.Visible = false;
                btnSendDueDateReminder.Attributes["onclick"] = "javascript:return confirm('Are you sure you want to send due date reminder to the ULO users ?');";
                //btnSendEmail.Attributes["onclick"] = "javascript:return confirm('Are you sure you want to send email ?');";

                if (Page.User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) == true) // BD admin
                {

                    //panDueReminder.Visible = true;
                    //panCustomEmail.Visible = false;
                    //rbEmailType.Enabled = false;
                    tr_graphics.Visible = false;
                }

                if (Page.User.IsInRole(((int)UserRoles.urSysAdmin).ToString()) == true) //Sys admin
                {
                    //panDueReminder.Visible = false;
                    //panCustomEmail.Visible = false;
                    //rbEmailType.Enabled = true;
                    tr_graphics.Visible = true;
                }

                panDueReminder.Visible = false;
                panCustomEmail.Visible = false;
                rbEmailType.Enabled = true;



                ddlULOUsers.Attributes["onChange"] = "AddEmail();";
                //txtSendTo.SetReadOnly();

                try
                {
                    // load reviews
                    FillCombos(ddlLoads, "load", "- select -");
                    FillCombos(ddlULOUsers, "users", "- select -");

                }
                catch (Exception ex)
                {
                    lblError.Text = DisplayMessage(lblError, "Error: " + ex.Message, true);
                }  
            }

        }
        protected string DisplayMessage(Label oControl,string sMessage, bool bError)
        {

            if (bError == true)
            {
                oControl.CssClass = "error";
            }
            else
            {
                oControl.CssClass = "blue_message";
            }
            return sMessage;
        }

        protected void FillCombos(DropDownList ddl, string sComboName, string sDefault)
        {
            //int iID = 0;

            switch (sComboName)
            {

                case "load":
                    var loadList = Admin.GetLoadListWithActiveDueDate();
                    ddl.Items.Clear();
                    ddl.DataSource = loadList;
                    ddl.DataValueField = "LoadID";
                    ddl.DataTextField = "LoadDesc";

                    ddl.DataBind();

                    if (ddl.Items.Count == 0)
                    {
                        lblDueDateMessage.Text = DisplayMessage(lblDueDateMessage, "There are no loads with active due date", false);
                        //btnReset.Enabled = false;
                       // btnReset.ToolTip = "button disabled";
                        btnSendDueDateReminder.Enabled = false; //Send Reminder" OnClick="btnSendDueDateReminder_Click
                        btnSendDueDateReminder.ToolTip = "button disabled";
                    }
                    else
                    {
                        if (sDefault != "")
                        {
                            ddl.Items.Insert(0, sDefault);
                            //btnReset.Enabled = true;
                            btnSendDueDateReminder.Enabled = true;
                        }
                    }
                    break;
                case "users":
                    var users = Users.GetAllULOUsers();
                    ddlULOUsers.Items.Clear();
                    ddlULOUsers.DataSource = users;
                    ddlULOUsers.DataTextField = "FullName";
                    ddlULOUsers.DataValueField = "Email";
                    ddlULOUsers.DataBind();
                    ddlULOUsers.Items.Insert(0, "- select -");
                    break;

                default:
                    break;
            }
        }

        protected void btnSendDueDateReminder_Click(object sender, EventArgs e)
        {
            var sSentToR = "";
            var sSentToOA = "";
            var sSentToBDA = "";

            try
            {
                if (ddlLoads.SelectedIndex == 0)
                {
                    lblError.Text = "Please select an active review from the combo box above.";
                }
                else
                {
                    Admin.SendDueDateReminder(ddlLoads.SelectedItem.Value, txtLoadDate.Value, txtDueDate.Value, out sSentToR, out sSentToOA, out sSentToBDA);
                    lblError.Text = DisplayMessage(lblError, "Reminder has been sent to: <br>Reviewers: " + sSentToR + "<br>Org Admins: " + sSentToOA + "<br>Budget Admins:" + sSentToBDA, false);
                    //lblError.Text = DisplayMessage(lblError, "Reminder has been sent", false);   
                }
            }
            catch (Exception ex)
            {
                lblError.Text = DisplayMessage(lblError, "Error: " + ex.Message, true);
            }

            
        }
        //protected void btnReset_Click(object sender, EventArgs e)
        //{
        //    ddlLoads.SelectedIndex = 0;
        //    ddlULOUsers.SelectedIndex = 0;
        //}


        protected void ddlLoads_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ddlLoads.SelectedIndex == 0)
                {
                    btnSendDueDateReminder.Attributes.Clear();
                    btnSendDueDateReminder.Visible = false;
                    //btnReset.Visible = false;
                }
                else
                {
                    btnSendDueDateReminder.Visible = true;
                    //btnReset.Visible = true;
                    var ds = Admin.GetLoadDetails(Convert.ToInt32(ddlLoads.SelectedItem.Value));
                    txtLoadDate.Value = ds.First().LoadDate;
                    txtDueDate.Value = ds.First().DueDate;
                }
            }
            catch (Exception ex)
            {
                lblError.Text = DisplayMessage(lblError, "Error: " + ex.Message, true);
            }
             
        }

        protected void btnApplyGraphics_Click(object sender, EventArgs e)
        {
        }


        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSendTo.Text = "";
            ddlULOUsers.SelectedIndex = 0;
        }
        protected void btnSendEmail_Click(object sender, EventArgs e)
        {
            var sSentTo = "";
            var iCnt = 0;


            try
            {
                //if (ddlRecipients.SelectedIndex == 0)
                //{
                //    throw new Exception("Select Recipients.");
                //}

                //if (txtSubject.Text.Trim() == "")
                //{
                //    throw new Exception("'Subject field' is empty.");
                //}

                //if (txtBody.Text.Trim() == "")
                //{
                //    throw new Exception("The 'Body' field is empty.");
                //}

                
                txtGraphicsRed.Text.Replace(" ", "");
                txtGraphicsBlue.Text.Replace(" ", "");

                var sBody = txtBody.Text;

                if (txtGraphicsBold.Text.Trim() != "")
                {
                    var arrGraphicsBold = txtGraphicsBold.Text.Split(new char[] { '^' });
                    Array.Sort(arrGraphicsBold);

                    foreach (var s in arrGraphicsBold) //check for the duplicates and remove if any before sending 
                    {
                        if (s.Trim() != "")
                        {
                            sBody = sBody.Replace(s, "<b>" + s + "</b>");
                        }
                    }
                }

                if (txtGraphicsRed.Text.Trim() != "")
                {
                    var arrGraphicsR = txtGraphicsRed.Text.Split(new char[] { '^' });
                    Array.Sort(arrGraphicsR);

                    foreach (var s in arrGraphicsR) //check for the duplicates and remove if any before sending 
                    {
                        if (s.Trim() != "")
                        {
                            sBody = sBody.Replace(s, "<font color='red'>" + s + "</font>");
                        }
                    }
                }

                if (txtGraphicsBlue.Text.Trim() != "")
                {
                    var arrGraphicsB = txtGraphicsBlue.Text.Split(new char[] { '^' });
                    Array.Sort(arrGraphicsB);

                    foreach (var s in arrGraphicsB) //check for the duplicates and remove if any before sending 
                    {
                        if (s.Trim() != "")
                        {
                            sBody = sBody.Replace(s, "<font color='blue'>" + s + "</font>");
                        }
                    }
                }

                if (txtGraphicsGreen.Text.Trim() != "")
                {
                    var arrGraphicsG = txtGraphicsGreen.Text.Split(new char[] { '^' });
                    Array.Sort(arrGraphicsG);
                    foreach (var s in arrGraphicsG) //check for the duplicates and remove if any before sending 
                    {
                        if (s.Trim() != "")
                        {
                            sBody = sBody.Replace(s, "<font color='green'>" + s + "</font>");
                        }
                    }
                }

                sBody = sBody.Replace("\r", "<p>");
                //txtBody.Text = sBody;

                switch (ddlRecipients.SelectedIndex)
                {
                    case 0: // - select - 
                        //lblError.Text = "Select Recipients";
                        break;
                    case 1: // ULO Reviewer  
                        Admin.SendCustomEmailToReviewers(hMessageHeader.Value + sBody, txtSubject.Text, out sSentTo, out iCnt);
                        lblError.Text = DisplayMessage(lblError, "Message has been sent to the following Reviewers:<br>" + sSentTo + "<br>Total messages sent: " + iCnt, false);
                        break;
                    case 2: // ULO Organization Admin
                        Admin.SendCustomEmailToOrgAdmins(hMessageHeader.Value + sBody, txtSubject.Text, out sSentTo, out iCnt);
                        lblError.Text = DisplayMessage(lblError, "Message has been sent to the following Org Admins:<br>" + sSentTo + "<br>Total messages sent: " + iCnt, false);
                        break;
                    case 3: // ULO Budget Division Admin
                        Admin.SendCustomEmailToBDAdmins(hMessageHeader.Value + sBody, txtSubject.Text, out sSentTo, out iCnt);
                        lblError.Text = DisplayMessage(lblError, "Message has been sent to the following BD Admins:<br>" + sSentTo + "<br>Total messages sent: " + iCnt, false);
                        break;
                    case 4: // All ULO Users
                        Admin.SendCustomEmailToAllULOUsers(hMessageHeader.Value + sBody, txtSubject.Text, out sSentTo, out iCnt);
                        lblError.Text = DisplayMessage(lblError, "Message has been sent to the following ULO Users:<br>" + sSentTo + "<br>Total messages sent: " + iCnt, false);
                        break;
                    case 5: // Manual Entry
                        if (txtSendTo.Text.Trim() == "")
                        {
                            throw new Exception("The 'Send To' field is empty.");
                        }
                        //sErrorStatus = "Ready to go to AdminBO.SendCustomEmail";
                        Admin.SendCustomEmail(txtSendTo.Text, hMessageHeader.Value + sBody, txtSubject.Text, out sSentTo, out iCnt);
                        lblError.Text = DisplayMessage(lblError, "Message has been sent to the following ULO Users:<br>" + sSentTo + "<br>Total messages sent: " + iCnt, false);
                        break;
                                   }
                ddlULOUsers.SelectedIndex = 0;
                ddlLoads.SelectedIndex = 0;
                txtSendTo.Text = "";
            }
            catch (Exception ex)
            {
                lblError.Text = DisplayMessage(lblError, "Error: " + sErrorStatus + " " + ex.Message, true);
            }

     
        }
        protected void rbEmailType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rbEmailType.SelectedItem.Value == "1")
            {
            }

            switch (rbEmailType.SelectedItem.Value)
            {
                case "1": // due date reminder 
                    panDueReminder.Visible = true;
                    panCustomEmail.Visible = false;
                    break;
                case "2": // custom email
                    panDueReminder.Visible = false;
                    panCustomEmail.Visible = true;
                    break;
               
            }

        }
        protected void ddlRecipients_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (ddlRecipients.SelectedIndex == 5)
            {
                tr_custom_email_1.AddDisplay();
                tr_custom_email_2.AddDisplay();
            }
            else
            {
                tr_custom_email_1.AddDisplayNone();
                tr_custom_email_2.AddDisplayNone();
                txtSendTo.Text = "";
            } 

        }


        protected void ddlULOUsers_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (txtSendTo.Text.Trim() == "")
            {
                txtSendTo.Text = ddlULOUsers.SelectedItem.Value;
            }
            else
            {
                txtSendTo.Text = txtSendTo.Text + "," + ddlULOUsers.SelectedItem.Value;
            }

        }
}
}
