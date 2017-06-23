using GSA.UnliquidatedObligations.BusinessLayer.Data.DbCommandInterceptors;
using System.Data.Entity.Infrastructure.Interception;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class ULODBEntities
    {
        static ULODBEntities()
        {
            DbInterception.Add(SqlCommandTextInterceptor.SetArithAbortOnInstance);
            DbInterception.Add(SqlTracerInterceptor.Instance);
        }
    }
}
