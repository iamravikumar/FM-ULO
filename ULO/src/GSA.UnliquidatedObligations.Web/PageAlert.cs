using System.Threading;

namespace GSA.UnliquidatedObligations.Web
{
    public class PageAlert
    {
        public enum AlertTypes
        {
            Success,
            Info,
            Warning,
            Danger,
        }

        private static long AlertId_s;

        public long AlertId { get; } = Interlocked.Increment(ref AlertId_s);
        public AlertTypes AlertType { get; set; }
        public bool AutoDismiss { get; set; }
        public string Message { get; set; }

        public PageAlert(string message, bool autoDismiss = true, PageAlert.AlertTypes alertType = PageAlert.AlertTypes.Info)
        {
            Message = message;
            AutoDismiss = autoDismiss;
            AlertType = alertType;
        }
    }
}