using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CetCources.Database;
using CetCources.Extensions;
using Microsoft.AspNet.Identity;
using CetCources.Models;
using System.Threading;
using Resources;
using System.Threading.Tasks;

namespace CetCources.Controllers
{
    [Authorize]
    public class ChildController : Controller
    {
        private dbEntities db = new dbEntities();

        private const string ChildId = "_Sesstion_CHILDID";
        public const string ParentId = "_Sesstion_PARENTID";

        // GET: Child
        public ActionResult Index(string userid)
        {
            var userId = User.Identity.GetUserId();

            if (User.IsInRole("Admin") && !string.IsNullOrEmpty(userid))
            {
                ViewBag.UserId = userid;
                userId = userid;
            }
            
            var children = db.Children.Where(p => p.ParentId == userId).Include(c => c.CourseFrequency).Include(c => c.Group)./*Include(c => c.School).*/Include(c => c.YearGroup).OrderBy(x=>x.Inactive).ThenByDescending(x => x.ChildId);
            return View(children.ToList());
        }

        // GET: Child/Details/5
        public ActionResult Details(int? id, string UserId)
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
            Session[ParentId] = UserId;
            return View(child);
        }

        // GET: Child/Create
        [HttpGet]
        public ActionResult Do(int? id, string UserId)
        {
            Child child = null;
            if (id != null && id > 0)
            {
                child = db.Children.Find(id);// this may set the child object to null again
            }

            SetViewBagData(child, UserId);

            return View(child);
        }

        /// <summary>
        /// Post Create
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Do([Bind(Include = "ChildId,UserId,FullName,BirthDateString,YearId,SchoolId,ClassNo,FreqId,GroupId,EduLevel,Inactive,Comment")] Child child)
        {
            var bGroupChanged = false;
            if (!string.IsNullOrEmpty(child.UserId) && User.IsInRole("Admin"))
            {
                var d = db.AspNetUsers.Find(child.UserId);
                if (d == null)
                {
                    ModelState.AddModelError("UserId", "User Not found");
                }
            }
            if (ModelState.IsValid)
            {
                var yearid = db.YearGroups.Where(x => x.From == 4).First().YearGroupId;
                if (child.ChildId == 0) // adding new
                {
                    child.ParentId = User.IsInRole("Admin") ? (string.IsNullOrEmpty(child.UserId) ? User.Identity.GetUserId() : child.UserId) : User.Identity.GetUserId();
                    child.CreationDate = DateTime.Now;
                    child.EduLevel = User.IsInRole("Admin") ? child.EduLevel : 1;
                    child.FreqId = child.FreqId == 0 ? (child.YearId == yearid ? child.FreqId : null) : child.FreqId;

                    bGroupChanged = child.GroupId != 0;

                    child.GroupId = child.GroupId == 0 ? null : child.GroupId;
                    db.Children.Add(child);
                    db.SaveChanges();
                    Session[ChildId] = child.ChildId;
                    if (User.IsInRole("Admin"))
                    {
                        Session[ParentId] = child.ParentId;
                    }
                    await Mail.Send(Mails.ChildAdded, string.Format(Mails.ChildAddedBody, child.AspNetUser.FullName, child.FullName), child.AspNetUser.Email, child.AspNetUser.FullName);
                }
                else
                {
                    var dbChild = db.Children.Find(child.ChildId);
                    dbChild.FullName = child.FullName;
                    dbChild.BirthDate = child.BirthDate;
                    dbChild.YearId = child.YearId;
                    dbChild.FreqId = child.FreqId == 0 ? (child.YearId == yearid ? child.FreqId : null) : child.FreqId;

                    bGroupChanged = dbChild.GroupId != child.GroupId;

                    dbChild.GroupId = child.GroupId == 0 ? null : child.GroupId;
                    dbChild.SchoolId = child.SchoolId;
                    dbChild.ClassNo = child.ClassNo;
                    dbChild.Inactive = child.Inactive;
                    dbChild.Comment = child.Comment;
                    if (User.IsInRole("Admin"))
                    {
                        dbChild.EduLevel = child.EduLevel;
                    }
                    //child.ParentId = User.Identity.GetUserId();

                    db.Entry(dbChild).State = EntityState.Modified;
                    db.SaveChanges();
                    Session[ChildId] = child.ChildId;
                    if (User.IsInRole("Admin"))
                    {
                        Session[ParentId] = dbChild.ParentId;
                    }
                }

                // insert dates and times according to group
                AddTimesFromGroup(db, child);

                if (child.Inactive || (child.GroupId != null && child.GroupId > 0))
                {
                    if (child.GroupId > 0 && bGroupChanged)
                    {
                        child = db.Children.Find(child.ChildId);
                        if (child.Group.PersonCount <= child.Group.Children.Count)
                        {
                            //await Mail.Send($"Group {child.Group.GroupName} is full", $"The group <b>{child.Group.GroupName}</b> to which <b>{child.FullName}</b> joined is full and is ready to start", child.AspNetUser.Email, child.AspNetUser.FullName);
                            await Mail.Send(string.Format(Mails.GroupIsFull, child.Group.GroupName),
                                  string.Format(Mails.GroupIsFullBody, child.AspNetUser.FullName, child.Group.GroupName, child.FullName),
                                  child.AspNetUser.Email, child.AspNetUser.FullName);
                        }
                    }

                    return RedirectToAction("Index", string.IsNullOrEmpty(Session[ParentId] as string) ? new { userid = Session[ParentId] } : null);
                }
                else
                {
                    return RedirectToAction("Groups");
                }
            }

            //ViewBag.FreqId = new SelectList(db.CourseFrequencies.Where(x => !x.Inactive).OrderBy(x => x.FrequencyDescription), "FreqId", "FrequencyDescription", child.FreqId);
            //ViewBag.GroupId = new SelectList(db.Groups.Where(x => !x.Inactive && x.PersonCount > x.Children.Count).OrderBy(x => x.GroupName), "GroupId", "GroupName", child.GroupId);
            //ViewBag.SchoolId = new SelectList(db.Schools.OrderBy(x => x.SchoolName), "SchoolId", "SchoolName", child.SchoolId);
            //ViewBag.YearId = new SelectList(db.YearGroups.OrderBy(x => x.From), "YearGroupId", "GroupName", child.YearId);
            //ViewBag.YearGroups = db.YearGroups.OrderBy(x => x.From);

            SetViewBagData(child, child.UserId);

            return View(child);
        }

