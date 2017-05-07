using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Properties;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ULODBEntities DB;
        private readonly ApplicationUserManager UserManager;

        public DocumentsController(ULODBEntities db, ApplicationUserManager userManager)
        {
            DB = db;
            UserManager = userManager;
        }

        // GET: Documents
        public async Task<ActionResult> Index()
        {
            var documents = DB.Documents.Include(d => d.DocumentType).Include(d => d.Workflow);
            return View(await documents.ToListAsync());
        }

        // GET: Documents/Details/5
        public ActionResult View(int? documentId)
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
            return PartialView("~/Views/Ulo/Details/Documents/_View.cshtml", new DocumentModalViewModel(document.DocumentId, document.DocumentTypeId, documentTypes, document.Attachments.ToList()));
        }

        // GET: Documents/Create
        public ActionResult Create()
        {
            ViewBag.DocumentType = new SelectList(DB.DocumentTypes, "Id", "Name");
            ViewBag.WorkflowId = new SelectList(DB.Workflows, "WorkflowId", "WorkflowKey");
            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,DocumentType,UploadedBy,WorkflowId")] Document document)
        {
            if (ModelState.IsValid)
            {
                DB.Documents.Add(document);
                await DB.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.DocumentType = new SelectList(DB.DocumentTypes, "Id", "Name", document.DocumentType);
            ViewBag.WorkflowId = new SelectList(DB.Workflows, "WorkflowId", "WorkflowKey", document.WorkflowId);
            return View(document);
        }

        // GET: Documents/Edit/5
        public async Task<ActionResult> Edit(int? id)
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
            ViewBag.DocumentType = new SelectList(DB.DocumentTypes, "Id", "Name", document.DocumentType);
            ViewBag.WorkflowId = new SelectList(DB.Workflows, "WorkflowId", "WorkflowKey", document.WorkflowId);
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> Save(int? documentId, int workflowId, int documentTypeId)
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
                    }
                    await DB.SaveChangesAsync();
                }
                else
                {
                    document = new Document
                    {
                        UploadedByUserId = currentUser.Id,
                        DocumentTypeId = documentTypeId,
                        WorkflowId = workflowId,
                    };
                    DB.Documents.Add(document);
                    await DB.SaveChangesAsync();
                }
                if (TempData["attachments"] != null)
                {
                    List<Attachment> attachmentsTempData = (List<Attachment>) TempData["attachments"];
                    foreach (var tempAttachment in attachmentsTempData)
                    {
                        var newWebPath = tempAttachment.FilePath.Replace("Temp/", "");
                        var fileName = Path.GetFileName(tempAttachment.FilePath);
                        var tempPath = Path.Combine(HostingEnvironment.MapPath("~/Content/DocStorage/Temp"), fileName);
                        var newPhysicalPath = Path.Combine(HostingEnvironment.MapPath("~/Content/DocStorage"), fileName);
                        System.IO.File.Copy(tempPath, newPhysicalPath);
                        var attachment = new Attachment
                        {
                            FileName = tempAttachment.FileName,
                            FilePath = newWebPath,
                            DocumentId = document.DocumentId
                        };
                        DB.Attachments.Add(attachment);
                    }
                    await DB.SaveChangesAsync();
                    TempData["attachments"] = null;              
                }
                document.AspNetUser = new AspNetUser {UserName = currentUser.UserName};
                document.DocumentType = await DB.DocumentTypes.FindAsync(documentTypeId);
                return Json(document);
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Document document = await DB.Documents.FindAsync(id);
            DB.Documents.Remove(document);
            await DB.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
