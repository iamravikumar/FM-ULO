using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GSA.UnliquidatedObligations.Utility
{
    public static class DatabaseHelpers
    {
        public static void ExecuteNonQuerySql(this IDbConnection conn, string sql, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                sql = string.Format(sql, args);
            }
            using (var cmd = new SqlCommand(sql, (SqlConnection)conn)
            {
                CommandType = CommandType.Text,
            })
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static DataSet ExecuteReadDataSet(this SqlCommand command, string tableNamePattern = null)
        {
            var conn = command.Connection;
            var ds = new DataSet();
            var tableNameExpr = new Regex(tableNamePattern ?? @"\Wtable:(\w+)\s*$", RegexOptions.IgnoreCase);
            string lastInfoMessage = null;
            SqlInfoMessageEventHandler infoMessageHandler = (s, e) => lastInfoMessage = e.Message;
            conn.InfoMessage += infoMessageHandler;
            using (var reader = command.ExecuteReader())
            {
                try
                {
                    ReadTable:
                    DataTable dt = null;
                    for (;;)
                    {
                        bool hasRow = reader.Read();
                        if (dt == null)
                        {
                            var tableName = string.Format("Table{0}", ds.Tables.Count);
                            if (lastInfoMessage != null)
                            {
                                var m = tableNameExpr.Match(lastInfoMessage);
                                if (m.Success)
                                {
                                    tableName = m.Groups[1].Value;
                                }
                                lastInfoMessage = null;
                            }
                            dt = new DataTable(tableName);
                            ds.Tables.Add(dt);

                            var colsSeen = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
                            for (int z = 0; z < reader.FieldCount; ++z)
                            {
                                var c = new DataColumn(reader.GetName(z), reader.GetFieldType(z));
                                if (colsSeen.Contains(c.ColumnName))
                                {
                                    c.ColumnName = DataTableHelpers.OnDuplicateAppendSeqeuntialNumber(dt, c.ColumnName);
                                }
                                colsSeen.Add(c.ColumnName);
                                dt.Columns.Add(c);
                            }
                        }
                        if (!hasRow) break;
                        var vals = new object[reader.FieldCount];
                        reader.GetValues(vals);
                        dt.Rows.Add(vals);
                        if (dt.Rows.Count % 1000 == 0)
                        {
                            Trace.WriteLine(string.Format("Exported {0} rows from table [{1}]...",
                                                    dt.Rows.Count,
                                                    dt.TableName
                                                    ));
                        }
                    }
                    if (dt != null)
                    {
                        Trace.WriteLine(string.Format("Exported {0} rows from table [{1}]", dt.Rows.Count, dt.TableName));
                        if (reader.NextResult())
                        {
                            goto ReadTable;
                        }
                    }
                    return ds;
                }
                finally
                {
                    conn.InfoMessage -= infoMessageHandler;
                }
            }
        }
    }
}
