using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.UploadTable
{
    public static class Stuff
    {
        public const string Qbert = "@!#?@!";

        public static CultureInfo CultureThis = CultureInfo.CurrentCulture;

        /// <summary>
        /// Does nothing.  It is simply used as a line where one can set breakpoints
        /// </summary>
        /// <param name="args">Pass in parameters if you don't want them compiled out</param>
        [Conditional("DEBUG")]
        public static void Noop(params object[] args)
        {
        }

        public static string StringTrimOrNull(string s, int? maxLength = null)
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

        public static string Object2String(object o, string nullValue=null)
        {
            return o == null ? nullValue : o.ToString();
        }
    }
}
