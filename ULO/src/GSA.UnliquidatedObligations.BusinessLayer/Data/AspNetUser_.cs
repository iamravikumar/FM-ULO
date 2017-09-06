namespace GSA.UnliquidatedObligations.BusinessLayer.Data
{
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

        public override string ToString()
            => $"{this.GetType().Name} type={this.UserType} name=[{this.UserName}] id=[{this.Id}]";
    }
}
