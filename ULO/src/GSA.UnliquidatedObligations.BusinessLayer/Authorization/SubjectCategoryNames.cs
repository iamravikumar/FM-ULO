using System.ComponentModel.DataAnnotations;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    public enum SubjectCatagoryNames
    {
        [Display(Name = "UE")]
        UE,
        [Display(Name = "LR")]
        LR,
        [Display(Name = "CL")]
        CL,
        [Display(Name = "EN")]
        EN,
        [Display(Name = "EP")]
        EP,
        [Display(Name = "EQ")]
        EQ,
        [Display(Name = "GP")]
        GP,
        [Display(Name = "UE")]
        IX,
        [Display(Name = "LY")]
        LY,
        [Display(Name = "PJ")]
        PJ,
        [Display(Name = "PN")]
        PN,
        [Display(Name = "PX")]
        PX,
        [Display(Name = "QP")]
        QP,
        [Display(Name = "RB")]
        RB,
        [Display(Name = "RO")]
        RO,
        [Display(Name = "1B")]
        OneB,
        [Display(Name = "LO")]
        LO
    }
}
