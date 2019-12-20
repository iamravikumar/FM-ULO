using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Controllers;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web.ViewComponents
{
    public class DocumentInfoViewComponent : ViewComponent
    {
        private readonly IWorkflowManager Manager;

        private readonly UloDbContext UloDb;

        private readonly PortalHelpers PortalHelpers;

        private readonly UserHelpers UserHelpers;

        private readonly ICacher Cacher;

        public DocumentInfoViewComponent(IWorkflowManager manager, UloDbContext context, PortalHelpers portalHelpers, UserHelpers userHelpers, ICacher cacher)
        {
            UloDb = context;
            Manager = manager;
            PortalHelpers = portalHelpers;
            UserHelpers = userHelpers;
            Cacher = cacher;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? documentId, string docType, bool allowDocumentEdit = false)
        {
            if (docType == null && documentId.HasValue)
            {
                docType = await(UloDb.Documents.Where(z => z.DocumentId == documentId.Value).Select(d=>d.Workflow.TargetUlo.DocType)).FirstOrDefaultAsync();
            }
            Requires.NonNull(docType, nameof(docType));

            Document document;
            if (documentId.GetValueOrDefault() == 0)
            {
                document = new Document();
            }
            else
            {
                document = UloDb.Documents.
                    Include(z => z.DocumentAttachments).
                    Include(z => z.Workflow.OwnerUser).
                    Include(z => z.DocumentDocumentDocumentTypes).
                    FirstOrDefault(dt => dt.DocumentId == documentId);
                if (document == null)
                {
                    //return HttpNotFound(); 
                    
                }
            }            
         
            document.DocumentAttachments = document.DocumentAttachments ?? new List<Attachment>();
            var documentTypes = Cacher.FindOrCreateValue(
                    Cache.CreateKey(typeof(DocumentsController), "documentTypes", docType),
                    () => UloDb.DocumentTypes.Where(dt => dt.DocType == null || dt.DocType == docType).OrderBy(dt => dt.Name).ToList().AsReadOnly(),
                    PortalHelpers.MediumCacheTimeout);

            var workflowAssignedTo = document.DocumentId == 0 ? true : CheckOwner(document);

            return View("~/Views/Ulo/Details/Documents/_View.cshtml", new DocumentModalViewModel(document, documentTypes, workflowAssignedTo));          

        }

        private bool CheckOwner(Document document)
        {
            if (document.Workflow == null)
            {
                return false;
            }
            else
            {
                return UserHelpers.CurrentUserId == document.Workflow.OwnerUserId;
            }
        }




    }
}
