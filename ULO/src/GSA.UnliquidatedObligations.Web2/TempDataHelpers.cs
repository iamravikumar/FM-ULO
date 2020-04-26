using System.Collections.Generic;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RevolutionaryStuff.AspNetCore;

namespace GSA.UnliquidatedObligations.Web
{
    public static class TempDataHelpers
    {
        public static class TempDataKeys
        {
            public const string Attachments = "attachments";
        }

        public static IList<FileUploadAttachmentResult> FileUploadAttachmentResults(this ITempDataDictionary tdd, IList<FileUploadAttachmentResult> results = null)
        {
            results = results ?? tdd.GetFromJsonValue<IList<FileUploadAttachmentResult>>(TempDataKeys.Attachments) as IList<FileUploadAttachmentResult> ?? new List<FileUploadAttachmentResult>();
            tdd.SetAsJsonValue(TempDataKeys.Attachments, results);
            return results;
        }

        public static void ClearFileUploadAttachmentResults(this ITempDataDictionary tdd)
            => tdd.FileUploadAttachmentResults(new List<FileUploadAttachmentResult>());
    }
}
