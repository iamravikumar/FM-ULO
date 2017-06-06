using System.Data;
using System.Data.SqlClient;

namespace GSA.UnliquidatedObligations.UploadTable
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
    }
}
