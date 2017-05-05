using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GSA.UnliquidatedObligations.Web.Models;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class AttachmentsController : Controller
    {

        private readonly ULODBEntities DB;
        public AttachmentsController(ULODBEntities db)
        {
            DB = db;
        }

        [HttpPost]
        public async Task<JsonResult> FileUpload(int documentId)
        {
            var attachmentsAdded = new List<Attachment>();
            try
            {
                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        // get a stream
                        var stream = fileContent.InputStream;
                        // and optionally write the file to disk
                        var fileName = Path.GetFileName(fileContent.FileName);
                        var storageName = Guid.NewGuid() + Path.GetExtension(fileName);
                        var path = Path.Combine(Server.MapPath("~/App_Data/DocStorage"), storageName);
                        using (var fileStream = System.IO.File.Create(path))
                        {
                            stream.CopyTo(fileStream);
                        }
                        var attachment = new Attachment
                        {
                            FileName = fileName,
                            FilePath = path,
                            DocumentId = documentId
                        };
                        attachmentsAdded.Add(attachment);
                        DB.Attachments.Add(attachment);
                    }
                }
                await DB.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }

            return Json(attachmentsAdded);
        }


        // GET: Attachments
        public ActionResult Index()
        {
            return View();
        }

        // GET: Attachments/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Attachments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Attachments/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Attachments/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Attachments/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // POST: Attachments/Delete/5
        [HttpPost]
        public async Task<JsonResult> Delete(int attachmentId)
        {
            Attachment attachment;
            try
            {
                attachment = await DB.Attachments.FindAsync(attachmentId);
                DB.Attachments.Remove(attachment);
                await DB.SaveChangesAsync();
            }
            catch
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(Response);
            }

            return Json(attachment);
        }
    }
}
