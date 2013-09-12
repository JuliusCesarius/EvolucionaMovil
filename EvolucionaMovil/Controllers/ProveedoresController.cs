using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EvolucionaMovil.Models;
using EvolucionaMovil.Models.Classes;
using EvolucionaMovil.Repositories;
using AutoMapper;
using MvcMembership;
using System.Web.Security;
using EvolucionaMovil.Models.Enums;
using EvolucionaMovil.Attributes;

namespace EvolucionaMovil.Controllers
{ 
    public class ProveedoresController : CustomControllerBase
    {
        private EvolucionaMovilBDEntities db = new EvolucionaMovilBDEntities();
        private ProveedoresRepository repository = new ProveedoresRepository();

        public string FindProveedores(string term)
        {
            var Proveedores = repository.GetProveedorBySearchString(term).Select(x => new { label = x.Nombre, value = x.ProveedorId });
            return Newtonsoft.Json.JsonConvert.SerializeObject(Proveedores);
        }

        //
        // GET: /Proveedores/

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ViewResult Index()
        {
            return View(repository.ListAll().ToList().ToListOfDestination<ProveedorVM>());
        }

        //
        // GET: /Proveedores/Details/5

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ViewResult Details(int id)
        {
            var proveedor = repository.LoadById(id);
            ProveedorVM proveedorVM = new ProveedorVM();
            Mapper.Map(proveedor, proveedorVM);
            return View(proveedorVM);
        }

        //
        // GET: /Proveedores/Create

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult Create()
        {
            var tiposCuenta = new Dictionary<short, string>();
            tiposCuenta.Add(0, enumTipoCuenta.Pago_de_Servicios.ToString());
            tiposCuenta.Add(1, enumTipoCuenta.Recargas_Electronicas.ToString());
            ViewBag.TiposCuenta = tiposCuenta;
            return View();
        } 

        //
        // POST: /Proveedores/Create

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult Create(ProveedorVM proveedorVM)
        {
            if (ModelState.IsValid)
            {
                Proveedor proveedor = new Proveedor();
                Mapper.Map(proveedorVM, proveedor);
                repository.Add(proveedor);
                repository.Save();
                return RedirectToAction("Index");  
            }

            return View(proveedorVM);
        }
        
        //
        // GET: /Proveedores/Edit/5

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult Edit(int id)
        {
            var proveedor = repository.LoadById(id);
            ProveedorVM proveedorVM = new ProveedorVM();
            Mapper.Map(proveedor, proveedorVM);
            var tiposCuenta= new Dictionary<short, string>();
            tiposCuenta.Add(0, enumTipoCuenta.Pago_de_Servicios.ToString());
            tiposCuenta.Add(1, enumTipoCuenta.Recargas_Electronicas.ToString());
            ViewBag.TiposCuenta = tiposCuenta;
            return View(proveedorVM);
        }

        //
        // POST: /Proveedores/Edit/5

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult Edit(ProveedorVM proveedorVM)
        {
            Proveedor proveedor = repository.LoadById(proveedorVM.ProveedorId);
            if (ModelState.IsValid)
            {
                Mapper.Map(proveedorVM, proveedor);
                repository.Save();
                proveedorVM.ProveedorId = proveedor.ProveedorId;
                return View("Details", proveedorVM);
            }
            return View(proveedorVM);
        }

        //
        // GET: /Proveedores/Delete/5

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult Delete(int id)
        {
            ProveedorVM proveedorVM = new ProveedorVM();
            Proveedor proveedor = repository.LoadById(id);
            Mapper.Map(proveedor, proveedorVM);
            return View(proveedorVM);
        }

        //
        // POST: /Proveedores/Delete/5

        [HttpPost, ActionName("Delete")]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult DeleteConfirmed(int id)
        {
            ProveedorVM proveedorVM = new ProveedorVM();
            Proveedor proveedor = repository.LoadById(id);
            Mapper.Map(proveedor, proveedorVM);
            repository.Delete(proveedor);
            repository.Save();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}