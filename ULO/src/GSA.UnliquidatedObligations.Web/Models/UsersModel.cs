using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class UsersModel
    {
        public List<SelectListItem> Regions { get; set; }
        

        public int RegionId { get; set; }
        public List<UserModel> Users { get; set; }
        public UsersModel(List<int> regions, List<UserModel> userData )
        {
            Regions = ConvertToSelectList(regions);
            RegionId = Convert.ToInt32(Regions[0].Value);
            Users = userData;
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
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> Claims { get; set; }
        public List<string> Groups { get; set; }
        public List<int> OtherRegions { get; set; }

        public UserModel()
        {
            UserId = null;
            Claims = new List<string>();
        }
        public UserModel(AspNetUser user, List<AspnetUserApplicationPermissionClaim> regionApplicationPermissionClaims, List<AspnetUserSubjectCategoryClaim> subjectCategoryClaims, List<int> otherRegions )
        {
            UserName = user.UserName;
            UserId = user.Id;
            var regionApplicationPermissionClaimsStringList = regionApplicationPermissionClaims.Where(c => c.UserId == user.Id).Select(c => c.PermissionName).ToList();
            var subjectCategoryPermissionClaimsList = subjectCategoryClaims.Where(c => c.UserId == user.Id).ToList();
            var subjectCategoryClaimsStringsList = new List<string>();
            foreach (var subjectCategoryPermission in subjectCategoryPermissionClaimsList)
            {
                subjectCategoryClaimsStringsList.Add($"SC (DT: {subjectCategoryPermission.DocumentType}, BAC: {subjectCategoryPermission.BACode}, OC: {subjectCategoryPermission.OrgCode})");
            }
            Claims = regionApplicationPermissionClaimsStringList.Concat(subjectCategoryClaimsStringsList).ToList();
            Groups = user.UserUsers.Select(uu => uu.AspNetUser1.UserName).ToList();
            OtherRegions = otherRegions;
        }
    }
}