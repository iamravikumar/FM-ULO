﻿using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Authorization;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Models;
using GSA.UnliquidatedObligations.Web.Services;
using Hangfire;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.Web.Controllers
{
    public class ReviewsController : BaseController
    {
        public const string Name = "Reviews";

        public static class ActionNames
        {
            public const string Index = "Index";
        }

        private readonly IBackgroundJobClient BackgroundJobClient;

        public ReviewsController(IBackgroundJobClient backgroundJobClient, ULODBEntities db, IComponentContext componentContext, ICacher cacher)
            : base(db, componentContext, cacher)
        {
            BackgroundJobClient = backgroundJobClient;
        }

        [ActionName(ActionNames.Index)]
        public ActionResult Index(string sortCol, string sortDir, int? page, int? pageSize)
        {
            IQueryable reviews;
            if (sortCol == "ReviewTypeId")
            {
                reviews = ApplyBrowse(
                   DB.Reviews,
                   "ReviewTypeId", typeof(ReviewTypeEnum), sortDir ?? AspHelpers.SortDirDescending, page, pageSize);
            }
            else if (sortCol == "ReviewScopeId")
            {
                reviews = ApplyBrowse(
                   DB.Reviews,
                   "ReviewScopeId", typeof(ReviewScopeEnum), sortDir ?? AspHelpers.SortDirDescending, page, pageSize);
            }
            else
            {
                reviews = ApplyBrowse(
               DB.Reviews,
               sortCol ?? nameof(Review.CreatedAt), sortDir ?? AspHelpers.SortDirDescending, page, pageSize);

            }

            return View("", reviews);
        }

        private string convertToString(ReviewTypeEnum reviewTypeEnum)
        {
            return reviewTypeEnum.GetDisplayName();
        }

        private string convertToString(ReviewScopeEnum reviewScopeEnum)
        {
            return reviewScopeEnum.GetDisplayName();
        }

        // GET: Review/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var review = await DB.Reviews.FindAsync(id);
            var reviewStats = await DB.ReviewStats.FindAsync(id);
            var reviewDetailsModel = new ReviewDetailsModel(review, reviewStats);
            return View(reviewDetailsModel);
        }

        private async Task<ReviewModel> CreateReviewModelAsync()
        {
            var claimRegionIds = CurrentUser.GetApplicationPerimissionRegions(ApplicationPermissionNames.ManageUsers).ToList();
            var workflowDefinitions = await DB.WorkflowDefinitions.Where(wd => wd.IsActive).OrderBy(wfd => wfd.WorkflowDefinitionName).ToListAsync();
            return new ReviewModel(claimRegionIds, workflowDefinitions);
        }

        // GET: Review/Create
        [ApplicationPermissionAuthorize(ApplicationPermissionNames.CanCreateReviews)]
        public async Task<ActionResult> Create()
        {
            var m = await CreateReviewModelAsync();
            return View(m);
        }

        // POST: Review/Create
        [HttpPost]
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
                nameof(ReviewModel.WorkflowDefinitions)+","+
                nameof(ReviewModel.WorkflowDefinitionId)+","+
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
            if (ModelState.IsValid && !errors)
            {
                //content += "before review object create<br />";
                var review = new Review
                {
                    RegionId = reviewModel.RegionId,
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
            reviewModel.WorkflowDefinitions = m.WorkflowDefinitions;
            return View(reviewModel);
        }


        // GET: Review/Edit/5
        public ActionResult Edit(int id)
        {
            var review = DB.Reviews.Find(id);
            return View(review);
        }
    }
}
