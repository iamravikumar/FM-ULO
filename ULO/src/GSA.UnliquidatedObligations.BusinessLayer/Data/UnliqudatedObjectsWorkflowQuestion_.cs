using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class UnliqudatedObjectsWorkflowQuestion
    {
        public static class CommonAnswers
        {
            public const string RequestForReasssignment = "Request for Reasssignment";
            public const string Reassignment = "Reassignment";
            public const string Reassigned = "Reassigned";

            public const string Valid = "Valid";
            public const string Invalid = "Invalid";

            public static bool IsAnyTypeOfReassignmentAnswer(string answer)
                => answer == RequestForReasssignment || answer == Reassignment || answer == Reassigned;

            public static readonly IList<string> ReassignmentAnswers = new List<string> { RequestForReasssignment, Reassignment, Reassigned }.AsReadOnly();
        }
        public bool IsValid => Answer == CommonAnswers.Valid;

        public bool IsInvalid => Answer == CommonAnswers.Invalid;
    }
}
