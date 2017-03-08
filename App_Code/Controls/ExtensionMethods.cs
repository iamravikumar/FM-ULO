using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI.HtmlControls;

/// <summary>
/// Summary description for ExtensionMethods
/// </summary>
public static class ExtensionMethods
{

    public static void AddDisplayedMoneyCell(this HtmlTableCellCollection Cells, decimal ValueToDisplay, string CSSClass)
    {
        var td = new HtmlTableCell();
        if (ValueToDisplay < 0)
            td.InnerText = String.Format("{0:($0,0)}", ValueToDisplay * (-1));
        else if (ValueToDisplay > 9)
            td.InnerText = String.Format("{0:$0,0}", ValueToDisplay);
        else
            td.InnerText = String.Format("{0:$0}", ValueToDisplay);
        td.Attributes.Add("class", CSSClass);
        Cells.Add(td);
    }

    public static void AddDisplayedMoneyCell(this HtmlTableCellCollection Cells, decimal ValueToDisplay, string CSSClass, string Align)
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
        Cells.Add(td);

    }

    public static void AddTextCell(this HtmlTableCellCollection Cells, string ValueToDisplay, string CSSClass)
    {
        var td = new HtmlTableCell();
        td.InnerText = ValueToDisplay;
        td.Attributes.Add("class", CSSClass);
        Cells.Add(td);
    }


    public static void AddHeaderCell(this HtmlTableCellCollection Cells, string HeaderText, string CSSClass, string Align = "")
    {
        var td = CreateHeaderCell(HeaderText, CSSClass);
        if (Align != "")
            td.Align = Align;
        Cells.Add(td);
    }

    public static void AddHeaderCellWithTooltip(this HtmlTableCellCollection Cells, string HeaderText, string CSSClass, string Align, bool BuildReportForExcel, string AddToolTipPopupBody)
    {
        var td = CreateHeaderCell(HeaderText, CSSClass);
        if (!BuildReportForExcel)
            td.Attributes.Add("onMouseOver", AddToolTipPopupBody);
    }

private static HtmlTableCell CreateHeaderCell(string HeaderText, string CSSClass)
    {

        var td = new HtmlTableCell();
        td.InnerText = HeaderText;
        td.Attributes.Add("class", CSSClass);
        return td;
    }


}