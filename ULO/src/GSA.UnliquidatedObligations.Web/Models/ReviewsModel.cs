using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class ReviewModel
    {
        public List<SelectListItem> RegionChoices { get; set; }
        public Nullable<int> RegionId { get; set; }
        public string ReviewName { get; set; }
        public string ReviewStatus { get; set; }
        public string TypeOfReview { get; set; }
        public string Comments { get; set; }

        public ReviewModel(List<int> regionChoices)
        {
            RegionChoices = ConvertToSelectList(regionChoices.OrderBy(rc => rc).ToList());
            RegionId = null;
            ReviewName = "";
            ReviewStatus = "";
            TypeOfReview = "";
            Comments = "";
        }

        public ReviewModel()
        {
            
        }


        public static List<SelectListItem> ConvertToSelectList(List<int> regions)
        {
            var regionsSelect = new List<SelectListItem>();

            foreach (var region in regions)
            {
                regionsSelect.Add(new SelectListItem { Text = region.ToString(), Value = region.ToString() });
            }
            return regionsSelect;

        }
    }


}