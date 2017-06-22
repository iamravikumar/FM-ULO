using System.ComponentModel.DataAnnotations;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public enum ReviewTypeEnum
    {
        [Display(Name = "Semi-Annual")]
        SemiAnnual = 1,

        [Display(Name = "High-Risk")]
        HighRisk = 2
    }
}