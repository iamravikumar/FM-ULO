using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Properties;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class ReviewsController : BaseController
    {
        private readonly ULODBEntities DB;
        public ReviewsController(ULODBEntities db, IComponentContext componentContext)
            : base(componentContext)
        {
            DB = db;
        }

        // GET: Review
        public ActionResult Index()
        {
            var reviews = DB.Reviews.ToList();
            return View(reviews);
            //return View("~/Views/Reviews/Index.cshtml");
        }

        // GET: Review/Details/5
        public ActionResult Details(int id)
        {
           
            return View();
        }

        // GET: Review/Create
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanCreateReviews)]
        public async Task<ActionResult> Create()
        {
            var user = await DB.AspNetUsers.FirstOrDefaultAsync(u => u.UserName == this.User.Identity.Name);

            //get claim Region Ids for user
            var claimRegionIds = user.GetApplicationPerimissionRegions(ApplicationPermissionNames.ManageUsers).ToList();
            return View(new ReviewModel(claimRegionIds));
        }

        // POST: Review/Create
        [HttpPost]
        public ActionResult Create(IEnumerable<HttpPostedFileBase> files)
        {
            try
            {

                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    //if (fileContent != null && fileContent.ContentLength > 0)
                    //{
                    //    // get a stream
                    //    var stream = fileContent.InputStream;
                    //    // and optionally write the file to disk
                    //    var fileName = Path.GetFileName(fileContent.FileName);
                    //    var storageName = Guid.NewGuid() + Path.GetExtension(fileName);
                    //    var path = Path.Combine(HostingEnvironment.MapPath("~/Content/DocStorage/Temp"), storageName);
                    //    var webPath = Settings.Default.SiteUrl + "/Content/DocStorage/Temp/" + storageName;
                    //    using (var fileStream = System.IO.File.Create(path))
                    //    {
                    //        stream.CopyTo(fileStream);
                    //    }
                    //    var attachment = new Attachment
                    //    {
                    //        FileName = fileName,
                    //        FilePath = webPath,
                    //        DocumentId = documentId,
                    //    };
                    //    attachmentsAdded.Add(attachment);

                    //    //DB.Attachments.Add(attachment);
                    //}
                    //}
                    //attachmentsTempData.AddRange(attachmentsAdded);
                    //TempData["attachments"] = attachmentsTempData;
                    return RedirectToAction("Index");
                    //await DB.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                //Response.StatusCode = (int)HttpStatusCode;
                return Json(new
                {
                    Exception = ex.Message
                });
            }
            //try
            //{
            //    // TODO: Add insert logic here

            //    return RedirectToAction("Index");
            //}
            //catch
            //{
            //    return View();
            //}
        }

        // GET: Review/Edit/5
        public ActionResult Edit(int id)
        {
            var review = DB.Reviews.Find(id);
            return View(review);
        }

        // POST: Review/Edit/5
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

        // GET: Review/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Review/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
