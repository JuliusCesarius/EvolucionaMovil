using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EvolucionaMovil.Models;
using EvolucionaMovil.Repositories;
using AutoMapper;

namespace EvolucionaMovil.Controllers
{ 
    public class PayCentersController : Controller
    {
        private PayCentersRepository repository = new PayCentersRepository();

        //
        // GET: /PayCenters/

        public ViewResult Index()
        {
            var paycenters = repository.ListAll();
            return View(paycenters.ToListOfDestination<PayCenterVM>());
        }

        //
        // GET: /PayCenters/Details/5

        public ViewResult Details(int id)
        {
            PayCenter paycenter = repository.LoadById(id);
            PayCenterVM paycenterVM = new PayCenterVM();
            Mapper.Map(paycenter,paycenterVM);
            return View(paycenterVM);
        }

        //
        // GET: /PayCenters/Create

        public ActionResult Create()
        {
            ViewBag.ProspectoId = new SelectList(repository.LoadProspectos().ToListOfDestination<ProspectoVM>(), "ProspectoId", "Email");
            ViewBag.UsuarioId = new SelectList(repository.LoadUsuarios().ToListOfDestination<UsuarioVM>(), "UsuarioId", "Email");
            return View();
        } 

        //
        // POST: /PayCenters/Create

        [HttpPost]
        public ActionResult Create(PayCenterVM paycenterVM)
        {
            if (ModelState.IsValid)
            {
                PayCenter paycenter = new PayCenter();
                //TODO:Leer valor de la imagen del comprobante de domicilio
                paycenterVM.Comprobante = string.Empty;
                Mapper.Map(paycenterVM, paycenter);
                repository.Add(paycenter);
                repository.Save();
                return RedirectToAction("Index");  
            }

            ViewBag.ProspectoId = new SelectList(repository.LoadProspectos().ToListOfDestination<ProspectoVM>(), "ProspectoId", "Email", paycenterVM.ProspectoId);
            ViewBag.UsuarioId = new SelectList(repository.LoadUsuarios().ToListOfDestination<UsuarioVM>(), "UsuarioId", "Email", paycenterVM.UsuarioId);
            return View(paycenterVM);
        }
        
        //
        // GET: /PayCenters/Edit/5
 
        public ActionResult Edit(int id)
        {
            PayCenter paycenter = repository.LoadById(id);
            PayCenterVM paycenterVM = new PayCenterVM();
            Mapper.Map(paycenter, paycenterVM);
            ViewBag.ProspectoId = new SelectList(repository.LoadProspectos(), "ProspectoId", "Email", paycenter.ProspectoId);
            ViewBag.UsuarioId = new SelectList(repository.LoadUsuarios(), "UsuarioId", "Email", paycenter.UsuarioId);
            return View(paycenterVM);
        }

        //
        // POST: /PayCenters/Edit/5

        [HttpPost]
        public ActionResult Edit(PayCenterVM paycenterVM)
        {
            if (ModelState.IsValid)
            {
                
                //TODO: Asignar valor al comprobante
                paycenterVM.Comprobante = string.Empty;
                PayCenter paycenter = repository.LoadById(paycenterVM.PayCenterId);
                Mapper.Map(paycenterVM, paycenter);
                Mapper.Map(paycenterVM.Cuentas, paycenter.Cuentas);
                Mapper.Map(paycenterVM.Abonos, paycenter.Abonos);

                repository.Save();
                return RedirectToAction("Index");
            }
            ViewBag.ProspectoId = new SelectList(repository.LoadProspectos(), "ProspectoId", "Email", paycenterVM.ProspectoId);
            ViewBag.UsuarioId = new SelectList(repository.LoadUsuarios(), "UsuarioId", "Email", paycenterVM.UsuarioId);
            return View(paycenterVM);
        }

        //
        // GET: /PayCenters/Delete/5
 
        public ActionResult Delete(int id)
        {
            PayCenter paycenter = repository.LoadById(id);
            PayCenterVM paycenterVM = new PayCenterVM();
            Mapper.Map(paycenter, paycenterVM);
            return View(paycenterVM);
        }

        //
        // POST: /PayCenters/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            PayCenter paycenter = repository.LoadById(id);
            repository.Delete(paycenter);
            repository.Save();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }
    }
}