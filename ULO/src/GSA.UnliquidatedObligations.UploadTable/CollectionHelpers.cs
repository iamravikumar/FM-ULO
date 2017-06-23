using GSA.UnliquidatedObligations.Utility.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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

        public static string Format(this IEnumerable e, string sep = "", string format = "{0}")
        {
            if (null == e) return "";
            var sb = new StringBuilder();
            int x = 0;
            foreach (object o in e)
            {
                if (x > 0 && null != sep)
                {
                    sb.Append(sep);
                }
                sb.AppendFormat(format, o, x++);
            }
            return sb.ToString();
        }

        public static string Format<T>(this IEnumerable<T> e, string sep, Func<T, int, string> formatter)
        {
            if (null == e) return "";
            var sb = new StringBuilder();
            int x = 0;
            foreach (T o in e)
            {
                if (x > 0 && null != sep)
                {
                    sb.Append(sep);
                }
                sb.Append(formatter(o, x++));
            }
            return sb.ToString();
        }

        private static Expression NestedProperty(Expression arg, string fieldName)
        {
            var left = fieldName.LeftOf(".");
            var right = StringHelpers.TrimOrNull(fieldName.RightOf("."));
            var leftExp = Expression.Property(arg, left);
            if (right == null) return leftExp;
            return NestedProperty(leftExp, right);
        }

        /// <remarks>http://stackoverflow.com/questions/12284085/sort-using-linq-expressions-expression</remarks>
        public static IQueryable<T> OrderByField<T>(this IQueryable<T> q, string sortColumn, bool asc)
        {
            if (string.IsNullOrEmpty(sortColumn)) return q;
            //Requires.Match(RegexHelpers.Common.CSharpIdentifier, sortColumn, nameof(sortColumn));
            var param = Expression.Parameter(typeof(T), "p");
            var prop = NestedProperty(param, sortColumn);
            var exp = Expression.Lambda(prop, param);
            string method = asc ? "OrderBy" : "OrderByDescending";
            Type[] types = new[] { q.ElementType, exp.Body.Type };
            var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
            return q.Provider.CreateQuery<T>(mce);
        }

        public static List<TOutput> ConvertAll<T, TOutput>(this IEnumerable<T> datas, Func<T, TOutput> converter)
        {
            var ret = new List<TOutput>();
            if (datas != null)
            {
                foreach (var data in datas)
                {
                    ret.Add(converter(data));
                }
            }
            return ret;
        }
    }
}
