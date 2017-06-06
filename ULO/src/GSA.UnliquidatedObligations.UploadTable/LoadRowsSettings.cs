using System;
using System.Data;

namespace GSA.UnliquidatedObligations.UploadTable
{
    public class LoadRowsSettings
    {
        public Action<Exception, int> RowAddErrorHandler { get; set; }
        public string RowNumberColumnName { get; set; }
        public Func<DataTable, string, string> DuplicateColumnRenamer { get; set; }
        public Func<string, string> ColumnMapper { get; set; }
    }
}
