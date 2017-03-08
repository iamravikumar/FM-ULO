namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web.UI.WebControls;

    public partial class DocAdministration : PageBase
    {
        private readonly DocumentBO Document;
        public DocAdministration()
        {
            Document = new DocumentBO(this.Dal);
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

                        ctrlCriteria.DisplayPrevSelectedValues(ItemsViewSelectedValues);
                        BindGridData();
                    }
                    else
                    {
                        ItemsViewSourcePath = "DocAdministration.aspx";
                        ItemsViewSelectedValues = null;
                        ItemsPageNumber = 0;
                        ItemsSortExpression = null;

                        //select default organization:
                        ctrlCriteria.DefaultOrganization = OIConstants.OpenItemsGridFilter_TotalUniverse;
                        ctrlCriteria.DisplayDefaults();
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
            ctrlCriteria.SubmitButtonText = "Get Documents List";
            ctrlCriteria.AddWorkloadFilterValue = false;
            ctrlCriteria.DisplaySearchFields = false;
            //ctrlCriteria.ItemTypeIsRequired = true;
            ctrlCriteria.LoadSelectionIsRequired = true;
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
        }

        void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var strDocNumber = (string)((DataRowView)e.Row.DataItem)[1];
                e.Row.Attributes.Add("onclick", "javascript:view_att('" + strDocNumber + "');");
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
                var ds = Document.GetLoadAttachments(ctrlCriteria.LoadID, ctrlCriteria.Organization);

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

                if (!DisplaySendEmailColumns())
                {
                    gvItems.Columns[5].Visible = false;
                    gvItems.Columns[6].Visible = false;
                    gvItems.Columns[7].Visible = false;
                }
            }
            else
                gvItems.Visible = false;


            ItemsViewSelectedValues = ctrlCriteria.SaveCurrentSelectedValues();
        }

        private bool DisplaySendEmailColumns()
        {
            //DataSet ds = LoadList;
            //DataRow[] dr = ds.Tables[0].Select("LoadID = " + LoadID.ToString());
            //if ((int)dr[0]["OIType"] == (int)OpenItemType.tpSample)
            //    return true;
            //else
            return false;
        }

    }
}