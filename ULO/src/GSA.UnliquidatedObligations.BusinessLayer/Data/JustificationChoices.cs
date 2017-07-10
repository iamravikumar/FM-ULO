using System;
using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{

    public static class JustificationChoices
    {
        public static readonly Dictionary<JustificationEnum, Justification> Choices = new Dictionary<JustificationEnum, Justification>()
        {

            {
                JustificationEnum.ContractNotComplete,
                new Justification(Convert.ToInt32(JustificationEnum.ContractNotComplete),
                    "The contract is not complete and work is on-going until (Date of Expected Completion). There has been no recent financial activity because ....(please explain the reason for the lack of financial activity)")
            },
            {
                JustificationEnum.ServicePeriodNotExpired,
                new Justification(Convert.ToInt32(JustificationEnum.ServicePeriodNotExpired), "The service period has not expired. There has been no recent financial activity because ....(please explain the reason for the lack of financial activity).")
            },
            {
                JustificationEnum.ContractorFiledClaim,
                new Justification(Convert.ToInt32(JustificationEnum.ContractorFiledClaim), "Contractor has filed claim against GSA (Please provide claim #)")
            },
            {
                JustificationEnum.WatingOnRelease,
                new Justification(Convert.ToInt32(JustificationEnum.WatingOnRelease), "Waiting on release of claims from vendor (if this is your justification, please provide the last communication date between the contracting officer and the vendor- if email exists - please provide as documentation)")
            },
            {
                JustificationEnum.NoRecentActivity, 
                new Justification(Convert.ToInt32(JustificationEnum.NoRecentActivity), "TThere has been no recent financial activity, the project was delayed because_____ (please provide explanation); A notice to proceed will be issued within the next # days")
            },
            {
                JustificationEnum.ItemInvalid,
                new Justification(Convert.ToInt32(JustificationEnum.ItemInvalid), "Item is invalid and Contracting Officer is working on modification to deobligate the balance")
            },
            {
                JustificationEnum.InvalidRecurringContract, 
                new Justification(Convert.ToInt32(JustificationEnum.InvalidRecurringContract), "Invalid - Recurring Contract - $ not needed - will adjust accordingly")
            },
            {
                JustificationEnum.ValidRecurringContract, 
                new Justification(Convert.ToInt32(JustificationEnum.ValidRecurringContract), "Valid - Recurring Contract - $ needed")
            },
            {
                JustificationEnum.ReassignNeedHelp,
                new Justification(Convert.ToInt32(JustificationEnum.ReassignNeedHelp), "User needs additional help with this")
            },
            {
                JustificationEnum.ReassignVaction,
                new Justification(Convert.ToInt32(JustificationEnum.ReassignVaction), "User is on Vacation this week.")
            },
             {
                JustificationEnum.Other,
                new Justification(Convert.ToInt32(JustificationEnum.Other), "Other")
            },
    };
}


}
