using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Autofac;
using GSA.UnliquidatedObligations.Web.Properties;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class AttachmentsController : BaseController
    {

        private readonly ULODBEntities DB;
        public AttachmentsController(ULODBEntities db, IComponentContext componentContext)
            : base(componentContext)
        {
            DB = db;
        }

        [HttpPost]
        public async Task<JsonResult> FileUpload(int documentId)
        {
            var attachmentsAdded = new List<Attachment>();
            var docPath = Properties.Settings.Default.DocPath;
            List<Attachment> attachmentsTempData;
            if (TempData["attachments"] != null)
            {
                attachmentsTempData = (List<Attachment>) TempData["attachments"];
            }
            else
            {
                attachmentsTempData = new List<Attachment>();
            }
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

                        //var path = Path.Combine(HostingEnvironment.MapPath("~/Content/DocStorage/Temp"), storageName);
                        var path = docPath + "/Temp";
                        if (!Directory.Exists(path))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(path);
                        }
                        var storagePath = Path.Combine(path, storageName);
                        var webPath = Settings.Default.SiteUrl + "/Content/DocStorage/Temp/" + storageName;
                        using (var fileStream = System.IO.File.Create(storagePath))
                        {
                            stream.CopyTo(fileStream);
                        }
                        var attachment = new Attachment
                        {
                            FileName = fileName,
                            FilePath = webPath,
                            DocumentId = documentId,
                        };
                        attachmentsAdded.Add(attachment);
                        
                        //DB.Attachments.Add(attachment);
                    }
                }
                attachmentsTempData.AddRange(attachmentsAdded);
                TempData["attachments"] = attachmentsTempData;
                //await DB.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //Response.StatusCode = (int)HttpStatusCode;
                return Json(new
                {
                    Exception = ex.Message
                }); 
            }

            return Json(attachmentsAdded);
        }

        [HttpGet]
        public async Task<FileResult> Download(int attachmentId)
        {
            try
            {
                var attachment = await DB.Attachments.FindAsync(attachmentId);

                var path = attachment.FilePath;
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Octet, attachment.FileName);
 
            }
            catch (Exception)
            {
                throw new HttpException(404, "Error Downloading file");
            }
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
