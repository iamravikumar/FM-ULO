using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class UsersModel
    {
        public List<SelectListItem> Regions { get; set; }


        public int RegionId { get; set; }
        public List<UserModel> Users { get; set; }
        public UsersModel(List<int> regions, List<UserModel> userData)
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
                regionsSelect.Add(new SelectListItem { Text = region.ToString(), Value = region.ToString() });
            }
            return regionsSelect;

        }

    }
    public class UserModel
    {
        private AspNetUser u;
        private List<AspnetUserApplicationPermissionClaim> applicationPermissionRegionPermissionClaims;
        private List<AspnetUserSubjectCategoryClaim> subjectCategoryPermissionClaims;

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
        public UserModel(AspNetUser user, List<AspnetUserApplicationPermissionClaim> regionApplicationPermissionClaims, List<AspnetUserSubjectCategoryClaim> subjectCategoryClaims, List<int> otherRegions, int regionId)
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

            Groups = user.UserUsers.Where(uu => uu.RegionId == regionId).Select(uu => uu.AspNetUser1.UserName).ToList();
            OtherRegions = otherRegions;
        }

    }

    public class OtherRegionInfo
    {
        public string UserId { get; set; }
        public int RegionId { get; set; }

        public OtherRegionInfo()
        {

        }

        public OtherRegionInfo(string userId, int regionId)
        {
            UserId = userId;
            RegionId = regionId;
        }
    }

    public class EditUserBodyModel
    {
        public string UserId { get; set; }
        public List<EditApplicationPermissionClaimModel> ApplicationPermissionClaims { get; set; }
        public List<EditSubjectPermissionClaimModel> SubjectCategoryClaims { get; set; }
        public List<EditGroupsModel> Groups { get; set; }

        public EditUserBodyModel()
        {

        }
        public EditUserBodyModel(AspNetUser user, List<AspnetUserApplicationPermissionClaim> applicationPermissionClaims, List<AspnetUserSubjectCategoryClaim> subjectCategoryClaims, List<string> applicationPermissionClaimNames, List<string> subjectCategoryPermissionClaimNames, List<AspNetUser> groups, int regionId)
        {
            UserId = user.Id;
            var usersApplicationPermissions = applicationPermissionClaims.Select(ac => ac.PermissionName);
            ApplicationPermissionClaims = applicationPermissionClaimNames.Select(c => new EditApplicationPermissionClaimModel(c, usersApplicationPermissions.Contains(c))).ToList();
            var subjectCategorySelectList = subjectCategoryPermissionClaimNames.ConvertToSelectList();
            SubjectCategoryClaims = subjectCategoryClaims.Select(scc => new EditSubjectPermissionClaimModel(scc, subjectCategorySelectList)).ToList();
            var usersGroups = user.UserUsers.Where(uu => uu.RegionId == regionId).Select(uu => uu.AspNetUser1.UserName).ToList();
            Groups = groups.Select(g => new EditGroupsModel(g.UserName, g.Id, usersGroups.Contains(g.UserName))).ToList();
        }

    }


    public class EditUserModel {

        public string UserName { get;  set;}

        public EditUserBodyModel Body { get; set; }

        public EditUserModel()
        {
            UserName = "";
            Body = new EditUserBodyModel();
        }

        public EditUserModel(AspNetUser user, List<AspnetUserApplicationPermissionClaim> applicationPermissionClaims, List<AspnetUserSubjectCategoryClaim> subjectCategoryClaims, List<string> applicationPermissionClaimNames, List<string> subjectCategoryPermissionClaimNames, List<AspNetUser> groups, int regionId)
        {
            UserName = user.UserName;
            Body = new EditUserBodyModel(user, applicationPermissionClaims, subjectCategoryClaims, applicationPermissionClaimNames, subjectCategoryPermissionClaimNames, groups, regionId);
        }

    }

    public class CreateUserModel
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public List<EditApplicationPermissionClaimModel> ApplicationPermissionClaims { get; set; }
        public List<EditSubjectPermissionClaimModel> SubjectCategoryClaims { get; set; }
        public List<EditGroupsModel> Groups { get; set; }

        public CreateUserModel()
        {

        }
        public CreateUserModel(List<string> applicationPermissionClaimNames, List<string> subjectCategoryPermissionClaimNames, List<AspNetUser> groups)
        {
            ApplicationPermissionClaims = applicationPermissionClaimNames.Select(c => new EditApplicationPermissionClaimModel(c, false)).ToList();
            var subjectCategorySelectList = subjectCategoryPermissionClaimNames.ConvertToSelectList();
            SubjectCategoryClaims = new List<EditSubjectPermissionClaimModel>()
            {
                new EditSubjectPermissionClaimModel(new AspnetUserSubjectCategoryClaim
                    {
                        BACode = "",
                        OrgCode = "",
                        DocumentType = ""
                    },
                    subjectCategorySelectList)
            };
            Groups = groups.Select(g => new EditGroupsModel(g.UserName, g.Id, false)).ToList();
        }

    }

    public class EditApplicationPermissionClaimModel
    {
        public string ApplicationPermission { get; set; }

        public bool Selected { get; set; }

        public EditApplicationPermissionClaimModel()
        {

        }

        public EditApplicationPermissionClaimModel(string applicationPermission, bool selected)
        {
            ApplicationPermission = applicationPermission;
            Selected = selected;
        }
    }

    public class EditGroupsModel
    {
        public string GroupName { get; set; }
        public string GroupId { get; set; }
        public bool Selected { get; set; }

        public EditGroupsModel(string groupName, string groupId, bool selected)
        {
            GroupName = groupName;
            GroupId = groupId;
            Selected = selected;
        }
    }

    public class EditSubjectPermissionClaimModel
    {
        public string DocType { get; set; }
        public string BACode { get; set; }
        public string OrgCode { get; set; }
        public List<SelectListItem> DocTypes { get; set; }

        public EditSubjectPermissionClaimModel(List<SelectListItem> docTypes)
        {
            DocTypes = docTypes;
        }

        public EditSubjectPermissionClaimModel(AspnetUserSubjectCategoryClaim subjectCategoryClaim, List<SelectListItem> docTypes)
        {
            DocType = subjectCategoryClaim.DocumentType;
            BACode = subjectCategoryClaim.BACode;
            OrgCode = subjectCategoryClaim.OrgCode;
            DocTypes = docTypes;
        }
    }

    public class EditUserPostData
    {
        [JsonProperty]
        public List<string> ApplicationPermissionNames { get; set; }
        [JsonProperty]
        public List<PostSubjectCategoryClaim> SubjectCategoryClaims { get; set; }
        [JsonProperty]
        public List<string> GroupIds { get; set; }
        [JsonProperty]
        public string UserId { get; set; }
        [JsonProperty]
        public int RegionId { get; set; }

    }

    public class CreateUserPostData
    {
        [JsonProperty]
        public string UserName { get; set; }
        [JsonProperty]
        public string UserEmail { get; set; }
        [JsonProperty]
        public List<string> ApplicationPermissionNames { get; set; }
        [JsonProperty]
        public List<PostSubjectCategoryClaim> SubjectCategoryClaims { get; set; }
        [JsonProperty]
        public List<string> GroupIds { get; set; }
        [JsonProperty]
        public int RegionId { get; set; }

        

    }

    [JsonObject]
    public class PostSubjectCategoryClaim
    {
        [JsonProperty]
        public string DocType { get; set; }
        [JsonProperty]
        public string BACode { get; set; }
        [JsonProperty]
        public string OrgCode { get; set; }
    }


}