using System.ComponentModel.DataAnnotations;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public enum ReviewScopeEnum
    {
        [Display(Name = "Region")]
        Region= 1,

        [Display(Name = "Whole Agency")]
        WholeAgency = 2
    }
}