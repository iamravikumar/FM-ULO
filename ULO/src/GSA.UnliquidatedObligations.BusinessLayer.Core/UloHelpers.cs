using System;

namespace GSA.UnliquidatedObligations.BusinessLayer
{
    public static class UloHelpers
    {
        public const string GsaUrn = "urn:gsa.gov/";
        public const string UloUrn = GsaUrn + "unliquidatedObligation/";
        public const string WorkflowDescUrn = UloUrn + "WorkflowDescriptions/";

        public static string ToRfc8601(this DateTime dt)
            => dt.ToUniversalTime().ToString("o");

        internal static string CreatePdnWithInstance(string pegasysDocumentNumber, int? pegasysDocumentNumberInstance)
            => pegasysDocumentNumberInstance.HasValue ? $"{pegasysDocumentNumber}.{pegasysDocumentNumberInstance}" : $"{pegasysDocumentNumber}";
    }
}
