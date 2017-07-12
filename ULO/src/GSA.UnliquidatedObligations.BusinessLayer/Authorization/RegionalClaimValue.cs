using RevolutionaryStuff.Core;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{

    [DataContract(Namespace = ClaimTypePrefix)]
    public abstract class RegionalClaimValue
    {

        private readonly DataContractSerializer Serializer;

        protected RegionalClaimValue(DataContractSerializer serializer)
        {
            Serializer = serializer;
        }

        public virtual string ToXml()
            => Serializer.WriteObjectToString(this);
    
        [DataMember(Name = "Regions")]
        public HashSet<int> Regions { get; set; }


        internal const string ClaimTypePrefix = UloHelpers.UloUrn+"claims/";
    }
}
