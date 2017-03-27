namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;

    public partial class ReviewOpenItems : PageBase
    {
        private readonly EmailsBO Email;
        private readonly ItemsBO Items;
        public ReviewOpenItems()
        {
            Email = new EmailsBO(this.Dal);
            Items = new ItemsBO(this.Dal);
        }
        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {

                if (!IsPostBack)
                {
                    InitControls();

                    if (Request.QueryString["back"] != null && Request.QueryString["back"] == "y" && ItemsViewSelectedValues != null)
                    {
                        gvItems.Visible = true;
                        //gvItems.HeaderRow.Cells[0].HorizontalAlign = HorizontalAlign.Left;

                        ctrlCriteria.DisplayPrevSelectedValues(ItemsViewSelectedValues);
                        BindGridData();
                    }
                    else
                    {
                        ItemsViewSourcePath = "ReviewOpenItems.aspx";
                        ItemsViewSelectedValues = null;
                        ItemsPageNumber = 0;
                        ItemsSortExpression = null;

                        //select default organization:
                        if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) || User.IsInRole(((int)UserRoles.urReadOnlyUser).ToString()))
                            ctrlCriteria.DefaultOrganization = OIConstants.OpenItemsGridFilter_TotalUniverse;
                        else
                            if (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                            ctrlCriteria.DefaultOrganization = CurrentUserOrganization;
                        else
                                if (User.IsInRole(((int)UserRoles.urReviewer).ToString()))
                            ctrlCriteria.DefaultOrganization = OIConstants.OpenItemsGridFilter_MyWorkload;

                        ctrlCriteria.DisplayDefaults();

                        if (ctrlCriteria.ItemTypeCode == (int)OpenItemType.tpBA53)
                            lblHeaderLabel.Text = "BA53 Items List";
                        else
                            lblHeaderLabel.Text = "Open Items List";
                        //gvItems.Visible = false;
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
            ctrlCriteria.SubmitButtonText = "Get Items List";
            ctrlCriteria.AddWorkloadFilterValue = true;
            ctrlCriteria.DisplaySearchFields = false;
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
            btnEmail.Click += new EventHandler(btnEmail_Click);
        }

        void btnEmail_Click(object sender, EventArgs e)
        {
            try
            {
                if (ctrlCriteria.LoadID != 0)
                {
                    Email.SendAssignInfoEmails(CurrentUserID, ctrlCriteria.LoadID);
                    BindGridData();
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

        void ctrlCriteria_Submit(object sender, CriteriaSubmitEventArgs e)
        {

            if (ctrlCriteria.ItemTypeCode == (int)OpenItemType.tpBA53)
                lblHeaderLabel.Text = "BA53 Items List";
            else
                lblHeaderLabel.Text = "Open Items List";

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

            lblHeaderLabel.Text = "Open Items List";

            ItemsDataView = null;
            LoadID = 0;
            gvItems.Visible = false;
            lblMessages.Text = "";
            btnEmail.Visible = false;
            lblEmail.Visible = false;
        }

        void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //if (e.Row.RowType == DataControlRowType.Header)
            //{
            //    if (e.Row.RowType == DataControlRowType.Header)
            //    {
            //        //string headerTitle = string.Empty;
            //        int iCnt = 0;
            //        foreach (TableCell cell in e.Row.Cells)
            //        {
            //            if (iCnt == 2)
            //            {
            //                cell.Attributes.Add(title", "Number of lines in document for current reviewer");
            //            }
            //            iCnt = iCnt + 1;
            //        }
            //    }
            //}
            var iCnt = 0;

            foreach (TableCell cell in e.Row.Cells)
            {
                if (iCnt == 2)
                {
                    cell.AddTitle("Number of lines in document for current reviewer");
                }
                iCnt = iCnt + 1;
            }


            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var intItemID = (int)((DataRowView)e.Row.DataItem)[0];
                var strOrgCode = (string)((DataRowView)e.Row.DataItem)[2];
                var intItemStatus = (int)((DataRowView)e.Row.DataItem)[10];
                var iLoadID = (int)((DataRowView)e.Row.DataItem)[14];

                var strDocNumber = (string)((DataRowView)e.Row.DataItem)[1];
                var intCurrentReviewer = 0;
                if (((DataRowView)e.Row.DataItem)[8] != DBNull.Value)
                    intCurrentReviewer = (int)((DataRowView)e.Row.DataItem)[8];

                var chkVerify = (HtmlInputCheckBox)e.Row.FindControl("chkVerify");
                if (chkVerify != null)
                {
                    if (ctrlCriteria.LoadIsArchived || ctrlCriteria.ParentLoadID > 0)
                        chkVerify.Disabled = true;
                    else
                    {
                        if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) ||
                            User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                        {
                            if (intCurrentReviewer != 0 && intItemStatus == (int)OpenItemStatus.stNewItem &&
                                    (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) ||
                                    (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) && ((DataRowView)e.Row.DataItem)[3].ToString() == CurrentUserOrganization)))
                                chkVerify.AddOnClick("javascript:on_verify(this, " + intItemID.ToString() + ", '" + strOrgCode + "'," + intCurrentReviewer.ToString() + ");");
                            else
                                chkVerify.Disabled = true;
                        }
                        else
                            chkVerify.Disabled = true;
                    }
                }
                var btnAssign = (HtmlImage)e.Row.FindControl("btnAssign");
                if (btnAssign != null)
                {
                    if (ctrlCriteria.LoadIsArchived || ctrlCriteria.ParentLoadID > 0)
                        btnAssign.Disabled = true;
                    else
                    {
                        if (!(intItemStatus == (int)OpenItemStatus.stReassignRequest) &&
                            (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) ||
                                User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) &&
                                ((DataRowView)e.Row.DataItem)[3].ToString() == CurrentUserOrganization))
                        {
                            btnAssign.AddOnClick("javascript:reassign_item(this," + intItemID.ToString() + ",'" + strDocNumber + "','" + strOrgCode + "'," + intCurrentReviewer.ToString() + ");");
                        }
                        else
                            btnAssign.Disabled = true;
                    }
                }

                string strItemPage;

                if (ctrlCriteria.ItemTypeCode == (int)OpenItemType.tpBA53)
                    strItemPage = "OIBA53Review.aspx";
                else
                    strItemPage = "OpenItemDetails.aspx";

                /* we need to assign javascript client function "row_on_click" to all cells of the grid
                 * but cells contain controls, 
                 * like checkbox for verify - cells[8] 
                 * or icon to reassign - cells[9]
                 */
                for (var i = 0; i < e.Row.Cells.Count; i++)
                {
                    if (i != 8 && i != 9)
                        // here we use function located @ OpenItems\include\ReviewItems.js
                        e.Row.Cells[i].AddOnClick("javascript:row_on_click(this,'" + strItemPage + "'," + intItemID.ToString() + ",'" + strOrgCode + "'," + intCurrentReviewer.ToString() + "," + iLoadID.ToString() + ");");
                    if (i == 6)
                    {
                        e.Row.Cells[i].ToolTip = strOrgCode;
                    }
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
            DataSet ds;

            if (ItemsDataView == null)
            {
                if (ctrlCriteria.ItemTypeCode == (int)OpenItemType.tpBA53)
                {
                    ds = Items.GetBA53ItemsList(ctrlCriteria.LoadID, ctrlCriteria.Organization, CurrentUserID);
                }
                else
                {
                    ds = Items.GetItemsList(ctrlCriteria.LoadID, ctrlCriteria.Organization, CurrentUserID);

                }

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
                ///////////////// added by SM
                //if (ctrlCriteria.ItemTypeCode == (int)OpenItemType.tpBA53)
                //{
                //    ds = Items.GetBA53ItemsList(iCurrentLoadID, ctrlCriteria.Organization, CurrentUserID);
                //}
                //else
                //{
                //    ds = Items.GetItemsList(iCurrentLoadID, ctrlCriteria.Organization, CurrentUserID);

                //}

                //if (ds == null || ds.Tables[0].Rows.Count == 0)
                //{
                //    lblMessages.Text = "No records found.";
                //}
                //else
                //{
                //    lblMessages.Text = "";
                //    ItemsDataView = ds.Tables[0].DefaultView;
                //    LoadID = iCurrentLoadID;
                //}
                ///////////////////////////

                ItemsDataView.Sort = ItemsSortExpression;
                gvItems.DataSource = ItemsDataView;
                gvItems.PageIndex = ItemsPageNumber;

                var selected_org = ctrlCriteria.Organization;

                /************************************************************/
                //add here columns selection for BA53
                //if (ctrlCriteria.ItemTypeCode == (int)OpenItemType.tpBA53)

                if (ctrlCriteria.LoadIsArchived ||
                    ctrlCriteria.ParentLoadID > 0 ||
                    !User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) &&
                    !User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) ||
                    !User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) &&
                    User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) &&
                    selected_org != CurrentUserOrganization &&
                    selected_org != ((int)OpenItemsGridFilter.gfTotalUniverse).ToString() &&
                    selected_org != ((int)OpenItemsGridFilter.gfMyWorkload).ToString())
                {
                    gvItems.Columns[9].Visible = false; //reassign form
                    gvItems.Columns[8].Visible = false; //verification checkbox
                }

                gvItems.DataBind();
                gvItems.Visible = true;
            }
            else
                gvItems.Visible = false;


            ItemsViewSelectedValues = ctrlCriteria.SaveCurrentSelectedValues();

            if ((User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) ||
                User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                && !ctrlCriteria.LoadIsArchived && ctrlCriteria.ParentLoadID == 0)
            {
                btnEmail.Visible = true;
                lblEmail.Visible = true;
            }
            else
            {
                btnEmail.Visible = false;
                lblEmail.Visible = false;
            }

            if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                txtReassignTargetPage.Value = "Reroute";
            else if (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                txtReassignTargetPage.Value = "Reassign";
        }


    }
}