using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public UserModel(AspNetUser user, List<AspnetUserApplicationPermissionClaim> regionApplicationPermissionClaims, List<AspnetUserSubjectCategoryClaim> subjectCategoryClaims, List<int> otherRegions)
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

    public class EditUserModel
    {
        public string UserId { get; set; }
        public List<EditApplicationPermissionClaimModel> ApplicationPermissionClaims { get; set; }
        public List<EditSubjectPermissionClaimModel> SubjectCategoryClaims { get; set; }
        public List<EditGroupsModel> Groups { get; set; }

        public EditUserModel()
        {

        }
        public EditUserModel(AspNetUser user, List<AspnetUserApplicationPermissionClaim> applicationPermissionClaims, List<AspnetUserSubjectCategoryClaim> subjectCategoryClaims, List<string> applicationPermissionClaimNames, List<string> subjectCategoryPermissionClaimNames, List<string> groupNames)
        {
            UserId = user.Id;
            var usersApplicationPermissions = applicationPermissionClaims.Select(ac => ac.PermissionName);
            ApplicationPermissionClaims = applicationPermissionClaimNames.Select(c => new EditApplicationPermissionClaimModel(c, usersApplicationPermissions.Contains(c))).ToList();
            var subjectCategorySelectList = ConvertToSelectList(subjectCategoryPermissionClaimNames);
            SubjectCategoryClaims = subjectCategoryClaims.Select(scc => new EditSubjectPermissionClaimModel(scc, subjectCategorySelectList)).ToList();
            var usersGroups = user.UserUsers.Select(uu => uu.AspNetUser1.UserName).ToList();
            Groups = groupNames.Select(g => new EditGroupsModel(g, usersGroups.Contains(g))).ToList();
        }

        public static List<SelectListItem> ConvertToSelectList(List<string> docTypes)
        {
            var docTypesSelect = new List<SelectListItem>();

            foreach (var docType in docTypes)
            {
                docTypesSelect.Add(new SelectListItem { Text = docType, Value = docType });
            }
            return docTypesSelect;

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
        public bool Selected { get; set; }

        public EditGroupsModel(string groupName, bool selected)
        {
            GroupName = groupName;
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


}