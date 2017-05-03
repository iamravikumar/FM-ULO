using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class UserUsersData
    {

        public static List<UserUser> GenerateData(int listSize, string withChildUserId, string groupId)
        {
            var UserUsers = Builder<UserUser>
                .CreateListOfSize(listSize)
                .TheFirst(1)
                .With(u => u.ChildUserId = withChildUserId)
                .With(u => u.ParentUserId = groupId)
                .With(u => u.RegionId = 4)
                .Build()
                .ToList();

            return UserUsers;
        }
    }
}