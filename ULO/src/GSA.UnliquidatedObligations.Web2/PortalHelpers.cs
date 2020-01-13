using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using Serilog;

namespace GSA.UnliquidatedObligations.Web
{
    public class PortalHelpers
    {
        public TimeZoneInfo DisplayTimeZone
        {
            get
            {
                if (DisplayTimeZone_p == null)
                {
                    try
                    {
                        DisplayTimeZone_p = TimeZoneInfo.FindSystemTimeZoneById(ConfigOptions.Value.TimezoneId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Problem loading timezone with id = [{timezoneId}]", ConfigOptions.Value.TimezoneId);
                        DisplayTimeZone_p = TimeZoneInfo.Local;
                        try
                        {
                            Logger.Information(
                                "Valid timeZoneIds include {validIds}",
                                TimeZoneInfo.GetSystemTimeZones().ConvertAll(t => t.Id).Format(", ", "[{0}]")
                                );
                        }
                        catch (Exception) { }
                    }
                }
                return DisplayTimeZone_p;
            }
        }
        
        
        private TimeZoneInfo DisplayTimeZone_p;

        public const string Wildcard = "*";

        public const string FormFieldsBreak = "`";
        public TimeSpan MediumCacheTimeout => ConfigOptions.Value.MediumCacheTimeout;

        public TimeSpan ShortCacheTimeout => ConfigOptions.Value.ShortCacheTimeout;

        public string AdministratorEmail => ConfigOptions.Value.AdministratorEmail;

        public bool UseOldGetEligibleReviewersAlgorithm => ConfigOptions.Value.UseOldGetEligibleReviewersAlgorithm;

        public bool SendBatchEmailsDuringAssignWorkflows => ConfigOptions.Value.SendBatchEmailsDuringAssignWorkflows;

        public bool AllowFileShareInfo => ConfigOptions.Value.AllowFileShareInfo;

        public string GetEligibleReviewersQualifiedUsernameFormat => ConfigOptions.Value.GetEligibleReviewersQualifiedUsernameFormat;

        public string GetEligibleReviewersNotQualifiedUsernameFormat => ConfigOptions.Value.GetEligibleReviewersNotQualifiedUsernameFormat;

        public bool UseDevAuthentication => AccountConfigOptions.Value.UseDevAuthentication;

        public string StaleWorkflowErrorMessageTemplate => ConfigOptions.Value.StaleWorkflowErrorMessageTemplate;

        public string DocPath => ConfigOptions.Value.DocPath;

        public string AttachmentFileUploadAccept => ConfigOptions.Value.AttachmentFileUploadAccept;

        public string AttachmentFileUploadAcceptMessage => ConfigOptions.Value.AttachmentFileUploadAcceptMessage;

        public class Config
        {
            public const string ConfigSectionName = "PortalHelpersConfig";

            public string TimezoneId { get; set; } = "Eastern Standard Time";

            public Uri ExternalSiteUrl { get; set; }

            public TimeSpan MediumCacheTimeout { get; set; } = TimeSpan.Parse("00:05:00");

            public TimeSpan ShortCacheTimeout { get; set; } = TimeSpan.Parse("00:01:00");

            public string AdministratorEmail { get; set; }

            public string[][] DocTypes { get; set; }

            public bool UseOldGetEligibleReviewersAlgorithm { get; set; }

            public bool SendBatchEmailsDuringAssignWorkflows { get; set; }

            public bool AllowFileShareInfo { get; set; }

            public string GetEligibleReviewersQualifiedUsernameFormat { get; set; }

            public string GetEligibleReviewersNotQualifiedUsernameFormat { get; set; }

            public string StaleWorkflowErrorMessageTemplate { get; set; }

            public string DocPath { get; set; }

            public string AttachmentFileUploadAccept { get; set; }

            public string AttachmentFileUploadAcceptMessage { get; set; }
        }

        public readonly IOptions<SprintConfig> SprintConfigOptions;

        private readonly IOptions<Config> ConfigOptions;

        private readonly IOptions<Controllers.AccountController.Config> AccountConfigOptions;
        private readonly UloDbContext DB;
        private readonly ICacher Cacher;
        private readonly ILogger Logger;
        private readonly IConfiguration Configuration;
        private readonly IHostEnvironment HostingEnvironment;

        public const string DefaultConectionStringName = "DefaultConnection";

        public string DefaultUloConnectionString => Configuration.GetConnectionString(DefaultConectionStringName);

        public PortalHelpers(IConfiguration configuration, IHostEnvironment hostingEnvironment,IOptions<SprintConfig> sprintConfigOptions, IOptions<Config> configOptions, IOptions<Controllers.AccountController.Config> accountConfigOptions, UloDbContext db, ICacher cacher, ILogger logger)
        {
            Requires.NonNull(sprintConfigOptions, nameof(sprintConfigOptions));
            Requires.NonNull(configOptions, nameof(configOptions));
            Requires.NonNull(accountConfigOptions, nameof(accountConfigOptions));
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
            SprintConfigOptions = sprintConfigOptions;
            ConfigOptions = configOptions;
            AccountConfigOptions = accountConfigOptions;
            DB = db;
            Cacher = cacher;
            Logger = logger;
        }

        public bool VerifyFileAccept(string filename, string contentType, string accepts=null)
        {
            accepts = accepts ?? AttachmentFileUploadAccept;
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
        public IList<SelectListItem> CreateZoneSelectListItems()
            => Cacher.FindOrCreateValue(
                nameof(CreateZoneSelectListItems),
                () =>
                    DB.Zones.OrderBy(z => z.ZoneName).ConvertAll(
                        z => new SelectListItem { Text = $"{z.ZoneName}", Value = z.ZoneId.ToString() }).
                        OrderBy(z => z.Text).
                        ToList().
                        AsReadOnly(),
                MediumCacheTimeout
                ).Copy();

        public IList<SelectListItem> CreateRegionSelectListItems(bool includeAllRegions = false, string allRegionsValue = "*")
           => Cacher.FindOrCreateValue(
               nameof(CreateRegionSelectListItems),
               () =>
               {
                    var items = DB.Regions.AsNoTracking().OrderBy(r => r.RegionName).ConvertAll(
                        r => new SelectListItem { Text = $"{r.RegionNumber.PadLeft(2, '0')} - {r.RegionName}", Value = r.RegionId.ToString() }).
                        OrderBy(z => z.Text).
                        ToList();
                    if (includeAllRegions)
                    {
                        items.Insert(0, new SelectListItem { Text = "*", Value = allRegionsValue });
                    }
                    return items.AsReadOnly();
               },
               MediumCacheTimeout
               ).Copy();


        public IList<SelectListItem> CreateAllGroupNamesSelectListItems()
      => Cacher.FindOrCreateValue(
          nameof(CreateAllGroupNamesSelectListItems),
          () =>
          DB.AspNetUsers.AsNoTracking().Where(u => u.UserType == AspNetUser.UserTypes.Group).ConvertAll(
                              r => new SelectListItem
                              {
                                  Text = r.UserName,
                                  Value = r.Id
                              }).
                              ToList().
                              AsReadOnly(),
          MediumCacheTimeout
          ).Copy();

        public IList<SelectListItem> CreateReviewSelectListItems()
        => Cacher.FindOrCreateValue(Cache.CreateKey(nameof(CreateReviewSelectListItems)),
            () =>
                DB.Reviews.AsNoTracking().OrderByDescending(r => r.ReviewId).ConvertAll(
                                r => new SelectListItem
                                {
                                    Text = $"{r.ReviewName} (#{r.ReviewId}) - {AspHelpers.GetDisplayName(r.ReviewScopeId)} - {AspHelpers.GetDisplayName(r.ReviewTypeId)}",
                                    Value = r.ReviewId.ToString()
                                }).
                                ToList().
                                AsReadOnly(),
            ShortCacheTimeout
            ).Copy();

        public DateTime ToLocalizedDateTime(DateTime utc)
        {
            if (utc.Kind == DateTimeKind.Unspecified)
            {
                utc = new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, utc.Second, utc.Millisecond, DateTimeKind.Utc);
            }
            return utc.ToTimeZone(DisplayTimeZone);
        }

        public string ToLocalizedDisplayDateString(DateTime utc, bool includeTime = false)
        {
            var local = ToLocalizedDateTime(utc);
            var s = local.Date.ToString("MM/dd/yyyy");
            if (includeTime)
            {
                s += " " + local.ToString("t");
            }
            return s;
        }

        public bool HideLoginLinks(dynamic viewBag, bool? set = null)
        {
            if (set.HasValue)
            {
                viewBag.HideLoginLinks = set.Value;
            }
            var o = viewBag.HideLoginLinks;
            if (o == null || !(o is bool))
            {
                return false;
            }
            return (bool)o;
        }

        ////Sreeni changes
        ///
        public IList<SelectListItem> CreateDocumentTypeSelectListItems()
          => ConfigOptions.Value.DocTypes.Where(r => r.Length == 2).ConvertAll(
              r => new SelectListItem { Value = StringHelpers.TrimOrNull(r[0]), Text = StringHelpers.TrimOrNull(r[1]) }).Copy();      

       
        public string GetUserId(string username)
        {
            if (username != null)
            {
                return Cacher.FindOrCreateValue(
                    username,
                    () =>
                     DB.AspNetUsers.Where(z => z.UserName == username).Select(z => z.Id).FirstOrDefault(),
                    MediumCacheTimeout
                    );
            }
            return null;
        }

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
               MediumCacheTimeout
               );

