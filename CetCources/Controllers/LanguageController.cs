using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CetCources.Controllers
{
    public class LanguageController : Controller
    {
        // set language
        [HttpPost]
        public ActionResult Set(string lang)
        {
            var culture = "en-US";
            //var lang = Convert.ToString(Request.QueryString["lang"]);
            if (!string.IsNullOrEmpty(lang))
            {
                if (lang == "en")
                {
                    culture = "en-US";
                }
                if (lang == "hy")
                {
                    culture = "hy-AM";
                }
            }
            var langCookie = new HttpCookie("language", culture);
            langCookie.Expires = DateTime.Now.AddYears(5);
            Response.Cookies.Add(langCookie);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}