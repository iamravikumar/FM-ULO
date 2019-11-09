using System;
using System.Collections.Generic;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                        Logger.Error(ex, "Problem loading timezone with id = {timezoneId}", ConfigOptions.Value.TimezoneId);
                        DisplayTimeZone_p = TimeZoneInfo.Local;
                    }
                }
                return DisplayTimeZone_p;
            }
        }
        private TimeZoneInfo DisplayTimeZone_p;

        public TimeSpan MediumCacheTimeout =>
            ConfigOptions.Value.MediumCacheTimeout;

        public TimeSpan ShortCacheTimeout =>
            ConfigOptions.Value.ShortCacheTimeout;

        public string AdministratorEmail =>
            ConfigOptions.Value.AdministratorEmail;

        public bool UseDevAuthentication =>
            AccountConfigOptions.Value.UseDevAuthentication;

        public class Config
        {
            public const string ConfigSectionName = "PortalHelpersConfig";

            public string TimezoneId { get; set; } = "Eastern Standard Time";

            public TimeSpan MediumCacheTimeout { get; set; } = TimeSpan.Parse("00:05:00");

            public TimeSpan ShortCacheTimeout { get; set; } = TimeSpan.Parse("00:01:00");

            public string AdministratorEmail { get; set; }
        }

        public readonly IOptions<SprintConfig> SprintConfigOptions;

        private readonly IOptions<Config> ConfigOptions;

        private readonly IOptions<Controllers.AccountController.Config> AccountConfigOptions;
        private readonly UloDbContext DB;
        private readonly ICacher Cacher;
        private readonly ILogger Logger;

        public PortalHelpers(IOptions<SprintConfig> sprintConfigOptions, IOptions<Config> configOptions, IOptions<Controllers.AccountController.Config> accountConfigOptions, UloDbContext db, ICacher cacher, ILogger logger)
        {
            Requires.NonNull(sprintConfigOptions, nameof(sprintConfigOptions));
            Requires.NonNull(configOptions, nameof(configOptions));
            Requires.NonNull(accountConfigOptions, nameof(accountConfigOptions));

            SprintConfigOptions = sprintConfigOptions;
            ConfigOptions = configOptions;
            AccountConfigOptions = accountConfigOptions;
            DB = db;
            Cacher = cacher;
            Logger = logger;
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

        public IList<SelectListItem> CreateAllGroupNamesSelectListItems()
        => Cacher.FindOrCreateValue(
            nameof(CreateAllGroupNamesSelectListItems),
            () =>
            DB.AspNetUsers.Where(u => u.UserType == AspNetUser.UserTypes.Group).ConvertAll(
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
        => Cacher.FindOrCreateValue(
            nameof(CreateReviewSelectListItems),
            () =>
                DB.Reviews.OrderByDescending(r => r.ReviewId).ConvertAll(
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
    }
}
