using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace GSA.UnliquidatedObligations.Utility
{
    public static partial class DataTableHelpers
    {
        public static void ToSpreadSheet(this DataSet ds, string path)
        {
            using (var st = File.Create(path))
            {
                ds.ToSpreadSheet(st);
            }
        }

        private static void AddDefaultStylesPart(WorkbookPart workbookpart)
        {
            //https://stackoverflow.com/questions/11116176/cell-styles-in-openxml-spreadsheet-spreadsheetml
            var stylesPart = workbookpart.AddNewPart<WorkbookStylesPart>();
            var dst = stylesPart.GetStream();
            var st = ResourceHelpers.GetEmbeddedResourceAsStream("styles.xml");
            st.CopyTo(dst);
        }

        public static void ToSpreadSheet(this DataSet ds, Stream st)
        {
            using (var sd = SpreadsheetDocument.Create(st, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {

                // Add a WorkbookPart to the document.
                var workbookpart = sd.AddWorkbookPart();
                var workbook = workbookpart.Workbook = new Workbook();

                AddDefaultStylesPart(workbookpart);

                // Add Sheets to the Workbook.
                var sheets = workbook.AppendChild<Sheets>(new Sheets());

                var indexBySharedString = new Dictionary<string, int>();

                uint sheetNum = 0;
                foreach (DataTable dt in ds.Tables)
                {
                    // Add a WorksheetPart to the WorkbookPart.
                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    var worksheet = worksheetPart.Worksheet = new Worksheet(sheetData);

                    // Append a new worksheet and associate it with the workbook.
                    var sheet = new Sheet()
                    {
                        Id = sd.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = ++sheetNum,
                        Name = dt.TableName
                    };
                    sheets.Append(sheet);

                    var ssRow = new Row { RowIndex = 1 };
                    foreach (DataColumn dc in dt.Columns)
                    {
                        var ssCell = new Cell() { CellReference = CreateCellReference(dc.Ordinal, 0), StyleIndex = 14 };
                        //                        var ssCell = new Cell() { CellReference = CreateCellReference(dc.Ordinal, 0) };
                        SetValue(ssCell, dc.ColumnName, indexBySharedString);
                        ssRow.Append(ssCell);
                    }
                    sheetData.Append(ssRow);
                    uint rowNum = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        ssRow = new Row { RowIndex = (++rowNum) + 1 };
                        for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
                        {
                            var ssCell = new Cell() { CellReference = CreateCellReference(colNum, (int)rowNum) };
                            SetValue(ssCell, dr[colNum], indexBySharedString);
                            ssRow.Append(ssCell);
                        }
                        sheetData.Append(ssRow);
                    }

                    //Freeze Pane
                    //https://stackoverflow.com/questions/6428590/freeze-panes-in-openxml-sdk-2-0-for-excel-document
                    var sheetviews = new SheetViews();
                    worksheet.InsertAt(sheetviews, 0);
                    var sv = new SheetView()
                    {
                        WorkbookViewId = 0
                    };
                    sv.Pane = new Pane() { VerticalSplit = 1D, TopLeftCell = "A2", ActivePane = PaneValues.BottomLeft, State = PaneStateValues.Frozen };
                    sv.Append(new Selection() { Pane = PaneValues.BottomLeft });
                    sheetviews.Append(sv);
                    worksheet.AppendChild(new AutoFilter() { Reference = CreateCellReference(0, 0, dt.Columns.Count - 1, 0) });
                }

                workbookpart.Workbook.Save();

                SharedStringTableCreate(sd, indexBySharedString);

                sd.Save();

                sd.Close();
            }
        }

        private static string CreateCellReference(int colStart, int rowStart, int colEnd, int rowEnd)
        {
            return CreateCellReference(colStart, rowStart) + ":" + CreateCellReference(colEnd, rowEnd);
        }

        private static string CreateCellReference(int col, int row)
        {
            var cr = "";
            do
            {
                cr = ((char)('A' + col % 26)) + cr;
                col = col / 26;
            }
            while (col > 0);
            cr = cr + (row + 1).ToString();
            return cr;
        }

        private static void SetValue(Cell cell, object val, IDictionary<string, int> indexBySharedString)
        {
            if (val == null || val == DBNull.Value) val = "";
            var t = val.GetType();
            if (t.IsNumber())
            {
                cell.CellValue = new CellValue(val.ToString());
                cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.Number);
            }
            else if (t == typeof(bool))
            {
                cell.CellValue = new CellValue((bool)val ? "1" : "0");
                cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.Boolean);
            }
            else if (t == typeof(DateTime))
            {
                //                cell.CellValue = new CellValue(((DateTime)val).Date.ToOADate().ToString());
                //                cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.Date);
                cell.CellValue = new CellValue(((DateTime)val).Date.ToShortDateString());
                cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.String);
            }
            else
            {
                var sVal = (string)val;
                int pos;
                if (!indexBySharedString.TryGetValue(sVal, out pos))
                {
                    pos = indexBySharedString.Count;
                    indexBySharedString[sVal] = pos;
                }
                cell.CellValue = new CellValue(pos.ToString());
                cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.SharedString);
            }
        }

        private static void SharedStringTableCreate(SpreadsheetDocument sd, IDictionary<string, int> indexBySharedString)
        {
            SharedStringTablePart shareStringPart;
            if (sd.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
            {
                shareStringPart = sd.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
            }
            else
            {
                shareStringPart = sd.WorkbookPart.AddNewPart<SharedStringTablePart>();
            }

            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            if (shareStringPart.SharedStringTable.Elements<SharedStringItem>().Count() > 0)
            {
                throw new InvalidOperationException("The workbook's shared string table already has values");
            }

            var items = indexBySharedString.OrderBy(kvp => kvp.Value).ToList();

            int iLast = -1;
            foreach (var kvp in items)
            {
                if (kvp.Value != ++iLast)
                {
                    throw new InvalidOperationException("The shared string dictionary has gaps or is not zero based");
                }
            }

            foreach (var kvp in items)
            {
                shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(kvp.Key)));
            }
            shareStringPart.SharedStringTable.Save();
        }
    }
}
