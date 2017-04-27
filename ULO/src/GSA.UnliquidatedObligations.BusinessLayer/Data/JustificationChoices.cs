using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{

    public static class JustificationChoices
    {
        public static readonly Dictionary<JustificationEnum, Justification> Choices = new Dictionary<JustificationEnum, Justification>()
        {

            {
                JustificationEnum.ContractNotComplete,
                new Justification(Convert.ToInt32(JustificationEnum.ContractNotComplete),
                    "The Contract is not complete and work is on-going")
            },
            {
                JustificationEnum.ServicePeriodNotExpired,
                new Justification(Convert.ToInt32(JustificationEnum.ServicePeriodNotExpired), "The service period has not Expired")
            },
            {
                JustificationEnum.ContractorFiledClaim,
                new Justification(Convert.ToInt32(JustificationEnum.ContractorFiledClaim), "Contractor has filed claim against GSA")
            },
            {
                JustificationEnum.WatingOnRelease,
                new Justification(Convert.ToInt32(JustificationEnum.WatingOnRelease), "Waiting on release of claims from vendor")
            },
            {
                JustificationEnum.NoRecentActivity, 
                new Justification(Convert.ToInt32(JustificationEnum.NoRecentActivity), "There has been no recent activity but a notice to proceed will be issued")
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
                JustificationEnum.Other, 
                new Justification(Convert.ToInt32(JustificationEnum.Other), "Other")
            }
    };
}


}
