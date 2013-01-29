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
    public class ServiciosController : Controller
    {
        private ServiciosRepository repository = new ServiciosRepository();

        //
        // GET: /Servicios/

        public ViewResult Index()
        {
            return View(repository.ListAll().ToListOfDestination<ServicioVM>());
        }

        //
        // GET: /Servicios/Details/5

        public ViewResult Details(int id)
        {
            ServicioVM servicioVM = new ServicioVM();
            Servicio servicio = repository.LoadById(id);
            Mapper.Map(servicio, servicioVM);
            return View(servicioVM);
        }

        //
        // GET: /Servicios/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Servicios/Create

        [HttpPost]
        public ActionResult Create(ServicioVM servicioVM)
        {
            Servicio servicio = new Servicio();
            if (ModelState.IsValid)
            {
                Mapper.Map(servicioVM, servicio);
                repository.Add(servicio);
                repository.Save();
                return RedirectToAction("Index");  
            }

            return View(servicioVM);
        }
        
        //
        // GET: /Servicios/Edit/5
 
        public ActionResult Edit(int id)
        {
            ServicioVM servicioVM = new ServicioVM();
            Servicio servicio = repository.LoadById(id);
            Mapper.Map(servicio, servicioVM);
            return View(servicioVM);
        }

        //
        // POST: /Servicios/Edit/5

        [HttpPost]
        public ActionResult Edit(ServicioVM servicioVM)
        {
            Servicio servicio = repository.LoadById(servicioVM.ServicioId);
            if (ModelState.IsValid)
            {
                Mapper.Map(servicioVM, servicio);
                repository.Add(servicio);
                repository.Save();
                servicioVM.ServicioId = servicio.ServicioId;
                return RedirectToAction("Index");
            }
            return View(servicioVM);
        }

        //
        // GET: /Servicios/Delete/5
 
        public ActionResult Delete(int id)
        {
            ServicioVM servicioVM = new ServicioVM(); 
            Servicio servicio = repository.LoadById(id);
            Mapper.Map(servicio, servicioVM);
            return View(servicioVM);
        }

        //
        // POST: /Servicios/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            ServicioVM servicioVM = new ServicioVM();
            Servicio servicio = repository.LoadById(id);
            Mapper.Map(servicio, servicioVM);
            repository.Delete(servicio);
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