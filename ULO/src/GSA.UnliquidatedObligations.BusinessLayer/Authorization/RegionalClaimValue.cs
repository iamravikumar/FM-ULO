using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    public abstract class RegionalClaimValue
    {
        internal const string ClaimTypePrefix = UloHelpers.UloUrn+"claims/";
        /// <summary>
        /// The Region to which this claim applies.  
        /// When null, this claim applies to every region.
        /// </summary>
        [JsonProperty("regionNumber")]
        public RegionNumbers Regions { get; set; }
        
        protected RegionalClaimValue(RegionNumbers region=RegionNumbers.AllRegions)
        {
            Regions = region;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        private static readonly ICollection<Func<string, RegionalClaimValue>> CreateFromJsonFunctions =
            new List<Func<string, RegionalClaimValue>>
            {
                ApplicationPermissionClaimValue.CreateFromJson,
                SubjectCatagoryClaimValue.CreateFromJson,
            };

        public static RegionalClaimValue CreateFromJson(string json)
        {
            foreach (var f in CreateFromJsonFunctions)
            {
                try
                {
                    var rcv = (RegionalClaimValue)f(json);
                    return rcv;
                }
                catch (Exception) { }
            }
            return null;
        }
    }
}
