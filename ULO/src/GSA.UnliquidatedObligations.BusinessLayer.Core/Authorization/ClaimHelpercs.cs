using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    public static class ClaimHelpercs
    {
        public const string UloClaimTypePrefix = UloHelpers.UloUrn + "claims/";

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

        public static bool HasPermission(this System.Security.Principal.IPrincipal user, ApplicationPermissionNames permissionName)
        {
            if (!user.Identity.IsAuthenticated) return false;
            var cp = user as System.Security.Claims.ClaimsPrincipal;
            return cp != null && cp.Claims.GetApplicationPerimissionRegions(permissionName).Count > 0;
        }

        public static HashSet<int> GetApplicationPerimissionRegions(this System.Security.Principal.IPrincipal user, ApplicationPermissionNames? permission)
        {
            var cp = user as ClaimsPrincipal;
            if (cp == null) return new HashSet<int>();
            return cp.Claims.GetApplicationPerimissionRegions(permission);
        }
    }
}
