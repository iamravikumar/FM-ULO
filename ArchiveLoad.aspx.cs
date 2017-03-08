namespace GSA.OpenItems.Web
{
    using System;

    public partial class ArchiveLoad : PageBase 
    {
        private readonly AdminBO Admin;
        public ArchiveLoad()
        {
            Admin= new AdminBO(this.Dal);
        }

        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            lblError.Text = "";
            lblError.CssClass = "error";
            var iCount = 0;
            //int iID = 0;
            if (IsPostBack == false)
            {
                btnArchive.Attributes.Add("style", "display:none");
                btnUnarchive.Attributes.Add("style", "display:none");
                try
                {
                    // load all ULO loads
                    mgLoads.FillMultiGrid("spGetLoadInfo", "LoadID", "", "", false,out iCount);                  
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

        protected void btnArchive_Click(object sender, EventArgs e)
        {
            var iCount = 0;
            var sLoadName = mgLoads.Table.Rows[mgLoads.SelectedIndex][2].ToString();
            try
            {
                Admin.ArchiveLoad(mgLoads.ItemID);
                mgLoads.FillMultiGrid("spGetLoadInfo", "LoadID", "", "", false, out iCount);
                lblError.Text = DisplayMessage("Review archived.", false);
                btnArchive.Attributes.Add("style", "display:none");
                btnUnarchive.Attributes.Add("style", "display:none");
                mgLoads.SelectedIndex = -1;
                SendEmail("The " + sLoadName + " has been archived", "The " + sLoadName + " has been archived");
            }
            catch(Exception ex)
            {
                lblError.Text = DisplayMessage("Error: " + ex.Message, true);
            }
        }

        protected void btnUnarchive_Click(object sender, EventArgs e)
        {
            var iCount = 0;
            try
            {
                var sLoadName = mgLoads.Table.Rows[mgLoads.SelectedIndex][2].ToString();
                Admin.UnarchiveLoad(mgLoads.ItemID);
                mgLoads.FillMultiGrid("spGetLoadInfo", "LoadID", "", "", false, out iCount);
                lblError.Text = DisplayMessage("Review unarchived.", false);
                btnArchive.Attributes.Add("style", "display:none");
                btnUnarchive.Attributes.Add("style", "display:none");
                mgLoads.SelectedIndex = -1;
                SendEmail("The " + sLoadName + " has been re-activated", "The " + sLoadName + " has been re-activated");
            }
            catch (Exception ex)
            {
                lblError.Text = DisplayMessage("Error: " + ex.Message, true);
            }

        }
        protected void SendEmail(string sSubject, string sMessageText)
        {
            try
            {
                var iCnt = 0;
                var sSentTo = "";
                var sMessageHeader = "<font color='red'>*** This is an auto-generated email. Please do not reply or forward to ***</font><br><br>";
                sMessageText = sMessageHeader + sMessageText;
                Admin.SendCustomEmailToBDAdmins(sMessageText, sSubject, out sSentTo, out iCnt);
                lblError.Text = DisplayMessage("Message has been sent to the ULO BA (" + sSentTo + ")", false);
            }
            catch(Exception ex)
            {
                lblError.Text = DisplayMessage("Request has been processed but email message has not been sent due to the following error: " + ex.Message,false);
            }
        }
}
}
