namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public enum JustificationEnum
    {
        ContractNotComplete,
        ServicePeriodNotExpired,
        ContractorFiledClaim,
        WatingOnRelease,
        NoRecentActivity,
        ItemInvalid,
        InvalidRecurringContract,
        ValidRecurringContract,
        ReassignVaction,
        ReassignNeedHelp,
        Other
    }
}