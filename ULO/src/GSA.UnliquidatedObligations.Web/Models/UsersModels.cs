using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class UsersModels
    {
        public List<SelectListItem> Regions { get; set; }

        public int RegionId { get; set; }
        public List<AspNetUser> Users { get; set; }
        public UsersModels(HashSet<int> regions, List<AspNetUser> users)
        {
            Regions = ConvertToSelectList(regions);
            RegionId = Convert.ToInt32(Regions[0].Value);
            Users = users;
        }

        private List<SelectListItem> ConvertToSelectList(HashSet<int> regions)
        {
            var regionsSelect = new List<SelectListItem>();

            foreach (var region in regions)
            {
                regionsSelect.Add(new SelectListItem {Text = region.ToString(), Value = region.ToString()});
            }
            return regionsSelect;

        }
    }
}