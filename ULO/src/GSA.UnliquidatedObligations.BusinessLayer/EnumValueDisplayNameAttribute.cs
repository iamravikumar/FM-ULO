using System;

namespace GSA.UnliquidatedObligations.BusinessLayer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumValueDisplayNameAttribute : Attribute
    {
        public string DisplayName { get; private set; }

        public EnumValueDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public static string GetDisplayName(Enum e)
        {
            foreach (EnumValueDisplayNameAttribute a in e.GetType().GetCustomAttributes(typeof(EnumValueDisplayNameAttribute), true))
            {
                return a.DisplayName;
            }
            return e.ToString();
        }
    }
}