        public string GetRegionName(int regionId)
          => Cacher.FindOrCreateValue(
              Cache.CreateKey(nameof(GetRegionName), regionId),
              () => CreateRegionSelectListItems(false).FirstOrDefault(i => i.Value == regionId.ToString())?.Text);

        public int RegionCount
            => CreateRegionSelectListItems(false).Count;


        public Expression<Func<Workflow, bool>> GetWorkflowsRegionIdPredicate(IEnumerable<int?> regionIds)
        {
            var predicate = PredicateBuilder.Create<Workflow>(wf => false);
            foreach (var regionId in regionIds)
            {
                var rid = regionId.GetValueOrDefault();
                predicate = predicate.Or(wf => wf.TargetUlo.RegionId == rid);
            }
            return predicate;
        }

        public Expression<Func<Workflow, bool>> GetWorkflowsWorkflowIdPredicate(IEnumerable<int> workflowIds)
        {
            /*
            var workflows = DB.Workflows.Where(w => workflowIds.Contains(w.WorkflowId));
            For whatever reason, linq 2 sql wont translate the above into an IN statement (maybe it only does this for string),
            As such, we have to build out a big long nasty OR predicate then apply which we do below.             
             */
            var predicate = PredicateBuilder.Create<Workflow>(wf => false);
            foreach (var wfid in workflowIds)
            {
                predicate = predicate.Or(wf => wf.WorkflowId == wfid);
            }
            return predicate;
        }

