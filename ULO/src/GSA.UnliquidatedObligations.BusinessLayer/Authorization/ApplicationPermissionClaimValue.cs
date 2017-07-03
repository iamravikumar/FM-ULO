using System.Runtime.Serialization;
using GSA.UnliquidatedObligations.BusinessLayer.Helpers;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    [DataContract(Namespace = ClaimTypePrefix)]
    public class ApplicationPermissionClaimValue : RegionalClaimValue
    {
        public const string ClaimType = ClaimTypePrefix + "ApplicationPermissionClaim";
        private static readonly DataContractSerializer Serializer = new DataContractSerializer(typeof(ApplicationPermissionClaimValue), new[] { typeof(RegionalClaimValue) });

        public ApplicationPermissionClaimValue() : base(Serializer)
        {
        }

        /// <summary>
        /// The application permission which is granted
        /// </summary>
        [DataMember]
        public ApplicationPermissionNames ApplicationPermissionName { get; set; }

        public static ApplicationPermissionClaimValue Load(string xml)
        {
            return (ApplicationPermissionClaimValue) Serializer.ReadObject(xml);
        }
    }
}
