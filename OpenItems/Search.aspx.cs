namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;


    public partial class Search : PageBase
    {
        // update spSearchItems
        DataTable _dtDistinctLoad = new DataTable();
        private readonly ItemsBO Items;

        public Search()
        {
            Items = new ItemsBO(this.Dal);
        }

        private DataTable dtDistinctLoad
        {
            get
            {
                if (ViewState["dtDistinctLoad"] == null)
                    return _dtDistinctLoad;
                else
                    return (DataTable)ViewState["dtDistinctLoad"];
            }
            set
            {
                _dtDistinctLoad = value;
                ViewState["dtDistinctLoad"] = value;
            }

        }
        private MergedColumnsInfo info
        {
            get
            {
                if (ViewState["info"] == null)
                    ViewState["info"] = new MergedColumnsInfo();
                return (MergedColumnsInfo)ViewState["info"];
            }
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
                        ItemsViewSourcePath = "Search.aspx";
                        ItemsViewSelectedValues = null;
                        ItemsPageNumber = 0;

                        ItemsSortExpression = "DocNumber, LoadDate DESC";

                        ctrlCriteria.DisplayDefaults();
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
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }

        private void InitControls()
        {
            ctrlCriteria.SubmitButtonText = "Search";
            ctrlCriteria.AddWorkloadFilterValue = false;
            ctrlCriteria.DisplaySearchFields = true;
            //ctrlCriteria.ItemTypeIsRequired = true;
            ctrlCriteria.LoadSelectionIsRequired = false;
            ctrlCriteria.DisplayArchiveOption = false;
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
                if ((ctrlCriteria.LoadID == 0) && (ctrlCriteria.DocNumber == "") && (ctrlCriteria.ProjNumber == "") &&
                    (ctrlCriteria.BA == "") && (ctrlCriteria.AwardNumber == ""))
                {
                    throw new Exception("Please fill at least one Criteria field.");
                }
                else
                {
                    BindGridData();
                }
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                {
                    lblError.Text = GetErrors();
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
            lblAssign.Visible = false;
            btnAssign.Visible = false;
            txtAllSelected.Value = "";
            excelTD.Visible = false;
            gvTest.Visible = false;
        }

        void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            try
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    var intItemID = (int)((DataRowView)e.Row.DataItem)[0];
                    var intLineNum = (int)((DataRowView)e.Row.DataItem)[18];
                    var strULOOrgCode = (string)((DataRowView)e.Row.DataItem)[2];
                    var strOrganization = ((DataRowView)e.Row.DataItem)[3].ToString();
                    var intReviewerUserID = (int)((DataRowView)e.Row.DataItem)[5];
                    var intItemStatus = (int)((DataRowView)e.Row.DataItem)[7];
                    var intLoadID = (int)((DataRowView)e.Row.DataItem)[19];
                    var intLOadType = (int)((DataRowView)e.Row.DataItem)[25];
                    var dtArchiveDate = (DateTime)Utility.GetNotNullValue(((DataRowView)e.Row.DataItem)[20], "DateTime");

                    var chkAssign = (HtmlInputCheckBox)e.Row.FindControl("chkAssign");
                    if (chkAssign != null)
                    {
                        if (ctrlCriteria.LoadIsArchived || ctrlCriteria.ParentLoadID > 0)
                            chkAssign.Disabled = true;
                        else
                        {
                            if (!(intItemStatus == (int)OpenItemStatus.stReassignRequest) &&
                                dtArchiveDate == DateTime.MinValue &&
                                (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) ||
                                    User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()) &&
                                    strOrganization == CurrentUserOrganization))
                            {
                                //***SM_
                                chkAssign.Attributes.Add("onclick", "javascript:check_for_assign(this," + intItemID.ToString() + "," + intLineNum.ToString() + ",'" + strULOOrgCode + "');"); txtAllSelected.Value = txtAllSelected.Value + intItemID.ToString() + "_" + intLineNum.ToString() + ",";
                            }
                            else
                                chkAssign.Disabled = true;
                        }
                    }

                    /* we need to assign javascript client function "row_on_click" to all cells of the grid
                    * but cells contain controls, 
                    * like checkbox to group assign - cells[1]                
                    */
                    for (var i = 2; i < e.Row.Cells.Count; i++)
                    {
                        e.Row.Cells[i].Attributes.Add("onclick", "javascript:row_on_click(" + intItemID.ToString() + ",'" + strULOOrgCode + "'," + intReviewerUserID.ToString() + "," + intLoadID.ToString() + "," + intLOadType.ToString() + ");");
                    }
                }
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }

        void gvItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                ItemsPageNumber = e.NewPageIndex;

                BindGridData();
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }

        void addPagerMessage(int pageNumber)
        {
            var dv = (DataView)gvItems.DataSource;
            var pageRowcount = gvItems.PageSize;

            var firstRecord = ((pageRowcount * pageNumber) + 1).ToString();
            var LastRecord = ((pageRowcount * pageNumber) + pageRowcount).ToString();

            if ((pageRowcount * pageNumber) + pageRowcount > dv.Table.Rows.Count)
                LastRecord = dv.Table.Rows.Count.ToString();

            var pagerTable = (gvItems.BottomPagerRow.Cells[0].Controls[0] as Table);
            var txtLabel = new Label();
            txtLabel.Text = "Displaying " + firstRecord + " to " + LastRecord + " of  " + dv.Table.Rows.Count + " records ";

            var tcell = new TableCell();
            tcell.Controls.Add(txtLabel);
            tcell.Width = Unit.Percentage(100);
            tcell.HorizontalAlign = HorizontalAlign.Right;

            ((TableRow)(pagerTable.Controls[0])).Cells.Add(tcell);
        }

        void gvItems_Sorting(object sender, GridViewSortEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }

        private void BindGridData()
        {
            if (ItemsDataView == null)
            {
                var ds = Items.SearchItems(ctrlCriteria.LoadID, ctrlCriteria.Organization, ctrlCriteria.DocNumber,
                        ctrlCriteria.ProjNumber, ctrlCriteria.BA, ctrlCriteria.AwardNumber);

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    lblMessages.Text = "No records found.";
                }
                else
                {
                    if (ctrlCriteria.ValHistSearch)
                    {
                        gvTest.Visible = true;
                        gvTest.Enabled = true;
                        divValSearch.Visible = true;

                        lblMessages.Text = "";
                        ItemsDataView = formatDS(ds.Tables[0].DefaultView);
                        LoadID = ctrlCriteria.LoadID;

                        if (ctrlCriteria.LatestReview)
                            ItemsDataView = formatDS(filterData(ds.Tables[0].DefaultView));
                    }
                    else
                    {
                        gvTest.Visible = false;
                        gvTest.Enabled = false;
                        divValSearch.Visible = false;
                        lblMessages.Text = "";
                        ItemsDataView = ds.Tables[0].DefaultView;
                        LoadID = ctrlCriteria.LoadID;

                        if (ctrlCriteria.LatestReview)
                            ItemsDataView = filterData(ds.Tables[0].DefaultView);
                    }
                }
            }

            if (ItemsDataView != null)
            {
                if (ctrlCriteria.ValHistSearch)
                {
                    excelImage.Attributes.Add("onclick", "window.open('ReportCustom.aspx?id=8');");
                    MergeHeaders();
                    gvItems = gvTest;
                    //ItemsSortExpression = "DocNumber, LineNum DESC";
                }
                else
                {
                    excelImage.Attributes.Add("onclick", "window.open('ReportCustom.aspx?id=7');");
                    //ItemsSortExpression = "DocNumber, LoadDate DESC";

                    if (ctrlCriteria.ParentLoadID > 0)
                    {
                        gvItems.Columns[1].Visible = false; //checkbox for group assign
                        gvItems.Columns[4].Visible = false; //line number has no meaning for feedback load
                    }
                    else if (ctrlCriteria.LoadIsArchived)
                        gvItems.Columns[1].Visible = false;
                    else if (!User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()) &&
                           !User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                        gvItems.Columns[1].Visible = false; //checkbox for group assign - current user has no permission
                    else
                    {
                        //lblAssign.Visible = true; //SM
                        lblAssign.Visible = false;
                        //btnAssign.Visible = true; //SM
                        btnAssign.Visible = false;
                        btnAssign.Attributes.Add("onclick", "javascript:reassign_items();");

                        txtAllSelected.Value = "";

                        if (User.IsInRole(((int)UserRoles.urBudgetDivisionAdmin).ToString()))
                            txtReassignTargetPage.Value = "Reroute";
                        else if (User.IsInRole(((int)UserRoles.urOrganizationAdmin).ToString()))
                            txtReassignTargetPage.Value = "Reassign";
                    }
                }
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

        protected void gvItems_DataBound(object sender, EventArgs e)
        {
            try
            {
                if (((DataView)(gvItems.DataSource)).Table.Rows.Count > 0)
                {
                    excelTD.Visible = true;
                    addPagerMessage(gvItems.PageIndex);
                }
                else
                {
                    excelTD.Visible = false;
                }
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }

        private DataView filterData(DataView dvSource)
        {
            var htFilter = new Hashtable();

            for (var i = 0; i < dvSource.Table.Rows.Count; i++)
            {
                if (htFilter[dvSource.Table.Rows[i]["DocNumber"].ToString() + "," + dvSource.Table.Rows[i]["ItemLNum"].ToString()] != null)
                {
                    if (Convert.ToDateTime(htFilter[dvSource.Table.Rows[i]["DocNumber"].ToString() + "," + dvSource.Table.Rows[i]["ItemLNum"].ToString()]) < Convert.ToDateTime(dvSource.Table.Rows[i]["LoadDate"]))
                    {
                        htFilter.Remove(dvSource.Table.Rows[i]["DocNumber"].ToString() + "," + dvSource.Table.Rows[i]["ItemLNum"].ToString());
                        htFilter.Add(dvSource.Table.Rows[i]["DocNumber"].ToString() + "," + dvSource.Table.Rows[i]["ItemLNum"].ToString(), Convert.ToDateTime(dvSource.Table.Rows[i]["LoadDate"]));
                    }
                }
                else
                {
                    htFilter.Add(dvSource.Table.Rows[i]["DocNumber"].ToString() + "," + dvSource.Table.Rows[i]["ItemLNum"].ToString(), Convert.ToDateTime(dvSource.Table.Rows[i]["LoadDate"]));
                }

            }

            var newTable = dvSource.Table.Clone();

            for (var k = 0; k < dvSource.Table.Rows.Count; k++)
            {
                if (Convert.ToDateTime(htFilter[dvSource.Table.Rows[k]["DocNumber"].ToString() + "," + dvSource.Table.Rows[k]["ItemLNum"].ToString()]) == Convert.ToDateTime(dvSource.Table.Rows[k]["LoadDate"]))
                {
                    newTable.ImportRow(dvSource.Table.Rows[k]);
                }
            }

            formatDS(dvSource);
            return newTable.DefaultView;
        }

        private DataView formatDS(DataView dvSource)
        {
            excelTD.Visible = true;
            var dtNew = new DataTable();

            //dtNew.Columns.Add(createColumn("LoadDate"));
            //dtNew.Columns.Add(createColumn("LoadID"));
            dtNew.Columns.Add(createColumn("DocNumber"));
            dtNew.Columns.Add(createColumn("LineNum"));
            dtNew.Columns.Add(createColumn("BA"));
            dtNew.Columns.Add(createColumn("Organization"));

            dtDistinctLoad = dvSource.ToTable(true, new string[] { "LoadID", "LoadDate", "LoadDescription" });
            for (var i = 0; i < dtDistinctLoad.Rows.Count; i++)
            {
                dtNew.Columns.Add(createColumn(dtDistinctLoad.Rows[i]["LoadID"].ToString() + "_UDO"));
                dtNew.Columns.Add(createColumn(dtDistinctLoad.Rows[i]["LoadID"].ToString() + "_DO"));
                dtNew.Columns.Add(createColumn(dtDistinctLoad.Rows[i]["LoadID"].ToString() + "_Valid"));
                dtNew.Columns.Add(createColumn(dtDistinctLoad.Rows[i]["LoadID"].ToString() + "_Justification"));
                dtNew.Columns.Add(createColumn(dtDistinctLoad.Rows[i]["LoadID"].ToString() + "_ReviewerName"));

            }

            var dtInitInfo = dvSource.ToTable(true, new string[] { "DocNumber", "ItemLNum", "BA", "Organization" });

            for (var j = 0; j < dtInitInfo.Rows.Count; j++)
            {
                var dr = dtNew.NewRow();
                dr["DocNumber"] = dtInitInfo.Rows[j]["DocNumber"];
                dr["LineNum"] = dtInitInfo.Rows[j]["ItemLNum"];
                dr["BA"] = dtInitInfo.Rows[j]["BA"];
                dr["Organization"] = dtInitInfo.Rows[j]["Organization"];
                for (var k = 0; k < dtDistinctLoad.Rows.Count; k++)
                {
                    var filterExp = "DocNumber='" + dtInitInfo.Rows[j]["DocNumber"].ToString() + "' and ItemLNum='" + dtInitInfo.Rows[j]["ItemLNum"].ToString() + "' and LoadID='" + dtDistinctLoad.Rows[k]["LoadID"].ToString() + "'";

                    if (dvSource.Table.Select(filterExp).Length > 0)
                    {
                        //dr.IsDBNull(2) ? "" : Convert.ToDecimal(dr[2]).ToString("#,##0.00")
                        dr[dtDistinctLoad.Rows[k]["LoadID"].ToString() + "_UDO"] = Utility.DisplayMoneyFormat(dvSource.Table.Select(filterExp)[0].IsNull("UDO") ? 0 : Convert.ToDecimal(dvSource.Table.Select(filterExp)[0]["UDO"].ToString()));
                        dr[dtDistinctLoad.Rows[k]["LoadID"].ToString() + "_DO"] = Utility.DisplayMoneyFormat(dvSource.Table.Select(filterExp)[0].IsNull("DO") ? 0 : Convert.ToDecimal(dvSource.Table.Select(filterExp)[0]["DO"].ToString()));
                        dr[dtDistinctLoad.Rows[k]["LoadID"].ToString() + "_Valid"] = dvSource.Table.Select(filterExp)[0]["ValidValueDescription"].ToString();
                        dr[dtDistinctLoad.Rows[k]["LoadID"].ToString() + "_Justification"] = dvSource.Table.Select(filterExp)[0]["Justification"].ToString();
                        dr[dtDistinctLoad.Rows[k]["LoadID"].ToString() + "_ReviewerName"] = dvSource.Table.Select(filterExp)[0]["ReviewerName"].ToString();
                    }
                }
                dtNew.Rows.Add(dr);
            }
            return dtNew.DefaultView;
        }

        private DataColumn createColumn(string columnName)
        {
            var dc = new DataColumn();
            dc.DataType = typeof(string);
            dc.ColumnName = columnName;
            dc.ReadOnly = false;
            dc.Unique = false;

            return dc;
        }

        protected void gvTest_RowCreated(object sender, GridViewRowEventArgs e)
        {
            try
            {
                if (ctrlCriteria.ValHistSearch)
                {
                    //call the method for custom rendering the columns headers	
                    if (e.Row.RowType == DataControlRowType.Header)
                    {
                        if (e.Row.RowType == DataControlRowType.Header)
                        {
                            for (var i = 0; i < dtDistinctLoad.Rows.Count; i++)
                            {
                                if (((LinkButton)(e.Row.Cells[i * 5 + 4 + 4]).Controls[0]).Text == dtDistinctLoad.Rows[i]["LoadID"] + "_ReviewerName")
                                {
                                    ((LinkButton)(e.Row.Cells[i * 5 + 4]).Controls[0]).Text = "UDO";
                                    ((LinkButton)(e.Row.Cells[i * 5 + 4 + 1]).Controls[0]).Text = "DO";
                                    ((LinkButton)(e.Row.Cells[i * 5 + 4 + 2]).Controls[0]).Text = "Valid";
                                    ((LinkButton)(e.Row.Cells[i * 5 + 4 + 3]).Controls[0]).Text = "Justification";
                                    ((LinkButton)(e.Row.Cells[i * 5 + 4 + 4]).Controls[0]).Text = "Reviewer Name";

                                }
                            }
                        }
                        e.Row.SetRenderMethodDelegate(RenderHeader);
                    }
                }
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }

        private void RenderHeader(HtmlTextWriter output, Control container)
        {
            for (var i = 0; i < container.Controls.Count; i++)
            {
                var cell = (TableCell)container.Controls[i];
                //stretch non merged columns for two rows
                if (!info.MergedColumns.Contains(i))
                {
                    cell.Attributes["rowspan"] = "2";
                    //cell.Attributes["class"] = "testCSS";
                    cell.RenderControl(output);
                }
                else //render merged columns common title
                    if (info.StartColumns.Contains(i))
                {
                    output.Write(string.Format("<th  align='center' colspan='{0}'>{1}</th>",
                             info.StartColumns[i], info.Titles[i]));
                }
            }

            //close the first row	
            output.RenderEndTag();
            //set attributes for the second row
            gvTest.HeaderStyle.AddAttributesToRender(output);
            //start the second row
            output.RenderBeginTag("tr");

            //render the second row (only the merged columns)
            for (var i = 0; i < info.MergedColumns.Count; i++)
            {
                var cell = (TableCell)container.Controls[info.MergedColumns[i]];
                //cell.Attributes["class"] = "testCSS";
                cell.RenderControl(output);
            }
        }

        private void MergeHeaders()
        {
            ViewState["info"] = null;
            for (var i = 0; i < dtDistinctLoad.Rows.Count; i++)
            {
                info.AddMergedColumns(new int[] { i * 5 + 4, i * 5 + 4 + 1, i * 5 + 4 + 2, i * 5 + 4 + 3, i * 5 + 4 + 4 }, dtDistinctLoad.Rows[i]["LoadDescription"].ToString());
            }

        }
        protected void gvTest_Sorting(object sender, GridViewSortEventArgs e)
        {
            try
            {
                gvItems_Sorting(sender, e);
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }
        protected void gvTest_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                gvItems_PageIndexChanging(sender, e);
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }

        protected void gvTest_DataBound(object sender, EventArgs e)
        {
            try
            {
                gvItems_DataBound(sender, e);
            }
            catch (Exception ex)
            {
                AddError(ex);
            }
            finally
            {
                if (Errors.Count > 0)
                    lblError.Text = GetErrors();
            }
        }
    }
    [Serializable]
    public class MergedColumnsInfo
    {
        // indexes of merged columns
        public List<int> MergedColumns = new List<int>();
        // key-value pairs: key = the first column index, value = number of the merged columns
        public Hashtable StartColumns = new Hashtable();
        // key-value pairs: key = the first column index, value = common title of the merged columns 
        public Hashtable Titles = new Hashtable();

        //parameters: the merged columns indexes, common title of the merged columns 
        public void AddMergedColumns(int[] columnsIndexes, string title)
        {
            MergedColumns.AddRange(columnsIndexes);
            StartColumns.Add(columnsIndexes[0], columnsIndexes.Length);
            Titles.Add(columnsIndexes[0], title);
        }
    }

}


