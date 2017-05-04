using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class UloData
    {
        public static List<UnliquidatedObligation> GenerateData(int listSize, int withUloId, List<AspNetUser> userData)
        {
            var notes = Builder<Note>
                .CreateListOfSize(3)
                .Build()
                .ToList();

            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].AspNetUser = userData[i];
            }

            return Builder<UnliquidatedObligation>
                .CreateListOfSize(listSize)
                .TheFirst(1)
                .With(ulo => ulo.Notes = notes)
                .With(ulo => ulo.UloId = withUloId)
                .Build().ToList();
        }
    }
}