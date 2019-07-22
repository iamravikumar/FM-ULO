using System;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.EntityFrameworkCore;
using D = GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.BusinessLayer
{
    public static class UloHelpers
    {
        public const string GsaUrn = "urn:gsa.gov/";
        public const string UloUrn = GsaUrn + "unliquidatedObligation/";
        public const string WorkflowDescUrn = UloUrn + "WorkflowDescriptions/";

        public static string ToRfc8601(this DateTime dt)
            => dt.ToUniversalTime().ToString("o");

        internal static string CreatePdnWithInstance(string pegasysDocumentNumber, int? pegasysDocumentNumberInstance)
            => pegasysDocumentNumberInstance.HasValue ? $"{pegasysDocumentNumber}.{pegasysDocumentNumberInstance}" : $"{pegasysDocumentNumber}";

        public static async Task<D.Workflow> FindWorkflowAsync(this UloDbContext db, int workflowId)
            => await db.Workflows
            .Include(q => q.OwnerUser)
            .Include(q => q.WorkflowDocuments)
            .Include(q => q.TargetUlo)
            .Include(q => q.WorkflowUnliqudatedObjectsWorkflowQuestions)
            .WhereReviewExists()
            .FirstOrDefaultAsync(q => q.WorkflowId == workflowId);

        public static IQueryable<UnliquidatedObligation> WhereReviewExists(this IQueryable<UnliquidatedObligation> wf)
            => wf.Where(z => z.Review != null);

        public static IQueryable<D.Workflow> WhereReviewExists(this IQueryable<D.Workflow> wf)
            => wf.Where(z => z.TargetUlo.Review != null);
    }
}
