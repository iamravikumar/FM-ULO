using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Autofac;
using GSA.UnliquidatedObligations.BusinessLayer.Data;

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
        public ActionResult Create()
        {
            return View();
        }

        // POST: Review/Create
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
