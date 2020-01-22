using System.Collections.Generic;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using Newtonsoft.Json;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class FileUploadAttachmentResult
    {
        [JsonProperty]
        public bool Added { get; set; }

        [JsonProperty]
        public bool Whitelisted { get; set; }

        [JsonProperty]
        public bool SaveError { get; set; }

        [JsonProperty]
        public IList<string> ErrorMessages { get; set; } = new List<string>();

        [JsonProperty]
        public int AttachmentsId { get; set; }

        [JsonProperty]
        public string FileName { get; set; }

        [JsonProperty]
        public string FilePath { get; set; }

        [JsonProperty]
        public string CreatedByUserId { get; set; }

        [JsonProperty]
        public int DocumentId { get; set; }

        [JsonProperty]
        public long FileSize { get; set; }

        [JsonProperty]
        public string ContentType { get; set; }

        public FileUploadAttachmentResult()
        { }

        public FileUploadAttachmentResult(Attachment a)
        {
            AttachmentsId = a.AttachmentsId;
            FileName = a.FileName;
            FilePath = a.FilePath;
            CreatedByUserId = a.CreatedByUserId;
            DocumentId = a.DocumentId.GetValueOrDefault();
            FileSize = a.FileSize;
            ContentType = a.ContentType;
        }
    }
}
