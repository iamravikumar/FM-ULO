using GSA.UnliquidatedObligations.BusinessLayer.Data.Reporting;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class ReportDefinition
    {
        private ReportDescription Description_p;

        public ReportDescription Description
        {
            get
            {
                if (Description_p == null)
                {
                    if (!string.IsNullOrEmpty(DescriptionJson))
                    {
                        try
                        {
                            Description_p = JsonConvert.DeserializeObject<ReportDescription>(DescriptionJson);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                        }
                    }
                    Description_p = Description_p ?? new ReportDescription();
                }
                return Description_p;
            }
        }
    }
}
