using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Authorization;
using GSA.UnliquidatedObligations.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    public class AttachmentsController : BasePageController
    {
        public const string Name = "Attachments";

        public static class ActionNames
        {
            public const string Download = "Download";
            public const string View = "View";
            public const string FileUpload = "FileUpload";
        }

        public AttachmentsController(UloDbContext db, ICacher cacher, PortalHelpers portalHelpers, UserHelpers userHelpers, Serilog.ILogger logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        { }

        public JsonResult Throw(string message)
        {
            try
            {
                throw new Exception(message ?? "NoMessage");
            }
            catch (Exception ex)
            {
                return Json(new ExceptionError(ex));
            }
        }

        public JsonResult FileShareInfo(string relativePath, bool create = false)
        {
            try
            {
                if (!PortalHelpers.AllowFileShareInfo)
                {
                    Throw("This functionality has been disabled via configuration");
                    //throw new HttpException((int)HttpStatusCode.Forbidden, "This functionality has been disabled via configuration");                    
                }
                var ret = new
                {
                    docPath = PortalHelpers.DocPath,
                    relativePath = relativePath,
                    fullPath = PortalHelpers.GetStorageFolderPath(relativePath, create),
                    create = create
                };
                return Json(ret);
            }
            catch (Exception ex)
            {
                return Json(new ExceptionError(ex));
            }
        }

        [ActionName(ActionNames.FileUpload)]
        [HttpPost]
        public async Task<IActionResult> FileUpload(int documentId)
        {
            var results = new List<FileUploadAttachmentResult>();
            var attachmentsTempData = TempData.FileUploadAttachmentResults();
            foreach (var file in Request.Form.Files)
            {
                if(file.Length > 0)                
                {
                    var path = PortalHelpers.GetStorageFolderPath($"Temp/{Guid.NewGuid()}.dat");
                    var attachment = new FileUploadAttachmentResult
                    {
                        AttachmentsId = Stuff.Random.Next(int.MaxValue),
                        FileName = file.FileName,
                        FilePath = path,
                        CreatedByUserId = CurrentUserId,
                        DocumentId = documentId,
                        FileSize = file.Length,
                        ContentType = file.ContentType ?? System.Net.Mime.MediaTypeNames.Application.Octet
                    };
                    attachment.Whitelisted = PortalHelpers.VerifyFileAccept(attachment.FileName, attachment.ContentType);
                    if (attachment.Whitelisted)
                    {
                        try
                        {
                            using (var fileStream = System.IO.File.Create(path))
                            {
                                await file.CopyToAsync(fileStream);
                            }
                            attachment.Added = true;
                            attachmentsTempData.Add(attachment);
                        }
                        catch (Exception ex)
                        {
                            attachment.SaveError = true;
                            attachment.ErrorMessages.Add(ex.Message);
                        }
                    }
                    else
                    {
                        attachment.ErrorMessages.Add(PortalHelpers.AttachmentFileUploadAcceptMessage);
                    }
                    results.Add(attachment);
                }
            }
            TempData.FileUploadAttachmentResults(attachmentsTempData);
            return Json(results);
        }

        [HttpGet]
        [ActionName(ActionNames.View)]
        [Route("attachments/{attachmentId}/view")]
        public async Task<ActionResult> View(int attachmentId)
        {
            var attachment = await DB.Attachments.FindAsync(attachmentId);
            if (attachment == null) return NotFound();
            var path = PortalHelpers.GetStorageFolderPath(attachment.FilePath, false);
            Logger.Information("Attachment {AttachmentId} was viewed from {AttachmentPath}", attachmentId, path);
            return File(System.IO.File.OpenRead(path), attachment.ContentType);
        }

        [HttpGet]
        [ActionName(ActionNames.Download)]
        [Route("attachments/{attachmentId}/download")]
        public async Task<ActionResult> Download(int attachmentId)
        {
            var attachment = await DB.Attachments.FindAsync(attachmentId);
            if (attachment == null) return NotFound();
            var path = PortalHelpers.GetStorageFolderPath(attachment.FilePath, false);
            Logger.Information("Attachment {AttachmentId} was downloaded from {AttachmentPath}", attachmentId, path);
            return File(System.IO.File.OpenRead(path), attachment.ContentType, attachment.FileName);
        }


        private static JsonSerializerOptions JSO_Depth2 = new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = null,
            MaxDepth = 2
        };


        // POST: Attachments/Delete/5
        [HttpPost]
        public async Task<JsonResult> Delete(int attachmentId)
        {
            try
            {
                await CheckStalenessFromFormAsync();
                var attachment = await DB.Attachments.Include(a => a.Document).FirstOrDefaultAsync(a => a.AttachmentsId == attachmentId);
                if (attachment == null) throw new FileNotFoundException();
                try
                {
                    var document = attachment.Document;
                    Logger.Information("Attachment {AttachmentId} was soft deleted from {DocumentId}", attachmentId, document.DocumentId);
                    attachment.Delete(CurrentUserId);
                    if (document.DocumentAttachments.Where(a => a.AttachmentsId != attachmentId).Count() == 0)
                    {
                        Logger.Information("Document {DocumentId} was soft deleted because it had no more attachments", document.DocumentId);
                        document.Delete(CurrentUserId);
                    }
                    await DB.SaveChangesAsync();
                }
                catch
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(Response);
                }
                return Json(attachment, JSO_Depth2);
            }
            catch (Exception ex)
            {
                return base.CreateJsonError(ex);
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
    }
}
