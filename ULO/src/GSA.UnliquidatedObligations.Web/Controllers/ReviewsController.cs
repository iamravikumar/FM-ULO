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
using GSA.UnliquidatedObligations.Web.Services;
using Hangfire;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class ReviewsController : BaseController
    {
        private readonly ULODBEntities DB;
        private readonly IBackgroundJobClient BackgroundJobClient;
        public ReviewsController(ULODBEntities db, IBackgroundJobClient backgroundJobClient, IComponentContext componentContext)
            : base(componentContext)
        {
            DB = db;
            BackgroundJobClient = backgroundJobClient;
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
        public ActionResult Create([Bind(Include = "RegionId,ReviewName,ReviewStatus,TypeOfReview,Comments")] ReviewModel reviewModel)
        {
            try
            {
                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        var stream = fileContent.InputStream;
                        var fileName = Path.GetFileName(fileContent.FileName);
                        var storageName = Guid.NewGuid() + Path.GetExtension(fileName);
                        var path = Path.Combine(HostingEnvironment.MapPath("~/Content/DocStorage/ReviewUploads"), storageName);
                        using (var fileStream = System.IO.File.Create(path))
                        {
                            stream.CopyTo(fileStream);
                        }
                        var review = new Review
                        {
                            RegionId = reviewModel.RegionId.Value,
                            ReviewName = reviewModel.ReviewName,
                            Status = reviewModel.ReviewStatus,
                            TypeOfReview = reviewModel.TypeOfReview,
                            Comments = reviewModel.Comments,
                            CreatedAtUtc = DateTime.Now
                        };
                        DB.Reviews.Add(review);
                        DB.SaveChanges();
                        BackgroundJobClient.Enqueue<IBackgroundTasks>(bt => bt.UploadReviewHoldIngTable(review.ReviewId, path));
                    }
                }

                return RedirectToAction("Index");
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
