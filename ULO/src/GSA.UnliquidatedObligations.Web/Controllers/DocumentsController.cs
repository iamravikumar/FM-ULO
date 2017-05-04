using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ULODBEntities DB;

        public DocumentsController(ULODBEntities db)
        {
            DB = db;
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
            if (documentId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var document = DB.Documents.FirstOrDefault(dt => dt.DocumentId == documentId);
            var documentTypes = DB.DocumentTypes.OrderBy(dt => dt.Name).ToList();
            if (document == null)
            {
                return HttpNotFound();
            }
            return PartialView("~/Views/Ulo/Details/Documents/_View.cshtml", new DocumentModalViewModel(document.DocumentId, document.DocumentTypeId, documentTypes));
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,DocumentType,UploadedBy,WorkflowId")] Document document)
        {
            if (ModelState.IsValid)
            {
                DB.Entry(document).State = EntityState.Modified;
                await DB.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.DocumentType = new SelectList(DB.DocumentTypes, "Id", "Name", document.DocumentType);
            ViewBag.WorkflowId = new SelectList(DB.Workflows, "WorkflowId", "WorkflowKey", document.WorkflowId);
            return View(document);
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
