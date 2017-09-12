using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GSA.UnliquidatedObligations.Utility
{
    public static partial class SpreadsheetHelpers
    {
        public static object ExcelTypeConverter(object val, Type t)
        {
            try
            {
                var sval = val as string;
                if (t == typeof(DateTime) && sval!=null)
                {
                    double d;
                    if (double.TryParse(sval, out d))
                    {
                        return DateTime.FromOADate(d);
                    }
                }
                else if (t == typeof(Decimal) && sval != null)
                {
                    Decimal d;
                    //https://stackoverflow.com/questions/22291165/parsing-decimal-in-scientific-notation
                    if (Decimal.TryParse(sval, System.Globalization.NumberStyles.Float, null, out d))
                    {
                        return d;
                    }
                }
                return Convert.ChangeType(val, t);
            }
            catch (Exception ex)
            {
                if (t == typeof(DateTime))
                {
                    return DateTime.FromOADate(Convert.ToDouble(val));
                }
                throw ex;
            }
        }

        public static void LoadSheetsFromExcel(this DataSet ds, Stream st, LoadTablesFromSpreadsheetSettings settings = null)
        {
            settings = settings ?? new LoadTablesFromSpreadsheetSettings();
            using (var sd = SpreadsheetDocument.Open(st, false))
            {
                var sheetSettings = settings.SheetSettings;
                if (sheetSettings == null || sheetSettings.Count == 0)
                {
                    sheetSettings = new List<LoadRowsFromSpreadsheetSettings>();
                    for (int n = 0; n < sd.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().Count(); ++n)
                    {
                        sheetSettings.Add(new LoadRowsFromSpreadsheetSettings(settings.LoadAllSheetsDefaultSettings) { SheetNumber = n, UseSheetNameForTableName = true, TypeConverter = ExcelTypeConverter });
                    }
                }
                foreach (var ss in sheetSettings)
                {
                    var dt = settings.CreateDataTable == null ? new DataTable() : settings.CreateDataTable();
                    dt.LoadRowsFromExcel(sd, ss);
                    ds.Tables.Add(dt);
                }
            }
        }

        public static void LoadRowsFromExcel(this DataTable dt, Stream st, LoadRowsFromSpreadsheetSettings settings)
        {
            using (var sd = SpreadsheetDocument.Open(st, false))
            {
                dt.LoadRowsFromExcel(sd, settings ?? new LoadRowsFromSpreadsheetSettings { SheetNumber = 0 });
            }
        }

        private static void LoadRowsFromExcel(this DataTable dt, SpreadsheetDocument sd, LoadRowsFromSpreadsheetSettings settings)
        {
            DataTableHelpers.RequiresZeroRows(dt, nameof(dt));
            Requires.NonNull(sd, nameof(sd));
            Requires.NonNull(settings, nameof(settings));

            var rows = new List<IList<object>>();

            var sharedStringDictionary = ConvertSharedStringTableToDictionary(sd);
            var sheets = sd.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
            int sheetNumber = 0;
            foreach (var sheet in sheets)
            {
                if (sheetNumber == settings.SheetNumber || 0 == string.Compare(settings.SheetName, sheet.Name, true))
                {
                    if (settings.UseSheetNameForTableName)
                    {
                        dt.TableName = sheet.Name;
                    }
                    string relationshipId = sheet.Id.Value;
                    var worksheetPart = (WorksheetPart)sd.WorkbookPart.GetPartById(relationshipId);
                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    IEnumerable<Row> eRows = sheetData.Descendants<Row>();
                    foreach (Row erow in eRows)
                    {
                        CreateRow:
                        var row = new List<object>();
                        rows.Add(row);
                        foreach (var cell in erow.Descendants<Cell>())
                        {
                            var cr = GetColRowFromCellReference(cell.CellReference);
                            if (rows.Count <= cr.Item2) goto CreateRow;
                            while (row.Count < cr.Item1)
                            {
                                row.Add(null);
                            }
                            Debug.Assert(row.Count == cr.Item1);
                            var val = GetCellValue(sd, cell, settings.TreatAllValuesAsText, sharedStringDictionary);
                            row.Add(val);
                        }
                    }
                    GC.Collect();
                    IEnumerable<IList<object>> positionnedRows;
                    if (settings.SkipRawRows.HasValue)
                    {
                        positionnedRows = rows.Skip(settings.SkipRawRows.Value);
                    }
                    else if (settings.SkipWhileTester != null)
                    {
                        positionnedRows = rows.SkipWhile(settings.SkipWhileTester);
                    }
                    else
                    {
                        positionnedRows = rows;
                    }
                    dt.LoadRows(positionnedRows, settings);
                    return;
                }
                ++sheetNumber;
            }
            throw new Exception(string.Format(
                "Sheet [{0}] was not found",
                (object)settings.SheetNumber ?? (object)settings.SheetName));
        }

        private static readonly Regex ColRowExpr = new Regex(@"\s*([A-Z]+)(\d+)\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static Tuple<int, int> GetColRowFromCellReference(string cellReference)
        {
            var m = ColRowExpr.Match(cellReference);
            int colNum = 0;
            string colRef = m.Groups[1].Value.ToLower();
            for (int z = 0; z < colRef.Length; ++z)
            {
                colNum = colNum * 26 + (colRef[z] - 'a' + 1);
            }
            return new Tuple<int, int>(colNum - 1, int.Parse(m.Groups[2].Value) - 1);
        }

        private static IDictionary<int, string> ConvertSharedStringTableToDictionary(SpreadsheetDocument document)
        {
            var d = new Dictionary<int, string>();
            var stringTablePart = document.WorkbookPart.SharedStringTablePart;
            var pos = 0;
            foreach (DocumentFormat.OpenXml.OpenXmlElement el in stringTablePart.SharedStringTable.ChildElements)
            {
                d[pos++] = el.InnerText;
            }
            return d;
        }

        private static object GetCellValue(SpreadsheetDocument document, Cell cell, bool treatAllValuesAsText, IDictionary<int, string> sharedStringDictionary)
        {
            if (cell == null || cell.CellValue == null) return null;
            string value = cell.CellValue.InnerText;
            if (cell.DataType == null) return value;
            var t = cell.DataType.Value;
            if (treatAllValuesAsText && t != CellValues.SharedString)
            {
                return value;
            }
            switch (t)
            {
                case CellValues.String:
                    return value;
                case CellValues.SharedString:
                    return sharedStringDictionary[Int32.Parse(value)];
                case CellValues.Boolean:
                    return value == "1";
                case CellValues.Number:
                    return value;
                case CellValues.Date:
                    return value;
                default:
                    return value;
            }
        }
    }
}
