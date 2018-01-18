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
    public class YearGroupsController : Controller
    {
        private dbEntities db = new dbEntities();

        // GET: Admin/YearGroups
        public ActionResult Index()
        {
            return View(db.YearGroups.ToList());
        }

        // GET: Admin/YearGroups/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            YearGroup yearGroup = db.YearGroups.Find(id);
            if (yearGroup == null)
            {
                return HttpNotFound();
            }
            return View(yearGroup);
        }

        // GET: Admin/YearGroups/Create
        public ActionResult Create()
        {
            ViewBag.Days = db.DayOfWeeks.ToList();
            return View();
        }

        // POST: Admin/YearGroups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "YearGroupId,GroupName,From,To")] YearGroup yearGroup)
        {
            if (ModelState.IsValid)
            {
                var daysSelected = false;
                var prefix = "chk_";
                foreach (var item in Request.Form.AllKeys.Where(p => p.Contains(prefix)))
                {
                    if (Request.Form[item].Split(',').Length > 1)
                    {
                        yearGroup.YearGroupDays.Add(new YearGroupDay { Day = Convert.ToInt32(item.Replace(prefix, "")) });
                        daysSelected = true;
                    }
                }
                if (!daysSelected)
                {
                    ModelState.AddModelError(string.Empty, "Select at least one day of week");
                }
                else
                {
                    db.YearGroups.Add(yearGroup);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.Days = db.DayOfWeeks.ToList();
            return View(yearGroup);
        }

        // GET: Admin/YearGroups/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            YearGroup yearGroup = db.YearGroups.Find(id);
            if (yearGroup == null)
            {
                return HttpNotFound();
            }
            var query = (from days in db.DayOfWeeks
                         join ygDays in db.YearGroupDays.Where(p=>p.YearGroupId==yearGroup.YearGroupId)
                         on days.TheDay equals ygDays.Day
                         into joined
                         from lftJoined in joined.DefaultIfEmpty()
                         orderby days.TheDay
                         select new
                         {
                             TheDay = days.TheDay,
                             Day = days.Day,
                             IsChecked = lftJoined != null
                         })
                         .ToList();

            ViewBag.Days = query.Select(
                    p => new Database.DayOfWeek {
                        TheDay = p.TheDay,
                        Day = p.Day,
                        IsChecked = p.IsChecked
                    })
                    .ToList();

            return View(yearGroup);
        }

        // POST: Admin/YearGroups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "YearGroupId,GroupName,From,To")] YearGroup yearGroup)
        {
            if (ModelState.IsValid)
            {
                var prefix = "chk_";
                var daysSelected = false;
                foreach (var item in Request.Form.AllKeys.Where(p => p.Contains(prefix)))
                {
                    if (Request.Form[item].Split(',').Length > 1)
                    {
                        daysSelected = true;
                        break;
                    }
                }

                if (!daysSelected)
                {
                    ModelState.AddModelError(string.Empty, "Select at least one day of week");
                }
                else
                {
                    db.Entry(yearGroup).State = EntityState.Modified;
                    db.SaveChanges();
                    db.YearGroupDays.RemoveRange(db.YearGroupDays.Where(p => p.YearGroupId == yearGroup.YearGroupId));
                    foreach (var item in Request.Form.AllKeys.Where(p => p.Contains(prefix)))
                    {
                        if (Request.Form[item].Split(',').Length > 1)
                        {
                            db.YearGroupDays.Add(new YearGroupDay { YearGroupId = yearGroup.YearGroupId, Day = Convert.ToInt32(item.Replace(prefix, "")) });
                        }
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            var query = (from days in db.DayOfWeeks
                         join ygDays in db.YearGroupDays.Where(p => p.YearGroupId == yearGroup.YearGroupId)
                         on days.TheDay equals ygDays.Day
                         into joined
                         from lftJoined in joined.DefaultIfEmpty()
                         orderby days.TheDay
                         select new
                         {
                             TheDay = days.TheDay,
                             Day = days.Day,
                             IsChecked = lftJoined != null
                         })
                     .ToList();

            ViewBag.Days = query.Select(
                    p => new Database.DayOfWeek
                    {
                        TheDay = p.TheDay,
                        Day = p.Day,
                        IsChecked = p.IsChecked
                    })
                    .ToList();

            return View(yearGroup);
        }

        // GET: Admin/YearGroups/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            YearGroup yearGroup = db.YearGroups.Find(id);
            if (yearGroup == null)
            {
                return HttpNotFound();
            }
            return View(yearGroup);
        }

        // POST: Admin/YearGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            YearGroup yearGroup = db.YearGroups.Find(id);
            db.YearGroupDays.RemoveRange(db.YearGroupDays.Where(p => p.YearGroupId == id));
            db.YearGroups.Remove(yearGroup);
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
