using System.Linq;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Configuration;
    using System.Data.SqlClient;

    public partial class Justification : PageBase
    {
        private readonly AdminBO Admin;
        public Justification()
        {
            Admin = new AdminBO(this.Dal);
        }
        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            if (IsPostBack == false)
            {
                tr_chkAddField.AddDisplayNone();
                tr_ddlJustTypes.AddDisplayNone();
                tr_txtJustTypes.AddDisplayNone();
                tr_line2.AddDisplayNone();
                btnSave.Visible = false;
                FillDDList();
            }

        }
        protected void FillDDList()
        {
            var justificationTypes = Admin.GetDefaultJustificationTypes(); 
            ddlJustTypes.Items.Clear();
            ddlJustTypes.DataSource = justificationTypes;
            ddlJustTypes.DataTextField = "JustificationDescription";
            ddlJustTypes.DataValueField = "Justification";
            ddlJustTypes.DataBind();
            ddlJustTypes.Items.Insert(0, "- select -");
        }
        protected void rbEditOrNew_SelectedIndexChanged(object sender, EventArgs e)
        {
            tr_chkAddField.AddDisplay();
            tr_txtAddFieldValue.AddDisplayNone();
            tr_line2.AddDisplay();
            tr_txtJustTypes.AddDisplay();
            btnSave.Visible = true;
            txtJustType.Text = "";
            lblError.Text = "";

            if (rbEditOrNew.SelectedIndex == 0) // new Justification
            {
                tr_ddlJustTypes.AddDisplayNone();
                txtJustType.ToolTip = "Enter a new Justification value";
                hJustTypeCode.Value = "0";
                chkAddField.Enabled = true;
            }
            else // edit existing Justification
            {
                chkAddField.Enabled = false;
                btnSave.Visible = false;
                tr_ddlJustTypes.AddDisplay();
                txtJustType.ToolTip = "Enter the corrected value";
                hJustTypeCode.Value = "1";
            }
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (txtJustType.Text.Trim() == "")
            {
                lblError.Text = "Justification is a mandatory field";
            }
            else
            {
                try
                {
                    bool DisplayAddOnField;
                    string AddOnDescription;

                    if (txtAddFieldValue.Text.Trim() != "")
                    {
                        DisplayAddOnField = true;
                        AddOnDescription = txtAddFieldValue.Text;
                    }
                    else
                    {
                        DisplayAddOnField = false;
                        AddOnDescription = "";
                    }

                    var iID = Admin.SaveJustification(txtJustType.Text.Trim(), Convert.ToInt16(hJustTypeCode.Value), DisplayAddOnField, AddOnDescription);
                    FillDDList();
                    if (iID == 0)
                    {
                        if (hJustTypeCode.Value == "0") //Create New Justification
                        {
                            lblError.Text = DisplayMessage("Justification has been saved", false);
                        }
                        else
                        {
                            lblError.Text = DisplayMessage("Justification has been updated", false);
                        }
                    }
                    else // error: duplicate record
                    {
                        throw new Exception("Duplicate record error: Justification '" + txtJustType.Text.Trim() + "' already exists in the ULO database");
                    }
                    txtJustType.Text = "";
                    txtAddFieldValue.Text = "";
                }
                catch (Exception ex)
                {
                    lblError.Text = DisplayMessage(ex.Message, true);
                }
            }

        }

        protected void ddlJustTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtJustType.Text = ddlJustTypes.SelectedItem.Text.ToString();
            hJustTypeCode.Value = ddlJustTypes.SelectedItem.Value.ToString();

            if (txtJustType.Text == "- select -")
            {
                btnSave.Visible = false;
                txtAddFieldValue.Text = "";
                tr_txtAddFieldValue.AddDisplayNone();
                txtJustType.Text = "";
                chkAddField.Text = "Add Additional field";
                chkAddField.Enabled = false;

            }
            else
            {
                btnSave.Visible = true;
                chkAddField.Enabled = true;
                // get additional field details (if exists)and display them
                var justificationType = Admin.GetJustificationTypeByID(Convert.ToInt32(ddlJustTypes.SelectedItem.Value)).First();
                var sDisplayAddOnField = justificationType.DisplayAddOnField;
                var sAddOnDescription = justificationType.AddOnDescription;



                if (sDisplayAddOnField)
                {
                    tr_txtAddFieldValue.AddStyle("display:inline");
                    txtAddFieldValue.Text = sAddOnDescription;
                    chkAddField.Text = "Remove Additional field";
                    //chkAddField.Checked = true;
                }
                else
                {
                    tr_txtAddFieldValue.AddDisplayNone();
                    chkAddField.Text = "Add Additional field";
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

    }
}
