using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSA.UnliquidatedObligations.BusinessLayer.Helpers;

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
        {
            return Serializer.WriteObject(this);
        }

    
        [DataMember(Name = "Regions")]
        public HashSet<int> Regions { get; set; }


        internal const string ClaimTypePrefix = UloHelpers.UloUrn+"claims/";
        /// <summary>
        /// The Region to which this claim applies.  
        /// When null, this claim applies to every region.
        /// </summary>
        //[JsonProperty("regionNumber")]
        //public RegionNumbers Regions { get; set; }

        //[JsonProperty("regionIds")]
        //public HashSet<int> RegionIds { get; set; }

        //protected RegionalClaimValue(HashSet<int> RegionIds = null)
        //{
        //    if (RegionIds != null)
        //    {
        //        RegionIds = RegionIds;
        //    }
        //    else
        //    {
        //        RegionIds = new HashSet<int>()
        //        {
        //            1,
        //            2,
        //            3,
        //            4,
        //            5,
        //            6,
        //            7,
        //            8,
        //            9,
        //            10,
        //            11
        //        };
        //    }
        //}

        //public string ToJson()
        //{
        //    return JsonConvert.SerializeObject(this);
        //}


        //private static readonly ICollection<Func<string, RegionalClaimValue>> CreateFromJsonFunctions =
        //    new List<Func<string, RegionalClaimValue>>
        //    {
        //        ApplicationPermissionClaimValue.CreateFromJson,
        //        SubjectCatagoryClaimValue.CreateFromJson,
        //    };

        //public static RegionalClaimValue CreateFromJson(string json)
        //{
        //    foreach (var f in CreateFromJsonFunctions)
        //    {
        //        try
        //        {
        //            var rcv = (RegionalClaimValue)f(json);
        //            return rcv;
        //        }
        //        catch (Exception) { }
        //    }
        //    return null;
        //}
    }
}
