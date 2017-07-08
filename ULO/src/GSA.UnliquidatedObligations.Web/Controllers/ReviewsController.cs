using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Hangfire;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class ReviewsController : BaseController
    {
        private readonly IBackgroundJobClient BackgroundJobClient;

        public ReviewsController(IBackgroundJobClient backgroundJobClient, ULODBEntities db, IComponentContext componentContext)
            : base(db, componentContext)
        {
            BackgroundJobClient = backgroundJobClient;
        }

        // GET: Review
        public ActionResult Index(string sortCol, string sortDir, int? page, int? pageSize)
        {
            var reviews = ApplyBrowse(
                DB.Reviews,
                sortCol ?? nameof(Review.CreatedAt), sortDir??AspHelpers.SortDirDescending, page, pageSize);
            return View("", reviews);
        }

        // GET: Review/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var review = await DB.Reviews.FindAsync(id);
            var reviewStats = await DB.ReviewStats.FindAsync(id);
            var reviewDetailsModel = new ReviewDetailsModel(review, reviewStats);
            return View(reviewDetailsModel);
        }

        // GET: Review/Create
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanCreateReviews)]
        public async Task<ActionResult> Create()
        {
            var user = await DB.AspNetUsers.FirstOrDefaultAsync(u => u.UserName == this.User.Identity.Name);

            //get claim Region Ids for user
            var claimRegionIds = user.GetApplicationPerimissionRegions(ApplicationPermissionNames.ManageUsers).ToList();

            var reviewTypes = Enum.GetValues(typeof(ReviewTypeEnum)).Cast<ReviewTypeEnum>().ToList();
            var reviewScopes = Enum.GetValues(typeof(ReviewScopeEnum)).Cast<ReviewScopeEnum>().ToList();
            var workflowDefinitions = await DB.WorkflowDefinitions.Where(wd=>wd.IsActive).OrderBy(wfd => wfd.WorkflowDefinitionName).ToListAsync();
            return View(new ReviewModel(claimRegionIds, reviewTypes, reviewScopes, workflowDefinitions));
        }

        // POST: Review/Create
        [HttpPost]
        public async Task<ActionResult> Create([Bind(Include = "RegionId,ReviewName,ReviewStatus,ReviewId,ReviewTypeId,ReviewScopeId,Comments,Review,WorkflowDefinitionId,ReviewDateInitiated")] ReviewModel reviewModel)
        {
            //var content = "inside create<br />";
            try
            {
                if (DB.Reviews.Any(r => r.ReviewName == reviewModel.ReviewName))
                {
                    ModelState.AddModelError("ReviewName", "Review Name is already being used in system.");
                }
                if (ModelState.IsValid)
                {

                    //content += "before review object create<br />";
                    var review = new Review
                    {
                        RegionId = reviewModel.RegionId.Value,
                        ReviewName = reviewModel.ReviewName,
                        Status = "Open",
                        ReviewTypeId = reviewModel.ReviewTypeId.Value,
                        Comments = reviewModel.Comments,
                        ReviewScopeId = reviewModel.ReviewScopeId.Value,
                        WorkflowDefinitionId = reviewModel.WorkflowDefinitionId.Value,
                        CreatedAtUtc = DateTime.UtcNow,
                        ReviewDateInitiated = reviewModel.ReviewDateInitiated
                        //ProjectDueDate = reviewModel.ProjectDueDate.Value,
                        
                    };
                    DB.Reviews.Add(review);
                    await DB.SaveChangesAsync();
                    //content += "after review save<br />";

                    var uploadFiles = new UploadFilesModel(review.ReviewId);
                    foreach (string file in Request.Files)
                    {
                        //content += "staring " + file + "upload <br />";
                        var fileContent = Request.Files[file];
                        if (fileContent != null && fileContent.ContentLength > 0)
                        {
                            var path = PortalHelpers.GetStorageFolderPath($"ReviewUploads/{review.ReviewId / 1024}/{review.ReviewId}/{Guid.NewGuid()}.dat");
                            using (var fileStream = System.IO.File.Create(path))
                            {
                                await fileContent.InputStream.CopyToAsync(fileStream);
                            }
                            switch (file)
                            {
                                case "pegasysFiles":
                                    uploadFiles.PegasysFilePathsList.Add(path);
                                    break;
                                case "retaFiles":
                                    uploadFiles.RetaFileList.Add(path);
                                    break;
                                case "easiFiles":
                                    uploadFiles.EasiFileList.Add(path);
                                    break;
                                case "One92Files":
                                    uploadFiles.One92FileList.Add(path);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    var uploadFilesJobId = BackgroundJobClient.Enqueue<IBackgroundTasks>(
                            bt => bt.UploadFiles(uploadFiles));

                    var jobId2 = BackgroundJob.ContinueWith<IBackgroundTasks>(uploadFilesJobId,
                        bt => bt.CreateULOsAndAssign(review.ReviewId, review.WorkflowDefinitionId, review.ReviewDateInitiated.Value));

                    BackgroundJob.ContinueWith<IBackgroundTasks>(jobId2, bt => bt.AssignWorkFlows(review.ReviewId));
                    return RedirectToAction("Index");
                }
                return await Create();

            }
            catch (Exception ex)
            {
                //Response.StatusCode = (int)HttpStatusCode;
                return Json(new
                {
                    Exception = ex.Message
                });
            }
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
