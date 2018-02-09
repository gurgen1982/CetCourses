using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CetCources.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Child");
            }
            return View();
        }

        public ActionResult About()
        {
           // ViewBag.Message = AboutRes.AboutMessage;

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = AboutRes.ContactPage;

            return View();
        }

        public ActionResult Terms()
        {
            ViewBag.Message = AboutRes.ContactPage;

            return View();
        }
    }
}