using OpenItems.Data;
using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Configuration;
    using System.Collections;
    using System.Web.UI.WebControls;
    using Data;

    public class CriteriaSubmitEventArgs : EventArgs
    {
        public int LoadID { get; set; }

        public string Organization { get; set; }
    }

    public partial class Controls_CriteriaFields : System.Web.UI.UserControl
    {
        bool _valHistSearch = false;
        //TODO: Create UserControl Base class that defines these. Not able to use constructor.
        private static readonly IDataLayer Dal = new DataLayer(new zoneFinder(), new ULOContext());
        private readonly Lookups LookupsBO = new Lookups(Dal, new AdminBO(Dal));

        public delegate void CriteriaSubmitEventHandler(object sender, CriteriaSubmitEventArgs e);
        public event CriteriaSubmitEventHandler Submit;

        public event System.EventHandler ClearGridResults;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (IsPostBack)
                {
                    txtLoadTypeID.AddDisplayNone();
                    //ddlLoad.Attributes["onChange"] = "DispayReportButtons();";


                    lblCriteriaMsg.Text = "";
                }
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            ddlLoad.Attributes["onChange"] = "DispayReportButtons();";
            //set default values:
            AddWorkloadFilterValue = true;
            AddBudgetDivisionFilterValue = false;
            DisplaySearchFields = false;
            DisplaySubmitSection = true;
            DefaultOrganization = "0";
            LoadID_IsRequired = true;

            //events:
            ddlLoad.SelectedIndexChanged += new EventHandler(ddlLoad_SelectedIndexChanged);
            ddlViewFilter.SelectedIndexChanged += new EventHandler(ddlViewFilter_SelectedIndexChanged);
            btnSubmit.Click += new EventHandler(btnSubmit_Click);
            //chkUnArchive.CheckedChanged += new EventHandler(chkUnArchive_CheckedChanged);
            //chkArchive.CheckedChanged += new EventHandler(chkArchive_CheckedChanged);
        }

        void ddlLoad_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                //erase event ClearGrid:

                if (this.ClearGridResults != null)
                    this.ClearGridResults(this, new EventArgs());

                UpdateControlsByLoad();
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }

        /* 
        void chkArchive_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chkArchive.Checked)
                {                    
                    int load_id = Int32.Parse(ddlLoad.SelectedValue);
                    Items.ArchiveItems(load_id, (new PageBase()).CurrentUserID);

                    LoadListTable = Lookups.GetLoadFullInfoList(true).Copy();
                    if (!LoadID_IsRequired)
                    {
                        DataRow dr = LoadListTable.NewRow();
                        dr["LoadID"] = 0;
                        dr["LoadDesc"] = "";
                        LoadListTable.Rows.InsertAt(dr, 0);
                    }
                    ddlLoad.DataSource = LoadListTable;
                    ddlLoad.DataValueField = "LoadID";
                    ddlLoad.DataTextField = "LoadDesc";
                    ddlLoad.DataBind();

                    ddlLoad.SelectedValue = load_id.ToString();
                    UpdateControlsByLoad();
                }
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }*/

        /*
        void chkUnArchive_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chkUnArchive.Checked)
                {
                    int load_id = Int32.Parse(ddlLoad.SelectedValue);
                    Items.UnArchiveItems(load_id, (new PageBase()).CurrentUserID);

                    LoadListTable = Lookups.GetLoadFullInfoList(true).Copy();
                    if (!LoadID_IsRequired)
                    {
                        DataRow dr = LoadListTable.NewRow();
                        dr["LoadID"] = 0;
                        dr["LoadDesc"] = "";
                        LoadListTable.Rows.InsertAt(dr, 0);
                    }
                    ddlLoad.DataSource = LoadListTable;
                    ddlLoad.DataValueField = "LoadID";
                    ddlLoad.DataTextField = "LoadDesc";
                    ddlLoad.DataBind();

                    ddlLoad.SelectedValue = load_id.ToString();
                    UpdateControlsByLoad();
                }
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }*/

        void btnSubmit_Click(object sender, EventArgs e)
        {
            if (sender != null && ((Button)sender).ID.ToUpper() == "BTNSUBMIT")
            {
                if (this.Parent.ClientID == "frmSearch")
                    ((PageBase)(this.Parent.Parent)).ItemsSortExpression = "DocNumber, LoadDate DESC";
                this.ValHistSearch = false;
            }
            else if (sender != null && ((Button)sender).ID.ToUpper() == "BTNVALHISTORY")
            {
                ((PageBase)(this.Parent.Parent)).ItemsSortExpression = "DocNumber, LineNum DESC";
                this.ValHistSearch = true;
            }
            try
            {
                var organization = ddlViewFilter.SelectedValue;
                if (organization == OIConstants.OpenItemsGridFilter_TotalUniverse)
                    organization = ((int)OpenItemsGridFilter.gfTotalUniverse).ToString();
                if (organization == OIConstants.OpenItemsGridFilter_MyWorkload)
                    organization = ((int)OpenItemsGridFilter.gfMyWorkload).ToString();

                //first erase event to reset Grid results:
                if (this.ClearGridResults != null)
                    this.ClearGridResults(this, new EventArgs());

                //then erase click on submit button event:
                if (this.Submit != null)
                {
                    var arg = new CriteriaSubmitEventArgs();
                    arg.LoadID = Int32.Parse(ddlLoad.SelectedValue);
                    arg.Organization = organization;
                    this.Submit(this, arg);
                }
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }

        void ddlViewFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!tblSearch.Visible && btnSubmit.Enabled)
            {
                btnSubmit_Click(null, null);
            }
        }

        private void UpdateControlsByLoad()
        {
            chkArchive.Checked = false;
            chkUnArchive.Checked = false;

            //update DataSource label:
            if (ddlLoad.SelectedValue != "0")
            {
                var dr = LoadListTable.Select("LoadID = " + ddlLoad.SelectedValue)[0];
                lblSource.Text = dr["DataSourceDescription"].ToString();
                lblRound.Text = GetReviewRoundDesc((int)dr["ReviewRound"]);
                lblDueDate.Text = ((DateTime)dr["DueDate"]).ToString("MMM dd, yyyy");

                if ((DateTime)dr["DueDate"] <= DateTime.Now && dr["ArchiveDate"] == DBNull.Value)
                    lblDueDate.CssClass = "regBldRedText";
                else
                    lblDueDate.CssClass = "";

                if (dr["ArchiveDate"] != DBNull.Value)
                {
                    chkArchive.Visible = false;
                    SelectedLoadIsArchived = true;
                    if (DisplayArchive && this.Page.User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                        chkUnArchive.Visible = true;
                }
                else
                {
                    chkUnArchive.Visible = false;
                    SelectedLoadIsArchived = false;
                    if (DisplayArchive && this.Page.User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                        chkArchive.Visible = true;
                }
                SelectedLoadParentID = (int)dr["ParentLoadID"];
                SelectedOpenItemTypeID = (int)dr["OIType"];
                txtLoadTypeID.Text = dr["OIType"].ToString();
                //ItemTypeCode = lblLoadTypeID.Text;
                //lblLoadTypeID.Text = SelectedOpenItemTypeID.ToString();

                Session["ReportType"] = txtLoadTypeID.Text;

            }
            else
            {
                lblSource.Text = "";
                lblDueDate.Text = "";
                lblRound.Text = "";
                chkUnArchive.Visible = false;
                SelectedLoadIsArchived = false;
                SelectedLoadParentID = 0;
                SelectedOpenItemTypeID = 0;
            }
        }

        private string GetReviewRoundDesc(int ReviewRound)
        {
            var result = String.Format("{0}", ReviewRound);
            if (result.Length == 1)
                switch (result)
                {
                    case "1":
                        result += "st";
                        break;
                    case "2":
                        result += "nd";
                        break;
                    case "3":
                        result += "rd";
                        break;
                    default:
                        result += "th";
                        break;
                }
            return result;
        }

        #region PublicProperties

        public bool DisplayArchiveOption
        {
            get { return DisplayArchive; }
            set { DisplayArchive = value; }
        }

        public bool AddWorkloadFilterValue { get; set; }

        public bool AddBudgetDivisionFilterValue { get; set; }

        public bool LoadSelectionIsRequired
        {
            set { LoadID_IsRequired = value; }
        }

        public bool DisplaySearchFields { get; set; }

        public bool DisplaySubmitSection { get; set; }

        public string SubmitButtonText
        {
            set { btnSubmit.Text = value; }
        }



        public int ItemTypeCode
        {
            get { return SelectedOpenItemTypeID; }
            //set { _SelectedOpenItemTypeID = value; }
        }

        public string LoadRound
        {
            get { return LoadRoundID; }
            //set { _SelectedOpenItemTypeID = value; }
        }

        public int LoadID
        {
            get { return Int32.Parse(ddlLoad.SelectedValue); }
        }

        public int ParentLoadID
        {
            get { return SelectedLoadParentID; }
        }

        public bool LoadIsArchived
        {
            get { return SelectedLoadIsArchived; }
        }

        public string DefaultOrganization { get; set; }

        public string Organization
        {
            get
            {
                var organization = ddlViewFilter.SelectedValue;
                if (organization == OIConstants.OpenItemsGridFilter_TotalUniverse)
                    organization = ((int)OpenItemsGridFilter.gfTotalUniverse).ToString();
                if (organization == OIConstants.OpenItemsGridFilter_MyWorkload)
                    organization = ((int)OpenItemsGridFilter.gfMyWorkload).ToString();

                return organization;
            }
        }

        public string DocNumber
        {
            get { return txtDocNumber.Text.Trim(); }
        }

        public string ProjNumber
        {
            get { return txtProjNum.Text.Trim(); }
        }

        public string BA
        {
            get { return txtBA.Text.Trim(); }
        }

        public string AwardNumber
        {
            get { return txtAwardNum.Text.Trim(); }
        }

        public bool LatestReview
        {
            get { return chkLatestReview.Checked; }
        }

        public bool ValHistSearch
        {
            get
            {
                if (ViewState["ValHistSearch"] != null)
                    return (bool)ViewState["ValHistSearch"];
                else return _valHistSearch;
            }
            set
            {
                _valHistSearch = value;
                ViewState["ValHistSearch"] = value;
            }
        }

        #endregion PublicProperties

        #region PublicMethods

        public void DisplayPrevSelectedValues(Hashtable StorageObject)
        {

            if (StorageObject != null)
            {
                ddlLoad.SelectedValue = (string)StorageObject["Load"];
                UpdateControlsByLoad();
                ddlViewFilter.SelectedValue = (string)StorageObject["Filter"];
                if (tblSearch.Visible)
                {
                    txtDocNumber.Text = (string)StorageObject["DocNumber"];
                    txtProjNum.Text = (string)StorageObject["ProjNumber"];
                    txtBA.Text = (string)StorageObject["BA"];
                    txtAwardNum.Text = (string)StorageObject["Award"];
                }
            }
        }

        public Hashtable SaveCurrentSelectedValues()
        {
            var ht = new Hashtable();
            ht.Add("Load", ddlLoad.SelectedValue);
            ht.Add("Filter", ddlViewFilter.SelectedValue);
            if (tblSearch.Visible)
            {
                ht.Add("DocNumber", txtDocNumber.Text.Trim());
                ht.Add("ProjNumber", txtProjNum.Text.Trim());
                ht.Add("BA", txtBA.Text.Trim());
                ht.Add("Award", txtAwardNum.Text.Trim());
            }
            return ht;

        }

        public void InitControls()
        {
            tblSearch.Visible = DisplaySearchFields;
            tblSubmit.Visible = DisplaySubmitSection;

            if (DisplaySearchFields)
                btnValHistory.Visible = true;

            //LoadListTable = Lookups.GetLoadFullInfoList(false).Copy();

            LoadListTable = LookupsBO.GetLoadList().Tables[0].Copy();
            if (!LoadID_IsRequired)
            {
                var dr = LoadListTable.NewRow();
                dr["LoadID"] = 0;
                dr["LoadDesc"] = "";
                LoadListTable.Rows.InsertAt(dr, 0);
            }
            ddlLoad.DataSource = LoadListTable;
            ddlLoad.DataValueField = "LoadID";
            ddlLoad.DataTextField = "LoadDesc";
            ddlLoad.DataBind();

            lblSource.Text = "";

            //init Organizations list:
            var dto = LookupsBO.GetUniqueOrganizationList().Tables[0].Copy();
            var dro = dto.NewRow();
            dro["Organization"] = OIConstants.OpenItemsGridFilter_TotalUniverse;
            dto.Rows.InsertAt(dro, 0);
            if (AddWorkloadFilterValue)
            {
                dro = dto.NewRow();
                dro["Organization"] = OIConstants.OpenItemsGridFilter_MyWorkload;
                dto.Rows.InsertAt(dro, 1);
            }
            else if (AddBudgetDivisionFilterValue)
            {
                dro = dto.NewRow();
                dro["Organization"] = OIConstants.OpenItemsGridFilter_BDResponsibility;
                dto.Rows.InsertAt(dro, 1);
            }
            ddlViewFilter.DataSource = dto;
            ddlViewFilter.DataValueField = "Organization";
            ddlViewFilter.DataTextField = "Organization";
            ddlViewFilter.DataBind();

        }

        public void DisplayDefaults()
        {
            UpdateControlsByLoad();

            //select default organization for current logged on user:
            ddlViewFilter.SelectedValue = DefaultOrganization;

            //ddlViewFilter_SelectedIndexChanged(null, null);
            if (!tblSearch.Visible && btnSubmit.Enabled)
                btnSubmit_Click(null, null);

            //this.btnValHistory.Visible = false;

            //if (this.Parent.ClientID == "frmSearch") 
            //this.btnValHistory.Visible = true;
        }

        public void DisplayDefaultsRep()
        {
            UpdateControlsByLoad();

            //select default organization for current logged on user:
            //ddlViewFilter.SelectedValue = _default_selected_org;

            //ddlViewFilter_SelectedIndexChanged(null, null);
            if (!tblSearch.Visible && btnSubmit.Enabled)
                btnSubmit_Click(null, null);

            //this.btnValHistory.Visible = false;

            //if (this.Parent.ClientID == "frmSearch") 
            //this.btnValHistory.Visible = true;
        }

        #endregion PublicMethods

        #region PrivateProperties

        private DataTable LoadListTable
        {
            get
            {
                if (ViewState["LoadListTable"] != null)
                    return (DataTable)ViewState["LoadListTable"];
                else
                    return null;
            }
            set
            {
                ViewState["LoadListTable"] = value;
            }
        }

        private bool DisplayArchive
        {
            get
            {
                if (ViewState["DisplayArchive"] != null)
                    return (bool)ViewState["DisplayArchive"];
                else
                    return Settings.Default.DisplayArchiveOption;
            }
            set
            {
                ViewState["DisplayArchive"] = value && Settings.Default.DisplayArchiveOption;
            }
        }

        private bool LoadID_IsRequired
        {
            get
            {
                if (ViewState["LoadIDIsRequired"] != null)
                    return (bool)ViewState["LoadIDIsRequired"];
                else
                    return true;
            }
            set
            {
                ViewState["LoadIDIsRequired"] = value;
            }
        }

        private bool SelectedLoadIsArchived
        {
            get
            {
                if (ViewState["IsArchived"] != null)
                    return (bool)ViewState["IsArchived"];
                else
                    return false;
            }
            set
            {
                ViewState["IsArchived"] = value;
            }
        }

        private int SelectedLoadParentID
        {
            get
            {
                if (ViewState["ParentLoadID"] != null)
                    return (int)ViewState["ParentLoadID"];
                else
                    return 0;
            }
            set
            {
                ViewState["ParentLoadID"] = value;
            }
        }

        private string LoadRoundID
        {
            get
            {
                if (lblRound.Text.Trim() != "")
                {
                    return lblRound.Text.Trim();
                }
                else
                    return "1st";
            }
        }

        private int SelectedOpenItemTypeID
        {
            get
            {
                if (ViewState["OpenItemTypeID"] != null)
                {
                    if (txtLoadTypeID.Text == "6")
                    {
                        return 6;
                    }
                    else
                    {
                        return (int)ViewState["OpenItemTypeID"];
                        //return 1; 
                    }

                }
                else
                {
                    return 0;
                }
            }
            set
            {
                ViewState["OpenItemTypeID"] = value;
            }
        }

        #endregion PrivateProperties




        protected void btnValHistory_Click(object sender, EventArgs e)
        {
            ValHistSearch = true;
            btnSubmit_Click(sender, e);
        }
    }
}