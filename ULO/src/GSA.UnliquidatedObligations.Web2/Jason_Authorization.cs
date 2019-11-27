//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.Extensions.DependencyInjection;

//namespace GSA.UnliquidatedObligations.Web
//{
//    public class Jason_Authorization
//    {
//        public static void AddPermissions(this IServiceCollection services)
//        {
//            services.AddAuthorization(o =>
//            {

//                foreach (ApplicationPermissionNames p in Enum.GetValues(typeof(ApplicationPermissionNames)))
//                {
//                    o.AddPolicy(CreatePolicyName(p), policy => policy.AddRequirements(new PermissionRequirement<ApplicationPermissionNames>(p)));
//                }

//            });
//            services.AddTransient<IAuthorizationHandler, PermissionHandler>();
//        }



//        public class PermissionHandler : AuthorizationHandler<PermissionRequirementBase>
//        {
//            private readonly IAppFinderService AppFinderService;

//            public PermissionHandler(IAppFinderService appFinderService)
//            {
//                Requires.NonNull(appFinderService, nameof(appFinderService));
//                AppFinderService = appFinderService;
//            }

//            protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirementBase requirement)
//            {
//                var claims = context.User.FindAll(c => c.Type == requirement.ClaimType).ToList();
//                if (claims.Count != 0)
//                {
//                    foreach (var c in claims)
//                    {
//                        var pcv = PermissionClaimValue.CreateFromJson(c.Value);
//                        if (pcv.Granted && pcv.AppId == AppFinderService.AppId)
//                        {
//                            context.Succeed(requirement);
//                            break;
//                        }
//                    }
//                }
//            }
//        }

//        public abstract class PermissionRequirementBase : IAuthorizationRequirement
//        {
//            protected PermissionRequirementBase(string claimType, string permissionName)
//            {
//                ClaimType = claimType;
//                PermissionName = permissionName;
//            }

//            public string ClaimType { get; protected set; }
//            public string PermissionName { get; protected set; }

//            public override string ToString() => $"{GetType().Name} permission={PermissionName} claimType=[{ClaimType}]";
//        }

//        public class PermissionRequirement<T> : PermissionRequirementBase
//            where T : struct
//        {
//            public PermissionRequirement(T permission)
//                : base(PermissionHelpers.CreateClaimType(permission), permission.ToString())
//            {
//            }
//        }

//        public static string CreateClaimType(ApplicationPermissionNames permissionName)
//        => $"urn:gsa.gov/unliquidatedObligation/claims/ApplicationPermissionClaim";

//        protected static string CreatePolicyName(string policyNamePrefix, Type enumType, object enumValue)
//        {
//            if (!enumType.IsEnum)
//            {
//                throw new Exception($"Type {enumType} must be an enum for {nameof(PermissionAuthorizeAttributeBase)} attribute");
//            }
//            if (!Enum.TryParse(enumType, enumValue.ToString(), true, out var e))
//            {
//                throw new Exception($"Object {enumValue} must be of Type {enumType} for {nameof(PermissionAuthorizeAttributeBase)} attribute");
//            }
//            policyNamePrefix = policyNamePrefix ?? nameof(PermissionAuthorizeAttributeBase);
//            var result = $"{policyNamePrefix}.{e}";
//            return result;
//        }

//    }
//}
