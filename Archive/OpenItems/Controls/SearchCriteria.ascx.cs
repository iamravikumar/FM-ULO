using OpenItems.Data;
using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Configuration;
    using System.Collections;
    using System.Xml;
    using Data;

    public partial class Controls_SearchCriteria : System.Web.UI.UserControl
    {
        const string HT_KEY_BL = "BL";
        const string HT_KEY_ORG = "Org";
        const string HT_KEY_YEAR = "Year";
        const string HT_KEY_BA = "BA";
        const string HT_KEY_BOOK_MONTH = "BookMonth";
        const string HT_KEY_VIEW = "View";
        const string HT_KEY_GROUP = "Group";
        const string HT_KEY_SUM_FUNC = "SumFunc";
        const string HT_KEY_OC = "OC";
        const string HT_KEY_CE = "CE";
        const string HT_KEY_DOC_NUM = "DocNum";
        const string HT_KEY_PARENT_URL = "ParentScreen";
        static string[] Month = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        public event EventHandler Submit;

        private readonly FundStatusBO FundStatus = new FundStatusBO(new DataLayer(new zoneFinder(), new ULOContext()));

        public bool VMode
        {
            get { return aView.Visible; }
            set { aView.Visible = false; }
        }
        public bool GroupFunc
        {
            get { return aGroup.Visible; }
            set { aGroup.Visible = false; }
        }
        public bool Function
        {
            get { return aFunc.Visible; }
            set { aFunc.Visible = false; }
        }
        public bool OCC
        {
            get { return aOCC.Visible; }
            set { aOCC.Visible = false; }
        }
        public bool CostElem
        {
            get { return aCE.Visible; }
            set { aCE.Visible = false; }
        }
        private int IndexDG(DataTable dt, string ID)
        {

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][0].ToString() == ID)
                    return i;
            }
            return 0;
        }
        public bool Doc
        {
            get { return aDoc.Visible; }
            set { aDoc.Visible = false; }
        }
        public string FiscalYear
        {
            get { return ddlFiscalYear.ItemID; }
            set { ddlFiscalYear.SelectedIndex = IndexDG(ddlFiscalYear.Table, value); }
        }
        public string BudgetActivity
        {
            get { return ddgBA.ItemID; }
            set { ddgBA.SelectedIndex = IndexDG(ddgBA.Table, value); }
        }

        public string BusinessLine
        {
            get { return ddgBLine.ItemID; }
            set
            {
                ddgBLine.SelectedIndex = IndexDG(ddgBLine.Table, value);
                ddlOrganization.SelectedIndexes = SetMultiByParent(ddlOrganization.Table, value);
            }
        }
        public string BusinessLineName
        {
            get { return ddgBLine.Table.Rows[ddgBLine.SelectedIndex][1].ToString(); }
        }

        public int ViewMode
        {
            get
            {
                if (ddlView.SelectedIndex == 0)
                    return (int)FundsReviewViewMode.fvObligations;
                if (ddlView.SelectedIndex == 1)
                    return (int)FundsReviewViewMode.fvIncome;
                return (int)FundsReviewViewMode.fvOneTimeAdjustments;
            }
            set
            {
                if ((int)FundsReviewViewMode.fvObligations == value)
                    ddlView.SelectedIndex = 0;
                else if ((int)FundsReviewViewMode.fvIncome == value)
                    ddlView.SelectedIndex = 1;
                else if ((int)FundsReviewViewMode.fvOneTimeAdjustments == value)
                    ddlView.SelectedIndex = 2;
            }
        }
        public string DocNumber
        {
            get { return txtDocNumber.Text; }
            set { txtDocNumber.Text = value; }
        }
        private string GetMulti(DataTable dt, int[] ii)
        {
            var s = "";
            foreach (var i in ii)
            {
                s += dt.Rows[i][0].ToString() + ",";
            }
            if (s.Length > 1)
                s = s.Substring(0, s.Length - 1);
            return s;
        }
        private int[] SetMulti(DataTable dt, string s)
        {
            int[] iIndexes;

            var list = new ArrayList();
            //ArrayList names_list = new ArrayList(); //check list due to not select doubles    //&& !names_list.Contains(dt.Rows[i][0].ToString())
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                if (s.Contains(dt.Rows[i][0].ToString()) && !list.Contains(i))
                {
                    list.Add(i);
                    //names_list.Add(dt.Rows[i][0].ToString());
                }
            }
            iIndexes = new int[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                iIndexes[i] = (int)list[i];
            }

            return iIndexes;
        }
        private int[] SetMultiByParent(DataTable dt, string s)
        {
            int[] iIndexes;
            var list = new ArrayList();
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][1].ToString() == s)
                    list.Add(i);
            }
            iIndexes = new int[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                iIndexes[i] = (int)list[i];
            }

            return iIndexes;
        }
        public string Organization
        {
            get { return GetMulti(ddlOrganization.Table, ddlOrganization.SelectedIndexes); }
            set { ddlOrganization.SelectedIndexes = SetMulti(ddlOrganization.Table, value); }
        }
        public string BookMonthNames
        {
            get { return GetMulti(ddlBookMonth.Table, ddlBookMonth.SelectedIndexes); }
        }
        public string BookMonth
        {
            get
            {
                var s = "";
                var dt = ddlBookMonth.Table;
                var ii = ddlBookMonth.SelectedIndexes;
                foreach (var i in ii)
                {
                    s += String.Format("{0:MM}", DateTime.Parse(dt.Rows[i][0].ToString() + "/2000")) + ",";
                }
                if (s.Length > 1)
                    s = s.Substring(0, s.Length - 1);
                return s;
            }
            set
            {
                var dt = ddlBookMonth.Table;
                var list = new ArrayList();
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    if (value.Contains(String.Format("{0:MM}", DateTime.Parse(dt.Rows[i][0].ToString() + "/2000"))))
                        list.Add(i);
                }
                var iIndexes = new int[list.Count];
                for (var i = 0; i < list.Count; i++)
                {
                    iIndexes[i] = (int)list[i];
                }
                ddlBookMonth.SelectedIndexes = iIndexes;
            }
        }
        public string GroupCD
        {
            get
            {
                if (lstGroupCD.SelectedIndex == -1)
                    return "";
                else
                    return lstGroupCD.Table.Rows[lstGroupCD.SelectedIndex][0].ToString();
            }
            set
            {
                for (var i = 0; i < lstGroupCD.Table.Rows.Count; i++)
                {
                    if (lstGroupCD.Table.Rows[i][0].ToString() == value)
                        lstGroupCD.SelectedIndex = i;
                }
            }
        }
        public string GroupCDName
        {
            get
            {
                if (lstGroupCD.SelectedIndex == -1)
                    return "";
                else
                    return lstGroupCD.Table.Rows[lstGroupCD.SelectedIndex][1].ToString();
            }
        }
        public string SummaryFunction
        {
            get { return GetMulti(lstSumFunc.Table, lstSumFunc.SelectedIndexes); }
            set { lstSumFunc.SelectedIndexes = SetMulti(lstSumFunc.Table, value); }
        }
        public string ObjClassCode
        {
            get { return GetMulti(lstOCCode.Table, lstOCCode.SelectedIndexes); }
            set { lstOCCode.SelectedIndexes = SetMulti(lstOCCode.Table, value); }
        }
        public string CostElement
        {
            get { return GetMulti(mgCostElem.Table, mgCostElem.SelectedIndexes); }
            set { mgCostElem.SelectedIndexes = SetMulti(mgCostElem.Table, value); }
        }
        public Hashtable Criteria
        {
            get
            {
                var ht = new Hashtable();
                ht.Add(HT_KEY_BL, BusinessLine);
                ht.Add(HT_KEY_ORG, Organization);
                ht.Add(HT_KEY_YEAR, FiscalYear);
                ht.Add(HT_KEY_BA, BudgetActivity);
                ht.Add(HT_KEY_BOOK_MONTH, BookMonth);
                ht.Add(HT_KEY_VIEW, ViewMode.ToString());
                ht.Add(HT_KEY_GROUP, GroupCD);
                ht.Add(HT_KEY_SUM_FUNC, SummaryFunction);
                ht.Add(HT_KEY_OC, ObjClassCode);
                ht.Add(HT_KEY_CE, CostElement);
                ht.Add(HT_KEY_DOC_NUM, txtDocNumber.Text.Trim());
                return ht;
            }
        }

        public void FillControls()
        {
            try
            {
                Search.AddOnClick("return checkOrg();");
                DataRow dr;

                var dt = FundStatus.GetBusinessLineList().ToDataSet().Tables[0];
                dt.Columns[0].ReadOnly = true;
                ddgBLine.Table = dt;
                //init Organizations list:
                var ds = FundStatus.GetOrganizationList().ToDataSet();
                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Application error. Organization list is missing.");

                dt = ds.Tables[0];
                while (dt.Columns.Count != 2)
                {
                    dt.Columns.RemoveAt(2);
                }
                dt.Columns[1].ReadOnly = true;
                ddlOrganization.Table = dt;//"ORG_CD";

                //init fiscal year list:  
                var doc = new XmlDocument();
                doc.Load(Request.PhysicalApplicationPath + "FundStatus\\include\\FS_SupportedFiscalYears.xml");
                dt = new DataTable();
                dt.Columns.Add("Year");
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    dr = dt.NewRow();
                    dr[0] = node.InnerText;
                    dt.Rows.Add(dr);
                }
                var s = FiscalYear;
                ddlFiscalYear.Table = dt;
                if (s == "")
                    FiscalYear = DateTime.Now.Year.ToString();

                ds = FundStatus.GetBAList().ToDataSet();
                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Application error. No Budget Activity found.");
                s = BudgetActivity;
                ddgBA.Table = ds.Tables[0];
                if (s == "")
                    BudgetActivity = Settings.Default.Default_BUDGET_ACTIVITY;

                //init book month list for selected fiscal year:
                dt = new DataTable();
                dt.Columns.Add("BOOK_MONTH");
                for (var i = 0; i < 12; i++)
                {
                    dr = dt.NewRow();
                    dr[0] = Month[i];
                    dt.Rows.Add(dr);
                }
                ddlBookMonth.Table = dt;

                if (VMode)
                {
                    //init view mode:
                    dt = new DataTable();
                    dt.Columns.Add("Mode");
                    dr = dt.NewRow();
                    dr[0] = "Obligations";
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr[0] = "Income";
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr[0] = "One Time Adjustments";
                    dt.Rows.Add(dr);
                    ddlView.Table = dt;
                }
                if (GroupFunc)
                {
                    //init Report Function Group Code list:
                    dt = FundStatus.GetReportFunctionGroupList().Tables[0];
                    dt.Columns[0].ReadOnly = true;
                    lstGroupCD.Table = dt;
                }
                if (Function)
                {   //init Summary Functions list:
                    lstSumFunc.Table = FundStatus.GetSummaryFunctionsList().ToDataSet().Tables[0];
                }
                if (OCC)
                {   //init all object class codes:
                    dt = FundStatus.GetObjectClassCodeList().ToDataSet().Tables[0];
                    dt.Columns[0].ReadOnly = true;
                    lstOCCode.Table = dt;
                }
                if (CostElem)
                {
                    dt = FundStatus.GetCostElemList().ToDataSet().Tables[0];
                    dt.Columns[0].ReadOnly = true;
                    mgCostElem.Table = dt;
                }
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            Search.Click += new EventHandler(Search_Click);
            this.PreRender += new System.EventHandler(this.MyRender);
        }
        protected void MyRender(object sender, System.EventArgs e)
        {
            if (this.Visible)
                ddlView.ClientOnLoad = "setTxt()";
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Submit != null)
                    this.Submit(this, new EventArgs());
            }
            catch (Exception ex)
            {
                lblCriteriaMsg.CssClass = "errorsum";
                lblCriteriaMsg.Text = ex.Message;
            }
        }
    }
}