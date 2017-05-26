using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    public static class ClaimHelpercs
    {
        public static HashSet<int> GetApplicationPerimissionRegions(string claimType, string claimValue, ApplicationPermissionNames? applicationPermission)
        {
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
                                return ap.Regions;
                            }
                        }
                        else
                        {
                            return ap.Regions;
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                }

            }
            return new HashSet<int>();
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
            HashSet<int> regionIds = new HashSet<int>();
            foreach (var c in claims)
            {
                regionIds.UnionWith(GetApplicationPerimissionRegions(c.Type, c.Value, permission));
            }
            return regionIds;
        }

        public static HashSet<int> GetSubjectCategoryRegions(this IEnumerable<Claim> claims, string docType, string baCode, string orgCode)
        {
            HashSet<int> regionIds = new HashSet<int>();
            foreach (var c in claims)
            {
                regionIds.UnionWith(GetSubjectCategoryRegions(c.Type, c.Value, docType, orgCode, baCode));
            }
            return regionIds;
        }

        public static ICollection<Claim> GetClaims(this AspNetUser user)
        {
            var claims = new List<Claim>();
            foreach (var c in user.AspNetUserClaims)
            {
                claims.Add(new Claim(c.ClaimType, c.ClaimValue, c.ClaimType));
            }

            return claims;
        }

        public static HashSet<int> GetApplicationPerimissionRegions(this AspNetUser user, ApplicationPermissionNames? permission)
        {
            return user.GetClaims().GetApplicationPerimissionRegions(permission);
        }

        public static HashSet<int> GetSubjectCategoryRegions(this AspNetUser user, string docType, string baCode, string orgCode)
        {
            return user.GetClaims().GetSubjectCategoryRegions(docType, baCode, orgCode);
        }


        //public static ICollection<AspNetUserClaim> GetClaimsByRegion(this DbSet<AspNetUser> userClaims, int regionId)
        //{
        //    var region
        //    var applicationClaims = userClaims.Where(uc => uc.GetApplicationPerimissionRegions(null).Contains(re)).;
        //}
    }
}
