using System.Data.SqlClient;

namespace GSA.UnliquidatedObligations.Utility
{
    public class UploadIntoSqlServerSettings
    {
        public string Schema { get; set; }
        public bool GenerateTable { get; set; }
        public int RowsCopiedNotifyIncrement { get; set; }
        public SqlRowsCopiedEventHandler RowsCopiedNotifyHandler { get; set; }

        public UploadIntoSqlServerSettings()
        {
            Schema = "dbo";
            RowsCopiedNotifyIncrement = 1000;
        }
    }
}
