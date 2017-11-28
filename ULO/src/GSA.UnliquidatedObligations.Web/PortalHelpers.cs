using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web
{
    public static class PortalHelpers
    {
        public static bool UseDevAuthentication => Properties.Settings.Default.UseDevAuthentication;
        public static bool ShowSprintNameOnFooter => Properties.Settings.Default.ShowSprintNameOnFooter;
        public static string SprintName => Properties.Settings.Default.SprintName;
        public static string AdministratorEmail => Properties.Settings.Default.AdminstratorEmail;
        public static string AttachmentFileUploadAccept => Properties.Settings.Default.AttachmentFileUploadAccept;
        public static string AttachmentFileUploadAcceptMessage => Properties.Settings.Default.AttachmentFileUploadAcceptMessage;

        public const string Wildcard = "*";

        public static bool VerifyFileAccept(string accepts, string filename, string contentType)
        {
            accepts = accepts ?? "";
            var ext = Path.GetExtension(filename);
            foreach (var accept in accepts.Split('|'))
            {
                try
                {
                    if (accept.Length < 2) continue;
                    if (accept[0] == '.')
                    {
                        if (0 == string.Compare(ext, accept, true)) return true;
                    }
                    else if (accept.Contains("/"))
                    {
                        if (MimeType.IsA(contentType, accept)) return true;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
            return false;
        }

        public static readonly string DefaultUloConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        private static readonly ICacher Cacher = Cache.DataCacher;
        internal static Func<ULODBEntities> UloDbCreator = () => new ULODBEntities();

        static PortalHelpers()
        {
            var d = new Dictionary<ReviewScopeEnum, string>();
            foreach (var row in CSV.ParseText(Properties.Settings.Default.ReviewScopeWorkflowMapCsv))
            {
                if (row.Length != 2) continue;
                var scope = Parse.ParseEnum<ReviewScopeEnum>(row[0]);
                d[scope] = StringHelpers.TrimOrNull(row[1]);
            }
            WorkflowDefinitionNameByReviewScope = d.AsReadOnly();
        }

        public static readonly IDictionary<ReviewScopeEnum, string> WorkflowDefinitionNameByReviewScope;

        public static string GetStorageFolderPath(string relativePath, bool createFolderInNotExists=true)
        {
            relativePath = relativePath ?? "";
            var dir = Properties.Settings.Default.DocPath;
            if (0==string.Compare(dir, "%temp%", true))
            {
                dir = Path.GetTempPath();
            }
            else if (dir.StartsWith("~"))
            {
                dir = HostingEnvironment.MapPath(dir);
            }
            var forwardSlash = dir.Contains("/");
            dir = dir.Replace("\\", "/");
            if (!dir.EndsWith("/"))
            {
                dir += "/";
            }
            relativePath = relativePath.Replace("\\", "/");
            if (relativePath.StartsWith("/"))
            {
                relativePath = relativePath.Substring(1);
            }
            var path = dir + relativePath;
            if (!forwardSlash)
            {
                path = path.Replace("/", "\\");
            }
            if (createFolderInNotExists)
            {
                dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            return path;
        }

        public static bool HasPermission(this IPrincipal user, ApplicationPermissionNames permissionName)
        {
            var userClaims = Cache.DataCacher.FindOrCreateValWithSimpleKey(
                user.Identity?.Name,
                () => CurrentComponentContext.Resolve<ULODBEntities>().AspNetUsers.Include(u => u.AspNetUserClaims).FirstOrDefault(u => u.UserName == user.Identity.Name)?.GetClaims(),
                UloHelpers.ShortCacheTimeout
                );

            return userClaims!=null && userClaims.GetApplicationPerimissionRegions(permissionName).Count>0;
        }

        public static IComponentContext GetComponentContext(this HttpContext context)
            => (IComponentContext) context.Items["ComponentContext"];

        public static IComponentContext CurrentComponentContext
            => HttpContext.Current.GetComponentContext();

        public static string GetUserId(string username)
        {
            if (username != null)
            {
                return Cacher.FindOrCreateValWithSimpleKey(
                    username,
                    () =>
                    {
                        using (var db = UloDbCreator())
                        {
                            return db.AspNetUsers.Where(z => z.UserName == username).Select(z => z.Id).FirstOrDefault();
                        }
                    },
                    UloHelpers.MediumCacheTimeout
                    );
            }
            return null;
        }

        public static string PreAssignmentUserUserId
            => GetUserId(Properties.Settings.Default.PreAssignmentUserUsername);

        public static string ReassignGroupUserId
            => GetUserId(Properties.Settings.Default.ReassignGroupUserName);
        public static string ReassignGroupUserName
            => Properties.Settings.Default.ReassignGroupUserName;

        public static IList<int?> GetUserGroupRegions(this IPrincipal user, string groupNameOrId)
            => Cacher.FindOrCreateValWithSimpleKey(
                Cache.CreateKey(nameof(GetReassignmentGroupRegions), user.Identity.Name, groupNameOrId),
                () =>
                {
                    using (var db = UloDbCreator())
                    {
                        var groupId = PortalHelpers.GetUserId(groupNameOrId);

                        var userId = GetUserId(user?.Identity?.Name);

                        return db.UserUsers
                            .Where(uu => (uu.ParentUserId == groupId || uu.ParentUserId == groupNameOrId) && uu.ChildUserId == userId)
                            .Select(uu => uu.RegionId)
                            .Distinct()
                            .ToList()
                            .AsReadOnly();
                    }
                },
                UloHelpers.MediumCacheTimeout
                );

        public static IQueryable<UnliquidatedObligation> WhereReviewExists(this IQueryable<UnliquidatedObligation> wf)
            => wf.Where(z => z.Review != null);

        public static IQueryable<Workflow> WhereReviewExists(this IQueryable<Workflow> wf)
            => wf.Where(z => z.UnliquidatedObligation.Review != null);

        public static IList<int?> GetReassignmentGroupRegions(this IPrincipal user)
            => user.GetUserGroupRegions(Properties.Settings.Default.ReassignGroupUserName);

        public static Expression<Func<Workflow, bool>> GenerateWorkflowPredicate(int? uloId, string pegasysDocumentNumber, string organization,
           int? region, int? zone, string fund, string baCode, string pegasysTitleNumber, string pegasysVendorName, string docType, string contractingOfficersName, string currentlyAssignedTo, string hasBeenAssignedTo, string awardNumber, string reasonIncludedInReview, bool? valid, string status, int? reviewId)
        {
            pegasysDocumentNumber = StringHelpers.TrimOrNull(pegasysDocumentNumber);
            organization = StringHelpers.TrimOrNull(organization);
            fund = StringHelpers.TrimOrNull(fund);
            baCode = StringHelpers.TrimOrNull(baCode);
            pegasysTitleNumber = StringHelpers.TrimOrNull(pegasysTitleNumber);
            pegasysVendorName = StringHelpers.TrimOrNull(pegasysVendorName);
            docType = StringHelpers.TrimOrNull(docType);
            contractingOfficersName = StringHelpers.TrimOrNull(contractingOfficersName);
            currentlyAssignedTo = StringHelpers.TrimOrNull(currentlyAssignedTo);
            hasBeenAssignedTo = StringHelpers.TrimOrNull(hasBeenAssignedTo);
            awardNumber = StringHelpers.TrimOrNull(awardNumber);
            reasonIncludedInReview = StringHelpers.TrimOrNull(reasonIncludedInReview);
            status = StringHelpers.TrimOrNull(status);

            if (uloId == null &&
                pegasysDocumentNumber == null &&
                organization == null &&
                region == null &&
                zone == null &&
                fund == null &&
                baCode == null &&
                pegasysTitleNumber == null &&
                pegasysVendorName == null &&
                docType == null &&
                contractingOfficersName == null &&
                currentlyAssignedTo == null &&
                hasBeenAssignedTo == null &&
                awardNumber == null &&
                reasonIncludedInReview == null &&
                valid == null &&
                status == null &&
                reviewId == null)
            {
                return null;
            }

            var originalPredicate = PredicateBuilder.Create<Workflow>(wf => true);

            var predicate = originalPredicate;

            if (uloId != null)
            {
                predicate = predicate.And(wf => wf.TargetUloId == uloId);
            }

            if (pegasysDocumentNumber!=null)
            {
                var criteria = pegasysDocumentNumber.Replace(Wildcard, "");
                if (pegasysDocumentNumber.StartsWith(Wildcard) && pegasysDocumentNumber.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.PegasysDocumentNumber.Contains(criteria));
                }
                else if (pegasysDocumentNumber.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysDocumentNumber.TrimEnd().EndsWith(criteria));
                }
                else if (pegasysDocumentNumber.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysDocumentNumber.TrimStart().StartsWith(criteria));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysDocumentNumber.Trim() == criteria);
                }
            }

            if (organization!=null)
            {
                var criteria = organization.Replace(Wildcard, "");
                if (organization.StartsWith(Wildcard) && organization.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Organization.Contains(criteria));
                }
                else if (organization.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Organization.Trim().EndsWith(criteria));
                }
                else if (organization.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Organization.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate =
                        predicate.And(wf => wf.UnliquidatedObligation.Organization.Trim() == criteria);
                }
            }

            if (region != null)
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.RegionId == region);
            }

            if (zone != null)
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.Region.ZoneId == zone);
            }

            if (fund!=null)
            {
                var criteria = fund.Replace(Wildcard, "");
                if (fund.StartsWith(Wildcard) && fund.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Fund.Contains(criteria));
                }
                else if (fund.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Fund.Trim().EndsWith(criteria));
                }
                else if (fund.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Fund.StartsWith(criteria));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.Fund.Trim() == criteria);
                }
            }

            if (baCode!=null)
            {
                var criteria = baCode.Replace(Wildcard, "");
                if (baCode.StartsWith(Wildcard) && baCode.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Prog.Contains(criteria));
                }
                else if (baCode.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Prog.Trim().EndsWith(criteria));
                }
                else if (baCode.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Prog.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.Prog.Trim() == criteria);
                }
                
            }

            if (pegasysTitleNumber!=null)
            {
                var criteria = pegasysTitleNumber.Replace(Wildcard, "");
                if (pegasysTitleNumber.StartsWith(Wildcard) && pegasysTitleNumber.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.PegasysTitleNumber.Contains(criteria));
                }
                else if (pegasysTitleNumber.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysTitleNumber.Trim().EndsWith(criteria));
                }
                else if (pegasysTitleNumber.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysTitleNumber.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf =>
                                wf.UnliquidatedObligation.PegasysTitleNumber.Trim() ==
                                criteria);
                }
            }

            if (pegasysVendorName!=null)
            {
                var criteria = pegasysVendorName.Replace(Wildcard, "");
                if (pegasysVendorName.StartsWith(Wildcard) && pegasysVendorName.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.VendorName.Contains(criteria));
                }
                else if (pegasysVendorName.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.VendorName.Trim().EndsWith(criteria));
                }
                else if (pegasysVendorName.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.VendorName.Trim().StartsWith(criteria));
                } 
                else
                {
                    predicate =
                        predicate.And(wf =>
                            wf.UnliquidatedObligation.VendorName.Trim() == criteria);
                }
            }

            if (docType!=null)
            {
                predicate =
                   predicate.And(
                       wf => wf.UnliquidatedObligation.DocType == docType);
            }

            if (contractingOfficersName!=null)
            {
                var criteria = contractingOfficersName.Replace(Wildcard, "");
                if (contractingOfficersName.StartsWith(Wildcard) && contractingOfficersName.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.ContractingOfficersName.Contains(criteria));
                }
                else if (contractingOfficersName.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.ContractingOfficersName.Trim().EndsWith(criteria));
                }
                else if (contractingOfficersName.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.ContractingOfficersName.StartsWith(criteria));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf =>
                                wf.UnliquidatedObligation.ContractingOfficersName.Trim() ==
                                criteria);
                }
            }

            if (currentlyAssignedTo!=null)
            {
                var criteria = currentlyAssignedTo.Replace(Wildcard, "");
                if (currentlyAssignedTo.StartsWith(Wildcard) && currentlyAssignedTo.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.AspNetUser.UserName.Contains(criteria));
                }
                else if (currentlyAssignedTo.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.AspNetUser.UserName.Trim().EndsWith(criteria));
                }
                else if (currentlyAssignedTo.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.AspNetUser.UserName.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate = predicate.And(wf => wf.AspNetUser.UserName.Trim() == currentlyAssignedTo);
                }

            }

            if (hasBeenAssignedTo!=null)
            {
                var criteria = hasBeenAssignedTo.Replace(Wildcard, "");
                if (hasBeenAssignedTo.StartsWith(Wildcard) && hasBeenAssignedTo.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.WorkflowHistories.Any(wfh => wfh.AspNetUser.UserName.Contains(criteria)));
                }
                else if (hasBeenAssignedTo.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.WorkflowHistories.Any(wfh => wfh.AspNetUser.UserName.EndsWith(criteria)));
                }
                else if (hasBeenAssignedTo.EndsWith(Wildcard))
                {
                    predicate =
                     predicate.And(
                         wf => wf.WorkflowHistories.Any(wfh => wfh.AspNetUser.UserName.StartsWith(criteria)));
                }
                else
                {
                    predicate =
                     predicate.And(
                         wf => wf.WorkflowHistories.Any(wfh => wfh.AspNetUser.UserName == criteria));
                }
            }

            if (awardNumber!=null)
            {
                var criteria = awardNumber.Replace(Wildcard, "");
                if (awardNumber.StartsWith(Wildcard) && awardNumber.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.AwardNbr.Contains(criteria));
                }
                else if (awardNumber.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.AwardNbr.Trim().EndsWith(criteria));
                }
                else if (awardNumber.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.AwardNbr.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.AwardNbr.Trim() == criteria);
                }
               
            }

            if (reasonIncludedInReview!=null)
            {
                var criteria = reasonIncludedInReview.Replace(Wildcard, "");
                if (reasonIncludedInReview.StartsWith(Wildcard) && reasonIncludedInReview.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.ReasonIncludedInReview.Contains(criteria));
                }
                else if (reasonIncludedInReview.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.ReasonIncludedInReview.Trim().EndsWith(criteria));
                }
                else if (reasonIncludedInReview.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.ReasonIncludedInReview.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf =>
                                wf.UnliquidatedObligation.ReasonIncludedInReview.Trim() ==
                                criteria);
                }
            }

            if (valid.HasValue)
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.Valid == valid);
            }

            if (status!=null)
            {
                var criteria = status.Replace(Wildcard, "");
                if (status.StartsWith(Wildcard) && status.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Status.Contains(criteria));
                }
                else if (status.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Status.Trim().EndsWith(criteria));
                }
                else if (status.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Status.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.Status.Trim() == criteria);
                }
            }

            if (reviewId != null)
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.ReviewId == reviewId);
            }

            return predicate;
        }

        public static string GetDisplayName(this Enum value)
            => value.GetDisplayAttribute()?.GetName() ?? value.ToString();

        public static string GetDescription(this Enum value)
            => value.GetDisplayAttribute()?.GetDescription();

        private static DisplayAttribute GetDisplayAttribute(this Enum value)
        {
            var type = value.GetType();
            if (!type.IsEnum) throw new ArgumentException(String.Format("Type '{0}' is not Enum", type));

            var members = type.GetMember(value.ToString());
            if (members.Length == 0) throw new ArgumentException(String.Format("Member '{0}' not found in type '{1}'", value, type.Name));

            var member = members[0];
            var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            return (DisplayAttribute)attributes.FirstOrDefault();
        }

        public static SelectListItem CreateSelectListItem(Justification j)
            => new SelectListItem { Value = j.Key, Text = j.Description };

        public static IList<SelectListItem> CreateSelectList(IEnumerable<Justification> justifications)
            => justifications.ConvertAll(j => CreateSelectListItem(j)).ToList();

        public static IList<SelectListItem> CreateSelectList(IEnumerable<AspNetUser> aspNetUsers)
            => aspNetUsers.Select(z => CreateUserSelectListItem(z.Id, z.UserName)).ToList();

        public static SelectListItem ToSelectListItem(this AspNetUser u, bool disabled=false)
            => CreateUserSelectListItem(u.Id, u.UserName, disabled);

        public static SelectListItem CreateUserSelectListItem(string userId, string username, bool disabled=false)
            => new SelectListItem
            {
                Text = username,
                Value = userId,
                Disabled = disabled
            };

        public static IList<SelectListItem> ConvertToSelectList(this IEnumerable<string> stringsToConvert)
        {
            var stringsSelect = new List<SelectListItem>();

            foreach (var stringToConvert in stringsToConvert)
            {
                stringsSelect.Add(new SelectListItem { Text = stringToConvert, Value = stringToConvert });
            }
            return stringsSelect;

        }

        public static IList<SelectListItem> ConvertToSelectList(this IEnumerable<SelectListItem> selectListItems)
        {
            var selectList = new List<SelectListItem>();

            foreach (var selectListItem in selectListItems)
            {
                selectList.Add(selectListItem);
            }
            return selectList;

        }

        public static IList<SelectListItem> ConvertToSelectList(this IEnumerable<int> nums)
        {
            var numsSelect = new List<SelectListItem>();

            foreach (var num in nums)
            {
                numsSelect.Add(new SelectListItem { Text = num.ToString(), Value = num.ToString() });
            }
            return numsSelect;
        }

        public static IList<SelectListItem> ConvertToSelectList(this IEnumerable<WorkflowDefinition> workFlowDefintions)
        {
            var workFlowDefintionsSelect = new List<SelectListItem>();

            foreach (var workflowDefinition in workFlowDefintions)
            {
                workFlowDefintionsSelect.Add(new SelectListItem {
                    Text = workflowDefinition.WorkflowDefinitionName,
                    Value = workflowDefinition.WorkflowDefinitionId.ToString()
                });
            }
            return workFlowDefintionsSelect;

        }

        public static string Currency(this HtmlHelper helper, decimal data, string locale = "en-US", bool woCurrency = false)
        {
            var culture = new System.Globalization.CultureInfo(locale);

            if (woCurrency || (helper.ViewData["woCurrency"] != null && (bool)helper.ViewData["woCurrency"]))
                return data.ToString(culture);

            return data.ToString("C", culture);
        }

        public static IList<SelectListItem> ConvertNamesToSelectList<T>() where T : struct
            => ((IEnumerable<T>)Enum.GetValues(typeof(T))).ConvertToSelectList(true);

        public static IList<SelectListItem> ConvertToSelectList<T>(this IEnumerable<T> enums, bool names=false) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            var eNumsSelect = new List<SelectListItem>();

            foreach (var enu in Enum.GetValues(typeof(T)))
            {
                var e = (Enum)Enum.Parse(typeof(T), enu.ToString());
                var displayName = e.GetDisplayName();
                var desc = e.GetDescription();
                string value;
                if (names)
                {
                    value = enu.ToString();
                }
                else
                {
                    value = ((int)Enum.Parse(typeof(T), enu.ToString())).ToString();
                }
                eNumsSelect.Add(new ExtendedSelectListItem { Text = displayName, Value = value, Description = desc });
            }
            return eNumsSelect;
        }

        public static T BodyAsJsonObject<T>(this HttpRequestBase req)
        {
            req.InputStream.Seek(0, SeekOrigin.Begin);
            var json = new StreamReader(req.InputStream).ReadToEnd();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }


        public static IList<SelectListItem> CreateReviewSelectListItems()
        => Cacher.FindOrCreateValWithSimpleKey(
            nameof(CreateReviewSelectListItems),
            () => 
            {
                using (var db = UloDbCreator())
                {
                    return db.Reviews.OrderByDescending(r => r.ReviewId).ConvertAll(
                                r => new SelectListItem
                                {
                                    Text = $"{r.ReviewName} (#{r.ReviewId}) - {AspHelpers.GetDisplayName(r.ReviewScope)} - {AspHelpers.GetDisplayName(r.ReviewType)}",
                                    Value = r.ReviewId.ToString()
                                }).
                                ToList().
                                AsReadOnly();
                }
            },
            UloHelpers.ShortCacheTimeout
            );

        public static IList<SelectListItem> CreateUserTypesSelectListItems(bool creatableOnly = true)
            =>new[] {
                AspNetUser.UserTypes.Person,
                AspNetUser.UserTypes.Group,
                creatableOnly ? null : AspNetUser.UserTypes.System,
            }.WhereNotNull().OrderBy().ConvertToSelectList();

        public static string GetRegionName(int regionId)
            => Cacher.FindOrCreateValWithSimpleKey(
                Cache.CreateKey(nameof(GetRegionName), regionId),
                () => CreateRegionSelectListItems(false).FirstOrDefault(i => i.Value == regionId.ToString())?.Text);

        public static IList<SelectListItem> CreateRegionSelectListItems(bool includeAllRegions=false, string allRegionsValue="*")
            => Cacher.FindOrCreateValWithSimpleKey(
                nameof(CreateRegionSelectListItems),
                () => 
                {
                    using (var db = UloDbCreator())
                    {
                        var items = db.Regions.OrderBy(r => r.RegionName).ConvertAll(
                            r => new SelectListItem { Text = $"{r.RegionNumber.PadLeft(2, '0')} - {r.RegionName}", Value = r.RegionId.ToString() }).
                            OrderBy(z => z.Text).
                            ToList();
                        if (includeAllRegions)
                        {
                            items.Insert(0, new SelectListItem { Text = "*", Value = allRegionsValue });
                        }
                        return items.AsReadOnly();
                    }
                },
                UloHelpers.MediumCacheTimeout
                ).Copy();

        public static IList<SelectListItem> CreateDocumentTypeSelectListItems()
            => CSV.ParseText(Properties.Settings.Default.DocTypesCsv).Where(r => r.Length == 2).ConvertAll(
                r => new SelectListItem { Value = StringHelpers.TrimOrNull(r[0]), Text = StringHelpers.TrimOrNull(r[1]) }).Copy();

        public static IList<SelectListItem> CreateSelectListItems(this IEnumerable<Models.QuestionChoicesViewModel> items)
            => items.OrderBy(z=>z.Text).ConvertAll(z => new SelectListItem { Text = z.Text, Value = z.Value });

        public static IList<SelectListItem> PleaseSelect(this IList<SelectListItem> items)
        {
            bool alreadySelected = items.FirstOrDefault(z => z.Selected) != null;
            items.Insert(0, new SelectListItem { Text = AspHelpers.PleaseSelectOne, Disabled = true, Selected=!alreadySelected, Value="" });
            return items;
        }

        public static IList<SelectListItem> Select(this IList<SelectListItem> items, object selectedValue)
        {
            var v = Stuff.ObjectToString(selectedValue);
            foreach (var i in items)
            {
                i.Selected = i.Value == v;
            }
            return items;
        }

        public static IList<SelectListItem> Copy(this IEnumerable<SelectListItem> items)
        {
            var ret = new List<SelectListItem>();
            foreach (var i in items)
            {
                ret.Add(new SelectListItem
                {
                    Disabled = i.Disabled,
                    Group = i.Group,
                    Selected = i.Selected,
                    Text = i.Text,
                    Value = i.Value
                });
            }
            return ret;
        }

        public static IList<SelectListItem> CreateZoneSelectListItems()
            => Cacher.FindOrCreateValWithSimpleKey(
                nameof(CreateZoneSelectListItems),
                () =>
                {
                    using (var db = UloDbCreator())
                    {
                        return db.Zones.OrderBy(z => z.ZoneName).ConvertAll(
                            z => new SelectListItem { Text = $"{z.ZoneName}", Value = z.ZoneId.ToString() }).
                            OrderBy(z => z.Text).
                            ToList().
                            AsReadOnly();
                    }
                },
                UloHelpers.MediumCacheTimeout
                ).Copy();

        public static string ToFriendlySubjectCategoryClaimString(string documentType, string baCode, string orgCode, int? region)
            => $"SC (DT: {documentType}, BAC: {baCode}, OC: {orgCode}), R:{(region.HasValue ? region.ToString() : "*")}";

        public static string ToFriendlyString(this AspnetUserSubjectCategoryClaim claim)
            => ToFriendlySubjectCategoryClaimString(claim.DocumentType, claim.BACode, claim.OrgCode, claim.Region);

        private const string CorrelationIdHeaderString = "X-Correlation-ID";
        public static string CorrelationId(this HttpContext ctx)
        {
            string correlationId = ctx.Response.Headers[CorrelationIdHeaderString];
            if (correlationId == null)
            {
                correlationId = ctx.Response.Headers[CorrelationIdHeaderString] = ctx.Request.Headers[CorrelationIdHeaderString] ?? "XCID" + Guid.NewGuid().ToString();                
            }
            return correlationId;
        }

        public static RequestForReassignment GetReassignmentRequest(this Workflow wf)
            => wf.RequestForReassignments.OrderByDescending(z => z.RequestForReassignmentID).FirstOrDefault();

        internal static IQueryable<Workflow> ApplyStandardIncludes(this IQueryable<Workflow> workflows)
            => workflows.Include(wf => wf.UnliquidatedObligation).Include(wf => wf.UnliquidatedObligation.Region).Include(wf => wf.UnliquidatedObligation.Review);
    }
}