using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System;

namespace GSA.UnliquidatedObligations.Web.Models
{

    public class ReviewItemModel {

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

        [Required(ErrorMessage = "Workflow Definition is required")]
        public int? WorkflowDefinitionId { get; set; }
        public IList<SelectListItem> WorkflowDefinitions { get; set; }
        [Required(ErrorMessage = "Review Name is required")]
        public string ReviewName { get; set; }

        [Required(ErrorMessage = "Review Date is required")]
        [DataType(DataType.Date)]
        public DateTime ReviewDateInitiated { get; set; }
        public string Comments { get; set; }

        //[Required(ErrorMessage = "Project Due Date is required")]
        //[DataType(DataType.Date)]
        //public DateTime? ProjectDueDate { get; set; }

        public ReviewModel(List<int> regionChoices, List<ReviewTypeEnum> reviewTypeEnums, List<ReviewScopeEnum> reviewScopeEnums, List<WorkflowDefinition> workflowDefinitions)
        {
            RegionChoices = regionChoices.OrderBy(rc => rc).ToList().ConvertToSelectList();
            ReviewTypes = reviewTypeEnums.ConvertToSelectList();
            ReviewScopes = reviewScopeEnums.ConvertToSelectList();
            WorkflowDefinitions = workflowDefinitions.ConvertToSelectList();
            ReviewDateInitiated = DateTime.Today;
        }

        public ReviewModel()
        {

        }

        //private static List<SelectListItem> ConvertToSelectList(List<string> rev)
        //{
        //    var regionsSelect = new List<SelectListItem>();

        //    foreach (var region in regions)
        //    {
        //        regionsSelect.Add(new SelectListItem { Text = region.ToString(), Value = region.ToString() });
        //    }
        //    return regionsSelect;

        //}


    }

    public class ReviewDetailsModel
    {
        public Review Review { get; set; }
        public ReviewUploadStatsModel ReviewUploadStats { get; set; }

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
        public ReviewUploadStatsModel(ReviewStat reviewStat)
        {
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

        public List<string> PegasysFilePathsList { get; set; }

        public List<string> RetaFileList { get; set; }

        public List<string> EasiFileList { get; set; }

        public List<string> One92FileList { get; set; }

        public UploadFilesModel(int reviewId)
        {
            ReviewId = reviewId;
            PegasysFilePathsList = new List<string>();
            RetaFileList = new List<string>();
            EasiFileList = new List<string>();
            One92FileList = new List<string>();
        }
    }


}