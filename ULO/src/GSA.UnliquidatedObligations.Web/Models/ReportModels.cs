using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Data.Reporting;
using System.Collections.Generic;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class ConfigureReportModel
    {
        public ReportDescription Description { get; set; }
        public List<SelectListItem> RegionItems { get; } = new List<SelectListItem>();
        public List<SelectListItem> ReviewItems { get; } = new List<SelectListItem>();

        public ConfigureReportModel()
        { }

        public ConfigureReportModel(ULODBEntities db, ReportDescription description=null)
        {
            Description = description;
            RegionItems.AddRange(PortalHelpers.CreateRegionSelectListItems());
            ReviewItems.AddRange(PortalHelpers.CreateReviewSelectListItems());
        }
    }
}
