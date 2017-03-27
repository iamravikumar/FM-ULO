using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

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
        td.AddCssClass(CSSClass);
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
        td.AddCssClass(CSSClass);
        if (Align != "")
            td.Align = Align;
        Cells.Add(td);

    }

    public static void AddNewMoneyCell(this HtmlTableCellCollection Cells, decimal Amount, string CssClass, string Align = "right")
    {
        var td = new HtmlTableCell();
        if (Amount >= 0)
            td.InnerText = String.Format("{0:$0,0.00}", Amount);
        else
            td.InnerText = String.Format("{0:($0,0.00)}", Decimal.Negate(Amount));

        td.Align = Align;
        td.AddCssClass(CssClass);
        Cells.Add(td);
    }

    public static void AddMoneyCellLink(this HtmlTableCellCollection Cells, decimal Amount, string BookMonth, string Func, string OC, string CellLinkOnClick, string Align = "right")
    {
        var td = new HtmlTableCell();
        if (Amount >= 0)
            td.InnerText = String.Format("{0:$0,0.00}", Amount);
        else
            td.InnerText = String.Format("{0:($0,0.00)}", Decimal.Negate(Amount));

        td.Align = Align;
        td.AddCssClass("tableLink");
        td.AddOnClick("get_link('" + String.Format(CellLinkOnClick, BookMonth, Func, OC) + "');");

        Cells.Add(td);
    }


    public static void AddTextCell(this HtmlTableCellCollection Cells, string ValueToDisplay, string CSSClass)
    {
        var td = new HtmlTableCell();
        td.InnerText = ValueToDisplay;
        td.AddCssClass(CSSClass);
        Cells.Add(td);
    }

    public static void AddCell(this HtmlTableCellCollection Cells, string InnerText, string CssClass, int ColSpan = -1)
    {
        var HCell = new HtmlTableCell();
        HCell.InnerText = InnerText;
        if (CssClass != "")
            HCell.Attributes["class"] = CssClass;
        HCell.Align = "center";
        if (ColSpan > 1)
            HCell.ColSpan = ColSpan;

        Cells.Add(HCell);
    }
    public static void AddCell(this HtmlTableCellCollection Cells, string InnerText, string CssClass, string CellAlign)
    {
        var objCell = new HtmlTableCell();
        objCell.InnerText = InnerText;
        objCell.Attributes["class"] = CssClass;
        if (CellAlign != "")
            objCell.Align = CellAlign;
        Cells.Add(objCell);
    }

    public static void AddCellLeft(this HtmlTableCellCollection Cells, string InnerText, string CssClass, int cSpan)
    {
        var HCell = new HtmlTableCell();
        HCell.InnerText = InnerText;
        if (CssClass != "")
            HCell.Attributes["class"] = CssClass;
        HCell.Align = "left";
        if (cSpan > 1)
            HCell.ColSpan = cSpan;
        Cells.Add(HCell);
    }




    public static void AddHeaderCell(this HtmlTableCellCollection Cells, string HeaderText, string CSSClass, string Align = "")
    {
        var td = CreateHeaderCell(HeaderText, CSSClass);
        if (Align != "")
            td.Align = Align;
        Cells.Add(td);
    }

    public static void AddHeaderCellWithTooltip(this HtmlTableCellCollection Cells, string HeaderText, string CSSClass, bool BuildReportForExcel, string AddToolTipPopupBody)
    {
        var td = CreateHeaderCell(HeaderText, CSSClass);
        if (!BuildReportForExcel)
            td.AddOnMouseOver(AddToolTipPopupBody);
    }

    public static void AddDisplayNone(this WebControl control)
    {
        control.AddStyle("display:none;");
    }

    public static void AddDisplayNone(this HtmlControl control)
    {
        control.AddStyle("display:none;");
    }

    public static void AddDisplay(this WebControl control)
    {
        control.AddDisplay();
    }

    public static void AddDisplay(this HtmlControl control)
    {
        control.AddDisplay();
    }

    public static void AddVerticalAlignTop(this HtmlControl control)
    {
        control.AddStyle("vertical-align:top");
    }

    public static void AddCssClass(this HtmlControl control, string CSSClass)
    {
        control.Attributes.Add("class", CSSClass);
    }

    public static void AddBldGreyText(this HtmlControl control)
    {
        control.AddCssClass("regBldGreyText");
    }

    public static void AddBldBlueText(this HtmlControl control)
    {
        control.AddCssClass("regBldBlueText");
    }

    public static void AddRegText(this HtmlControl control)
    {
        control.AddCssClass("regText");
    }

    public static void AddEvenTD(this HtmlControl control)
    {
        control.AddCssClass("evenTD");
    }

    public static void AddReportHeaderBlue(this HtmlControl control)
    {
        control.AddCssClass("reportHeaderBlue");
    }

    public static void AddReporHeaderGreen(this HtmlControl control)
    {
        control.AddCssClass("reportHeaderGreen");
    }

    public static void AddReportRow(this HtmlControl control)
    {
        control.AddCssClass("reportRow");
    }

    public static void AddReportTable(this HtmlControl control)
    {
        control.AddCssClass("reportTotal");
    }

    public static void AddOnMouseOut(this HtmlControl control, string functionBody)
    {
        control.Attributes.Add("onmouseout", functionBody);
    }

    public static void AddOnMouseOut(this WebControl control, string functionBody)
    {
        control.Attributes.Add("onmouseout", functionBody);
    }

    public static void AddOnClick(this WebControl control, string functionBody)
    {
        control.Attributes.Add("onclick", functionBody);
    }

    public static void AddOnClick(this HtmlControl control, string functionBody)
    {
        control.Attributes.Add("onclick", functionBody);
    }

    public static void AddOnClick(this ListItem control, string functionBody)
    {
        control.Attributes.Add("onclick", functionBody);
    }

    public static void AddOnKeyUp(this HtmlControl control, string functionBody)
    {
        control.Attributes.Add("onkeyup", functionBody);
    }

    public static void AddOnKeyUp(this WebControl control, string functionBody)
    {
        control.Attributes.Add("onkeyup", functionBody);
    }

    public static void AddOnDblClick(this HtmlControl control, string functionBody)
    {
        control.Attributes.Add("ondblclick", functionBody);
    }
    public static void AddOnDblClick(this WebControl control, string functionBody)
    {
        control.Attributes.Add("ondblclick", functionBody);
    }

    public static void AddTabIndex(this HtmlControl control, int tabIndex)
    {
        control.Attributes.Add("tabOndex", tabIndex.ToString());
    }

    public static void AddTabIndex(this WebControl control, int tabIndex)
    {
        control.Attributes.Add("tabOndex", tabIndex.ToString());
    }

    public static void AddTitle(this WebControl control, string title)
    {
        control.Attributes.Add("title", title);
    }

    public static void AddTitle(this HtmlControl control, string title)
    {
        control.Attributes.Add("title", title);
    }


    public static void AddVisibilityHidden(this WebControl control)
    {
        control.AddStyle("visibility:hidden;");
    }

    public static void AddVisibilityHidden(this HtmlControl control)
    {
        control.AddStyle("visibility:hidden;");
    }

    public static void SetReadOnly(this WebControl control, bool isReadOnly = true)
    {
        control.Attributes.Add("readOnly", isReadOnly.ToString());
    }

    public static void SetReadOnly(this HtmlControl control, bool isReadOnly = true)
    {
        control.Attributes.Add("readOnly", isReadOnly.ToString());
    }

    public static void AddStyle(this HtmlControl control, string styleString)
    {
        control.Attributes.Add("style", styleString);
    }

    public static void AddStyle(this WebControl control, string styleString)
    {
        control.Attributes.Add("style", styleString);
    }


    public static void AddWidth(this HtmlControl control, int px)
    {
        control.Attributes.Add("width", px + "px");
    }

    public static void AddWidth(this WebControl control, int px)
    {
        control.Attributes.Add("width", px + "px");
    }

    public static void AddHeight(this HtmlControl control, int px)
    {
        control.Attributes.Add("height", px + "px");
    }

    public static void AddHeight(this WebControl control, int px)
    {
        control.Attributes.Add("height", px + "px");
    }

    public static void AddKeyPressBlockNonNumbers(this HtmlControl control)
    {
        control.AddOnKeyPress("return blockNonNumbers(this,event,false,false);");
    }

    public static void AddOnKeyPress(this HtmlControl control, string functionBody)
    {
        control.Attributes.Add("onkeypress", functionBody);
    }

    public static void AddOnKeyPress(this WebControl control, string functionBody)
    {
        control.Attributes.Add("onkeypress", functionBody);
    }

    public static void AddOnMouseOver(this WebControl control, string functionBody)
    {
        control.Attributes.Add("onmouseover", functionBody);
    }

    public static void AddOnMouseOver(this HtmlControl control, string functionBody)
    {
        control.Attributes.Add("onmouseover", functionBody);
    }

  
    private static HtmlTableCell CreateHeaderCell(string HeaderText, string CSSClass)
    {

        var td = new HtmlTableCell();
        td.InnerText = HeaderText;
        td.AddCssClass(CSSClass);
        return td;
    }

    public static DataSet ToDataSet<T>(this List<T> items)
    {
        var dataTable = new DataTable(typeof(T).Name);

        if (typeof(T).Namespace == "System")
        {
            dataTable.Columns.Add("");
            foreach (var item in items)
            {
                dataTable.Rows.Add(item);
            }
        }
        else
        {
            var Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item);
                }
                dataTable.Rows.Add(values);
            }
        }

        //Get all the properties

        
        var dataSet = new DataSet();
        dataSet.Tables.Add(dataTable);
        //put a breakpoint here and check datatable
        return dataSet;
    }


}