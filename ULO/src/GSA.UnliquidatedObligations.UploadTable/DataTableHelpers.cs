using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GSA.UnliquidatedObligations.UploadTable
{
    public static class DataTableHelpers
    {
        public static string GenerateCreateTableSQL(this DataTable dt, string schema = "dbo")
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
                if (dc.DataType == typeof(int))
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

        public static void UploadIntoSqlServer(this DataTable dt, Func<SqlConnection> createConnection, UploadIntoSqlServerSettings settings=null)
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
                            var ts = Stuff.StringTrimOrNull(s);
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
            dt.SetColumnWithValue(columnName, (a,b) => value);
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

        private static void RequiresZeroRows(DataTable dt, string argName=null)
        {
            Requires.NonNull(dt, argName??nameof(dt));
            if (dt.Rows.Count > 0) throw new ArgumentException("dt must not already have any rows", nameof(dt));
        }

        public static void RowAddErrorRethrow(Exception ex, int rowNum)
        {
            throw new Exception(string.Format("Problem adding row {0}", rowNum), ex);
        }

        public static void RowAddErrorTraceAndIgnore(Exception ex, int rowNum)
        {
            Trace.WriteLine(string.Format("Problem adding row {0}.  Will Skip.\n{1}", rowNum, ex));
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

        public static void LoadRows(this DataTable dt, IEnumerable<IList<object>> rows, LoadRowsSettings settings=null)
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
                var colName = Stuff.StringTrimOrNull(Stuff.Object2String(headerRow[z]));
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
                else if (c==null)
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
                var fields = new object[dt.Columns.Count];
                try
                {
                    for (int z = 0; z < columnMap.Length; ++z)
                    {
                        var c = columnMap[z];
                        if (c == null) continue;
                        object val = row[z];
                        if (val == null)
                        {
                            val = DBNull.Value;
                        }
                        else if (val.GetType()!=c.DataType)
                        {
                            val = Convert.ChangeType(val, c.DataType);
                        }
                        fields[c.Ordinal] = val;
                    }
                    if (rowNumberColumn != null)
                    {
                        fields[rowNumberColumn.Ordinal] = rowNum;
                    }
                    dt.Rows.Add(fields);
                }
                catch (Exception ex)
                {
                    onRowAddError(ex, rowNum);
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
            s = s.ToCamelCase();
            return s;
        }

        public static Func<string, string> CreateDictionaryMapper(IDictionary<string, string> m, bool onMissingPassthrough=false)
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
