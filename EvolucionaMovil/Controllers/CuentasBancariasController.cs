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
using EvolucionaMovil.Attributes;
using EvolucionaMovil.Models.Classes;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Controllers
{ 
    public class CuentasBancariasController : CustomControllerBase
    {
        private CuentasBancariasRepository repository = new CuentasBancariasRepository();

        //
        // GET: /CuentasBancarias/
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ViewResult Index()
        {
            var cuentasBancarias = repository.ListAll().ToListOfDestination<CuentaBancariaVM>();
            var proveedores = new ProveedoresRepository().ListAll().ToListOfDestination<ProveedorVM>();
            ViewBag.Proveedores = proveedores.OrderBy(x=>x.Nombre);
            ViewBag.Bancos = new BancosRepository().ListAll().ToListOfDestination<BancoVM>();
            return View(cuentasBancarias);
        }

        //
        // GET: /CuentasBancarias/Details/5

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ViewResult Details(int id)
        {
            CuentaBancariaVM cuentaBancariaVM = new CuentaBancariaVM();
            CuentaBancaria cuentaBancaria = repository.LoadById(id);
            Mapper.Map(cuentaBancaria, cuentaBancariaVM);
            return View(cuentaBancariaVM);
        }

        //
        // GET: /CuentasBancarias/Create

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public PartialViewResult Create()
        {
            return PartialView(new CuentaBancariaVM());
        }
        
        //
        // POST: /CuentasBancarias/Create

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult Create(CuentaBancariaVM cuentaBancariaVM)
        {
            CuentaBancaria cuentaBancaria = new CuentaBancaria();
            //Valido que al menos tenga uno de estos campos
            if (cuentaBancariaVM.ClabeInterbancaria == null &&
                cuentaBancariaVM.NumeroCuenta == null &&
                cuentaBancariaVM.NumeroDeTarjeta == null)
            {
                ModelState.AddModelError("NumeroCuenta", "Debe de especificarse al menos un Número de Cuenta");
            }
            if (ModelState.IsValid)
            {
                Mapper.Map(cuentaBancariaVM, cuentaBancaria);
                repository.Add(cuentaBancaria);
                repository.Save();
                cuentaBancariaVM.CuentaId = cuentaBancaria.CuentaId;
                return PartialView("_Details", cuentaBancariaVM);
            }
            return PartialView(cuentaBancariaVM);
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult _Edit(int id)
        {
            CuentaBancariaVM cuentaBancariaVM = new CuentaBancariaVM();
            var cuentaBancaria = repository.LoadById(id);
            Mapper.Map(cuentaBancaria, cuentaBancariaVM);
            return PartialView(cuentaBancariaVM);
        }



        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public JsonResult SaveConfig(ConfigProvCuentaVM form)
        {
            var proveedoresConfigurados = form.Proveedores.Where(x => x.Selected).Select(x => x.ProveedorId).ToList();
            repository.SaveConfigCuentaProveedores(form.CuentaBancariaId, proveedoresConfigurados,true);
            ConfigProvCuentaResponseVM configProvCuentaResponseVM = new ConfigProvCuentaResponseVM
            {
                CuentaBancariaId = form.CuentaBancariaId,
                Proveedores = proveedoresConfigurados
            };
            return this.Json(configProvCuentaResponseVM);
        }
                
        //
        // GET: /CuentasBancarias/Edit/5

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult Edit(int id)
        {
            CuentaBancariaVM cuentaBancariaVM = new CuentaBancariaVM();
            CuentaBancaria cuentaBancaria = repository.LoadById(id);
            Mapper.Map(cuentaBancaria, cuentaBancariaVM);
            return View(cuentaBancariaVM);
        }

        //
        // POST: /CuentasBancarias/Edit/5

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult _Edit(CuentaBancariaVM cuentaBancariaVM)
        {
            CuentaBancaria cuentaBancaria = repository.LoadById(cuentaBancariaVM.CuentaId);
            if (ModelState.IsValid)
            {
                Mapper.Map(cuentaBancariaVM, cuentaBancaria);
                repository.Save();
                cuentaBancariaVM.CuentaId = cuentaBancaria.CuentaId;
                return PartialView("_Details", cuentaBancariaVM);
            }
            return View(cuentaBancariaVM);
        }

        //
        // GET: /CuentasBancarias/Delete/5
 
        public ActionResult Delete(int id)
        {
            CuentaBancaria cuentaBancaria = repository.LoadById(id);
            CuentaBancariaVM cuentaBancariaVM = new CuentaBancariaVM();
            Mapper.Map(cuentaBancaria, cuentaBancariaVM);
            return View(cuentaBancariaVM);
        }

        //
        // POST: /CuentasBancarias/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {           
            CuentaBancaria cuentaBancaria = repository.LoadById(id);
            repository.Delete(cuentaBancaria);
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