using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Models
{

    public class ReviewItemModel
    {
        public int ReviewId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReviewName { get; set; }
        public string Status { get; set; }
        public string ReviewTypeName { get; set; }
        public string ScopeName { get; set; }
        public string ReviewAsofDate { get; set; }
        public string Comments { get; set; }
        public string RegionName { get; set; }

        public ReviewItemModel()
        {

        }

        public ReviewItemModel(Review review)
        {
            ReviewId = review.ReviewId;
            CreatedAt = review.CreatedAt;
            ReviewName = review.ReviewName;
            Status = review.Status;

            var reviewTypeEnum = (ReviewTypeEnum)review.ReviewTypeId;
            ReviewTypeName = reviewTypeEnum.GetDisplayName();

            var scopeEnum = (ReviewScopeEnum)review.ReviewScopeId;
            ScopeName = scopeEnum.GetDisplayName();

            Comments = review.Comments;
            RegionName = review.Region.RegionName;
        }
    }

    public class ReviewModel
    {
        public IList<SelectListItem> RegionChoices { get; set; }

        public int? RegionId { get; set; }

        [Required(ErrorMessage = "Review Type is required")]
        public int? ReviewTypeId { get; set; }

        public IList<SelectListItem> ReviewTypes { get; set; }

        [Required(ErrorMessage = "Review Scope is required")]
        public int? ReviewScopeId { get; set; }

        public IList<SelectListItem> ReviewScopes { get; set; }

        [MaxLength(100)]
        public string ReviewName { get; set; }

        [Required(ErrorMessage = "Review Date is required")]
        [DataType(DataType.Date)]
        public DateTime ReviewDateInitiated { get; set; }

        [MaxLength(500)]
        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }


        public ReviewModel()
        {
            ReviewTypes = Enum.GetValues(typeof(ReviewTypeEnum)).Cast<ReviewTypeEnum>().ToList().ConvertToSelectList();
            ReviewScopes = Enum.GetValues(typeof(ReviewScopeEnum)).Cast<ReviewScopeEnum>().ToList().ConvertToSelectList();
        }

        public ReviewModel(IList<int> permissableRegionIds)
            : this()
        {
            var permissibleVals = permissableRegionIds.ConvertAll(r => r.ToString()).Distinct().ToDictionary(z => z, z=>true);
            RegionChoices = PortalHelpers.CreateRegionSelectListItems().Where(s => permissibleVals.ContainsKey(s.Value)).ToList();
            ReviewDateInitiated = DateTime.Today;
        }
    }

    public class ReviewDetailsModel
    {
        public Review Review { get; set; }
        public ReviewUploadStatsModel ReviewUploadStats { get; set; }

        public ReviewDetailsModel()
        { }

        public ReviewDetailsModel(Review review, ReviewStat reviewStats)
        {
            Review = review;
            ReviewUploadStats = new ReviewUploadStatsModel(reviewStats);
        }
    }

    public class ReviewUploadStatsModel
    {
        public int PO442UploadStats { get; set; }
        public int RetaUploadStats { get; set; }
        public int EasiUploadStats { get; set; }
        public int PO192Stats { get; set; }
        public int UlosCreatedStats { get; set; }
        public int AssignedToPersonStats { get; set; }
        public int AssignedToGroupStats { get; set; }
        public int AssignedToSystemStats { get; set; }
        public int AssignedToSystemTheCloserStats { get; set; }
        public int AssignedToSystePreAssignmentStats { get; set; }

        public int CreditCardAliasesStats { get; set; }
        public int ActiveCardholdersStats { get; set; }
        public int PegasysOpenItemsCreditCardsStats { get; set; }

        public ReviewUploadStatsModel()
        { }

        public ReviewUploadStatsModel(ReviewStat reviewStat)
        {
            CreditCardAliasesStats = reviewStat.CreditCardAliasesCnt.GetValueOrDefault();
            ActiveCardholdersStats = reviewStat.ActiveCardholdersCnt.GetValueOrDefault();
            PegasysOpenItemsCreditCardsStats = reviewStat.PegasysOpenItemsCreditCardsCnt.GetValueOrDefault();
            PO442UploadStats = reviewStat.PegasysObligations442Cnt.GetValueOrDefault();
            RetaUploadStats = reviewStat.RetaCnt.GetValueOrDefault();
            EasiUploadStats = reviewStat.EasiCnt.GetValueOrDefault();
            PO192Stats = reviewStat.PegasysObligations192Cnt.GetValueOrDefault();
            UlosCreatedStats = reviewStat.UloCnt.GetValueOrDefault();
            AssignedToPersonStats = reviewStat.PersonCnt.GetValueOrDefault(); 
            AssignedToGroupStats = reviewStat.GroupCnt.GetValueOrDefault();
            AssignedToSystemStats = reviewStat.SystemCnt.GetValueOrDefault();
            AssignedToSystemTheCloserStats = reviewStat.TheCloserCnt.GetValueOrDefault();
            AssignedToSystePreAssignmentStats = reviewStat.PreAssignmentCnt.GetValueOrDefault();
        }
    }

    public class UploadFilesModel
    {
        public int ReviewId { get; set; }

        public IList<string> PegasysFilePathsList { get; set; } = new List<string>();

        public IList<string> RetaFileList { get; set; } = new List<string>();

        public IList<string> EasiFileList { get; set; } = new List<string>();

        public IList<string> One92FileList { get; set; } = new List<string>();

        public IList<string> ActiveCardholderFiles { get; set; } = new List<string>();

        public IList<string> PegasysOpenItemsCreditCards { get; set; } = new List<string>();

        public IList<string> CreditCardAliasCrosswalkFiles { get; set; } = new List<string>();

        public UploadFilesModel(int reviewId)
        {
            ReviewId = reviewId;
        }
    }
}