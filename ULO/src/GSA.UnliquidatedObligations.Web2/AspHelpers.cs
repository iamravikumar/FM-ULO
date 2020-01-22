using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using RevolutionaryStuff.Core;
using RASP = RevolutionaryStuff.AspNetCore;

namespace GSA.UnliquidatedObligations.Web
{
    public static class AspHelpers
    {
        public static object CssClass(string className)
            => new { @class = className};

        public const string PleaseSelectOne = "-- Please Select --";

        public const string NoResultsMessage = "There are not currently any results to show";

        public static SelectListItem CreatePleaseSelectListItem(bool selected=true)
            => new SelectListItem
            {
                Disabled = true,
                Selected = selected,
                Text = PleaseSelectOne,                    
            };

        public static string ToShortDateString(this DateTime? dt)
            => dt.HasValue ? dt.Value.ToShortDateString() : "";

        public static DateTime ToTimeZone(this DateTime dt, TimeZoneInfo zoneInfo)
            => TimeZoneInfo.ConvertTime(dt, zoneInfo);

        public static bool IsSortDirAscending(string sortDir)
            => RASP.AspHelpers.IsSortDirAscending(sortDir);       
       
        public static string Currency(this IHtmlHelper helper, decimal data, string locale = "en-US", bool woCurrency = false)
        {
            var culture = new System.Globalization.CultureInfo(locale);

            if (woCurrency || (helper.ViewData["woCurrency"] != null && (bool)helper.ViewData["woCurrency"]))
                return data.ToString(culture);

            return data.ToString("C", culture);
        }


        public static IList<SelectListItem> Copy(this IEnumerable<SelectListItem> items)
        {
            var ret = new List<SelectListItem>();
            foreach (var i in items)
            {
                ret.Add(new SelectListItem
                {
                    Disabled = i.Disabled,
                    Group = i.Group,
                    Selected = i.Selected,
                    Text = i.Text,
                    Value = i.Value
                });
            }
            return ret;
        }

        public static string DisplayIsValid(this IHtmlHelper hh, bool? isValid)
            => isValid.HasValue ? (isValid.Value ? "Yes" : "No") : null;

        public static string FriendlyNameFor<TModelItem, TResult>(this IHtmlHelper<TModelItem> hh, Expression<Func<TModelItem, TResult>> columnExpression)
        {
            var fieldMemberInfo = columnExpression.GetMembers().Last();
            if (fieldMemberInfo != null)
            {
                var dna = fieldMemberInfo.GetCustomAttribute<DisplayNameAttribute>();
                if (dna != null) return dna.DisplayName;
            }
            return fieldMemberInfo.Name;
        }

        public static string FriendlyNameFor<TModelItem, TResult>(this IHtmlHelper<IEnumerable<TModelItem>> hh, Expression<Func<TModelItem, TResult>> columnExpression)
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

        public static void SetTitles(this RazorPage page, PageKeys pageKey, string title, string subTitle = null, string browserTitle = null)
        {
            page.ViewBag.PageKey = pageKey;
            page.ViewBag.Title = browserTitle ?? title;
            page.ViewBag.PageTitle = title;
            page.ViewBag.PageSubTitle = subTitle;
        }

        public static IHtmlContent SortableHeaderFor<TModelItem, TResult>(this IHtmlHelper<IEnumerable<TModelItem>> hh, Expression<Func<TModelItem, TResult>> columnExpression, string currentSortColName = null, string currentSortDir = null, string actionName = null, string overrideDisplayName=null)
        {
            currentSortColName = currentSortColName ?? hh.ViewBag.SortCol as string;
            currentSortDir = currentSortDir ?? hh.ViewBag.SortDir as string;

            actionName = actionName ?? hh.ViewContext.RouteData.Values["action"] as string;
            var colName = columnExpression.GetFullyQualifiedName();
            var displayName = overrideDisplayName ?? hh.FriendlyNameFor(columnExpression);

            var routeValues = new RouteValueDictionary();
            foreach (string key in hh.ViewContext.HttpContext.Request.Query.Keys)
            {
                if (key == null) continue;
                var val = hh.ViewContext.HttpContext.Request.Query[key];
                if (val.Count != 0)
                {
                    routeValues[key] = val;
                }
            }
            routeValues[RASP.AspHelpers.SortColKeyName] = colName;
            if (colName == currentSortColName)
            {
                routeValues[RASP.AspHelpers.SortDirKeyName] = IsSortDirAscending(currentSortDir) ? RASP.AspHelpers.SortDirDescending : RASP.AspHelpers.SortDirAscending;
                var h = hh.ActionLink(
                    displayName,
                    actionName,
                    routeValues);

                var htmlList = new List<object> { h };
                var builder = new HtmlContentBuilder(htmlList);
                if (currentSortDir != null)
                {
                    var html = builder.AppendHtml(currentSortDir == RASP.AspHelpers.SortDirAscending ? " <span class='caret-up'>&#9650;</span>" : " <span class='caret-down'>&#9660;</span>");
                    return html;
                }

                return builder;
            }
            else
            {
                routeValues[RASP.AspHelpers.SortDirKeyName] = RASP.AspHelpers.SortDirAscending;
                return hh.ActionLink(displayName, actionName, routeValues);
            }
        }

