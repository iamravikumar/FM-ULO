using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    public class ApplicationPermissionClaimValue : RegionalClaimValue
    {
        public const string ClaimType = ClaimTypePrefix + "ApplicationPermissionClaim";

        /// <summary>
        /// The application permission which is granted
        /// </summary>
        [JsonProperty("applicationPermissionName")]
        public ApplicationPermissionNames ApplicationPermissionName { get; set; }

        public static new ApplicationPermissionClaimValue CreateFromJson(string json)
        {
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<ApplicationPermissionClaimValue>(json);
        }
    }
}
