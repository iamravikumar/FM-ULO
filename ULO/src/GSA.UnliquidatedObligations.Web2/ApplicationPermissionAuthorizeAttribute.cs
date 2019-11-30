
//using System;
//using System.Security.Claims;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.Extensions.DependencyInjection;
//using GSA.UnliquidatedObligations.BusinessLayer.Data;
//using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;

//namespace GSA.UnliquidatedObligations.Web
//{
//    public class PermissionRequirement : IAuthorizationRequirement
//    {
//        public PermissionRequirement(ApplicationPermissionNames permissionName)
//        {
//            //ClaimType = claimType;
//            PermissionName = permissionName;
//        }

//       // public string ClaimType { get; protected set; }
//        public ApplicationPermissionNames PermissionName { get; protected set; }


//    }

//    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
//    {
//        private readonly IServiceProvider ServiceProvider;
//        public PermissionHandler(IServiceProvider serviceProvider)
//        {
//            ServiceProvider = serviceProvider;
//        }

//        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
//        {
//            var UserManager = ServiceProvider.GetRequiredService<UserManager<AspNetUser>>();

//            var user = await UserManager.GetUserAsync(context.User);

//            var claimList = (await UserManager.GetClaimsAsync(user)).Select(p => p.Value);

//            foreach (var currentClaim in claimList)
//            {
//                var pcv = ApplicationPermissionClaimValue.Load(currentClaim);
//                if (pcv.ApplicationPermissionName == requirement.PermissionName)
//                {
//                    context.Succeed(requirement);
//                    break;
//                }
//            }
//        }

//        public static bool HasPermission(ClaimsPrincipal user, ApplicationPermissionNames permission)
//        {
//            try
//            {
//                return user.Claims.GetApplicationPerimissionRegions(permission).Count > 0;
//            }
//            catch (Exception)
//            {
//                return false;
//            }
//        }
//    }

//    ////
//}


