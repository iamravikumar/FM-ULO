using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    public static class ClaimHelpercs
    {
        public static HashSet<int> GetApplicationPerimissionRegions(string claimType, string claimValue, ApplicationPermissionNames applicationPermission)
        {
            if (claimType == ApplicationPermissionClaimValue.ClaimType)
            {
                try
                {
                    var ap = ApplicationPermissionClaimValue.CreateFromJson(claimValue);
                    if (ap != null)
                    {
                        if (ap.ApplicationPermissionName == applicationPermission)
                        {
                            return ap.RegionIds;
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                }

            }
            return new HashSet<int>();
        }

        public static HashSet<int> GetSubjectCategoryRegions(string claimType, string claimValue, SubjectCatagoryNames subjectCategory)
        {
            if (claimType == SubjectCatagoryClaimValue.ClaimType)
            {
                try
                {
                    var ap = SubjectCatagoryClaimValue.CreateFromJson(claimValue);
                    if (ap != null)
                    {
                        if (ap.SubjectCatagoryName == subjectCategory)
                        {
                            return ap.RegionIds;
                        }
                    }
                }
                catch (Exception)
                { }
            }
            return new HashSet<int>();
        }

        private static HashSet<int> GetClaimSubjectRegions<TClaimSubject>(this IEnumerable<Claim> claims, TClaimSubject claimSubject, Func<string, string, TClaimSubject, HashSet<int>> f)
        {
            HashSet<int> regionIds = new HashSet<int>();
            foreach (var c in claims)
            {
                regionIds.UnionWith(f(c.Type, c.Value, claimSubject));
            }
            return regionIds;
        }

        public static HashSet<int> GetApplicationPerimissionRegions(this IEnumerable<Claim> claims, ApplicationPermissionNames permission)
        {
            return claims.GetClaimSubjectRegions(permission, GetApplicationPerimissionRegions);
        }

        public static HashSet<int> GetSubjectCategoryRegions(this IEnumerable<Claim> claims, SubjectCatagoryNames subjectCategory)
        {
            return claims.GetClaimSubjectRegions(subjectCategory, GetSubjectCategoryRegions);
        }

        public static ICollection<Claim> GetClaims(this AspNetUser user)
        {
            var claims = new List<Claim>();
            foreach (var c in user.AspNetUserClaims)
            {
                claims.Add(new Claim(c.ClaimType, c.ClaimValue, c.ClaimType));
            }
            foreach (var r in user.AspNetRoles)
            {
                //TODO: Ask Jason about
                //foreach (var c in r.AspNetRoleClaims)
                //{
                //    claims.Add(new Claim(c.ClaimType, c.ClaimValue, c.ClaimType));
                //}
            }
            return claims;
        }


        public static HashSet<int> GetApplicationPerimissionRegions(this AspNetUser user, ApplicationPermissionNames permission)
        {
            return user.GetClaims().GetApplicationPerimissionRegions(permission);
        }

        public static HashSet<int> GetSubjectCategoryRegions(this AspNetUser user, SubjectCatagoryNames subjectCategory)
        {
            return user.GetClaims().GetSubjectCategoryRegions(subjectCategory);
        }
    }
}
