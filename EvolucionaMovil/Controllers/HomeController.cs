using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EvolucionaMovil.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Logon","Account");
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
