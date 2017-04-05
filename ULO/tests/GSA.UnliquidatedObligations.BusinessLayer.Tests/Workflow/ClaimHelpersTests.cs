using System;
using System.Collections.Generic;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSA.UnliquidatedObligations.BusinessLayer.Tests.Workflow
{
    [TestClass]
    public class ClaimHelperTests
    {

        [TestMethod]
        public void GetClaimsTest()
        {
            var guid = Guid.NewGuid().ToString();
            var testUser = new AspNetUser
            {
                Id = guid,
                AspNetUserClaims = new List<AspNetUserClaim>
                {
                    new AspNetUserClaim()
                    {
                        Id = 1,
                        UserId = guid,
                        ClaimType = "testClaimType",
                        ClaimValue = "testClaimValue"
                    },
                    new AspNetUserClaim()
                    {
                        Id = 2,
                        UserId = guid,
                        ClaimType = "testClaimType2",
                        ClaimValue = "testClaimValue2"
                    },
                    new AspNetUserClaim()
                    {
                        Id = 3,
                        UserId = guid,
                        ClaimType = "testClaimType3",
                        ClaimValue = "testClaimValue3"
                    }
                }
            };

            var claims = testUser.GetClaims();
            Assert.AreEqual(3, claims.Count);


        }
    }
}
