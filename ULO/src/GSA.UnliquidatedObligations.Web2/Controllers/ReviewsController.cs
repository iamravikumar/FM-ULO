using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Authorization;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    [Authorize]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.ApplicationUser)]
    [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanViewReviews)]
    public class ReviewsController : BasePageController
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
            public const string ActiveCardholderFiles = "ActiveCardholderFiles";
            public const string PegasysOpenItemsCreditCards = "PegasysOpenItemsCreditCardsFiles";
            public const string CreditCardAliasCrosswalkFiles = "CreditCardAliasCrosswalkFiles";

            public const int ReviewFileTypeCount = 7;
        }

        private readonly IBackgroundJobClient BackgroundJobClient;
       
        public ReviewsController(IBackgroundJobClient backgroundJobClient, UloDbContext db, PortalHelpers portalHelpers, UserHelpers userHelpers, ICacher cacher, Serilog.ILogger logger)
            : base(db, cacher, portalHelpers, userHelpers, logger)
        {
            BackgroundJobClient = backgroundJobClient;

        }

        [ActionName(ActionNames.Index)]
        [Route("reviews")]
        public ActionResult Index(string sortCol, string sortDir, int? page, int? pageSize)
        {
            var reviews = DB.Reviews.AsQueryable();

            if (sortCol == nameof(ReviewModel.ReviewTypeId))
            {
                reviews = ApplyBrowse(
                    reviews,
                    typeof(ReviewTypeEnum), nameof(ReviewModel.ReviewTypeId), sortDir, page, pageSize);
            }
            else if (sortCol == nameof(ReviewModel.ReviewScopeId))
            {
                reviews = ApplyBrowse(
                   reviews,
                   typeof(ReviewScopeEnum), nameof(ReviewModel.ReviewScopeId), sortDir, page, pageSize);
            }
            else
            {
                if (StringHelpers.TrimOrNull(sortCol)==null)
                {
                    sortCol = nameof(Review.ReviewId);
                    sortDir = RevolutionaryStuff.AspNetCore.AspHelpers.SortDirDescending;
                }
                reviews = ApplyBrowse(reviews, sortCol, sortDir, page, pageSize);
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
            var reviewStats = DB.ReviewStats.FirstOrDefault(z=>z.ReviewId==reviewId); 
            var reviewDetailsModel = new ReviewDetailsModel(review, reviewStats);            
            return reviewDetailsModel;
        }


        [Route("reviews/save")]
        [HttpPost]
        [ActionName(ActionNames.Save)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanCreateReviews)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(
            [Bind(new[]{
                nameof(ReviewDetailsModel.Review),
                nameof(ReviewDetailsModel.Review)+"."+nameof(Review.ReviewId),
                nameof(ReviewDetailsModel.Review)+"."+nameof(Review.IsClosed),
                nameof(ReviewDetailsModel.Review)+"."+nameof(Review.ReviewName),
                nameof(ReviewDetailsModel.Review)+"."+nameof(Review.Comments),
                nameof(ReviewDetailsModel.Review)+"."+nameof(Review.Status),
            })]
            ReviewDetailsModel m)
        {
            var reviewId = m.Review.ReviewId;
            bool errors = false;
            if (DB.Reviews.Any(r => r.ReviewName == m.Review.ReviewName && r.ReviewId != reviewId))
            {
                ModelState.AddModelError("ReviewName", "The name of this review is already in use.");
                errors = true;
            }
            if (!errors && ModelState.IsValid)
            {
                var r = await DB.Reviews.FindAsync(reviewId);
                if (r == null) return StatusCode(404);
                r.ReviewName = m.Review.ReviewName;
                r.Comments = m.Review.Comments;
                r.SetStatusDependingOnClosedBit(m.Review.IsClosed);
                await DB.SaveChangesAsync();
                return RedirectToIndex();
            }

            //var mulligan = await CreateReviewDetailsModelAsync(reviewId);
            //mulligan.Review.ReviewName = m.Review.ReviewName;
            //mulligan.Review.IsClosed = m.Review.IsClosed;
            return RedirectToAction(ActionNames.Details, new { id = reviewId });
        }


        [Route("reviews/delete")]
        [HttpPost]
        [ActionName(ActionNames.Delete)]
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanCreateReviews)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(
            [Bind(new[]{
                nameof(ReviewDetailsModel.Review),
                nameof(ReviewDetailsModel.Review)+"."+nameof(Review.ReviewId),
            })]
            ReviewDetailsModel m)
        {
            var r = await DB.Reviews.FindAsync(m.Review.ReviewId);
            if (r == null) return StatusCode(404);
            r.Delete();
            await DB.SaveChangesAsync();
            return RedirectToIndex();
        }

        private Task<ReviewModel> CreateReviewModelAsync()
        {
            var claimRegionIds = CurrentUser.GetApplicationPerimissionRegions(ApplicationPermissionNames.CanCreateReviews).ToList();
            return Task.FromResult(new ReviewModel(claimRegionIds,PortalHelpers));
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
        [Obsolete]
        public async Task<ActionResult> Create(
            [Bind(new[]{
                nameof(ReviewModel.RegionId),
                nameof(ReviewModel.ReviewName),                
                nameof(ReviewModel.ReviewTypeId),
                nameof(ReviewModel.ReviewScopeId),
                nameof(ReviewModel.Comments),               
                nameof(ReviewModel.RegionChoices),
                nameof(ReviewModel.ReviewDateInitiated)})]
            ReviewModel reviewModel)
        {
            bool errors = false;
            if (DB.Reviews.Any(r => r.ReviewName == reviewModel.ReviewName))
            {
                ModelState.AddModelError("ReviewName", "The name of this review is already in use.");
                errors = true;
            }
            var distinctFileTypes = new HashSet<string>();
            var distinctfilePath = Path.GetTempFileName();
            foreach (var formFile in Request.Form.Files)
            {               
                if (formFile.Length > 0)
                {
                    using (var inputStream = new FileStream(distinctfilePath, FileMode.Create))
                    {
                        // read file to stream
                        await formFile.CopyToAsync(inputStream);
                        // stream to byte array
                        byte[] array = new byte[inputStream.Length];
                        inputStream.Seek(0, SeekOrigin.Begin);
                        inputStream.Read(array, 0, array.Length);
                        // get file name
                        string file = formFile.FileName;
                        distinctFileTypes.Add(file);
                    }
                }
            }
            if (distinctFileTypes.Count != ReviewFileDesignators.ReviewFileTypeCount)
            {
                ModelState.AddModelError("Files", "You must upload at least one of each type of file.");
                errors = true;
            }
            var reviewScope = (ReviewScopeEnum)reviewModel.ReviewScopeId.GetValueOrDefault(-1);
            string workflowDefinitionName;
            if (!AspHelpers.WorkflowDefinitionNameByReviewScope.TryGetValue(reviewScope, out workflowDefinitionName))
            {
                ModelState.AddModelError("", $"Can't find workflowDefinitionName for scope={reviewScope}");
                errors = true;
            }
            if (ModelState.IsValid && !errors)
            {
                var wd = await DB.WorkflowDefinitions.Where(z => z.IsActive && z.WorkflowDefinitionName == workflowDefinitionName).OrderByDescending(z => z.WorkflowDefinitionId).FirstOrDefaultAsync();

                if (wd == null)
                {
                    ModelState.AddModelError("", $"Can't find active workflowDefinition for workflowDefinitionName={workflowDefinitionName}");
                    errors = true;
                    goto ErrorReturn;
                }

                //content += "before review object create<br />";
                var review = new Review
                {
                    RegionId = reviewModel.RegionId,
                    ReviewName = reviewModel.ReviewName,
                    Status = Review.StatusNames.Creating,
                    ReviewTypeId = (ReviewTypeEnum)reviewModel.ReviewTypeId.Value,  
                    Comments = reviewModel.Comments,
                    ReviewScopeId = (ReviewScopeEnum)reviewModel.ReviewScopeId.Value,
                    WorkflowDefinitionId = wd.WorkflowDefinitionId,
                    CreatedAtUtc = DateTime.UtcNow,
                    ReviewDateInitiated = reviewModel.ReviewDateInitiated
                    //ProjectDueDate = reviewModel.ProjectDueDate.Value,
                };
                DB.Reviews.Add(review);
                await DB.SaveChangesAsync();

                var uploadFiles = new UploadFilesModel(review.ReviewId);

                var path = PortalHelpers.GetStorageFolderPath($"ReviewUploads/{review.ReviewId / 1024}/{review.ReviewId}/{Guid.NewGuid()}.dat");
                foreach (var formFile in Request.Form.Files)
                {
                    if (formFile.Length > 0)
                    {
                        using (var inputStream = System.IO.File.Create(path))
                        {
                            // read file to stream
                            await formFile.CopyToAsync(inputStream);
                            // stream to byte array
                            byte[] array = new byte[inputStream.Length];
                            inputStream.Seek(0, SeekOrigin.Begin);
                            inputStream.Read(array, 0, array.Length);
                            // get file name
                            string fName = formFile.FileName;
                        }
                   
                        switch (formFile.FileName)
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
                            case ReviewFileDesignators.ActiveCardholderFiles:
                                uploadFiles.ActiveCardholderFiles.Add(path);
                                break;
                            case ReviewFileDesignators.PegasysOpenItemsCreditCards:
                                uploadFiles.PegasysOpenItemsCreditCards.Add(path);
                                break;
                            case ReviewFileDesignators.CreditCardAliasCrosswalkFiles:
                                uploadFiles.CreditCardAliasCrosswalkFiles.Add(path);
                                break;
                            default:
                                throw new UnexpectedSwitchValueException(formFile.FileName);
                        }
                    }
                }

                var uploadFilesJobId = BackgroundJobClient.Enqueue<IBackgroundTasks>(bt => bt.UploadFiles(uploadFiles));

                var jobId2 = BackgroundJob.ContinueWith<IBackgroundTasks>(uploadFilesJobId,
                    bt => bt.CreateULOsAndAssign(review.ReviewId, review.WorkflowDefinitionId, review.ReviewDateInitiated.Value));

                BackgroundJob.ContinueJobWith<IBackgroundTasks>(jobId2, bt => bt.AssignWorkFlows(review.ReviewId, PortalHelpers.SendBatchEmailsDuringAssignWorkflows));
                return RedirectToIndex();
            }
    ErrorReturn:
            var m = await CreateReviewModelAsync();
            reviewModel.RegionChoices = m.RegionChoices;
            reviewModel.ReviewTypes = m.ReviewTypes;
            reviewModel.ReviewScopes = m.ReviewScopes;
            return View(reviewModel);
        }
    }
}
