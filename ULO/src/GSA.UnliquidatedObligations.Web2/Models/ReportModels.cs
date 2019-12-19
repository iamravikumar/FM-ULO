using System.Collections.Generic;
using GSA.UnliquidatedObligations.BusinessLayer.Data.Reporting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class ConfigureReportModel
    {
        public ReportDescription Description { get; set; }
        public List<SelectListItem> RegionItems { get; } = new List<SelectListItem>();
        public List<SelectListItem> ReviewItems { get; } = new List<SelectListItem>();
        public string CurrentUserEmail { get; set; }

        public ConfigureReportModel()
        { }

        public ConfigureReportModel(PortalHelpers portalHelpers, ReportDescription description = null)
        {
            Description = description;
            RegionItems.AddRange(portalHelpers.CreateRegionSelectListItems());
            ReviewItems.AddRange(portalHelpers.CreateReviewSelectListItems());
        }
    }
}
