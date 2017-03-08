namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Collections;
    using System.Web;
    using Data;

    /// <summary>
    /// Summary description for PageBase
    /// </summary>
    public class PageBase: System.Web.UI.Page
    {


        virtual protected void PageLoadEvent(object sender, System.EventArgs e)
        {
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            var s = "";
            if (!Page.ClientScript.IsClientScriptBlockRegistered(Page.GetType(), "alert2"))
            {
                s = Page.Request.Url.AbsoluteUri;
                s = s.Substring(0, s.IndexOf("OpenItems") + 9) + "/include/alert2.js";
                Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "alert2",
                    "<script language='javascript' src='" + s + "'></script>");
            }
            if (Page.User.Identity.Name.Trim().Length > 0)
                ClientScript.RegisterStartupScript(Page.GetType(), "ST",
                    "<script language='javascript'>_ST(" + (HttpContext.Current.Session.Timeout * 60000).ToString() + ");</script>");

            if (!IsPostBack)
            {
                ClearErrors();
            }

            PageLoadEvent(sender, e);
        }

        #region ErrorHandling

        public ArrayList Errors = new ArrayList();

        public void AddError(Exception ex)
        {
            Errors.Add(ex);
        }

        public int ErrorCount
        {
            get
            {
                return Errors.Count;
            }
        }

        public void ClearErrors()
        {
            Errors.Clear();
        }

        public string GetErrors()
        {
            var sError = "";

            if (Errors.Count > 0)
            {
                foreach (Exception ex in Errors)
                {
                    if (sError.Length > 0)
                    {
                        sError = sError + "<br/>" + ex.Message;
                    }
                    else
                        sError = ex.Message;
                }
                sError += "<br/><br/>";
            }
            return sError;
        }

        protected override void OnError(EventArgs e)
        {
            if (HttpContext.Current.Session == null)
            {
                Response.Redirect("Default.aspx");
            }
            else
            {
                if (!HttpContext.Current.Session.IsNewSession)
                {
                    if (HttpContext.Current.Session.Count <= 0)
                    {
                        Response.Redirect("Default.aspx");
                    }
                }
            }
            base.OnError(e);
        }

        #endregion ErrorHandling


        //****************************************
        //***   Key Constants   ******************
        //****************************************

        private const string KEY_APP_DATA_SOURCE_TYPES = "App:DataSourceTypes:";
        private const string KEY_APP_OPEN_ITEMS_TYPES = "App:OpenItemTypes:";
        private const string KEY_APP_ORGANIZATION_LIST = "App:OrgList:";
        private const string KEY_APP_DEFAULT_JUSTIFICATIONS = "App:DefaultJustifications:";
        private const string KEY_APP_VALIDATION_VALUES = "App:ValidValues:";
        private const string KEY_APP_ACTIVE_CODE_LIST = "App:ActiveCodeList:";
        private const string KEY_APP_FULL_CODE_LIST = "App:FullCodeList:";
        private const string KEY_APP_CONTACT_ROLE_LIST = "App:ContactRoles:";
        private const string KEY_APP_ACCRUAL_TYPES = "App:AccrualTypes:";
        private const string KEY_APP_ACCRUAL_TYPE_ACTIONS = "App:AccrualActionTypes:";
        private const string KEY_APP_REVIEWER_REASON_CODES = "App:ReviewerReasonCodes:";
        private const string INVALID_KEY_APP_REVIEWER_REASON_CODES = "App:InvalidReviewerReasonCodes:";

        private const string KEY_SESSION_LOAD_FULL_INFO_LIST = "Session:LoadFullInfoList:";
        private const string KEY_SESSION_LOAD_LIST = "Session:LoadList:";
        private const string KEY_SESSION_DOC_TYPES = "Session:DocTypes:";

        private const string KEY_SESSION_CURRENT_USER_ID = "Session:CurrentUserID:";
        private const string KEY_SESSION_CURRENT_USER_NAME = "Session:UserName:";
        private const string KEY_SESSION_CURRENT_USER_LOGIN_NAME = "Session:UserLogin:";
        private const string KEY_SESSION_CURRENT_USER_ORG = "Session:UserOrg:";
        private const string KEY_SESSION_CURRENT_USER_ROLE = "Session:UserRole:";
        private const string KEY_SESSION_CURRENT_USER_DEFAULT_APP = "Session:UserApp:";

        private const string KEY_SESSION_ITEMS_VIEW = "Session:ItemsDataView:";
        private const string KEY_SESSION_ITEMS_SORT_EXP = "Session:ItemsSortExp:";
        private const string KEY_SESSION_ITEMS_PAGE = "Session:ItemsPage:";
        private const string KEY_SESSION_ITEMS_FILTER = "Session:ItemsFilter:";
        private const string KEY_SESSION_ITEMS_SELECTED = "Session:ItemsSelectedValues:";
        private const string KEY_SESSION_ITEMS_SOURCE_URL = "Session:ItemsSourceURL:";
        private const string KEY_SESSION_ITEMS_VIEW_MODE = "Session:ItemsViewMode:";

        private const string KEY_SESSION_LOAD_ID = "Session:LoadID:";
        private const string KEY_SESSION_OI_REVIEWER_ID = "Session:ItemReviewerID:";
        private const string KEY_SESSION_OI_ID = "Session:OItemID:";
        private const string KEY_SESSION_ORG_CODE = "Session:OrgCode:";
        private const string KEY_SESSION_DOC_NUM = "Session:DocNumber:";
        private const string KEY_SESSION_LINE_NUM = "Session:LineNum:";
        private const string KEY_SESSION_ITEM_LINES = "Session:ItemLines:";

        private const string KEY_SESSION_FUNDS_TABLE = "Session:FundsTable:";
        private const string KEY_SESSION_FUNDS_CRITERIA = "Session:FundsCriteria:";
        private const string KEY_SESSION_FUNDS_SEARCH_CRITERIA = "Session:FundsSearchCrt:";
        private const string KEY_SESSION_FUNDS_STATUS_CRITERIA = "Session:FundsStatusCrt:";
        private const string KEY_SESSION_FUNDS_SUMMARY_REPORT_CRITERIA = "Session:FundsSumRepCrt:";

        private const string KEY_STATE_LOADS_LIST = "State:LoadList:";
        private const string KEY_STATE_ITEMS_LINES_SORT = "State:LinesSort:";
        protected readonly IDataLayer Dal = new DataLayer(new zoneFinder());


        //****************************************
        //***   Application Storage   ************
        //****************************************      

        public DataView DataSourceTypes
        {
            get
            {
                if (HttpContext.Current.Application[KEY_APP_DATA_SOURCE_TYPES] != null)
                    return (DataView)HttpContext.Current.Application[KEY_APP_DATA_SOURCE_TYPES];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[KEY_APP_DATA_SOURCE_TYPES] = value;
            }
        }

        public DataTable BA53AccrualTypes
        {
            get
            {
                if (HttpContext.Current.Application[KEY_APP_ACCRUAL_TYPES] != null)
                    return (DataTable)HttpContext.Current.Application[KEY_APP_ACCRUAL_TYPES];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[KEY_APP_ACCRUAL_TYPES] = value;
            }
        }

        public DataTable BA53AccrualTypeActions
        {
            get
            {
                if (HttpContext.Current.Application[KEY_APP_ACCRUAL_TYPE_ACTIONS] != null)
                    return (DataTable)HttpContext.Current.Application[KEY_APP_ACCRUAL_TYPE_ACTIONS];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[KEY_APP_ACCRUAL_TYPE_ACTIONS] = value;
            }
        }

        public DataTable ReviewerReasonCodes
        {
            get
            {
                if (HttpContext.Current.Application[KEY_APP_REVIEWER_REASON_CODES] != null)
                    return (DataTable)HttpContext.Current.Application[KEY_APP_REVIEWER_REASON_CODES];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[KEY_APP_REVIEWER_REASON_CODES] = value;
            }
        }

        public DataTable InvalidReviewerReasonCodes
        {
            get
            {
                if (HttpContext.Current.Application[INVALID_KEY_APP_REVIEWER_REASON_CODES] != null)
                    return (DataTable)HttpContext.Current.Application[INVALID_KEY_APP_REVIEWER_REASON_CODES];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[INVALID_KEY_APP_REVIEWER_REASON_CODES] = value;
            }
        }

        public DataTable OpenItemsTypes
        {
            get
            {
                if (HttpContext.Current.Application[KEY_APP_OPEN_ITEMS_TYPES] != null)
                    return (DataTable)HttpContext.Current.Application[KEY_APP_OPEN_ITEMS_TYPES];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[KEY_APP_OPEN_ITEMS_TYPES] = value;
            }
        }

        public DataSet OrganizationsList
        {
            get
            {
                if (HttpContext.Current.Application[KEY_APP_ORGANIZATION_LIST] != null)
                    return (DataSet)HttpContext.Current.Application[KEY_APP_ORGANIZATION_LIST];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[KEY_APP_ORGANIZATION_LIST] = value;
            }
        }

        public DataTable DefaultJustificationValues
        {
            get
            {
                if (HttpContext.Current.Application[KEY_APP_DEFAULT_JUSTIFICATIONS] != null)
                    return (DataTable)HttpContext.Current.Application[KEY_APP_DEFAULT_JUSTIFICATIONS];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[KEY_APP_DEFAULT_JUSTIFICATIONS] = value;
            }
        }

        public DataView ValidationValues
        {
            get
            {
                if (HttpContext.Current.Application[KEY_APP_VALIDATION_VALUES] != null)
                    return (DataView)HttpContext.Current.Application[KEY_APP_VALIDATION_VALUES];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[KEY_APP_VALIDATION_VALUES] = value;
            }
        }

        public DataTable ActiveCodesList
        {
            get
            {
                if (HttpContext.Current.Application[KEY_APP_ACTIVE_CODE_LIST] != null)
                    return (DataTable)HttpContext.Current.Application[KEY_APP_ACTIVE_CODE_LIST];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[KEY_APP_ACTIVE_CODE_LIST] = value;
            }
        }

        public DataTable ActiveAndExpiredCodesList
        {
            get
            {
                if (HttpContext.Current.Application[KEY_APP_FULL_CODE_LIST] != null)
                    return (DataTable)HttpContext.Current.Application[KEY_APP_FULL_CODE_LIST];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[KEY_APP_FULL_CODE_LIST] = value;
            }
        }

        public DataView ContactRolesList
        {
            get
            {
                if (HttpContext.Current.Application[KEY_APP_CONTACT_ROLE_LIST] != null)
                    return (DataView)HttpContext.Current.Application[KEY_APP_CONTACT_ROLE_LIST];
                else
                    return null;
            }
            set
            {
                HttpContext.Current.Application[KEY_APP_CONTACT_ROLE_LIST] = value;
            }
        }


        //****************************************
        //***   Session Storage   ****************
        //****************************************

        /*
        public DataTable FundsReviewTable
        {
            get
            {
                if (Session[KEY_SESSION_FUNDS_TABLE] != null)
                    return (DataTable)Session[KEY_SESSION_FUNDS_TABLE];
                else
                    return null;
            }
            set
            {
                Session[KEY_SESSION_FUNDS_TABLE] = value;
            }
        }
         */

        public DataTable LoadListFullInfo
        {
            get
            {
                if (Session[KEY_SESSION_LOAD_FULL_INFO_LIST] != null)
                    return (DataTable)Session[KEY_SESSION_LOAD_FULL_INFO_LIST];
                else
                    return null;
            }
            set
            {
                Session[KEY_SESSION_LOAD_FULL_INFO_LIST] = value;
            }
        }

        public DataSet LoadList
        {
            get
            {
                if (Session[KEY_SESSION_LOAD_LIST] != null)
                    return (DataSet)Session[KEY_SESSION_LOAD_LIST];
                else
                    return null;
            }
            set
            {
                Session[KEY_SESSION_LOAD_LIST] = value;
            }
        }

        public DataSet DocumentTypes
        {
            get
            {
                if (Session[KEY_SESSION_DOC_TYPES] != null)
                    return (DataSet)Session[KEY_SESSION_DOC_TYPES];
                else
                    return null;
            }
            set
            {
                Session[KEY_SESSION_DOC_TYPES] = value;
            }
        }

        public int CurrentUserID
        {
            get
            {
                if (Session[KEY_SESSION_CURRENT_USER_ID] != null)
                    return (int)Session[KEY_SESSION_CURRENT_USER_ID];
                else
                    return 0;
            }
            set
            {
                Session[KEY_SESSION_CURRENT_USER_ID] = value;
            }
        }

        public int CurrentUserDefaultApp
        {
            get
            {
                return Session[KEY_SESSION_CURRENT_USER_DEFAULT_APP] != null ? (int)Session[KEY_SESSION_CURRENT_USER_DEFAULT_APP] : 0;
            }
            set
            {
                Session[KEY_SESSION_CURRENT_USER_DEFAULT_APP] = value;
            }
        }

        public string CurrentUserRoles
        {
            get
            {
                if (Session[KEY_SESSION_CURRENT_USER_ROLE] != null)
                    return (string)Session[KEY_SESSION_CURRENT_USER_ROLE];
                else
                    return "";
            }
            set
            {
                Session[KEY_SESSION_CURRENT_USER_ROLE] = value;
            }
        }

        public string CurrentUserName
        {
            get
            {
                if (Session[KEY_SESSION_CURRENT_USER_NAME] != null)
                    return (string)Session[KEY_SESSION_CURRENT_USER_NAME];
                else
                    return "";
            }
            set
            {
                Session[KEY_SESSION_CURRENT_USER_NAME] = value;
            }
        }

        public string CurrentUserLogin
        {
            get
            {
                if (Session[KEY_SESSION_CURRENT_USER_LOGIN_NAME] != null)
                    return (string)Session[KEY_SESSION_CURRENT_USER_LOGIN_NAME];
                else
                    return "";
            }
            set
            {
                Session[KEY_SESSION_CURRENT_USER_LOGIN_NAME] = value;
            }
        }

        public string CurrentUserOrganization
        {
            get
            {
                if (Session[KEY_SESSION_CURRENT_USER_ORG] != null)
                    return (string)Session[KEY_SESSION_CURRENT_USER_ORG];
                else
                    return "";
            }
            set
            {
                Session[KEY_SESSION_CURRENT_USER_ORG] = value;
            }
        }

        public DataView ItemsDataView
        {
            get
            {
                if (Session[KEY_SESSION_ITEMS_VIEW] != null)
                    return (DataView)Session[KEY_SESSION_ITEMS_VIEW];
                else
                    return null;
            }
            set
            {
                Session[KEY_SESSION_ITEMS_VIEW] = value;
            }
        }

        public string ItemsSortExpression
        {
            get
            {
                if (Session[KEY_SESSION_ITEMS_SORT_EXP] != null)
                    return (string)Session[KEY_SESSION_ITEMS_SORT_EXP];
                else
                    return "DocNumber";

                return "DocNumber";
            }
            set
            {
                Session[KEY_SESSION_ITEMS_SORT_EXP] = value;
            }
        }

        public int ItemsPageNumber
        {
            get
            {
                if (Session[KEY_SESSION_ITEMS_PAGE] != null)
                    return (int)Session[KEY_SESSION_ITEMS_PAGE];
                else
                    return 0;
            }
            set
            {
                Session[KEY_SESSION_ITEMS_PAGE] = value;
            }
        }

        public string ItemsOrgFilter
        {
            get
            {
                if (Session[KEY_SESSION_ITEMS_FILTER] != null)
                    return (string)Session[KEY_SESSION_ITEMS_FILTER];
                else
                    return "0";
            }
            set
            {
                Session[KEY_SESSION_ITEMS_FILTER] = value;
            }
        }

        public Hashtable ItemsViewSelectedValues
        {
            get
            {
                return (Hashtable)Session[KEY_SESSION_ITEMS_SELECTED];
            }
            set
            {
                Session[KEY_SESSION_ITEMS_SELECTED] = value;
            }
        }

        public Hashtable FundsReviewSelectedValues
        {
            get
            {
                return (Hashtable)Session[KEY_SESSION_FUNDS_CRITERIA];
            }
            set
            {
                Session[KEY_SESSION_FUNDS_CRITERIA] = value;
            }
        }

        public Hashtable FundsSearchSelectedValues
        {
            get
            {
                return (Hashtable)Session[KEY_SESSION_FUNDS_SEARCH_CRITERIA];
            }
            set
            {
                Session[KEY_SESSION_FUNDS_SEARCH_CRITERIA] = value;
            }
        }

        public Hashtable FundsStatusSelectedValues
        {
            get
            {
                return (Hashtable)Session[KEY_SESSION_FUNDS_STATUS_CRITERIA];
            }
            set
            {
                Session[KEY_SESSION_FUNDS_STATUS_CRITERIA] = value;
            }
        }

        public Hashtable FundsSummaryReportSelectedValues
        {
            get
            {
                return (Hashtable)Session[KEY_SESSION_FUNDS_SUMMARY_REPORT_CRITERIA];
            }
            set
            {
                Session[KEY_SESSION_FUNDS_SUMMARY_REPORT_CRITERIA] = value;
            }
        }

        public int LoadID
        {
            get
            {
                if (Session[KEY_SESSION_LOAD_ID] != null)
                    return (int)Session[KEY_SESSION_LOAD_ID];
                else
                    return 0;
            }
            set
            {
                Session[KEY_SESSION_LOAD_ID] = value;
            }
        }
        
        public int CurrentItemReviewerID
        {
            get
            {
                if (Session[KEY_SESSION_OI_REVIEWER_ID] != null)
                    return (int)Session[KEY_SESSION_OI_REVIEWER_ID];
                else
                    return 0;
            }
            set
            {
                Session[KEY_SESSION_OI_REVIEWER_ID] = value;
            }
        }
        
        public int OItemID
        {
            get
            {
                if (Session[KEY_SESSION_OI_ID] != null)
                    return (int)Session[KEY_SESSION_OI_ID];
                else
                    return 0;
            }
            set
            {
                Session[KEY_SESSION_OI_ID] = value;
            }
        }

        public int LineNum
        {
            get
            {
                if (Session[KEY_SESSION_LINE_NUM] != null)
                    return (int)Session[KEY_SESSION_LINE_NUM];
                else
                    return 0;
            }
            set
            {
                Session[KEY_SESSION_LINE_NUM] = value;
            }
        }        

        public string OrgCode
        {
            get
            {
                if (Session[KEY_SESSION_ORG_CODE] != null)
                    return (string)Session[KEY_SESSION_ORG_CODE];
                else
                    return "";
            }
            set
            {
                Session[KEY_SESSION_ORG_CODE] = value;
            }
        }

        public string DocNumber
        {
            get
            {
                if (Session[KEY_SESSION_DOC_NUM] != null)
                    return (string)Session[KEY_SESSION_DOC_NUM];
                else
                    return "";
            }
            set
            {
                Session[KEY_SESSION_DOC_NUM] = value;
            }
        }

        public DataView ItemLinesDataView
        {
            get
            {
                if (Session[KEY_SESSION_ITEM_LINES] != null)
                    return (DataView)Session[KEY_SESSION_ITEM_LINES];
                else
                    return null;
            }
            set
            {
                Session[KEY_SESSION_ITEM_LINES] = value;
            }
        }

        public string ItemsViewSourcePath
        {
            get
            {
                if (Session[KEY_SESSION_ITEMS_SOURCE_URL] == null)
                    //return default page:
                    return "ReviewOpenItems.aspx";
                else
                    return (string)Session[KEY_SESSION_ITEMS_SOURCE_URL];
            }
            set
            {
                Session[KEY_SESSION_ITEMS_SOURCE_URL] = value;
            }
        }

        public string ItemsViewMode
        {
            get
            {
                if (Session[KEY_SESSION_ITEMS_VIEW_MODE] != null)
                    return (string)Session[KEY_SESSION_ITEMS_VIEW_MODE];
                else
                    return "load";
            }
            set
            {
                Session[KEY_SESSION_ITEMS_VIEW_MODE] = value;
            }
        }

        //public bool FundStatusReportRefresh
        //{
        //    get
        //    {
        //        if (Session[KEY_SESSION_FUNDS_STATUS_REFRESH] != null && Session[KEY_SESSION_FUNDS_STATUS_REFRESH] == 1)
        //            return true;
        //        else
        //            return false;
        //    }
        //    set
        //    {
        //        Session[KEY_SESSION_FUNDS_STATUS_REFRESH] = (value) ? 1 : 0;
        //    }
        //}

        //****************************************
        //***   View State Storage   *************
        //****************************************

        public DataSet LoadsList
        {
            get
            {
                if (ViewState[KEY_STATE_LOADS_LIST] != null)
                    return (DataSet)ViewState[KEY_STATE_LOADS_LIST];
                else
                    return null;
            }
            set
            {
                ViewState[KEY_STATE_LOADS_LIST] = value;
            }
        }        

        public string ItemLinesSortExp
        {
            get
            {
                if (ViewState[KEY_STATE_ITEMS_LINES_SORT] != null)
                    return (string)ViewState[KEY_STATE_ITEMS_LINES_SORT];
                else
                    return "ItemLNum";
            }
            set
            {
                ViewState[KEY_STATE_ITEMS_LINES_SORT] = value;
            }
        }

    }
}