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
        public static List<AspNetUserClaim> GenerateData(int listSize, string withUserId )
        {
            var canViewReviewsClaimValue = new ApplicationPermissionClaimValue
            {
                Regions = new HashSet<int>() {1, 4},
                ApplicationPermissionName = ApplicationPermissionNames.CanViewReviews
            };

            var canManageUsersClaimValue = new ApplicationPermissionClaimValue
            {
                Regions = new HashSet<int>() { 1, 4 },
                ApplicationPermissionName = ApplicationPermissionNames.ManageUsers
            };
            var scClaimValue = new SubjectCatagoryClaimValue
            {
                Regions = new HashSet<int>() { 1, 4 },
                DocType = "UE",
                BACode = "F40000",
                OrgCode = "G1234"
            };

            var claimType = ApplicationPermissionClaimValue.ClaimType;

            var serializedCanViewReviewsClaimValue = canViewReviewsClaimValue.ToXml();
            var serializedCanManageUsersClaimValue = canManageUsersClaimValue.ToXml();
            var serializedScClaimValue = scClaimValue.ToXml();
            var claims = Builder<AspNetUserClaim>
                .CreateListOfSize(listSize)
                .TheFirst(2)
                .With(u => u.UserId = withUserId)
                .With(u => u.ClaimType = claimType)
                .Build()
                .ToList();

            claims[0].ClaimValue = serializedCanViewReviewsClaimValue;
            claims[1].ClaimValue = serializedCanManageUsersClaimValue;

            return claims;
        }
    }
}