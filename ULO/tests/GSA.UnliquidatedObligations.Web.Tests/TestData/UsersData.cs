using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class UsersData
    {
        public static List<AspNetUser> GenerateData(int listSize, string withUserID, string withUserType = "Person", bool addClaimsForAllUsers = false)
        {
            var claims = AspNetUserClaimsData.GenerateData(3, withUserID);

            var users =  Builder<AspNetUser>
                .CreateListOfSize(listSize)
                .Random(1)
                .With(u => u.Id = withUserID)
                .With(u => u.UserType = withUserType)
                .With(u => u.AspNetUserClaims = claims)
                .Build()
                .ToList();

            if (addClaimsForAllUsers)
            {
                
            }



            return users;
        }

    }
}