using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class PdnAttributeListItem : IComparable<PdnAttributeListItem>
    {
        public Region Region { get; set; }
        public int ExcludedCount { get; set; }
        public string RegionKey
            => Region == null ? Controllers.PdnController.AllRegionsKey : Region.RegionId.ToString();

        int IComparable<PdnAttributeListItem>.CompareTo(PdnAttributeListItem other)
        {
            var lnum = Parse.ParseInt32(RegionKey, -1);
            var rnum = Parse.ParseInt32(other.RegionKey, -1);
            return lnum.CompareTo(rnum);
        }
    }

    public class PdnDetailsModel
    {
        public string RegionKey { get; set; }
        public Region Region { get; set; }
        public IList<PdnAttribute> ExcludedPDNs { get; set; }
    }
}