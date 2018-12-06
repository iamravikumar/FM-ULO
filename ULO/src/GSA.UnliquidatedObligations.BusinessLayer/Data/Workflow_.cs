using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.EncoderDecoders;
using System.Collections.Generic;
using System.Linq;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class Workflow
    {
        private string GetMostRecentAnswer(bool? isReal, bool? isReassignment)
        {
            foreach (var a in this.UnliqudatedObjectsWorkflowQuestions.OrderByDescending(z => z.UnliqudatedWorkflowQuestionsId))
            {
                if (a.Pending) continue;
                if (isReal.HasValue && isReal.Value == a.IsAnswerReal) return a.Answer;
                if (isReassignment.HasValue && isReassignment.Value == a.IsAnswerReassignment) return a.Answer;
            }
            return null;
        }

        public string MostRecentRealAnswer
        {
            get
            {
                if (!MostRecentRealAnswerFound)
                {
                    MostRecentRealAnswer_p = GetMostRecentAnswer(true, null);
                    MostRecentRealAnswerFound = true;
                }
                return MostRecentRealAnswer_p;
            }
        }
        private string MostRecentRealAnswer_p;
        private bool MostRecentRealAnswerFound;

        public string MostRecentNonReassignmentAnswer
        {
            get
            {
                if (!MostRecentNonReassignmentAnswerFound)
                {
                    MostRecentNonReassignmentAnswer_p = GetMostRecentAnswer(null, false);
                    MostRecentNonReassignmentAnswerFound = true;
                }
                return MostRecentNonReassignmentAnswer_p;
            }
        }
        private string MostRecentNonReassignmentAnswer_p;
        private bool MostRecentNonReassignmentAnswerFound;

        public string WorkflowRowVersionString
            => Base16.Encode(this.WorkflowRowVersion);
    }
}
