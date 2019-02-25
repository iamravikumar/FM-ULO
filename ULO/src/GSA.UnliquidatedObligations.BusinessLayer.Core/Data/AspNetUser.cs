using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using RevolutionaryStuff.Core.ApplicationParts;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
    public partial class AspNetRoleClaim
    {
        object IPrimaryKey.Key { get { return Id; } }

        int IPrimaryKey<int>.Key { get { return Id; } }

        //LinksTo:dbo.AspNetRoles
        [ForeignKey("RoleId")]
        [IgnoreDataMember]
        public AspNetRole Role { get; set; }
    }

    public partial class AspNetRole
    {
        object IPrimaryKey.Key { get { return Id; } }

        string IPrimaryKey<string>.Key { get { return Id; } }

    }

    public partial class AspNetUserRole
    {
        //LinksTo:dbo.AspNetUsers
        [ForeignKey("UserId")]
        [IgnoreDataMember]
        public AspNetUser User { get; set; }

        //LinksTo:dbo.AspNetRoles
        [ForeignKey("RoleId")]
        [IgnoreDataMember]
        public AspNetRole Role { get; set; }
    }

    public partial class AspNetUserClaim
    {
        object IPrimaryKey.Key { get { return Id; } }

        int IPrimaryKey<int>.Key { get { return Id; } }

        //LinksTo:dbo.AspNetUsers
        [ForeignKey("UserId")]
        [IgnoreDataMember]
        public AspNetUser User { get; set; }
    }

    public partial class AspNetUserLogin
    {
        //LinksTo:dbo.AspNetUsers
        [ForeignKey("UserId")]
        [IgnoreDataMember]
        public AspNetUser User { get; set; }
    }

    public partial class AspNetUser
    {
        public static class UserTypes
        {
            public const string Person = "Person";
            public const string Group = "Group";
            public const string System = "System";
        }

        public bool IsPerson => 0 == string.Compare(UserTypes.Person, UserType, true);
        public bool IsGroup => 0 == string.Compare(UserTypes.Group, UserType, true);
        public bool IsSystem => 0 == string.Compare(UserTypes.System, UserType, true);

        object IPrimaryKey.Key { get { return Id; } }

        string IPrimaryKey<string>.Key { get { return Id; } }

        partial void OnToString(ref string extras)
        {
            extras = $"type={this.UserType} name=[{this.UserName}]";
        }
    }
}
