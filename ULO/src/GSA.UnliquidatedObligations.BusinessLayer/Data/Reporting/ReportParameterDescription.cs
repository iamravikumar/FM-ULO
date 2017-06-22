using System;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data.Reporting
{
    public class ReportParameterDescription : IReportItemDescription
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Type DataType { get; set; }
        public string SqlName { get; set; }
    }
}
