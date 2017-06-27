using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class AttachmentsController : BaseController
    {
        public AttachmentsController(ULODBEntities db, IComponentContext componentContext)
            : base(db, componentContext)
        { }

        [HttpPost]
        public async Task<JsonResult> FileUpload(int documentId)
        {
            var attachmentsAdded = new List<Attachment>();
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
                        var path = PortalHelpers.GetStorageFolderPath($"Temp/{Guid.NewGuid()}.dat");
                        using (var fileStream = System.IO.File.Create(path))
                        {
                            await fileContent.InputStream.CopyToAsync(fileStream);
                        }
                        var attachment = new Attachment
                        {
                            FileName = fileContent.FileName,
                            FilePath = path,
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
            var attachment = await DB.Attachments.FindAsync(attachmentId);
            var path = PortalHelpers.GetStorageFolderPath(attachment.FilePath, false);
            return File(System.IO.File.OpenRead(path), System.Net.Mime.MediaTypeNames.Application.Octet, attachment.FileName);
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
