using System.Globalization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public class Justification
    {
        public readonly int JustificationId;
        public readonly string JustificationText;

        public Justification(int justificationId, string justificationText)
        {
            JustificationId = justificationId;
            JustificationText = justificationText;
        }
    }
}
