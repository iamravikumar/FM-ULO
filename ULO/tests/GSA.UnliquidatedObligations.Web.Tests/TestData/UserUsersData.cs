using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class UserUsersData
    {

        public static List<UserUser> GenerateData(int listSize, AspNetUser user, string groupId)
        {
            var UserUsers = Builder<UserUser>
                .CreateListOfSize(listSize)
                .TheFirst(1)
                .With(uu => uu.ChildUserId = user.Id)
                .With(uu => uu.ParentUserId = groupId)
                .With(uu => uu.RegionId = 4)
                .With(uu => uu.AspNetUser1 = user)
                .Build()
                .ToList();

            return UserUsers;
        }
    }
}