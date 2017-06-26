namespace GSA.UnliquidatedObligations.Utility.Caching
{
    public interface ICacheEntry
    {
        object Value { get; }
        bool IsExpired { get; }
    }
}
