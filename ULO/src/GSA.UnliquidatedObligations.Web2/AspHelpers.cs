using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GSA.UnliquidatedObligations.BusinessLayer.Workflow;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using RevolutionaryStuff.Core;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc.Razor;

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
        private const string SortDirKeyName = "sortDir";
        private const string SortColKeyName = "sortCol";

        public static readonly TimeZoneInfo DisplayTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

        public static string ToShortDateString(this DateTime? dt)
            => dt.HasValue ? dt.Value.ToShortDateString() : "";

        public static DateTime ToLocalizedDateTime(this DateTime utc)
        {
            if (utc.Kind == DateTimeKind.Unspecified)
            {
                utc = new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, utc.Second, utc.Millisecond, DateTimeKind.Utc);
            }
            return utc.ToTimeZone(DisplayTimeZone);
        }

        public static string ToLocalizedDisplayDateString(this DateTime utc, bool includeTime = false)
        {
            var local = utc.ToLocalizedDateTime();
            var s = local.Date.ToString("MM/dd/yyyy");
            if (includeTime)
            {
                s += " " + local.ToString("t");
            }
            return s;
        }

        public static DateTime ToTimeZone(this DateTime dt, TimeZoneInfo zoneInfo)
            => TimeZoneInfo.ConvertTime(dt, zoneInfo);

        public static bool IsSortDirAscending(string sortDir)
            => !(0 == string.Compare(SortDirDescending, sortDir, true));

       
       
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
            routeValues["sortCol"] = colName;
            if (colName == currentSortColName)
            {
                routeValues["sortDir"] = IsSortDirAscending(currentSortDir) ? SortDirDescending : SortDirAscending;
                var h = hh.ActionLink(
                    displayName,
                    actionName,
                    routeValues);

                var htmlList = new List<object> { h };
                var builder = new HtmlContentBuilder(htmlList);
                if (currentSortDir != null)
                {
                    var html = builder.AppendHtml(currentSortDir == SortDirAscending ? " <span class='caret-up'>&#9650;</span>" : " <span class='caret-down'>&#9660;</span>");
                    return html;
                }

                return builder;
            }
            else
            {
                routeValues["sortDir"] = SortDirAscending;
                return hh.ActionLink(displayName, actionName, routeValues);
            }
        }


