using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Web;
using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web
{
    public static class PortalHelpers
    {
        public static bool HasPermission(this IPrincipal user, ApplicationPermissionNames permissionName)
        {
            var componentContext = (IComponentContext)HttpContext.Current.Items["ComponentContext"];
            var DB = componentContext.Resolve<ULODBEntities>();

            var AspNetUser =
                DB.AspNetUsers.Include(u => u.AspNetUserClaims).FirstOrDefault(u => u.UserName == user.Identity.Name);
            if (AspNetUser == null)
                return false;

            return AspNetUser.GetApplicationPerimissionRegions(permissionName).Count > 0;

        }

        public static Expression<Func<Workflow, bool>> GenerateWorkflowPredicate(this Expression<Func<Workflow, bool>> originalPredicate, string pegasysDocumentNumber, string organization,
           int? region, int? zone, string fund, string baCode, string pegasysTitleNumber, string pegasysVendorName)
        {

            var predicate = originalPredicate;
            var pdnDecoded = HttpUtility.HtmlDecode(pegasysDocumentNumber);
            var orgDecoded = HttpUtility.HtmlDecode(organization);
            var fundDecoded = HttpUtility.HtmlDecode(fund);
            var baCodeDecoded = HttpUtility.HtmlDecode(baCode);
            var pegasysTitleNumberDecoded = HttpUtility.HtmlDecode(pegasysTitleNumber);
            var pegasysVendorNameDecoded = HttpUtility.HtmlDecode(pegasysVendorName);

            if (!string.IsNullOrEmpty(pdnDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.PegasusDocumentNumber == pdnDecoded);
            }

            if (!string.IsNullOrEmpty(orgDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.Organization == orgDecoded);
            }

            if (region != null)
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.RegionId == region);
            }

            if (zone != null)
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.Region.ZoneId == zone);
            }

            if (!string.IsNullOrEmpty(fundDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.Fund == fundDecoded);
            }

            if (!string.IsNullOrEmpty(baCodeDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.Prog == baCodeDecoded);
            }

            if (!string.IsNullOrEmpty(pegasysTitleNumberDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.PegasysTitleNumber == pegasysTitleNumberDecoded);
            }

            if (!string.IsNullOrEmpty(pegasysVendorNameDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.PegasysVendorName == pegasysVendorNameDecoded);
            }

            return predicate;

        }



    }
}