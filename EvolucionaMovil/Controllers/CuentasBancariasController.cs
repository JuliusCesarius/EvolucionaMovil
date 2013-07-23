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
    public class CuentasBancariasController : Controller
    {
        private CuentasBancariasRepository repository = new CuentasBancariasRepository();

        //
        // GET: /CuentasBancarias/

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

        public ViewResult Details(int id)
        {
            CuentaBancariaVM cuentaBancariaVM = new CuentaBancariaVM();
            CuentaBancaria cuentaBancaria = repository.LoadById(id);
            Mapper.Map(cuentaBancaria, cuentaBancariaVM);
            return View(cuentaBancariaVM);
        }

        //
        // GET: /CuentasBancarias/Create

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
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
        // POST: /CuentasBancarias/Create

        [HttpPost]
        public ActionResult Create(CuentaBancariaVM cuentaBancariaVM)
        {
            CuentaBancaria cuentaBancaria = new CuentaBancaria();
            if (ModelState.IsValid)
            {
                Mapper.Map(cuentaBancariaVM, cuentaBancaria);
                repository.Add(cuentaBancaria);
                repository.Save();
                return RedirectToAction("Index");  
            }
            return View(cuentaBancariaVM);
        }
        
        //
        // GET: /CuentasBancarias/Edit/5
 
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
        public ActionResult Edit(CuentaBancariaVM cuentaBancariaVM)
        {
            CuentaBancaria cuentaBancaria = repository.LoadById(cuentaBancariaVM.CuentaId);
            if (ModelState.IsValid)
            {
                Mapper.Map(cuentaBancariaVM, cuentaBancaria);
                repository.Save();
                cuentaBancariaVM.CuentaId = cuentaBancaria.CuentaId;
                return RedirectToAction("Index");
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