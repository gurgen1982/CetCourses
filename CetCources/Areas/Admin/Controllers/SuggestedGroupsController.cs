using CetCources.Database;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CetCources.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SuggestedGroupsController : Controller
    {
        private dbEntities db = new dbEntities();

        // GET: Admin/SuggestedGroups
        public ActionResult Index()
        {
            var fullyMatchedList = new List<Child>();
            var halfMatchedList = new List<Child>();
            var notMatchedList = new List<Child>();
            foreach (var child in
                db.Children
                .Where(x => !x.Inactive && (x.GroupId == null || x.GroupId == 0))
                .OrderBy(x => x.YearGroup.From)
                .ThenBy(x => x.CourseFrequency.DaysCount))
            {
                var foundGroups = db.Groups
                    .Where(x => !x.Inactive && x.YearId == child.YearId
                        //&& x.FreqId == child.FreqId 
                        && x.EduLevel == child.EduLevel);
                foreach (var group in foundGroups)
                {
                    var count = 0.0;
                    //if (group.Sunday != null && child.FreeHours.Any(x => x.DayHours.DayOfWeek ==
                    //    (int)System.DayOfWeek.Sunday
                    //    && x.DayHours.BaseHourId == group.Sunday))
                    //{
                    //    count++;
                    //}

                    Action<IEnumerable<FreeHour>, HourShifts> fnc = (free, shift) =>
                    {
                        if (free.Any(x => x.DayHours.BaseHourId == shift.BaseHourId))
                        {
                            if (shift.BaseHourId != shift.StartHourShifted)
                            {
                                if (free.Any(x => x.DayHours.BaseHourId == shift.BaseHourId + 1))
                                {
                                    count += 1;
                                }
                                else
                                {
                                    count += 0.5;
                                }
                            }
                            else
                            {
                                count += 1;
                            }
                        }
                    };

                    if (group.Sunday != null)
                    {
                        fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Sunday),
                            group.HourShifts_Sun);
                    }
                    if (group.Monday != null)
                    {
                        fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Monday),
                            group.HourShifts_Mon);
                    }
                    if (group.Tuesday != null)
                    {
                        fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Tuesday),
                            group.HourShifts_Tue);
                    }
                    if (group.Wednesday != null)
                    {
                        fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Wednesday),
                            group.HourShifts_Wed);
                    }
                    if (group.Thursday != null)
                    {
                        fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Thursday),
                            group.HourShifts_Thu);
                    }

                    if (group.Friday != null)
                    {
                        fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Friday),
                            group.HourShifts_Fri);
                    }
                    if (group.Saturday != null)
                    {
                        fnc(child.FreeHours.Where(x => x.DayHours.DayOfWeek == (int)System.DayOfWeek.Saturday),
                            group.HourShifts_Sat);
                    }

                    if (count >= group.CourseFrequency.DaysCount)
                    {
                        var ch = Clone(child);
                        ch.GroupId = group.GroupId;
                        ch.Group = group;
                        fullyMatchedList.Add(ch);
                    }
                    else if (count > 0)
                    {
                        var ch = Clone(child);
                        ch.GroupId = group.GroupId;
                        ch.Group = group;
                        ch.AdditionalData = count + "/" + group.CourseFrequency.DaysCount;
                        halfMatchedList.Add(ch);
                    }
                    else
                    {
                        if (!notMatchedList.Any(x => x.ChildId == child.ChildId))
                        {
                            notMatchedList.Add(child);
                        }
                    }
                }
                if (foundGroups.Count() == 0)
                {
                    if (!notMatchedList.Any(x => x.ChildId == child.ChildId))
                    {
                        notMatchedList.Add(child);
                    }
                }
            }
            ViewBag.HalfMatchedList = halfMatchedList; //.Where(x => !x.Inactive).OrderByDescending(x => x.ChildId).ThenByDescending(x => x.AdditionalData);
            ViewBag.NotMatchedList = notMatchedList; //.Where(x=>!x.Inactive).OrderByDescending(x => x.ChildId);
            return View(fullyMatchedList);
        }

        [HttpPost]
        public async Task<JsonResult> Accept(int id, int id2)
        {
            var child = db.Children.Find(id);
            child.GroupId = id2;
            db.Entry(child).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            child = db.Children.Find(child.ChildId);
            //await Mail.Send("Accepted to group", $"<b>{child.FullName}</b> have just been added to group <b>{child.Group.GroupName}</b>", child.AspNetUser.Email, child.AspNetUser.FullName);

            await Mail.Send(Mails.AcceptedToGroup,
                           string.Format(Mails.AcceptedToGroupBody, child.AspNetUser.FullName, child.FullName, child.Group.GroupName, string.Empty),
                           child.AspNetUser.Email, child.AspNetUser.FullName);


            if (child.Group.PersonCount <= child.Group.Children.Count)
            {
                //await Mail.Send($"Group {child.Group.GroupName} is full", $"The group <b>{child.Group.GroupName}</b> to which <b>{child.FullName}</b> joined is full and is ready to start", child.AspNetUser.Email, child.AspNetUser.FullName);

                await Mail.Send(string.Format(Mails.GroupIsFull, child.Group.GroupName),
                                  string.Format(Mails.GroupIsFullBody, child.AspNetUser.FullName, child.Group.GroupName, child.FullName),
                                  child.AspNetUser.Email, child.AspNetUser.FullName);
            }

            return Json(child.ChildId, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">child id</param>
        /// <param name="id2">group id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Details(int? id, int? id2)
        {
            if (id == null || id2 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Child child = db.Children.Find(id);
            if (child == null)
            {
                return HttpNotFound();
            }

            Group group = db.Groups.Find(id2);
            if (group == null)
            {
                return HttpNotFound();
            }

            ViewBag.FreeHours = db.ChildDayHours(id);

            ViewBag.Group = group;

            return View(child);
        }

        public Child Clone(Child child)
        {
            var ch = new Child();
            ch.AdditionalData = child.AdditionalData;
            ch.AspNetUser = child.AspNetUser;
            //ch.BirthDate = child.BirthDate;
            ch.BirthDateString = child.BirthDateString;
            ch.ChildId = child.ChildId;
            ch.ClassNo = child.ClassNo;
            ch.Comment = child.Comment;
            ch.CourseFrequency = child.CourseFrequency;
            ch.CreationDate = child.CreationDate;
            ch.EduLevel = child.EduLevel;
            ch.FreeHours = child.FreeHours;
            ch.FreqId = child.FreqId;
            ch.FullName = child.FullName;
            ch.Group = child.Group;
            ch.GroupId = child.GroupId;
            ch.Inactive = child.Inactive;
            ch.ParentId = child.ParentId;
            //ch.School = child.School;
            ch.SchoolId = child.SchoolId;
            ch.YearGroup = child.YearGroup;
            ch.YearId = child.YearId;

            return ch;
        }
        //// GET: Admin/SuggestedGroups
        //public ActionResult Index()
        //{
        //    return View();
        //}

        //public ActionResult GetSuggestedGroups()
        //{
        //    var query = from gs in db.GroupSuggestions
        //                orderby gs.From ascending, gs.Day ascending, gs.HourId
        //                select new { gs.Hours, gs.ChildCount, gs.YearGroupName, gs.Day, gs.HourId, gs.YearGroupId, gs.DayOfWeek };

        //    return Json(query.ToList(), JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult RowChildren(int? hourid, int? day, int? yearid)
        //{
        //    if (hourid == null || day == null || yearid == null)
        //    {
        //        return Json(null, JsonRequestBehavior.AllowGet);
        //    }

        //    var query = from c in db.Children
        //                join p in db.AspNetUsers
        //                on c.ParentId equals p.Id
        //                join fh in db.FreeHours
        //                on c.ChildId equals fh.ChildId
        //                join h in db.Hours
        //                on fh.HourId equals h.HourId
        //                where c.GroupId == null && fh.HourId == hourid && h.DayOfWeek == day && c.YearId == yearid
        //                select new { c.FullName, p.PhoneNumber };

        //    return Json(query.ToList(), JsonRequestBehavior.AllowGet);
        //}
    }
}