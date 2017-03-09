namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;

    public partial class Deobligation : PageBase
    {
        private readonly ItemsBO Items;

        public Deobligation()
        {
            Items = new ItemsBO(this.Dal);
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
                        ItemsViewSourcePath = "Deobligation.aspx";
                        ItemsViewSelectedValues = null;
                        ItemsSortExpression = "cert_date";

                        //select default organization:
                        if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                            ctrlCriteria.DefaultOrganization = OIConstants.OpenItemsGridFilter_TotalUniverse;
                        else
                            if (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                            ctrlCriteria.DefaultOrganization = CurrentUserOrganization;

                        ctrlCriteria.DisplayDefaults();
                        gvItems.Visible = false;
                    }
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
            ctrlCriteria.SubmitButtonText = "Get Invalid Items List";
            ctrlCriteria.AddWorkloadFilterValue = false;
            //ctrlCriteria.ItemTypeIsRequired = true;
            ctrlCriteria.LoadSelectionIsRequired = true;
            ctrlCriteria.InitControls();
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


        protected void Page_Init(object sender, System.EventArgs e)
        {
            gvItems.Sorting += new GridViewSortEventHandler(gvItems_Sorting);
            gvItems.PageIndexChanging += new GridViewPageEventHandler(gvItems_PageIndexChanging);
            gvItems.RowDataBound += new GridViewRowEventHandler(gvItems_RowDataBound);
            ctrlCriteria.ClearGridResults += new EventHandler(ctrlCriteria_ClearGridResults);
            ctrlCriteria.Submit += new Controls_CriteriaFields.CriteriaSubmitEventHandler(ctrlCriteria_Submit);
        }

        private void BindGridData()
        {
            if (ItemsDataView == null)
            {
                var ds = Items.GetItemsLinesToDeobligate(ctrlCriteria.LoadID, ctrlCriteria.Organization).ToDataSet();
                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    lblMessages.Text = "No records found.";
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

        void gvItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ItemsPageNumber = e.NewPageIndex;
            BindGridData();
        }

        void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                var intItemID = (int)((DataRowView)e.Row.DataItem)[0];
                var intLineN = (int)((DataRowView)e.Row.DataItem)[2];
                var strOrgCode = (string)((DataRowView)e.Row.DataItem)[3];
                var strOrganization = (string)((DataRowView)e.Row.DataItem)[9];
                var intReviewerUserID = (int)((DataRowView)e.Row.DataItem)[10];

                var chkDeobligate = (HtmlInputCheckBox)e.Row.FindControl("chkDeobligate");
                if (chkDeobligate != null)
                {
                    if (ctrlCriteria.LoadIsArchived ||
                        (!User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) &&
                        strOrganization != CurrentUserOrganization))
                        chkDeobligate.Disabled = true;
                    else
                        chkDeobligate.AddOnClick("javascript:return on_certify(this," + intItemID.ToString() + "," + intLineN.ToString() + ",'" + strOrgCode + "');");

                    if (((DataRowView)e.Row.DataItem)[8] != DBNull.Value)
                        chkDeobligate.Checked = true;
                }

                /* we need to assign javascript code to all cells of the grid
                 * but cells contain controls, 
                 * like checkbox to deobligate item - cells[6] 
                 */
                for (var i = 0; i < e.Row.Cells.Count; i++)
                {
                    if (i != 6)
                        e.Row.Cells[i].AddOnClick("javascript:window.location.href='OpenItemDetails.aspx?id=" + intItemID.ToString() + "&org=" + strOrgCode + "&user=" + intReviewerUserID.ToString() + "';");
                }
            }
        }

    }
}