using RevolutionaryStuff.Core;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data.DbCommandInterceptors
{
    /// <remarks>
    /// http://dba.stackexchange.com/questions/2500/make-sqlclient-default-to-arithabort-on
    /// </remarks>
    internal class SqlCommandTextInterceptor : IDbCommandInterceptor
    {
        internal static readonly IDbInterceptor SetArithAbortOnInstance = new SqlCommandTextInterceptor("SET ARITHABORT ON;");

        private readonly string PrependText;

        private SqlCommandTextInterceptor(string prependText)
        {
            Requires.Text(prependText, nameof(prependText));
            PrependText = prependText;
        }

        private void FixupCommand(DbCommand command)
        {
            switch (command.CommandType)
            {
                case CommandType.Text:
                    command.CommandText = PrependText + command.CommandText;
                    break;
                case CommandType.TableDirect:
                case CommandType.StoredProcedure:
                    Stuff.Noop();
                    break;
            }
        }

        void IDbCommandInterceptor.NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        void IDbCommandInterceptor.NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            FixupCommand(command);
        }

        void IDbCommandInterceptor.ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
        }

        void IDbCommandInterceptor.ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            FixupCommand(command);
        }

        void IDbCommandInterceptor.ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }

        void IDbCommandInterceptor.ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            FixupCommand(command);
        }
    }
}
