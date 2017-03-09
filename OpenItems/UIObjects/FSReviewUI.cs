namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web.UI.HtmlControls;

    /// <summary>
    /// Summary description for FSReviewUI:
    /// Build User Interface for FundStatus screens
    /// </summary>
    public class FSReviewUI
    {


        //const string COL_FUNCTION_CAPTION = "Function";
        //const string COL_OCCODE_CAPTION = "OC";
        //const string COL_TOTAL_CAPTION = "Total";

        //const string DATA_FUNCTION_NAME = "FUNC_CD";
        //const string DATA_OCCODE_NAME = "OBJ_CLASS_CD";
        //const string DATA_BOOK_MONTH_NAME = "BOOK_MONTH";
        //const string DATA_COL_TOTAL_NAME = "SUM_TotalAmount";

        //const bool MERGE_2COLUMNS_TOTAL_ROW = true;
        //const bool MAKE_DEFAULT_COL_WIDTH = true;
        const string DEFAULT_COL_WIDTH = "80px";

        const string CELL_ALIGN_RIGHT = "right";
        const string CELL_ALIGN_LEFT = "left";
        const string CELL_ALIGN_CENTER = "center";

        string month_list = "10,11,12,01,02,03,04,05,06,07,08,09";
        string[] month_arr = null;

        public HtmlTable TableToDraw { get; set; }

        public DataTable SourceDataTable { get; set; }

        public DataTable TotalsDataTable { get; set; }

        public bool MonthlyView { get; set; }

        public string BookMonth { get; set; }

        public string CellLinkOnClick { get; set; }

        public bool BuildReportForExcel { get; set; }

        public bool DisplayColumnObjClassCode { get; set; }

        public void BuildTheTable()
        {

            //make sure that all properties have been initialized:
            if (TableToDraw == null || TotalsDataTable == null)
                throw new Exception("Application Error. Building report table has failed.");

            if (BookMonth != "00")
                month_list = month_list.Substring(0, month_list.IndexOf(BookMonth) + 2);
            month_arr = month_list.Split(new char[] { ',' });

            //build the results table:
            DrawTableHeader();
            DrawTableContent();
        }

        private void DrawTableContent()
        {
            var tr = new HtmlTableRow();
            HtmlTableCell td;
            DataRow[] dr;
            var arr_totals = new decimal[13];
            var arr_func_totals = new decimal[13];
            decimal current_amount;
            var func = "";
            int m_index;

            foreach (DataRow dr_total in TotalsDataTable.Rows)
            {

                if (func != dr_total["SUM_FUNC"].ToString())
                {
                    //new function section:
                    /************************************************/
                    //first, totals row for previous function:
                    /************************************************/
                    if (func != "")
                    {
                        //func can be equal "" only for the first row - in this case we don't have prev.totals row
                        tr = new HtmlTableRow();

                        td = GetNewCellText("Total", "tableTotal", CELL_ALIGN_LEFT);
                        if (DisplayColumnObjClassCode)
                            td.ColSpan = 2;
                        tr.Cells.Add(td);
                        if (MonthlyView)
                        {
                            foreach (var m in month_arr)
                            {
                                m_index = Int32.Parse(m);
                                tr.Cells.AddNewMoneyCell(arr_func_totals[m_index], "tableTotal", CELL_ALIGN_RIGHT);
                                arr_func_totals[m_index] = 0;
                            }
                        }
                        tr.Cells.AddNewMoneyCell(arr_func_totals[0], "tableTotal", CELL_ALIGN_RIGHT);

                        TableToDraw.Rows.Add(tr);
                    }
                    /************************************************/
                    //then, start new function section:
                    /************************************************/
                    tr = new HtmlTableRow();

                    func = dr_total["SUM_FUNC"].ToString();

                    tr.Cells.AddCell(func, "tableBold", CELL_ALIGN_LEFT);
                    if (DisplayColumnObjClassCode)
                        tr.Cells.AddCell(dr_total["OBJ_CLASS_CD"].ToString(), "tableBold", CELL_ALIGN_RIGHT);

                    if (MonthlyView)
                    {
                        foreach (var m in month_arr)
                        {
                            m_index = Int32.Parse(m);
                            dr = SourceDataTable.Select(String.Format("SUM_FUNC='{0}' AND OBJ_CLASS_CD='{1}' AND BookMonth='{2}'", dr_total["SUM_FUNC"], dr_total["OBJ_CLASS_CD"], m));
                            if (dr.Length > 0)
                                current_amount = (decimal)dr[0]["Amount"];
                            else
                                current_amount = 0;
                            arr_totals[m_index] += current_amount;
                            arr_func_totals[m_index] += current_amount;
                            if (BuildReportForExcel)
                                tr.Cells.AddNewMoneyCell(current_amount, "tableRow");
                            else
                                tr.Cells.AddMoneyCellLink(current_amount, m, dr_total["SUM_FUNC"].ToString(), dr_total["OBJ_CLASS_CD"].ToString(), CellLinkOnClick);
                        }
                    }

                    current_amount = (decimal)dr_total["Amount"];
                    arr_totals[0] += current_amount;
                    arr_func_totals[0] += current_amount;
                    tr.Cells.AddNewMoneyCell(current_amount, "tableRow");

                    TableToDraw.Rows.Add(tr);
                }
                /************************************************/
                //continue the same function section:
                /************************************************/
                else
                {
                    tr = new HtmlTableRow();

                    tr.Cells.AddCell("", "tableBold", CELL_ALIGN_LEFT);
                    if (DisplayColumnObjClassCode)
                        tr.Cells.AddCell(dr_total["OBJ_CLASS_CD"].ToString(), "tableBold", CELL_ALIGN_RIGHT);

                    if (MonthlyView)
                    {
                        foreach (var m in month_arr)
                        {
                            m_index = Int32.Parse(m);
                            dr = SourceDataTable.Select(String.Format("SUM_FUNC='{0}' AND OBJ_CLASS_CD='{1}' AND BookMonth='{2}'", dr_total["SUM_FUNC"], dr_total["OBJ_CLASS_CD"], m));
                            if (dr.Length > 0)
                                current_amount = (decimal)dr[0]["Amount"];
                            else
                                current_amount = 0;
                            arr_totals[m_index] += current_amount;
                            arr_func_totals[m_index] += current_amount;
                            if (BuildReportForExcel)
                                tr.Cells.AddNewMoneyCell(current_amount, "tableRow");
                            else
                                tr.Cells.AddMoneyCellLink(current_amount, m, dr_total["SUM_FUNC"].ToString(), dr_total["OBJ_CLASS_CD"].ToString(), CellLinkOnClick);
                        }
                    }

                    current_amount = (decimal)dr_total["Amount"];
                    arr_totals[0] += current_amount;
                    arr_func_totals[0] += current_amount;
                    tr.Cells.AddNewMoneyCell(current_amount, "tableRow");

                    TableToDraw.Rows.Add(tr);
                }
            }
            /************************************************/
            //add total totals section:
            /************************************************/
            tr = new HtmlTableRow();

            td = GetNewCellText("Total", "tableFooter", CELL_ALIGN_LEFT);
            if (DisplayColumnObjClassCode)
                td.ColSpan = 2;
            tr.Cells.Add(td);
            if (MonthlyView)
            {
                foreach (var m in month_arr)
                {
                    m_index = Int32.Parse(m);
                    tr.Cells.AddNewMoneyCell(arr_totals[m_index], "tableFooter");
                }
            }
            tr.Cells.AddNewMoneyCell(arr_totals[0], "tableFooter");

            TableToDraw.Rows.Add(tr);

        }

        private void DrawTableHeader()
        {
            //headers row:
            var tr = new HtmlTableRow();
            HtmlTableCell td;
            //SUM_FUNC:                
            tr.Cells.AddCell("SUM FUNC", "tableCaption", CELL_ALIGN_CENTER);
            if (DisplayColumnObjClassCode)
            {
                //OBJ CLASS CODE:      
                td = GetNewCellText("OC", "tableCaption", CELL_ALIGN_CENTER);
                td.Width = DEFAULT_COL_WIDTH;
                tr.Cells.Add(td);
            }
            //MONTH NAME:
            string caption;
            if (MonthlyView)
                foreach (var m in month_arr)
                {
                    caption = String.Format("{0:MMMM}", DateTime.Parse(m + "/01/2010"));
                    tr.Cells.AddCell(caption, "tableCaption", CELL_ALIGN_CENTER);
                }
            //TOTALS:                
            tr.Cells.AddCell("TOTAL", "tableCaption", CELL_ALIGN_CENTER);

            TableToDraw.Rows.Add(tr);

        }

        private HtmlTableCell GetNewCellText(string InnerText, string CssClass, string Align)
        {
            var td = new HtmlTableCell();
            td.InnerText = InnerText;
            td.Align = Align;
            td.AddCssClass(CssClass);

            return td;
        }

        private HtmlTableCell GetNewCellMoney(decimal Amount, string CssClass)
        {
            var td = new HtmlTableCell();
            if (Amount >= 0)
                td.InnerText = String.Format("{0:$0,0.00}", Amount);
            else
                td.InnerText = String.Format("{0:($0,0.00)}", Decimal.Negate(Amount));

            td.Align = CELL_ALIGN_RIGHT;
            td.AddCssClass(CssClass);

            return td;
        }

        private HtmlTableCell GetNewCellMoneyLink(decimal Amount, string BookMonth, string Func, string OC)
        {
            var td = new HtmlTableCell();
            if (Amount >= 0)
                td.InnerText = String.Format("{0:$0,0.00}", Amount);
            else
                td.InnerText = String.Format("{0:($0,0.00)}", Decimal.Negate(Amount));

            td.Align = CELL_ALIGN_RIGHT;
            td.AddCssClass("tableLink");
            td.AddOnClick("get_link('" + String.Format(CellLinkOnClick, BookMonth, Func, OC) + "');");

            return td;
        }

    }
}
