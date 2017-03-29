using System.Data.Entity;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    //Delete this after we regenerate the EDMX

    public class Workflow
    {
        public int WorkflowId { get; set; }
        public string WorkflowKey { get; set; }
        public string CurrentWorkflowActivityKey { get; set; }
        public int Version { get; set; }
        public int RegionId { get; set; }
        public virtual Region Region { get; set; }
        public int? TargetUloId { get; set; }
        public virtual UnliquidatedObligation TargetUlo { get; set; }
        public string OwnerUserId { get; set; }
        public virtual AspNetUser OwnerUser { get; set; }

    }

    public class WorkflowDefinition
    {
        public int WorkflowDefinitionId { get; set; }
        public string WorkflowKey { get; set; }
        public int Version { get; set; }

        public string DescriptionJson { get; set; }
    }

    public partial class UnliquidatedObligation
    {
        public int UloId { get; set; }
        public string FieldS0 { get; set; }
        public string FieldS1 { get; set; }
        public string FieldS2 { get; set; }
    }

    public partial class ULODBEntities
    {
        public virtual DbSet<Workflow> Workflows { get; set; }
        public virtual DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
        public virtual DbSet<UnliquidatedObligation> UnliquidatedObligations { get; set; }
        
    }
}
