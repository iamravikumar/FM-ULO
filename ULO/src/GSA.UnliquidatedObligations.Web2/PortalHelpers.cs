using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.Web2
{
    public class PortalHelpers
    {
        public TimeZoneInfo DisplayTimeZone =>
            TimeZoneInfo.FindSystemTimeZoneById(ConfigOptions.Value.TimezoneId);

        public class Config
        {
            public const string ConfigSectionName = "PortalHelpersConfig";

            public string TimezoneId { get; set; } = "Eastern Standard Time";
        }

        public readonly IOptions<SprintConfig> SprintConfigOptions;

        public readonly IOptions<Config> ConfigOptions;

        public PortalHelpers(IOptions<SprintConfig> sprintConfigOptions, IOptions<Config> configOptions)
        {
            Requires.NonNull(sprintConfigOptions, nameof(sprintConfigOptions));
            Requires.NonNull(configOptions, nameof(configOptions));

            SprintConfigOptions = sprintConfigOptions;
            ConfigOptions = configOptions;
        }
    }
}
