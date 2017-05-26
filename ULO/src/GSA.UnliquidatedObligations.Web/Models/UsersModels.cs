using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class UsersModels
    {
        public List<SelectListItem> Regions { get; set; }

        public int RegionId { get; set; }
        public List<UserModel> Users { get; set; }
        public UsersModels(List<int> regions, List<AspNetUser> users, List<AspnetUserApplicationPermissionClaim> regionClaims, List<AspnetUserApplicationPermissionClaim> otherRegionClaims)
        {
            Regions = ConvertToSelectList(regions);
            RegionId = Convert.ToInt32(Regions[0].Value);
            Users = users.Select(u => new UserModel(u, regionClaims)).ToList();
        }

        private List<SelectListItem> ConvertToSelectList(List<int> regions)
        {
            var regionsSelect = new List<SelectListItem>();

            foreach (var region in regions)
            {
                regionsSelect.Add(new SelectListItem {Text = region.ToString(), Value = region.ToString()});
            }
            return regionsSelect;

        }
    }

    public class UserModel
    {
        public string UserName { get; set; }
        public List<string> Claims { get; set; }
        public List<string> Groups { get; set; }
        public UserModel(AspNetUser user, List<AspnetUserApplicationPermissionClaim> regionClaims )
        {
            UserName = user.UserName;
            Claims = regionClaims.Where(c => c.UserId == user.Id).Select(c => c.PermissionName).ToList();
            Groups = user.UserUsers.Select(uu => uu.AspNetUser1.UserName).ToList();
        }
    }
}