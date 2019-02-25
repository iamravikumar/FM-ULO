using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web
{
    public class UserHelpers
    {
        private readonly ICacher Cacher;
        private readonly UloDbContext DB;
        private readonly IHttpContextAccessor Acc;

        private readonly PortalHelpers PortalHelpers;

        public UserHelpers(UloDbContext db, ICacher cacher, IHttpContextAccessor acc, PortalHelpers portalHelpers)
        {
            Requires.NonNull(db, nameof(db));
            Requires.NonNull(acc, nameof(acc));
            Requires.NonNull(cacher, nameof(cacher));
            Requires.NonNull(portalHelpers, nameof(portalHelpers));

            DB = db;
            Cacher = cacher;
            Acc = acc;
            PortalHelpers = portalHelpers;
        }

        public bool HasPermission(ApplicationPermissionNames permissionName, IPrincipal user = null)
        {
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
        }

    }
}
