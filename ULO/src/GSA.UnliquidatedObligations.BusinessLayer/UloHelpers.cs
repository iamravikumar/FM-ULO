using System;

namespace GSA.UnliquidatedObligations.BusinessLayer
{
    public static class UloHelpers
    {
        public const string GsaUrn = "urn:gsa.gov/";
        public const string UloUrn = GsaUrn + "unliquidatedObligation/";
        public const string WorkflowDescUrn = UloUrn + "WorkflowDescriptions/";

        public static TimeSpan ShortCacheTimeout
            => Properties.Settings.Default.ShortCacheTimeout;

        public static TimeSpan MediumCacheTimeout
            => Properties.Settings.Default.MediumCacheTimeout;

        public static string ToRfc8601(this DateTime dt)
            => dt.ToUniversalTime().ToString("o");
    }
}
