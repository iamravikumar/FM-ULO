using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GSA.UnliquidatedObligations.Web
{
    public class ExtendedSelectListItem : SelectListItem
    {
        public string Description { get; set; }

        public static string GetDescription(SelectListItem sli, string fallback = null)
            => (sli as ExtendedSelectListItem)?.Description ?? fallback;
    }
}
