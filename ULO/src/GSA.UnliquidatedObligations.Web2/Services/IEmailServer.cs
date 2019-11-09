namespace GSA.UnliquidatedObligations.Web.Services
{
    public interface IEmailServer
    {
        void SendEmail(string subject, string body, string bodyHtml, string recipient);
    }
}
