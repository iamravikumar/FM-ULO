using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.Web.Permission
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string claimType, ApplicationPermissionNames permissionName)
        {
            ClaimType = claimType;
            PermissionName = permissionName;
        }

        public string ClaimType { get; protected set; }
        public ApplicationPermissionNames PermissionName { get; protected set; }

        
    }
}
