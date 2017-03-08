namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Configuration;
    using System.Data.SqlClient;

    public partial class AttachmentType : PageBase
    {
        private readonly AdminBO Admin;
        private readonly DocumentBO Document;
        public AttachmentType()
        {
            Admin = new AdminBO(this.Dal);
            Document = new DocumentBO(this.Dal);
        }

        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            if (IsPostBack == false)
            {
                tr_ddlAttTypes.Attributes.Add("style", "display:none");
                tr_txtAttTypes.Attributes.Add("style", "display:none");
                tr_line2.Attributes.Add("style", "display:none");
                btnSave.Visible = false;
                FillDDList();
            }

        }
        protected void FillDDList()
        {
            var ds = Admin.GetAllAttachmentTypes(); //active and inactive
            ddlAttTypes.Items.Clear();
            ddlAttTypes.DataSource = ds;
            ddlAttTypes.DataTextField = ds.Tables[0].Columns["DocTypeName"].ToString();
            ddlAttTypes.DataValueField = ds.Tables[0].Columns["DocTypeCode"].ToString();
            ddlAttTypes.DataBind();
            ddlAttTypes.Items.Insert(0, "- select -");
        }
        protected void rbEditOrNew_SelectedIndexChanged(object sender, EventArgs e)
        {
            tr_line2.Attributes.Add("style", "display:");
            tr_txtAttTypes.Attributes.Add("style", "display:");
            btnSave.Visible = true;
            txtAttType.Text = "";
            lblError.Text = "";

            if (rbEditOrNew.SelectedIndex == 0) // new attachment type
            {
                tr_ddlAttTypes.Attributes.Add("style", "display:none");
                txtAttType.ToolTip = "Enter a new attachment type value";
                hDocTypeCode.Value = "0";
            }
            else // edit existing attachment type
            {
                btnSave.Visible = false;
                tr_ddlAttTypes.Attributes.Add("style", "display:");
                txtAttType.ToolTip = "Edit the attachment type value";
                hDocTypeCode.Value = "1";
            }
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            var iID = 0;

            if (txtAttType.Text.Trim() == "")
            {
                //lblError.Text = "Attachment Type is a mandatory field";
                lblError.Text = DisplayMessage("Attachment Type is a mandatory field", true);
            }
            else
            {
                try
                {
                    if (hDocTypeCode.Value.Trim() == "")
                    {
                        hDocTypeCode.Value = "0"; //Create New Attachment Type
                    }

                    Document.SaveAttachmentType(txtAttType.Text.Trim(), Convert.ToInt16(hDocTypeCode.Value), out iID);
                    FillDDList();
                    if (iID == 0)
                    {
                        if (hDocTypeCode.Value == "0") //Create New Attachment Type
                        {
                            lblError.Text = DisplayMessage("Attachment Type has been saved", false);
                        }
                        else
                        {
                            lblError.Text = DisplayMessage("Attachment Type has been updated", false);
                        }
                    }
                    else // error: duplicate record
                    {
                        throw new Exception("Duplicate record error: AttachmentType '" + txtAttType.Text.Trim() + "' already exists in the ULO database");
                    }
                    txtAttType.Text = "";
                }
                catch(Exception ex)
                {
                    lblError.Text = DisplayMessage(ex.Message, true);
                }
            }

        }

        protected void ddlAttTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtAttType.Text = ddlAttTypes.SelectedItem.Text.ToString();
            hDocTypeCode.Value = ddlAttTypes.SelectedItem.Value.ToString();
            if (hDocTypeCode.Value == "- select -")
            {
                btnSave.Visible = false;
                txtAttType.Text = "";
            }
            else
            {
                btnSave.Visible = true;
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
       
}
}
