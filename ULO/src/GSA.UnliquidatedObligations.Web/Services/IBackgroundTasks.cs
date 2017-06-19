using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IBackgroundTasks
    {
        void Email(string subject, string recipient, string template, object model);

        void UploadFiles(UploadFilesModel files);

        void CreateULOsAndAssign(int reviewId, int WorkflowDefinitionId);

        Task AssignWorkFlows(int reviewId);

    }
}
