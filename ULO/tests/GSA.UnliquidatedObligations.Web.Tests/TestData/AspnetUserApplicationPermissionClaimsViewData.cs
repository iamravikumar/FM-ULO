using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class AspnetUserApplicationPermissionClaimsViewData
    {
        public static List<AspnetUserApplicationPermissionClaim> GenerateData(List<AspNetUserClaim> claims)
        {
            var viewList = new List<AspnetUserApplicationPermissionClaim>();
            foreach (var claim in claims)
            {
                var loadedClaim = ApplicationPermissionClaimValue.Load(claim.ClaimValue);
                foreach (var loadedClaimRegion in loadedClaim.Regions)
                {
                   viewList.Add(new AspnetUserApplicationPermissionClaim
                   {
                       PermissionName = Enum.GetName(typeof(ApplicationPermissionNames), loadedClaim.ApplicationPermissionName),
                       Region = loadedClaimRegion,
                       UserId = claim.UserId
                   });
                }
            }
            return viewList;
        }
    }
}