        public ActionResult GetGroupList(int? YearId, int? FreqId, int? ChildId)
        {
            var childGroupId = 0;
            var list = db.Groups.Where(x => !x.Inactive);

            if (YearId.HasValue && YearId > 0)
            {
                list = list.Where(x => x.YearId == YearId);
            }
            //if (FreqId.HasValue && FreqId > 0)
            //{
            //    list = list.Where(x => x.FreqId == FreqId);
            //}

            if (ChildId.HasValue && ChildId > 0)
            {
                var child = db.Children.Find(ChildId);
                if (child != null)
                {
                    list = list.Where(x => x.EduLevel == child.EduLevel);
                    childGroupId = child.GroupId ?? 0;
                }
            }
            var listed = list.ToList();
            if (childGroupId > 0 && !list.Any(x => x.GroupId == childGroupId))
            {
                var grp = db.Groups.Find(childGroupId);
                if (grp != null)
                {
                    listed.Add(grp);
                }
            }
            var lst = listed.Select(x => new { x.GroupId, x.GroupName }).OrderBy(x => x.GroupName).ToList();
            //   if (lst.Count > 0)
            //   {
            lst.Insert(0, new { GroupId = 0, GroupName = CommonRes.PleaseSelect });
            //    }
            return Json(lst, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Called from js when Year Group ddl is changed
        /// </summary>
        /// <param name="fromYear">depending on this param returns only one day record or</param>
        /// <returns></returns>
        public ActionResult GetFreqList(int? fromYear)
        {
            // take the only one record corresponding to the day from db
            // kind of hard coded data 
            // see CourseFrequency table, first 7 records // they are not editable/visible from admin part
            // we consider /this is hard coded part/!!!! 
            // that the group of 4-6 years old kids can have only one active day
            if (fromYear == 4)
            {
                //var days = db.YearGroups.Where(x => x.From == 4).Include(x => x.YearGroupDays).ToList();
                //var theDay = days.First().YearGroupDays.First().Day;
                //var days = db.YearGroups.Where(x => x.YearGroupId == child.YearId)
                //    .Include(x => x.YearGroupDays).ToList();
                var theDay = db.YearGroups.Where(x => x.From == 4).Select(x => x.YearGroupDays.FirstOrDefault().Day).First();
                var list = db.CourseFrequencies.Where(x => x.FreqId == theDay).Select(x => new { x.FreqId, x.FrequencyDescription });

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else // take all records besides hidden first 7
            {
                var list = db.CourseFrequencies.Where(x => !x.Inactive && x.FreqId > 6)
                    .Select(x => new { x.FreqId, x.FrequencyDescription }).OrderBy(x => x.FrequencyDescription).ToList();
                if (list.Count > 0)
                {
                    list.Insert(0, new { FreqId = 0, FrequencyDescription = CommonRes.PleaseSelect });
                }

                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult Groups()
        {
            var id = Session[ChildId];
            if (id == null)
            {
                return RedirectToAction("Index", string.IsNullOrEmpty(Session[ParentId] as string) ? new { userid = Session[ParentId] } : null);
            }
            Child child = db.Children.Find(id);
            if (child == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(child);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Groups([Bind(Include = "GroupId")]Child childGrp)
        {
            if (childGrp == null || childGrp.GroupId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //var group = db.Groups.Where(x => x.GroupId == childGrp.GroupId && !x.Inactive).First();
            var group = db.Groups.Find(childGrp.GroupId);
            if (group == null || group.Inactive)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var childId = Session[ChildId];
            if (childId == null)
            {
                return RedirectToAction("Index", string.IsNullOrEmpty(Session[ParentId] as string) ? new { userid = Session[ParentId] } : null);
            }
            var child = db.Children.Find(childId);
            if (child == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // change group Id
            child.GroupId = group.GroupId;
            child.FreqId = group.FreqId;
            db.Entry(child).State = EntityState.Modified;
            db.SaveChanges();

            // insert dates and times according to group
            AddTimesFromGroup(db, child);

            child = db.Children.Find(child.ChildId);
            if (child.Group.PersonCount <= child.Group.Children.Count)
            {
                //await Mail.Send($"Group {child.Group.GroupName} is full", $"The group <b>{child.Group.GroupName}</b> to which <b>{child.FullName}</b> joined is full and is ready to start", child.AspNetUser.Email, child.AspNetUser.FullName);
                await Mail.Send(string.Format(Mails.GroupIsFull, child.Group.GroupName),
                                  string.Format(Mails.GroupIsFullBody, child.AspNetUser.FullName, child.Group.GroupName, child.FullName),
                                  child.AspNetUser.Email, child.AspNetUser.FullName);
            }

            return RedirectToAction("Index", string.IsNullOrEmpty(Session[ParentId] as string) ? new { userid = Session[ParentId] } : null);
        }

        /// <summary>
        /// Groups' ListView(second tab) dataSource action
        /// </summary>
        /// <returns></returns>
        public JsonResult GroupListData()
        {
            var isEnglish = Thread.CurrentThread.CurrentUICulture.LCID == 1033;
            var childId = Session[ChildId];
            if (childId == null)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            var child1 = db.Children.Find(childId);
            var child = db.Children.Find(childId);

            //var groups = db.Groups.Where(x=>x.YearId == child.YearId && !x.Inactive)//.Join(db.Children, cc=>cc.GroupId, gg=>gg.GroupId, (c1, g1)=> new {c1. })
            //.GroupBy(gr => gr.Children.)

            var groups = db.Groups.Where(x => x.YearId == child.YearId
                                        //&& x.FreqId == child.FreqId 
                                        && !x.Inactive && x.EduLevel == child.EduLevel)
              .Include(g => g.CourseFrequency)
              .Include(g => g.HourShifts_Sun)
              .Include(g => g.HourShifts_Mon)
              .Include(g => g.HourShifts_Tue)
              .Include(g => g.HourShifts_Wed)
              .Include(g => g.HourShifts_Thu)
              .Include(g => g.HourShifts_Fri)
              .Include(g => g.HourShifts_Sat)
              .Include(g => g.Children);
            var data = from g in groups
                       select new
                       {
                           GroupName = g.GroupName,
                           FrequencyDescription = g.CourseFrequency.FrequencyDescription,
                           Sunday = g.HourShifts_Sun.HourShift,
                           Monday = g.HourShifts_Mon.HourShift,
                           Tuesday = g.HourShifts_Tue.HourShift,
                           Wednesday = g.HourShifts_Wed.HourShift,
                           Thursday = g.HourShifts_Thu.HourShift,
                           Friday = g.HourShifts_Fri.HourShift,
                           Saturday = g.HourShifts_Sat.HourShift,
                           FillCounts = isEnglish ? g.Children.Count + " of " + g.PersonCount :
                           g.Children.Count + "/" + g.PersonCount + "-ից",
                           GroupId = g.GroupId,
                           ChildCount = g.Children.Count,
                           PersonCount = g.PersonCount
                       };

            return Json(data.ToList(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GroupFilteredListData()
        {
            var isEnglish = Thread.CurrentThread.CurrentUICulture.LCID == 1033;
            var childId = Session[ChildId];
            if (childId == null)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            var child = db.Children.Find(childId);

            var groups = db.Groups.Where(x => x.YearId == child.YearId
                                        //&& x.FreqId == child.FreqId 
                                        && !x.Inactive && x.EduLevel == child.EduLevel)
                .Include(g => g.CourseFrequency)
                .Include(g => g.HourShifts_Sun)
                .Include(g => g.HourShifts_Mon)
                .Include(g => g.HourShifts_Tue)
                .Include(g => g.HourShifts_Wed)
                .Include(g => g.HourShifts_Thu)
                .Include(g => g.HourShifts_Fri)
                .Include(g => g.HourShifts_Sat)
                .Include(g => g.Children);

            var data = from g in groups
                       select new
                       {
                           GroupName = g.GroupName,
                           FrequencyDescription = g.CourseFrequency.FrequencyDescription,
                           Sunday = g.HourShifts_Sun.HourShift,
                           Monday = g.HourShifts_Mon.HourShift,
                           Tuesday = g.HourShifts_Tue.HourShift,
                           Wednesday = g.HourShifts_Wed.HourShift,
                           Thursday = g.HourShifts_Thu.HourShift,
                           Friday = g.HourShifts_Fri.HourShift,
                           Saturday = g.HourShifts_Sat.HourShift,
                           FillCounts = isEnglish ? g.Children.Count + " of " + g.PersonCount :
                           g.Children.Count + "/" + g.PersonCount + "-ից",
                           GroupId = g.GroupId,
                           ChildCount = g.Children.Count,
                           PersonCount = g.PersonCount,

                           SundayId = g.Sunday,
                           MondayId = g.Monday,
                           TuesdayId = g.Tuesday,
                           WednesdayId = g.Wednesday,
                           ThursdayId = g.Thursday,
                           FridayId = g.Friday,
                           SaturdayId = g.Saturday,
                           DaysCount = g.CourseFrequency.DaysCount,
                       };

            var groupList = data.ToList();
            var childFreeHours = db.FreeHours.Where(x => x.ChildId == child.ChildId).Include(x => x.DayHours);
            foreach (var group in data)
            {
                if (group.ChildCount >= group.PersonCount) continue;
                var count = group.DaysCount;
                var matchedCount = 0;

                Action<IEnumerable<FreeHour>, HourShifts> fnc = (free, shift) =>
                {
                    var found = free.Any(x => x.DayHours.BaseHourId == shift.BaseHourId);
                    if (found && shift.BaseHourId != shift.StartHourShifted)
                    {
                        if (!free.Any(x => x.DayHours.BaseHourId == shift.BaseHourId + 1))
                        {
                            found = false;
                        }
                    }
                    if (found)
                    {
                        matchedCount++;
                    }
                };


                if (group.Sunday != null)
                {
                    //matchedCount += childFreeHours
                    //    .Any(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Sunday && x.DayHours.BaseHourId == group.SundayId) ? 1 : 0;
                    fnc(db.FreeHours
                        .Where(x => x.ChildId == child.ChildId && x.DayHours.DayOfWeek == (int)System.DayOfWeek.Sunday),
                        db.HourShifts.Find(group.SundayId));
                }

                if (group.Monday != null)
                {
                    fnc(db.FreeHours
                        .Where(x => x.ChildId == child.ChildId && x.DayHours.DayOfWeek == (int)System.DayOfWeek.Monday),
                        db.HourShifts.Find(group.MondayId));
                }
                if (group.Tuesday != null)
                {
                    fnc(db.FreeHours
                         .Where(x => x.ChildId == child.ChildId && x.DayHours.DayOfWeek == (int)System.DayOfWeek.Tuesday),
                         db.HourShifts.Find(group.TuesdayId));
                }
                if (group.Wednesday != null)
                {
                    fnc(db.FreeHours
                            .Where(x => x.ChildId == child.ChildId && x.DayHours.DayOfWeek == (int)System.DayOfWeek.Wednesday),
                            db.HourShifts.Find(group.WednesdayId));
                }
                if (group.Thursday != null)
                {
                    fnc(db.FreeHours
                           .Where(x => x.ChildId == child.ChildId && x.DayHours.DayOfWeek == (int)System.DayOfWeek.Thursday),
                           db.HourShifts.Find(group.ThursdayId));
                }
                if (group.Friday != null)
                {
                    fnc(db.FreeHours
                       .Where(x => x.ChildId == child.ChildId && x.DayHours.DayOfWeek == (int)System.DayOfWeek.Friday),
                       db.HourShifts.Find(group.FridayId));
                }
                if (group.Saturday != null)
                {
                    fnc(db.FreeHours
                     .Where(x => x.ChildId == child.ChildId && x.DayHours.DayOfWeek == (int)System.DayOfWeek.Saturday),
                     db.HourShifts.Find(group.SaturdayId));
                }
                if (count != matchedCount)
                {
                    groupList.Remove(group);
                }
            }

            return Json(groupList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Times(int? id)
        {
            if (Session[ChildId] == null)
            {
                return RedirectToAction("Index", string.IsNullOrEmpty(Session[ParentId] as string) ? new { userid = Session[ParentId] } : null);
            }
            // get childId from session
            var childId = Convert.ToInt32(Session[ChildId]);

            //var activeDays = from c in db.Children.Where(x => x.ChildId == childId)
            //                 join yg in db.YearGroups on c.YearId equals yg.YearGroupId
            //                 join ygd in db.YearGroupDays on yg.YearGroupId equals ygd.YearGroupId
            //                 select ygd.Day;
            if (id == 0)
            {
                ViewBag.NoGroup = true;
                ViewBag.ChildId = Session[ChildId];
            }
            else
            {
                ViewBag.NoGroup = false;
            }
            var days = db.ChildDayHours(childId);
            return View(days);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Times(FormCollection form)
        {
            // check if the session still exists
            if (Session[ChildId] == null)
            {
                return RedirectToAction("Index", string.IsNullOrEmpty(Session[ParentId] as string) ? new { userid = Session[ParentId] } : null);
            }
            // get childId from session
            var childId = Convert.ToInt32(Session[ChildId]);

            var id = Convert.ToInt32(form["id"]);
            // if form state is invalid, return back to form
            if (!ModelState.IsValid)
            {
                if (id == 0)
                {
                    ViewBag.NoGroup = true;
                    ViewBag.ChildId = Session[ChildId];
                }
                else
                {
                    ViewBag.NoGroup = false;
                }
                return View();
            }

            // first remove all records related to the child
            db.FreeHours.RemoveRange(db.FreeHours.Where(p => p.ChildId == childId));

            // then loop throw the form element find checked hours and insert them
            var somethingFound = false;
            foreach (var key in Request.Form.AllKeys)
            {
                try
                {
                    var value = Request.Form.GetValues(key);
                    if (key.Contains("chk_") && value.Length > 1)
                    {
                        var keySplitted = key.Split('_');
                        if (keySplitted.Length == 3)
                        {

                            var hourId = Convert.ToInt32(keySplitted[2]);
                            var freeHour = new FreeHour { ChildId = childId, DayHourId = hourId };
                            db.Entry(freeHour).State = EntityState.Added;
                            somethingFound = true;

                        }
                    }
                }
                catch { }
            }

            if (somethingFound)
            {
                db.SaveChanges();

                var child = db.Children.Find(childId);
                var yearId = child.YearId;
                var freqId = child.FreqId;

                var groups = db.Groups.Where(x => x.YearId == yearId && !x.Inactive)
                        .Include(g => g.HourShifts_Sun)
                        .Include(g => g.HourShifts_Mon)
                        .Include(g => g.HourShifts_Tue)
                        .Include(g => g.HourShifts_Wed)
                        .Include(g => g.HourShifts_Thu)
                        .Include(g => g.HourShifts_Fri)
                        .Include(g => g.HourShifts_Sat);
                var childFreeHours = db.FreeHours.Where(x => x.ChildId == childId).Include(x => x.DayHours);
                var groupList = new List<Group>();
                foreach (var group in groups)
                {
                    if (group.PersonCount <= group.Children.Count()) continue;
                    var count = group.CourseFrequency.DaysCount;
                    var matchedCount = 0;
                    //if (group.Hour_Sunday != null)
                    //{
                    //    matchedCount += childFreeHours
                    //        .Any(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Sunday && x.DayHours.BaseHourId == group.Hour_Sunday.BaseHourId) ? 1 : 0;
                    //}
                    Action<IQueryable<FreeHour>, HourShifts> fnc = (free, shift) =>
                    {
                        var found = free.Any(x => x.DayHours.BaseHourId == shift.BaseHourId);
                        if (found && shift.BaseHourId != shift.StartHourShifted)
                        {
                            if (!free.Any(x => x.DayHours.BaseHourId == shift.BaseHourId + 1))
                            {
                                found = false;
                            }
                        }
                        if (found)
                            matchedCount++;
                    };

                    if (group.Sunday != null)
                    {
                        fnc(childFreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Sunday),
                            db.HourShifts.Find(group.Sunday));
                    }
                    if (group.Monday != null)
                    {
                        fnc(childFreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Monday),
                            db.HourShifts.Find(group.Monday));
                    }
                    if (group.Tuesday != null)
                    {
                        fnc(childFreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Tuesday),
                            db.HourShifts.Find(group.Tuesday));
                    }
                    if (group.Wednesday != null)
                    {
                        fnc(childFreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Wednesday),
                            db.HourShifts.Find(group.Wednesday));
                    }
                    if (group.Thursday != null)
                    {
                        fnc(childFreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Thursday),
                            db.HourShifts.Find(group.Thursday));
                    }
                    if (group.Friday != null)
                    {
                        fnc(childFreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Friday),
                            db.HourShifts.Find(group.Friday));
                    }
                    if (group.Saturday != null)
                    {
                        fnc(childFreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Saturday),
                            db.HourShifts.Find(group.Saturday));
                    }

                    if (count == matchedCount)
                    {
                        groupList.Add(group);
                    }
                }

                if (groupList.Count > 0)
                {
                    return RedirectToAction("SuggestedGroups");
                }

                return RedirectToAction("Index", !string.IsNullOrEmpty(Session[ParentId] as string) ? new { userid = Session[ParentId] } : null);
            }

            var days = db.ChildDayHours(childId);
            if (id == 0)
            {
                ViewBag.NoGroup = true;
                ViewBag.ChildId = Session[ChildId];
            }
            else
            {
                ViewBag.NoGroup = false;
            }
            return View(days);
        }

        [HttpGet]
        public ActionResult SuggestedGroups()
        {
            if (Session[ChildId] == null)
            {
                return RedirectToAction("Index", string.IsNullOrEmpty(Session[ParentId] as string) ? new { userid = Session[ParentId] } : null);
            }
            var child = db.Children.Find(Session[ChildId]);
            return View(child);
        }

        public JsonResult GetParentList()
        {
            var filter = Convert.ToString(Request.QueryString[0]);
            if (string.IsNullOrEmpty(filter))
            {
                return Json(db.AspNetUsers.OrderBy(p => p.Email).Select(p => new { Name = p.Email+" - "+ p.FullName, Id = p.Id }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                filter = filter.ToLower();
                return Json(db.AspNetUsers.Where(x=>x.Email.ToLower().Contains(filter) || x.FullName.ToLower().Contains(filter)).OrderBy(p => p.Email).Select(p => new { Name = p.Email + " - " + p.FullName, Id = p.Id }), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Delete(int? id, string UserId)
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
            Session[ParentId] = UserId;
            return View(child);
        }

        // POST: Child/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string UserId)
        {
            Child child = db.Children.Find(id);
            db.Children.Remove(child);
            db.SaveChanges();
            return RedirectToAction("Index", new { userid = UserId });
        }

        //public ActionResult DayHours(int day)
        //{
        //    // check if the session still exists
        //    if (Session[ChildId] == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.RequestTimeout);
        //    }
        //    // get childId from session
        //    var childId = Convert.ToInt32(Session[ChildId]);
        //    var theDay=db.DayOfWeeks.Where(p => p.TheDay == day).Take(1).First().Day;
        //    ViewBag.DayOfWeek = theDay;
        //    var dayProp = typeof(Resources.CommonRes).GetProperty(theDay);
        //    if (dayProp != null)
        //    {
        //        ViewBag.DayOfWeek = dayProp.GetValue(null);
        //    }
        //    //var model = (from h in db.Hours
        //    //             where h.DayOfWeek == day
        //    //             group h by h.HourId into gh
        //    //             join fh in db.FreeHours.Where(p=>p.ChildId == childId) 
        //    //             on gh.FirstOrDefault().HourId equals fh.HourId
        //    //             into hj
        //    //             from fhh in hj.DefaultIfEmpty()
        //    //             orderby gh.FirstOrDefault().Hours
        //    //             select new DayHoursModel
        //    //             {
        //    //                 DayOfWeek = gh.FirstOrDefault().DayOfWeek,
        //    //                 HourId = gh.FirstOrDefault().HourId,
        //    //                 Hours = gh.FirstOrDefault().Hours,
        //    //                 IsChecked = fhh != null
        //    //             }).ToList();
        //    // build correct data
        //    var model = db.DayHours.Where(x => x.DayOfWeek == day && !x.Inactive)
        //        .Include(x=>x.FreeHours);
        //    return PartialView(model);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public static void AddTimesFromGroup(dbEntities db, Child child)
        {
            // nothing to do if groupId is not defined
            if (child.GroupId == null || child.GroupId < 1) return;

            /// inline checker
            Func<int?, bool> IsOk = d => d != null && d > 0;

            var group = db.Groups.Find(child.GroupId);
            if (group != null)
            {
                ////// first remove all records related to the child
                ////db.FreeHours.RemoveRange(db.FreeHours.Where(p => p.ChildId == child.ChildId));

                // now check each day and insert it if needed...
                if (IsOk(group.Sunday))
                {
                    var freeHour = new FreeHour
                    {
                        ChildId = child.ChildId,
                        DayHourId = (int)group.HourShifts_Sun.BaseHours.DayHours.Where(x =>
                                x.DayOfWeek == (int)System.DayOfWeek.Sunday).First().DayHourId
                    };
                    if (!db.FreeHours.Any(x => x.ChildId == freeHour.ChildId && x.DayHourId == freeHour.DayHourId))
                        db.Entry(freeHour).State = EntityState.Added;
                }
                if (IsOk(group.Monday))
                {
                    var freeHour = new FreeHour
                    {
                        ChildId = child.ChildId,
                        DayHourId = (int)group.HourShifts_Mon.BaseHours.DayHours.Where(x =>
                        x.DayOfWeek == (int)System.DayOfWeek.Monday).First().DayHourId
                    };
                    if (!db.FreeHours.Any(x => x.ChildId == freeHour.ChildId && x.DayHourId == freeHour.DayHourId))
                        db.Entry(freeHour).State = EntityState.Added;
                }
                if (IsOk(group.Tuesday))
                {
                    var freeHour = new FreeHour
                    {
                        ChildId = child.ChildId,
                        DayHourId = (int)group.HourShifts_Tue.BaseHours.DayHours.Where(x =>
                        x.DayOfWeek == (int)System.DayOfWeek.Tuesday).First().DayHourId
                    };
                    if (!db.FreeHours.Any(x => x.ChildId == freeHour.ChildId && x.DayHourId == freeHour.DayHourId))
                        db.Entry(freeHour).State = EntityState.Added;
                }
                if (IsOk(group.Wednesday))
                {
                    var freeHour = new FreeHour
                    {
                        ChildId = child.ChildId,
                        DayHourId = (int)group.HourShifts_Wed.BaseHours.DayHours.Where(x =>
                        x.DayOfWeek == (int)System.DayOfWeek.Wednesday).First().DayHourId
                    };
                    if (!db.FreeHours.Any(x => x.ChildId == freeHour.ChildId && x.DayHourId == freeHour.DayHourId))
                        db.Entry(freeHour).State = EntityState.Added;
                }
                if (IsOk(group.Thursday))
                {
                    var freeHour = new FreeHour
                    {
                        ChildId = child.ChildId,
                        DayHourId = (int)group.HourShifts_Thu.BaseHours.DayHours.Where(x =>
                        x.DayOfWeek == (int)System.DayOfWeek.Thursday).First().DayHourId
                    };
                    if (!db.FreeHours.Any(x => x.ChildId == freeHour.ChildId && x.DayHourId == freeHour.DayHourId))
                        db.Entry(freeHour).State = EntityState.Added;
                }
                if (IsOk(group.Friday))
                {
                    var freeHour = new FreeHour
                    {
                        ChildId = child.ChildId,
                        DayHourId = (int)group.HourShifts_Fri.BaseHours.DayHours.Where(x =>
                        x.DayOfWeek == (int)System.DayOfWeek.Friday).First().DayHourId
                    };
                    if (!db.FreeHours.Any(x => x.ChildId == freeHour.ChildId && x.DayHourId == freeHour.DayHourId))
                        db.Entry(freeHour).State = EntityState.Added;
                }
                if (IsOk(group.Saturday))
                {
                    var freeHour = new FreeHour
                    {
                        ChildId = child.ChildId,
                        DayHourId = (int)group.HourShifts_Sat.BaseHours.DayHours.Where(x =>
                        x.DayOfWeek == (int)System.DayOfWeek.Saturday).First().DayHourId
                    };
                    if (!db.FreeHours.Any(x => x.ChildId == freeHour.ChildId && x.DayHourId == freeHour.DayHourId))
                        db.Entry(freeHour).State = EntityState.Added;
                }
                db.SaveChanges();
            }
        }

        public void SetViewBagData(Child child, string UserId)
        {
            // take the only one record corresponding to the day from db
            // kind of hard coded data 
            // see CourseFrequency table, first 7 records // they are not editable/visible from admin part
            // we consider /this is hard coded part/!!!! 
            // that the group of 4-6 years old kids can have only one active day
            if (child != null && child.YearGroup?.From == 4)
            {
                //var days = db.YearGroups.Where(x => x.YearGroupId == child.YearId)
                //    .Include(x => x.YearGroupDays).ToList();
                var theDay = db.YearGroups.Where(x => x.YearGroupId == child.YearId).Select(x => x.YearGroupDays.FirstOrDefault().Day).First();
                ViewBag.FreqId = new SelectList(db.CourseFrequencies
                    .Where(x => x.FreqId == theDay),
                    "FreqId", "FrequencyDescription", child.FreqId);
            }
            else // take all records besides hidden first 7
            {
                ViewBag.FreqId = new SelectList(db.CourseFrequencies
                .Where(x => !x.Inactive && x.FreqId > 6)
                .OrderBy(x => x.FrequencyDescription),
                "FreqId", "FrequencyDescription", child?.FreqId ?? null);
            }

            // select * active groups with free places
            ViewBag.GroupId = new SelectList(db.Groups.Where(x => !x.Inactive && x.PersonCount > x.Children.Count).OrderBy(x => x.GroupName), "GroupId", "GroupName", child?.GroupId ?? null);

            if (child != null)
            {
                // select * actve groups with free places and with YearId and FreqId equal to child's ones
                ViewBag.GroupId =
                    new SelectList(
                        db.Groups
                        .Where(x =>
                            !x.Inactive &&
                            x.YearId == child.YearId &&
                            //     x.FreqId == child.FreqId &&
                            x.EduLevel == child.EduLevel &&
                            x.PersonCount > x.Children.Count)
                        .OrderBy(x => x.GroupName),
                        "GroupId", "GroupName", child?.GroupId ?? null);

                if (child.GroupId != null)
                {
                    ViewBag.GroupId =
                    new SelectList(
                        db.Groups
                        .Where(x =>
                            x.GroupId == child.GroupId || // this one must be selected because filtering PersonCount>Children.Count may filter child's group too!
                            (!x.Inactive &&
                            x.YearId == child.YearId &&
                            //     x.FreqId == child.FreqId &&
                            x.EduLevel == child.EduLevel &&
                            x.PersonCount > x.Children.Count &&
                            x.GroupId != child.GroupId))
                        .OrderBy(x => x.GroupName),
                        "GroupId", "GroupName", child?.GroupId ?? null);
                }
            }

            //ViewBag.SchoolId = new SelectList(db.Schools.OrderBy(x => x.SchoolName), "SchoolId", "SchoolName", child?.SchoolId ?? null);
            ViewBag.YearId = new SelectList(db.YearGroups.OrderBy(x => x.From), "YearGroupId", "GroupName", child?.YearId ?? null);
            ViewBag.YearGroups = db.YearGroups.OrderBy(x => x.From);

            if (User.IsInRole("Admin") && (!string.IsNullOrEmpty(UserId) || child != null))
            {
                var parent = db.AspNetUsers.Find(!string.IsNullOrEmpty(UserId) ? UserId : child.ParentId);
                if (parent != null)
                {
                    ViewBag.Parent = parent.FullName;
                }
            }
        }
    }
}
