using RevolutionaryStuff.Core;
using System.Collections.Generic;
using System.Linq;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class Workflow
    {
        public string GetMostRecentAnswer(ICollection<string> ignorableAnswers = null)
        {
            ignorableAnswers = ignorableAnswers ?? Empty.StringArray;
            foreach (var a in this.UnliqudatedObjectsWorkflowQuestions.OrderByDescending(z => z.UnliqudatedWorkflowQuestionsId))
            {
                if (a.Pending || ignorableAnswers.Contains(a.Answer)) continue;
                return a.Answer;
            }
            return null;
        }

        public string MostRecentNonReassignmentAnswer
        {
            get
            {
                if (!MostRecentNonReassignmentAnswerFound)
                {
                    MostRecentNonReassignmentAnswer_p = GetMostRecentAnswer(UnliqudatedObjectsWorkflowQuestion.CommonAnswers.ReassignmentAnswers);
                    MostRecentNonReassignmentAnswerFound = true;
                }
                return MostRecentNonReassignmentAnswer_p;
            }
        }
        private string MostRecentNonReassignmentAnswer_p;
        private bool MostRecentNonReassignmentAnswerFound;
    }
}
