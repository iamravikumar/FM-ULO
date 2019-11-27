//using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Security.Claims;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Collections.Generic;

//namespace GSA.UnliquidatedObligations.Web
//{
//    public class PermissionRequirement : IAuthorizationRequirement
//    {
//        public IEnumerable<ApplicationPermissionNames> RequiredPermissions { get; }
//        public PermissionRequirement(IEnumerable<ApplicationPermissionNames> requiredpermissions)
//        {
//            RequiredPermissions = requiredpermissions;
//        }

//    }

//    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
//    {

//        private readonly IEnumerable<ApplicationPermissionNames> PermissionRepository;

//        public PermissionHandler(IEnumerable<ApplicationPermissionNames> permissionRepository)
//        {
//            if (permissionRepository == null)
//                throw new ArgumentNullException(nameof(permissionRepository));

//            PermissionRepository = permissionRepository;
//        }

//        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
//        {
//            bool hasPermission = false;


//            foreach (var p in requirement.RequiredPermissions)
//            {
//                hasPermission = HasPermission(context.User, p);
//            }

//            if (hasPermission)
//            {
//                context.Succeed(requirement);
//            }
//            else
//            {
//                context.Fail();
//            }

//            return Task.CompletedTask;
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


