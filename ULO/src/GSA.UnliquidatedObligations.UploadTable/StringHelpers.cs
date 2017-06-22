using System.Text.RegularExpressions;

namespace GSA.UnliquidatedObligations.Utility
{
    public static class StringHelpers
    {
        public static string TrimOrNull(string s, int? maxLength = null)
        {
            if (s != null)
            {
                s = s.Trim();
                if (s.Length == 0) s = null;
            }
            if (s != null && maxLength.HasValue)
            {
                s = s.Left(maxLength.Value);
            }
            return s;
        }

        public static string Left(this string s, int firstNChars)
        {
            if (s == null) return null;
            if (s.Length > firstNChars)
            {
                s = s.Substring(0, firstNChars);
            }
            return s;
        }

        private static readonly Regex WhiteSpaceExpr = new Regex("\\s", RegexOptions.Compiled);

        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.ToLower().ToTitleCase();
            s = WhiteSpaceExpr.Replace(s, "");
            return s;
        }

        public static string ToTitleCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return Stuff.CultureThis.TextInfo.ToTitleCase(s);
        }

        /// <summary>
        /// Splits a string on a pivot point
        /// </summary>
        /// <param name="s">The string</param>
        /// <param name="sep">The pivot, this will not be included</param>
        /// <param name="first">When true, use the first instance of the pivot, else use the last</param>
        /// <param name="left">The left side of the pivot point</param>
        /// <param name="right">The right side of the pivot point</param>
        /// <returns>true if a split occurred, else false</returns>
        /// <example>
        /// StringSides("1234567", "34", true, "12", "567")
        /// </example>
        public static bool Split(this string s, string sep, bool first, out string left, out string right)
        {
            int n = first ? s.IndexOf(sep) : s.LastIndexOf(sep);
            left = right = "";
            if (n < 0)
            {
                left = s;
                return false;
            }
            else
            {
                left = s.Substring(0, n);
                right = s.Substring(n + sep.Length);
                return true;
            }
        }

        public static string LeftOf(this string s, string pivot)
        {
            if (s == null) return null;
            string left, right;
            s.Split(pivot, true, out left, out right);
            return left;
        }

        public static string RightOf(this string s, string pivot, bool returnFullStringIfPivotIsMissing = false)
        {
            if (s == null) return null;
            string left, right;
            return s.Split(pivot, true, out left, out right) || !returnFullStringIfPivotIsMissing ? right : s;
        }
    }
}
