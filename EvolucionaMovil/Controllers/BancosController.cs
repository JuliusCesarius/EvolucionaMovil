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
    public class BancosController : Controller
    {
        private BancosRepository repository = new BancosRepository();

        //
        // GET: /Bancos/

        public ViewResult Index()
        {
            var bancos = repository.ListAll().ToListOfDestination<BancoVM>();
            return View(bancos);
        }

        //
        // GET: /Bancos/Details/5

        public ViewResult Details(int id)
        {
            BancoVM bancoVM = new BancoVM();
            Banco banco = repository.LoadById(id);
            Mapper.Map(banco, bancoVM);
            return View(bancoVM);
        }

        //
        // GET: /Bancos/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Bancos/Create

        [HttpPost]
        public ActionResult Create(BancoVM bancoVM)
        {
            Banco banco = new Banco();
            if (ModelState.IsValid)
            {
                Mapper.Map(bancoVM, banco);
                repository.Add(banco);
                repository.Save();
                return RedirectToAction("Index");  
            }
            return View(bancoVM);
        }
        
        //
        // GET: /Bancos/Edit/5
 
        public ActionResult Edit(int id)
        {
            BancoVM bancoVM = new BancoVM();
            Banco banco = repository.LoadById(id);
            Mapper.Map(banco, bancoVM);
            return View(bancoVM);
        }

        //
        // POST: /Bancos/Edit/5

        [HttpPost]
        public ActionResult Edit(BancoVM bancoVM)
        {
            Banco banco = repository.LoadById(bancoVM.BancoId);
            if (ModelState.IsValid)
            {
                Mapper.Map(bancoVM, banco);
                repository.Save();
                bancoVM.BancoId = banco.BancoId;
                return RedirectToAction("Index");
            }
            return View(bancoVM);
        }

        //
        // GET: /Bancos/Delete/5
 
        public ActionResult Delete(int id)
        {
            Banco banco = repository.LoadById(id);
            BancoVM bancoVM = new BancoVM();
            Mapper.Map(banco, bancoVM);
            return View(bancoVM);
        }

        //
        // POST: /Bancos/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {           
            Banco banco = repository.LoadById(id);
            repository.Delete(banco);
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