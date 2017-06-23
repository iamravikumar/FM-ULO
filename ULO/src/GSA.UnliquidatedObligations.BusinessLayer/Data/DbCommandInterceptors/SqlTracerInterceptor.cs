using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace GSA.UnliquidatedObligations.BusinessLayer.Data.DbCommandInterceptors
{
    /// <remarks>http://blog.oneunicorn.com/2013/05/14/ef6-sql-logging-part-3-interception-building-blocks/</remarks>
    internal class SqlTracerInterceptor : IDbCommandInterceptor
    {
        internal static readonly IDbInterceptor Instance = new SqlTracerInterceptor();

        private SqlTracerInterceptor()
        { }

        private void TraceInterception<T>(DbCommand command, DbCommandInterceptionContext<T> interceptionContext, [CallerMemberName] string caller = null)
        {
            Trace.WriteLine(string.Format("{0}: Type={1} Text={2}", caller, command.CommandType, command.CommandText));
        }

        private void TraceInterception(DbCommand command, DbCommandInterceptionContext<int> interceptionContext, [CallerMemberName] string caller = null)
        {
            Trace.WriteLine(string.Format("{0}: Type={1} Text={2}", caller, command.CommandType, command.CommandText));
        }

        void IDbCommandInterceptor.NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            TraceInterception(command, interceptionContext);
        }

        void IDbCommandInterceptor.NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            TraceInterception(command, interceptionContext);
        }

        void IDbCommandInterceptor.ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            TraceInterception(command, interceptionContext);
        }

        void IDbCommandInterceptor.ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            TraceInterception(command, interceptionContext);
        }

        void IDbCommandInterceptor.ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            TraceInterception(command, interceptionContext);
        }

        void IDbCommandInterceptor.ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            TraceInterception(command, interceptionContext);
        }
    }
}
