using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;

namespace GSA.UnliquidatedObligations.Web.Tests.Mocks
{
    class WorkflowDescriptionMock : IWorkflowDescription
    {
        public IEnumerable<WorkflowActivity> Activities { get; set; }

        public ICollection<WebActionWorkflowActivity> WebActionWorkflowActivities { get; set; }
    }
}
