using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using RevolutionaryStuff.Core.ApplicationParts;

namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{


    public class PersonUser : AspNetUser
    { }

    public class GroupUser : AspNetUser
    { }

    public class SystemUser : AspNetUser
    { }

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

        partial void OnToString(ref string extras)
        {
            extras = $"type={this.UserType} name=[{this.UserName}]";
        }
    }
}
