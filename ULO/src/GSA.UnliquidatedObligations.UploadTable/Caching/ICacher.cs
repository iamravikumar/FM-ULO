using System;

namespace GSA.UnliquidatedObligations.Utility.Caching
{
    public interface ICacher
    {
        CacheEntry<TVal> FindOrCreate<TVal>(string key, Func<string, CacheEntry<TVal>> creator = null, bool forceCreate = false, TimeSpan? timeout = null);
    }
}
