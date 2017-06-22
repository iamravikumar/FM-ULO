using GSA.UnliquidatedObligations.Utility;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data.Reporting
{
    public class ReportDescription : IReportItemDescription
    {
        private static readonly Regex NonWordChars = new Regex(@"\W", RegexOptions.Compiled | RegexOptions.Singleline);

        public string Name
        {
            get
            {
                var n = Title;
                n = NonWordChars.Replace(n, " ");
                return StringHelpers.ToCamelCase(n);
            }
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string SprocSchema { get; set; }
        public string SprocName { get; set; }
        public IList<ReportParameterDescription> ParameterDescriptions { get; set; }
    }
}
