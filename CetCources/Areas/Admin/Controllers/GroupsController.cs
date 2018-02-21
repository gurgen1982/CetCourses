
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CetCources.Database;
using System;
using System.Collections.Generic;
using Resources;
using CetCources.Controllers;
using System.Threading.Tasks;

namespace CetCources.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GroupsController : Controller
    {
        private dbEntities db = new dbEntities();

        // GET: Admin/Groups
        public ActionResult Index()
        {
            var groups = db.Groups.Include(g => g.CourseFrequency).Include(g => g.HourShifts_Sun).Include(g => g.HourShifts_Mon).Include(g => g.HourShifts_Tue).Include(g => g.HourShifts_Wed).Include(g => g.HourShifts_Thu).Include(g => g.HourShifts_Fri).Include(g => g.HourShifts_Sat)
                .Include(g => g.Children).OrderBy(p => p.Inactive).ThenByDescending(p => p.GroupName);
            return View(groups.ToList());
        }

        // GET: Admin/Groups/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // GET: Admin/Groups/Create
        public ActionResult Do(int? id)
        {
            Group group = null;
            if (id != null)
            {
                group = db.Groups.Find(id);
            }

            SetViewBagData(group);

            if (group == null)
            {
                var currentYear = DateTime.Now.Year;
                var yearPart = currentYear - 2000;
                var lastGroup = db.Groups.Where(x => x.CreationDate.Value.Year == currentYear).OrderByDescending(x => x.CreationDate).Take(1);
                var r = lastGroup.FirstOrDefault()?.GroupName.Split(new string[] { "G-" }, StringSplitOptions.None);
                var groupCount = 1;
                if (lastGroup.Count() > 0)
                    groupCount = Convert.ToInt32(r[1]) + 1;
                group = new Group { PersonCount = 8, GroupName = yearPart + "G-" + groupCount.ToString("000"), PlaceId = 0, EduLevel = 1, MailMessage = GroupRes.MailMessageBody };
            }
            return View(group);
        }

        // POST: Admin/Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Do([Bind(Include = "GroupId,YearId,FreqId,GroupName,PersonCount,Inactive,PlaceId,EduLevel,Sunday,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,MailMessage,Trainer")] Group group)
        {
            var bCountChanged = false;
            if (ModelState.IsValid)
            {
                var bAdded = false;
                if (group.GroupId == 0)
                {
                    group.CreationDate = DateTime.Now;
                    db.Groups.Add(group);
                    bAdded = true;
                }
                else
                {
                    var dbGroup = db.Groups.Find(group.GroupId);

                    bCountChanged = group.PersonCount != dbGroup.PersonCount;

                    dbGroup.YearId = group.YearId;
                    dbGroup.FreqId = group.FreqId;
                    dbGroup.Inactive = group.Inactive;
                    dbGroup.PersonCount = group.PersonCount;
                    dbGroup.PlaceId = group.PlaceId;
                    dbGroup.EduLevel = group.EduLevel;
                    dbGroup.Sunday = group.Sunday;
                    dbGroup.Monday = group.Monday;
                    dbGroup.Tuesday = group.Tuesday;
                    dbGroup.Wednesday = group.Wednesday;
                    dbGroup.Thursday = group.Thursday;
                    dbGroup.Friday = group.Friday;
                    dbGroup.Saturday = group.Saturday;
                    dbGroup.Trainer = group.Trainer;

                    db.Entry(dbGroup).State = EntityState.Modified;
                }
                db.SaveChanges();

                group = db.Groups.Where(x => x.GroupId == group.GroupId).Include(x => x.CourseFrequency).First();

                IList<int> childIdList = new List<int>();
                /// if adding, then add all corresponding children to the group!!
                if (bAdded && !group.Inactive)
                {
                    var childrenFreeHours = (db.Children.Where(x => (x.GroupId == null || x.GroupId == 0) && x.YearId == group.YearId
                            //&& x.FreqId == group.FreqId 
                            && x.EduLevel == group.EduLevel)
                        .Include(x => x.FreeHours)
                        .Include(x => x.CourseFrequency)).ToList();
                    foreach (var child in childrenFreeHours)
                    {
                        //if (group.Monday != null && child.FreeHours.Any(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Monday && x.DayHours.BaseHourId == group.Monday))
                        //{
                        //    matchedDayCount++;
                        //}

                        var matchedDayCount = 0;
                        var dayCount = group.CourseFrequency.DaysCount;

                        Action<IEnumerable<FreeHour>, HourShifts> fnc = (free, shift) =>
                        {
                            var found = free.Any(x => x.DayHours.BaseHourId == shift.BaseHourId);

                            if (found && shift.StartHourShifted != shift.BaseHourId)// shifted
                            {
                                if (!free.Any(x => x.DayHours.BaseHourId == shift.BaseHourId + 1))
                                {
                                    found = false;
                                }
                            }
                            if (found)
                                matchedDayCount++;
                        };

                        if (group.Sunday != null)
                        {
                            fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Sunday),
                                db.HourShifts.Find(group.Sunday));
                        }
                        if (group.Monday != null)
                        {
                            fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Monday),
                                db.HourShifts.Find(group.Monday));
                        }
                        if (group.Tuesday != null)
                        {
                            fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Tuesday),
                                db.HourShifts.Find(group.Tuesday));
                        }
                        if (group.Wednesday != null)
                        {
                            fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Wednesday),
                                db.HourShifts.Find(group.Wednesday));
                        }
                        if (group.Thursday != null)
                        {
                            fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Thursday),
                                db.HourShifts.Find(group.Thursday));
                        }
                        if (group.Friday != null)
                        {
                            fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Friday),
                                db.HourShifts.Find(group.Friday));
                        }
                        if (group.Saturday != null)
                        {
                            fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Saturday),
                                db.HourShifts.Find(group.Saturday));
                        }

                        if (matchedDayCount >= dayCount)
                        {
                            child.GroupId = group.GroupId;
                            child.FreqId = group.FreqId;
                            child.EduLevel = group.EduLevel ?? 1;
                            db.Entry(group).State = EntityState.Modified;
                            db.SaveChanges();

                            ChildController.AddTimesFromGroup(db, child);

                            childIdList.Add(child.ChildId);
                        }
                    }
                    if (childIdList.Count > 0)
                    {
                        var grp = db.Groups.Find(group.GroupId);
                        var isFull = (grp.PersonCount <= grp.Children.Count);
                        foreach (var chld in childIdList)
                        {
                            var ch = db.Children.Find(chld);
                            Mail.AdminInfo = ch.AspNetUser.PhoneNumber;
                            Mail.Send(Mails.AcceptedToGroup,
                                string.Format(Mails.AcceptedToGroupBody, ch.AspNetUser.FullName, ch.FullName, ch.Group.GroupName, group.MailMessage),
                                ch.AspNetUser.Email, ch.AspNetUser.FullName);

                            //////Dear { Parent Name}, { ChildFullName}
                            //////has just been added to group { GroupName}. { Additional Message typed by admin when creating the group.}

                            //////Հարգելի { Parent Name}, { ChildFullName}
                            //////ընդգրկվել է { GroupName}
                            //////խմբում։ { Additional Message typed by admin when creating the group.}

                            //$"<b>{ch.FullName}</b> have just been added to group <b>{ch.Group.GroupName}</b>" +
                            //group.MailMessage,
                            //ch.AspNetUser.Email, ch.AspNetUser.FullName);
                            if (isFull)
                            {
                                Mail.AdminInfo = ch.AspNetUser.PhoneNumber;
                                Mail.Send(
                                    string.Format(Mails.GroupIsFull, ch.Group.GroupName),//$"Group {ch.Group.GroupName} is full", 
                                    string.Format(Mails.GroupIsFullBody, ch.AspNetUser.FullName, ch.Group.GroupName, ch.FullName),//$"The group <b>{ch.Group.GroupName}</b> to which <b>{ch.FullName}</b> joined is full and is ready to start", 
                                    ch.AspNetUser.Email, ch.AspNetUser.FullName);
                            }
                        }
                    }
                }
                else // editing the group.
                {
                    // check for inactivity and remove children from the group
                    if (group.Inactive)
                    {
                        var children = db.Children.Where(x => x.GroupId == group.GroupId).ToList();
                        foreach (var child in children)
                        {
                            child.GroupId = null;
                            db.Entry(child).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                    if (bCountChanged)
                    {
                        var grp = db.Groups.Find(group.GroupId);
                        if (grp.PersonCount <= grp.Children.Count)
                        {
                            foreach (var ch in grp.Children)
                            {
                                Mail.AdminInfo = ch.AspNetUser.PhoneNumber;
                                Mail.Send(
                                      string.Format(Mails.GroupIsFull, ch.Group.GroupName),//$"Group {ch.Group.GroupName} is full", 
                                      string.Format(Mails.GroupIsFullBody, ch.AspNetUser.FullName, ch.Group.GroupName, ch.FullName),//$"The group <b>{ch.Group.GroupName}</b> to which <b>{ch.FullName}</b> joined is full and is ready to start", 
                                      ch.AspNetUser.Email, ch.AspNetUser.FullName);
                            }
                        }
                    }

                    return RedirectToAction("Index");
                }

                return RedirectToAction("Details", new { id = group.GroupId });
            }

            SetViewBagData(group);

            return View(group);
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Admin/Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Group group = db.Groups.Find(id);
            db.Groups.Remove(group);
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

        public ActionResult GetMatchedChildrenList(Group group)
        {
            var res = db.MatchingChildrenByHours(group.YearId, group.FreqId, group?.EduLevel ?? 0, group.Sunday, group.Monday, group.Tuesday, group.Wednesday, group.Thursday, group.Friday, group.Saturday).ToList();
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Completed(int groupId)
        {
            var group = db.Groups.Where(x => x.GroupId == groupId).FirstOrDefault();
            if (group==null || group.EduLevel == 8) return null;
            group.EduLevel += 1;
            db.Entry(group).State = EntityState.Modified;
            var children = group.Children;
            foreach (var child in children)
            {
                child.EduLevel += 1;
                db.Entry(child).State = EntityState.Modified;
                db.SaveChanges();
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public bool RemoveFromGroup(int? id)
        {
            try
            {
                var child = db.Children.Find(id);
                child.GroupId = null;
                db.Entry(child).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch 
            {
                return false;
            }
            return true;
        }

        private void SetViewBagData(Group group)
        {
            var freqCourse = db.CourseFrequencies.Where(p => !p.Inactive).OrderBy(p => p.FrequencyDescription);
            ViewBag.FreqCourse = freqCourse;

            var yearGroups = db.YearGroups.OrderBy(p => p.From).Include(x => x.YearGroupDays);
            ViewBag.YearGroups = yearGroups;

            ViewBag.FreqId = new SelectList(freqCourse, "FreqId", "FrequencyDescription", group?.FreqId);
            ViewBag.YearId = new SelectList(yearGroups, "YearGroupId", "GroupName", group?.YearId);

            var r = yearGroups.Where(x => x.From == 4).FirstOrDefault().YearGroupDays.FirstOrDefault();
            ViewBag.For4_6FreqID = r.Day;
            ViewBag.For4_6YearGroupID = r.YearGroupId;

            var otherGroups = db.Groups.Where(x => !x.Inactive);
            if (group != null)
            {
                otherGroups = otherGroups.Where(x => !x.Inactive && x.GroupId != group.GroupId);
            }

            Func<List<HourShifts>, List<HourShifts>> RemoveInter = (dShifts) =>
            {
                var shft = dShifts.ToList();
                int index = 0;
                foreach (var item in shft)
                {
                    var count = 0;
                    for (int j = item.ShiftId; j < item.ShiftId + 4; j++)
                    {
                        count += dShifts.Any(x => x.ShiftId.Equals(j)) ? 1 : 0;
                    }
                    if (count != 4)
                    {
                        if (count < shft.Count - index)
                            dShifts.RemoveAll(x => x.ShiftId.Equals(item.ShiftId));
                    }
                    index++;
                }

                return dShifts;
            };

            for (int i = 0; i < 7; i++)
            {
                var dShifts = (from sh in db.HourShifts
                               join dh in db.DayHours on sh.BaseHourId equals dh.BaseHourId
                               where dh.DayOfWeek == i && !dh.Inactive
                               select sh).ToList();

                switch (i)
                {
                    case 0:
                        foreach (var oG in otherGroups)
                        {
                            dShifts.RemoveAll(x => x.ShiftId == oG.Sunday || x.ShiftId == oG.Sunday + 1 || x.ShiftId == oG.Sunday + 2 || x.ShiftId == oG.Sunday + 3);
                        }

                        ViewBag.Sunday = new SelectList(RemoveInter(dShifts).Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Sunday);
                        break;
                    case 1:
                        foreach (var oG in otherGroups)
                        {
                            dShifts.RemoveAll(x => x.ShiftId == oG.Monday || x.ShiftId == oG.Monday + 1 || x.ShiftId == oG.Monday + 2 || x.ShiftId == oG.Monday + 3);
                        }
                        ViewBag.Monday = new SelectList(RemoveInter(dShifts).Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Monday);
                        break;
                    case 2:
                        foreach (var oG in otherGroups)
                        {
                            dShifts.RemoveAll(x => x.ShiftId == oG.Tuesday || x.ShiftId == oG.Tuesday + 1 || x.ShiftId == oG.Tuesday + 2 || x.ShiftId == oG.Tuesday + 3);
                        }
                        ViewBag.Tuesday = new SelectList(RemoveInter(dShifts).Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Tuesday);
                        break;
                    case 3:
                        foreach (var oG in otherGroups)
                        {
                            dShifts.RemoveAll(x => x.ShiftId == oG.Wednesday || x.ShiftId == oG.Wednesday + 1 || x.ShiftId == oG.Wednesday + 2 || x.ShiftId == oG.Wednesday + 3);
                        }
                        ViewBag.Wednesday = new SelectList(RemoveInter(dShifts).Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Wednesday);
                        break;
                    case 4:
                        foreach (var oG in otherGroups)
                        {
                            dShifts.RemoveAll(x => x.ShiftId == oG.Thursday || x.ShiftId == oG.Thursday + 1 || x.ShiftId == oG.Thursday + 2 || x.ShiftId == oG.Thursday + 3);
                        }
                        ViewBag.Thursday = new SelectList(RemoveInter(dShifts).Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Thursday);
                        break;
                    case 5:
                        foreach (var oG in otherGroups)
                        {
                            dShifts.RemoveAll(x => x.ShiftId == oG.Friday || x.ShiftId == oG.Friday + 1 || x.ShiftId == oG.Friday + 2 || x.ShiftId == oG.Friday + 3);
                        }
                        ViewBag.Friday = new SelectList(RemoveInter(dShifts).Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Friday);
                        break;
                    case 6:
                        foreach (var oG in otherGroups)
                        {
                            dShifts.RemoveAll(x => x.ShiftId == oG.Saturday || x.ShiftId == oG.Saturday + 1 || x.ShiftId == oG.Saturday + 2 || x.ShiftId == oG.Saturday + 3);
                        }
                        ViewBag.Saturday = new SelectList(RemoveInter(dShifts).Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Saturday);
                        break;
                }
            }



            //ViewBag.Sunday = new SelectList(db.HourShifts.Where(x =>
            //            !db.DayHours
            //                .Where(p => p.Inactive && p.DayOfWeek == (int)System.DayOfWeek.Sunday)
            //                .Any(d => d.BaseHourId == x.BaseHourId)
            //            && !otherGroups.Any(g => g.Sunday == x.ShiftId))
            //    .Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Sunday);
            //ViewBag.Monday = new SelectList(db.HourShifts.Where(x =>
            //            !db.DayHours
            //                .Where(p => p.Inactive && p.DayOfWeek == (int)System.DayOfWeek.Monday)
            //                .Any(d => d.BaseHourId == x.BaseHourId)
            //            && !otherGroups.Any(g => g.Monday == x.ShiftId))
            //    .Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Monday);
            //ViewBag.Tuesday = new SelectList(db.HourShifts.Where(x =>
            //            !db.DayHours
            //                .Where(p => p.Inactive && p.DayOfWeek == (int)System.DayOfWeek.Tuesday)
            //                .Any(d => d.BaseHourId == x.BaseHourId)
            //            && !otherGroups.Any(g => g.Tuesday == x.ShiftId))
            //    .Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Tuesday);
            //ViewBag.Wednesday = new SelectList(db.HourShifts.Where(x =>
            //            !db.DayHours
            //                .Where(p => p.Inactive && p.DayOfWeek == (int)System.DayOfWeek.Wednesday)
            //                .Any(d => d.BaseHourId == x.BaseHourId)
            //            && !otherGroups.Any(g => g.Wednesday == x.ShiftId))
            //    .Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Wednesday);
            //ViewBag.Thursday = new SelectList(db.HourShifts.Where(x =>
            //            !db.DayHours
            //                .Where(p => p.Inactive && p.DayOfWeek == (int)System.DayOfWeek.Thursday)
            //                .Any(d => d.BaseHourId == x.BaseHourId)
            //            && !otherGroups.Any(g => g.Thursday == x.ShiftId))
            //    .Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Thursday);
            //ViewBag.Friday = new SelectList(db.HourShifts.Where(x =>
            //            !db.DayHours
            //                .Where(p => p.Inactive && p.DayOfWeek == (int)System.DayOfWeek.Friday)
            //                .Any(d => d.BaseHourId == x.BaseHourId)
            //            && !otherGroups.Any(g => g.Friday == x.ShiftId))
            //    .Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Friday);
            //ViewBag.Saturday = new SelectList(db.HourShifts.Where(x =>
            //            !db.DayHours
            //                .Where(p => p.Inactive && p.DayOfWeek == (int)System.DayOfWeek.Saturday)
            //                .Any(d => d.BaseHourId == x.BaseHourId)
            //            && !otherGroups.Any(g => g.Saturday == x.ShiftId))
            //    .Select(x => new { x.ShiftId, x.HourShift }).OrderBy(x => x.ShiftId), "ShiftId", "HourShift", group?.Saturday);

            ViewBag.IsEditMode = group != null;
        }
    }
}
