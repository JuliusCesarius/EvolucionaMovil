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
    public class ProspectosController : Controller
    {
        //private List<string> Mensajes = new List<string>();
        private ProspectosRepository repository = new ProspectosRepository();

        //
        // GET: /Prospectos/

        public ViewResult Index()
        {
            return View(repository.ListAll().ToListOfDestination<ProspectoVM>());
        }

        //
        // GET: /Prospectos/Details/5

        public ViewResult Details(int id)
        {
            Prospecto prospecto = repository.LoadById(id);
            ProspectoVM prospectoVM = new ProspectoVM();
            Mapper.Map(prospecto, prospectoVM);
            return View(prospectoVM);
        }

        //
        // GET: /Prospectos/Preafiliacion

        public ActionResult Preafiliacion()
        {
            return View();
        }

        //
        // POST: /Prospectos/Preafiliacion

        [HttpPost]
        public ActionResult Preafiliacion(ProspectoVM prospectoVM)
        {
            //bool exito = EmailUnico(prospectoVM.Email);
            //if (!exito)
            //{
            //    Mensajes.Add("El Email proporcionado ya existe.");
            //}
            bool exito = false;
            if (ModelState.IsValid) //&& exito)
            {
                Prospecto prospecto = new Prospecto();
                Mapper.Map(prospectoVM, prospecto);
                repository.Add(prospecto);
                exito = repository.Save();
                prospectoVM.ProspectoId = prospecto.ProspectoId;
            }
            if (exito)
            {
                return RedirectToAction("Confirmacion/" + prospectoVM.ProspectoId.ToString());
            }
            else
            {
                //ViewBag.Mensajes = Mensajes;
                return View(prospectoVM);
            }
        }

        public ViewResult Confirmacion(Int32 id)
        {
            return View();
        }

        //
        // GET: /Prospectos/Edit/5

        public ActionResult Edit(int id)
        {
            Prospecto prospecto = repository.LoadById(id);
            ProspectoVM prospectoVM = new ProspectoVM();
            Mapper.Map(prospecto, prospectoVM);
            return View(prospectoVM);
        }

        //
        // POST: /Prospectos/Edit/5

        [HttpPost]
        public ActionResult Edit(ProspectoVM prospectoVM)
        {
            if (ModelState.IsValid)
            {
                Prospecto prospecto = repository.LoadById(prospectoVM.ProspectoId);
                Mapper.Map(prospectoVM, prospecto);
                repository.Save();
                return RedirectToAction("Index");
            }
            return View(prospectoVM);
        }

        //
        // GET: /Prospectos/Delete/5

        public ActionResult Delete(int id)
        {
            Prospecto prospecto = repository.LoadById(id);
            ProspectoVM prospectoVM = new ProspectoVM();
            Mapper.Map(prospecto, prospectoVM);
            return View(prospectoVM);
        }

        //
        // POST: /Prospectos/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Prospecto prospecto = repository.LoadById(id);
            repository.Delete(prospecto);
            repository.Save();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        //Validar si existe el email en la base de datos
        //private static bool EmailUnico(string Email) {
        //    return !repository.ExisteEmail(Email);
        //}
    }
}