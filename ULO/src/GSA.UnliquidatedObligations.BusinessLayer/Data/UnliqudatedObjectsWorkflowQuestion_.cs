namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class UnliqudatedObjectsWorkflowQuestion
    {
        public static class CommonAnswers
        {
            public const string RequestForReasssignment = "Request for Reasssignment";
            public const string Reassignment = "Reassignment";

            public static bool IsAnyTypeOfReassignmentAnswer(string answer)
                => answer == RequestForReasssignment || answer == Reassignment;
        }
        public bool IsValid => Answer == "Valid";

        public bool IsInvalid => Answer == "Invalid";
    }
}
