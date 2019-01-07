namespace GSA.UnliquidatedObligations.Web.Models
{
    public class WorkflowListTab
    {
        public static readonly WorkflowListTab[] None = new WorkflowListTab[0];
        public string TabName { get; set; }
        public string TabKey { get; set; }
        public int ItemCount { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsAggregateTab { get; set; }
    }
}
