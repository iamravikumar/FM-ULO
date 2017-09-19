using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    public class DocumentsController : BaseController
    {
        public const string Name = "Documents";

        public static class ActionNames
        {
            public const string View = "View";
        }

        private readonly ApplicationUserManager UserManager;

        public DocumentsController(ApplicationUserManager userManager, ULODBEntities db, IComponentContext componentContext, ICacher cacher, Serilog.ILogger logger)
            : base(db, componentContext, cacher, logger)
        {
            UserManager = userManager;
            PopulateDocumentTypeNameByDocumentTypeIdInViewBag();
        }

        // GET: Documents
        public async Task<ActionResult> Index()
        {
            var documents = DB.Documents.Include(d => d.DocumentDocumentTypes).Include(d => d.Workflow);
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
                        select z.Workflow.UnliquidatedObligation.DocType
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
                    Include(z=>z.Attachments).
                    Include(z=>z.AspNetUser).
                    Include(z => z.DocumentDocumentTypes).
                    FirstOrDefault(dt => dt.DocumentId == documentId);
                if (document == null)
                {
                    return HttpNotFound();
                }
            }
            document.Attachments = document.Attachments ?? new List<Attachment>();
            var documentTypes = Cacher.FindOrCreateValWithSimpleKey(
                Cache.CreateKey(typeof(DocumentsController), "documentTypes", docType),
                () => DB.DocumentTypes.Where(dt => dt.DocType == null || dt.DocType == docType).OrderBy(dt => dt.Name).ToList().AsReadOnly(),
                UloHelpers.MediumCacheTimeout);

            var workflowAssignedTo = document.DocumentId == 0 ? true : checkOwner(document);

            return PartialView("~/Views/Ulo/Details/Documents/_View.cshtml", new DocumentModalViewModel(document, documentTypes, workflowAssignedTo));
        }

        private bool checkOwner(Document document)
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

        // POST: Documents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> Save(int? documentId, string documentName, int workflowId)
        {
            if (!ModelState.IsValid) throw new ArgumentException(ModelState.ToString(), nameof(ModelState));
            Document document;
            if (documentId.GetValueOrDefault(0) > 0)
            {
                document = await DB.Documents.FindAsync(documentId);
                if (document == null)
                {
                    throw new KeyNotFoundException($"could not find documentId={documentId}");
                }
                document.DocumentDocumentTypes.ToList().ForEach(dt => DB.DocumentDocumentTypes.Remove(dt));
            }
            else
            {
                document = new Document
                {
                    UploadedByUserId = CurrentUserId,
                    WorkflowId = workflowId,
                    CreatedAtUtc = DateTime.UtcNow
                };
                DB.Documents.Add(document);
            }
            document.DocumentName = StringHelpers.TrimOrNull(documentName);
            var documentTypeIds = CSV.ParseIntegerRow(Request["documentTypeId"]);
            var documentTypeNames = new List<string>();
            var d = base.PopulateDocumentTypeNameByDocumentTypeIdInViewBag();
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
            if (TempData["attachments"] != null)
            {
                var attachmentsTempData = (List<Attachment>)TempData["attachments"];
                foreach (var tempAttachment in attachmentsTempData)
                {
                    var attachment = new Attachment
                    {
                        FileName = tempAttachment.FileName,
                        FilePath = $"Attachments/{document.DocumentId / 1024}/{document.DocumentId}/{Guid.NewGuid()}.dat",
                        DocumentId = document.DocumentId,
                        FileSize = tempAttachment.FileSize,
                        ContentType = tempAttachment.ContentType,
                        CreatedByUserId = tempAttachment.CreatedByUserId
                    };
                    var path = PortalHelpers.GetStorageFolderPath(attachment.FilePath);
                    System.IO.File.Copy(tempAttachment.FilePath, path);
                    DB.Attachments.Add(attachment);
                    Stuff.FileTryDelete(tempAttachment.FileName);
                }
                await DB.SaveChangesAsync();
                TempData["attachments"] = null;
            }
            return Json(new
            {
                Id = document.DocumentId,
                UserName = User.Identity.Name,
                Name = document.DocumentName,
                DocumentTypeNames = documentTypeNames.WhereNotNull().OrderBy().ToList(),
                AttachmentCount = document.Attachments.Count,
                UploadedDate = document.CreatedAtUtc.ToLocalTime().ToString("MM/dd/yyyy")
            });
        }

        public void Clear()
        {
            if (TempData["attachments"] != null)
            {
                var path = HostingEnvironment.MapPath("~/Content/DocStorage/Temp");
                var di = new DirectoryInfo(path);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                TempData["attachments"] = null;
            }
        }

        // GET: Documents/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var document = await DB.Documents.FindAsync(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int documentId)
        {
            var document = await DB.Documents.FindAsync(documentId);
            if (document == null)
            {
                return HttpNotFound();
            }
            document.Delete(CurrentUserId);
            await DB.SaveChangesAsync();
            Log.Information("Document {DocumentId} was soft deleted", document.DocumentId);
            return Json(new
            {
                Id = documentId
            });
        }
    }
}
