using GSA.UnliquidatedObligations.Utility.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GSA.UnliquidatedObligations.Utility
{
    public static class CollectionHelpers
    {
        public static V FindOrMissing<K, V>(this IDictionary<K, V> d, K key, V missing)
        {
            V ret;
            if (!d.TryGetValue(key, out ret))
            {
                ret = missing;
            }
            return ret;
        }

        public static IList<T> AsReadOnly<T>(this IEnumerable<T> items)
        {
            if (items == null) return new T[0];
            return new List<T>(items).AsReadOnly();
        }

        public static IDictionary<K, V> AsReadOnly<K, V>(this IDictionary<K, V> dict)
        {
            return new ReadonlyDictionary<K, V>(dict);
        }

        /// <remarks>http://stackoverflow.com/questions/12284085/sort-using-linq-expressions-expression</remarks>
        public static IQueryable<T> OrderByField<T>(this IQueryable<T> q, string sortColumn, bool asc)
        {
            if (string.IsNullOrEmpty(sortColumn)) return q;
            Requires.Match(RegexHelpers.Common.CSharpIdentifier, sortColumn, nameof(sortColumn));
            var param = Expression.Parameter(typeof(T), "p");
            var prop = Expression.Property(param, sortColumn);
            var exp = Expression.Lambda(prop, param);
            string method = asc ? "OrderBy" : "OrderByDescending";
            Type[] types = new[] { q.ElementType, exp.Body.Type };
            var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
            return q.Provider.CreateQuery<T>(mce);
        }
    }
}
