namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public interface IActivityChooser
    {
        string GetNextActivityKey(Data.Workflow wf, string settings);
    }
}