        public static Task<T> BodyAsJsonObjectAsync<T>(this HttpRequest req)
            => RASP.AspHelpers.BodyAsJsonObjectAsync<T>(req, false);

#if false

        public static HtmlString AnchorTag(this IHtmlHelper hh, string url, string displayText)
        {
            var u = hh.ViewContext.HttpContext.Server;
            var html = $"<a href=\"{url}\">{u.HtmlEncode(displayText)}</a>";
            return new HtmlString(html);
        }

       
#endif

        //public static string ActionLinkUrl(this IHtmlHelper hh, string actionName, string controllerName, object routeValues)
        //{
        //    var a = hh.ActionLink("jbt", actionName, controllerName, routeValues, new { });
        //    var doc = new HtmlAgilityPack.HtmlDocument();
        //    doc.LoadHtml(a.ToHtmlString());
        //    var href = doc.DocumentNode.FirstChild.Attributes["href"];
        //    return href.Value;
        //}
        private static readonly Regex BeginningOfTheEnd = new Regex(
            @"(.+)(\<\/\w+>\s*)", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static HtmlString AppendChildHtml(this HtmlString hs, string newChildHtml)
        {
            if (newChildHtml == null || newChildHtml == "") return hs;
            var html = hs.Value;
            var m = BeginningOfTheEnd.Match(html);
            if (m.Success)
            {
                html = m.Groups[1].Value + newChildHtml + m.Groups[2].Value;
                hs = new HtmlString(html);
            }
            return hs;
        }       

        private const string BREAK_TOKEN = "^^ BrEaKer bReaKER 123 vv";
        private const int FORCE_BREAK_LEN = 16;

        public static HtmlString ToBreakableString(this IHtmlHelper hh, string s)
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

        public static HtmlString CheckBoxListFor<TModelItem>(
            this IHtmlHelper<TModelItem> hh,
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
                var desc = RASP.ExtendedSelectListItem.GetDescription(item);
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
            return new HtmlString(sb.ToString());
        }

        static AspHelpers()
        {
            var d = new Dictionary<ReviewScopeEnum, string>();
            foreach (var row in CSV.ParseText("Region,Region Workflow WholeAgency, ULO Workflow"))
            {
                if (row.Length != 2) continue;
                var scope = Parse.ParseEnum<ReviewScopeEnum>(row[0]);
                d[scope] = StringHelpers.TrimOrNull(row[1]);
            }
            WorkflowDefinitionNameByReviewScope = d.AsReadOnly();
        }

        public static readonly IDictionary<ReviewScopeEnum, string> WorkflowDefinitionNameByReviewScope;

        #region SelectListItems

        public static SelectListItem CreateSelectListItem(Justification j)
            => new SelectListItem { Value = j.Key, Text = j.Description };

        public static IList<SelectListItem> CreateSelectList(IEnumerable<Justification> justifications)
            => justifications.ConvertAll(j => CreateSelectListItem(j)).ToList();

        public static IList<SelectListItem> CreateSelectList(this IEnumerable<string> items)
            => RASP.AspHelpers.CreateSelectList(items);

        public static IList<SelectListItem> CreateSelectList(this IEnumerable<SelectListItem> items)
            => RASP.AspHelpers.CreateSelectList(items);

        public static IList<SelectListItem> CreateSelectList(this IEnumerable<int> items)
            => RASP.AspHelpers.CreateSelectList(items);


        public static IList<SelectListItem> ConvertToSelectList(this IEnumerable<string> stringsToConvert)
        {
            var stringsSelect = new List<SelectListItem>();

            foreach (var stringToConvert in stringsToConvert)
            {
                stringsSelect.Add(new SelectListItem { Text = stringToConvert, Value = stringToConvert });
            }
            return stringsSelect;
        }

        public static string GetDisplayName(this Enum value)
            => RASP.AspHelpers.GetDisplayName(value);

        public static IList<SelectListItem> CreateSelectList<TEnum>(bool valAsName = true, bool sortByText = false) where TEnum : Enum
            => RASP.AspHelpers.CreateSelectList<TEnum>(valAsName, sortByText);

        public static IList<SelectListItem> CreateSelectList<TEnum>(this IEnumerable<TEnum> enums, bool valAsName = true, bool sortByText = false) where TEnum : Enum
            => RASP.AspHelpers.CreateSelectList(enums, valAsName, sortByText);

        public static IList<SelectListItem> CreateUserTypesSelectListItems(bool creatableOnly = true)
          => new[] {
                AspNetUser.UserTypes.Person,
                AspNetUser.UserTypes.Group,
                creatableOnly ? null : AspNetUser.UserTypes.System,
          }.WhereNotNull().OrderBy().ConvertToSelectList();

        
        public static IList<SelectListItem> SelectedValues(this IList<SelectListItem> items, IEnumerable<string> values)
        {
            foreach (var item in items)
            {
                item.Selected = values != null && values.Contains(item.Value);
            }
            return items;
        }

        public static IList<SelectListItem> CreateSelectListItems(this IEnumerable<Models.QuestionChoicesViewModel> items)
            => items.OrderBy(z => z.Text).ConvertAll(z => new SelectListItem { Text = z.Text, Value = z.Value });

        #endregion
    }
}
