using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class UserModel
    {
        public string UserId { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "User Type")]
        public string UserType { get; set; }

        public IList<string> Claims { get; set; } = new List<string>();

        public IList<AspnetUserSubjectCategoryClaim> SubjectCategoryClaims { get; set; } = new List<AspnetUserSubjectCategoryClaim>();

        public IList<string> Permissions { get; set; } = new List<string>();

        public IList<string> Groups { get; set; } = new List<string>();

        public UserModel() { }

        public UserModel(AspNetUser user, IEnumerable<UserUser> groups, IEnumerable<AspnetUserApplicationPermissionClaim> applicationPermissionClaim, IEnumerable<AspnetUserSubjectCategoryClaim> subjectCategoryClaims)
        {
            UserName = user.UserName;
            UserId = user.Id;
            Email = user.Email;
            UserType = user.UserType;
            Groups = groups.ConvertAll(z => z.ParentUser.UserName).Distinct().OrderBy().ToList();
            SubjectCategoryClaims = subjectCategoryClaims.ToList();
            Claims = SubjectCategoryClaims.ConvertAll(z=>z.ToFriendlyString()).Distinct().OrderBy().ToList();
            Permissions = applicationPermissionClaim.ConvertAll(z => z.PermissionName).Distinct().OrderBy().ToList();
        }
    }
}