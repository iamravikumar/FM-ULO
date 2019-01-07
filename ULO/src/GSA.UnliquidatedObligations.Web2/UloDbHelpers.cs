using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web
{
    public static class UloDbHelpers
    {
        public static IQueryable<UnliquidatedObligation> WhereReviewExists(this IQueryable<UnliquidatedObligation> wf)
            => wf.Where(z => z.Review != null);

        public static IQueryable<Workflow> WhereReviewExists(this IQueryable<Workflow> wf)
            => wf.Where(z => z.TargetUlo.Review != null);
    }
}
