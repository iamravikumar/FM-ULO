using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Authorization;
using GSA.UnliquidatedObligations.Web.Identity;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]   
    public class DocumentsController : BasePageController
    {
        public const string Name = "Documents";

        public static class ActionNames
        {
            public const string CopyUniqueMissingLineageDocuments = "CopyUniqueMissingLineageDocuments";
            public const string Save = "Save";
            public const string View = "View";
        }

        private readonly SpecialFolderProvider SpecialFolderProvider;
        private readonly UloUserManager UserManager;
        
        private readonly IHostEnvironment HostingEnvironment;
       
        public DocumentsController(SpecialFolderProvider specialFolderProvider, UloUserManager userManager, IHostEnvironment hostingEnvironment, UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, Serilog.ILogger logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            SpecialFolderProvider = specialFolderProvider;
            UserManager = userManager;
            HostingEnvironment = hostingEnvironment;
            PopulateDocumentTypeNameByDocumentTypeIdInViewBag();
        }

        // GET: Documents
        public async Task<ActionResult> Index()
        {
            var documents = DB.Documents.Include(d => d.DocumentDocumentDocumentTypes).Include(d => d.Workflow);
            return View(await documents.ToListAsync());
        }

        [ActionName(ActionNames.View)]
        public ActionResult View(int? documentId, string docType, bool allowDocumentEdit = false)
        {
            if (docType == null && documentId.HasValue)
            {
                docType =
                    (
                        from z in DB.Documents
                        where z.DocumentId == documentId.Value
                        select z.Workflow.TargetUlo.DocType
                    ).FirstOrDefault();
            }
            Requires.NonNull(docType, nameof(docType));
            Document document;
            if (documentId.GetValueOrDefault() == 0)
            {
                document = new Document();
            }
            else
            {
                document = DB.Documents.
                    Include(z => z.DocumentAttachments).
                    Include(z => z.Workflow.OwnerUser).
                    Include(z => z.DocumentDocumentDocumentTypes).
                    FirstOrDefault(dt => dt.DocumentId == documentId);
                if (document == null)
                {
                    return NotFound();
                }
            }
            document.DocumentAttachments = document.DocumentAttachments ?? new List<Attachment>();
            var documentTypes = Cacher.FindOrCreateValue(
                Cache.CreateKey(typeof(DocumentsController), "documentTypes", docType),
                () => DB.DocumentTypes.Where(dt => dt.DocType == null || dt.DocType == docType).OrderBy(dt => dt.Name).ToList().AsReadOnly(),
                PortalHelpers.MediumCacheTimeout);

            var workflowAssignedTo = document.DocumentId == 0 ? true : CheckOwner(document);

            return PartialView("~/Views/Ulo/Details/Documents/_View.cshtml", new DocumentModalViewModel(document, documentTypes, workflowAssignedTo));
        }

        private bool CheckOwner(Document document)
        {
            if (document.Workflow == null)
            {
                return false;
            }
            else
            {
                return CurrentUserId == document.Workflow.OwnerUserId;
            }
        }

        protected async Task CheckStalenessFromFormAsync(Workflow wf = null)
        {
            int workflowId = Parse.ParseInt32(Request.Form[WorkflowStalenessMagicFieldNames.WorkflowId]);
            var editingBeganAtUtc = DateTime.Parse(Request.Form[WorkflowStalenessMagicFieldNames.EditingBeganAtUtc]);
            string workflowRowVersionString = Request.Form[WorkflowStalenessMagicFieldNames.WorkflowRowVersionString];
            if (wf == null || wf.WorkflowId != workflowId)
            {
                wf = await DB.Workflows.FindAsync(workflowId);
            }
            if (wf == null) throw new FileNotFoundException();
            if (wf.WorkflowRowVersionString != workflowRowVersionString)
            {
                LogStaleWorkflowError(wf, workflowRowVersionString, editingBeganAtUtc);
                var staleMessage = GetStaleWorkflowErrorMessage(wf, workflowRowVersionString, editingBeganAtUtc);
                throw new Exception(staleMessage);
            }
        }
        // POST: Documents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ActionName(ActionNames.Save)]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> Save(int? documentId, string documentName, int workflowId, string newRemovedAttachmentIds)
        {
            try
            {
                if (!ModelState.IsValid) throw new ArgumentException(ModelState.ToString(), nameof(ModelState));
                await CheckStalenessFromFormAsync();
                Document document;
                if (documentId.GetValueOrDefault(0) > 0)
                {
                    document = await DB.Documents.FindAsync(documentId);
                    if (document == null)
                    {
                        throw new KeyNotFoundException($"could not find documentId={documentId}");
                    }
                    document.DocumentDocumentDocumentTypes.ToList().ForEach(dt => DB.DocumentDocumentTypes.Remove(dt));
                }
                else
                {
                    document = new Document
                    {
                        UploadedByUserId = CurrentUserId,
                        WorkflowId = workflowId,
                        //CreatedAtUtc = DateTime.UtcNow
                    };
                    DB.Documents.Add(document);
                }
                document.DocumentName = StringHelpers.TrimOrNull(documentName);
                var documentTypeIds = CSV.ParseIntegerRow(Request.Query["documentTypeId"]);
                var documentTypeNames = new List<string>();
                var d = PopulateDocumentTypeNameByDocumentTypeIdInViewBag();
                foreach (var id in documentTypeIds)
                {
                    DB.DocumentDocumentTypes.Add(new DocumentDocumentType
                    {
                        DocumentTypeId = id,
                        Document = document
                    });
                    documentTypeNames.Add(d.FindOrDefault(id));
                }
                await DB.SaveChangesAsync();
                //DB.Refresh(document);
                var attachmentsTempData = TempData.FileUploadAttachmentResults();
                if (attachmentsTempData.Count>0)
                {
                    var rids = CSV.ParseIntegerRow(newRemovedAttachmentIds);
                    foreach (var tempAttachment in attachmentsTempData)
                    {
                        if (!rids.Contains(tempAttachment.AttachmentsId))
                        {
                            var folder = await SpecialFolderProvider.GetDocumentFolderAsync(document.DocumentId);
                            var file = await folder.CreateFileAsync(Path.GetFileName(tempAttachment.FilePath));
                            using (var st = await file.OpenWriteAsync())
                            {
                                using (var tmp = System.IO.File.OpenRead(tempAttachment.FilePath))
                                {
                                    await tmp.CopyToAsync(st);
                                }
                            }
                            Stuff.FileTryDelete(tempAttachment.FilePath);
                            var attachment = new Attachment
                            {
                                FileName = tempAttachment.FileName,
                                FilePath = file.FullRelativePath,
                                DocumentId = document.DocumentId,
                                FileSize = tempAttachment.FileSize,
                                ContentType = tempAttachment.ContentType,
                                CreatedByUserId = tempAttachment.CreatedByUserId
                            };
                            DB.Attachments.Add(attachment);
                        }
                    }
                    await DB.SaveChangesAsync();
                    TempData.ClearFileUploadAttachmentResults();
                }
                return Json(new
                {
                    Id = document.DocumentId,
                    UserName = User.Identity.Name,
                    Name = document.DocumentName,
                    DocumentTypeNames = documentTypeNames.WhereNotNull().OrderBy().ToList(),
                    AttachmentCount = document.DocumentAttachments.Count,
                    UploadedDate = PortalHelpers.ToLocalizedDisplayDateString(document.CreatedAtUtc)
                });
            }
            catch (Exception ex)
            {
                return base.CreateJsonError(ex);
            }
        }

        // GET: Documents/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
               //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var document = await DB.Documents.FindAsync(id);
            if (document == null)
            {               
                return NotFound();
            }
            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int documentId)
        {
            try
            {
                await CheckStalenessFromFormAsync();
                var document = await DB.Documents.FindAsync(documentId);
                if (document == null)
                {
                    return NotFound();
                    // return HttpNotFound();
                }
                document.Delete(CurrentUserId);
                await DB.SaveChangesAsync();
                Logger.Information("Document {DocumentId} was soft deleted", document.DocumentId);
                return Json(new
                {
                    Id = documentId
                });
            }
            catch (Exception ex)
            {
                return base.CreateJsonError(ex);
            }
        }

        [HttpPost]
        [ActionName(ActionNames.CopyUniqueMissingLineageDocuments)]
        [Route("documents/copyUniqueMissingLineageDocuments/{workflowId}")]
        public async Task<ActionResult> CopyUniqueMissingLineageDocuments(int workflowId)
        {
            try
            {
                await CheckStalenessFromFormAsync();
                var wf = await DB.FindWorkflowAsync(workflowId);
                if (wf == null) return NotFound();
                var otherDocs = DB.GetUniqueMissingLineageDocuments(wf);
                int copiedDocumentCount = 0;
                int copiedAttachmentCount = 0;
                var copiedDocs = new List<Document>();
                foreach (var od in otherDocs)
                {
                    var d = new Document
                    {
                        Workflow = wf,
                        DocumentName = od.DocumentName,
                        UploadedByUserId = CurrentUserId,
                        CreatedAtUtc = od.CreatedAtUtc
                    };
                    wf.WorkflowDocuments.Add(d);
                    copiedDocs.Add(d);
                    ++copiedDocumentCount;
                    foreach (var oa in od.DocumentAttachments)
                    {
                        d.DocumentAttachments.Add(new Attachment
                        {
                            FileName = oa.FileName,
                            FilePath = oa.FilePath,
                            Document = d,
                            FileSize = oa.FileSize,
                            ContentType = oa.ContentType,
                            CreatedByUserId = oa.CreatedByUserId,
                            CreatedAtUtc = oa.CreatedAtUtc
                        });
                        ++copiedAttachmentCount;
                    }
                    foreach (var odt in od.DocumentDocumentDocumentTypes)
                    {
                        d.DocumentDocumentDocumentTypes.Add(new DocumentDocumentType
                        {
                            DocumentTypeId = odt.DocumentTypeId,
                            DocumentType = odt.DocumentType
                        });
                    }
                }
                await DB.SaveChangesAsync();
                //DB.Refresh(copiedDocs);
                Logger.Information(
                    "CopyUniqueMissingLineageDocuments({workflowId}) => {copiedDocumentCount}, {copiedAttachmentCount}",
                    workflowId,
                    copiedDocumentCount,
                    copiedAttachmentCount);
                return Json(new
                {
                    workflowId,
                    copiedDocumentCount,
                    copiedAttachmentCount,
                    documents = copiedDocs.Select(z => new
                    {
                        UserName = CurrentUser.UserName,
                        UploadedDate = PortalHelpers.ToLocalizedDisplayDateString(z.CreatedAtUtc),
                        AttachmentCount = z.DocumentAttachments.Count,
                        Name = z.DocumentName,
                        Id = z.DocumentId,
                        DocumentTypeNames = z.DocumentDocumentDocumentTypes.Select(dt => dt.DocumentType?.Name)
                    })
                });
            }
            catch (Exception ex)
            {
                return base.CreateJsonError(ex);
            }
        }
    }
}
