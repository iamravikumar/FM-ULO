using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GSA.UnliquidatedObligations.Utility
{
    public static partial class DataTableHelpers
    {
        public static string GenerateCreateTableSQL(this DataTable dt, string schema = "dbo", IDictionary<Type, string> typeMap = null)
        {
            Requires.NonNull(dt, nameof(dt));
            Requires.Text(dt.TableName, nameof(dt.TableName));
            Requires.Text(schema, nameof(schema));

            var sb = new StringBuilder();
            sb.AppendFormat("create table [{0}].[{1}]\n(\n", schema, dt.TableName);
            for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
            {
                var dc = (DataColumn)dt.Columns[colNum];
                string sqlType;
                if (typeMap != null && typeMap.ContainsKey(dc.DataType))
                {
                    sqlType = typeMap[dc.DataType];
                }
                else if (dc.DataType == typeof(int))
                {
                    sqlType = "int";
                }
                else if (dc.DataType == typeof(bool))
                {
                    sqlType = "bit";
                }
                else if (dc.DataType == typeof(float) ||
                         dc.DataType == typeof(double))
                {
                    sqlType = "float";
                }
                else if (dc.DataType == typeof(DateTime))
                {
                    sqlType = "datetime";
                }
                else if (dc.DataType == typeof(Decimal))
                {
                    sqlType = "money";
                }
                else if (dc.DataType == typeof(Byte))
                {
                    sqlType = "tinyint";
                }
                else if (dc.DataType == typeof(Int16))
                {
                    sqlType = "smallint";
                }
                else if (dc.DataType == typeof(Int64))
                {
                    sqlType = "bigint";
                }
                else if (dc.DataType == typeof(string))
                {
                    sqlType = string.Format("nvarchar({0})",
                        (dc.MaxLength <= 0 || dc.MaxLength > 4000) ? "max" : dc.MaxLength.ToString());
                }
                else
                {
                    throw new ArgumentException(string.Format("cannot translate type {0} to sql", dc.DataType.Name), dc.ColumnName);
                }
                sb.AppendFormat("\t[{0}] {1} {2}{3}\n",
                    dc.ColumnName,
                    sqlType,
                    dc.AllowDBNull ? "NULL" : "NOT NULL",
                    colNum == dt.Columns.Count - 1 ? "" : ",");
            }
            sb.AppendFormat(")\n");
            return sb.ToString();
        }

        public static void UploadIntoSqlServer(this DataTable dt, Func<SqlConnection> createConnection, UploadIntoSqlServerSettings settings = null)
        {
            Requires.NonNull(dt, nameof(dt));
            settings = settings ?? new UploadIntoSqlServerSettings();

            //Create table and insert 1 batch at a time
            using (var conn = createConnection())
            {
                conn.Open();

                if (settings.GenerateTable)
                {
                    var sql = dt.GenerateCreateTableSQL(settings.Schema);
                    Trace.WriteLine(sql);
                    conn.ExecuteNonQuerySql(sql);
                }

                var copy = new SqlBulkCopy(conn);
                copy.BulkCopyTimeout = 60 * 60 * 4;
                copy.DestinationTableName = string.Format("[{0}].[{1}]", settings.Schema, dt.TableName);

                copy.NotifyAfter = settings.RowsCopiedNotifyIncrement;
                copy.SqlRowsCopied += (sender, e) => Trace.WriteLine(string.Format("Uploaded {0}/{1} rows",
                    e.RowsCopied,
                    dt.Rows.Count
                    ));
                if (settings.RowsCopiedNotifyHandler != null)
                {
                    copy.SqlRowsCopied += settings.RowsCopiedNotifyHandler;
                }
                copy.WriteToServer(dt);
                copy.Close();
            }
            Trace.WriteLine(string.Format("Uploaded {0} rows", dt.Rows.Count));
        }

        public static void MakeDateColumnsFitSqlServerBounds(this DataTable dt, DateTime? minDate = null, DateTime? maxDate = null)
        {
            Requires.NonNull(dt, nameof(dt));

            var lower = minDate.GetValueOrDefault(SqlServerMinDateTime);
            var upper = maxDate.GetValueOrDefault(SqlServerMaxDateTime);

            for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
            {
                var dc = (DataColumn)dt.Columns[colNum];
                if (dc.DataType != typeof(DateTime)) continue;
                int changeCount = 0;
                for (int rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
                {
                    var o = dt.Rows[rowNum][dc];
                    if (o == DBNull.Value) continue;
                    var val = (DateTime)o;
                    if (val < lower)
                    {
                        ++changeCount;
                        dt.Rows[rowNum][dc] = lower;
                    }
                    else if (val > upper)
                    {
                        ++changeCount;
                        dt.Rows[rowNum][dc] = upper;
                    }
                }
                Trace.WriteLine($"MakeDateColumnsFitSqlServerBounds table({dt.TableName}) column({dc.ColumnName}) {colNum}/{dt.Columns.Count} => {changeCount} changes");
            }
        }

        private static readonly DateTime SqlServerMinDateTime = new DateTime(1753, 1, 1);
        private static readonly DateTime SqlServerMaxDateTime = new DateTime(9999, 12, 31);

        public static void IdealizeStringColumns(this DataTable dt, bool trimAndNullifyStringData = false)
        {
            Requires.NonNull(dt, nameof(dt));

            for (int colNum = 0; colNum < dt.Columns.Count; ++colNum)
            {
                var dc = (DataColumn)dt.Columns[colNum];
                Trace.WriteLine($"IdealizeStringColumns table({dt.TableName}) column({dc.ColumnName}) {colNum}/{dt.Columns.Count}");
                if (dc.DataType != typeof(string)) continue;
                var len = 0;
                bool hasNulls = false;
                for (int rowNum = 0; rowNum < dt.Rows.Count; ++rowNum)
                {
                    var o = dt.Rows[rowNum][dc];
                    var s = o as string;
                    if (s == null)
                    {
                        hasNulls = true;
                    }
                    else
                    {
                        if (trimAndNullifyStringData)
                        {
                            var ts = StringHelpers.TrimOrNull(s);
                            if (ts != s)
                            {
                                if (ts == null)
                                {
                                    hasNulls = true;
                                    dt.Rows[rowNum][dc] = DBNull.Value;
                                    continue;
                                }
                                s = ts;
                                dt.Rows[rowNum][dc] = s;
                            }
                        }
                        len = Math.Max(len, s.Length);
                    }
                }
                dc.AllowDBNull = hasNulls;
                dc.MaxLength = Math.Max(1, len);
            }
        }

        public static void SetColumnWithValue<T>(this DataTable dt, string columnName, T value)
        {
            dt.SetColumnWithValue(columnName, (a, b) => value);
        }

        public static void SetColumnWithValue<T>(this DataTable dt, string columnName, Func<DataRow, int, T> valueGenerator)
        {
            Requires.NonNull(dt, nameof(dt));
            Requires.Text(columnName, nameof(columnName));
            Requires.NonNull(valueGenerator, nameof(valueGenerator));

            var pos = dt.Columns[columnName].Ordinal;
            for (int z = 0; z < dt.Rows.Count; ++z)
            {
                var dr = dt.Rows[z];
                var val = valueGenerator(dr, z);
                dr[pos] = val;
            }
        }

        private static void RequiresZeroRows(DataTable dt, string argName = null)
        {
            Requires.NonNull(dt, argName ?? nameof(dt));
            if (dt.Rows.Count > 0) throw new ArgumentException("dt must not already have any rows", nameof(dt));
        }

        public static void RowAddErrorIgnore(Exception ex, int rowNum)
        {
            Trace.WriteLine(string.Format("Problem adding row {0} because [{1}]", rowNum, ex.Message));
        }

        public static void RowAddErrorRethrow(Exception ex, int rowNum)
        {
            throw new Exception(string.Format("Problem adding row {0}", rowNum), ex);
        }

        public static bool DontAddEmptyRows(DataTable dt, object[] row)
        {
            foreach (var v in row)
            {
                if (v != DBNull.Value && v != null && v.ToString() != "") return true;
            }
            return false;
        }

        public static void RowAddErrorTraceAndIgnore(Exception ex, int rowNum)
        {
            Trace.WriteLine(string.Format("Problem adding row {0}.  Will Skip.\n{1}", rowNum, ex));
        }

        public static object ExcelTypeConverter(object val, Type t)
        {
            try
            {
                if (t == typeof(DateTime) && val is string)
                {
                    double d;
                    if (double.TryParse((string)val, out d))
                    {
                        return DateTime.FromOADate(d);
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

        public static void LoadSheetsFromExcel(this DataSet ds, Stream st, LoadSheetsFromExcelSettings settings = null)
        {
            settings = settings ?? new LoadSheetsFromExcelSettings();
            using (var sd = SpreadsheetDocument.Open(st, false))
            {
                var sheetSettings = settings.SheetSettings;
                if (sheetSettings == null || sheetSettings.Count == 0)
                {
                    sheetSettings = new List<LoadRowsFromExcelSettings>();
                    for (int n = 0; n < sd.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().Count(); ++n)
                    {
                        sheetSettings.Add(new LoadRowsFromExcelSettings(settings.LoadAllSheetsDefaultSettings) { SheetNumber = n, UseSheetNameForTableName = true, TypeConverter = ExcelTypeConverter });
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

        public static void LoadRowsFromExcel(this DataTable dt, Stream st, LoadRowsFromExcelSettings settings)
        {
            using (var sd = SpreadsheetDocument.Open(st, false))
            {
                dt.LoadRowsFromExcel(sd, settings ?? new LoadRowsFromExcelSettings { SheetNumber = 0 });
            }
        }

        private static void LoadRowsFromExcel(this DataTable dt, SpreadsheetDocument sd, LoadRowsFromExcelSettings settings)
        {
            RequiresZeroRows(dt, nameof(dt));
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
                    else if (settings.SkipWhileTester!=null)
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

        public static void LoadRowsFromDelineatedText(this DataTable dt, Stream st, LoadRowsFromDelineatedTextSettings settings)
        {
            RequiresZeroRows(dt, nameof(dt));
            Requires.ReadableStreamArg(st, nameof(st));
            Requires.NonNull(settings, nameof(settings));

            IList<string[]> rows = null;
            if (settings.Format == LoadRowsFromDelineatedTextFormats.PipeSeparatedValues)
            {
                var sections = new List<string[]>();
                int lineNum = 0;
                using (var sr = new StreamReader(st))
                {
                    var maxCapacity = 1024 * 1024 * 127;
                    var sb = new StringBuilder(maxCapacity + 1024 * 1024);
                    for (;;)
                    {
                        var line = sr.ReadLine();
                        if (line != null)
                        {
                            ++lineNum;
                            sb.AppendLine(line);
                        }
                        if (sb.Length > maxCapacity || line == null)
                        {
                            var csv = sb.ToString();
                            sb.Clear();
                            rows = CSV.ParseText(csv, '|', null);
                            csv = null;
                            sections.AddRange(rows);
                            rows = null;
                            if (line == null) break;
                        }
                    }
                }
                rows = sections;
            }
            else if (settings.Format == LoadRowsFromDelineatedTextFormats.CommaSeparatedValues)
            {
                var data = new StreamReader(st).ReadToEnd();
                rows = CSV.ParseText(data);
            }
            else if (settings.Format == LoadRowsFromDelineatedTextFormats.Custom)
            {
                var data = new StreamReader(st).ReadToEnd();
                rows = CSV.ParseText(data, settings.FieldDelim, settings.QuoteChar);
            }
            else
            {
                throw new UnexpectedSwitchValueException(settings.Format);
            }
            GC.Collect();
            dt.LoadRows(rows.Skip(settings.SkipRawRows), settings);
        }

        public static void LoadRows(this DataTable dt, IEnumerable<IList<object>> rows, LoadRowsSettings settings = null)
        {
            RequiresZeroRows(dt, nameof(dt));
            Requires.NonNull(rows, nameof(rows));
            settings = settings ?? new LoadRowsSettings();

            var e = rows.GetEnumerator();
            if (!e.MoveNext())
            {
                return;
            }

            bool createColumns = dt.Columns.Count == 0;
            DataColumn rowNumberColumn = null;
            if (settings.RowNumberColumnName != null)
            {
                rowNumberColumn = dt.Columns[settings.RowNumberColumnName];
                if (rowNumberColumn == null)
                {
                    rowNumberColumn = new DataColumn(settings.RowNumberColumnName, typeof(int)) { AllowDBNull = false };
                    dt.Columns.Add(rowNumberColumn);
                }
                else
                {
                    if (!(rowNumberColumn.DataType == typeof(int) || rowNumberColumn.DataType == typeof(long)))
                    {
                        throw new InvalidOperationException(string.Format("Existing table has a rowNumberColumn of an incompatible data type"));
                    }
                }
            }

            var headerRow = e.Current;
            var columnMapper = settings.ColumnMapper ?? DataTableHelpers.OneToOneColumnNameMapper;
            var duplicateColumnRenamer = settings.DuplicateColumnRenamer ?? DataTableHelpers.OnDuplicateColumnNameThrow;
            var columnMap = new DataColumn[headerRow.Count];
            for (int z = 0; z < headerRow.Count(); ++z)
            {
                var colName = StringHelpers.TrimOrNull(Stuff.ObjectToString(headerRow[z]));
                if (colName == null)
                {
                    continue;
                }
                colName = columnMapper(colName);
                if (colName == null)
                {
                    continue;
                }
                var c = dt.Columns[colName];
                if (createColumns)
                {
                    if (c == null)
                    {
                        c = new DataColumn(colName);
                        dt.Columns.Add(c);
                    }
                    else
                    {
                        colName = duplicateColumnRenamer(dt, colName);
                        c = new DataColumn(colName);
                        dt.Columns.Add(c);
                    }
                }
                else if (c == null)
                {
                    Trace.WriteLine(string.Format("Will ignore source column #{0} with name=[{1}]", z, colName));
                }
                columnMap[z] = c;
            }

            int rowNum = -1;
            var onRowAddError = settings.RowAddErrorHandler ?? RowAddErrorRethrow;
            while (e.MoveNext())
            {
                ++rowNum;
                var row = e.Current;
                if (row.Count == 0) continue;
                var fields = new object[dt.Columns.Count];
                try
                {
                    for (int z = 0; z < columnMap.Length; ++z)
                    {
                        var c = columnMap[z];
                        if (c == null) continue;
                        object val = z >= row.Count ? null : row[z];
                        if (val == null)
                        {
                            val = DBNull.Value;
                        }
                        else if (val.GetType() != c.DataType)
                        {
                            val = settings.TypeConverter(val, c.DataType);
                        }
                        fields[c.Ordinal] = val;
                    }
                    if (rowNumberColumn != null)
                    {
                        fields[rowNumberColumn.Ordinal] = rowNum;
                    }
                    if (null == settings.ShouldAddRow || settings.ShouldAddRow(dt, fields))
                    {
                        dt.Rows.Add(fields);
                    }
                }
                catch (Exception ex)
                {
                    onRowAddError(ex, rowNum);
                }
            }
        }

        public static DataColumn CloneToUnbound(this DataColumn c)
            => new DataColumn(c.ColumnName, c.DataType)
            {
                AllowDBNull = c.AllowDBNull,
                Caption = c.Caption,
                AutoIncrement = c.AutoIncrement,
                AutoIncrementSeed = c.AutoIncrementSeed,
                AutoIncrementStep = c.AutoIncrementStep,
                DateTimeMode = c.DateTimeMode,
                DefaultValue = c.DefaultValue,
                Expression = c.Expression,
                MaxLength = c.MaxLength,
                Namespace = c.Namespace,
                Prefix = c.Prefix,
                ReadOnly = c.ReadOnly,
                Unique = c.Unique
            };

        public static void Append(this DataTable dt, DataTable other, bool appendOtherTableColumns = false)
        {
            Requires.NonNull(other, nameof(other));

            bool sameStructure = dt.Columns.Count == other.Columns.Count;
            var dtWasBlank = dt.Columns.Count == 0;
            var columnNamesToAppend = new List<string>();
            foreach (DataColumn bCol in other.Columns)
            {
                var aCol = dt.Columns[bCol.ColumnName];
                sameStructure = sameStructure && aCol != null && aCol.Ordinal == bCol.Ordinal;
                if (aCol == null)
                {
                    if (!appendOtherTableColumns) continue;
                    dt.Columns.Add(bCol.CloneToUnbound());
                }
                columnNamesToAppend.Add(bCol.ColumnName);
            }
            sameStructure = (sameStructure || dtWasBlank) && (dt.Columns.Count == other.Columns.Count);
            foreach (DataRow bRow in other.Rows)
            {
                if (sameStructure)
                {
                    dt.Rows.Add(bRow.ItemArray);
                }
                else
                {
                    var aRow = dt.NewRow();
                    foreach (var columnName in columnNamesToAppend)
                    {
                        aRow[columnName] = bRow[columnName];
                    }
                    dt.Rows.Add(aRow);
                }
            }
        }

        #region Data Column Helpers

        public static string OneToOneColumnNameMapper(string inboundColumnName)
        {
            return inboundColumnName;
        }

        private static readonly Regex MakeFriendlyExpr = new Regex("[^0-9a-z]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string UpperCamelNoSpecialCharactersColumnNameMapper(string inboundColumnName)
        {
            var s = MakeFriendlyExpr.Replace(inboundColumnName, " ");
            s = s.ToUpperCamelCase();
            return s;
        }

        public static Func<string, string> CreateDictionaryMapper(IDictionary<string, string> m, bool onMissingPassthrough = false)
        {
            Requires.NonNull(m, nameof(m));
            Func<string, string> f = delegate (string s)
            {
                if (m.ContainsKey(s)) return m[s];
                return onMissingPassthrough ? s : null;
            };
            return f;
        }

        public static string OnDuplicateColumnNameThrow(DataTable dt, string duplicateColumnName)
        {
            throw new Exception(string.Format("Datatable cannot have duplicate column names.  [{0}] occurs at least twice", duplicateColumnName));
        }

        public static string OnDuplicateAppendSeqeuntialNumber(DataTable dt, string inboundColumnName)
        {
            for (int z = 2; ; ++z)
            {
                var newName = string.Format("{0}_{1}", inboundColumnName, z);
                if (!dt.Columns.Contains(newName)) return newName;
            }
        }

        #endregion
    }
}
