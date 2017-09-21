namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class UnliqudatedObjectsWorkflowQuestion
    {
        public bool IsValid => Answer == "Valid";

        public bool IsInvalid => Answer == "Invalid";
    }
}
