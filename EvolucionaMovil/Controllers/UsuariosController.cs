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
    public class UsuariosController : Controller
    {
        private UsuariosRepository repository = new UsuariosRepository();

        //
        // GET: /Usuarios/

        public ViewResult Index()
        {
            var usuarios = repository.ListAll();
            return View(usuarios.ToListOfDestination<UsuarioVM>());
        }

        //
        // GET: /Usuarios/Details/5

        public ViewResult Details(int id)
        {
            UsuarioVM usuarioVM = new UsuarioVM();
            Usuario usuario = repository.LoadById(id);
            Mapper.Map(usuario, usuarioVM);
            return View(usuarioVM);
        }

        //
        // GET: /Usuarios/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Usuarios/Create

        [HttpPost]
        public ActionResult Create(UsuarioVM usuarioVM)
        {
            Usuario usuario = new Usuario();
            if (ModelState.IsValid)
            {
                Mapper.Map(usuarioVM, usuario);
                repository.Add(usuario);
                repository.Save();
                usuarioVM.UsuarioId = usuario.UsuarioId;
                return RedirectToAction("Index");  
            }
            return View(usuarioVM);
        }
        
        //
        // GET: /Usuarios/Edit/5
 
        public ActionResult Edit(int id)
        {
            UsuarioVM usuarioVM = new UsuarioVM();
            Usuario usuario = repository.LoadById(id);
            Mapper.Map(usuario, usuarioVM);
            return View(usuarioVM);
        }

        //
        // POST: /Usuarios/Edit/5

        [HttpPost]
        public ActionResult Edit(UsuarioVM usuarioVM)
        {
            Usuario usuario = repository.LoadById(usuarioVM.UsuarioId);
            if (ModelState.IsValid)
            {
                Mapper.Map(usuarioVM, usuario);
                repository.Save();
                usuarioVM.UsuarioId = usuario.UsuarioId;
                return RedirectToAction("Index");
            }
            return View(usuarioVM);
        }

        //
        // GET: /Usuarios/Delete/5
 
        public ActionResult Delete(int id)
        {
            UsuarioVM usuarioVM = new UsuarioVM();
            Usuario usuario = repository.LoadById(id);
            Mapper.Map(usuarioVM, usuario);
            return View(usuarioVM);
        }

        //
        // POST: /Usuarios/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            UsuarioVM usuarioVM = new UsuarioVM();
            Usuario usuario = repository.LoadById(id);
            Mapper.Map(usuario, usuarioVM);
            repository.Delete(usuario);
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