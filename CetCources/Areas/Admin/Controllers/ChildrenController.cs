using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CetCources.Database;
using PagedList;

namespace CetCources.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ChildrenController : Controller
    {
        private dbEntities db = new dbEntities();



        // GET: Admin/Children
        public ActionResult Index()
        {
            var children = db.Children.Include(c => c.CourseFrequency).Include(c => c.Group)./*Include(c => c.School).*/Include(c => c.YearGroup).OrderByDescending(x => x.ChildId);

            var groups = db.Groups.OrderBy(p => p.Inactive).Select(x => new { GroupId = x.GroupId, GroupName = x.GroupName + (x.Inactive ? "(Inactive)" : "") }).ToList();
            groups.Insert(0, new { GroupId = -1, GroupName = "No Group" });

            ViewBag.Groups = new SelectList(groups, "GroupId", "GroupName");
            ViewBag.YearGroup = new SelectList(db.YearGroups, "YearGroupId", "GroupName");
            var inactiveList = new List<SelectListItem>();
            inactiveList.Add(new SelectListItem { Value = "0", Text = "All" });
            inactiveList.Add(new SelectListItem { Value = "1", Text = "Active only", Selected = true });
            inactiveList.Add(new SelectListItem { Value = "2", Text = "Inactive only" });
            ViewBag.InactiveList = new SelectList(inactiveList, "Value", "Text", 1);
            return View(children.Where(x => !x.Inactive).ToPagedList(1, 20));
        }
        public ActionResult GetFilteredList()
        {
            var id = Request.Form["filterById"];
            var name = Request.Form["filterFullName"];
            var group = Request.Form["Groups"];
            var yearGroup = Request.Form["YearGroup"];
            var page = Request.Form["page"];
            var InactiveList = Request.Form["InactiveList"];

            var children = db.Children.Include(c => c.CourseFrequency).Include(c => c.Group)./*Include(c => c.School).*/Include(c => c.YearGroup);
            if (InactiveList.Equals("1"))
            {
                children = children.Where(x => !x.Inactive);
            }
            if (InactiveList.Equals("2"))
            {
                children = children.Where(x => x.Inactive);
            }
            if (!string.IsNullOrEmpty(id))
            {
                children = children.Where(x => x.ChildId.ToString().Contains(id));
            }
            if (!string.IsNullOrEmpty(name))
            {
                children = children.Where(x => x.FullName.Contains(name));
            }
            if (!string.IsNullOrEmpty(group))
            {
                var iGroup = Convert.ToInt32(group);
                if (iGroup == -1)
                {
                    children = children.Where(x => x.GroupId == null || x.GroupId == 0);
                }
                else
                {
                    children = children.Where(x => x.GroupId == iGroup);
                }
            }
            if (!string.IsNullOrEmpty(yearGroup))
            {
                var iYearGroup = Convert.ToInt32(yearGroup);
                children = children.Where(x => x.YearId == iYearGroup);
            }
            var pageNo = Convert.ToInt32(page);
            pageNo = pageNo == 0 ? 1 : pageNo;
            return PartialView("ChildrenList", children.OrderByDescending(x => x.ChildId).ToPagedList(pageNo, 20));
        }
        // GET: Admin/Children/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Child child = db.Children.Find(id);
            if (child == null)
            {
                return HttpNotFound();
            }
            ViewBag.FreeHours = db.ChildDayHours(id);
            return View(child);
        }

        // GET: Admin/Children/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Child child = db.Children.Find(id);
            if (child == null)
            {
                return HttpNotFound();
            }


            ViewBag.FreqId = new SelectList(db.CourseFrequencies.Where(x => !x.Inactive).OrderBy(x => x.FrequencyDescription), "FreqId", "FrequencyDescription", child.FreqId);
            ViewBag.GroupId = new SelectList(db.Groups.Where(x => !x.Inactive).OrderBy(x => x.GroupName), "GroupId", "GroupName", child.GroupId);
            //ViewBag.SchoolId = new SelectList(db.Schools.OrderBy(x => x.SchoolName), "SchoolId", "SchoolName", child.SchoolId);
            ViewBag.YearId = new SelectList(db.YearGroups.OrderBy(x => x.From), "YearGroupId", "GroupName", child.YearId);
            ViewBag.YearGroups = db.YearGroups.OrderBy(x => x.From);

            ViewBag.For4_6FreqID = db.YearGroups.Where(x => x.From == 4).First().YearGroupDays.FirstOrDefault().Day;

            return View(child);
        }

        // POST: Admin/Children/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ChildId,FullName,BirthDateString,YearId,SchoolId,ClassNo,FreqId,GroupId,EduLevel,Inactive,Comment")] Child child)
        {
            if (ModelState.IsValid)
            {
                var dbChild = db.Children.Find(child.ChildId);
                dbChild.FullName = child.FullName;
                dbChild.BirthDate = child.BirthDate;
                dbChild.YearId = child.YearId;
                dbChild.FreqId = child.FreqId;
                dbChild.GroupId = child.GroupId;
                dbChild.SchoolId = child.SchoolId;
                dbChild.ClassNo = child.ClassNo;
                dbChild.EduLevel = child.EduLevel;
                dbChild.Inactive = child.Inactive;
                dbChild.Comment = child.Comment;
                db.Entry(dbChild).State = EntityState.Modified;
                db.SaveChanges();

                // insert dates and times according to group
                AddTimesFromGroup(child);

                return RedirectToAction("Index");
            }
            ViewBag.FreqId = new SelectList(db.CourseFrequencies.Where(x => !x.Inactive).OrderBy(x => x.FrequencyDescription), "FreqId", "FrequencyDescription", child.FreqId);
            ViewBag.GroupId = new SelectList(db.Groups.Where(x => !x.Inactive).OrderBy(x => x.GroupName), "GroupId", "GroupName", child.GroupId);
            //ViewBag.SchoolId = new SelectList(db.Schools.OrderBy(x => x.SchoolName), "SchoolId", "SchoolName", child.SchoolId);
            ViewBag.YearId = new SelectList(db.YearGroups.OrderBy(x => x.From), "YearGroupId", "GroupName", child.YearId);
            ViewBag.YearGroups = db.YearGroups.OrderBy(x => x.From);

            ViewBag.For4_6FreqID = db.YearGroups.Where(x => x.From == 4).First().YearGroupDays.FirstOrDefault().Day;
            return View(child);
        }

        //// GET: Admin/Children/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Child child = db.Children.Find(id);
        //    if (child == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(child);
        //}

        //// POST: Admin/Children/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Child child = db.Children.Find(id);
        //    db.Children.Remove(child);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult GetFullName()
        {
            var filter = Convert.ToString(Request.QueryString[0]);
            if (string.IsNullOrEmpty(filter))
            {
                return Json(db.Children.GroupBy(p => p.FullName).Select(p => new { FullName = p.FirstOrDefault().FullName }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(db.Children.Where(p => p.FullName.ToLower().Contains(filter.ToLower())).GroupBy(p => p.FullName).Select(p => new { FullName = p.FirstOrDefault().FullName }), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetID()
        {
            var filter = Convert.ToString(Request.QueryString[0]);
            if (string.IsNullOrEmpty(filter))
            {
                return Json(db.Children.Select(p => new { ID = p.ChildId }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(db.Children
                    .Where(p => p.ChildId.ToString().ToLower().Contains(filter.ToLower()))
                    .Select(p => new { ID = p.ChildId }),
                    JsonRequestBehavior.AllowGet);
            }
        }

        private void AddTimesFromGroup(Child child)
        {
            // nothing to do if groupId is not defined
            if (child.GroupId == null || child.GroupId < 1) return;

            /// inline checker
            Func<int?, bool> IsOk = d => d != null && d > 0;

            var group = db.Groups.Find(child.GroupId);
            if (group != null)
            {
                // first remove all records related to the child
                db.FreeHours.RemoveRange(db.FreeHours.Where(p => p.ChildId == child.ChildId));

                // now check each day and insert it if needed...
                //if (IsOk(group.Sunday))
                //{
                //    var freeHour = new FreeHour { ChildId = child.ChildId, HourId = (int)group.Sunday };
                //    db.Entry(freeHour).State = EntityState.Added;
                //}
                //if (IsOk(group.Monday))
                //{
                //    var freeHour = new FreeHour { ChildId = child.ChildId, HourId = (int)group.Monday };
                //    db.Entry(freeHour).State = EntityState.Added;
                //}
                //if (IsOk(group.Tuesday))
                //{
                //    var freeHour = new FreeHour { ChildId = child.ChildId, HourId = (int)group.Tuesday };
                //    db.Entry(freeHour).State = EntityState.Added;
                //}
                //if (IsOk(group.Wednesday))
                //{
                //    var freeHour = new FreeHour { ChildId = child.ChildId, HourId = (int)group.Wednesday };
                //    db.Entry(freeHour).State = EntityState.Added;
                //}
                //if (IsOk(group.Thursday))
                //{
                //    var freeHour = new FreeHour { ChildId = child.ChildId, HourId = (int)group.Thursday };
                //    db.Entry(freeHour).State = EntityState.Added;
                //}
                //if (IsOk(group.Friday))
                //{
                //    var freeHour = new FreeHour { ChildId = child.ChildId, HourId = (int)group.Friday };
                //    db.Entry(freeHour).State = EntityState.Added;
                //}
                //if (IsOk(group.Saturday))
                //{
                //    var freeHour = new FreeHour { ChildId = child.ChildId, HourId = (int)group.Saturday };
                //    db.Entry(freeHour).State = EntityState.Added;
                //}
                db.SaveChanges();
            }
        }
    }
}
