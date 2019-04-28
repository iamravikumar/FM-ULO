namespace GSA.Authentication.LegacyFormsAuthentication
{
    public interface ILegacyFormsAuthenticationService
    {
        FormsAuthenticationCookie Unprotect(string cookie);
    }
}
