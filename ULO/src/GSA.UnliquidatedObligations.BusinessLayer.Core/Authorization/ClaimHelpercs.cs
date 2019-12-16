using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    public static class ClaimHelpercs
    {
        private static List<AspNetUserClaim> AspNetUserClaimsList;
        public static HashSet<int> GetApplicationPerimissionRegions(string claimType, string claimValue, ApplicationPermissionNames? applicationPermission)
        {
            HashSet<int> regions = null;
            if (claimType == ApplicationPermissionClaimValue.ClaimType)
            {
                try
                {
                    var ap = ApplicationPermissionClaimValue.Load(claimValue);
                    if (ap != null)
                    {
                        if (applicationPermission != null)
                        {
                            if (ap.ApplicationPermissionName == applicationPermission)
                            {
                                regions = ap.Regions;
                                goto Cleanup;
                            }
                        }
                        else
                        {
                            regions = ap.Regions;
                            goto Cleanup;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
Cleanup:
            return regions ?? new HashSet<int>();
        }

        public static HashSet<int> GetSubjectCategoryRegions(string claimType, string claimValue, string docType, string baCode, string orgCode)
        {
            if (claimType == SubjectCatagoryClaimValue.ClaimType)
            {
                try
                {
                    var ap = SubjectCatagoryClaimValue.Load(claimValue);
                    if (ap != null)
                    {
                        //TODO: Maybe check for wildcards
                        if (ap.DocType == docType && ap.BACode == baCode && ap.OrgCode == orgCode)
                        {
                            return ap.Regions;
                        }
                    }
                }
                catch (Exception)
                { }
            }
            return new HashSet<int>();
        }

        public static HashSet<int> GetApplicationPerimissionRegions(this IEnumerable<Claim> claims, ApplicationPermissionNames? permission)
        {
            var regionIds = new HashSet<int>();
            foreach (var c in claims)
            {
                regionIds.UnionWith(GetApplicationPerimissionRegions(c.Type, c.Value, permission));
            }
            return regionIds;
        }

        public static HashSet<int> GetSubjectCategoryRegions(this IEnumerable<Claim> claims, string docType, string baCode, string orgCode)
        {
            var regionIds = new HashSet<int>();
            foreach (var c in claims)
            {
                regionIds.UnionWith(GetSubjectCategoryRegions(c.Type, c.Value, docType, orgCode, baCode));
            }
            return regionIds;
        }

        public static ICollection<Claim> GetClaims(AspNetUser user)
        {
            //throw new NotImplementedException();

            var claims = new List<Claim>();
            foreach (var c in user.UserAspNetUserClaims)
            {
                claims.Add(new Claim(c.ClaimType, c.ClaimValue, c.ClaimType));
            }
            return claims;

        }

        //public static ICollection<Claim> GetClaims(this List<AspNetUserClaim> userClaims)
        //{
        //    //throw new NotImplementedException();
        //    AspNetUserClaimsList = userClaims;
        //    var claims = new List<Claim>();
        //    foreach (var c in AspNetUserClaimsList)
        //    {
        //        claims.Add(new Claim(c.ClaimType, c.ClaimValue, c.ClaimType));
        //    }           
        //    return claims;

        //}

        public static HashSet<int> GetApplicationPerimissionRegions(this AspNetUser user, ApplicationPermissionNames? permission)
        {
            return GetClaims(user).GetApplicationPerimissionRegions(permission);
        }

        public static HashSet<int> GetSubjectCategoryRegions(this AspNetUser user, string docType, string baCode, string orgCode)
        {
            return GetClaims(user).GetSubjectCategoryRegions(docType, baCode, orgCode);
        }
    }
}
