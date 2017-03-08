namespace GSA.OpenItems.Web
{
    using Data;
    using System;
    using System.Web.UI.WebControls;

    public partial class SendDateDueReminder : PageBase
    {
        private readonly AdminBO Admin;
        public SendDateDueReminder() { 
            Admin = new AdminBO(this.Dal);
        }
        
        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            //ddlLoads.OnChangeHandler += new EventHandler(ddlLoads_changed);
  
            lblError.Text = "";
            
            lblError.CssClass = "error";
            //int iID = 0;
            if (IsPostBack == false)
            {
                try
                {
                    // load all NCS users
                    FillCombos(ddlLoads, "load", "- select -");
                    //ddlLoads.Items.Insert(0, "- select -");


                }
                catch (Exception ex)
                {
                    lblError.Text = DisplayMessage("Error: " + ex.Message, true);
                }
            }

        }
        protected string DisplayMessage(string sMessage, bool bError)
        {

            if (bError == true)
            {
                lblError.CssClass = "error";
            }
            else
            {
                lblError.CssClass = "blue_message";
            }
            return sMessage;
        }

        protected void FillCombos(DropDownList ddl, string sComboName, string sDefault)
        {

            switch (sComboName)
            {
                case "load":
                    var ds = Admin.GetLoadListWithActiveDueDate();
                    ddl.Items.Clear();
                    ddl.DataSource = ds;
                    ddl.DataValueField = ds.Tables[0].Columns["LoadID"].ToString();
                    ddl.DataTextField = ds.Tables[0].Columns["LoadDesc"].ToString();

                    ddl.DataBind();

                    if (ddl.Items.Count == 0)
                    {
                        lblError.Text = DisplayMessage("There are no loads with active due date", false);
                    }
                    else
                    {
                        if (sDefault != "")
                        {
                            ddl.Items.Insert(0, sDefault);
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            var sSentToR = "";
            var sSentToOA = "";
            var sSentToBDA = "";
            try
            {
                Admin.SendDueDateReminder(ddlLoads.SelectedItem.Value, txtLoadDate.Value, txtDueDate.Value, out sSentToR, out sSentToOA, out sSentToBDA);
                lblError.Text = DisplayMessage("Reminder has been sent to: <br>Reviewers: " + sSentToR + "<br>Org Admins: " + sSentToOA + "<br>Budget Admins:" + sSentToBDA, false);
            }
            catch (Exception ex)
            {
                lblError.Text = DisplayMessage("Error: " + ex.Message, true);
            }
            
        }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            ddlLoads.SelectedIndex = -1;
        }


        protected void ddlLoads_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ds = Admin.GetLoadDetails(Convert.ToInt32(ddlLoads.SelectedItem.Value));
            txtLoadDate.Value = ds.Tables[0].Rows[0]["LoadDate"].ToString();
            txtDueDate.Value = ds.Tables[0].Rows[0]["DueDate"].ToString();
        }
        protected void btnSendEmailToOrgAdnins_Click(object sender, EventArgs e)
        {
            var sSentToOA = "";

            //Admin.SendEmailToOrgAdmins(txtBody.Value, txtSubject.Value, out sSentToOA);
            //lblError.Text = DisplayMessage("Reminder has been sent to: <br>Org Admins: " + sSentToOA , false);
        }
}
}
