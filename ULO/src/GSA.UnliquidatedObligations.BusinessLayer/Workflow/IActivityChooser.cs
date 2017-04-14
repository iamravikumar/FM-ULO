using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public interface IActivityChooser
    {
        string GetNextActivityKey(Data.Workflow wf, UnliqudatedObjectsWorkflowQuestion querstion, string settings);
    }
}
