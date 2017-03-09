using System.Collections.Generic;
using System.Linq;
using OpenItems.Data;

namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Xml;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    public partial class ReportCustom : PageBase
    {

        public int p_iHeaderLines = 1;		//will be inserted to Excel.xml,
        public string s_PageBrakes = "";    //which included into sheet

        public Label lblMsg;
        public HtmlTable Sheet;

        private readonly Lookups LookupsBO;
        private readonly ReportBO Report;

        public ReportCustom()
        {
            LookupsBO = new Lookups(this.Dal, new AdminBO(this.Dal));
            Report = new ReportBO(this.Dal, new EmailsBO(this.Dal));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                lblMsg = (Label)Master.FindControl("lblMsg");
                Sheet = (HtmlTable)Master.FindControl("Sheet");

                if (!IsPostBack)
                {
                    if (Request.QueryString["id"] != null && Request.QueryString["id"] != "" &&
                        Request.QueryString["load"] != null && Request.QueryString["load"] != "")
                    {
                        var report_id = Int32.Parse(Request.QueryString["id"]);
                        var load_id = Int32.Parse(Request.QueryString["load"]);
                        DataRow drLoad;

                        switch (report_id)
                        {
                            case (int)OIReports.rpDaily:

                                var dailyReports = Report.GetDailyReport(load_id);
                                if (!dailyReports.Any())
                                    throw new Exception("Missing Report.");
                                else
                                    BuildDailyReport(dailyReports, null, load_id);
                                break;

                            case (int)OIReports.rpDARA:

                                var orgTotals = Report.GetTotalByOrganization(load_id);
                                if (!orgTotals.Any())
                                    throw new Exception("Missing Report.");
                                else
                                    BuildDARAReport(orgTotals, false);

                                break;

                            case (int)OIReports.rpDaraByDocNum:

                                var daraNew = Report.GetTotalDaraNew(load_id);
                                if (!daraNew.Any())
                                    throw new Exception("Missing Report.");
                                else
                                    BuildDARAReport(daraNew, true);

                                break;

                            case (int)OIReports.rpUniversityTotal:

                                var totalSum = Report.GetTotalSumReport(load_id);
                                var totalByValid = Report.GetTotalSumReportByValid(load_id);
                                if (!totalSum.Any() || !totalByValid.Any())
                                    throw new Exception("Missing Report.");
                                else
                                    BuildTotalUniverseReport(totalSum, totalByValid, null, load_id);

                                break;

                            case (int)OIReports.rpCOTotal:

                                var ds = Report.GetCOTotalReport(load_id);
                                if (ds == null)
                                    throw new Exception("Missing Report.");
                                else
                                    BuildGeneralExcelOutput(ds.Tables[0]);
                                break;

                            case (int)OIReports.rpValidationByLine:

                                var dsValidationReport = Report.GetValidationByLineReport(load_id);
                                if (dsValidationReport == null)
                                    throw new Exception("Missing Report.");
                                else
                                    BuildGeneralExcelOutput(dsValidationReport.Tables[0]);

                                break;


                            case (int)OIReports.rpDocuments:

                                var dsDocs = Report.GetDocumentsReport(load_id);
                                if (dsDocs == null)
                                    throw new Exception("Missing Report.");
                                else
                                    BuildGeneralExcelOutput(dsDocs.Tables[0]);

                                break;
                        }


                        FinalizeOutput();
                    }
                    else if (Request.QueryString["id"] != null && Request.QueryString["id"] != "")
                    {

                        var report_id = Int32.Parse(Request.QueryString["id"]);

                        switch (report_id)
                        {
                            case (int)OIReports.rpRegSearchExcel:
                                BuildGeneralExcelOutput(copyDataView((DataView)Session["Session:ItemsDataView:"]));
                                FinalizeOutput();
                                break;

                            case (int)OIReports.rpValHistSearchExcel:
                                BuildValHistorySearchReport(((DataView)Session["Session:ItemsDataView:"]).Table);
                                FinalizeOutput();
                                break;

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                lblMsg.Text = ex.Message;
            }
        }

        private DataRow GetLoadInfo(int LoadID)
        {
            var dsLoad = LookupsBO.GetLoadList();
            return dsLoad.Tables[0].Select("LoadID = " + LoadID.ToString())[0];
        }

        private void CompleteExcelRow(ref HtmlTableRow tRow, int MaxCells)
        {
            for (var i = tRow.Cells.Count; i < MaxCells; i++)
            {
                tRow.Cells.AddCell("", "");
            }
        }

        private void BuildDARAReportForSample(DataTable dtTotal, DataTable dtSent, int LoadID)
        {
            HtmlTableRow objRow;
            var max_columns = 5;
            var excelRow = 1;

            //title row
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("DARA", "label");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;

            //add empty rows:
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;

            //add table titles row:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Total For Review", "titleBoldNavy");
            objRow.Cells.AddCell("Sent to Central Office", "titleBoldNavy");
            objRow.Cells.AddCell("Pending", "titleBoldNavy");
            Sheet.Rows.Add(objRow);
            excelRow++;

            //create table according data defined in the include/ReportDARA.xml:
            var doc = new XmlDocument();
            doc.Load(Request.PhysicalApplicationPath + "OpenItems\\include\\ReportDARA.xml");

            var total_rows = new int[doc.DocumentElement.ChildNodes.Count];
            var i = 0;

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                int start_count_row;
                int finish_count_row;

                //add empty rows:
                objRow = new HtmlTableRow();
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
                excelRow++;

                //add row for responsible person:
                objRow = new HtmlTableRow();
                objRow.Cells.AddCell("", "");
                objRow.Cells.AddCell(node.Attributes.GetNamedItem("name").Value, "titleBoldNavy");
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
                excelRow++;

                start_count_row = excelRow;

                foreach (XmlNode child_node in node.ChildNodes)
                {
                    int total_count;
                    int sent_count;
                    int pending;
                    DataRow[] dr;
                    var org_symbol = child_node.Attributes.GetNamedItem("symbol").Value;

                    objRow = new HtmlTableRow();
                    objRow.Cells.AddCell(org_symbol, "textLeft");
                    objRow.Cells.AddCell(child_node.Attributes.GetNamedItem("division").Value, "textLeft");

                    //add total doc numbers:
                    dr = dtTotal.Select("Organization = '" + org_symbol + "'");
                    if (dr.Length > 0)
                        total_count = (int)dr[0]["DocNumbersCount"];
                    else
                        total_count = 0;
                    objRow.Cells.AddCell(total_count.ToString(), "WholeNumber");

                    //add sent doc numbers:
                    dr = dtSent.Select("Organization = '" + org_symbol + "'");
                    if (dr.Length > 0)
                        sent_count = (int)dr[0]["DocNumSentCount"];
                    else
                        sent_count = 0;
                    objRow.Cells.AddCell(sent_count.ToString(), "WholeNumber");

                    //add pending doc numbers amount:    
                    pending = total_count - sent_count;
                    objRow.Cells.AddCell(pending.ToString(), "WholeNumber");

                    Sheet.Rows.Add(objRow);
                    excelRow++;
                }

                finish_count_row = excelRow - 1;

                total_rows[i] = excelRow;
                i++;

                //add total row per responsible person:
                objRow = new HtmlTableRow();
                objRow.Cells.AddCell("", "");
                objRow.Cells.AddCell("Total", "textBoldBlack");
                if (finish_count_row >= start_count_row)
                {
                    objRow.Cells.AddCell(String.Format("=SUM(C{0}:C{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(D{0}:D{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(E{0}:E{1})", start_count_row, finish_count_row), "WholeNumberBold");
                }
                else
                {
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                }
                Sheet.Rows.Add(objRow);
                excelRow++;
                //add empty rows:
                objRow = new HtmlTableRow();
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
                excelRow++;
            }

            //add empty rows:
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;

            //add final total records value:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Total Count", "titleBoldNavy");
            objRow.Cells.AddCell(MakeSumSentence("C", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("D", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("E", total_rows), "WholeNumberBold");
            Sheet.Rows.Add(objRow);
        }


        private void BuildDARAReport(IEnumerable<spReportTotalByOrg_Result> reportTotal, bool DARAByDoc)
        {
            string s_value;
            HtmlTableRow objRow;
            var max_columns = 12;
            var excelRow = 1;

            //title row
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            if (DARAByDoc)
            {
                objRow.Cells.AddCell("DARA by DocNum", "label");
            }
            else
            {
                objRow.Cells.AddCell("DARA", "label");
            }

            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;

            //add empty rows:
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;

            //add table titles row:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Invalid", "titleBoldNavy");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Valid", "titleBoldNavy");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Total Reviewed", "titleBoldNavy");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Pending", "titleBoldNavy");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Total", "titleBoldNavy");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Invalid # Records", "textBoldBlack");
            objRow.Cells.AddCell("Invalid $ Value", "textBoldBlack");
            objRow.Cells.AddCell("Valid # Records", "textBoldBlack");
            objRow.Cells.AddCell("Valid $ Value", "textBoldBlack");
            objRow.Cells.AddCell("Total Reviewed # Records", "titleBoldNavy");
            objRow.Cells.AddCell("Total Reviewed $ Value", "titleBoldNavy");
            objRow.Cells.AddCell("Pending # Records", "textBoldBlack");
            objRow.Cells.AddCell("Pending $ Value", "textBoldBlack");
            objRow.Cells.AddCell("Total # Records", "titleBoldNavy");
            objRow.Cells.AddCell("Total $ Value", "titleBoldNavy");
            Sheet.Rows.Add(objRow);
            excelRow++;

            //create table according data defined in the include/ReportDARA.xml:
            var doc = new XmlDocument();
            if (DARAByDoc)
            {
                doc.Load(Request.PhysicalApplicationPath + "OpenItems\\include\\ReportDARAbyDocNum.xml");
            }
            else
            {
                doc.Load(Request.PhysicalApplicationPath + "OpenItems\\include\\ReportDARA.xml");
            }


            var total_rows = new int[doc.DocumentElement.ChildNodes.Count];
            var i = 0;

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                int start_count_row;
                int finish_count_row;

                //add empty rows:
                objRow = new HtmlTableRow();
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
                excelRow++;

                //add row for responsible person:
                objRow = new HtmlTableRow();
                objRow.Cells.AddCell("", "");
                objRow.Cells.AddCell(node.Attributes.GetNamedItem("name").Value, "titleBoldNavy");
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
                excelRow++;

                start_count_row = excelRow;

                foreach (XmlNode child_node in node.ChildNodes)
                {
                    string records_count;
                    string money_value;
                    var org_symbol = child_node.Attributes.GetNamedItem("symbol").Value;

                    objRow = new HtmlTableRow();
                    objRow.Cells.AddCell(org_symbol, "textLeft");
                    objRow.Cells.AddCell(child_node.Attributes.GetNamedItem("division").Value, "textLeft");

                    //add invalid values:
                    
                    var report = reportTotal.Where(r => r.Organization == org_symbol && r.Valid == (int)OpenItemValidation.vlInvalid);
                    if (report.Any())
                    {
                        records_count = report.First().RecordsCount.ToString();
                        money_value = report.First().TotalSum.ToString();
                    }
                    else
                    {
                        records_count = "0";
                        money_value = "0";
                    }
                    objRow.Cells.AddCell(records_count, "WholeNumber");
                    objRow.Cells.AddCell(money_value, "Money");

                    //add valid values:
                    s_value = String.Format("Organization = '{0}' AND Valid = {1}", org_symbol, (int)OpenItemValidation.vlValid);
                    report = reportTotal.Where(r => r.Organization == org_symbol && r.Valid == (int)OpenItemValidation.vlValid);
                    if (report.Any())
                    {
                        records_count = report.First().RecordsCount.ToString();
                        money_value = report.First().TotalSum.ToString();
                    }
                    else
                    {
                        records_count = "0";
                        money_value = "0";
                    }
                    objRow.Cells.AddCell(records_count, "WholeNumber");
                    objRow.Cells.AddCell(money_value, "Money");

                    //add total reviewed records:
                    s_value = String.Format("=SUM(C{0},E{0})", excelRow);
                    objRow.Cells.AddCell(s_value, "WholeNumber");
                    //add total reviewed value:
                    s_value = String.Format("=SUM(D{0},F{0})", excelRow);
                    objRow.Cells.AddCell(s_value, "Money");

                    //add pending values:
                    report = reportTotal.Where(r => r.Organization == org_symbol && r.Valid == (int)OpenItemValidation.vlValidationNotAssigned);
                    if (report.Any())
                    {
                        records_count = report.First().RecordsCount.ToString();
                        money_value = report.First().TotalSum.ToString();
                    }
                    else
                    {
                        records_count = "0";
                        money_value = "0";
                    }
                    objRow.Cells.AddCell(records_count, "WholeNumber");
                    objRow.Cells.AddCell(money_value, "Money");

                    //add total records:
                    s_value = String.Format("=SUM(G{0},I{0})", excelRow);
                    objRow.Cells.AddCell(s_value, "WholeNumber");
                    //add total value:
                    s_value = String.Format("=SUM(H{0},J{0})", excelRow);
                    objRow.Cells.AddCell(s_value, "Money");

                    Sheet.Rows.Add(objRow);
                    excelRow++;
                }

                finish_count_row = excelRow - 1;

                total_rows[i] = excelRow;
                i++;

                //add total row per responsible person:
                objRow = new HtmlTableRow();
                objRow.Cells.AddCell("", "");
                objRow.Cells.AddCell("Total", "textBoldBlack");
                if (finish_count_row >= start_count_row)
                {
                    objRow.Cells.AddCell(String.Format("=SUM(C{0}:C{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(D{0}:D{1})", start_count_row, finish_count_row), "MoneyBold");
                    objRow.Cells.AddCell(String.Format("=SUM(E{0}:E{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(F{0}:F{1})", start_count_row, finish_count_row), "MoneyBold");
                    objRow.Cells.AddCell(String.Format("=SUM(G{0}:G{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(H{0}:H{1})", start_count_row, finish_count_row), "MoneyBold");
                    objRow.Cells.AddCell(String.Format("=SUM(I{0}:I{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(J{0}:J{1})", start_count_row, finish_count_row), "MoneyBold");
                    objRow.Cells.AddCell(String.Format("=SUM(K{0}:K{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(L{0}:L{1})", start_count_row, finish_count_row), "MoneyBold");
                }
                else
                {
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "MoneyBold");
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "MoneyBold");
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "MoneyBold");
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "MoneyBold");
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "MoneyBold");
                }
                Sheet.Rows.Add(objRow);
                excelRow++;
                //add empty rows:
                objRow = new HtmlTableRow();
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
                excelRow++;
            }

            //add empty rows:
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;

            //add final total records value:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Total Records", "titleBoldNavy");
            objRow.Cells.AddCell(MakeSumSentence("C", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("D", total_rows), "MoneyBold");
            objRow.Cells.AddCell(MakeSumSentence("E", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("F", total_rows), "MoneyBold");
            objRow.Cells.AddCell(MakeSumSentence("G", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("H", total_rows), "MoneyBold");
            objRow.Cells.AddCell(MakeSumSentence("I", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("J", total_rows), "MoneyBold");
            objRow.Cells.AddCell(MakeSumSentence("K", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("L", total_rows), "MoneyBold");
            Sheet.Rows.Add(objRow);
        }

        private void BuildDARAReport(IEnumerable<spDaraByDocNum_Result> reportTotal, bool DARAByDoc)
        {
            string s_value;
            HtmlTableRow objRow;
            var max_columns = 12;
            var excelRow = 1;

            //title row
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            if (DARAByDoc)
            {
                objRow.Cells.AddCell("DARA by DocNum", "label");
            }
            else
            {
                objRow.Cells.AddCell("DARA", "label");
            }

            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;

            //add empty rows:
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;

            //add table titles row:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Invalid", "titleBoldNavy");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Valid", "titleBoldNavy");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Total Reviewed", "titleBoldNavy");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Pending", "titleBoldNavy");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Total", "titleBoldNavy");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Invalid # Records", "textBoldBlack");
            objRow.Cells.AddCell("Invalid $ Value", "textBoldBlack");
            objRow.Cells.AddCell("Valid # Records", "textBoldBlack");
            objRow.Cells.AddCell("Valid $ Value", "textBoldBlack");
            objRow.Cells.AddCell("Total Reviewed # Records", "titleBoldNavy");
            objRow.Cells.AddCell("Total Reviewed $ Value", "titleBoldNavy");
            objRow.Cells.AddCell("Pending # Records", "textBoldBlack");
            objRow.Cells.AddCell("Pending $ Value", "textBoldBlack");
            objRow.Cells.AddCell("Total # Records", "titleBoldNavy");
            objRow.Cells.AddCell("Total $ Value", "titleBoldNavy");
            Sheet.Rows.Add(objRow);
            excelRow++;

            //create table according data defined in the include/ReportDARA.xml:
            var doc = new XmlDocument();
            if (DARAByDoc)
            {
                doc.Load(Request.PhysicalApplicationPath + "OpenItems\\include\\ReportDARAbyDocNum.xml");
            }
            else
            {
                doc.Load(Request.PhysicalApplicationPath + "OpenItems\\include\\ReportDARA.xml");
            }


            var total_rows = new int[doc.DocumentElement.ChildNodes.Count];
            var i = 0;

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                int start_count_row;
                int finish_count_row;

                //add empty rows:
                objRow = new HtmlTableRow();
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
                excelRow++;

                //add row for responsible person:
                objRow = new HtmlTableRow();
                objRow.Cells.AddCell("", "");
                objRow.Cells.AddCell(node.Attributes.GetNamedItem("name").Value, "titleBoldNavy");
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
                excelRow++;

                start_count_row = excelRow;

                foreach (XmlNode child_node in node.ChildNodes)
                {
                    string records_count;
                    string money_value;
                    var org_symbol = child_node.Attributes.GetNamedItem("symbol").Value;

                    objRow = new HtmlTableRow();
                    objRow.Cells.AddCell(org_symbol, "textLeft");
                    objRow.Cells.AddCell(child_node.Attributes.GetNamedItem("division").Value, "textLeft");

                    //add invalid values:

                    var report = reportTotal.Where(r => r.Organization == org_symbol && r.Valid == (int)OpenItemValidation.vlInvalid);
                    if (report.Any())
                    {
                        records_count = report.First().RecordsCount.ToString();
                        money_value = report.First().TotalSum.ToString();
                    }
                    else
                    {
                        records_count = "0";
                        money_value = "0";
                    }
                    objRow.Cells.AddCell(records_count, "WholeNumber");
                    objRow.Cells.AddCell(money_value, "Money");

                    //add valid values:
                    s_value = String.Format("Organization = '{0}' AND Valid = {1}", org_symbol, (int)OpenItemValidation.vlValid);
                    report = reportTotal.Where(r => r.Organization == org_symbol && r.Valid == (int)OpenItemValidation.vlValid);
                    if (report.Any())
                    {
                        records_count = report.First().RecordsCount.ToString();
                        money_value = report.First().TotalSum.ToString();
                    }
                    else
                    {
                        records_count = "0";
                        money_value = "0";
                    }
                    objRow.Cells.AddCell(records_count, "WholeNumber");
                    objRow.Cells.AddCell(money_value, "Money");

                    //add total reviewed records:
                    s_value = String.Format("=SUM(C{0},E{0})", excelRow);
                    objRow.Cells.AddCell(s_value, "WholeNumber");
                    //add total reviewed value:
                    s_value = String.Format("=SUM(D{0},F{0})", excelRow);
                    objRow.Cells.AddCell(s_value, "Money");

                    //add pending values:
                    report = reportTotal.Where(r => r.Organization == org_symbol && r.Valid == (int)OpenItemValidation.vlValidationNotAssigned);
                    if (report.Any())
                    {
                        records_count = report.First().RecordsCount.ToString();
                        money_value = report.First().TotalSum.ToString();
                    }
                    else
                    {
                        records_count = "0";
                        money_value = "0";
                    }
                    objRow.Cells.AddCell(records_count, "WholeNumber");
                    objRow.Cells.AddCell(money_value, "Money");

                    //add total records:
                    s_value = String.Format("=SUM(G{0},I{0})", excelRow);
                    objRow.Cells.AddCell(s_value, "WholeNumber");
                    //add total value:
                    s_value = String.Format("=SUM(H{0},J{0})", excelRow);
                    objRow.Cells.AddCell(s_value, "Money");

                    Sheet.Rows.Add(objRow);
                    excelRow++;
                }

                finish_count_row = excelRow - 1;

                total_rows[i] = excelRow;
                i++;

                //add total row per responsible person:
                objRow = new HtmlTableRow();
                objRow.Cells.AddCell("", "");
                objRow.Cells.AddCell("Total", "textBoldBlack");
                if (finish_count_row >= start_count_row)
                {
                    objRow.Cells.AddCell(String.Format("=SUM(C{0}:C{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(D{0}:D{1})", start_count_row, finish_count_row), "MoneyBold");
                    objRow.Cells.AddCell(String.Format("=SUM(E{0}:E{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(F{0}:F{1})", start_count_row, finish_count_row), "MoneyBold");
                    objRow.Cells.AddCell(String.Format("=SUM(G{0}:G{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(H{0}:H{1})", start_count_row, finish_count_row), "MoneyBold");
                    objRow.Cells.AddCell(String.Format("=SUM(I{0}:I{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(J{0}:J{1})", start_count_row, finish_count_row), "MoneyBold");
                    objRow.Cells.AddCell(String.Format("=SUM(K{0}:K{1})", start_count_row, finish_count_row), "WholeNumberBold");
                    objRow.Cells.AddCell(String.Format("=SUM(L{0}:L{1})", start_count_row, finish_count_row), "MoneyBold");
                }
                else
                {
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "MoneyBold");
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "MoneyBold");
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "MoneyBold");
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "MoneyBold");
                    objRow.Cells.AddCell("0", "WholeNumberBold");
                    objRow.Cells.AddCell("0", "MoneyBold");
                }
                Sheet.Rows.Add(objRow);
                excelRow++;
                //add empty rows:
                objRow = new HtmlTableRow();
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
                excelRow++;
            }

            //add empty rows:
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;

            //add final total records value:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Total Records", "titleBoldNavy");
            objRow.Cells.AddCell(MakeSumSentence("C", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("D", total_rows), "MoneyBold");
            objRow.Cells.AddCell(MakeSumSentence("E", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("F", total_rows), "MoneyBold");
            objRow.Cells.AddCell(MakeSumSentence("G", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("H", total_rows), "MoneyBold");
            objRow.Cells.AddCell(MakeSumSentence("I", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("J", total_rows), "MoneyBold");
            objRow.Cells.AddCell(MakeSumSentence("K", total_rows), "WholeNumberBold");
            objRow.Cells.AddCell(MakeSumSentence("L", total_rows), "MoneyBold");
            Sheet.Rows.Add(objRow);
        }

        private string MakeSumSentence(string Letter, int[] total_arr)
        {
            var value = "=SUM(";
            foreach (var n in total_arr)
                value = value + Letter + n.ToString() + ",";
            value = value.Substring(0, value.Length - 1) + ")";
            return value;
        }

        private void BuildTotalUniverseReport(List<spReportTotalSum_Result> reportTotal, List<spReportTotalByValid_Result> reportTotalByValid, DataTable dtDeobligated, int LoadID)
        {
            var display_deobligated = (dtDeobligated == null) ? false : true;
            var s_msg = "";
            string records;
            string total_value;
            var max_columns = 5;
            HtmlTableRow objRow;

            //title row
            objRow = new HtmlTableRow();
            s_msg = "SUMMARY OF NCR's as of " + DateTime.Now.ToString("MMM dd, yyyy");
            objRow.Cells.AddCell(s_msg, "titleBoldNavy");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);

            //add load info:
            var dr = GetLoadInfo(LoadID);
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Type:", "textBoldBlack");
            objRow.Cells.AddCell((string)Utility.GetNotNullValue(dr["OITypeDescription"], "String"), "textLeft");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Source:", "textBoldBlack");
            objRow.Cells.AddCell(dr["DataSourceDescription"].ToString(), "textLeft");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("ULO Load Date:", "textBoldBlack");
            objRow.Cells.AddCell(((DateTime)dr["LoadDate"]).ToString("MMM dd, yyyy"), "textLeft");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);

            //add empty rows:
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);

            //total records:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Total Records", "textBoldBlack");
            objRow.Cells.AddCell(reportTotal.First().TotalCount.ToString(), "WholeNumberBold");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Total Dollar Amt $", "textBoldBlack");
            objRow.Cells.AddCell(reportTotal.First().TotalSum.ToString(), "MoneyBold");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);

            //add empty rows:
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);

            //table by valid values:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Status", "textBoldBlack");
            objRow.Cells.AddCell("Records", "textBoldBlack");
            objRow.Cells.AddCell("Dollar Value", "textBoldBlack");
            objRow.Cells.AddCell("% Dollar Value Researched", "textBoldBlack");
            objRow.Cells.AddCell("% Total Records", "textBoldBlack");
            Sheet.Rows.Add(objRow);

            //for Valid row:
            var reportValidRow = reportTotalByValid.Where(r => r.Valid == (int)OpenItemValidation.vlValid);
            if (!reportValidRow.Any())
            {
                records = "0";
                total_value = "0";
            }
            else
            {
                records = reportValidRow.First().TotalCount.ToString();
                total_value = reportValidRow.First().TotalSum.ToString();
            }
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Valid", "textLeft");
            objRow.Cells.AddCell(records, "WholeNumber");
            objRow.Cells.AddCell(total_value, "Money");
            objRow.Cells.AddCell("=C11/B7", "Percent");
            objRow.Cells.AddCell("=B11/B6", "Percent");
            Sheet.Rows.Add(objRow);

            //for Invalid row:
            reportValidRow = reportTotalByValid.Where(r => r.Valid == (int)OpenItemValidation.vlInvalid);
            if (!reportValidRow.Any())
            {
                records = "0";
                total_value = "0";
            }
            else
            {
                records = reportValidRow.First().TotalCount.ToString();
                total_value = reportValidRow.First().TotalSum.ToString();
            }
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Invalid", "textLeft");
            objRow.Cells.AddCell(records, "WholeNumber");
            objRow.Cells.AddCell(total_value, "Money");
            objRow.Cells.AddCell("=C12/B7", "Percent");
            objRow.Cells.AddCell("=B12/B6", "Percent");
            Sheet.Rows.Add(objRow);

            //for Total reviewed:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Total Reviewed", "textLeft");
            objRow.Cells.AddCell("=B11+B12", "WholeNumber");
            objRow.Cells.AddCell("=C11+C12", "Money");
            objRow.Cells.AddCell("=C13/B7", "Percent");
            objRow.Cells.AddCell("=B13/B6", "Percent");
            Sheet.Rows.Add(objRow);

            //for needed review:
            reportValidRow = reportTotalByValid.Where(r => r.Valid == (int)OpenItemValidation.vlValidationNotAssigned);
            if (!reportValidRow.Any())
            {
                records = "0";
                total_value = "0";
            }
            else
            {
                records = reportValidRow.First().TotalCount.ToString();
                total_value = reportValidRow.First().TotalSum.ToString();
            }
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Needing Review", "textLeft");
            objRow.Cells.AddCell(records, "WholeNumber");
            objRow.Cells.AddCell(total_value, "Money");
            objRow.Cells.AddCell("=C14/B7", "Percent");
            objRow.Cells.AddCell("=B14/B6", "Percent");
            Sheet.Rows.Add(objRow);

            //for Total summary:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Total", "textBoldBlack");
            objRow.Cells.AddCell("=SUM(B11,B12,B14)", "WholeNumberBold");
            objRow.Cells.AddCell("=SUM(C11,C12,C14)", "MoneyBold");
            objRow.Cells.AddCell("=SUM(D11,D12,D14)", "PercentBold");
            objRow.Cells.AddCell("=SUM(E11,E12,E14)", "PercentBold");
            Sheet.Rows.Add(objRow);

            //add empty rows:
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);

            if (display_deobligated)
            {
                //add rows for deobligated info:
                objRow = new HtmlTableRow();
                objRow.Cells.AddCell("Total Records Deobligated", "textLeft");
                objRow.Cells.AddCell(dtDeobligated.Rows[0]["RecordsCount"].ToString(), "WholeNumber");
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
                objRow = new HtmlTableRow();
                objRow.Cells.AddCell("Total Dollar Amt $", "textLeft");
                objRow.Cells.AddCell(dtDeobligated.Rows[0]["TotalSum"].ToString(), "Money");
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);

                //add empty rows:
                objRow = new HtmlTableRow();
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);

                //add rows for pending deobligation:
                objRow = new HtmlTableRow();
                objRow.Cells.AddCell("Records Pending Deobligation", "textLeft");
                objRow.Cells.AddCell("=B12-B18", "WholeNumber");
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
                objRow = new HtmlTableRow();
                objRow.Cells.AddCell("Total Dollar Amt $", "textLeft");
                objRow.Cells.AddCell("=C12-B19", "Money");
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);

                //add empty rows:
                objRow = new HtmlTableRow();
                CompleteExcelRow(ref objRow, max_columns);
                Sheet.Rows.Add(objRow);
            }

            //add rows where data will be inserted by user:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("As of ", "textBoldBlack");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Region Wide OI Records", "textLeft");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Total Dollar Amt $", "textLeft");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
        }

        private void BuildValHistorySearchReport(DataTable dtSearch)
        {
            if (Session["dtLoad"] == null)
            {
                Session["dtLoad"] = LookupsBO.GetLoadList();
            }

            var dtLoad = ((DataSet)Session["dtLoad"]).Tables[0];

            var titleRow = new HtmlTableRow();
            HtmlTableCell titleCell;


            for (var i = 0; i < 4; i++)
            {
                titleCell = CreateCell(dtSearch.Columns[i].Caption.ToString(), "header");
                titleCell.RowSpan = 2;
                titleCell.VAlign = "bottom";
                titleRow.Cells.Add(titleCell);
            }

            var noofLoads = (dtSearch.Columns.Count - 4) / 5;

            if (noofLoads > 0)
            {
                for (var k = 0; k < noofLoads; k++)
                {
                    var loadID = (dtSearch.Columns[4 + (k + 1) * 5 - 1].Caption.ToUpper()).TrimEnd("_REVIEWERNAME".ToCharArray());
                    var loadDesc = dtLoad.Select("LoadID = " + loadID.ToString())[0]["LoadDesc"].ToString();

                    if (k % 2 == 0)
                    {
                        titleCell = CreateCell(loadDesc, "reportHeaderBlue");
                    }
                    else
                    {
                        titleCell = CreateCell(loadDesc, "reportHeaderGreen");
                    }

                    titleCell.ColSpan = 5;

                    titleRow.Cells.Add(titleCell);

                }
                CompleteExcelRow(ref titleRow, 1);
                Sheet.Rows.Add(titleRow);

                titleRow = new HtmlTableRow();
                for (var j = 4; j < 4 + noofLoads * 5; j++)
                {
                    titleCell = CreateCell((dtSearch.Columns[j].Caption).Substring(2), "header");
                    titleCell.RowSpan = 1;
                    titleRow.Cells.Add(titleCell);
                }
                CompleteExcelRow(ref titleRow, 1);
                Sheet.Rows.Add(titleRow);
            }

            HtmlTableRow objRow;
            for (var p = 0; p < dtSearch.Rows.Count; p++)
            {
                objRow = new HtmlTableRow();
                for (var q = 0; q < dtSearch.Columns.Count; q++)
                {
                    objRow.Cells.AddCell(dtSearch.Rows[p][q].ToString(), "textLeft");
                }
                CompleteExcelRow(ref objRow, 1);
                Sheet.Rows.Add(objRow);
            }
            //if (noofLoads > 0)
            //{
            //    titleCell = CreateCell(dtSearch.Columns[i].Caption.ToString(), "titleBoldNavy");
            //    titleCell.RowSpan = 5;
            //    titleRow.Cells.Add(titleCell);  

            //    for (int j = 4; j < 4 + noofLoads * 5; j++)
            //    { 

            //    }
            //}


            //  CompleteExcelRow(ref titleRow, 1);
            //  Sheet.Rows.Add(titleRow);

            //HtmlTableCell titleCell = CreateCell("DocNumber", "titleBoldNavy");
            //titleCell.RowSpan = 2;
            //titleCell.Align = "center";
            //titleCell.BgColor = "";
            //titleRow.Cells.Add(titleCell);
            //CompleteExcelRow(ref titleRow, 1);
            //Sheet.Rows.Add(titleRow);
        }
        private void BuildDailyReport(List<spReportDaily_Result> reportDaily, DataTable dtDeobligated, int LoadID)
        {
            var display_deobligated = (dtDeobligated == null) ? false : true;
            var s_msg = "";
            var excelRow = 1;
            var max_columns = (display_deobligated) ? 12 : 10;
            HtmlTableRow objRow;

            //title row
            objRow = new HtmlTableRow();
            s_msg = "Daily Report (Total) as of " + DateTime.Now.ToString("MMM dd, yyyy");
            objRow.Cells.AddCell(s_msg, "titleBoldNavy");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);

            excelRow++;

            //add load info:
            var dr = GetLoadInfo(LoadID);
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Type:", "textBoldBlack");
            objRow.Cells.AddCell((string)Utility.GetNotNullValue(dr["OITypeDescription"], "String"), "textLeft");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Source:", "textBoldBlack");
            objRow.Cells.AddCell(dr["DataSourceDescription"].ToString(), "textLeft");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("ULO Load Date:", "textBoldBlack");
            objRow.Cells.AddCell(((DateTime)dr["LoadDate"]).ToString("MMM dd, yyyy"), "textLeft");
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;

            //add empty rows:
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;
            objRow = new HtmlTableRow();
            CompleteExcelRow(ref objRow, max_columns);
            Sheet.Rows.Add(objRow);
            excelRow++;

            //data table:
            //first create headers:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("Off Code", "textBoldBlack");
            objRow.Cells.AddCell("Off. Symb.", "textBoldBlack");
            objRow.Cells.AddCell("Division", "textBoldBlack");
            objRow.Cells.AddCell("Invalid", "textBoldBlack");
            if (display_deobligated)
                objRow.Cells.AddCell("Deobligated (included in Invalid)", "textBoldBlack");
            objRow.Cells.AddCell("Invalid $ Value", "textBoldBlack");
            if (display_deobligated)
                objRow.Cells.AddCell("Deobligated$ Value (included in Invalid)", "textBoldBlack");
            objRow.Cells.AddCell("Valid", "textBoldBlack");
            objRow.Cells.AddCell("Valid $ Value", "textBoldBlack");
            objRow.Cells.AddCell("Pending", "textBoldBlack");
            objRow.Cells.AddCell("Pending $ Value", "textBoldBlack");
            objRow.Cells.AddCell("Records", "textBoldBlack");
            Sheet.Rows.Add(objRow);
            excelRow++;

            //then fill the data:
            string sOrg;
            string sOrgSymbol;
            string sDivision;
            int valid;
            int count_valid;
            decimal total_valid;
            int count_invalid;
            decimal total_invalid;
            int count_pend;
            decimal total_pend;
            int count_deobl;
            decimal total_deobl;
            DataRow[] dr_deobl;

            var table_row_start = excelRow;

            for (var i = 0; i < reportDaily.Count;)
            {
                sOrg = reportDaily[i].OrgCode;
                sOrgSymbol = reportDaily[i].Organization;
                sDivision = reportDaily[i].Division;

                valid = reportDaily[i].Valid;
                if (valid == (int)OpenItemValidation.vlValidationNotAssigned) //0
                {
                    //the row data exists, fill values and move to the next row
                    count_pend = reportDaily[i].RecordsCount.GetValueOrDefault();
                    total_pend = reportDaily[i].TotalSum.GetValueOrDefault();
                    i++;
                    if (i < reportDaily.Count && sOrg == reportDaily[i].OrgCode)
                        valid = reportDaily[i].Valid;
                }
                else
                {
                    //there is no data row, create record with default values
                    count_pend = 0;
                    total_pend = 0;
                }

                if (valid == (int)OpenItemValidation.vlInvalid) //1
                {
                    //the row data exists, fill values and move to the next row
                    count_invalid = reportDaily[i].RecordsCount.GetValueOrDefault();
                    total_invalid = reportDaily[i].TotalSum.GetValueOrDefault();
                    i++;
                    if (i < reportDaily.Count && sOrg == reportDaily[i].OrgCode)
                        valid = reportDaily[i].Valid;
                }
                else
                {
                    //there is no data row, create record with default values
                    count_invalid = 0;
                    total_invalid = 0;
                }

                if (valid == (int)OpenItemValidation.vlValid)//2
                {
                    //the row data exists, fill values and move to the next row
                    count_valid = reportDaily[i].RecordsCount.GetValueOrDefault();
                    total_valid = reportDaily[i].TotalSum.GetValueOrDefault();
                    i++;

                }
                else
                {
                    //there is no data row, create record with default values
                    count_valid = 0;
                    total_valid = 0;
                }
                //find deobligated values:
                if (display_deobligated)
                {
                    dr_deobl = dtDeobligated.Select("OrgCode = '" + sOrg + "'");
                    if (dr_deobl != null && dr_deobl.Length > 0)
                    {
                        count_deobl = (int)Utility.GetNotNullValue(dr_deobl[0]["RecordsCount"], "Int");
                        total_deobl = (decimal)Utility.GetNotNullValue(dr_deobl[0]["TotalSum"], "Decimal");
                    }
                    else
                    {
                        count_deobl = 0;
                        total_deobl = 0;
                    }
                }
                else
                {
                    count_deobl = 0;
                    total_deobl = 0;
                }

                objRow = new HtmlTableRow();
                objRow.Cells.AddCell(sOrg, "textLeft");
                objRow.Cells.AddCell(sOrgSymbol, "textLeft");
                objRow.Cells.AddCell(sDivision, "textLeft");

                objRow.Cells.AddCell(count_invalid.ToString(), "WholeNumber");
                if (display_deobligated)
                    objRow.Cells.AddCell(count_deobl.ToString(), "WholeNumber");
                objRow.Cells.AddCell(total_invalid.ToString(), "Money");
                if (display_deobligated)
                    objRow.Cells.AddCell(total_deobl.ToString(), "Money");
                objRow.Cells.AddCell(count_valid.ToString(), "WholeNumber");
                objRow.Cells.AddCell(total_valid.ToString(), "Money");
                objRow.Cells.AddCell(count_pend.ToString(), "WholeNumber");
                objRow.Cells.AddCell(total_pend.ToString(), "Money");

                if (display_deobligated)
                    s_msg = String.Format("=SUM(D{0},H{0},J{0})", excelRow);
                else
                    s_msg = String.Format("=SUM(D{0},F{0},H{0})", excelRow);
                objRow.Cells.AddCell(s_msg, "WholeNumber");
                Sheet.Rows.Add(objRow);
                excelRow++;
            }

            //add total row:
            objRow = new HtmlTableRow();
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("", "");
            objRow.Cells.AddCell("Grand Total", "textBoldBlack");
            if (display_deobligated)
            {
                objRow.Cells.AddCell(String.Format("=SUM(D{0}:D{1})", table_row_start, excelRow - 1), "WholeNumberBold");
                objRow.Cells.AddCell(String.Format("=SUM(E{0}:E{1})", table_row_start, excelRow - 1), "WholeNumberBold");
                objRow.Cells.AddCell(String.Format("=SUM(F{0}:F{1})", table_row_start, excelRow - 1), "MoneyBold");
                objRow.Cells.AddCell(String.Format("=SUM(G{0}:G{1})", table_row_start, excelRow - 1), "MoneyBold");
                objRow.Cells.AddCell(String.Format("=SUM(H{0}:H{1})", table_row_start, excelRow - 1), "WholeNumberBold");
                objRow.Cells.AddCell(String.Format("=SUM(I{0}:I{1})", table_row_start, excelRow - 1), "MoneyBold");
                objRow.Cells.AddCell(String.Format("=SUM(J{0}:J{1})", table_row_start, excelRow - 1), "WholeNumberBold");
                objRow.Cells.AddCell(String.Format("=SUM(K{0}:K{1})", table_row_start, excelRow - 1), "MoneyBold");
                objRow.Cells.AddCell(String.Format("=SUM(L{0}:L{1})", table_row_start, excelRow - 1), "WholeNumberBold");
            }
            else
            {
                objRow.Cells.AddCell(String.Format("=SUM(D{0}:D{1})", table_row_start, excelRow - 1), "WholeNumberBold");
                objRow.Cells.AddCell(String.Format("=SUM(E{0}:E{1})", table_row_start, excelRow - 1), "MoneyBold");
                objRow.Cells.AddCell(String.Format("=SUM(F{0}:F{1})", table_row_start, excelRow - 1), "WholeNumberBold");
                objRow.Cells.AddCell(String.Format("=SUM(G{0}:G{1})", table_row_start, excelRow - 1), "MoneyBold");
                objRow.Cells.AddCell(String.Format("=SUM(H{0}:H{1})", table_row_start, excelRow - 1), "WholeNumberBold");
                objRow.Cells.AddCell(String.Format("=SUM(I{0}:I{1})", table_row_start, excelRow - 1), "MoneyBold");
                objRow.Cells.AddCell(String.Format("=SUM(J{0}:J{1})", table_row_start, excelRow - 1), "WholeNumberBold");
            }
            Sheet.Rows.Add(objRow);

        }

        private HtmlTableCell CreateCell(string InnerText, string CssClass)
        {
            var objCell = new HtmlTableCell();
            objCell.InnerText = InnerText;
            objCell.Attributes["class"] = CssClass;
            return objCell;
        }

        public void BuildGeneralExcelOutput(DataTable dt)
        {
            HtmlTableCell objCell;
            var objRow = new HtmlTableRow();            //add header first

            for (var i = 0; i < dt.Columns.Count; i++)
                objRow.Cells.AddCell(dt.Columns[i].ToString(), "titleBoldNavy");

            Sheet.Rows.Add(objRow);

            var ExcelLine = 1;
            foreach (DataRow row in dt.Rows)
            {
                objRow = new HtmlTableRow();

                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    objCell = new HtmlTableCell();
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
                            var colX = dt.Columns[i].ColumnName;
                            if (colX.ToUpper() == "BBFY")
                                objCell.Attributes["class"] = "Year";
                            else
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

        private DataTable copyDataView(DataView dvSource)
        {

            var dtNew = new DataTable();

            dtNew.Columns.Add(createColumn("LoadDate"));
            dtNew.Columns.Add(createColumn("DocNumber"));
            dtNew.Columns.Add(createColumn("Line"));
            dtNew.Columns.Add(createColumn("BA"));
            dtNew.Columns.Add(createColumn("ProjectNumber"));
            dtNew.Columns.Add(createColumn("AwardNumber"));
            dtNew.Columns.Add(createColumn("Contr.Officer by CO"));
            dtNew.Columns.Add(createColumn("Organization"));
            dtNew.Columns.Add(createColumn("Reviewer"));
            dtNew.Columns.Add(createColumn("Status"));
            dtNew.Columns.Add(createColumn("Valid Invalid"));

            for (var i = 0; i < dvSource.Table.Rows.Count; i++)
            {
                var dr = dtNew.NewRow();
                dr["LoadDate"] = dvSource.Table.Rows[i]["LoadDate"].ToString();
                dr["DocNumber"] = dvSource.Table.Rows[i]["DocNumber"].ToString();
                dr["Line"] = dvSource.Table.Rows[i]["ItemLNum"].ToString();
                dr["BA"] = dvSource.Table.Rows[i]["BA"].ToString();
                dr["ProjectNumber"] = dvSource.Table.Rows[i]["ProjNum"].ToString();
                dr["AwardNumber"] = dvSource.Table.Rows[i]["AwardNumber"].ToString();
                dr["Contr.Officer by CO"] = dvSource.Table.Rows[i]["ContractingOfficer"].ToString();
                dr["Organization"] = dvSource.Table.Rows[i]["Organization"].ToString();
                dr["Reviewer"] = dvSource.Table.Rows[i]["ReviewerName"].ToString();
                dr["Status"] = dvSource.Table.Rows[i]["StatusDescription"].ToString();
                dr["Valid Invalid"] = dvSource.Table.Rows[i]["ValidValueDescription"].ToString();

                dtNew.Rows.Add(dr);
            }

            return dtNew;
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

    }
}