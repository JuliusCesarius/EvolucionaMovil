using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EvolucionaMovil.Controllers
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Authorization()
        {
            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult GenericError(object e)
        {
            return View();
        }
    }
}
