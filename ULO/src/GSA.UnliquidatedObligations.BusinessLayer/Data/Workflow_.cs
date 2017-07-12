using GSA.UnliquidatedObligations.BusinessLayer.Workflow;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class WorkflowDefinition
    {
        public WorkflowDescription Description
        {
            get
            {
                if (WorkflowDescription_p == null)
                {
                    WorkflowDescription_p = WorkflowDescription.DeserializeFromXml(DescriptionXml);
                }
                return WorkflowDescription_p;
            }
        }
        private WorkflowDescription WorkflowDescription_p;
    }
}
