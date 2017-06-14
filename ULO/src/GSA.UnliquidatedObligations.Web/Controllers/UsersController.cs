using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using System.Web.UI.WebControls.Expressions;
using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using System;
using System.Web;
using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.Web.Controllers
{

    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ManageUsers)]
    public class UsersController : BaseController
    {
        private readonly ULODBEntities DB;

        public UsersController(ULODBEntities db, IComponentContext context) : base(context)
        {
            DB = db;
        }

        // GET: Users
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.ManageUsers)]
        public async Task<ActionResult> Index()
        {
            //get user object for user logged in.
            var user = await DB.AspNetUsers.FirstOrDefaultAsync(u => u.UserName == this.User.Identity.Name);

            //get claim Region Ids for user
            var claimRegionIds = user.GetApplicationPerimissionRegions(ApplicationPermissionNames.ManageUsers).ToList();
            var userData = await GetUsersByRegion(claimRegionIds[0]);
            return View(new UsersModel(claimRegionIds, userData));

        }

        public async Task<ActionResult> Search(int regionId, string username = "")
        {
            var usersData = await GetUsersByRegion(regionId);
            return PartialView("_Data", usersData);
        }


        private async Task<List<UserModel>> GetUsersByRegion(int regionId, string username = "")
        {
            //get application permission regions
            var applicationPermissionRegionPermissionClaims =
                DB.AspnetUserApplicationPermissionClaims.Where(c => c.Region.Value == regionId);

            //DB.Database.Log = s => Trace.WriteLine(s);
            //get subject category claims
            var subjectCategoryPermissionClaims =
                DB.AspnetUserSubjectCategoryClaims.Where(c => c.Region.Value == regionId);

            //get userIDClaimREgions
            var userIdsforClaimRegion =
                 applicationPermissionRegionPermissionClaims.Select(c => c.UserId)
                 .Concat(subjectCategoryPermissionClaims.Select(c => c.UserId))
                    .Distinct()
                    .ToList();

            var usersOtherApplicationClaimRegions =
                DB.AspnetUserApplicationPermissionClaims
                    .Where(c => userIdsforClaimRegion.Contains(c.UserId) && c.Region.Value != regionId)
                    .Select(c => c.Region.Value);

            var usersOtherSubjectCategoryClaimRegions =
                DB.AspnetUserApplicationPermissionClaims
                    .Where(c => userIdsforClaimRegion.Contains(c.UserId) && c.Region.Value != regionId)
                    .OrderBy(scc => scc.PermissionName)
                    .Select(c => c.Region.Value);

            var usersOtherClaimRegions = await
                usersOtherSubjectCategoryClaimRegions
                .Concat(usersOtherApplicationClaimRegions)
                .Distinct()
                .ToListAsync();

            var users = await DB.AspNetUsers.Where(u => userIdsforClaimRegion.Contains(u.Id) && u.UserType == "Person").Include("UserUsers.AspNetUser1").ToListAsync();

            return users
                .Select(u => new UserModel(u, applicationPermissionRegionPermissionClaims.ToList(), subjectCategoryPermissionClaims.ToList(), usersOtherClaimRegions.ToList()))
                .OrderBy(u => u.UserName)
                .ToList();
        }

        private async Task<EditUserModel> GetEditUserById(string userID, int regionId)
        {
            //get application permission regions
            var applicationPermissionRegionPermissionClaims =
                DB.AspnetUserApplicationPermissionClaims.Where(c => c.UserId == userID && c.Region.Value == regionId);

            //DB.Database.Log = s => Trace.WriteLine(s);
            //get subject category claims
            var subjectCategoryPermissionClaims =
                DB.AspnetUserSubjectCategoryClaims.Where(c => c.UserId == userID && c.Region.Value == regionId);

            var allApplicationPermissionNames = Enum.GetNames(typeof(ApplicationPermissionNames)).OrderBy(ap => ap).ToList();
            var allSubjectCategoryClaimsValues =
                Enum.GetValues(typeof(SubjectCatagoryNames))
                    .Cast<SubjectCatagoryNames>()
                    .Select(scc => scc.GetDisplayName())
                    .OrderBy(scc => scc)
                    .ToList();
            var user = await DB.AspNetUsers.FirstOrDefaultAsync(u => u.Id == userID);
            var groups = await DB.AspNetUsers.Where(u => u.UserType == "Group").ToListAsync();
            return new EditUserModel(user, applicationPermissionRegionPermissionClaims.ToList(), subjectCategoryPermissionClaims.ToList(), allApplicationPermissionNames, allSubjectCategoryClaimsValues, groups);
        }
        // GET: Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = await DB.AspNetUsers.FindAsync(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,UserType")] AspNetUser aspNetUser)
        {
            if (ModelState.IsValid)
            {
                DB.AspNetUsers.Add(aspNetUser);
                await DB.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(aspNetUser);
        }

        // GET: Users/Edit/5
        public async Task<ActionResult> Edit(string userId, int regionId)
        {
            var user = await GetEditUserById(userId, regionId);
            return PartialView("Edit/Body/_Index", user);
        }

        public ActionResult AddSubjectCategoryRow()
        {

            var allSubjectCategoryClaimsValues =
                 Enum.GetValues(typeof(SubjectCatagoryNames))
                     .Cast<SubjectCatagoryNames>()
                     .Select(scc => scc.GetDisplayName())
                     .OrderBy(scc => scc)
                     .ToList();

            var allSubjectCategoryClaims = EditUserModel.ConvertToSelectList(allSubjectCategoryClaimsValues);



            return PartialView("Edit/Body/SubjectCategories/_SubjectCategory",
                new EditSubjectPermissionClaimModel(allSubjectCategoryClaims));
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit()
        {
            var userData = Request.BodyAsJsonObject<EditUserPostData>();
            var user = await DB.AspNetUsers.FirstOrDefaultAsync(u => u.Id == userData.UserId);
            await SaveApplicationPermissionUserClaims(userData, user);
            await SaveSubjectCategories(userData);
            await DB.SaveChangesAsync();
            foreach (var groupId in userData.GroupIds)
            {
                var userUser = new UserUser
                {
                    ParentUserId = groupId,
                    ChildUserId = userData.UserId,
                    RegionId = userData.RegionId,
                    AutoAssignUser = true
                };

                DB.UserUsers.Add(userUser);
            }

            DB.UserUsers.RemoveRange(DB.UserUsers.Where(uu => uu.ChildUserId == userData.UserId).ToList());
            await DB.SaveChangesAsync();

            return await Search(userData.RegionId);
        }

        private async Task SaveApplicationPermissionUserClaims(EditUserPostData userData, AspNetUser user)
        {
            var allApplicationPermissionNames = Enum.GetNames(typeof(ApplicationPermissionNames)).ToList();
            var applicationPermisionClaimsToAdd = new List<AspNetUserClaim>();
            foreach (var applicationPermission in allApplicationPermissionNames)
            {
                var applicationPermissionClaim =
                    (ApplicationPermissionNames)
                    Enum.Parse(typeof(ApplicationPermissionNames), applicationPermission);

                var claimRegionIds = user.GetApplicationPerimissionRegions(applicationPermissionClaim);
                if (userData.ApplicationPermissionNames.Contains(applicationPermission))
                {
                    claimRegionIds.Add(userData.RegionId);
                }
                if (claimRegionIds.Count > 0)
                {
                    var claim = new ApplicationPermissionClaimValue
                    {
                        Regions = claimRegionIds,
                        ApplicationPermissionName = applicationPermissionClaim
                    }.ToXml();

                    applicationPermisionClaimsToAdd.Add(new AspNetUserClaim
                    {
                        UserId = userData.UserId,
                        ClaimType = ApplicationPermissionClaimValue.ClaimType,
                        ClaimValue = claim
                    });
                }

            }
            DB.AspNetUserClaims.RemoveRange(DB.AspNetUserClaims.Where(c => c.UserId == userData.UserId
                    && c.ClaimType == ApplicationPermissionClaimValue.ClaimType).ToList());

            await DB.SaveChangesAsync();
            DB.AspNetUserClaims.AddRange(applicationPermisionClaimsToAdd);
        }

        private async Task SaveSubjectCategories(EditUserPostData userData)
        {
            var postedClaims = userData.SubjectCategoryClaims;
            var userCurrentSubjectCategoryClaims = await DB.AspnetUserSubjectCategoryClaims.Where(c => c.UserId == userData.UserId).ToListAsync();
            var usersCurrentSubjectCategoryClaimsNotInPostData =
                userCurrentSubjectCategoryClaims.Where(c => c.UserId == userData.UserId && c.Region.Value == userData.RegionId &&
                    !postedClaims.Any(pc => pc.DocType == c.DocumentType && pc.BACode == c.BACode && pc.OrgCode == c.OrgCode));
            var subjectCategoryClaimsToAdd = new List<AspNetUserClaim>();
            var subjectCategoryClaimIdsToDelete = new List<int>();
            foreach (var subjectCategoryClaim in postedClaims)
            {
                var claimsForSubjectCategoryClaim =
                    userCurrentSubjectCategoryClaims.Where(c => c.UserId == userData.UserId
                                                               && c.DocumentType == subjectCategoryClaim.DocType &&
                                                               c.BACode == subjectCategoryClaim.BACode &&
                                                               c.OrgCode == subjectCategoryClaim.OrgCode)
                                                               .ToList();

                var claimRegionIds = new HashSet<int>(claimsForSubjectCategoryClaim.Select(c => c.Region.Value));

                claimRegionIds.Add(userData.RegionId);

                var subjectClaimValue = new SubjectCatagoryClaimValue
                {
                    Regions = claimRegionIds,
                    DocType = subjectCategoryClaim.DocType,
                    BACode = subjectCategoryClaim.BACode,
                    OrgCode = subjectCategoryClaim.OrgCode
                }.ToXml();

                subjectCategoryClaimsToAdd.Add(new AspNetUserClaim
                {
                    UserId = userData.UserId,
                    ClaimType = SubjectCatagoryClaimValue.ClaimType,
                    ClaimValue = subjectClaimValue
                });

                if (claimsForSubjectCategoryClaim.Count > 0)
                {
                    subjectCategoryClaimIdsToDelete.Add(claimsForSubjectCategoryClaim[0].ClaimId);
                }
            }

            foreach (var subjectCategoryClaim in usersCurrentSubjectCategoryClaimsNotInPostData)
            {
                var claimsForSubjectCategoryClaim =
                    userCurrentSubjectCategoryClaims.Where(c => c.UserId == userData.UserId
                                                                && c.DocumentType == subjectCategoryClaim.DocumentType &&
                                                                c.BACode == subjectCategoryClaim.BACode &&
                                                                c.OrgCode == subjectCategoryClaim.OrgCode)
                                                                .ToList();
                var claimRegionIds = new HashSet<int>(claimsForSubjectCategoryClaim.Select(c => c.Region.Value));

                claimRegionIds.Remove(userData.RegionId);

                if (claimRegionIds.Count > 0)
                {
                    var subjectClaimValue = new SubjectCatagoryClaimValue
                    {
                        Regions = claimRegionIds,
                        DocType = subjectCategoryClaim.DocumentType,
                        BACode = subjectCategoryClaim.BACode,
                        OrgCode = subjectCategoryClaim.OrgCode
                    }.ToXml();

                    subjectCategoryClaimsToAdd.Add(new AspNetUserClaim
                    {
                        UserId = userData.UserId,
                        ClaimType = SubjectCatagoryClaimValue.ClaimType,
                        ClaimValue = subjectClaimValue
                    });
                }

                if (claimsForSubjectCategoryClaim.Count > 0)
                {
                    subjectCategoryClaimIdsToDelete.Add(claimsForSubjectCategoryClaim[0].ClaimId);
                }
            }
            DB.AspNetUserClaims.RemoveRange(DB.AspNetUserClaims.Where(c => subjectCategoryClaimIdsToDelete.Contains(c.Id)));
            await DB.SaveChangesAsync();
            DB.AspNetUserClaims.AddRange(subjectCategoryClaimsToAdd);

        }

        public class EditUserPostData
        {
            [JsonProperty]
            public List<string> ApplicationPermissionNames { get; set; }
            [JsonProperty]
            public List<EditUserPostSubjectCategoryClaim> SubjectCategoryClaims { get; set; }
            [JsonProperty]
            public List<string> GroupIds { get; set; }
            [JsonProperty]
            public string UserId { get; set; }
            [JsonProperty]
            public int RegionId { get; set; }

            [JsonObject]
            public class EditUserPostSubjectCategoryClaim
            {
                [JsonProperty]
                public string DocType { get; set; }
                [JsonProperty]
                public string BACode { get; set; }
                [JsonProperty]
                public string OrgCode { get; set; }
            }

        }



        // GET: Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = await DB.AspNetUsers.FindAsync(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            AspNetUser aspNetUser = await DB.AspNetUsers.FindAsync(id);
            DB.AspNetUsers.Remove(aspNetUser);
            await DB.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DB.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
