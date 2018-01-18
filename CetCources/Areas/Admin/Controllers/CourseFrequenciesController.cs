using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CetCources.Database;

namespace CetCources.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CourseFrequenciesController : Controller
    {
        private dbEntities db = new dbEntities();

        // GET: Admin/CourseFrequencies
        public ActionResult Index()
        {
            return View(db.CourseFrequencies.Where(x=>x.FreqId>6).ToList());
        }

        // GET: Admin/CourseFrequencies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseFrequency courseFrequency = db.CourseFrequencies.Find(id);
            if (courseFrequency == null)
            {
                return HttpNotFound();
            }
            return View(courseFrequency);
        }

        // GET: Admin/CourseFrequencies/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/CourseFrequencies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FreqId,DaysCount,FrequencyDescription,Inactive")] CourseFrequency courseFrequency)
        {
            if (ModelState.IsValid)
            {
                db.CourseFrequencies.Add(courseFrequency);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(courseFrequency);
        }

        // GET: Admin/CourseFrequencies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseFrequency courseFrequency = db.CourseFrequencies.Find(id);
            if (courseFrequency == null)
            {
                return HttpNotFound();
            }
            return View(courseFrequency);
        }

        // POST: Admin/CourseFrequencies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FreqId,DaysCount,FrequencyDescription,Inactive")] CourseFrequency courseFrequency)
        {
            if (ModelState.IsValid)
            {
                db.Entry(courseFrequency).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(courseFrequency);
        }

        // GET: Admin/CourseFrequencies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseFrequency courseFrequency = db.CourseFrequencies.Find(id);
            if (courseFrequency == null)
            {
                return HttpNotFound();
            }
            return View(courseFrequency);
        }

        // POST: Admin/CourseFrequencies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CourseFrequency courseFrequency = db.CourseFrequencies.Find(id);
            db.CourseFrequencies.Remove(courseFrequency);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
