using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Authorization;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using Serilog;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanViewReviews)]
    public class PdnController : BasePageController
    {
        public const string AllRegionsKey = "*";
        public const string Name = "Pdn";

        public static class ActionNames
        {
            public const string Index = "Index";
            public const string Details = "Details";
            public const string Save = "Save";
        }

        public PdnController(UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, ILogger logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        { }

        [ActionName(ActionNames.Index)]
        [Route("pdns")]
        public ActionResult Index()
        {
            var summary =               
                from p in DB.PdnAttributes
                where p.IsExcluded
                group p by p.RegionId into g
                select new { RegionId = g.Key, Cnt = g.Count() };

            var d = DB.Regions.ToDictionary(r => r.RegionId);

            var items = new List<PdnAttributeListItem>
            {
                /*
                 * uncomment if we want exlcuded PDNs that span all regions
                new PdnAttributeListItem { ExcludedCount = summary.FirstOrDefault(z => z.RegionId == null)?.Cnt ?? 0 }
                */
            };
            foreach (var s in summary)
            {
                if (s.RegionId == null) continue;
                items.Add(new PdnAttributeListItem { ExcludedCount = s.Cnt, Region = d[s.RegionId.Value] });
                d.Remove(s.RegionId.Value);
            }
            d.Values.ForEach(z => items.Add(new PdnAttributeListItem { Region = z }));
            items.Sort();
            return View(items);
        }

        [ActionName(ActionNames.Details)]
        [Route("pdns/{regionKey}")]
        public ActionResult Details(string regionKey)
        {
            var m = new PdnDetailsModel
            {
                RegionKey = regionKey
            };
            int regionId;
            if (int.TryParse(regionKey, out regionId))
            {
                m.Region = DB.Regions.Find(regionId);
            }
            var pdns = DB.PdnAttributes.Where(z =>
                (regionKey == AllRegionsKey && z.RegionId == null) ||
                (regionKey != AllRegionsKey && z.RegionId == regionId)).ToList();
            pdns.Sort((a,b)=>a.PegasysDocumentNumber.CompareTo(b.PegasysDocumentNumber));
            m.ExcludedPDNs = pdns;
            return View(m);
        }

        private static readonly Regex PdnFinderExpr = new Regex("\\b(\\w[\\w\\d]*)", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        [ActionName(ActionNames.Save)]
        [Route("pdns/{regionKey}/save")]
        public async Task<ActionResult> Save(string regionKey)
        {
            int? regionId = Parse.ParseNullableInt32(regionKey);
            var input = Request.Form["pdns"];
            var pdns = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            foreach (Match m in PdnFinderExpr.Matches(input))
            {
                pdns.Add(m.Groups[1].Value);
            }


            var paByPdn = DB.PdnAttributes.Where(z =>
                (regionKey == AllRegionsKey && z.RegionId == null) ||
                (regionKey != AllRegionsKey && z.RegionId == regionId.Value)).ToDictionary(z => z.PegasysDocumentNumber, Comparers.CaseInsensitiveStringComparer);
            foreach (var pa in paByPdn.Values)
            {
                if (!pdns.Contains(pa.PegasysDocumentNumber))
                {
                    DB.PdnAttributes.Remove(pa);
                }
            }

            foreach (var pdn in pdns)
            {
                if (paByPdn.ContainsKey(pdn)) continue;
                DB.PdnAttributes.Add(new PdnAttribute
                {
                    IsExcluded = true,
                    PegasysDocumentNumber = pdn,
                    RegionId = regionId,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            await DB.SaveChangesAsync();

            return RedirectToIndex();
        }
    }
}
