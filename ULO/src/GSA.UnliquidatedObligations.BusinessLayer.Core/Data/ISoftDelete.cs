namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public interface ISoftDelete
    {
        void Delete(string deletorUserId = null);
        //bool IsDeleted { get; }
        string DeleteKey { get; }
    }
}
