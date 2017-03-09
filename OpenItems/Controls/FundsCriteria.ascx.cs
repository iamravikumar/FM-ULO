namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Collections;
    using System.Text;
    using System.Xml;
    using System.Web.UI.WebControls;
    using Data;
    using global::OpenItems.Data;

    public partial class Controls_FundsCriteria : System.Web.UI.UserControl
    {

        const string HT_KEY_BL = "BL";
        const string HT_KEY_BL_NAME = "BLName";
        const string HT_KEY_ORG = "Org";
        const string HT_KEY_YEAR = "Year";
        const string HT_KEY_BA = "BA";
        const string HT_KEY_BOOK_MONTH = "BookMonth";
        const string HT_KEY_VIEW = "View";
        const string HT_KEY_SUM_FUNC = "SumFunc";
        const string HT_KEY_OC = "OC";
        const string HT_KEY_CE = "CE";
        const string HT_KEY_DOC_NUM = "DocNum";
        const string HT_KEY_PARENT_URL = "ParentScreen";

        public event System.EventHandler Submit;
        public event System.EventHandler ExportToExcel;
        public event System.EventHandler RequestEmail;
        public event System.EventHandler ClearSearchResults;

        private readonly FundStatusBO FundStatus = new FundStatusBO(new DataLayer(new zoneFinder(), new ULOContext()));

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (IsPostBack)
                    lblCriteriaMsg.Text = "";

                //if (btnBack.Visible)
                //    RegisterSearchBackScript();

            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            btnSubmit.Click += new EventHandler(btnSubmit_Click);
            btnClear.Click += new EventHandler(btnClear_Click);
            btnEdit.Click += new EventHandler(btnEdit_Click);
            btnEmail.Click += new EventHandler(btnEmail_Click);
            btnExcel.Click += new EventHandler(btnExcel_Click);
            btnExpand.Click += new EventHandler(btnExpand_Click);
            ddlBusinessLine.SelectedIndexChanged += new EventHandler(ddlBusinessLine_SelectedIndexChanged);
        }

        void ddlBusinessLine_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var val = ddlBusinessLine.SelectedValue;
                if (val == "All")
                {
                    //all organizations in list:
                    ddlOrganization.DataSource = OrganizationsDT;
                    ddlOrganization.DataTextField = "ORG_CD";
                    ddlOrganization.DataValueField = "ORG_CD";
                    ddlOrganization.DataBind();
                }
                else
                {
                    var dv = OrganizationsDT.DefaultView;
                    dv.RowFilter = String.Format("BL_CD='{0}'", val);
                    var dt = dv.ToTable();
                    var dr = dt.NewRow();
                    dr["ORG_CD"] = "";
                    dt.Rows.InsertAt(dr, 0);
                    ddlOrganization.DataSource = dt;
                    ddlOrganization.DataTextField = "ORG_CD";
                    ddlOrganization.DataValueField = "ORG_CD";
                    ddlOrganization.DataBind();
                }
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }

        void btnExpand_Click(object sender, EventArgs e)
        {
            try
            {
                ExpandedByMonthView = !ExpandedByMonthView;

                //erase event of Submit button:
                if (this.Submit != null)
                    this.Submit(this, new EventArgs());
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }

        void btnExcel_Click(object sender, EventArgs e)
        {
            try
            {

                //erase event of ExportToExcel button:
                if (this.ExportToExcel != null)
                    this.ExportToExcel(this, new EventArgs());
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }

        void btnEmail_Click(object sender, EventArgs e)
        {
            try
            {
                //erase event of RequestEmail button:
                if (this.RequestEmail != null)
                    this.RequestEmail(this, new EventArgs());
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }

        void btnEdit_Click(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        void btnClear_Click(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                //check selected criteria for Funds Search Documents mode:


                //save criteria:
                SaveSelectedCriteriaInStorage();

                btnEmail.Enabled = true;
                btnExcel.Enabled = true;
                btnExpand.Enabled = true;

                //erase event of Submit button:
                if (this.Submit != null)
                    this.Submit(this, new EventArgs());
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }

        private void RegisterSearchBackScript()
        {

            if (!Page.ClientScript.IsClientScriptBlockRegistered("search_back"))
            {
                var sb = new StringBuilder();
                sb.Append("<script language='javascript'> ");
                sb.Append("function search_go_back(url) ");
                sb.Append("{ location.href= unescape(url); ");
                sb.Append("return false; } ");
                sb.Append("</script>");
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "search_back", sb.ToString());
            }
        }

        #region PrivateProperties

        private DataTable OrganizationsDT
        {
            get
            {
                if (ViewState["ORG_TABLE"] == null)
                {
                    var ds = FundStatus.GetOrganizationList().ToDataSet();
                    if (ds == null || ds.Tables[0].Rows.Count == 0)
                        throw new Exception("Application error. Organization list is missing.");
                    ViewState["ORG_TABLE"] = ds.Tables[0];
                }
                return (DataTable)ViewState["ORG_TABLE"];
            }
            set { ViewState["ORG_TABLE"] = value; }
        }

        private Hashtable CriteriaStorage
        {
            get
            {
                if (ViewState["SELECTED_HT"] == null)
                    return null;
                else
                    return (Hashtable)ViewState["SELECTED_HT"];
            }
            set { ViewState["SELECTED_HT"] = value; }
        }

        private string ParentScreenURL
        {
            get
            {
                if (ViewState["HT_KEY_PARENT_URL"] == null)
                    return "";
                else
                    return (string)ViewState["HT_KEY_PARENT_URL"];
            }
            set { ViewState["HT_KEY_PARENT_URL"] = value; }
        }

        private void SaveSelectedCriteriaInStorage()
        {
            var ht = new Hashtable();

            ht.Add(HT_KEY_BL, ddlBusinessLine.SelectedValue);
            ht.Add(HT_KEY_BL_NAME, ddlBusinessLine.SelectedItem.Text);
            ht.Add(HT_KEY_ORG, ddlOrganization.SelectedValue);
            ht.Add(HT_KEY_YEAR, ddlFiscalYear.SelectedValue);
            ht.Add(HT_KEY_BOOK_MONTH, ddlBookMonth.SelectedValue);
            ht.Add(HT_KEY_PARENT_URL, ParentScreenURL);

            if (trViewSelection.Visible)
                ht.Add(HT_KEY_VIEW, ddlView.SelectedValue);
            else
                ht.Add(HT_KEY_VIEW, "");

            if (trSearchCriteria.Visible)
            {
                //get multiple selections of sum functions:
                var arr = "";
                foreach (ListItem li in lstSumFunc.Items)
                {
                    if (li.Selected)
                        arr = arr + li.Value + ",";
                }
                if (arr != "")
                    arr = arr.Substring(0, arr.Length - 1);
                ht.Add(HT_KEY_SUM_FUNC, arr);

                //get multiple selections of oc codes:
                arr = "";
                foreach (ListItem li in lstOCCode.Items)
                {
                    if (li.Selected)
                        arr = arr + li.Value + ",";
                }
                if (arr != "")
                    arr = arr.Substring(0, arr.Length - 1);
                ht.Add(HT_KEY_OC, arr);

                ht.Add(HT_KEY_DOC_NUM, txtDocNumber.Text.Trim());
            }
            else
            {
                ht.Add(HT_KEY_SUM_FUNC, "");
                ht.Add(HT_KEY_OC, "");
                ht.Add(HT_KEY_DOC_NUM, "");
            }
            CriteriaStorage = ht;
        }

        private void DisplaySelectedCriteriaFromStorage()
        {
            if (CriteriaStorage != null)
            {
                string[] arr;
                ddlBusinessLine.SelectedValue = (string)CriteriaStorage[HT_KEY_BL];

                ddlOrganization.SelectedValue = (string)CriteriaStorage[HT_KEY_ORG];

                ddlFiscalYear.SelectedValue = (string)CriteriaStorage[HT_KEY_YEAR];

                ddlBookMonth.SelectedValue = (string)CriteriaStorage[HT_KEY_BOOK_MONTH];

                ddlView.SelectedValue = (string)CriteriaStorage[HT_KEY_VIEW];


                arr = ((string)CriteriaStorage[HT_KEY_SUM_FUNC]).Split(new char[] { ',' });
                foreach (var arr_value in arr)
                {
                    lstSumFunc.Items.FindByValue(arr_value).Selected = true;
                }

                arr = ((string)CriteriaStorage[HT_KEY_OC]).Split(new char[] { ',' });
                foreach (var arr_value in arr)
                {
                    lstOCCode.Items.FindByValue(arr_value).Selected = true;
                }

                txtDocNumber.Text = (string)CriteriaStorage[HT_KEY_DOC_NUM];

                ParentScreenURL = "";
                ParentScreenURL = (string)CriteriaStorage[HT_KEY_PARENT_URL];

                if (ParentScreenURL != "")
                {
                    btnBack.Visible = true;
                    btnBack.AddOnClick("javascript:return search_go_back('" + ParentScreenURL + "');");
                    RegisterSearchBackScript();
                }
            }

        }

        #endregion PrivateProperties

        #region PublicFunctions        

        public void InitControls()
        {


            DataRow dr;
            DataSet ds;
            //init Business Line list:
            ds = FundStatus.GetBusinessLineList().ToDataSet();
            if (ds == null || ds.Tables[0].Rows.Count == 0)
                throw new Exception("Application error. Business Line list is missing.");
            ddlBusinessLine.DataSource = ds.Tables[0];
            ddlBusinessLine.DataTextField = "BLDesc";
            ddlBusinessLine.DataValueField = "BL_CD";
            ddlBusinessLine.DataBind();

            //init Organizations list:
            DataTable dt;
            if (ScreenType == (int)FundsStatusScreenType.stFundsSearch)
            {
                dt = OrganizationsDT.Copy();
                dr = dt.NewRow();
                dr["ORG_CD"] = "";
                dt.Rows.InsertAt(dr, 0);
            }
            else
                dt = OrganizationsDT;
            ddlOrganization.DataSource = dt;
            ddlOrganization.DataTextField = "ORG_CD";
            ddlOrganization.DataValueField = "ORG_CD";
            ddlOrganization.DataBind();

            //init fiscal year list:                
            ddlFiscalYear.DataSource = LoadFiscalYearList();
            ddlFiscalYear.DataTextField = "Text";
            ddlFiscalYear.DataValueField = "Value";
            ddlFiscalYear.DataBind();
            ddlFiscalYear.SelectedValue = DateTime.Now.Year.ToString();

            ArrayList al;
            ListItem li;

            if (ScreenType != (int)FundsStatusScreenType.stFundSummaryReport)
            {
                //init book month list for selected fiscal year:
                al = new ArrayList();
                if (ScreenType == (int)FundsStatusScreenType.stFundsSearch)
                {
                    li = new ListItem("", "");
                    al.Add(li);
                }
                if (ScreenType == (int)FundsStatusScreenType.stFundsReview)
                {
                    li = new ListItem("All Available", "00");
                    al.Add(li);
                }
                var str_month = "";
                for (var i = 1; i < 13; i++)
                {
                    str_month = String.Format("{0:MMMM}", DateTime.Parse((i < 10 ? "0" + i.ToString() : i.ToString()) + "/2000"));
                    li = new ListItem(str_month, i < 10 ? "0" + i.ToString() : i.ToString());
                    al.Add(li);
                }
                ddlBookMonth.DataSource = al;
                ddlBookMonth.DataTextField = "Text";
                ddlBookMonth.DataValueField = "Value";
                ddlBookMonth.DataBind();
            }
            else
            {
                lblBookMonth.Visible = false;
                ddlBookMonth.Visible = false;
            }

            if (ScreenType == (int)FundsStatusScreenType.stFundsReview ||
                ScreenType == (int)FundsStatusScreenType.stFundsSearch)
            {
                //init view mode:
                al = new ArrayList();
                li = new ListItem("Obligations", ((int)FundsReviewViewMode.fvObligations).ToString());
                al.Add(li);
                li = new ListItem("Income", ((int)FundsReviewViewMode.fvIncome).ToString());
                al.Add(li);
                li = new ListItem("One Time Adjustments", ((int)FundsReviewViewMode.fvOneTimeAdjustments).ToString());
                al.Add(li);
                ddlView.DataSource = al;
                ddlView.DataTextField = "Text";
                ddlView.DataValueField = "Value";
                ddlView.DataBind();
            }
            else
            {
                trViewSelection.Visible = false;
            }

            if (ScreenType == (int)FundsStatusScreenType.stFundsSearch)
            {
                //init Summary Functions list:
                ds = FundStatus.GetSummaryFunctionsList().ToDataSet();
                dr = ds.Tables[0].NewRow();
                dr["FUNC_CD"] = "";
                ds.Tables[0].Rows.InsertAt(dr, 0);
                lstSumFunc.DataSource = ds.Tables[0];
                lstSumFunc.DataTextField = "FUNC_CD";
                lstSumFunc.DataValueField = "FUNC_CD";
                lstSumFunc.DataBind();

                //init all object class codes:
                ds = FundStatus.GetObjectClassCodeList().ToDataSet();
                dr = ds.Tables[0].NewRow();
                dr["OBJ_CLASS_CD"] = "";
                dr["OC_DESC"] = "";
                ds.Tables[0].Rows.InsertAt(dr, 0);
                lstOCCode.DataSource = ds.Tables[0];
                lstOCCode.DataTextField = "OC_DESC";
                lstOCCode.DataValueField = "OBJ_CLASS_CD";
                lstOCCode.DataBind();

                btnClear.Visible = true;
                btnEmail.Visible = true;
            }
            else
                trSearchCriteria.Visible = false;

            if (ScreenType == (int)FundsStatusScreenType.stFundsReview)
                btnExpand.Visible = true;

            if (ScreenType == (int)FundsStatusScreenType.stFundStatusReport || ScreenType == (int)FundsStatusScreenType.stFundSummaryReport)
            {
                btnSubmit.Text = "View Report";
                btnSubmit.ToolTip = "View Report";
            }
            else
            {
                btnSubmit.Text = "Search";
                btnSubmit.ToolTip = "Search";
            }

            //add client side functions:
            btnExcel.AddOnClick("javascript:return funds_data_to_excel(" + ScreenType.ToString() + ");");


        }

        private ArrayList LoadFiscalYearList()
        {
            var al = new ArrayList();
            ListItem li;
            var doc = new XmlDocument();
            doc.Load(Request.PhysicalApplicationPath + "FundStatus\\include\\FS_SupportedFiscalYears.xml");
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                li = new ListItem(node.InnerText);
                al.Add(li);
            }
            return al;
        }

        public void DisplayPrevSelectedValues(Hashtable StorageObject)
        {
            if (StorageObject != null)
            {
                CriteriaStorage = StorageObject;
                DisplaySelectedCriteriaFromStorage();
            }
            //display previously selected values can happen only on returning back to the previously screen
            //in this case we want make all buttons, related to previously showed search result, enabled:
            btnEmail.Enabled = true;
            btnExcel.Enabled = true;
            btnExpand.Enabled = true;
        }

        public Hashtable SaveCurrentSelectedValues()
        {
            SaveSelectedCriteriaInStorage();
            return CriteriaStorage;
        }

        public void AllowReturnBack(string ReturnURL)
        {
            ParentScreenURL = ReturnURL;
            btnBack.Visible = true;
            btnBack.AddOnClick("javascript:return search_go_back('" + ReturnURL + "');");

            RegisterSearchBackScript();

            //AllowReturnBack means we already have selected criteria and displayed search results.
            //in this case we want make all buttons, related to showed search result, enabled:
            btnEmail.Enabled = true;
            btnExcel.Enabled = true;
            btnExpand.Enabled = true;
        }


        #endregion PublicFunctions

        #region PublicProperties

        public bool ExpandedByMonthView
        {
            get
            {
                if (btnExpand.Text.Substring(0, 3) == "Exp" || ViewState["EXP_VIEW"] == null || ViewState["EXP_VIEW"] == "0")
                    return false;
                else
                    return true;
            }
            set
            {
                if (value)
                {
                    ViewState["EXP_VIEW"] = "1";
                    btnExpand.Text = "Collapse to Total";
                    btnExpand.ToolTip = "Get Totals only";
                }
                else
                {
                    ViewState["EXP_VIEW"] = "0";
                    btnExpand.Text = "Expand by Month";
                    btnExpand.ToolTip = "Get Expanded View by Month";
                }
            }
        }

        public string FiscalYear
        {
            get { return (string)CriteriaStorage[HT_KEY_YEAR]; }
            set { ddlFiscalYear.SelectedValue = value; }
        }

        public string BusinessLine
        {
            get { return (string)CriteriaStorage[HT_KEY_BL]; }
            set
            {
                ddlBusinessLine.SelectedValue = value.ToString();
                ddlBusinessLine_SelectedIndexChanged(null, null);
            }
        }

        public string Organization
        {
            get { return (string)CriteriaStorage[HT_KEY_ORG]; }
            set { ddlOrganization.SelectedValue = value; }
        }

        public int ViewMode
        {
            get { return Int32.Parse((string)CriteriaStorage[HT_KEY_VIEW]); }
            set { ddlView.SelectedValue = value.ToString(); }
        }

        //public string ExportToExcelClientFunction
        //{
        //    set { btnExcel.AddOnClick("javascript:return " + value + ";"); }
        //}

        //public string RequestEmailClientFunction
        //{
        //    set { btnEmail.AddOnClick("javascript:return " + value + ";"); }
        //}

        public int ScreenType
        {
            get
            {
                if (ViewState["SCREEN_TYPE"] == null)
                    return (int)FundsStatusScreenType.stFundsReview;
                else
                    return (int)ViewState["SCREEN_TYPE"];
            }
            set { ViewState["SCREEN_TYPE"] = value; }
        }

        public string DocNumber
        {
            get { return (string)CriteriaStorage[HT_KEY_DOC_NUM]; }
            set { txtDocNumber.Text = value; }
        }

        public string BookMonth
        {
            get { return (string)CriteriaStorage[HT_KEY_BOOK_MONTH]; }
            set { ddlBookMonth.SelectedValue = value; }
        }

        public string SummaryFunction
        {
            //gets or sets string - list of values separated by ','
            get
            {
                //string arr = "";
                //foreach (ListItem li in lstSumFunc.Items)
                //{
                //    if (li.Selected)
                //        arr = arr + li.Value + ",";
                //}
                //if (arr != "")
                //    arr = arr.Substring(0, arr.Length - 1);

                return (string)CriteriaStorage[HT_KEY_SUM_FUNC];
            }
            set
            {
                var arr = value.Split(new char[] { ',' });
                foreach (var arr_value in arr)
                {
                    lstSumFunc.Items.FindByValue(arr_value).Selected = true;
                }
            }
        }

        public string ObjClassCode
        {
            //gets or sets string - list of values separated by ','
            get
            {
                //string arr = "";
                //foreach (ListItem li in lstOCCode.Items)
                //{
                //    if (li.Selected)
                //        arr = arr + li.Value + ",";
                //}
                //if (arr != "")
                //    arr = arr.Substring(0, arr.Length - 1);

                return (string)CriteriaStorage[HT_KEY_OC];
            }
            set
            {
                var arr = value.Split(new char[] { ',' });
                foreach (var arr_value in arr)
                {
                    lstOCCode.Items.FindByValue(arr_value).Selected = true;
                }
            }
        }

        #endregion PublicProperties
    }
}