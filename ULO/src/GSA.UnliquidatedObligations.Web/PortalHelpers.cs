using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Web;
using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Hosting;

namespace GSA.UnliquidatedObligations.Web
{
    public static class PortalHelpers
    {
        public static readonly string DefaultUloConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public static string GetStorageFolderPath(string relativePath, bool createFolderInNotExists=true)
        {
            relativePath = relativePath ?? "";
            var dir = Properties.Settings.Default.DocPath;
            if (0==string.Compare(dir, "%temp%", true))
            {
                dir = Path.GetTempPath();
            }
            else if (dir.StartsWith("~"))
            {
                dir = HostingEnvironment.MapPath(dir);
            }
            var forwardSlash = dir.Contains("/");
            dir = dir.Replace("\\", "/");
            if (!dir.EndsWith("/"))
            {
                dir += "/";
            }
            relativePath = relativePath.Replace("\\", "/");
            if (relativePath.StartsWith("/"))
            {
                relativePath = relativePath.Substring(1);
            }
            var path = dir + relativePath;
            if (!forwardSlash)
            {
                path = path.Replace("/", "\\");
            }
            if (createFolderInNotExists)
            {
                dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            return path;
        }

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
            var pdnDecoded = HttpUtility.HtmlDecode(pegasysDocumentNumber).Trim().ToLower();
            var orgDecoded = HttpUtility.HtmlDecode(organization).Trim().ToLower();
            var fundDecoded = HttpUtility.HtmlDecode(fund).Trim().ToLower();
            var baCodeDecoded = HttpUtility.HtmlDecode(baCode).Trim().ToLower();
            var pegasysTitleNumberDecoded = HttpUtility.HtmlDecode(pegasysTitleNumber).Trim().ToLower();
            var pegasysVendorNameDecoded = HttpUtility.HtmlDecode(pegasysVendorName).Trim().ToLower();
            var docTypeDecoded = HttpUtility.HtmlDecode(docType).Trim().ToLower();
            var contractingOfficersNameDecoded = HttpUtility.HtmlDecode(contractingOfficersName).Trim().ToLower();
            var awardNumberDecoded = HttpUtility.HtmlDecode(awardNumber).Trim().ToLower();
            var reasonIncludedInReviewDecoded = HttpUtility.HtmlDecode(reasonIncludedInReview).Trim().ToLower();
            var statusDecoded = HttpUtility.HtmlDecode(status).Trim().ToLower();

            if (!String.IsNullOrEmpty(pdnDecoded))
            {
                if (pdnDecoded.StartsWith("%") && pdnDecoded.EndsWith("%"))
                {
                    var pdn = pdnDecoded.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.PegasysDocumentNumber.Trim().ToLower().Contains(pdn));
                }
                else if (pdnDecoded.StartsWith("%"))
                {
                    var pdn = pdnDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysDocumentNumber.Trim().ToLower().EndsWith(pdn));
                }
                else if (pdnDecoded.EndsWith("%"))
                {
                    var pdn = pdnDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysDocumentNumber.Trim().ToLower().StartsWith(pdn));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysDocumentNumber.Trim().ToLower() == pdnDecoded);
                }
            }

            if (!String.IsNullOrEmpty(orgDecoded))
            {
                if (orgDecoded.StartsWith("%") && orgDecoded.EndsWith("%"))
                {
                    var orgCode = orgDecoded.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Organization.Trim().ToLower().Contains(orgCode));
                }
                else if (orgDecoded.StartsWith("%"))
                {
                    var orgCode = orgDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Organization.Trim().ToLower().EndsWith(orgCode));
                }
                else if (orgDecoded.EndsWith("%"))
                {
                    var orgCode = orgDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Organization.Trim().ToLower().StartsWith(orgCode));
                }
                else
                {
                    predicate =
                        predicate.And(wf => wf.UnliquidatedObligation.Organization.Trim().ToLower() == orgDecoded);
                }
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
                if (fundDecoded.StartsWith("%") && fundDecoded.Trim().ToLower().EndsWith("%"))
                {
                    var fund1 = fundDecoded.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Fund.Trim().ToLower().Contains(fund1));
                }
                else if (fundDecoded.StartsWith("%"))
                {
                    var fund1 = fundDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Fund.Trim().ToLower().EndsWith(fund1));
                }
                else if (fundDecoded.EndsWith("%"))
                {
                    var fund1 = fundDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Fund.StartsWith(fund1));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.Fund.Trim().ToLower() == fundDecoded);
                }
            }

            if (!String.IsNullOrEmpty(baCodeDecoded))
            {
                if (baCodeDecoded.StartsWith("%") && baCodeDecoded.EndsWith("%"))
                {
                    var prog = baCodeDecoded.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Prog.Trim().ToLower().Contains(prog));
                }
                else if (baCodeDecoded.StartsWith("%"))
                {
                    var prog = baCodeDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Prog.Trim().ToLower().EndsWith(prog));
                }
                else if (baCodeDecoded.EndsWith("%"))
                {
                    var prog = baCodeDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Prog.Trim().ToLower().StartsWith(prog));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.Prog.Trim().ToLower() == baCodeDecoded);
                }
                
            }

            if (!String.IsNullOrEmpty(pegasysTitleNumberDecoded))
            {
                if (pegasysTitleNumberDecoded.StartsWith("%") && pegasysTitleNumberDecoded.EndsWith("%"))
                {
                    var ptn = pegasysTitleNumberDecoded.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.PegasysTitleNumber.Trim().ToLower().Contains(ptn));
                }
                else if (pegasysTitleNumberDecoded.StartsWith("%"))
                {
                    var ptn = pegasysTitleNumberDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysTitleNumber.Trim().ToLower().EndsWith(ptn));
                }
                else if (pegasysTitleNumberDecoded.EndsWith("%"))
                {
                    var ptn = pegasysTitleNumberDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysTitleNumber.Trim().ToLower().StartsWith(ptn));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf =>
                                wf.UnliquidatedObligation.PegasysTitleNumber.Trim().ToLower() ==
                                pegasysTitleNumberDecoded);
                }
            }

            if (!String.IsNullOrEmpty(pegasysVendorNameDecoded))
            {
                if (pegasysVendorNameDecoded.StartsWith("%") && pegasysVendorNameDecoded.EndsWith("%"))
                {
                    var pgVendorName = pegasysVendorNameDecoded.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.VendorName.Trim().ToLower().Contains(pgVendorName));
                }
                else if (pegasysVendorNameDecoded.StartsWith("%"))
                {
                    var pgVendorName = pegasysVendorNameDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.VendorName.Trim().ToLower().EndsWith(pgVendorName));
                }
                else if (pegasysVendorNameDecoded.EndsWith("%"))
                {
                    var pgVendorName = pegasysVendorNameDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.VendorName.Trim().ToLower().StartsWith(pgVendorName));
                } 
                else
                {
                    predicate =
                        predicate.And(wf =>
                            wf.UnliquidatedObligation.VendorName.Trim().ToLower() == pegasysVendorNameDecoded);
                }
            }

            if (!String.IsNullOrEmpty(docTypeDecoded))
            {
                //TODO: Add Doctype comparison code
                //predicate = predicate.And(wf => wf.UnliquidatedObligation.PegasysVendorName == pegasysVendorNameDecoded);
            }

            if (!String.IsNullOrEmpty(contractingOfficersNameDecoded))
            {
                if (contractingOfficersNameDecoded.StartsWith("%") && contractingOfficersNameDecoded.EndsWith("%"))
                {
                    var ctoName = contractingOfficersNameDecoded.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.ContractingOfficersName.Trim().ToLower().Contains(ctoName));
                }
                else if (contractingOfficersNameDecoded.StartsWith("%"))
                {
                    var ctoName = contractingOfficersNameDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.ContractingOfficersName.Trim().ToLower().EndsWith(ctoName));
                }
                else if (contractingOfficersNameDecoded.EndsWith("%"))
                {
                    var ctoName = contractingOfficersNameDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.ContractingOfficersName.StartsWith(ctoName));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf =>
                                wf.UnliquidatedObligation.ContractingOfficersName.Trim().ToLower() ==
                                contractingOfficersNameDecoded);
                }
            }

            if (!String.IsNullOrEmpty(awardNumberDecoded))
            {
                if (awardNumberDecoded.StartsWith("%") && awardNumberDecoded.EndsWith("%"))
                {
                    var awdNumber = awardNumberDecoded.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.AwardNbr.Trim().ToLower().Contains(awdNumber));
                }
                else if (awardNumberDecoded.StartsWith("%"))
                {
                    var awdNumber = awardNumberDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.AwardNbr.Trim().ToLower().EndsWith(awdNumber));
                }
                else if (awardNumberDecoded.EndsWith("%"))
                {
                    var awdNumber = awardNumberDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.AwardNbr.Trim().ToLower().StartsWith(awdNumber));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.AwardNbr.Trim().ToLower() == awardNumberDecoded);
                }
               
            }

            if (!String.IsNullOrEmpty(reasonIncludedInReviewDecoded))
            {
                if (reasonIncludedInReviewDecoded.StartsWith("%") && reasonIncludedInReviewDecoded.EndsWith("%"))
                {
                    var reason = reasonIncludedInReviewDecoded.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.ReasonIncludedInReview.Trim().ToLower().Contains(reason));
                }
                else if (reasonIncludedInReviewDecoded.StartsWith("%"))
                {
                    var reason = reasonIncludedInReviewDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.ReasonIncludedInReview.Trim().ToLower().EndsWith(reason));
                }
                else if (reasonIncludedInReviewDecoded.EndsWith("%"))
                {
                    var reason = reasonIncludedInReviewDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.ReasonIncludedInReview.Trim().ToLower().StartsWith(reason));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf =>
                                wf.UnliquidatedObligation.ReasonIncludedInReview.Trim().ToLower() ==
                                reasonIncludedInReview);
                }
            }

            if (valid.HasValue)
            {
                predicate = predicate.And(wf => wf.UnliquidatedObligation.Valid == valid);
            }

            if (!String.IsNullOrEmpty(statusDecoded))
            {
                if (statusDecoded.StartsWith("%") && statusDecoded.EndsWith("%"))
                {
                    var stat = statusDecoded.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Status.Trim().ToLower().Contains(stat));
                }
                else if (statusDecoded.StartsWith("%"))
                {
                    var stat = statusDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Status.Trim().ToLower().EndsWith(stat));
                }
                else if (statusDecoded.EndsWith("%"))
                {
                    var stat = statusDecoded.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Status.Trim().ToLower().StartsWith(stat));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.Status.Trim().ToLower() == statusDecoded);
                }
            }

            return predicate;

        }

        public static string GetDisplayName(this Enum value)
        {
            var type = value.GetType();
            if (!type.IsEnum) throw new ArgumentException(String.Format("Type '{0}' is not Enum", type));

            var members = type.GetMember(value.ToString());
            if (members.Length == 0) throw new ArgumentException(String.Format("Member '{0}' not found in type '{1}'", value, type.Name));

            var member = members[0];
            var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes.Length == 0) throw new ArgumentException(String.Format("'{0}.{1}' doesn't have DisplayAttribute", type.Name, value));

            var attribute = (DisplayAttribute)attributes[0];
            return attribute.GetName();
        }

        public static List<SelectListItem> ConvertToSelectList(this List<string> stringsToConvert)
        {
            var stringsSelect = new List<SelectListItem>();

            foreach (var stringToConvert in stringsToConvert)
            {
                stringsSelect.Add(new SelectListItem { Text = stringToConvert, Value = stringToConvert });
            }
            return stringsSelect;

        }

        public static List<SelectListItem> ConvertToSelectList(this List<int> nums)
        {
            var numsSelect = new List<SelectListItem>();

            foreach (var num in nums)
            {
                numsSelect.Add(new SelectListItem { Text = num.ToString(), Value = num.ToString() });
            }
            return numsSelect;

        }

        public static List<SelectListItem> ConvertToSelectList(this List<WorkflowDefinition> workFlowDefintions)
        {
            var workFlowDefintionsSelect = new List<SelectListItem>();

            foreach (var workflowDefinition in workFlowDefintions)
            {
                workFlowDefintionsSelect.Add(new SelectListItem {
                    Text = workflowDefinition.WorkflowDefinitionName,
                    Value = workflowDefinition.WorkflowDefinitionId.ToString()
                });
            }
            return workFlowDefintionsSelect;

        }

        public static List<SelectListItem> ConvertToSelectList<T>(this List<T> enums) where T : struct, IConvertible
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
                var value = ((int) Enum.Parse(typeof(T), enu.ToString())).ToString();
                eNumsSelect.Add(new SelectListItem { Text = displayName, Value = value });
            }
            return eNumsSelect;
        }



        public static T BodyAsJsonObject<T>(this HttpRequestBase req)
        {
            req.InputStream.Seek(0, SeekOrigin.Begin);
            var json = new StreamReader(req.InputStream).ReadToEnd();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}