        public string GetStorageFolderPath(string relativePath, bool createFolderInNotExists = true)
        {
            relativePath = relativePath ?? "";
            var dir = "%temp%";
            if (0 == string.Compare(dir, "%temp%", true))
            {
                dir = Path.GetTempPath();
            }
            else if (dir.StartsWith("~"))
            {
                dir = HostingEnvironment.ApplicationName;
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

        public Expression<Func<Workflow, bool>> GenerateWorkflowPredicate(IPrincipal currentUser, int? uloId, string pegasysDocumentNumber, string organization,
         IList<int> regions, IList<int> zones, string fund, IList<string> baCode, string pegasysTitleNumber, string pegasysVendorName, IList<string> docType, string contractingOfficersName, string currentlyAssignedTo, string hasBeenAssignedTo, string awardNumber, IList<string> reasonIncludedInReview, IList<bool> valid, IList<string> status, IList<int> reviewId, bool? reassignableByMe)
        {
            pegasysDocumentNumber = StringHelpers.TrimOrNull(pegasysDocumentNumber);
            organization = StringHelpers.TrimOrNull(organization);
            fund = StringHelpers.TrimOrNull(fund);
            pegasysTitleNumber = StringHelpers.TrimOrNull(pegasysTitleNumber);
            pegasysVendorName = StringHelpers.TrimOrNull(pegasysVendorName);
            contractingOfficersName = StringHelpers.TrimOrNull(contractingOfficersName);
            currentlyAssignedTo = StringHelpers.TrimOrNull(currentlyAssignedTo);
            hasBeenAssignedTo = StringHelpers.TrimOrNull(hasBeenAssignedTo);
            awardNumber = StringHelpers.TrimOrNull(awardNumber);
            reasonIncludedInReview = reasonIncludedInReview ?? Empty.StringArray;

            bool hasFilters = false;

            var originalPredicate = PredicateBuilder.Create<Workflow>(wf => true);

            var predicate = originalPredicate;

            if (uloId != null)
            {
                hasFilters = true;
                predicate = predicate.And(wf => wf.TargetUloId == uloId);
            }

            if (pegasysDocumentNumber != null)
            {
                hasFilters = true;
                var criteria = pegasysDocumentNumber.Replace(Wildcard, "");
                if (pegasysDocumentNumber.StartsWith(Wildcard) && pegasysDocumentNumber.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.TargetUlo.PegasysDocumentNumber.Contains(criteria));
                }
                else if (pegasysDocumentNumber.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.PegasysDocumentNumber.TrimEnd().EndsWith(criteria));
                }
                else if (pegasysDocumentNumber.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.PegasysDocumentNumber.TrimStart().StartsWith(criteria));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.PegasysDocumentNumber.Trim() == criteria);
                }
            }

