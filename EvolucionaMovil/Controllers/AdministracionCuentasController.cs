using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EvolucionaMovil.Models;

namespace EvolucionaMovil.Controllers
{ 
    public class AdministracionCuentasController : Controller
    {
        private EvolucionaMovilBDEntities db = new EvolucionaMovilBDEntities();

        //
        // GET: /AdministracionCuentas/

        public ViewResult Index()
        {
            var movimientoempresas = db.Resumen_Movimientos;
            return View(movimientoempresas.OrderByDescending(x=>x.FechaActualizacion).ToList());
        }

        //
        // GET: /AdministracionCuentas/Details/5

        public ViewResult Details(int id)
        {
            MovimientoEmpresa movimientoempresa = db.MovimientoEmpresas.Single(m => m.MovimientoEmpresaId == id);
            return View(movimientoempresa);
        }

        //
        // GET: /AdministracionCuentas/Create

        public ActionResult Create()
        {
            ViewBag.PayCenterId = new SelectList(db.Movimientos, "PayCenterId", "Clave");
            return View();
        } 

        //
        // POST: /AdministracionCuentas/Create

        [HttpPost]
        public ActionResult Create(MovimientoEmpresa movimientoempresa)
        {
            if (ModelState.IsValid)
            {
                db.MovimientoEmpresas.AddObject(movimientoempresa);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.PayCenterId = new SelectList(db.Movimientos, "PayCenterId", "Clave", movimientoempresa.PayCenterId);
            return View(movimientoempresa);
        }
        
        //
        // GET: /AdministracionCuentas/Edit/5
 
        public ActionResult Edit(int id)
        {
            MovimientoEmpresa movimientoempresa = db.MovimientoEmpresas.Single(m => m.MovimientoEmpresaId == id);
            ViewBag.PayCenterId = new SelectList(db.Movimientos, "PayCenterId", "Clave", movimientoempresa.PayCenterId);
            return View(movimientoempresa);
        }

        //
        // POST: /AdministracionCuentas/Edit/5

        [HttpPost]
        public ActionResult Edit(MovimientoEmpresa movimientoempresa)
        {
            if (ModelState.IsValid)
            {
                db.MovimientoEmpresas.Attach(movimientoempresa);
                db.ObjectStateManager.ChangeObjectState(movimientoempresa, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PayCenterId = new SelectList(db.Movimientos, "PayCenterId", "Clave", movimientoempresa.PayCenterId);
            return View(movimientoempresa);
        }

        //
        // GET: /AdministracionCuentas/Delete/5
 
        public ActionResult Delete(int id)
        {
            MovimientoEmpresa movimientoempresa = db.MovimientoEmpresas.Single(m => m.MovimientoEmpresaId == id);
            return View(movimientoempresa);
        }

        //
        // POST: /AdministracionCuentas/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            MovimientoEmpresa movimientoempresa = db.MovimientoEmpresas.Single(m => m.MovimientoEmpresaId == id);
            db.MovimientoEmpresas.DeleteObject(movimientoempresa);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}