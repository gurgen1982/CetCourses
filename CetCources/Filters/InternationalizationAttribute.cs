using System;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class InternationalizationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        string language = filterContext.HttpContext.Request.Cookies["language"]?.Value;
        if (language == null)
        {
            var userLang = filterContext.HttpContext.Request.UserLanguages;
            if (userLang != null && userLang.Length > 0)
            {
                language = userLang[0];
                if (language.ToLower().Contains("hy-am"))
                {
                    language = "hy-AM";
                }
                else
                {
                    language = "en-US";
                }
            }
            else
            {
                language = "en-US";
            }
        }

        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(language);
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);

    }
}