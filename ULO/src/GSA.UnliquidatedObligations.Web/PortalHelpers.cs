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
        

            if (!String.IsNullOrEmpty(pegasysDocumentNumber))
            {
                pegasysDocumentNumber = pegasysDocumentNumber.Trim().ToLower();
                if (pegasysDocumentNumber.StartsWith("%") && pegasysDocumentNumber.EndsWith("%"))
                {
                    var pdn = pegasysDocumentNumber.Trim().ToLower().Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.PegasysDocumentNumber.Trim().ToLower().Contains(pdn));
                }
                else if (pegasysDocumentNumber.StartsWith("%"))
                {
                    var pdn = pegasysDocumentNumber.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysDocumentNumber.Trim().ToLower().EndsWith(pdn));
                }
                else if (pegasysDocumentNumber.EndsWith("%"))
                {
                    var pdn = pegasysDocumentNumber.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysDocumentNumber.Trim().ToLower().StartsWith(pdn));
                }
                else
                {
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysDocumentNumber.Trim().ToLower() == pegasysDocumentNumber);
                }
            }

            if (!String.IsNullOrEmpty(organization))
            {
                organization = organization.Trim().ToLower();
                if (organization.StartsWith("%") && organization.EndsWith("%"))
                {
                    var orgCode = organization.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Organization.Trim().ToLower().Contains(orgCode));
                }
                else if (organization.StartsWith("%"))
                {
                    var orgCode = organization.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Organization.Trim().ToLower().EndsWith(orgCode));
                }
                else if (organization.EndsWith("%"))
                {
                    var orgCode = organization.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Organization.Trim().ToLower().StartsWith(orgCode));
                }
                else
                {
                    predicate =
                        predicate.And(wf => wf.UnliquidatedObligation.Organization.Trim().ToLower() == organization);
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

            if (!String.IsNullOrEmpty(fund))
            {
                fund = fund.Trim().ToLower();
                if (fund.StartsWith("%") && fund.EndsWith("%"))
                {
                    var fund1 = fund.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Fund.Trim().ToLower().Contains(fund1));
                }
                else if (fund.StartsWith("%"))
                {
                    var fund1 = fund.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Fund.Trim().ToLower().EndsWith(fund1));
                }
                else if (fund.EndsWith("%"))
                {
                    var fund1 = fund.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Fund.StartsWith(fund1));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.Fund.Trim().ToLower() == fund);
                }
            }

            if (!String.IsNullOrEmpty(baCode))
            {
                baCode = baCode.Trim().ToLower();
                if (baCode.StartsWith("%") && baCode.EndsWith("%"))
                {
                    var prog = baCode.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Prog.Trim().ToLower().Contains(prog));
                }
                else if (baCode.StartsWith("%"))
                {
                    var prog = baCode.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Prog.Trim().ToLower().EndsWith(prog));
                }
                else if (baCode.EndsWith("%"))
                {
                    var prog = baCode.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Prog.Trim().ToLower().StartsWith(prog));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.Prog.Trim().ToLower() == baCode);
                }
                
            }

            if (!String.IsNullOrEmpty(pegasysTitleNumber))
            {
                pegasysTitleNumber = pegasysTitleNumber.Trim().ToLower();
                if (pegasysTitleNumber.StartsWith("%") && pegasysTitleNumber.EndsWith("%"))
                {
                    var ptn = pegasysTitleNumber.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.PegasysTitleNumber.Trim().ToLower().Contains(ptn));
                }
                else if (pegasysTitleNumber.StartsWith("%"))
                {
                    var ptn = pegasysTitleNumber.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.PegasysTitleNumber.Trim().ToLower().EndsWith(ptn));
                }
                else if (pegasysTitleNumber.EndsWith("%"))
                {
                    var ptn = pegasysTitleNumber.Replace("%", "");
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
                                pegasysTitleNumber);
                }
            }

            if (!String.IsNullOrEmpty(pegasysVendorName))
            {
                pegasysVendorName = pegasysVendorName.Trim().ToLower();
                if (pegasysVendorName.StartsWith("%") && pegasysVendorName.EndsWith("%"))
                {
                    var pgVendorName = pegasysVendorName.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.VendorName.Trim().ToLower().Contains(pgVendorName));
                }
                else if (pegasysVendorName.StartsWith("%"))
                {
                    var pgVendorName = pegasysVendorName.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.VendorName.Trim().ToLower().EndsWith(pgVendorName));
                }
                else if (pegasysVendorName.EndsWith("%"))
                {
                    var pgVendorName = pegasysVendorName.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.VendorName.Trim().ToLower().StartsWith(pgVendorName));
                } 
                else
                {
                    predicate =
                        predicate.And(wf =>
                            wf.UnliquidatedObligation.VendorName.Trim().ToLower() == pegasysVendorName);
                }
            }

            if (!String.IsNullOrEmpty(docType))
            {
                //TODO: Add Doctype comparison code
                //predicate = predicate.And(wf => wf.UnliquidatedObligation.PegasysVendorName == pegasysVendorNameDecoded);
            }

            if (!String.IsNullOrEmpty(contractingOfficersName))
            {
                contractingOfficersName = contractingOfficersName.Trim().ToLower();
                if (contractingOfficersName.StartsWith("%") && contractingOfficersName.EndsWith("%"))
                {
                    var ctoName = contractingOfficersName.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.ContractingOfficersName.Trim().ToLower().Contains(ctoName));
                }
                else if (contractingOfficersName.StartsWith("%"))
                {
                    var ctoName = contractingOfficersName.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.ContractingOfficersName.Trim().ToLower().EndsWith(ctoName));
                }
                else if (contractingOfficersName.EndsWith("%"))
                {
                    var ctoName = contractingOfficersName.Replace("%", "");
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
                                contractingOfficersName);
                }
            }

            if (!String.IsNullOrEmpty(awardNumber))
            {
                awardNumber = awardNumber.Trim().ToLower();
                if (awardNumber.StartsWith("%") && awardNumber.EndsWith("%"))
                {
                    var awdNumber = awardNumber.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.AwardNbr.Trim().ToLower().Contains(awdNumber));
                }
                else if (awardNumber.StartsWith("%"))
                {
                    var awdNumber = awardNumber.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.AwardNbr.Trim().ToLower().EndsWith(awdNumber));
                }
                else if (awardNumber.EndsWith("%"))
                {
                    var awdNumber = awardNumber.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.AwardNbr.Trim().ToLower().StartsWith(awdNumber));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.AwardNbr.Trim().ToLower() == awardNumber);
                }
               
            }

            if (!String.IsNullOrEmpty(reasonIncludedInReview))
            {
                reasonIncludedInReview = reasonIncludedInReview.Trim().ToLower();
                if (reasonIncludedInReview.StartsWith("%") && reasonIncludedInReview.EndsWith("%"))
                {
                    var reason = reasonIncludedInReview.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.ReasonIncludedInReview.Trim().ToLower().Contains(reason));
                }
                else if (reasonIncludedInReview.StartsWith("%"))
                {
                    var reason = reasonIncludedInReview.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.ReasonIncludedInReview.Trim().ToLower().EndsWith(reason));
                }
                else if (reasonIncludedInReview.EndsWith("%"))
                {
                    var reason = reasonIncludedInReview.Replace("%", "");
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

            if (!String.IsNullOrEmpty(status))
            {
                status = status.Trim().ToLower();
                if (status.StartsWith("%") && status.EndsWith("%"))
                {
                    var stat = status.Replace("%", "");
                    predicate =
                       predicate.And(
                           wf => wf.UnliquidatedObligation.Status.Trim().ToLower().Contains(stat));
                }
                else if (status.StartsWith("%"))
                {
                    var stat = status.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Status.Trim().ToLower().EndsWith(stat));
                }
                else if (status.EndsWith("%"))
                {
                    var stat = status.Replace("%", "");
                    predicate =
                        predicate.And(
                            wf => wf.UnliquidatedObligation.Status.Trim().ToLower().StartsWith(stat));
                }
                else
                {
                    predicate = predicate.And(wf => wf.UnliquidatedObligation.Status.Trim().ToLower() == status);
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

        public static string Currency(this HtmlHelper helper, decimal data, string locale = "en-US", bool woCurrency = false)
        {
            var culture = new System.Globalization.CultureInfo(locale);

            if (woCurrency || (helper.ViewData["woCurrency"] != null && (bool)helper.ViewData["woCurrency"]))
                return data.ToString(culture);

            return data.ToString("C", culture);
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