using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    public class SubjectCatagoryClaimValue : RegionalClaimValue
    {
        public const string ClaimType = ClaimTypePrefix+"SubjectCatagoryClaim";

        [JsonProperty("subjectCatagoryName", Required = Required.Always)]
        public SubjectCatagoryNames SubjectCatagoryName { get; set; }

        public static new SubjectCatagoryClaimValue CreateFromJson(string json)
        {
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<SubjectCatagoryClaimValue>(json);
        }
    }
}
