using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.Web.Models;
using Hangfire;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IBackgroundTasks
    {
        [AutomaticRetry(Attempts = 1)]
        void Email(string subject, string recipient, string template, object model);

        [AutomaticRetry(Attempts = 1)]
        Task Email(string recipient, string subjectTemplate, string bodyTemplate, string htmlBodyTemplate, object model);

        [AutomaticRetry(Attempts = 1)]
        Task EmailReport(string[] recipients, string subjectTemplate, string bodyTemplate, string htmlBodyTemplate, object model, string reportName, IDictionary<string, string> paramValueByParamName);

        [AutomaticRetry(Attempts = 0)]
        void UploadFiles(UploadFilesModel files);

        [AutomaticRetry(Attempts = 0)]
        Task SendAssignWorkFlowsBatchNotifications(int reviewId);

        [AutomaticRetry(Attempts = 1)]
        Task SendAssignWorkFlowsBatchNotifications(int reviewId, string userId);

        [AutomaticRetry(Attempts = 1)]
        Task SendAssignWorkFlowsBatchNotifications(int[] workflowIds, string userId);

        void CreateULOsAndAssign(int reviewId, int WorkflowDefinitionId, DateTime reviewDate);

        Task AssignWorkFlows(int reviewId, bool sendBatchNotifications);
    }
}
