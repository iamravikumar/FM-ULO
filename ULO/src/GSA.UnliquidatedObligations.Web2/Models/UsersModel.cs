using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
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
        [MaxLength(256)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "User Type")]
        public string UserType { get; set; }

        public IList<string> Claims { get; set; } = new List<string>();

        public IList<AspnetUserSubjectCategoryClaim> SubjectCategoryClaims { get; set; } = new List<AspnetUserSubjectCategoryClaim>();

        public IList<string> Permissions { get; set; } = new List<string>();

        public IList<string> Groups { get; set; } = new List<string>();

        public IList<int> GroupMembershipRegionIds { get; set; } = new List<int>();

        public UserModel() { }

        public UserModel(AspNetUser user, IEnumerable<UserUser> groups, IEnumerable<AspnetUserApplicationPermissionClaim> applicationPermissionClaim, IEnumerable<AspnetUserSubjectCategoryClaim> subjectCategoryClaims)
        {
            UserName = user.UserName;
            UserId = user.Id;
            Email = user.Email;
            UserType = user.UserType;
            Groups = groups.ConvertAll(z => z.ParentUser.UserName).Distinct().OrderBy().ToList();
            GroupMembershipRegionIds = groups.Where(z=>z.RegionId!=null).ConvertAll(z => z.RegionId.Value).Distinct().ToList();
            var d = subjectCategoryClaims.ToDictionaryOnConflictKeepLast(c => Cache.CreateKey(c.DocumentType, c.BACode, c.OrgCode, c.Region), c => c);
            SubjectCategoryClaims = d.Values.OrderBy(c=>c.DocumentType).ThenBy(c=>c.BACode).ThenBy(c=>c.OrgCode).ThenBy(c=>c.Region).ToList();
            Claims = SubjectCategoryClaims.ConvertAll(z=>z.ToFriendlyString()).Distinct().OrderBy().ToList();
            Permissions = applicationPermissionClaim.ConvertAll(z => z.PermissionName).Distinct().OrderBy().ToList();
        }
    }
}
