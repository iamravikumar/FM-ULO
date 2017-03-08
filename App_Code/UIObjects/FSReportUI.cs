namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Text;
    using System.Web.UI.HtmlControls;

    /// <summary>
    /// Summary description for FSReportUI
    /// </summary>
    public class FSReportUI
    {
        const int TABLE_MAX_ROWS = 9;

        const string DEFAULT_VALUE_BUDGET_ACTIVITY = "PG61";

        const string CAPTION_OBLIGATIONS = "Obligations";
        const string CAPTION_INCOME = "Income";
        const string CAPTION_ADJUSTMENTS = "Adjustments";
        const string CAPTION_TOTAL_OBLIGATIONS = "Total Obligations";
        const string CAPTION_PROJECTION = "Projection";
        const string CAPTION_ALLOWANCE = "Allowance";
        const string CAPTION_BALANCE = "Balance";
        const string CAPTION_VARIANCE = "%Variance";
        const string CAPTION_RWA = "RWA";
        const string CAPTION_OVERUNDERACCRUED = "Over/Under Accrued";
        const string CAPTION_ONETIMEADJ = "OneTimeAdj $";
        const string CAPTION_EXPBYYEAREND = "ExpByYearEnd $";
        const string CAPTION_Q_PROJECTION = "Projection";
        const string CAPTION_Q_ALLOWANCE = "Allowance";
        const string CAPTION_Q_BALANCE = "Balance";
        const string CAPTION_Q_ADJUSTMENTS = "$ Amount";
        const string CAPTION_EXPLANATION = "Explanation";



        public HtmlTable TableToDraw { get; set;}

        public string FiscalYear { get; set; }

        public string BookMonth { get; set; }

        public string Organization { get; set; }

        public string BusinessLine { get; set; }

        public bool BuildReportForExcel { get; set; }

        public void BuildHTMLTable()
        {
            DrawTable();
        }

        private string AddTooltipPopup(string Caption)
        {
            var sb = new StringBuilder();

            var sTooltipAttributes = "this.T_DELAY=500;  this.T_FONTCOLOR='#3B4963'; this.T_BGCOLOR='#ffffff'; this.T_BGIMG='../images/popup_bg.jpg'; " +
                "this.T_BORDERWIDTH=2; this.T_BORDERCOLOR='#677388'; this.T_PADDING=5; this.T_FONTSIZE='11px'; this.T_TITLESIZE='12px'; " +
                "this.T_TITLECOLOR='#ffffff'; this.T_STATIC=true; this.T_TEXTALIGN='center'; "; //this.T_WIDTH=300;

            switch (Caption)
            {
                case CAPTION_OBLIGATIONS:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    sb.Append("summary amounts from FMIS");
                    sb.Append("')");
                    break;

                case CAPTION_INCOME:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    sb.Append("summary amounts from FMIS");
                    sb.Append("')");
                    break;

                case CAPTION_ADJUSTMENTS:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    sb.Append("RWA + Over/Under Accrued");
                    sb.Append("')");
                    break;

                case CAPTION_TOTAL_OBLIGATIONS:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    sb.Append("Obligations - Income + Adjustments");
                    sb.Append("')");
                    break;

                case CAPTION_PROJECTION:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    //sb.Append("(Total Oblig - One Time Adj) / Paid Days * Days in CR + One Time Adj + Exp by YearEnd");
                    sb.Append("<table><tr><td>(Total Oblig - One Time Adj) / Paid Days * Days in CR</td></tr>");
                    sb.Append("<tr><td>+ One Time Adj + Exp by YearEnd</td></tr></table>");
                    sb.Append("')");
                    break;

                case CAPTION_ALLOWANCE:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    sb.Append("Allowance Amount Distributed to " + Organization);
                    sb.Append("')");
                    break;

                case CAPTION_VARIANCE:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    //sb.Append("<table><tr><td>Balance</td></tr><tr><td><hr /></td></tr><tr><td>Allowance</td></tr></table>");
                    sb.Append("Balance / Allowance");
                    sb.Append("')");
                    break;

                case CAPTION_BALANCE:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    sb.Append("Allowance - Projection");
                    sb.Append("')");
                    break;

                case CAPTION_RWA:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    sb.Append(" (Projection / Paid Days in the Year * Current Paid Days) - current Fiscal Year Income. <br/>Click here to change the distribution.");
                    sb.Append("')");
                    break;

                case CAPTION_OVERUNDERACCRUED:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    sb.Append("summary amount of Budget Analist records");
                    sb.Append("')");
                    break;

                case CAPTION_ONETIMEADJ:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    sb.Append("summary amount of PJ and PX documents plus Budget Analist records");
                    sb.Append("')");
                    break;

                case CAPTION_EXPBYYEAREND:
                    sb.Append("this.T_TITLE='");        //<b>
                    sb.Append(Caption);
                    sb.Append(" calculated as:';");     //</b>
                    sb.Append(sTooltipAttributes);
                    sb.Append("this.T_WIDTH=300; ");
                    sb.Append("return escape('");
                    sb.Append("summary amount of Budget Analist records");
                    sb.Append("')");
                    break;
            }

            return sb.ToString();
        }


        private string GetBookMonthList(string BookMonth)
        {
            var m_arr = "10,11,12,01,02,03,04,05,06,07,08,09";
            return m_arr.Substring(0, m_arr.IndexOf(BookMonth) + 2);
        }



        private void DrawTable()
        {
            var cell_link_onclick = String.Format("javascript:get_link('FundSearch.aspx?org={0}&fy={1}&bm={2}&bl={3}&ba={4}", Organization, FiscalYear, GetBookMonthList(BookMonth), BusinessLine, DEFAULT_VALUE_BUDGET_ACTIVITY);
            cell_link_onclick = cell_link_onclick + "&vm={0}&gcd={1}');";

            //before drawing the table:
            //if the Organization value is empty - take the BusinessLine value,
            //otherwise - ignore BusinessLine value and build the report on Organization level:
            if (Organization != "")
                BusinessLine = "";

            //link to user data entry screen:
            var cell_popup_onclick = String.Format("open_popup('org={0}&fy={1}&bm={2}&bl={3}", Organization, FiscalYear, BookMonth, BusinessLine);
            cell_popup_onclick = cell_popup_onclick + "&t={0}&gcd={1}&adj={2}');";

            HtmlTableRow tr;
            HtmlTableCell td;

            var dsState = FSSummaryReport.GetMonthState(FiscalYear, BookMonth, Organization, BusinessLine);
            var dsConfig = FSDataServices.GetFSReportConfiguration();
            /*
             * in case of report by Business Line the following should be recalculated and not taken from the database (State table):
             * - projection
             * - rwa
             * - total obligations, etc
             * because in calulation we use reference to paid days
             * */
            var dtState = dsState.Tables[0].Copy();
            if (BusinessLine != "")
                dtState = FSSummaryReport.RecalculateValuesForBusinessLine(dtState, BusinessLine, FiscalYear, BookMonth);

            decimal obligations = 0;
            decimal income = 0;
            decimal rwa = 0;
            decimal adjustments = 0;
            decimal totalobligations = 0;
            decimal overunderaccrued = 0;
            decimal projection = 0;
            decimal allowance = 0;
            decimal balance = 0;
            decimal onetimeadj = 0;
            decimal expbyend = 0;
            decimal variance = 0;

            var overunder_desc = "";
            var onetimeadj_desc = "";
            var expbyend_desc = "";

            decimal sum_obligations = 0;
            decimal sum_income = 0;
            decimal sum_adjustments = 0;
            decimal sum_totalobligations = 0;
            decimal sum_projection = 0;
            decimal sum_allowance = 0;
            decimal sum_balance = 0;
            decimal sum_variance = 0;

            decimal total_obligations = 0;
            decimal total_income = 0;
            decimal total_adjustments = 0;
            decimal total_totalobligations = 0;
            decimal total_projection = 0;
            decimal total_allowance = 0;
            decimal total_balance = 0;
            var total_variance = 0;

            decimal q_projection = 0;
            decimal q_allowance = 0;
            decimal q_balance = 0;
            decimal q_adjustments = 0;

            decimal sum_q_projection = 0;
            decimal sum_q_allowance = 0;
            decimal sum_q_balance = 0;
            decimal total_q_projection = 0;
            decimal total_q_allowance = 0;
            decimal total_q_balance = 0;

            DataRow[] dr_col;

            //first part of Report:
            /*****************************************************************************************/
            tr = new HtmlTableRow();
            //1 column
            td = new HtmlTableCell();
            td.Attributes.Add("class", "reportHeaderBlue");
            //td.Width = "300";
            tr.Cells.Add(td);
            //2,3,4,5 columns
            td = new HtmlTableCell();
            td.ColSpan = 4;
            td.InnerText = String.Format("{0:MMMM}", DateTime.Parse(BookMonth + "/01/2010"));
            td.Align = "center";
            td.Attributes.Add("class", "reportHeaderBlue");
            tr.Cells.Add(td);
            //6,7,8 columns
            td = new HtmlTableCell();
            td.ColSpan = 3;
            td.InnerText = "Quarterly Projection";
            td.Align = "center";
            td.Attributes.Add("class", "reportHeaderBlue");
            tr.Cells.Add(td);
            //9,10,11,12 columns
            td = new HtmlTableCell();
            td.ColSpan = 4;
            td.InnerText = "End-of-Year Projection";
            td.Align = "center";
            td.Attributes.Add("class", "reportHeaderBlue");
            tr.Cells.Add(td);
            TableToDraw.Rows.Add(tr);

            //headers row:
            tr = new HtmlTableRow();
            tr.Cells.AddHeaderCell("FUNCTIONS:", "reportHeaderBlueNAlign", "left");
            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_OBLIGATIONS, "reportHeaderGreen"));
            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_INCOME, "reportHeaderGreen"));
            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_ADJUSTMENTS, "reportHeaderGreen"));
            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_TOTAL_OBLIGATIONS, "reportHeaderGreen"));

            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_Q_PROJECTION, "reportHeaderYellow"));
            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_Q_ALLOWANCE, "reportHeaderYellow"));
            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_Q_BALANCE, "reportHeaderYellow"));

            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_PROJECTION, "reportHeaderGreen"));
            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_ALLOWANCE, "reportHeaderGreen"));
            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_BALANCE, "reportHeaderGreen"));
            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_VARIANCE, "reportHeaderGreen"));
            TableToDraw.Rows.Add(tr);

            /*****************************************************************************************/
            foreach (DataRow drConfig in dsConfig.Tables[0].Rows)
            {
                tr = new HtmlTableRow();
                tr.Attributes.Add("class", "reportRow");
                tr.Cells.AddHeaderCell(drConfig["Name"].ToString(), drConfig["CaptionStyle"].ToString(), "left");

                if ((int)drConfig["OrderNumber"] == 0)
                {
                    //complete empty row:
                    td = new HtmlTableCell();
                    td.ColSpan = 11;
                    td.InnerText = "";
                    tr.Cells.Add(td);
                }
                else if ((int)drConfig["OrderNumber"] == 99)
                {
                    //complete row with summary numbers:
                    tr.Cells.AddDisplayedMoneyCell(sum_obligations, "reportTotal");
                    tr.Cells.AddDisplayedMoneyCell(sum_income, "reportTotal");
                    tr.Cells.AddDisplayedMoneyCell(sum_adjustments, "reportTotal");
                    tr.Cells.AddDisplayedMoneyCell(sum_totalobligations, "reportTotal");

                    tr.Cells.AddDisplayedMoneyCell(sum_q_projection, "reportTotal");
                    tr.Cells.AddDisplayedMoneyCell(sum_q_allowance, "reportTotal");
                    sum_q_balance = sum_q_allowance - sum_q_projection;
                    tr.Cells.AddDisplayedMoneyCell(sum_q_balance, "reportTotal");

                    tr.Cells.AddDisplayedMoneyCell(sum_projection, "reportTotal");
                    tr.Cells.AddDisplayedMoneyCell(sum_allowance, "reportTotal");
                    sum_balance = sum_allowance - sum_projection;
                    tr.Cells.AddDisplayedMoneyCell(sum_balance, "reportTotal");
                    sum_variance = (sum_allowance == 0) ? 0 : sum_balance / sum_allowance * 100;
                    tr.Cells.AddTextCell(string.Format("{0:0}%", sum_variance), "reportTotal");

                    sum_obligations = 0;
                    sum_income = 0;
                    sum_adjustments = 0;
                    sum_totalobligations = 0;
                    sum_q_projection = 0;
                    sum_q_allowance = 0;
                    sum_q_balance = 0;
                    sum_projection = 0;
                    sum_allowance = 0;
                    sum_balance = 0;
                    sum_variance = 0;
                }
                else
                {
                    dr_col = dtState.Select(String.Format("GROUP_CD={0}", (int)drConfig["GROUP_CD"]));
                    if (dr_col.Length == 0)
                    {
                        obligations = 0;
                        income = 0;
                        adjustments = 0;
                        totalobligations = 0;
                        projection = 0;
                        allowance = 0;
                        balance = 0;
                        variance = 0;
                        q_projection = 0;
                        q_allowance = 0;
                        q_balance = 0;
                    }
                    else
                    {
                        obligations = (decimal)dr_col[0]["Obligations"];
                        income = (decimal)dr_col[0]["Income"];
                        adjustments = (decimal)dr_col[0]["Adjustments"];
                        totalobligations = (decimal)dr_col[0]["TotalObligations"];
                        projection = (decimal)dr_col[0]["Projection"];
                        allowance = (decimal)dr_col[0]["Allowance"];
                        balance = allowance - projection;
                        variance = (allowance == 0) ? 0 : balance / allowance * 100;
                        q_projection = (decimal)dr_col[0]["Projection_Q"];
                        q_allowance = (decimal)dr_col[0]["Allowance_Q"];
                        q_balance = q_allowance - q_projection;
                    }

                    sum_obligations += obligations;
                    total_obligations += obligations;
                    sum_income += income;
                    total_income += income;
                    sum_adjustments += adjustments;
                    total_adjustments += adjustments;
                    sum_totalobligations += totalobligations;
                    total_totalobligations += totalobligations;
                    sum_projection += projection;
                    total_projection += projection;
                    sum_allowance += allowance;
                    total_allowance += allowance;

                    sum_q_projection += q_projection;
                    sum_q_allowance += q_allowance;
                    total_q_projection += q_projection;
                    total_q_allowance += q_allowance;

                    //complete the row:
                    if (!BuildReportForExcel)
                    {
                        td = DisplayedMoneyCell(obligations, "reportAmountLink");
                        td.Attributes.Add("onclick", String.Format(cell_link_onclick, (int)FundsReviewViewMode.fvObligations, (int)drConfig["GROUP_CD"]));
                        tr.Cells.Add(td);
                    }
                    else
                        tr.Cells.AddDisplayedMoneyCell(obligations, "reportAmount");
                    if (!BuildReportForExcel)
                    {
                        td = DisplayedMoneyCell(income, "reportAmountLink");
                        td.Attributes.Add("onclick", String.Format(cell_link_onclick, (int)FundsReviewViewMode.fvIncome, (int)drConfig["GROUP_CD"]));
                        tr.Cells.Add(td);
                    }
                    else
                        tr.Cells.AddDisplayedMoneyCell(income, "reportAmount");
                    tr.Cells.AddDisplayedMoneyCell(adjustments, "reportAmount");
                    tr.Cells.AddDisplayedMoneyCell(totalobligations, "reportAmount");

                    tr.Cells.AddDisplayedMoneyCell(q_projection, "reportAmount");
                    tr.Cells.AddDisplayedMoneyCell(q_allowance, "reportAmount");
                    tr.Cells.AddDisplayedMoneyCell(q_balance, "reportAmount");

                    tr.Cells.AddDisplayedMoneyCell(projection, "reportAmount");
                    tr.Cells.AddDisplayedMoneyCell(allowance, "reportAmount");
                    tr.Cells.AddDisplayedMoneyCell(balance, "reportAmount");
                    tr.Cells.AddTextCell(String.Format("{0:0}%", variance), "reportAmount");
                }
                TableToDraw.Rows.Add(tr);
            }
            /*****************************************************************************************/
            //add Grand Total row:
            tr = new HtmlTableRow();
            tr.Attributes.Add("class", "reportRow");
            //display sum totals for the whole report:
            tr.Cells.AddHeaderCell("Grand Total", "reportHeaderBlueNAlign", "left");
            tr.Cells.AddDisplayedMoneyCell(total_obligations, "reportHeaderBlueNAlign", "right");
            tr.Cells.AddDisplayedMoneyCell(total_income, "reportHeaderBlueNAlign", "right");
            tr.Cells.AddDisplayedMoneyCell(total_adjustments, "reportHeaderBlueNAlign", "right");
            tr.Cells.AddDisplayedMoneyCell(total_totalobligations, "reportHeaderBlueNAlign", "right");

            tr.Cells.AddDisplayedMoneyCell(total_q_projection, "reportHeaderBlueNAlign", "right");
            tr.Cells.AddDisplayedMoneyCell(total_q_allowance, "reportHeaderBlueNAlign", "right");
            total_q_balance = total_q_allowance - total_q_projection;
            tr.Cells.AddDisplayedMoneyCell(total_q_balance, "reportHeaderBlueNAlign", "right");

            tr.Cells.AddDisplayedMoneyCell(total_projection, "reportHeaderBlueNAlign", "right");
            tr.Cells.AddDisplayedMoneyCell(total_allowance, "reportHeaderBlueNAlign", "right");
            total_balance = total_allowance - total_projection;
            tr.Cells.AddDisplayedMoneyCell(total_balance, "reportHeaderBlueNAlign", "right");
            total_variance = (int)((total_allowance == 0) ? 0 : total_balance / total_allowance * 100);
            tr.Cells.AddHeaderCell(String.Format("{0:0}%", total_variance), "reportHeaderBlueNAlign", "right");

            TableToDraw.Rows.Add(tr);
            /*****************************************************************************************/

            tr = new HtmlTableRow();
            tr.Attributes.Add("class", "reportRow");
            //complete empty row:
            td = new HtmlTableCell();
            td.ColSpan = 12;
            td.Height = "8";
            td.InnerText = "";
            tr.Cells.Add(td);
            TableToDraw.Rows.Add(tr);


            //second part of Report:
            /*****************************************************************************************/
            tr = new HtmlTableRow();
            //1 column
            td = new HtmlTableCell();
            td.Attributes.Add("class", "reportHeaderBlue");
            tr.Cells.Add(td);
            //2,3,4,5 columns
            td = new HtmlTableCell();
            td.ColSpan = 4;
            td.InnerText = "RWA and Over/Under Accrued Entries";
            td.Align = "center";
            td.Attributes.Add("class", "reportHeaderBlue");
            tr.Cells.Add(td);
            //6,7,8,9 columns
            td = new HtmlTableCell();
            td.ColSpan = 4;
            td.InnerText = "One-Time Adjustments";
            td.Align = "center";
            td.Attributes.Add("class", "reportHeaderBlue");
            tr.Cells.Add(td);
            //10,11,12 columns
            td = new HtmlTableCell();
            td.ColSpan = 3;
            td.InnerText = "Expected by Year End";
            td.Align = "center";
            td.Attributes.Add("class", "reportHeaderBlue");
            tr.Cells.Add(td);
            TableToDraw.Rows.Add(tr);

            //headers row:
            tr = new HtmlTableRow();
            tr.Cells.AddHeaderCell("Adjustments:", "reportHeaderBlueNAlign", "left");
            td = HeaderCellWithTooltip(CAPTION_RWA, "reportHeaderGreen");
            if (!BuildReportForExcel)
            {
                td.Attributes.Add("onclick", String.Format("open_rwa_popup('org={0}&fy={1}&bm={2}&bl={3}');", Organization, FiscalYear, BookMonth, BusinessLine));
            }
            tr.Cells.Add(td);
            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_OVERUNDERACCRUED, "reportHeaderGreen"));
            td = new HtmlTableCell();
            td.ColSpan = 2;
            td.InnerText = CAPTION_EXPLANATION;
            td.Align = "center";
            td.Attributes.Add("class", "reportHeaderGreen");
            tr.Cells.Add(td);

            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_ONETIMEADJ, "reportHeaderGreen"));
            td = new HtmlTableCell();
            td.ColSpan = 3;
            td.InnerText = CAPTION_EXPLANATION;
            td.Align = "center";
            td.Attributes.Add("class", "reportHeaderGreen");
            tr.Cells.Add(td);
            //tr.Cells.Add(HeaderCellWithTooltip(CAPTION_EXPLANATION, "reportHeaderGreen"));
            tr.Cells.Add(HeaderCellWithTooltip(CAPTION_EXPBYYEAREND, "reportHeaderGreen"));
            td = new HtmlTableCell();
            td.ColSpan = 2;
            td.InnerText = CAPTION_EXPLANATION;
            td.Align = "center";
            td.Attributes.Add("class", "reportHeaderGreen");
            tr.Cells.Add(td);
            //tr.Cells.Add(HeaderCellWithTooltip(CAPTION_EXPLANATION, "reportHeaderGreen"));
            TableToDraw.Rows.Add(tr);

            /*****************************************************************************************/
            foreach (DataRow drConfig in dsConfig.Tables[0].Rows)
            {


                if ((int)drConfig["OrderNumber"] == 0)
                {
                    tr = new HtmlTableRow();
                    tr.Attributes.Add("class", "reportRow");
                    tr.Cells.AddHeaderCell(drConfig["Name"].ToString(), drConfig["CaptionStyle"].ToString());

                    //complete empty row:
                    td = new HtmlTableCell();
                    td.Attributes.Add("class", "reportTotal");
                    td.ColSpan = 11;
                    td.InnerText = "";
                    tr.Cells.Add(td);

                    TableToDraw.Rows.Add(tr);

                }
                else if ((int)drConfig["OrderNumber"] != 99)
                //skip the 'Totals' row in this part!
                {
                    tr = new HtmlTableRow();
                    tr.Attributes.Add("class", "reportRow");
                    tr.Cells.AddHeaderCell(drConfig["Name"].ToString(), drConfig["CaptionStyle"].ToString());

                    dr_col = dtState.Select(String.Format("GROUP_CD={0}", (int)drConfig["GROUP_CD"]));
                    if (dr_col.Length == 0)
                    {
                        rwa = 0;
                        overunderaccrued = 0;
                        onetimeadj_desc = "";
                        onetimeadj = 0;
                        onetimeadj_desc = "";
                        expbyend = 0;
                        expbyend_desc = "";
                        q_adjustments = 0;
                    }
                    else
                    {
                        rwa = (decimal)dr_col[0]["RWA"];
                        overunderaccrued = (decimal)dr_col[0]["OverUnderAccrued"];
                        onetimeadj = (decimal)dr_col[0]["OneTimeAdjustments"];
                        expbyend = (decimal)dr_col[0]["ExpByYearEnd"];
                        q_adjustments = (decimal)dr_col[0]["Adjustments_Q"];
                        //no description for BusinessLine
                        if (BusinessLine == "")
                        {
                            overunder_desc = (string)Utility.GetNotNullValue(dr_col[0]["OverUnderAccrDesc"], "String");
                            onetimeadj_desc = (string)Utility.GetNotNullValue(dr_col[0]["OneTimeAdjDesc"], "String");
                            expbyend_desc = (string)Utility.GetNotNullValue(dr_col[0]["ExpByYearEndDesc"], "String");
                        }
                        else
                        {
                            overunder_desc = "";
                            onetimeadj_desc = "";
                            expbyend_desc = "";
                        }
                    }

                    //complete the row:
                    tr.Cells.AddDisplayedMoneyCell(rwa, "reportAmount");

                    td = DisplayedMoneyCell(overunderaccrued, "reportAmountLink");
                    if (!BuildReportForExcel)
                        td.Attributes.Add("onclick", String.Format(cell_popup_onclick, (int)FundsStatusUserEntryType.ueOverUnderAccrued, (int)drConfig["GROUP_CD"], ""));
                    tr.Cells.Add(td);
                    td = new HtmlTableCell();
                    td.ColSpan = 2;
                    td.InnerText = overunder_desc;
                    tr.Cells.Add(td);

                    td = DisplayedMoneyCell(onetimeadj, "reportAmountLink");
                    if (!BuildReportForExcel)
                        td.Attributes.Add("onclick", String.Format(cell_popup_onclick, (int)FundsStatusUserEntryType.ueOneTimeAdjustment, (int)drConfig["GROUP_CD"], (string)Utility.GetNotNullValue(drConfig["AddAdjustment"], "String")));
                    tr.Cells.Add(td);
                    td = new HtmlTableCell();
                    td.ColSpan = 3;
                    td.InnerText = onetimeadj_desc;
                    tr.Cells.Add(td);

                    td = DisplayedMoneyCell(expbyend, "reportAmountLink");
                    if (!BuildReportForExcel)
                        td.Attributes.Add("onclick", String.Format(cell_popup_onclick, (int)FundsStatusUserEntryType.ueExpectedByYearEnd, (int)drConfig["GROUP_CD"], ""));
                    tr.Cells.Add(td);
                    td = new HtmlTableCell();
                    td.ColSpan = 2;
                    td.InnerText = expbyend_desc;
                    tr.Cells.Add(td);

                    TableToDraw.Rows.Add(tr);
                }

            }
            tr = new HtmlTableRow();
            tr.Attributes.Add("class", "reportHeaderBlue");
            //complete empty row:
            td = new HtmlTableCell();
            td.ColSpan = 12;
            td.Height = "8";
            td.InnerText = "";
            tr.Cells.Add(td);
            TableToDraw.Rows.Add(tr);

            /*****************************************************************************************/

        }

        //private string DisplayMoney(decimal ValueToDisplay)
        //{
        //    if (ValueToDisplay < 0)
        //        return String.Format("{0:($0,0)}", ValueToDisplay * (-1));
        //    else if (ValueToDisplay > 9)
        //        return String.Format("{0:$0,0}", ValueToDisplay);
        //    else
        //        return String.Format("{0:$0}", ValueToDisplay);
        //}

        //private string DisplayPercent(int ValueToDisplay)
        //{
        //    return String.Format("{0}%", ValueToDisplay);
        //}

        //private HtmlTableCell TextCell(string ValueToDisplay, string CSSClass)
        //{

        //    var td = new HtmlTableCell();
        //    td.InnerText = ValueToDisplay;
        //    td.Attributes.Add("class", CSSClass);
        //    return td;
        //}

        //private HtmlTableCell DisplayedPercentCell(int ValueToDisplay, string CSSClass)
        //{

        //    var td = new HtmlTableCell();
        //    td.InnerText = String.Format("{0}%", ValueToDisplay);
        //    td.Attributes.Add("class", CSSClass);
        //    return td;
        //}

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
            return td;
        }

        private HtmlTableCell DisplayedMoneyCell(decimal ValueToDisplay, string CSSClass, string Align)
        {

            var td = new HtmlTableCell();
            if (ValueToDisplay < 0)
                td.InnerText = String.Format("{0:($0,0)}", ValueToDisplay * (-1));
            else if (ValueToDisplay > 9)
                td.InnerText = String.Format("{0:$0,0}", ValueToDisplay);
            else
                td.InnerText = String.Format("{0:$0}", ValueToDisplay);
            td.Attributes.Add("class", CSSClass);
            if (Align != "")
                td.Align = Align;
            return td;
        }

        private HtmlTableCell HeaderCell(string HeaderText, string CSSClass)
        {

            var td = new HtmlTableCell();
            td.InnerText = HeaderText;
            td.Attributes.Add("class", CSSClass);
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

        private HtmlTableCell HeaderCellWithTooltip(string HeaderText, string CSSClass)
        {
            var td = HeaderCell(HeaderText, CSSClass);
            if (!BuildReportForExcel)
                td.Attributes.Add("onMouseOver", AddTooltipPopup(HeaderText));
            return td;
        }

    }
}