#if false

        public static HtmlString AnchorTag(this IHtmlHelper hh, string url, string displayText)
        {
            var u = hh.ViewContext.HttpContext.Server;
            var html = $"<a href=\"{url}\">{u.HtmlEncode(displayText)}</a>";
            return new HtmlString(html);
        }

        private static readonly Regex BeginningOfTheEnd = new Regex(
            @"(.+)(\<\/\w+>\s*)", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static HtmlString AppendChildHtml(this HtmlString hs, string newChildHtml)
        {
            if (newChildHtml == null || newChildHtml == "") return hs;
            var html = hs.ToHtmlString();
            var m = BeginningOfTheEnd.Match(html);
            if (m.Success)
            {
                html = m.Groups[1].Value + newChildHtml + m.Groups[2].Value;
                hs = new HtmlString(html);
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
#endif
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

        /// <remarks>https://rburnham.wordpress.com/2015/03/13/asp-net-mvc-defining-scripts-in-partial-views/</remarks>
        public static HtmlString Script(this IHtmlHelper htmlHelper, Func<object, Microsoft.AspNetCore.Mvc.Razor.HelperResult> template)
        {
            htmlHelper.ViewContext.HttpContext.Items["_script_" + Guid.NewGuid()] = template;
            return HtmlString.Empty;
        }

        /// <remarks>https://rburnham.wordpress.com/2015/03/13/asp-net-mvc-defining-scripts-in-partial-views/</remarks>
        public static HtmlString RenderPartialViewScripts(this IHtmlHelper htmlHelper)
        {
            foreach (object key in htmlHelper.ViewContext.HttpContext.Items.Keys)
            {
                if (key.ToString().StartsWith("_script_"))
                {
                    var template = htmlHelper.ViewContext.HttpContext.Items[key] as Func<object, Microsoft.AspNetCore.Mvc.Razor.HelperResult>;
                    if (template != null)
                    {
                        htmlHelper.ViewContext.Writer.Write(template(null));
                    }
                }
            }
            return HtmlString.Empty;
        }

        #region SelectListItems

        public static SelectListItem CreateSelectListItem(Justification j)
            => new SelectListItem { Value = j.Key, Text = j.Description };

        public static IList<SelectListItem> CreateSelectList(IEnumerable<Justification> justifications)
            => justifications.ConvertAll(j => CreateSelectListItem(j)).ToList();

        public static IList<SelectListItem> CreateSelectList(this IEnumerable<string> stringsToConvert)
        {
            var stringsSelect = new List<SelectListItem>();

            foreach (var stringToConvert in stringsToConvert)
            {
                stringsSelect.Add(new SelectListItem { Text = stringToConvert, Value = stringToConvert });
            }
            return stringsSelect;
        }

        public static IList<SelectListItem> CreateSelectList(this IEnumerable<SelectListItem> selectListItems)
        {
            var selectList = new List<SelectListItem>();

            foreach (var selectListItem in selectListItems)
            {
                selectList.Add(selectListItem);
            }
            return selectList;
        }

        public static IList<SelectListItem> CreateSelectList(this IEnumerable<int> nums)
        {
            var numsSelect = new List<SelectListItem>();

            foreach (var num in nums)
            {
                numsSelect.Add(new SelectListItem { Text = num.ToString(), Value = num.ToString() });
            }
            return numsSelect;
        }

        public static int? IndexOfOccurrence<T>(this IList<T> items, Func<T, bool> test, int nthOccurrence, int? zeroThValue = null, int? missingValue = null)
        {
            Requires.NonNegative(nthOccurrence, nameof(nthOccurrence));

            if (nthOccurrence == 0) return zeroThValue;

            int cnt = 0;
            for (int z = 0; z < items.Count; ++z)
            {
                var i = items[z];
                bool hit = test(i);
                if (hit && ++cnt == nthOccurrence)
                {
                    return z;
                }
            }
            return missingValue;
        }

        public static int? IndexOfOccurrence<T>(this IList<T> items, T match, int nthOccurrence, int? zeroThValue = null, int? missingValue = null)
           => items.IndexOfOccurrence(i => {
               if (i == null)
               {
                   return match == null;
               }
               else
               {
                   return i.Equals(match);
               }
           }, nthOccurrence, zeroThValue, missingValue);

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
            => value.GetDisplayAttribute()?.GetName() ?? value.ToString();

        public static string GetDescription(this Enum value)
            => value.GetDisplayAttribute()?.GetDescription();

        private static DisplayAttribute GetDisplayAttribute(this Enum value)
        {
            var type = value.GetType();
            if (!type.IsEnum) throw new ArgumentException(String.Format("Type '{0}' is not Enum", type));

            var members = type.GetMember(value.ToString());
            if (members.Length == 0) throw new ArgumentException(String.Format("Member '{0}' not found in type '{1}'", value, type.Name));

            var member = members[0];
            var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            return (DisplayAttribute)attributes.FirstOrDefault();
        }

        public static IList<SelectListItem> ConvertNamesToSelectList<T>() where T : struct
           => ((IEnumerable<T>)Enum.GetValues(typeof(T))).ConvertToSelectList(true);

        public static IList<SelectListItem> ConvertToSelectList<T>(this IEnumerable<T> enums, bool names = false) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            var eNumsSelect = new List<SelectListItem>();

            foreach (var enu in Enum.GetValues(typeof(T)))
            {
                var e = (Enum)Enum.Parse(typeof(T), enu.ToString());
                var displayName = e.GetDisplayName();
                var desc = e.GetDescription();
                string value;
                if (names)
                {
                    value = enu.ToString();
                }
                else
                {
                    value = ((int)Enum.Parse(typeof(T), enu.ToString())).ToString();
                }
                eNumsSelect.Add(new ExtendedSelectListItem { Text = displayName, Value = value, Description = desc });
            }
            return eNumsSelect;
        }

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

        public static IList<SelectListItem> Select(this IList<SelectListItem> items, object selectedValue)
        {
            var v = Stuff.ObjectToString(selectedValue);
            foreach (var i in items)
            {
                i.Selected = i.Value == v;
            }
            return items;
        }

        #endregion
    }
}
