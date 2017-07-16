using System.ComponentModel.DataAnnotations;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public enum ReviewScopeEnum
    {
        [Display(Name = "Region", Order = 0)]
        Region= 1,

        [Display(Name = "Whole Agency", Order = 1)]
        WholeAgency = 2
    }
}