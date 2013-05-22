using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EvolucionaMovil.Models;
using EvolucionaMovil.Models.Classes;
using EvolucionaMovil.Attributes;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Controllers
{
    public class ExceptionsController : CustomControllerBase
    {
        private EvolucionaMovilBDEntities db = new EvolucionaMovilBDEntities();

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator})]
        public ViewResult Index()
        {
            //Regreso ordenados por fecha, y filtrados por el image not found que creo genera el IPad
            return View(db.ErrorLogs.Where(x => !x.Message.Contains("precomposed.png")).OrderByDescending(x => x.FechaCreacion).ToList());
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ViewResult Details(int id)
        {
            ErrorLog errorlog = db.ErrorLogs.Single(e => e.ErrorLogId == id);
            return View(errorlog);
        }
        [NonAction]
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}