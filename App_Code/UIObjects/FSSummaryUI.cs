namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web.UI.HtmlControls;


    /// <summary>
    /// Summary description for FSSummaryUI
    /// </summary>
    public class FSSummaryUI
    {
        const string DEFAULT_VALUE_BUDGET_ACTIVITY = "PG61";

        DataSet dsState = null;
        DataSet dsConfig = null;
        DataTable dtState = null;

        string[] month_arr = new string[] { "10", "11", "12", "01", "02", "03", "04", "05", "06", "07", "08", "09" };

        public HtmlTable TableToDraw { get; set; }

        public string FiscalYear { get; set; }

        public string Organization { get; set; }

        public string BusinessLine { get; set; }

        public int SummaryReportViewType { get; set; }

        public string ReportHeader { get; set; }

        public bool BuildReportForExcel { get; set; }

        public void BuildHTMLTable()
        {
            if (Organization == "" && BusinessLine == "")
                throw new Exception("Missing parameters. Please select Organization or Business Line.");

            DrawTable();
        }

        private void DrawTable()
        {

            TableToDraw.Attributes.Add("class", "reportTable");

            if (BuildReportForExcel)
            {
                DrawViewPart((int)FundStatusSummaryReportView.svObligations);
                DrawViewPart((int)FundStatusSummaryReportView.svObligationsMonthly);
                DrawViewPart((int)FundStatusSummaryReportView.svIncome);
                DrawViewPart((int)FundStatusSummaryReportView.svIncomeMonthly);
                DrawViewPart((int)FundStatusSummaryReportView.svAllowance);
                DrawViewPart((int)FundStatusSummaryReportView.svYearEndProjection);
                DrawViewPart((int)FundStatusSummaryReportView.svProjectionVariance);
                DrawViewPart((int)FundStatusSummaryReportView.svProjectedBalance);
            }
            else
            {
                DrawViewPart(SummaryReportViewType);
            }

        }

        private string GetBookMonthList(string BookMonth)
        {
            var m_arr = "10,11,12,01,02,03,04,05,06,07,08,09";
            return m_arr.Substring(0, m_arr.IndexOf(BookMonth) + 2);
        }

        private void DrawViewPart(int SummaryReportViewType)
        {

            var header_link_onclick = String.Format("javascript:get_link('FundsReport.aspx?org={0}&fy={1}&bl={2}", Organization, FiscalYear, BusinessLine);
            header_link_onclick = header_link_onclick + "&bm={0}');";

            var cell_link_onclick = String.Format("javascript:get_link('FundSearch.aspx?org={0}&fy={1}&bl={2}&ba={3}", Organization, FiscalYear, BusinessLine, DEFAULT_VALUE_BUDGET_ACTIVITY);
            cell_link_onclick = cell_link_onclick + "&vm={0}&bm={1}&gcd={2}');";

            //before drawing the table:
            //if the Organization value is empty - take the BusinessLine value,
            //otherwise - ignore BusinessLine value and build the report in Organization level:
            if (Organization != "")
                BusinessLine = "";

            HtmlTableRow tr;

            if (dsState == null || dsConfig == null)
            {
                dsState = FSSummaryReport.GetSummaryState(FiscalYear, Organization, BusinessLine);
                dsConfig = FSDataServices.GetFSReportConfiguration();
                /*
                 * in case of report by Business Line the following should be recalculated and not taken from the database (State table):
                 * - projection
                 * - rwa
                 * - total obligations, etc
                 * because in calulation we use reference to paid days
                 * */
                dtState = dsState.Tables[0].Copy();
                //check the values and then decide if we need it:
                if (BusinessLine != "")
                    dtState = FSSummaryReport.RecalculateSummaryValuesForBusinessLine(dtState, BusinessLine, FiscalYear);
            }

            //add first row - months captions:
            tr = new HtmlTableRow();
            tr.Attributes.Add("class", "reportRow");
            tr.Cells.AddHeaderCell(ReportHeader, "reportHeaderGreen", "center");

            HtmlTableCell td;
            string month_name;
            foreach (var bm in month_arr)
            {
                month_name = DateTime.Parse(bm + "/01/" + FiscalYear).ToString("MMMM");

                if (!BuildReportForExcel)
                {
                    td = new HtmlTableCell();
                    td.InnerText = month_name;
                    td.Align = "center";
                    td.Attributes.Add("class", "reportHeaderGreenLink");
                    td.Attributes.Add("onclick", String.Format(header_link_onclick, bm));
                    td.Attributes.Add("title", String.Format("Click here to see Fund Status Report for {0}", month_name));
                    td.Attributes.Add("onmouseover", "this.className='reportHeaderGreenLinkHover'");
                    td.Attributes.Add("onmouseout", "this.className='reportHeaderGreenLink'");
                    tr.Cells.Add(td);
                }
                else
                    tr.Cells.AddHeaderCell(month_name, "reportHeaderGreen", "center");
            }
            TableToDraw.Rows.Add(tr);

            decimal m_amount = 0;
            var arr_sum = new decimal[13];
            var arr_total_sum = new decimal[13];
            DataRow[] dr_col;

            foreach (DataRow drConfig in dsConfig.Tables[0].Rows)
            {
                if ((int)drConfig["OrderNumber"] != 0)
                {
                    tr = new HtmlTableRow();
                    tr.Attributes.Add("class", "reportRow");
                    tr.Cells.AddHeaderCell(drConfig["Name"].ToString(), drConfig["CaptionStyle"].ToString(), "left");

                    foreach (var bm in month_arr)
                    {
                        td = null;

                        if ((int)drConfig["OrderNumber"] == 99)
                        {
                            //display sum totals for this section:
                            m_amount = arr_sum[Int32.Parse(bm)];
                            tr.Cells.AddDisplayedMoneyCell(m_amount, "reportTotal");
                            arr_sum[Int32.Parse(bm)] = 0;
                        }
                        else
                        {
                            dr_col = dtState.Select(String.Format("BookMonth='{0}' and GROUP_CD={1}", bm, (int)drConfig["GROUP_CD"]));
                            if (dr_col.Length > 0)
                            {
                                switch (SummaryReportViewType)
                                {
                                    case (int)FundStatusSummaryReportView.svObligations:
                                        m_amount = (decimal)Utility.GetNotNullValue(dr_col[0]["Obligations"], "Decimal");
                                        if (!BuildReportForExcel)
                                        {
                                            td = DisplayedMoneyCell(m_amount, "reportAmountLink");
                                            td.Attributes.Add("onclick", String.Format(cell_link_onclick, (int)FundsReviewViewMode.fvObligations, GetBookMonthList(bm), (int)drConfig["GROUP_CD"]));
                                        }
                                        break;
                                    case (int)FundStatusSummaryReportView.svObligationsMonthly:
                                        m_amount = (decimal)Utility.GetNotNullValue(dr_col[0]["ObligationsMonthly"], "Decimal");
                                        if (!BuildReportForExcel)
                                        {
                                            td = DisplayedMoneyCell(m_amount, "reportAmountLink");
                                            td.Attributes.Add("onclick", String.Format(cell_link_onclick, (int)FundsReviewViewMode.fvObligations, bm, (int)drConfig["GROUP_CD"]));
                                        }
                                        break;
                                    case (int)FundStatusSummaryReportView.svIncome:
                                        m_amount = (decimal)Utility.GetNotNullValue(dr_col[0]["Income"], "Decimal");
                                        if (!BuildReportForExcel)
                                        {
                                            td = DisplayedMoneyCell(m_amount, "reportAmountLink");
                                            td.Attributes.Add("onclick", String.Format(cell_link_onclick, (int)FundsReviewViewMode.fvIncome, GetBookMonthList(bm), (int)drConfig["GROUP_CD"]));
                                        }
                                        break;
                                    case (int)FundStatusSummaryReportView.svIncomeMonthly:
                                        m_amount = (decimal)Utility.GetNotNullValue(dr_col[0]["IncomeMonthly"], "Decimal");
                                        if (!BuildReportForExcel)
                                        {
                                            td = DisplayedMoneyCell(m_amount, "reportAmountLink");
                                            td.Attributes.Add("onclick", String.Format(cell_link_onclick, (int)FundsReviewViewMode.fvIncome, bm, (int)drConfig["GROUP_CD"]));
                                        }
                                        break;
                                    case (int)FundStatusSummaryReportView.svAllowance:
                                        m_amount = (decimal)Utility.GetNotNullValue(dr_col[0]["Allowance"], "Decimal");
                                        break;
                                    case (int)FundStatusSummaryReportView.svYearEndProjection:
                                        m_amount = (decimal)Utility.GetNotNullValue(dr_col[0]["Projection"], "Decimal");
                                        break;
                                    case (int)FundStatusSummaryReportView.svProjectedBalance:
                                        m_amount = (decimal)Utility.GetNotNullValue(dr_col[0]["Allowance"], "Decimal") - (decimal)Utility.GetNotNullValue(dr_col[0]["Projection"], "Decimal");
                                        break;
                                    case (int)FundStatusSummaryReportView.svProjectionVariance:
                                        m_amount = (decimal)Utility.GetNotNullValue(dr_col[0]["Allowance"], "Decimal");
                                        m_amount = (m_amount == 0) ? 0 : (m_amount - (decimal)Utility.GetNotNullValue(dr_col[0]["Projection"], "Decimal")) / m_amount;
                                        break;
                                }
                            }
                            else
                                m_amount = 0;

                            if (td == null)
                                td = DisplayedMoneyCell(m_amount, "reportAmount");
                            tr.Cells.Add(td);
                            arr_sum[Int32.Parse(bm)] += m_amount;
                            arr_total_sum[Int32.Parse(bm)] += m_amount;
                        }
                    }
                    TableToDraw.Rows.Add(tr);
                }
            }

            tr = new HtmlTableRow();
            tr.Attributes.Add("class", "reportRow");
            //display sum totals for the whole report:
            tr.Cells.AddHeaderCell("Grand Total", "reportHeaderBlueNAlign", "left");
            foreach (var bm in month_arr)
            {
                m_amount = arr_total_sum[Int32.Parse(bm)];
                tr.Cells.AddDisplayedMoneyCell(m_amount, "reportHeaderBlueNAlign");
            }
            TableToDraw.Rows.Add(tr);

        }


        private void CompleteRow(ref string book_month, ref HtmlTableRow tr)
        {
            //complete row with 0 amount
            book_month = String.Format("{0:00}", Int32.Parse(book_month) % 12 + 1);
            for (var i = 0; i < month_arr.Length; i++)
            {
                if (book_month == month_arr[i])
                {
                    tr.Cells.AddDisplayedMoneyCell(0, "reportAmount");
                    book_month = (book_month == "09") ? "09" : month_arr[i + 1];
                }
            }
        }

        private HtmlTableCell TextCell(string ValueToDisplay, string CSSClass)
        {
            var td = new HtmlTableCell();
            td.InnerText = ValueToDisplay;
            td.Attributes.Add("class", CSSClass);
            return td;
        }

        private HtmlTableCell DisplayedPercentCell(int ValueToDisplay, string CSSClass)
        {
            var td = new HtmlTableCell();
            td.InnerText = String.Format("{0}%", ValueToDisplay);
            td.Attributes.Add("class", CSSClass);
            return td;

        }

        private HtmlTableCell DisplayedMoneyCell(decimal ValueToDisplay, string CSSClass)
        {
            var td = new HtmlTableCell();
            if (ValueToDisplay < 0)
                td.InnerText = String.Format("{0:($0,0)}", ValueToDisplay * (-1));
            else if (ValueToDisplay > 9)
                td.InnerText = String.Format("{0:$0,0}", ValueToDisplay);
            else
                td.InnerText = String.Format("{0:$0}", ValueToDisplay);
            td.Attributes.Add("class", CSSClass);
            td.Align = "right";
            return td;
        }

        private HtmlTableCell HeaderCell(string HeaderText, string CSSClass, string Align)
        {
            var td = new HtmlTableCell();
            td.InnerText = HeaderText;
            td.Attributes.Add("class", CSSClass);
            if (Align != "")
                td.Align = Align;
            return td;
        }

    }
}