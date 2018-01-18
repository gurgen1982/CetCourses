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
    public class HoursController : Controller
    {
        private dbEntities db = new dbEntities();

        // GET: Admin/Hours
        public ActionResult Index()//(int? id)
        {
            //var day = id == null ? 1 : (int)id;
            //ViewBag.DayOfWeekList = db.DayOfWeeks.Select(p => new SelectListItem { Value = p.TheDay.ToString(), Text = p.Day, Selected = p.TheDay == day }).ToList();
            //ViewBag.DayofWeek = day;
            //return View(db.Hours.Where(p => p.DayOfWeek == day).ToList());

            return View((db.DayHours.Include(x => x.BaseHours)).ToList());
        }
        [HttpPost]
        public ActionResult Index(FormCollection form)//([Bind(Include = "DayOfWeek")] Hour hour)
        {
            foreach (var key in Request.Form.AllKeys)
            {
                try
                {
                    var value = Request.Form.GetValues(key);
                    if (key.Contains("chk__"))
                    {
                        var keySplitted = key.Split(new string[] { "__" }, options: StringSplitOptions.None);
                        if (keySplitted.Length == 2)
                        {
                            var DayHourId = Convert.ToInt32(keySplitted[1]);
                            var dh = db.DayHours.Find(DayHourId);
                            if (dh != null)
                            {
                                dh.Inactive = value.Length > 1;
                            }
                            db.Entry(dh).State = EntityState.Modified;
                        }
                    }
                }
                catch { }
            }
            db.SaveChanges();
            
            return Index();
        }

        //// GET: Admin/Hours/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Hour hour = db.Hours.Find(id);
        //    if (hour == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(hour);
        //}

        //// GET: Admin/Hours/Create
        //public ActionResult Create(int id)
        //{
        //    return View(new Hour { DayOfWeek = id });
        //}

        //// POST: Admin/Hours/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "HourId,DayOfWeek,Hours,Inactive")] Hour hour)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Hours.Add(hour);
        //        db.SaveChanges();
        //        return RedirectToAction("Index", new { id = hour.DayOfWeek });
        //    }

        //    return View(hour);
        //}

        //// GET: Admin/Hours/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Hour hour = db.Hours.Find(id);
        //    if (hour == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(hour);
        //}

        //// POST: Admin/Hours/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "HourId,DayOfWeek,Hours,Inactive")] Hour hour)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(hour).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index", new { id = hour.DayOfWeek });
        //    }
        //    return View(hour);
        //}

        //// GET: Admin/Hours/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Hour hour = db.Hours.Find(id);
        //    if (hour == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(hour);
        //}

        //// POST: Admin/Hours/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Hour hour = db.Hours.Find(id);
        //    db.Hours.Remove(hour);
        //    db.SaveChanges();
        //    return RedirectToAction("Index", new { id = hour.DayOfWeek });
        //}

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
