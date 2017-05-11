using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class AspNetUserClaimsData
    {
        public static List<AspNetUserClaim> GenerateData(int listSize, string withUserId)
        {
            var claimValue = new ApplicationPermissionClaimValue()
            {
                RegionIds = new HashSet<int>() {1, 4},
                ApplicationPermissionName = ApplicationPermissionNames.CanViewOtherWorkflows
            };
            var claimType = ApplicationPermissionClaimValue.ClaimType;

            var serializedClaimValue = JsonConvert.SerializeObject(claimValue);

            return Builder<AspNetUserClaim>
                .CreateListOfSize(listSize)
                .Random(1)
                .With(u => u.UserId = withUserId)
                .With(u => u.ClaimType = claimType)
                .With(u => u.ClaimValue = serializedClaimValue)
                .Build()
                .ToList();
        }
    }
}