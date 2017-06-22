namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public class Justification
    {
        public int JustificationId { get; }
        public string JustificationText { get; }

        public Justification(int justificationId, string justificationText)
        {
            JustificationId = justificationId;
            JustificationText = justificationText;
        }
    }
}