            if (organization != null)
            {
                hasFilters = true;
                var criteria = organization.Replace(Wildcard, "");
                if (organization.StartsWith(Wildcard) && organization.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.TargetUlo.Organization.Contains(criteria));
                }
                else if (organization.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.Organization.Trim().EndsWith(criteria));
                }
                else if (organization.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.Organization.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate =
                        predicate.And(wf => wf.TargetUlo.Organization.Trim() == criteria);
                }
            }

            if (regions != null && regions.Count > 0)
            {
                hasFilters = true;
                predicate = predicate.And(wf => wf.TargetUlo.RegionId != null && regions.Contains((int)wf.TargetUlo.RegionId));
            }

            if (zones != null && zones.Count > 0)
            {
                hasFilters = true;
                predicate = predicate.And(wf => zones.Contains(wf.TargetUlo.Region.ZoneId));
            }

            if (fund != null)
            {
                hasFilters = true;
                var criteria = fund.Replace(Wildcard, "");
                if (fund.StartsWith(Wildcard) && fund.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.TargetUlo.Fund.Contains(criteria));
                }
                else if (fund.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.Fund.Trim().EndsWith(criteria));
                }
                else if (fund.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.Fund.StartsWith(criteria));
                }
                else
                {
                    predicate = predicate.And(wf => wf.TargetUlo.Fund.Trim() == criteria);
                }
            }

            if (baCode != null && baCode.Count > 0)
            {
                hasFilters = true;
                predicate = predicate.And(wf => baCode.Contains(wf.TargetUlo.Prog.Trim()));
            }

            if (pegasysTitleNumber != null)
            {
                hasFilters = true;
                var criteria = pegasysTitleNumber.Replace(Wildcard, "");
                if (pegasysTitleNumber.StartsWith(Wildcard) && pegasysTitleNumber.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.TargetUlo.PegasysTitleNumber.Contains(criteria));
                }
                else if (pegasysTitleNumber.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.PegasysTitleNumber.Trim().EndsWith(criteria));
                }
                else if (pegasysTitleNumber.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.PegasysTitleNumber.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf =>
                                wf.TargetUlo.PegasysTitleNumber.Trim() ==
                                criteria);
                }
            }

            if (pegasysVendorName != null)
            {
                hasFilters = true;
                var criteria = pegasysVendorName.Replace(Wildcard, "");
                if (pegasysVendorName.StartsWith(Wildcard) && pegasysVendorName.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.TargetUlo.VendorName.Contains(criteria));
                }
                else if (pegasysVendorName.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.VendorName.Trim().EndsWith(criteria));
                }
                else if (pegasysVendorName.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.VendorName.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate =
                        predicate.And(wf =>
                            wf.TargetUlo.VendorName.Trim() == criteria);
                }
            }

            if (docType != null && docType.Count > 0)
            {
                hasFilters = true;
                predicate = predicate.And(wf => docType.Contains(wf.TargetUlo.DocType));
            }

            if (contractingOfficersName != null)
            {
                hasFilters = true;
                var criteria = contractingOfficersName.Replace(Wildcard, "");
                if (contractingOfficersName.StartsWith(Wildcard) && contractingOfficersName.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.TargetUlo.ContractingOfficersName.Contains(criteria));
                }
                else if (contractingOfficersName.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.ContractingOfficersName.Trim().EndsWith(criteria));
                }
                else if (contractingOfficersName.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.ContractingOfficersName.StartsWith(criteria));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf =>
                                wf.TargetUlo.ContractingOfficersName.Trim() ==
                                criteria);
                }
            }

            if (currentlyAssignedTo != null)
            {
                hasFilters = true;
                var criteria = currentlyAssignedTo.Replace(Wildcard, "");
                if (currentlyAssignedTo.StartsWith(Wildcard) && currentlyAssignedTo.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.OwnerUser.UserName.Contains(criteria));
                }
                else if (currentlyAssignedTo.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.OwnerUser.UserName.Trim().EndsWith(criteria));
                }
                else if (currentlyAssignedTo.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.OwnerUser.UserName.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate = predicate.And(wf => wf.OwnerUser.UserName.Trim() == currentlyAssignedTo);
                }

            }

            if (hasBeenAssignedTo != null)
            {
                hasFilters = true;
                var criteria = hasBeenAssignedTo.Replace(Wildcard, "");
                if (hasBeenAssignedTo.StartsWith(Wildcard) && hasBeenAssignedTo.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.WorkflowWorkflowHistorys.Any(wfh => wfh.OwnerUser.UserName.Contains(criteria)));
                }
                else if (hasBeenAssignedTo.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.WorkflowWorkflowHistorys.Any(wfh => wfh.OwnerUser.UserName.EndsWith(criteria)));
                }
                else if (hasBeenAssignedTo.EndsWith(Wildcard))
                {
                    predicate =
                     predicate.And(
                         wf => wf.WorkflowWorkflowHistorys.Any(wfh => wfh.OwnerUser.UserName.StartsWith(criteria)));
                }
                else
                {
                    predicate =
                     predicate.And(
                         wf => wf.WorkflowWorkflowHistorys.Any(wfh => wfh.OwnerUser.UserName == criteria));
                }
            }

            if (awardNumber != null)
            {
                hasFilters = true;
                var criteria = awardNumber.Replace(Wildcard, "");
                if (awardNumber.StartsWith(Wildcard) && awardNumber.EndsWith(Wildcard))
                {
                    predicate =
                       predicate.And(
                           wf => wf.TargetUlo.AwardNbr.Contains(criteria));
                }
                else if (awardNumber.StartsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.AwardNbr.Trim().EndsWith(criteria));
                }
                else if (awardNumber.EndsWith(Wildcard))
                {
                    predicate =
                        predicate.And(
                            wf => wf.TargetUlo.AwardNbr.Trim().StartsWith(criteria));
                }
                else
                {
                    predicate = predicate.And(wf => wf.TargetUlo.AwardNbr.Trim() == criteria);
                }

            }

            if (reasonIncludedInReview != null && reasonIncludedInReview.Count > 0)
            {
                hasFilters = true;
                predicate = predicate.And(wf => reasonIncludedInReview.Contains(wf.TargetUlo.ReasonIncludedInReview.Trim()));
            }

            if (valid != null && valid.Count == 1)
            {
                hasFilters = true;
                var v = valid[0];
                predicate = predicate.And(wf => wf.TargetUlo.Valid == v);
            }

            if (status != null && status.Count > 0)
            {
                hasFilters = true;
                predicate = predicate.And(wf => status.Contains(wf.TargetUlo.Status.Trim()));
            }

            if (reviewId != null && reviewId.Count > 0)
            {
                hasFilters = true;
                predicate = predicate.And(wf => reviewId.Contains(wf.TargetUlo.ReviewId));
            }

            if (reassignableByMe.GetValueOrDefault())
            {
                hasFilters = true;
                var regionIds = GetUserGroupRegions(currentUser, GetUserId("Reassign Group"));
                predicate = predicate.And(GetWorkflowsRegionIdPredicate(regionIds));
            }

            return hasFilters ? predicate : null;
        }

        public EmailTemplate GetEmailTemplate(int emailTemplateId)
            => Cacher.FindOrCreateValue(
                   Cache.CreateKey(emailTemplateId, nameof(GetEmailTemplate)),
                   () => DB.EmailTemplates.AsNoTracking().FirstOrDefault(z => z.EmailTemplateId == emailTemplateId),
                    MediumCacheTimeout);

        public Task<IQueryable<BusinessLayer.Data.Reporting.ReportDescription>> GetReportsAsync(string name = null)
            => Task.FromResult(Cacher.FindOrCreateValue(
                Cache.CreateKey(nameof(GetReportsAsync), name),
                () => DB.ReportDefinitions.Where(rd => rd.IsActive == true).ConvertAll(rd => rd.Description).WhereNotNull().ToList().AsReadOnly()
                ).AsQueryable().Where(z => name == null || z.Name == name));
    }
}
