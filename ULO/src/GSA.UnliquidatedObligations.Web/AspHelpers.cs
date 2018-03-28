using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace GSA.UnliquidatedObligations.Web
{
    public static class AspHelpers
    {
        public const string PleaseSelectOne = "-- Please Select --";

        public const string NoResultsMessage = "There are not currently any results to show";

        public static SelectListItem CreatePleaseSelectListItem(bool selected=true)
            => new SelectListItem
            {
                Disabled = true,
                Selected = selected,
                Text = PleaseSelectOne,                    
            };

        public const string SortDirAscending = "asc";
        public const string SortDirDescending = "desc";

        public static DateTime ToTimeZone(this DateTime dt, TimeZoneInfo zoneInfo)
            => TimeZoneInfo.ConvertTime(dt, zoneInfo);

        public static bool IsSortDirAscending(string sortDir)
            => !(0 == string.Compare(SortDirDescending, sortDir, true));

        public static string GetDisplayName(Enum e)
            => e.GetCustomAttributes<DisplayAttribute>().FirstOrDefault(a => a.Name != null)?.Name ?? e.ToString();

        public static string GetDisplayDescription(Enum e)
        {
            var da = e.GetCustomAttributes<DisplayAttribute>().FirstOrDefault(a => a.Description != null);
            return da?.Description ?? da.Name ?? e.ToString();
        }

        public static string DisplayIsValid(this HtmlHelper hh, bool? isValid)
            => isValid.HasValue ? (isValid.Value ? "Yes" : "No") : null;

        public static string FriendlyNameFor<TModelItem, TResult>(this HtmlHelper<TModelItem> hh, Expression<Func<TModelItem, TResult>> columnExpression)
        {
            var fieldMemberInfo = columnExpression.GetMembers().Last();
            if (fieldMemberInfo != null)
            {
                var dna = fieldMemberInfo.GetCustomAttribute<DisplayNameAttribute>();
                if (dna != null) return dna.DisplayName;
            }
            return fieldMemberInfo.Name;
        }

        public static string FriendlyNameFor<TModelItem, TResult>(this HtmlHelper<IEnumerable<TModelItem>> hh, Expression<Func<TModelItem, TResult>> columnExpression)
        {
            try
            {
                var fieldMemberInfo = columnExpression.GetMembers().Last();
                if (fieldMemberInfo != null)
                {
                    var dna = fieldMemberInfo.GetCustomAttribute<DisplayNameAttribute>();
                    if (dna != null) return dna.DisplayName;
                }
            }
            catch { }
            return hh.DisplayNameFor(columnExpression).ToString();
        }

        public static MvcHtmlString SortableHeaderFor<TModelItem, TResult>(this HtmlHelper<IEnumerable<TModelItem>> hh, Expression<Func<TModelItem, TResult>> columnExpression, string currentSortColName = null, string currentSortDir = null, string actionName = null, string overrideDisplayName=null)
        {
            currentSortColName = currentSortColName ?? hh.ViewBag.SortCol as string;
            currentSortDir = currentSortDir ?? hh.ViewBag.SortDir as string;

            actionName = actionName ?? hh.ViewContext.RouteData.Values["action"] as string;
            var colName = columnExpression.GetFullyQualifiedName();
            var displayName = overrideDisplayName ?? hh.FriendlyNameFor(columnExpression);

            var routeValues = new System.Web.Routing.RouteValueDictionary();
            foreach (string key in hh.ViewContext.HttpContext.Request.QueryString.Keys)
            {
                if (key == null) continue;
                var val = hh.ViewContext.HttpContext.Request.QueryString[key];
                if (val != null)
                {
                    routeValues[key] = val;
                }
            }
            routeValues["sortCol"] = colName;
            if (colName == currentSortColName)
            {
                routeValues["sortDir"] = IsSortDirAscending(currentSortDir) ? SortDirDescending : SortDirAscending;
                var h = hh.ActionLink(
                    displayName,
                    actionName,
                    routeValues);
                h = h.AppendChildHtml(currentSortDir == SortDirAscending ? " <span class='caret-up'>^</span>" : " <span class='caret-down'>v</span>");
                return h;
            }
            else
            {
                routeValues["sortDir"] = SortDirAscending;
                return hh.ActionLink(displayName, actionName, routeValues);
            }
        }

        public static void SetTitles(this WebViewPage page, PageKeys pageKey, string title, string subTitle=null, string browserTitle=null)
        {
            page.ViewBag.PageKey = pageKey;
            page.ViewBag.Title = browserTitle??title;
            page.ViewBag.PageTitle = title;
            page.ViewBag.PageSubTitle = subTitle;
        }

        private static readonly Regex BeginningOfTheEnd = new Regex(
            @"(.+)(\<\/\w+>\s*)", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static MvcHtmlString AppendChildHtml(this MvcHtmlString hs, string newChildHtml)
        {
            if (newChildHtml == null || newChildHtml == "") return hs;
            var html = hs.ToHtmlString();
            var m = BeginningOfTheEnd.Match(html);
            if (m.Success)
            {
                html = m.Groups[1].Value + newChildHtml + m.Groups[2].Value;
                hs = new MvcHtmlString(html);
            }
            return hs;
        }

        public static string ActionLinkUrl(this HtmlHelper hh, string actionName, string controllerName, object routeValues)
        {
            var a = hh.ActionLink("jbt", actionName, controllerName, routeValues, new { });
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(a.ToHtmlString());
            var href = doc.DocumentNode.FirstChild.Attributes["href"];
            return href.Value;
        }

        private const string BREAK_TOKEN = "^^ BrEaKer bReaKER 123 vv";
        private const int FORCE_BREAK_LEN = 16;

        public static IHtmlString ToBreakableString(this HtmlHelper hh, string s)
        {
            s = StringHelpers.TrimOrNull(s) ?? "";
            if (s.Length > FORCE_BREAK_LEN)
            {
                var sb = new StringBuilder();
                int lastBreak = 0;
                bool forcedBreaks = false;
                for (int z = 0; z < s.Length; ++z)
                {
                    bool breakIt = false;
                    char ch = s[z];
                    if (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n')
                    {
                        lastBreak = z;
                    }
                    else if (!char.IsLetterOrDigit(ch) || (z - lastBreak) >= FORCE_BREAK_LEN)
                    {
                        breakIt = true;
                    }
                    sb.Append(ch);
                    if (breakIt)
                    {
                        forcedBreaks = true;
                        sb.Append(BREAK_TOKEN);
                        lastBreak = z;
                    }
                }
                if (forcedBreaks)
                {
                    string html = HttpUtility.HtmlEncode(sb.ToString());
                    html = html.Replace(BREAK_TOKEN, "<span class=\"wordBreaker\"> &nbsp; </span>");
                    return new HtmlString($"<span class=\"wordBreakerContainer\">{html}</span>");
                }
            }
            return new HtmlString(HttpUtility.HtmlEncode(s));
        }

        public static MvcHtmlString CheckBoxListFor<TModelItem>(
            this HtmlHelper<TModelItem> hh,
            Expression<Func<TModelItem, IEnumerable<string>>> columnExpression,
            IEnumerable<SelectListItem> items,
            object htmlAttributes=null)
        {
            var vals = new HashSet<string>();
            foreach (var val in columnExpression.Compile().Invoke(hh.ViewData.Model))
            {
                vals.Add(val);
            }
            var name = columnExpression.GetName();
            var sb = new StringBuilder();
            int z = 0;
            foreach (var item in items)
            {                
                var desc = ExtendedSelectListItem.GetDescription(item);
                var id = name + "_" + (z++).ToString();
                sb.AppendLine("<div>");
                sb.Append($"<input name=\"{name}\" id=\"{id}\" type=\"checkbox\" value=\"{item.Value}\"{(vals.Contains(item.Value)? " checked=\"checked\"":"")}");
                if (htmlAttributes != null) {
                    foreach (var p in htmlAttributes.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var val = StringHelpers.TrimOrNull(Stuff.ObjectToString(p.GetValue(htmlAttributes)));
                        if (val == null) continue;
                        sb.Append($" {p.Name}=\"{HttpUtility.HtmlAttributeEncode(val)}\"");
                    }
                }
                sb.AppendLine(">");
                sb.AppendLine($"<label for=\"{id}\"{(desc==null?"":$" title=\"{HttpUtility.HtmlAttributeEncode(desc)}\"")}>{item.Text}</label>");
                sb.AppendLine("</div>");
            }
            return new MvcHtmlString(sb.ToString());
        }

        public static string ToShortDateString(this DateTime? dt)
            => dt.HasValue ? dt.Value.ToShortDateString() : "";
    }
}
