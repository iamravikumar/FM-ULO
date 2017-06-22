using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class AspNetUserClaimsData
    {
        public static List<AspNetUserClaim> GenerateData(int listSize, string withUserId )
        {
            var applicationPermission = new ApplicationPermissionClaimValue
            {
                Regions = new HashSet<int>() { 1, 4 },
                ApplicationPermissionName = ApplicationPermissionNames.ApplicationUser
            };

            var canViewOtherWorkflowsClaimValue = new ApplicationPermissionClaimValue
            {
                Regions = new HashSet<int>() { 1, 4 },
                ApplicationPermissionName = ApplicationPermissionNames.CanViewOtherWorkflows
            };

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

            var canCreateReviews = new ApplicationPermissionClaimValue
            {
                Regions = new HashSet<int>() { 1, 4 },
                ApplicationPermissionName = ApplicationPermissionNames.CanCreateReviews
            };
            var scClaimValue = new SubjectCatagoryClaimValue
            {
                Regions = new HashSet<int>() { 1, 4 },
                DocType = "UE",
                BACode = "F40000",
                OrgCode = "G1234"
            };

            var claimType = ApplicationPermissionClaimValue.ClaimType;


            var seralizedCanViewOtherWorkflowsClaimValue = canViewOtherWorkflowsClaimValue.ToXml();
            var serializedCanViewReviewsClaimValue = canViewReviewsClaimValue.ToXml();
            var serializedCanManageUsersClaimValue = canManageUsersClaimValue.ToXml();
            var serializedScClaimValue = scClaimValue.ToXml();
            var serializedApplicationPermission = applicationPermission.ToXml();
            var serializedCreateReviews = canCreateReviews.ToXml();
            var claims = Builder<AspNetUserClaim>
                .CreateListOfSize(listSize)
                .TheFirst(4)
                .With(u => u.UserId = withUserId)
                .Build()
                .ToList();

            claims[0].ClaimValue = serializedCanViewReviewsClaimValue;
            claims[0].ClaimType = ApplicationPermissionClaimValue.ClaimType;
            claims[1].ClaimValue = serializedCanManageUsersClaimValue;
            claims[1].ClaimType = ApplicationPermissionClaimValue.ClaimType;
            claims[2].ClaimValue = serializedScClaimValue;
            claims[2].ClaimType = SubjectCatagoryClaimValue.ClaimType;
            claims[3].ClaimValue = seralizedCanViewOtherWorkflowsClaimValue;
            claims[3].ClaimType = ApplicationPermissionClaimValue.ClaimType;
            return claims;
        }

    }
}