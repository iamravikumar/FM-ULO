using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class UsersData
    {
        public static List<AspNetUser> GenerateData(int listSize, string withUserID, string withUserType = "Person", bool addClaimsForAllUsers = false, string userName = "")
        {
            var claims = AspNetUserClaimsData.GenerateData(4, withUserID);

            var users =  Builder<AspNetUser>
                .CreateListOfSize(listSize)
                .Random(1)
                .With(u => u.Id = withUserID)
                .With(u => u.AspNetUserClaims = claims)
                .All()
                .With(u => u.UserType = withUserType)
                .Build()
                .ToList();

            if (userName != String.Empty)
            {
                users.FirstOrDefault(u => u.Id == withUserID).UserName = userName;
            }


            return users;
        }

        public static void AddParentUser(this List<AspNetUser> users, string childId, UserUser parent)
        {
            var user = users.FirstOrDefault(u => u.Id == childId);
            var userUsers =  new List<UserUser>();
            userUsers.Add(parent);
            user.UserUsers = userUsers;
        }
    }
}