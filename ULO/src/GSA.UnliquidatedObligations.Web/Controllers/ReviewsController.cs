using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Hangfire;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanViewReviews)]
    public class ReviewsController : BaseController
    {
        public const string Name = "Reviews";

        public static class ActionNames
        {
            public const string Index = "Index";
            public const string Details = "Details";
            public const string Create = "Create";
            public const string Save = "Save";
            public const string Delete = "Delete";
        }

        public static class ReviewFileDesignators
        {
            public const string PegasysFiles = "PegasysFiles";
            public const string RetaFiles = "retaFiles";
            public const string EasiFiles = "easiFiles";
            public const string One92Files = "One92Files";
        }

        private readonly IBackgroundJobClient BackgroundJobClient;

        public ReviewsController(IBackgroundJobClient backgroundJobClient, ULODBEntities db, IComponentContext componentContext, ICacher cacher, Serilog.ILogger logger)
            : base(db, componentContext, cacher, logger)
        {
            BackgroundJobClient = backgroundJobClient;
        }

        [ActionName(ActionNames.Index)]
        [Route("reviews")]
        public ActionResult Index(string sortCol, string sortDir, int? page, int? pageSize)
        {
            var reviews = DB.Reviews.Include(z => z.Region);
            if (sortCol == nameof(ReviewModel.ReviewTypeId))
            {
                reviews = ApplyBrowse(
                   reviews,
                   nameof(ReviewModel.ReviewTypeId), typeof(ReviewTypeEnum), sortDir ?? AspHelpers.SortDirDescending, page, pageSize);
            }
            else if (sortCol == nameof(ReviewModel.ReviewScopeId))
            {
                reviews = ApplyBrowse(
                   reviews,
                   nameof(ReviewModel.ReviewScopeId), typeof(ReviewScopeEnum), sortDir ?? AspHelpers.SortDirDescending, page, pageSize);
            }
            else
            {
                reviews = ApplyBrowse(
                    reviews,
                    sortCol ?? nameof(Review.CreatedAt), sortDir ?? AspHelpers.SortDirDescending, page, pageSize);
            }
            return View(reviews);
        }

        [ActionName(ActionNames.Details)]
        [Route("reviews/{id}")]
        public async Task<ActionResult> Details(int id)
        {
            var m = await CreateReviewDetailsModelAsync(id);
            return View(m);
        }

        private async Task<ReviewDetailsModel> CreateReviewDetailsModelAsync(int reviewId)
        {
            var review = await DB.Reviews.FindAsync(reviewId);
            var reviewStats = await DB.ReviewStats.FindAsync(reviewId);
            var reviewDetailsModel = new ReviewDetailsModel(review, reviewStats);
            return reviewDetailsModel;
        }


        [Route("reviews/save")]
        [HttpPost]
        [ActionName(ActionNames.Save)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanCreateReviews)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(
            [Bind(Include =
                nameof(ReviewDetailsModel.Review)+","+
                nameof(ReviewDetailsModel.Review)+"."+nameof(Review.ReviewId)+","+
                nameof(ReviewDetailsModel.Review)+"."+nameof(Review.IsClosed)+","+
                nameof(ReviewDetailsModel.Review)+"."+nameof(Review.ReviewName)
            )]
            ReviewDetailsModel m)
        {
            var reviewId = m.Review.ReviewId;
            bool errors = false;
            if (DB.Reviews.Any(r => r.ReviewName == m.Review.ReviewName && r.ReviewId!=reviewId))
            {
                ModelState.AddModelError("ReviewName", "The name of this review is already in use.");
                errors = true;
            }
            if (!errors && ModelState.IsValid)
            {
                var r = await DB.Reviews.FirstOrDefaultAsync(z => z.ReviewId == reviewId);
                if (r == null) return HttpNotFound();
                r.ReviewName = m.Review.ReviewName;
                r.SetStatusDependingOnClosedBit(m.Review.IsClosed);
                await DB.SaveChangesAsync();
                return RedirectToIndex();
            }
            
            var mulligan = await CreateReviewDetailsModelAsync(reviewId);
            mulligan.Review.ReviewName = m.Review.ReviewName;
            mulligan.Review.IsClosed = m.Review.IsClosed;
            return RedirectToAction(ActionNames.Details, mulligan);
        }


        [Route("reviews/delete")]
        [HttpPost]
        [ActionName(ActionNames.Delete)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanCreateReviews)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(
            [Bind(Include =
                nameof(ReviewDetailsModel.Review)+","+
                nameof(ReviewDetailsModel.Review)+"."+nameof(Review.ReviewId)+","
            )]
            ReviewDetailsModel m)
        {
            var r = await DB.Reviews.FirstOrDefaultAsync(z => z.ReviewId == m.Review.ReviewId);
            if (r == null) return HttpNotFound();
            r.Delete();
            await DB.SaveChangesAsync();
            return RedirectToIndex();
        }

        private Task<ReviewModel> CreateReviewModelAsync()
        {
            var claimRegionIds = CurrentUser.GetApplicationPerimissionRegions(ApplicationPermissionNames.CanCreateReviews).ToList();
            return Task.FromResult(new ReviewModel(claimRegionIds));
        }

        // GET: Review/Create
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanCreateReviews)]
        [ActionName(ActionNames.Create)]
        [Route("reviews/create")]
        public async Task<ActionResult> Create()
        {
            var m = await CreateReviewModelAsync();
            return View(m);
        }

        // POST: Review/Create
        [HttpPost]
        [Route("reviews/create")]
        public async Task<ActionResult> Create(
            [Bind(Include =
                nameof(ReviewModel.RegionId)+","+
                nameof(ReviewModel.ReviewName)+","+
                //nameof(ReviewModel.ReviewStatus)+","+
                nameof(ReviewModel.ReviewTypeId)+","+
                nameof(ReviewModel.ReviewScopeId)+","+
                nameof(ReviewModel.Comments)+","+
                //nameof(ReviewModel.Review)+","+
                nameof(ReviewModel.RegionChoices)+","+
                nameof(ReviewModel.ReviewDateInitiated))]
            ReviewModel reviewModel)
        {
            bool errors = false;
            if (DB.Reviews.Any(r => r.ReviewName == reviewModel.ReviewName))
            {
                ModelState.AddModelError("ReviewName", "The name of this review is already in use.");
                errors = true;
            }
            var distinctFileTypes = new HashSet<string>();
            foreach (string file in Request.Files)
            {
                var fileContent = Request.Files[file];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    distinctFileTypes.Add(file);
                }
            }
            if (distinctFileTypes.Count != 4)
            {
                ModelState.AddModelError("Files", "You must upload at least one of each type of file.");
                errors = true;
            }
            var reviewScope = (ReviewScopeEnum)reviewModel.ReviewScopeId.Value;
            string workflowDefinitionName;
            if (!PortalHelpers.WorkflowDefinitionNameByReviewScope.TryGetValue(reviewScope, out workflowDefinitionName))
            {
                ModelState.AddModelError("", $"Can't find workflowDefinitionName for scope={reviewScope}");
                errors = true;
            }
            if (ModelState.IsValid && !errors)
            {
                var wd = await DB.WorkflowDefinitions.Where(z => z.IsActive && z.WorkflowDefinitionName == workflowDefinitionName).OrderByDescending(z => z.WorkflowDefinitionId).FirstOrDefaultAsync();

                //content += "before review object create<br />";
                var review = new Review
                {
                    RegionId = reviewModel.RegionId,
                    ReviewName = reviewModel.ReviewName,
                    Status = Review.StatusNames.Creating,
                    ReviewTypeId = reviewModel.ReviewTypeId.Value,
                    Comments = reviewModel.Comments,
                    ReviewScopeId = reviewModel.ReviewScopeId.Value,
                    WorkflowDefinitionId = wd.WorkflowDefinitionId,
                    CreatedAtUtc = DateTime.UtcNow,
                    ReviewDateInitiated = reviewModel.ReviewDateInitiated
                    //ProjectDueDate = reviewModel.ProjectDueDate.Value,
                };
                DB.Reviews.Add(review);
                await DB.SaveChangesAsync();

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
                            case ReviewFileDesignators.PegasysFiles:
                                uploadFiles.PegasysFilePathsList.Add(path);
                                break;
                            case ReviewFileDesignators.RetaFiles:
                                uploadFiles.RetaFileList.Add(path);
                                break;
                            case ReviewFileDesignators.EasiFiles:
                                uploadFiles.EasiFileList.Add(path);
                                break;
                            case ReviewFileDesignators.One92Files:
                                uploadFiles.One92FileList.Add(path);
                                break;
                            default:
                                throw new UnexpectedSwitchValueException(file);
                        }
                    }
                }

                var uploadFilesJobId = BackgroundJobClient.Enqueue<IBackgroundTasks>(bt => bt.UploadFiles(uploadFiles));

                var jobId2 = BackgroundJob.ContinueWith<IBackgroundTasks>(uploadFilesJobId,
                    bt => bt.CreateULOsAndAssign(review.ReviewId, review.WorkflowDefinitionId, review.ReviewDateInitiated.Value));

                BackgroundJob.ContinueWith<IBackgroundTasks>(jobId2, bt => bt.AssignWorkFlows(review.ReviewId));
                return RedirectToIndex();
            }
            var m = await CreateReviewModelAsync();
            reviewModel.RegionChoices = m.RegionChoices;
            reviewModel.ReviewTypes = m.ReviewTypes;
            reviewModel.ReviewScopes = m.ReviewScopes;
            return View(reviewModel);
        }
    }
}
