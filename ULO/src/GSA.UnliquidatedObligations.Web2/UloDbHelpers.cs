using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core;
using Serilog;
using Traffk.StorageProviders;

namespace GSA.UnliquidatedObligations.Web
{
    public static class UloDbHelpers
    {
        public static IQueryable<UnliquidatedObligation> WhereReviewExists(this IQueryable<UnliquidatedObligation> wf)
            => wf.Where(z => z.Review != null);

        public static IQueryable<Workflow> WhereReviewExists(this IQueryable<Workflow> wf)
            => wf.Where(z => z.TargetUlo.Review != null && z.TargetUlo.Review.DeletedAtUtc == null);

        public static string OnDuplicateAppendSeqeuntialNumber(DataTable dt, string inboundColumnName)
        {
            for (int z = 2; ; ++z)
            {
                var newName = string.Format("{0}_{1}", inboundColumnName, z);
                if (!dt.Columns.Contains(newName)) return newName;
            }
        }

        public static DataSet ExecuteReadDataSet(this SqlCommand command, ILogger logger=null, string tableNamePattern = null)
        {
            logger = logger ?? Serilog.Log.Logger;
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
                    for (; ; )
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
                            logger.Information("Exporting from table [{tableName}]...", dt.TableName);

                            var colsSeen = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
                            for (int z = 0; z < reader.FieldCount; ++z)
                            {
                                var c = new DataColumn(reader.GetName(z), reader.GetFieldType(z));
                                if (colsSeen.Contains(c.ColumnName))
                                {
                                    c.ColumnName = OnDuplicateAppendSeqeuntialNumber(dt, c.ColumnName);
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
                            logger.Debug("Exported {rowsExported} rows from table [{tableName}]...",
                                                    dt.Rows.Count,
                                                    dt.TableName
                                                    );
                        }
                    }
                    if (dt != null)
                    {
                        logger.Information("Exported {rowsExported} rows from table [{tableName}]", dt.Rows.Count, dt.TableName);
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
