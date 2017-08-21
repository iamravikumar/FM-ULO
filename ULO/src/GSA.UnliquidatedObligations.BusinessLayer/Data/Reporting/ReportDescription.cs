using Newtonsoft.Json;
using RevolutionaryStuff.Core;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data.Reporting
{
    public class ReportDescription : IReportItemDescription
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
            => $"Title={this.Title} Sproc={SprocSchema}.{SprocName}";

        private static readonly Regex NonWordChars = new Regex(@"\W", RegexOptions.Compiled | RegexOptions.Singleline);

        [JsonIgnore]
        public string Name
        {
            get
            {
                var n = Title ?? "";
                n = NonWordChars.Replace(n, " ");
                return n.ToUpperCamelCase();
            }
        }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("sprocSchema")]
        public string SprocSchema { get; set; }

        [JsonProperty("sprocName")]
        public string SprocName { get; set; }

        [JsonProperty("parameters")]
        public IList<ReportParameterDescription> Parameters { get; set; }
    }
}
