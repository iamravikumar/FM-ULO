namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using Data;

    public partial class FundsReview : PageBase
    {
        const string DEFAULT_VALUE_BUDGET_ACTIVITY = "PG61";

        protected override void PageLoadEvent(object sender, System.EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    ctrlCriteria.ScreenType = (int)FundsStatusScreenType.stFundsReview;
                    ctrlCriteria.InitControls();

                    if (Request.QueryString["back"] != null && Request.QueryString["back"] == "y")
                    {
                        //back from Funds Search results screen - display previous report:
                        ctrlCriteria.DisplayPrevSelectedValues(FundsReviewSelectedValues);
                        //build expanded detailed view:
                        ctrlCriteria.ExpandedByMonthView = true;

                        BuildResultsTable();
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

        protected void Page_Init(object sender, System.EventArgs e)
        {
            ctrlCriteria.Submit += new EventHandler(ctrlCriteria_Submit);
        }

        void ctrlCriteria_Submit(object sender, EventArgs e)
        {
            try
            {
                //save report criteria:
                FundsReviewSelectedValues = ctrlCriteria.SaveCurrentSelectedValues();

                BuildResultsTable();
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

        private void BuildResultsTable()
        {
            var last_book_month_included = (ctrlCriteria.BookMonth == "00") ? "09" : ctrlCriteria.BookMonth;
            //get Totals data always:
            DataSet ds = null;
            var dsTotals = FSTotalsReport.GetSummaryState(ctrlCriteria.FiscalYear, ctrlCriteria.Organization, ctrlCriteria.BusinessLine, last_book_month_included, ctrlCriteria.ViewMode, false);
            //get Monthly data only if needed:
            if (ctrlCriteria.ExpandedByMonthView)
                ds = FSTotalsReport.GetSummaryState(ctrlCriteria.FiscalYear, ctrlCriteria.Organization, ctrlCriteria.BusinessLine, last_book_month_included, ctrlCriteria.ViewMode, ctrlCriteria.ExpandedByMonthView);

            if (dsTotals == null || dsTotals.Tables[0].Rows.Count == 0)
            {
                lblMessages.Text = "No records found.";
            }
            else
            {
                lblMessages.Text = "";

                //draw the table tblData:
                var drawing_class = new FSReviewUI();
                drawing_class.TotalsDataTable = dsTotals.Tables[0];
                drawing_class.SourceDataTable = (ds == null) ? null : ds.Tables[0];
                drawing_class.TableToDraw = tblData;
                drawing_class.MonthlyView = ctrlCriteria.ExpandedByMonthView;
                if (ctrlCriteria.ViewMode == (int)FundsReviewViewMode.fvIncome)
                    drawing_class.DisplayColumnObjClassCode = false;
                drawing_class.BookMonth = ctrlCriteria.BookMonth;
                var link = String.Format("FundSearch.aspx?vm={0}&org={1}&fy={2}&ba={3}&bl={4}", ctrlCriteria.ViewMode, ctrlCriteria.Organization, ctrlCriteria.FiscalYear, DEFAULT_VALUE_BUDGET_ACTIVITY, ctrlCriteria.BusinessLine);
                link = link + "&bm={0}&sf={1}&oc={2}";
                drawing_class.CellLinkOnClick = link;
                drawing_class.BuildTheTable();
                tblData = drawing_class.TableToDraw;
            }
        }

    }
}