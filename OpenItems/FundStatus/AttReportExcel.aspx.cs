using Data;
using OpenItems.Properties;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web.UI;
    using System.Configuration;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    public partial class FundStatus_AttReportExcel : PageBase
    {
        private readonly FundStatusBO FundStatus;
        private readonly ReportBO Report;

        public FundStatus_AttReportExcel()
        {
            FundStatus = new FundStatusBO(this.Dal);
            Report = new ReportBO(this.Dal, new EmailsBO(this.Dal));
        }

        public int p_iHeaderLines = 0;		//will be inserted to Excel.xml,
        public string s_PageBrakes = "";    //which included into sheet

        public Label lblMsg;
        public HtmlTable Sheet;

        const string HT_KEY_BL = "BL";
        const string HT_KEY_ORG = "Org";
        const string HT_KEY_YEAR = "Year";
        const string HT_KEY_BOOK_MONTH = "BookMonth";
        const string HT_KEY_BA = "BA";
        const string HT_KEY_VIEW = "View";
        const string HT_KEY_GROUP = "Group";
        const string HT_KEY_SUM_FUNC = "SumFunc";
        const string HT_KEY_OC = "OC";
        const string HT_KEY_CE = "CE";
        const string HT_KEY_DOC_NUM = "DocNum";

        const string HT_KEY_FILTER = "SUM_FUNC_FILTER";
        const string HT_KEY_FILTER_SUM_FUNC = "REPORT_SUM_FUNCTION";
        const string HT_KEY_FILTER_OCCODE = "REPORT_OC_CODE";

        const string CELL_ALIGN_LEFT = "left";
        const string CELL_ALIGN_RIGHT = "right";
        const string CELL_ALIGN_MIDDLE = "middle";

        static string[] Month = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                lblMsg = (Label)Master.FindControl("lblMsg");
                Sheet = (HtmlTable)Master.FindControl("Sheet");
                var email_request = Request.QueryString["req"];
                var req_id = Int32.Parse(email_request);

                BuildFundsSearchTable(req_id);

                FinalizeOutput();
            }
            catch (Exception ex)
            {
                lblMsg.Text = ex.Message;
            }
        }

        private void BuildFundsSearchTable(int EmailRequestID)
        {
            //get Query parameters from tblHistory by EmailRequestID:
            var dsHistory = Report.GetHistoryRecordByEmailRequest(EmailRequestID);
            if (dsHistory == null || dsHistory.Tables[0] == null || dsHistory.Tables[0].Rows.Count == 0)
                throw new Exception("There is no enough data to complete search. Please try again to request another report. Thank you.");

            var criteria_fields = (string)Utility.GetNotNullValue(dsHistory.Tables[0].Rows[0]["CustomField01"], "String");
            var criteria_values = (string)Utility.GetNotNullValue(dsHistory.Tables[0].Rows[0]["CustomField02"], "String");

            var business_line = "";
            var organization = "";
            var fiscal_year = "";
            var book_month = "";
            var budget_activity = "";
            var group_cd = "";
            var summary_function = "";
            var oc_code = "";
            var CostElem = "";
            var doc_number = "";
            var obl_income_view = 1;    //default value

            var arr_fields = criteria_fields.Split(new char[] { '|' });
            var arr_values = criteria_values.Split(new char[] { '|' });

            for (var i = 0; i < arr_fields.Length; i++)
            {
                switch (arr_fields[i])
                {
                    case HT_KEY_ORG:
                        organization = arr_values[i];
                        break;
                    case HT_KEY_YEAR:
                        fiscal_year = arr_values[i];
                        break;
                    case HT_KEY_BOOK_MONTH:
                        book_month = arr_values[i];
                        break;
                    case HT_KEY_VIEW:
                        obl_income_view = Int32.Parse(arr_values[i]);
                        break;
                    case HT_KEY_SUM_FUNC:
                        summary_function = arr_values[i];
                        break;
                    case HT_KEY_OC:
                        oc_code = arr_values[i];
                        break;
                    case HT_KEY_CE:
                        CostElem = arr_values[i];
                        break;
                    case HT_KEY_DOC_NUM:
                        doc_number = arr_values[i];
                        break;
                    case HT_KEY_BA:
                        budget_activity = arr_values[i];
                        break;
                    case HT_KEY_BL:
                        business_line = arr_values[i];
                        break;
                    case HT_KEY_GROUP:
                        group_cd = arr_values[i];
                        break;
                }
            }

            var title_text = "";
            switch (obl_income_view)
            {
                case (int)FundsReviewViewMode.fvObligations:
                    title_text = "Obligations";
                    break;
                case (int)FundsReviewViewMode.fvIncome:
                    title_text = "Income";
                    break;
                case (int)FundsReviewViewMode.fvOneTimeAdjustments:
                    title_text = "One Time Adjustments";
                    break;
            }

            //start Excel headers:
            var tr = new HtmlTableRow();
            tr.Cells.AddCellLeft("Funds Search Results", "title", 14);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCellLeft(title_text, "title2", 14);
            Sheet.Rows.Add(tr);

            //display query parameters in Excel:
            tr = new HtmlTableRow();
            tr.Cells.AddCellLeft("Organization:", "reportCaption2", 2);
            tr.Cells.AddCellLeft(organization, "title2", 12);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCellLeft("Fiscal Year:", "reportCaption2", 2);
            tr.Cells.AddCellLeft(fiscal_year, "title2", 12);
            Sheet.Rows.Add(tr);
            tr = new HtmlTableRow();
            tr.Cells.AddCellLeft("Book Month:", "reportCaption2", 2);
            if (book_month == "")
                tr.Cells.AddCellLeft("All Available", "title2", 12);
            else
            {
                var s = "";
                var ss = book_month.Split(new char[] { ',' });
                foreach (var m in ss)
                {
                    s += String.Format("{0:MMMM}", DateTime.Parse(m + "/" + fiscal_year)) + ", ";
                }
                s = s.Substring(0, s.Length - 2);
                tr.Cells.AddCellLeft(s, "title2", 12);
            }
            Sheet.Rows.Add(tr);

            tr = new HtmlTableRow();
            tr.Cells.AddCellLeft("Budget Activity:", "reportCaption2", 2);
            tr.Cells.AddCellLeft(budget_activity, "title2", 12);
            Sheet.Rows.Add(tr);

            if (group_cd != null && group_cd != "")
            {
                var ds_g = FundStatus.GetReportFunctionGroupList();
                if (ds_g != null && ds_g.Tables[0] != null && ds_g.Tables[0].Rows.Count > 0)
                {
                    var dr_col = ds_g.Tables[0].Select("GROUP_CD=" + group_cd);
                    if (dr_col.Length > 0)
                    {
                        var group_name = dr_col[0]["Name"].ToString();
                        tr = new HtmlTableRow();
                        tr.Cells.AddCellLeft("Filter by Function Report Group:", "reportCaption2", 2);
                        tr.Cells.AddCellLeft(group_name, "tableBold", 12);
                        Sheet.Rows.Add(tr);
                    }
                }
            }
            if (summary_function != null && summary_function != "")
            {
                tr = new HtmlTableRow();
                tr.Cells.AddCellLeft("Filter by Functions:", "reportCaption2", 2);
                tr.Cells.AddCellLeft(summary_function, "tableBold", 12);
                Sheet.Rows.Add(tr);
            }
            if (oc_code != null && oc_code != "")
            {
                tr = new HtmlTableRow();
                tr.Cells.AddCellLeft("Filter by OC Code:", "reportCaption2", 2);
                tr.Cells.AddCellLeft(oc_code, "tableBold", 12);
                Sheet.Rows.Add(tr);
            }
            if (CostElem != null && CostElem != "")
            {
                tr = new HtmlTableRow();
                tr.Cells.AddCellLeft("Filter by Cost Elements:", "reportCaption2", 2);
                tr.Cells.AddCellLeft(CostElem, "tableBold", 12);
                Sheet.Rows.Add(tr);
            }
            if (doc_number != "")
            {
                tr = new HtmlTableRow();
                tr.Cells.AddCellLeft("DocNumber like:", "reportCaption2", 2);
                tr.Cells.AddCellLeft(doc_number, "tableBold", 12);
                Sheet.Rows.Add(tr);
            }

            tr = new HtmlTableRow();
            tr.Cells.AddCell("", "", 14);
            Sheet.Rows.Add(tr);
            p_iHeaderLines = Sheet.Rows.Count + 1;

            //get the data:
            var records = 0;
            decimal total_amount = 0;
            object obj_org_code = (organization != "") ? organization : null;
            object obj_book_month = (book_month != "") ? book_month : null;
            object obj_sum_func = (summary_function != "") ? summary_function : null;
            object obj_oc_code = (oc_code != "") ? oc_code : null;
            object obj_CE = (CostElem != "") ? CostElem : null;
            object obj_doc_num = (doc_number != "") ? doc_number : null;
            object obj_group_cd = (group_cd != "") ? group_cd : null;

            var ds = FundStatus.GetSearchResults(obl_income_view, fiscal_year, budget_activity, out records, out total_amount,
                obj_org_code, obj_book_month, obj_group_cd, obj_sum_func, obj_oc_code, obj_CE, obj_doc_num, Settings.Default.QueryResultMaxRecords, true);

            if (ds == null || records == 0)
            {
                //no records:
                tr = new HtmlTableRow();
                tr.Cells.AddCellLeft("No records found", "reportCaption", 14);
                Sheet.Rows.Add(tr);
                p_iHeaderLines = 0;
            }
            else
            {
                var dt = ds.Tables[0].Clone();
                dt.Columns["Amount"].DataType = typeof(int);
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    dt.ImportRow(r);
                }
                dt.Columns["Amount"].ColumnName = "$ Amount";
                foreach (DataRow r in dt.Rows)
                {
                    if (r["book month"].ToString() != "")
                        r["book month"] = String.Format("{0:MMMM}", DateTime.Parse(r["book month"].ToString() + "/2000"));
                    r["$ Amount"] = Convert.ToInt32(r["$ Amount"]);
                }
                BuildGeneralExcelOutput(dt);
            }
        }


        private HtmlTableCell CreateCellLeft(string InnerText, string CssClass, int cSpan)
        {
            var HCell = new HtmlTableCell();
            HCell.InnerText = InnerText;
            if (CssClass != "")
                HCell.Attributes["class"] = CssClass;
            HCell.Align = "left";
            if (cSpan > 1)
                HCell.ColSpan = cSpan;
            return HCell;
        }

        private HtmlTableCell CreateCell(string InnerText, string CssClass, int cSpan)
        {
            var HCell = new HtmlTableCell();
            HCell.InnerText = InnerText;
            if (CssClass != "")
                HCell.Attributes["class"] = CssClass;
            HCell.Align = "center";
            if (cSpan > 1)
                HCell.ColSpan = cSpan;
            return HCell;
        }

        private HtmlTableCell CreateCell(string InnerText, string CssClass, string CellAlign)
        {
            var objCell = new HtmlTableCell();
            objCell.InnerText = InnerText;
            objCell.Attributes["class"] = CssClass;
            if (CellAlign != "")
                objCell.Align = CellAlign;
            return objCell;
        }

        private void BuildGeneralExcelOutput(DataTable dt)
        {
            HtmlTableCell objCell;
            var objRow = new HtmlTableRow();            //add header first

            for (var i = 0; i < dt.Columns.Count; i++)
                objRow.Cells.AddCell(dt.Columns[i].ToString(), "tableCaption", "");

            Sheet.Rows.Add(objRow);

            var ExcelLine = 1;
            foreach (DataRow row in dt.Rows)
            {
                objRow = new HtmlTableRow();

                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    objCell = new HtmlTableCell();
                    //objCell.Style.Add("background-color", ExcelLine % 2 > 0 ? "#CCFFCC" : "#CCFFFF");
                    /*
                    if (i == (dt.Columns.Count - 1) && dt.Columns[i].DataType.Name == "String")
                    {
                        //only for the last column (usually it will include 'Comments'):
                        //check if the length of string more than 250 char - devide into 2 columns:
                        string str_comm = row[dt.Columns[i].ToString()].ToString();
                        if (str_comm.Length > 250)
                        {
                            objCell.InnerText = str_comm.Substring(0, 250);
                            objCell.Attributes["class"] = "textLeft";
                            objRow.Cells.Add(objCell);

                            objCell = new HtmlTableCell();
                            objCell.InnerText = str_comm.Substring(250);
                        }
                        else
                            objCell.InnerText = str_comm;
                    }
                    else
                     */

                    objCell.InnerText = row[dt.Columns[i].ToString()].ToString();

                    switch (dt.Columns[i].DataType.Name)
                    {
                        case "Int32":
                            objCell.Attributes["class"] = "WholeNumber";
                            break;
                        case "DateTime":
                            objCell.Attributes["class"] = "DateDigits";
                            break;
                        case "Decimal":
                            var col = dt.Columns[i].ColumnName;
                            if (col == "TotalLine" || col == "UDO" || col == "UDOShouldBe" ||
                                col == "DO" || col == "DOShouldBe" || col == "COM" || col == "ACCR" ||
                                col == "PENDPYMT" || col == "TotalOrder" || col == "PYMTS_TRAN" || col == "PYMTS_CONF" ||
                                col == "HOLDBACKS")
                                objCell.Attributes["class"] = "Money";
                            else
                                objCell.Attributes["class"] = "Number";
                            break;
                        case "String":
                        default:
                            objCell.Attributes["class"] = "textLeft";
                            break;
                    }
                    objRow.Cells.Add(objCell);
                }
                ExcelLine++;
                Sheet.Rows.Add(objRow);
            }
        }

        private void FinalizeOutput()
        {
            //Translate Form in HTML format and send it to client Excel
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";

            Response.ContentType = "application/vnd.ms-excel";
            this.EnableViewState = false;
            var Stringer = new System.IO.StringWriter();
            var Htmler = new HtmlTextWriter(Stringer);
            this.RenderControl(Htmler);
            var s = Stringer.ToString();
            Response.Write(s);

            /*
            //save file on the server for archiving if needed 
            string FILE_NAME = this.Page.Request.PhysicalApplicationPath + "Excel.xls";
            if (File.Exists(FILE_NAME))
                File.Delete(FILE_NAME);
            StreamWriter sw = File.CreateText(FILE_NAME);
            sw.Write(s);
            sw.Close();
            */
            Response.End();
        }

    }

}