using System.Collections.Generic;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class RequestForReassignmentViewModel
    {
        public int? RequestForReassignmentId { get; set; } 
        public int WorkflowId { get; set; }
        public string SuggestedReviewerId { get; set; }

        public List<SelectListItem> Users { get; set; }

        public int? JustificationId { get; set; }
        public string Comments { get; set; }
        public List<SelectListItem> Justifications { get; set; }

        public RequestForReassignmentViewModel()
        {

        }

        public RequestForReassignmentViewModel(string suggestedReviewerId, int? justificationId, int? requestForReassignmentId, string comments, int workflowId, List<AspNetUser> users, List<JustificationEnum> justificationEnums)
        {
            RequestForReassignmentId = requestForReassignmentId;
            SuggestedReviewerId = suggestedReviewerId;
            JustificationId = justificationId;
            Users = ConvertToSelectList(users);
            Comments = comments;
            Justifications = ConvertToSelectList(justificationEnums);
            WorkflowId = workflowId;

        }

        private List<SelectListItem> ConvertToSelectList(List<AspNetUser> aspNetUsers)
        {
            return aspNetUsers
                .Select(u => new SelectListItem
                {
                    Text = u.UserName,
                    Value = u.Id
                }).ToList();
        }

        private List<SelectListItem> ConvertToSelectList(List<JustificationEnum> justificationEnums)
        {
            var selectList = new List<SelectListItem>();

            foreach (var justificationEnum in justificationEnums)
            {
                var justification = JustificationChoices.Choices[justificationEnum];
                selectList.Add(new SelectListItem
                {
                    Text = justification.JustificationText,
                    Value = justification.JustificationId.ToString()
                });
            }

           return selectList;
        }

    }


}