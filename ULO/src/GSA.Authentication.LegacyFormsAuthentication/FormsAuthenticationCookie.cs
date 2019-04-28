using System;

namespace GSA.Authentication.LegacyFormsAuthentication
{
    public class FormsAuthenticationCookie
    {
        public FormsAuthenticationCookie()
        { }

        public override string ToString()
            => $"{base.ToString()} valid={IsWithinValidityWindow} userName=[{UserName}] userData=[{UserData}]";

        public DateTimeOffset Issued { get; set; }
        public DateTimeOffset Expires { get; set; }
        //public bool IsPersistent { get; set; }
        public string UserName { get; set; }
        public string UserData { get; set; }
        public string CookiePath { get; set; }

        public bool IsWithinValidityWindow
            => DateTimeOffset.Now < Expires && DateTimeOffset.Now > Issued;
    }
}
