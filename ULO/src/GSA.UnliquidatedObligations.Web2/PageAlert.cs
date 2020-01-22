using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.Web
{
    public class PageAlert
    {
        internal const string PageAlertsKey = "PAK";

        internal static IList<PageAlert> GetPageAlerts(ITempDataDictionary tdd)
        {
            var json = tdd.Peek(PageAlertsKey) as string;
            var alerts = new List<PageAlert>();
            if (json != null && json != "")
            { 
                var z = JsonConvert.DeserializeObject<IList<PageAlert>>(json);
                alerts.AddRange(z);
            }
            return alerts;
        }

        internal static void SetPageAlerts(ITempDataDictionary tdd, IList<PageAlert> alerts)
        {
            var json = JsonConvert.SerializeObject(alerts);
            tdd[PageAlertsKey] = json;
        }

        public static IList<PageAlert> GetThenClearAllMyPageAlerts(ITempDataDictionary tdd, ViewDataDictionary vdd)
        {
            var alerts = new List<PageAlert>();
            var z = vdd[PageAlertsKey] as IList<PageAlert>;
            if (z!=null)
            {
                alerts.AddRange(z);
            }
            var s = tdd[PageAlertsKey] as string;
            if (s != null)
            {
                z = JsonConvert.DeserializeObject<IList<PageAlert>>(s);
                alerts.AddRange(z);
                tdd.Remove(PageAlertsKey);
            }
            return alerts;
        }

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

        public PageAlert(string message, bool autoDismiss = true, AlertTypes alertType = PageAlert.AlertTypes.Info)
        {
            Message = message;
            AutoDismiss = autoDismiss;
            AlertType = alertType;
        }
    }
}
