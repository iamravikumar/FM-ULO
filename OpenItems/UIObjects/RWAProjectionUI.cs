namespace GSA.OpenItems.Web
{
    using System;
    using System.Data;
    using System.Web.UI.HtmlControls;


    public class RWAProjectionUI
    {

        public string FiscalYear { get; set; }

        public string BookMonth { get; set; }

        public string Organization { get; set; }

        public string BusinessLine { get; set; }

        public bool ReadOnly { get; set; }

        public DataTable IncomeTable { get; set; }

        public DataTable ProjectionTable { get; set; }

        public HtmlTable ChartTable { get; set; }

        private HtmlTableCell CreateCell(string InnerText, string CssClass)
        {
            var HCell = new HtmlTableCell();
            HCell.InnerText = InnerText;
            if (CssClass != "")
                HCell.Attributes["class"] = CssClass;
            return HCell;
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
            td.AddCssClass(CSSClass);
            if (Align != "")
                td.Align = Align;
            return td;
        }

        public void DrawDistributionTable()
        {
            HtmlTableRow tr;
            HtmlTableCell td;
            HtmlInputText txt;

            var ds = FSDataServices.GetFSReportConfiguration();
            int group_cd;
            string row_name;
            foreach (DataRow drConfig in ds.Tables[0].Rows)
            {
                //skip headers and totals rows
                if ((int)drConfig["GROUP_CD"] != 0)
                {
                    group_cd = (int)drConfig["GROUP_CD"];
                    row_name = (string)drConfig["Name"];

                    tr = new HtmlTableRow();
                    tr.AddReportRow();
                    tr.Cells.AddCell(row_name, "reportCaption");

                    var dr = IncomeTable.Select(String.Format("GROUP_CD={0} and BookMonth='{1}'", group_cd, BookMonth));
                    if (dr.Length > 0)
                        td = DisplayedMoneyCell((decimal)Utility.GetNotNullValue(dr[0]["Income"], "Decimal"), "reportTotal", "right");
                    else
                        td = DisplayedMoneyCell(0, "reportTotal", "right");
                    td.Attributes.Add("alt", group_cd.ToString());
                    tr.Cells.Add(td);

                    td = new HtmlTableCell();
                    txt = new HtmlInputText();
                    td.Controls.Add(txt);
                    txt.AddStyle("width:90px;height:15px;text-align:right");
                    if (!ReadOnly)
                    {
                        txt.AddOnMouseOut("extractNumber(this,0,false,0);");
                        txt.AddOnKeyUp("extractNumber(this,0,false,0);");
                        txt.AddKeyPressBlockNonNumbers();
                    }
                    else
                        txt.SetReadOnly();
                    dr = ProjectionTable.Select(String.Format("GROUP_CD={0} and BookMonth='{1}'", group_cd, BookMonth));
                    if (dr.Length > 0)
                        txt.Value = String.Format("{0:0}", (decimal)Utility.GetNotNullValue(dr[0]["Amount"], "Decimal"));
                    tr.Cells.Add(td);

                    tr.Cells.AddDisplayedMoneyCell(0, "reportTotal", "right");

                    ChartTable.Rows.Add(tr);
                }
            }
        }

    }

}