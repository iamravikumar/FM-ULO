using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Web;
using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Newtonsoft.Json;

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
           int? region, int? zone, string fund, string baCode, string pegasysTitleNumber, string pegasysVendorName, string docType, string contractingOfficersName, string awardNumber, string reasonIncludedInReview, bool? valid, string status)
        {

            var predicate = originalPredicate;
            var pdnDecoded = HttpUtility.HtmlDecode(pegasysDocumentNumber);
            var orgDecoded = HttpUtility.HtmlDecode(organization);
            var fundDecoded = HttpUtility.HtmlDecode(fund);
            var baCodeDecoded = HttpUtility.HtmlDecode(baCode);
            var pegasysTitleNumberDecoded = HttpUtility.HtmlDecode(pegasysTitleNumber);
            var pegasysVendorNameDecoded = HttpUtility.HtmlDecode(pegasysVendorName);
            var docTypeDecoded = HttpUtility.HtmlDecode(docType);
            var contractingOfficersNameDecoded = HttpUtility.HtmlDecode(contractingOfficersName);
            var awardNumberDecoded = HttpUtility.HtmlDecode(awardNumber);
            var reasonIncludedInReviewDecoded = HttpUtility.HtmlDecode(reasonIncludedInReview);
            var statusDecoded = HttpUtility.HtmlDecode(status);

            if (!String.IsNullOrEmpty(pdnDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.PegasusDocumentNumber == pdnDecoded);
            }

            if (!String.IsNullOrEmpty(orgDecoded))
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

            if (!String.IsNullOrEmpty(fundDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.Fund == fundDecoded);
            }

            if (!String.IsNullOrEmpty(baCodeDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.Prog == baCodeDecoded);
            }

            if (!String.IsNullOrEmpty(pegasysTitleNumberDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.PegasysTitleNumber == pegasysTitleNumberDecoded);
            }

            if (!String.IsNullOrEmpty(pegasysVendorNameDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.PegasysVendorName == pegasysVendorNameDecoded);
            }

            if (!String.IsNullOrEmpty(docTypeDecoded))
            {
                //TODO: Add Doctype comparison code
                //predicate = predicate.And(wf => wf.UnliquidatedObligation.PegasysVendorName == pegasysVendorNameDecoded);
            }

            if (!String.IsNullOrEmpty(contractingOfficersNameDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.ContractingOfficersName == contractingOfficersNameDecoded);
            }

            if (!String.IsNullOrEmpty(awardNumberDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.AwardNbr == awardNumberDecoded);
            }

            if (!String.IsNullOrEmpty(reasonIncludedInReviewDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.ReasonIncludedInReview == reasonIncludedInReview);
            }

            if (valid.HasValue)
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.Valid == valid);
            }

            if (!String.IsNullOrEmpty(statusDecoded))
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.Status == statusDecoded);
            }

            return predicate;

        }

        public static T BodyAsJsonObject<T>(this HttpRequestBase req)
        {
            req.InputStream.Seek(0, SeekOrigin.Begin);
            var json = new StreamReader(req.InputStream).ReadToEnd();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}