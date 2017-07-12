using GSA.UnliquidatedObligations.Web.Models;
using Hangfire;
using System;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IBackgroundTasks
    {
        [AutomaticRetry(Attempts = 1)]
        void Email(string subject, string recipient, string template, object model);

        [AutomaticRetry(Attempts = 0)]
        void UploadFiles(UploadFilesModel files);

        void CreateULOsAndAssign(int reviewId, int WorkflowDefinitionId, DateTime reviewDate);

        Task AssignWorkFlows(int reviewId);
    }
}
