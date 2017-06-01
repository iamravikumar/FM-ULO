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
                    .Select(c => c.Region.Value);

            var usersOtherClaimRegions = await
                usersOtherSubjectCategoryClaimRegions
                .Concat(usersOtherApplicationClaimRegions)
                .Distinct()
                .ToListAsync();

            var users = await DB.AspNetUsers.Where(u => userIdsforClaimRegion.Contains(u.Id) && u.UserType == "Person").ToListAsync();

            return users.Select(u => new UserModel(u, applicationPermissionRegionPermissionClaims.ToList(), subjectCategoryPermissionClaims.ToList(), usersOtherClaimRegions.ToList())).ToList();
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

            var allApplicationPermissionNames = Enum.GetNames(typeof(ApplicationPermissionNames)).ToList();
            var allSubjectCategoryClaims = Enum.GetNames(typeof(SubjectCatagoryNames)).ToList();
            var user = await DB.AspNetUsers.FirstOrDefaultAsync(u => u.Id == userID);
            var groupNames = await DB.AspNetUsers.Where(u => u.UserType == "Group").Select(u => u.UserName).ToListAsync();
            return new EditUserModel(user, applicationPermissionRegionPermissionClaims.ToList(), subjectCategoryPermissionClaims.ToList(), allApplicationPermissionNames, allSubjectCategoryClaims, groupNames);
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
            var allSubjectCategoryClaims = EditUserModel.ConvertToSelectList(Enum.GetNames(typeof(SubjectCatagoryNames)).ToList());

            return PartialView("Edit/Body/SubjectCategories/_SubjectCategory",
                new EditSubjectPermissionClaimModel(allSubjectCategoryClaims));
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,UserType")] AspNetUser aspNetUser)
        {
            if (ModelState.IsValid)
            {
                DB.Entry(aspNetUser).State = EntityState.Modified;
                await DB.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(aspNetUser);
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
