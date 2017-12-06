using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    public class AttachmentsController : BaseController
    {
        public const string Name = "Attachments";

        public static class ActionNames
        {
            public const string Download = "Download";
            public const string View = "View";
        }

        public AttachmentsController(ULODBEntities db, IComponentContext componentContext, ICacher cacher, Serilog.ILogger logger)
            : base(db, componentContext, cacher, logger)
        { }

        public JsonResult Throw(string message)
        {
            try
            {
                throw new Exception(message ?? "NoMessage");
            }
            catch (Exception ex)
            {
                return Json(new ExceptionError(ex), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult FileShareInfo(string relativePath, bool create=false)
        {
            try
            {
                if (!Properties.Settings.Default.AllowFileShareInfo)
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, "This functionality has been disabled via configuration");
                }
                var ret = new
                {
                    docPath = Properties.Settings.Default.DocPath,
                    relativePath = relativePath,
                    fullPath = PortalHelpers.GetStorageFolderPath(relativePath, create),
                    create = create
                };
                return Json(ret, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new ExceptionError(ex), JsonRequestBehavior.AllowGet);
            }
        }

        public class FileUploadAttachmentResult : Attachment
        {
            public bool Added { get; set; }
            public bool Whitelisted { get; set; }
            public bool SaveError { get; set; }
            public IList<string> ErrorMessages { get; set; } = new List<string>();
        }

        [HttpPost]
        public async Task<JsonResult> FileUpload(int documentId)
        {
            var results = new List<FileUploadAttachmentResult>();
            List<Attachment> attachmentsTempData = null;
            if (TempData[PortalHelpers.TempDataKeys.Attachments] != null)
            {
                attachmentsTempData = (List<Attachment>) TempData[PortalHelpers.TempDataKeys.Attachments];
            }
            attachmentsTempData = attachmentsTempData ?? new List<Attachment>();
            foreach (string file in Request.Files)
            {
                var fileContent = Request.Files[file];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    var path = PortalHelpers.GetStorageFolderPath($"Temp/{Guid.NewGuid()}.dat");
                    var attachment = new FileUploadAttachmentResult
                    {
                        AttachmentsId = Stuff.Random.Next(int.MaxValue),
                        FileName = fileContent.FileName,
                        FilePath = path,
                        CreatedByUserId = CurrentUserId,
                        DocumentId = documentId,
                        FileSize = fileContent.ContentLength,
                        ContentType = fileContent.ContentType ?? System.Net.Mime.MediaTypeNames.Application.Octet
                    };
                    attachment.Whitelisted = PortalHelpers.VerifyFileAccept(
                        Properties.Settings.Default.AttachmentFileUploadAccept,
                        attachment.FileName,
                        attachment.ContentType);
                    if (attachment.Whitelisted)
                    {
                        try
                        {
                            using (var fileStream = System.IO.File.Create(path))
                            {
                                await fileContent.InputStream.CopyToAsync(fileStream);
                            }
                            attachment.Added = true;
                        }
                        catch (Exception ex)
                        {
                            attachment.SaveError = true;
                            attachment.ErrorMessages.Add(ex.Message);
                        }
                    }
                    else
                    {
                        attachment.ErrorMessages.Add(Properties.Settings.Default.AttachmentFileUploadAcceptMessage);
                    }
                    results.Add(attachment);
                }
            }
            attachmentsTempData.AddRange(results.Where(z=>z.Added));
            TempData[PortalHelpers.TempDataKeys.Attachments] = attachmentsTempData;
            return Json(results);
        }

        [HttpGet]
        [ActionName(ActionNames.View)]
        [Route("attachments/{attachmentId}/view")]
        public async Task<ActionResult> View(int attachmentId)
        {
            var attachment = await DB.Attachments.FindAsync(attachmentId);
            if (attachment == null) return HttpNotFound();
            var path = PortalHelpers.GetStorageFolderPath(attachment.FilePath, false);
            Log.Information("Attachment {AttachmentId} was viewed from {AttachmentPath}", attachmentId, path);
            return File(System.IO.File.OpenRead(path), attachment.ContentType);
        }

        [HttpGet]
        [ActionName(ActionNames.Download)]
        [Route("attachments/{attachmentId}/download")]
        public async Task<ActionResult> Download(int attachmentId)
        {
            var attachment = await DB.Attachments.FindAsync(attachmentId);
            if (attachment == null) return HttpNotFound();
            var path = PortalHelpers.GetStorageFolderPath(attachment.FilePath, false);
            Log.Information("Attachment {AttachmentId} was downloaded from {AttachmentPath}", attachmentId, path);
            return File(System.IO.File.OpenRead(path), attachment.ContentType, attachment.FileName);
        }

        // POST: Attachments/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(int attachmentId)
        {
            var attachment = await DB.Attachments.FindAsync(attachmentId);
            if (attachment == null) return HttpNotFound();
            try
            {
                var document = attachment.Document;
                Log.Information("Attachment {AttachmentId} was soft deleted from {DocumentId}", attachmentId, document.DocumentId);
                attachment.Delete(CurrentUserId);
                if (document.Attachments.Where(a => a.AttachmentsId != attachmentId).Count() == 0)
                {
                    Log.Information("Document {DocumentId} was soft deleted because it had no more attachments", document.DocumentId);
                    document.Delete(CurrentUserId);
                }
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
