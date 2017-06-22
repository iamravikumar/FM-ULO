using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.Utility
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

        public static string Object2String(object o, string nullValue=null)
        {
            return o == null ? nullValue : o.ToString();
        }
    }
}
