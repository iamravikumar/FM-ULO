using GSA.UnliquidatedObligations.Utility.Crypto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.Utility.Caching
{
    public static class Cache
    {
        public static string CreateKey(params object[] args)
        {
            var sb = new StringBuilder();
            for (int pos = 0; pos < args.Length; ++pos)
            {
                object o = args[pos];
                if (o == null || o is string)
                {
                    Stuff.Noop();
                }
                else if (o is bool)
                {
                    o = (bool)o ? 1 : 0;
                }
                else if (o.GetType().GetTypeInfo().IsEnum)
                {
                    o = Convert.ToUInt64(o);
                }
                else if (o is IEnumerable)
                {
                    o = (o as IEnumerable).Format(",");
                }
                else if (o is Type)
                {
                    o = ((Type)o).FullName;
                }
                else if (o is TimeSpan)
                {
                    o = ((TimeSpan)o).TotalMilliseconds;
                }
                sb.AppendFormat("{0}: {1}\n", pos, o);
            }
            return CanonicalizeCacheKey(sb.ToString());
        }

        private static string CanonicalizeCacheKey(string key)
        {
            if (key == null) return "special:__NULL";
            if (key.Length < 123) return "lit:" + key;
            byte[] buf = Encoding.UTF8.GetBytes(key);
            return string.Format("urn:crc32:{0}{1}", CRC32Checksum.Do(buf), key.GetHashCode());
        }

        private static readonly IDictionary<int, object> LockByKey = new Dictionary<int, object>();

        private static int GetLockKeyName(object cacheGuy, object key) => (cacheGuy.GetHashCode() ^ (key ?? "").GetHashCode()) & 0x0FFF;


        public class BasicCacher : ICacher
        {
            private readonly IDictionary<string, ICacheEntry> EntriesByKey = new Dictionary<string, ICacheEntry>();

            public CacheEntry<TVal> FindOrCreate<TVal>(string key, Func<string, CacheEntry<TVal>> creator, bool forceCreate, TimeSpan? timeout)
            {
                ICacheEntry e = null;
                if (forceCreate || !EntriesByKey.TryGetValue(key, out e) || e.IsExpired)
                {
                    if (creator != null)
                    {
                        e = creator(key);
                        EntriesByKey[key] = e;
                    }
                }
                return e as CacheEntry<TVal>;
            }
        }

        private class PassthroughCacher : ICacher
        {
            CacheEntry<TVal> ICacher.FindOrCreate<TVal>(string key, Func<string, CacheEntry<TVal>> creator, bool forceCreate, TimeSpan? timeout) => creator(key);
        }

        public static readonly ICacher DataCacher = new SynchronizedCacher(new BasicCacher());

        public static readonly ICacher Passthrough = new PassthroughCacher();

        public class SynchronizedCacher : ICacher
        {
            private readonly ICacher Inner;

            public SynchronizedCacher(ICacher inner)
            {
                Requires.NonNull(inner, nameof(inner));
                Inner = inner;
            }

            CacheEntry<TVal> ICacher.FindOrCreate<TVal>(string key, Func<string, CacheEntry<TVal>> creator, bool forceCreate, TimeSpan? timeout)
            {
                var lockName = GetLockKeyName(Inner, key);
                Start:
                object o;
                lock (LockByKey)
                {
                    if (!LockByKey.TryGetValue(lockName, out o))
                    {
                        o = new object();
                        LockByKey[lockName] = o;
                    }
                    if (Monitor.TryEnter(o)) goto Run;
                }
                Monitor.Enter(o);
                Monitor.Exit(o);
                goto Start;
                Run:
                try
                {
                    return Inner.FindOrCreate(key, creator, forceCreate, timeout);
                }
                finally
                {
                    lock (LockByKey)
                    {
                        LockByKey.Remove(lockName);
                    }
                    Monitor.Exit(o);
                }
            }
        }

        private class ScopedCacher : ICacher
        {
            private readonly ICacher Inner;
            private readonly string ScopeKey;

            public ScopedCacher(ICacher inner, params object[] keyParts)
            {
                Requires.NonNull(inner, nameof(inner));
                Inner = inner;
                ScopeKey = CreateKey(keyParts);
            }

            public CacheEntry<TVal> FindOrCreate<TVal>(string key, Func<string, CacheEntry<TVal>> creator, bool forceCreate, TimeSpan? timeout = null)
                => Inner.FindOrCreate(CreateKey(key, ScopeKey), creator, forceCreate, timeout);
        }

        public static ICacher Synchronized(ICacher inner)
            => inner as SynchronizedCacher ?? new SynchronizedCacher(inner);

        public static ICacher CreateScope(this ICacher inner, params object[] keyParts)
            => new ScopedCacher(inner, keyParts);

        public static Task<CacheEntry<TVal>> FindOrCreateAsync<TVal>(this ICacher inner, string key, Func<string, Task<CacheEntry<TVal>>> asynccreator)
            => Task.FromResult(inner.FindOrCreate(key, k => asynccreator(k).ExecuteSynchronously()));

        public static TVal FindOrCreateValWithSimpleKey<TVal>(this ICacher inner, object key, Func<TVal> creator, TimeSpan? expiresIn = null)
            => inner.FindOrCreate(
                CreateKey(typeof(TVal), key),
                _ => new CacheEntry<TVal>(creator(), expiresIn)
                ).Value;

        public static async Task<TVal> FindOrCreateValWithSimpleKeyAsync<TVal>(this ICacher inner, object key, Func<Task<TVal>> asynccreator, TimeSpan? expiresIn=null)
            => (await inner.FindOrCreateAsync(
                CreateKey(typeof(TVal), key),
                async _ => new CacheEntry<TVal>(await asynccreator(), expiresIn)
                )).Value;
    }
}
