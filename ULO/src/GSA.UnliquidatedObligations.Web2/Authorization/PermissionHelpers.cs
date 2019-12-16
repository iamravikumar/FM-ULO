using System;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace GSA.UnliquidatedObligations.Web.Authorization
{
    public static class PermissionHelpers
    {
        public static string CreateClaimType<TEnum>(TEnum permissionName) where TEnum : System.Enum
        {
            if (permissionName is ApplicationPermissionNames)
            {
                var p = (ApplicationPermissionNames)(object)permissionName;
                return CreateClaimType(p);
            }
            throw new Exception($"Type {typeof(TEnum)} is not supported as a permission type.");
        }

        public static string CreateClaimType(ApplicationPermissionNames permissionName)
            => "ApplicationPermissionClaim";

        public static string CreatePolicyName<TEnum>(TEnum permissionName) where TEnum : System.Enum
        {
            if (permissionName is ApplicationPermissionNames)
            {
                return ApplicationPermissionAuthorize.GetPolicyName((ApplicationPermissionNames)(object)permissionName);
            }
            throw new Exception($"Type {typeof(TEnum)} is not supported as a permission type.");
        }

        public static void UseSitePermissions(this IServiceCollection services)
        {
            services.AddAuthorization(o =>
            {
                foreach (ApplicationPermissionNames p in Enum.GetValues(typeof(ApplicationPermissionNames)))
                {
                    o.AddPolicy(CreatePolicyName(p), policy => policy.AddRequirements(new PermissionRequirement<ApplicationPermissionNames>(p)));
                }
            });
            services.AddTransient<IAuthorizationHandler, PermissionHandler>();

        }
    }
}
