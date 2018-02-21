using CetCources.Database;
using Exporter;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System;
using System.IO;

namespace CetCources.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExportController : Controller
    {
        private dbEntities db = new dbEntities();
        // GET: Admin/Export
        public ActionResult Index()
        {
          //  var path = "";
            try
            {
                try
                {
                    // execute stored procedure on sql server
                    db.ExportData();
                }
                catch(Exception ex)
                {
                    return Json("db.ExportData(): " + ex.Message + "----------" + ex.InnerException + "----------" + ex.StackTrace, JsonRequestBehavior.AllowGet);
                }
                // now select the result and pass to method
                // to create xls file
                try
                {
                    var bytes = ExportTo.ExcelFileNP(db.qExportDatas.ToList());

                    return File(bytes, "application/vnd.ms-excel", "Export.xls");
                    
                }
                catch(Exception ex)
                {
                    return Json("ExportTo.ExcelFile: " + ex.Message + "<br />" + ex.StackTrace, JsonRequestBehavior.AllowGet);
                }
              
            }
            catch(Exception ex)
            {
                // return notification about error
                ViewBag.message = ex.Message;
                ViewBag.StackTrace = ex.StackTrace+"<br />Inner Exception error message: "+
                    ex.InnerException.Message;
                return View();
            }
        }
    }
}