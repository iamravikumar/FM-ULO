using System.Runtime.Serialization;
using GSA.UnliquidatedObligations.BusinessLayer.Helpers;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    [DataContract(Namespace = ClaimTypePrefix)]
    public class SubjectCatagoryClaimValue : RegionalClaimValue
    {
        public const string ClaimType = ClaimTypePrefix+"SubjectCatagoryClaim";
        private static readonly DataContractSerializer Serializer = new DataContractSerializer(typeof(SubjectCatagoryClaimValue), new[] { typeof(RegionalClaimValue) });

        public SubjectCatagoryClaimValue()
            : base(Serializer)
        { }

        public static SubjectCatagoryClaimValue Load(string xml)
        {
            return (SubjectCatagoryClaimValue)Serializer.ReadObject(xml);
        }


        [DataMember(Name = "DocumentType")]
        public string DocType { get; set; }

        [DataMember]
        public string BACode { get; set; }

        [DataMember]
        public string OrgCode { get; set; }

    }
}
