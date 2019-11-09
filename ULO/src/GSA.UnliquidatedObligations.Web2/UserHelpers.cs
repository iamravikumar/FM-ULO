using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web
{
    public class UserHelpers
    {
        private readonly ICacher Cacher;
        private readonly IOptions<Config> ConfigOptions;
        private readonly UloDbContext DB;
        private readonly IHttpContextAccessor Acc;

        private readonly PortalHelpers PortalHelpers;

        public class Config
        {
            public const string ConfigSectionName = "UserHelpersConfig";
            public string PreAssignmentUserUsername { get; set; }
            public string ReassignGroupUserName { get; set; }
        }

        public UserHelpers(IOptions<Config> configOptions, UloDbContext db, ICacher cacher, IHttpContextAccessor acc, PortalHelpers portalHelpers)
        {
            Requires.NonNull(db, nameof(db));
            Requires.NonNull(acc, nameof(acc));
            Requires.NonNull(cacher, nameof(cacher));
            Requires.NonNull(portalHelpers, nameof(portalHelpers));
            ConfigOptions = configOptions;
            DB = db;
            Cacher = cacher;
            Acc = acc;
            PortalHelpers = portalHelpers;
        }

        public string GetCurrentUserName(IPrincipal user = null)
        {
            user = user ?? Acc.HttpContext?.User;
            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                return user.Identity.Name;
            }
            return null;
        }

        public string CurrentUserName
            => GetCurrentUserName();

        public string CurrentUserId
        {
            get
            {
                var name = CurrentUserName;
                if (string.IsNullOrEmpty(name)) return null;
                return Cacher.FindOrCreateValue(
                    Cache.CreateKey(nameof(CurrentUserId), name),
                    () =>
                    {
                        var z = DB.AspNetUsers.AsNoTracking().FirstOrDefault(u => u.UserName == name);
                        return z?.Id;
                    });
            }
        }

        public string GetUserId(string username)
        {
            if (username != null)
            {
                return Cacher.FindOrCreateValue(
                    username,
                    () => DB.AspNetUsers.Where(z => z.UserName == username).Select(z => z.Id).FirstOrDefault(),
                    PortalHelpers.MediumCacheTimeout
                    );
            }
            return null;
        }

        public string PreAssignmentUserUserId
            => GetUserId(ConfigOptions.Value.PreAssignmentUserUsername);

        public string ReassignGroupUserId
            => GetUserId(ReassignGroupUserName);

        public string ReassignGroupUserName
            => ConfigOptions.Value.ReassignGroupUserName;

        public bool HasPermission(ApplicationPermissionNames permissionName, IPrincipal user = null)
        {
            return true;
            /*
            user = user ?? Acc.HttpContext?.User;
            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                return Cacher.FindOrCreateValue(
                    Cache.CreateKey(user.Identity.Name, permissionName),
                    () =>
                    {
                        var claims = DB.AspNetUsers.Include(u => u.UserAspNetUserClaims).AsNoTracking().FirstOrDefault(u => u.UserName == user.Identity.Name)?.GetClaims();
                        if (claims != null)
                        {
                            return claims.GetApplicationPerimissionRegions(permissionName).Count > 0;
                        }
                        return false;
                    }, PortalHelpers.ShortCacheTimeout
                    );
            }
            return false;
            */
        }

    }
}
