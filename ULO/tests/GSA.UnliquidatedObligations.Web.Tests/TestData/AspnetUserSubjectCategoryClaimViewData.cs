using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class AspnetUserSubjectCategoryClaimViewData
    {
        public static List<AspnetUserSubjectCategoryClaim> GenerateData(List<AspNetUserClaim> claims)
        {
            var viewList = new List<AspnetUserSubjectCategoryClaim>();
            foreach (var claim in claims)
            {
                var loadedClaim = SubjectCatagoryClaimValue.Load(claim.ClaimValue);
                foreach (var loadedClaimRegion in loadedClaim.Regions)
                {
                   viewList.Add(new AspnetUserSubjectCategoryClaim
                   {
                       BACode = loadedClaim.BACode,
                       DocumentType = loadedClaim.DocType,
                       OrgCode = loadedClaim.OrgCode,
                       Region = loadedClaimRegion,
                       UserId = claim.UserId
                   });
                }
            }
            return viewList;
        }
    }
}