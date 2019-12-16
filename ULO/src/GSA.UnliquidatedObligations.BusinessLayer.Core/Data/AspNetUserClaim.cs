using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class AspNetUserClaim
    {
        [DisplayName("User Id")]
        [Display(Name = "User Id")]
        [MaxLength(128)]
        [Column("UserId")]
        public override string UserId { get => base.UserId; set => base.UserId = value; }

        //LinksTo:dbo.AspNetUsers
        [ForeignKey("UserId")]
        [JsonIgnore]
        [IgnoreDataMember]
        public AspNetUser User { get; set; }
    }
}
