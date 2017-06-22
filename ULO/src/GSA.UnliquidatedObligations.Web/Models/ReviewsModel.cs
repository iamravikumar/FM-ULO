using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class ReviewModel
    {
        public List<SelectListItem> RegionChoices { get; set; }

        [Required(ErrorMessage = "Region is required")]
        public int? RegionId { get; set; }

        [Required(ErrorMessage = "Review Type is required")]
        public int? ReviewTypeId { get; set; }
        public List<SelectListItem> ReviewTypes { get; set; }

        [Required(ErrorMessage = "Review Scope is required")]
        public int? ReviewScopeId { get; set; }
        public List<SelectListItem> ReviewScopes { get; set; }

        [Required(ErrorMessage = "Workflow Definition is required")]
        public int? WorkflowDefinitionId { get; set; }
        public List<SelectListItem> WorkflowDefinitions { get; set; }
        [Required(ErrorMessage = "Review Name is required")]
        public string ReviewName { get; set; }
        public string Comments { get; set; }

        //[Required(ErrorMessage = "Project Due Date is required")]
        //[DataType(DataType.Date)]
        //public DateTime? ProjectDueDate { get; set; }

        public ReviewModel(List<int> regionChoices, List<ReviewTypeEnum> reviewTypeEnums, List<ReviewScopeEnum> reviewScopeEnums, List<WorkflowDefinition> workflowDefinitions  )
        {
            RegionChoices = regionChoices.OrderBy(rc => rc).ToList().ConvertToSelectList();
            ReviewTypes = reviewTypeEnums.ConvertToSelectList();
            ReviewScopes = reviewScopeEnums.ConvertToSelectList();
            WorkflowDefinitions = workflowDefinitions.ConvertToSelectList();
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