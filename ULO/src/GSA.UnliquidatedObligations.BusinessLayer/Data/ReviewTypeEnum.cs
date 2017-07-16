using System.ComponentModel.DataAnnotations;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public enum ReviewTypeEnum
    {
        [Display(Name = "Semi-Annual", Order = 1)]
        SemiAnnual = 1,

        [Display(Name = "High-Risk", Order = 0)]
        HighRisk = 2
    }
}