using System.Text.RegularExpressions;

namespace GSA.UnliquidatedObligations.Utility
{
    public static class RegexHelpers
    {
        public static class Common
        {
            public static readonly Regex CSharpIdentifier = new Regex(@"^[a-z][a-z0-9_]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            /// <remarks>
            /// Conforms to RCF 2822 
            /// http://dubinko.info/writing/xforms/book.html#id2848057
            /// </remarks>
            public static readonly Regex EmailAddress = new Regex(@"[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+(\.[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+)*@[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+(\.[A-Za-z0-9!#-'\*\+\-/=\?\^_`\{-~]+)*", RegexOptions.Compiled);
        }
    }
}