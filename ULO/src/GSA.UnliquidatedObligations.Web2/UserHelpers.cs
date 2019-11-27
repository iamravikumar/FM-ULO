using System.Linq;
using System.Collections.Generic;
using System.Security.Principal;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public IList<int?> GetReassignmentGroupRegions(IPrincipal user)
           => GetUserGroupRegions(user, ReassignGroupUserName);

        public IList<int?> GetUserGroupRegions(IPrincipal user, string groupNameOrId)
          => Cacher.FindOrCreateValue(
              Cache.CreateKey(nameof(GetUserGroupRegions), user.Identity.Name, groupNameOrId),
              () =>
                  DB.UserUsers
                          .Where(uu => (uu.ParentUserId == GetUserId(groupNameOrId) || uu.ParentUserId == groupNameOrId) && uu.ChildUserId == GetUserId(user.Identity.Name))
                          .Select(uu => uu.RegionId)
                          .Distinct()
                          .ToList()
                          .AsReadOnly(),
              PortalHelpers.MediumCacheTimeout
              );

        public IList<int?> GetUserGroupRegions(GetMyGroups_Result0 user)
         => Cacher.FindOrCreateValue(
             Cache.CreateKey(nameof(GetUserGroupRegions), user.UserName),
             () =>
                 DB.UserUsers
                         .Where(uu => uu.ChildUserId == user.UserId)
                         .Select(uu => uu.RegionId)
                         .Distinct()
                         .ToList()
                         .AsReadOnly(),
             PortalHelpers.MediumCacheTimeout
             );

        public IList<string> GetUserGroupNames(IPrincipal user, int regionId)
           => Cacher.FindOrCreateValue(
               Cache.CreateKey(nameof(GetUserGroupNames), user.Identity.Name, regionId),
               () =>
                       DB.UserUsers
                           .Where(uu => uu.ChildUserId == GetUserId(user.Identity.Name) && uu.RegionId == regionId)
                           .Select(uu => uu.ParentUser.UserName)
                           .Distinct()
                           .ToList()
                           .AsReadOnly()
                   ,
               PortalHelpers.MediumCacheTimeout
               );       

        public bool HasPermission(ApplicationPermissionNames permissionName, IPrincipal user = null)
        {
            return true;

            //user = user ?? Acc.HttpContext?.User;
            //if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            //{
            //    return Cacher.FindOrCreateValue(
            //        Cache.CreateKey(user.Identity.Name, permissionName),
            //        () =>
            //        {
            //            var claims = DB.AspNetUsers.Include(u => u.UserAspNetUserClaims).AsNoTracking().FirstOrDefault(u => u.UserName == user.Identity.Name)?.GetClaims();
            //            if (claims != null)
            //            {
            //                return claims.GetApplicationPerimissionRegions(permissionName).Count > 0;
            //            }
            //            return false;
            //        }, PortalHelpers.ShortCacheTimeout
            //        );
            //}
            //return false;

        }

        public  IList<SelectListItem> CreateSelectList(IEnumerable<AspNetUser> aspNetUsers)
          => aspNetUsers.Select(z => CreateUserSelectListItem(z.Id, z.UserName)).ToList();

        public  SelectListItem ToSelectListItem(AspNetUser u, bool disabled = false)
            => CreateUserSelectListItem(u.Id, u.UserName, disabled);

        public  SelectListItem CreateUserSelectListItem(string userId, string username, bool disabled = false)
            => new SelectListItem
            {
                Text = username,
                Value = userId,
                Disabled = disabled
            };

        public  IList<SelectListItem> ConvertToSelectList(IEnumerable<string> stringsToConvert)
        {
            var stringsSelect = new List<SelectListItem>();

            foreach (var stringToConvert in stringsToConvert)
            {
                stringsSelect.Add(new SelectListItem { Text = stringToConvert, Value = stringToConvert });
            }
            return stringsSelect;
        }

        public  IList<SelectListItem> ConvertToSelectList(IEnumerable<SelectListItem> selectListItems)
        {
            var selectList = new List<SelectListItem>();

            foreach (var selectListItem in selectListItems)
            {
                selectList.Add(selectListItem);
            }
            return selectList;
        }

        public  IList<SelectListItem> ConvertToSelectList(IEnumerable<int> nums)
        {
            var numsSelect = new List<SelectListItem>();

            foreach (var num in nums)
            {
                numsSelect.Add(new SelectListItem { Text = num.ToString(), Value = num.ToString() });
            }
            return numsSelect;
        }



    }
}
