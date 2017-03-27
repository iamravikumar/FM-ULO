namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;

    public partial class Assignments : PageBase
    {

        private readonly AssignBO Assign;
        public Assignments()
        {
            Assign = new AssignBO(this.Dal, new ItemBO(this.Dal));
        }
        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    InitControls();

                    if (Request.QueryString["back"] != null && Request.QueryString["back"] == "y")
                    {

                        gvItems.Visible = true;
                        ctrlCriteria.DisplayPrevSelectedValues(ItemsViewSelectedValues);
                        BindGridData();
                    }
                    else
                    {
                        ItemsViewSourcePath = "Assignments.aspx";
                        ItemsViewSelectedValues = null;
                        ItemsPageNumber = 0;
                        ItemsSortExpression = null;

                        //select default organization:
                        if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                            ctrlCriteria.DefaultOrganization = OIConstants.OpenItemsGridFilter_BDResponsibility;
                        else
                            if (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                            ctrlCriteria.DefaultOrganization = CurrentUserOrganization;

                        ctrlCriteria.DisplayDefaults();
                        gvItems.Visible = false;
                    }

                    if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                        txtReassignTargetPage.Value = "Reroute";
                    else if (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                        txtReassignTargetPage.Value = "Reassign";
                }
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                //if (Errors.Count > 0)
                //    lblError.Text = GetErrors();
            }
        }

        private void InitControls()
        {
            ctrlCriteria.SubmitButtonText = "Get Reassignment Request List";
            ctrlCriteria.AddWorkloadFilterValue = false;
            ctrlCriteria.AddBudgetDivisionFilterValue = true;
            //ctrlCriteria.ItemTypeIsRequired = true;
            ctrlCriteria.LoadSelectionIsRequired = true;
            ctrlCriteria.InitControls();

        }

        protected void Page_Init(object sender, System.EventArgs e)
        {
            gvItems.Sorting += new GridViewSortEventHandler(gvItems_Sorting);
            gvItems.PageIndexChanging += new GridViewPageEventHandler(gvItems_PageIndexChanging);
            gvItems.RowDataBound += new GridViewRowEventHandler(gvItems_RowDataBound);
            ctrlCriteria.ClearGridResults += new EventHandler(ctrlCriteria_ClearGridResults);
            ctrlCriteria.Submit += new Controls_CriteriaFields.CriteriaSubmitEventHandler(ctrlCriteria_Submit);
        }

        void ctrlCriteria_Submit(object sender, CriteriaSubmitEventArgs e)
        {
            try
            {
                BindGridData();
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                {
                    //lblError.Text = GetErrors();
                    gvItems.Visible = false;
                }
                else
                    gvItems.Visible = true;
            }
        }

        void ctrlCriteria_ClearGridResults(object sender, EventArgs e)
        {
            ItemsDataView = null;
            LoadID = 0;
            gvItems.Visible = false;
            lblMessages.Text = "";
        }

        void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var intItemID = (int)((DataRowView)e.Row.DataItem)[0];
                //int intLineNum = (int)((DataRowView)e.Row.DataItem)[18];
                var strOrgCode = (string)((DataRowView)e.Row.DataItem)[1];
                var strResponsibility = (string)((DataRowView)e.Row.DataItem)[19];
                var intRequestID = (int)((DataRowView)e.Row.DataItem)[16];
                var strPrevOrgDesc = (string)Utility.GetNotNullValue(((DataRowView)e.Row.DataItem)[8], "String");
                var strPrevOrganization = (string)Utility.GetNotNullValue(((DataRowView)e.Row.DataItem)[7], "String");
                var strRerouteOrganization = (string)Utility.GetNotNullValue(((DataRowView)e.Row.DataItem)[12], "String");
                var intPrevReviewerUserID = (int)Utility.GetNotNullValue(((DataRowView)e.Row.DataItem)[9], "Int");

                var intRerouteUserID = (int)Utility.GetNotNullValue(((DataRowView)e.Row.DataItem)[14], "Int");
                var chkVerify = (HtmlInputCheckBox)e.Row.FindControl("chkVerify");
                if (chkVerify != null)
                {
                    if (intRerouteUserID == 0 ||
                        !User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) &&
                        strResponsibility != CurrentUserOrganization ||
                        !User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) &&
                        strResponsibility == CurrentUserOrganization &&
                        strPrevOrganization != strRerouteOrganization)
                        chkVerify.Disabled = true;
                    else
                        chkVerify.AddOnClick("javascript:on_verify(this," + intRequestID.ToString() + "," + intRerouteUserID.ToString() + ",'" + strPrevOrgDesc + "'," + intPrevReviewerUserID.ToString() + ");");
                }

                var btnAssign = (HtmlImage)e.Row.FindControl("btnAssign");
                if (btnAssign != null)
                {
                    if (!User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) &&
                        strResponsibility != CurrentUserOrganization)
                        btnAssign.Disabled = true;
                    else
                        btnAssign.AddOnClick("javascript:reassign_item(this," + intRequestID.ToString() + ");");
                }
                /* we need to assign javascript code to all cells of the grid
                 * but cells contain controls, 
                 * like checkbox for verify - cells[9] 
                 * or icon to reroute - cells[10]
                 */
                for (var i = 0; i < e.Row.Cells.Count; i++)
                {
                    if (i != 9 && i != 10)
                        e.Row.Cells[i].AddOnClick("javascript:window.location.href='OpenItemDetails.aspx?id=" + intItemID.ToString() + "&org=" + strOrgCode + "&user=" + intPrevReviewerUserID.ToString() + "';");
                }
            }
        }

        void gvItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ItemsPageNumber = e.NewPageIndex;
            BindGridData();
        }

        void gvItems_Sorting(object sender, GridViewSortEventArgs e)
        {
            ItemsPageNumber = 0;
            var expSort = ItemsSortExpression;
            if ((expSort == e.SortExpression) || (expSort == e.SortExpression + " ASC"))
            {
                expSort = e.SortExpression + " DESC";
            }
            else
            {
                expSort = e.SortExpression + " ASC";
            }
            ItemsSortExpression = expSort;
            BindGridData();
        }

        private void BindGridData()
        {
            if (ItemsDataView == null)
            {
                var ds = Assign.GetRequestsList(ctrlCriteria.LoadID, ctrlCriteria.Organization);
                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    lblMessages.Text = "No records found.";
                    LoadID = 0;
                }
                else
                {
                    lblMessages.Text = "";
                    ItemsDataView = ds.Tables[0].DefaultView;
                    LoadID = ctrlCriteria.LoadID;
                }
            }

            if (ItemsDataView != null)
            {
                ItemsDataView.Sort = ItemsSortExpression;
                gvItems.DataSource = ItemsDataView;
                gvItems.PageIndex = ItemsPageNumber;

                gvItems.DataBind();
                gvItems.Visible = true;
            }
            else
                gvItems.Visible = false;


            ItemsViewSelectedValues = ctrlCriteria.SaveCurrentSelectedValues();
        }

    }
}