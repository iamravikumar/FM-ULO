using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Hosting;
using System.Web.Mvc;
using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Utility;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class DocumentsController : BaseController
    {
        private readonly ApplicationUserManager UserManager;

        public DocumentsController(ApplicationUserManager userManager, ULODBEntities db, IComponentContext componentContext)
            : base(db, componentContext)
        {
            UserManager = userManager;
        }

        // GET: Documents
        public async Task<ActionResult> Index()
        {
            var documents = DB.Documents.Include(d => d.DocumentType).Include(d => d.Workflow);
            return View(await documents.ToListAsync());
        }

        // GET: Documents/Details/5
        public ActionResult View(int? documentId, bool allowDocumentEdit = false)
        {
            Document document;
            if (documentId == 0)
            {
                document = new Document
                {
                    DocumentId = 0,
                    DocumentTypeId = 0,
                    Attachments = new List<Attachment>()
                };
            }
            else
            {
                document = DB.Documents.FirstOrDefault(dt => dt.DocumentId == documentId);
            }
            var documentTypes = DB.DocumentTypes.OrderBy(dt => dt.Name).ToList();
            if (document == null)
            {
                return HttpNotFound();
            }
            return PartialView("~/Views/Ulo/Details/Documents/_View.cshtml", new DocumentModalViewModel(document.DocumentId, document.DocumentName, document.DocumentTypeId, documentTypes, document.Attachments.ToList(), allowDocumentEdit));
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> Save(int? documentId, string documentName, int workflowId, int documentTypeId)
        {
            if (ModelState.IsValid)
            {

                var currentUser = await UserManager.FindByNameAsync(this.User.Identity.Name);
                Document document;
                if (documentId != 0)
                {
                    document = await DB.Documents.FindAsync(documentId);
                    if (document != null)
                    {
                        document.DocumentTypeId = documentTypeId;
                        document.DocumentName = documentName;
                    }
                }
                else
                {
                    document = new Document
                    {
                        UploadedByUserId = currentUser.Id,
                        DocumentTypeId = documentTypeId,
                        WorkflowId = workflowId,
                        DocumentName = documentName
                    };
                    DB.Documents.Add(document);

                }
                document.UploadDate = DateTime.Now;
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
                            ContentType = tempAttachment.ContentType
                        };
                        var path = PortalHelpers.GetStorageFolderPath(attachment.FilePath);
                        System.IO.File.Copy(tempAttachment.FilePath, path);
                        DB.Attachments.Add(attachment);
                        Stuff.FileTryDelete(tempAttachment.FileName);
                    }
                    await DB.SaveChangesAsync();
                    TempData["attachments"] = null;
                }

                var documentType = await DB.DocumentTypes.FindAsync(documentTypeId);
                return Json(new
                {
                    Id = document.DocumentId,
                    UserName = currentUser.UserName,
                    DocumentTypeName = documentType.Name,
                    Name = document.DocumentName,
                    UploadedDate = document.UploadDate.ToString("MM/dd/yyyy")
                });
            }
            return null;
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
            Document document = await DB.Documents.FindAsync(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteConfirmed(int documentId)
        {
            var document = await DB.Documents.FindAsync(documentId);
            var attachments = await DB.Attachments.Where(a => a.DocumentId == documentId).ToListAsync();

            //TODO: look into hangfire for this
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var fileName = Path.GetFileName(attachment.FilePath);
                    var physicalPath = Path.Combine(HostingEnvironment.MapPath("~/Content/DocStorage"), fileName);

                    System.IO.File.Delete(physicalPath);
                }
            }
            DB.Documents.Remove(document);
            await DB.SaveChangesAsync();
            return Json(new
            {
                Id = documentId
            });
        }
    }
}
