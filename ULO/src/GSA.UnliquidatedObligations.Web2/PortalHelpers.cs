using System;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.Web
{
    public class PortalHelpers
    {
        public TimeZoneInfo DisplayTimeZone =>
            TimeZoneInfo.FindSystemTimeZoneById(ConfigOptions.Value.TimezoneId);

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


        public PortalHelpers(IOptions<SprintConfig> sprintConfigOptions, IOptions<Config> configOptions, IOptions<Controllers.AccountController.Config> accountConfigOptions)
        {
            Requires.NonNull(sprintConfigOptions, nameof(sprintConfigOptions));
            Requires.NonNull(configOptions, nameof(configOptions));
            Requires.NonNull(accountConfigOptions, nameof(accountConfigOptions));

            SprintConfigOptions = sprintConfigOptions;
            ConfigOptions = configOptions;
            AccountConfigOptions = accountConfigOptions;
        }


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
