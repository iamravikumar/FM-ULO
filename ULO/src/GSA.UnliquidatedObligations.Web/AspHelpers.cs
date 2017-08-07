using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace GSA.UnliquidatedObligations.Web
{
    public static class AspHelpers
    {
        public const string PleaseSelectOneDropdownItemText = "-- Please Select --";

        public static SelectListItem CreatePleaseSelectListItem(bool selected=true)
            => new SelectListItem
            {
                Disabled = true,
                Selected = selected,
                Text = PleaseSelectOneDropdownItemText,                    
            };

        public const string SortDirAscending = "asc";
        public const string SortDirDescending = "desc";

        public static bool IsSortDirAscending(string sortDir)
            => !(0 == string.Compare(SortDirDescending, sortDir, true));

        public static string GetDisplayName(Enum e)
            => e.GetCustomAttributes<DisplayAttribute>().FirstOrDefault(a => a.Name != null)?.Name ?? e.ToString();

        public static string GetDisplayDescription(Enum e)
        {
            var da = e.GetCustomAttributes<DisplayAttribute>().FirstOrDefault(a => a.Description != null);
            return da?.Description ?? da.Name ?? e.ToString();
        }

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
    }
}
