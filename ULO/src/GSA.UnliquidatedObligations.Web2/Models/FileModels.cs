using System.Collections.Generic;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class DocumentModalViewModel
    {
        public int DocumentId { get; set; }

        [MaxLength(255)]
        public string DocumentName { get; set; }

        public IList<int> DocumentTypeIds { get; set; }

        public IList<SelectListItem> DocumentTypes { get; set; }

        public AttachmentsViewModel AttachmentsViewModel { get; set; }

        public bool AllowDocumentEdit { get; set; }

        public DocumentModalViewModel()
        {

        }

        public DocumentModalViewModel(Document document, IList<DocumentType> documentTypes, bool allowDocumentEdit)
        {
            DocumentId = document.DocumentId;
            DocumentName = document.DocumentName;
            DocumentTypeIds = document.DocumentDocumentDocumentTypes.ConvertAll(z => z.DocumentTypeId).ToList();
            DocumentTypes = ConvertToSelectList(documentTypes);
            AttachmentsViewModel = new AttachmentsViewModel(document.DocumentAttachments.ToList(), DocumentId, allowDocumentEdit);
            AllowDocumentEdit = allowDocumentEdit;
        }

        private IList<SelectListItem> ConvertToSelectList(IEnumerable<DocumentType> documentTypes)
            => documentTypes
            .Select(dt => new SelectListItem
            {
                Text = dt.Name,
                Value = dt.DocumentTypeId.ToString()
            }).ToList();
    }

    public class AttachmentsViewModel
    {
        public IList<Attachment> Attachments { get; set; }

        public int DocumentId { get; set; }

        public bool AllowDocumentEdit { get; set; }

        public AttachmentsViewModel()
        {
            
        }

        public AttachmentsViewModel(IList<Attachment> attachments, int documentId, bool allowDocumentEdits)
        {
            Attachments = attachments;
            DocumentId = documentId;
            AllowDocumentEdit = allowDocumentEdits;
        }
    }

    public class UploadViewModel
    {
        public int DocumentIdForUpload { get; set; }

        public UploadViewModel()
        {
            
        }
    }
}
