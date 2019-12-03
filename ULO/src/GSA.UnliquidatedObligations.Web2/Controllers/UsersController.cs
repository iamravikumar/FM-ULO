//using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Identity;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GSA.UnliquidatedObligations.Web.Controllers
{
    //[Authorize]
    //[ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    //[ApplicationPermissionAuthorize(ApplicationPermissionNames.ManageUsers, ApplicationPermissionNames.ViewUsers)]
    public class UsersController : BasePageController
    {
        public const string Name = "Users";

        public static class ActionNames
        {
            public const string Create = "Create";
            public const string Index = "Index";
            public const string Save = "Save";
        }

        private readonly ApplicationUser UserManager;

        public UsersController(ApplicationUser userManager, UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, Serilog.ILogger logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            UserManager = userManager;
        }

        [Route("users")]
        [ActionName(ActionNames.Index)]
        public async Task<ActionResult> Index(string q, IList<int> region, IList<string> role, string sortCol, string sortDir, int? page, int? pageSize)
        {
            q = StringHelpers.TrimOrNull(q);
            var users = DB.AspNetUsers.Where(u => q == null || u.UserName.Contains(q) || u.Email.Contains(q));
            if (region != null && region.Count > 0)
            {
                users = users.Where(u => u.ChildUserUserUsers.Any(uu => uu.RegionId != null && region.Contains((int)uu.RegionId)));
            }
            if (role != null && role.Count > 0)
            {
                users = users.Where(u => u.ChildUserUserUsers.Any(uu => role.Contains(uu.ParentUserId)));
            }
            users = ApplyBrowse(
                users,
                sortCol ?? nameof(AspNetUser.UserName), sortDir, page, pageSize);
            var m = await CreateModelAsync(users);
            return View(m);
        }

        private async Task<IList<UserModel>> CreateModelAsync(IEnumerable<AspNetUser> aspNetUsers)
        {
            var users = aspNetUsers.ToList();
            var userIds = users.ConvertAll(u => u.Id);
            var groups = DB.UserUsers.Include(uu => uu.ParentUser).Where(uu => userIds.Contains(uu.ChildUserId)).ToList();
            var applicationPermissionClaims = await DB.AspnetUserApplicationPermissionClaims.Where(c => userIds.Contains(c.UserId)).ToListAsync();
            var subjectCategoryClaims = await DB.AspnetUserSubjectCategoryClaims.Where(c => userIds.Contains(c.UserId)).ToListAsync();
            var groupsByUserId = groups.ToMultipleValueDictionary(z => z.ChildUserId);
            var applicationPermissionClaimsByUserId = applicationPermissionClaims.ToMultipleValueDictionary(z => z.UserId);
            var subjectCategoryClaimsByUserId = subjectCategoryClaims.ToMultipleValueDictionary(z => z.UserId);
            var userModels = users.ConvertAll(u => new UserModel(u, groupsByUserId[u.Id], applicationPermissionClaimsByUserId[u.Id], subjectCategoryClaimsByUserId[u.Id]));
            return userModels;
        }

        private Task PopulateDetailsViewBag()
        {
            var userNames = DB.AspNetUsers.Where(UserHelpers.GrouplikeUserPredicate).Select(u => u.UserName);
            ViewBag.GroupNames = userNames.Distinct().OrderBy().ToList();
            return Task.CompletedTask;
        }

        [Route("users/{username}", Order = 2)]
        public async Task<ActionResult> Details(string username)
        {
            var users = await DB.AspNetUsers.Where(u => u.UserName == username).ToListAsync();
            if (users.Count != 1) return StatusCode(404);
            var user = users[0];
            Log.Information("Viewing user with UserId={UserId} => UserName={UserName}, Email={Email}", user.Id, user.UserName, user.Email);
            var m = await CreateModelAsync(users);
            await PopulateDetailsViewBag();
            return View(m.Single());
        }

        [Route("users/create", Order = 1)]
        [ActionName(ActionNames.Create)]
        //[ApplicationPermissionAuthorize(ApplicationPermissionNames.ManageUsers)]
        public async Task<ActionResult> Create()
        {
            await PopulateDetailsViewBag();
            return View(new UserModel());
        }

        [Route("users/save")]
        [HttpPost]
        [ActionName(ActionNames.Save)]
        //[ApplicationPermissionAuthorize(ApplicationPermissionNames.ManageUsers)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(
            [Bind(new[]{
                nameof(UserModel.UserId),
                nameof(UserModel.UserName),
                nameof(UserModel.Email),
                nameof(UserModel.UserType),
                nameof(UserModel.Claims),
                nameof(UserModel.Permissions),
                nameof(UserModel.GroupMembershipRegionIds),
                nameof(UserModel.Groups),
                nameof(UserModel.SubjectCategoryClaims),
                nameof(UserModel.SubjectCategoryClaims)+"."+nameof(SubjectCatagoryClaimValue.BACode),
                nameof(UserModel.SubjectCategoryClaims)+"."+nameof(SubjectCatagoryClaimValue.DocType),
                nameof(UserModel.SubjectCategoryClaims)+"."+nameof(SubjectCatagoryClaimValue.OrgCode),
                nameof(UserModel.SubjectCategoryClaims)+"."+nameof(SubjectCatagoryClaimValue.Regions) }
            )]
            UserModel m)
        {
            AspNetUser u = null;
            m.UserId = StringHelpers.TrimOrNull(m.UserId);
            m.UserName = StringHelpers.TrimOrNull(m.UserName);
            m.Email = StringHelpers.TrimOrNull(m.Email);
            var users = await DB.AspNetUsers.Where(z => z.UserName == m.UserName || z.Email == m.Email || z.Id == m.UserId).ToListAsync();
            bool hasErrors = false;
            bool userNameError = false;
            bool emailError = false;
            foreach (var user in users)
            {
                if (user.Id == m.UserId)
                {
                    u = user;
                    continue;
                }
                if (0 == string.Compare(m.UserName, user.UserName, true) &&!userNameError && m.UserName!=null)
                {
                    ModelState.AddModelError(nameof(m.UserName), $"Username {m.UserName} already exists, cannot re-add");
                    hasErrors = true;
                    userNameError = true;
                }
                if (0 == string.Compare(m.Email, user.Email, true) && !emailError && m.Email!=null)
                {
                    ModelState.AddModelError(nameof(m.Email), $"Email {m.Email} already exists, cannot re-add");
                    hasErrors = true;
                    emailError = true;
                }
            }
            if (ModelState.IsValid && !hasErrors)
            {
                if (u == null && m.UserId == null)
                {
                    //var res = await UserManager.CreateAsync(new ApplicationUser { UserName = m.UserName, Email = StringHelpers.TrimOrNull(m.Email) });
                    u = await DB.AspNetUsers.FirstOrDefaultAsync(z => z.UserName == m.UserName);
                    Log.Information("Created new user UserId={UserId} => UserName={UserName}, Email={Email}", u.Id, u.UserName, u.Email);
                }
                else if (u==null)
                {
                    return StatusCode(404);
                }
                else
                {
                    Log.Information("Modified existing with UserId={UserId} => OldUserName={OldUserName}, OldEmail={OldEmail}, NewUserName={NewUserName}, NewEmail={NewEmail}", u.Id, u.UserName, u.Email, m.UserName, m.Email);
                    u.Email = m.Email;
                    u.UserName = m.UserName;
                }

                var myGroups = await DB.AspNetUsers.Where(UserHelpers.GrouplikeUserPredicate).Where(g => m.Groups.Contains(g.UserName)).ToListAsync();
                foreach (var uu in u.ChildUserUserUsers.ToList())
                {
                    DB.UserUsers.Remove(uu);
                }
                m.GroupMembershipRegionIds.ForEach(rid=>myGroups.ForEach(g => DB.UserUsers.Add(new UserUser { ParentUser = g, ChildUser = u, RegionId = rid })));
                foreach (var c in DB.AspNetUserClaims.Where(z => z.ClaimType == ApplicationPermissionClaimValue.ClaimType || z.ClaimType == SubjectCatagoryClaimValue.ClaimType).ToList())
                {
                    DB.AspNetUserClaims.Remove(c);
                }
                var allRegions = Cacher.FindOrCreateValue("allRegionsHashSet", () => DB.Regions.Select(z => z.RegionId).ToSet());
                foreach (var p in m.Permissions)
                {
                    DB.AspNetUserClaims.Add(new AspNetUserClaim
                    {
                        ClaimType = ApplicationPermissionClaimValue.ClaimType,
                        UserId = u.Id,
                        //AspNetUser = u,
                        ClaimValue = new ApplicationPermissionClaimValue
                        {
                            ApplicationPermissionName = Parse.ParseEnum<ApplicationPermissionNames>(p),
                            Regions = allRegions
                        }.ToXml()
                    });
                }

                var sccDocTypes = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                sccDocTypes["scc.docType"] = "scc.docType";

                var sccBaCodes = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                sccBaCodes["scc.bacode"] = "scc.bacode";

                var sccOrgCodes = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                sccOrgCodes["scc.orgcode"] = "scc.orgcode";

                var sccRegions = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                sccRegions["scc.region"] = "scc.region";
                //var sccDocTypes = HttpContext.Request.Form.ToDictionary(x=>x.Key"scc.docType") ?? Empty.StringArray;
                //var sccBaCodes = HttpContext.Request.Form.GetValues("scc.bacode") ?? Empty.StringArray;
                //var sccOrgCodes = HttpContext.Request.Form.GetValues("scc.orgcode") ?? Empty.StringArray;
                //var sccRegions = HttpContext.Request.Form.GetValues("scc.region") ?? Empty.StringArray;
                m.Claims.Clear();
                m.SubjectCategoryClaims = new List<AspnetUserSubjectCategoryClaim>();
                //int n = 0;
                //for (int z = 0; z < sccDocTypes.Count; ++z)
                //{
                //    //var docType = StringHelpers.TrimOrNull(sccDocTypes[z]);
                //    if (docType==null) continue;
                //    if (docType == PortalHelpers.FormFieldsBreak)
                //    {
                //        ++n;
                //        continue;
                //    }
                //    var sccv = new SubjectCatagoryClaimValue
                //    {
                //        //DocType = docType,
                //        //BACode = sccBaCodes[sccBaCodes.IndexOfOccurrence(PortalHelpers.FormFieldsBreak, n, -1).Value + 1],
                //        //OrgCode = sccOrgCodes[sccOrgCodes.IndexOfOccurrence(PortalHelpers.FormFieldsBreak, n, -1).Value + 1]
                //    };
                //    sccv.Regions = new HashSet<int>();
                //    for (int rn= sccRegions.IndexOfOccurrence(PortalHelpers.FormFieldsBreak, n, -1).Value+1; ; ++rn)
                //    {
                //        if (sccRegions[rn] == PortalHelpers.FormFieldsBreak) break;
                //        if (int.TryParse(sccRegions[rn], out int regionId))
                //        {
                //            sccv.Regions.Add(regionId);
                //        }                        
                //    }
                //    if (sccv.Regions.Count == PortalHelpers.RegionCount)
                //    {
                //        sccv.Regions.Clear();
                //    }
                //    DB.AspNetUserClaims.Add(new AspNetUserClaim
                //    {
                //        ClaimType = SubjectCatagoryClaimValue.ClaimType,
                //        UserId = u.Id,
                //        //AspNetUser = u,
                //        ClaimValue = sccv.ToXml()
                //    });
                //    var scc = new AspnetUserSubjectCategoryClaim
                //    {
                //        BACode = sccv.BACode,
                //        DocumentType = sccv.DocType,
                //        OrgCode = sccv.OrgCode,
                //    };
                //    m.SubjectCategoryClaims.Add(scc);
                //    m.Claims.Add(scc.ToString());
                //}
                await DB.SaveChangesAsync();
                return RedirectToIndex();
            }
            await PopulateDetailsViewBag();
            if (m.UserId == null) return View("Create", m);
            else return View("Details", m);
        }
    }
}
