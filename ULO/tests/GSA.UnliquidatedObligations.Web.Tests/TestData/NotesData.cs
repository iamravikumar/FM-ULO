using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

namespace GSA.UnliquidatedObligations.Web.Tests.TestData
{
    public static class NotesData
    {
        public static List<Note> GenerateData(int listSize, int withUloId, List<AspNetUser> userData)
        {
            var notes = Builder<Note>
                .CreateListOfSize(listSize)
                .All()
                .With(n => n.UloId = withUloId)
                .Build()
                .ToList();

            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].AspNetUser = userData[i];
            }

            return notes;
        }
    }
}