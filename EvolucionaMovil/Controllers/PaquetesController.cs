using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using EvolucionaMovil.Models;
using EvolucionaMovil.Models.Enums;
using EvolucionaMovil.Repositories;

namespace EvolucionaMovil.Controllers
{
    public class PaquetesController : Controller
    {
        //Modificación prueba Karla
        private PaquetesRepository repository = new PaquetesRepository();

        //
        // GET: /PaqueteVMs/

        public ViewResult Index()
        {
            return View(repository.ListAll().ToListOfDestination<PaqueteVM>());
        }

        public ViewResult Buy()
        {
            //TODO:Obtener paycenterId de cache or something like that
            ViewData["Eventos"] = repository.GetEventosByPayCenter(1);
            var repositoryEstadoDeCuenta = new Repositories.EstadoDeCuentaRepository();
            var edocuenta = repositoryEstadoDeCuenta.ListAll();
            ViewData["SaldoActual"] = (edocuenta.Where(x => x.IsAbono).Sum(x => x.Monto) - edocuenta.Where(x => !x.IsAbono).Sum(x => x.Monto)).ToString("C");

            return View(repository.ListAll().Select(x => new PaqueteVM { Creditos = x.Creditos, PaqueteId = x.PaqueteId, PrecioString = x.Precio.ToString("C"), Precio = x.Precio, PrecioPorEvento = (x.Precio / x.Creditos).ToString("C") }));
        }

        [HttpPost]
        public ViewResult Buy(IEnumerable<PaqueteVM> model)
        {
            var selected = model.Where(x => x.Selected);
            //TODO:Pasar el payCenterId
            var payCenter = new PayCentersRepository(repository.context).LoadById(1);
            var cuenta = payCenter.Cuentas.Where(x => x.TipoCuenta == enumTipoCuenta.Pago_de_Servicios.GetHashCode()).FirstOrDefault();
            int cuentaId;
            if (cuenta != null)
            {
                cuentaId = cuenta.CuentaId;
            }
            else
            {
                throw new Exception("El Pay Center no está configurado para generar Pago de servicios");
            }
            //TODO:Validar que tenga el saldo suficiente. Quiero agregar un campo al PayCenter para determinar su saldo sin tener que recalcularlo
            foreach (var paquete in selected)
            {
                var p = repository.LoadById(paquete.PaqueteId);
                if (p.FechaVencimiento >= DateTime.Now)
                {
                    CompraEvento compraEvento = new CompraEvento
                    {
                        Consumidos = 0,
                        Eventos = p.Creditos,
                        FechaCreacion = DateTime.Now,
                        Monto = p.Precio,
                        PaqueteId = p.PaqueteId,
                        //TODO:pasar el paycenterid
                        PayCenterId = payCenter.PayCenterId
                    };
                    repository.Add(compraEvento);

                    cuenta.Movimientos.Add(new Movimiento
                    {
                        //TODO:Establecer formato de la clave
                        Clave = "99999",
                        CuentaId = cuentaId,
                        CuentaOrigenId = 0,
                        FechaCreacion = DateTime.Now,
                        Monto = p.Precio,
                        Motivo = (short)enumMotivo.Cargo.GetHashCode(),
                        PayCenterId = payCenter.PayCenterId,
                        Id = paquete.PaqueteId,
                        IsAbono=false,
                        Status = (short)enumEstatusMovimiento.Aplicado.GetHashCode()
                    });
                }
            }
            var exito = repository.Save();
            if (!exito)
            {
                //TODO:Ver como manejar excepciones en el guardado
                throw new Exception("No fue posible guardar");
            }
            return View();
        }
        //
        // GET: /PaqueteVMs/Details/5

        public ViewResult Details(int id)
        {
            PaqueteVM paqueteVM = new PaqueteVM();
            Paquete paquete = repository.LoadById(id);
            Mapper.Map(paquete, paqueteVM);
            return View(paqueteVM);
        }

        //
        // GET: /PaqueteVMs/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /PaqueteVMs/Create

        [HttpPost]
        public ActionResult Create(PaqueteVM paqueteVM)
        {
            Paquete paquete = new Paquete();
            if (ModelState.IsValid)
            {
                Mapper.Map(paqueteVM, paquete);
                repository.Add(paquete);
                repository.Save();
                return RedirectToAction("Index");
            }

            return View(paqueteVM);
        }

        //
        // GET: /PaqueteVMs/Edit/5

        public ActionResult Edit(int id)
        {
            PaqueteVM paqueteVM = new PaqueteVM();
            Paquete paquete = repository.LoadById(id);
            Mapper.Map(paquete, paqueteVM);
            return View(paqueteVM);
        }

        //
        // POST: /PaqueteVMs/Edit/5

        [HttpPost]
        public ActionResult Edit(PaqueteVM paqueteVM)
        {
            Paquete paquete = repository.LoadById(paqueteVM.PaqueteId);
            if (ModelState.IsValid)
            {
                Mapper.Map(paqueteVM, paquete);
                repository.Save();
                paqueteVM.PaqueteId = paquete.PaqueteId;
                return RedirectToAction("Index");
            }
            return View(paqueteVM);
        }

        //
        // GET: /PaqueteVMs/Delete/5

        public ActionResult Delete(int id)
        {
            PaqueteVM paqueteVM = new PaqueteVM();
            Paquete paquete = repository.LoadById(id);
            Mapper.Map(paquete, paqueteVM);
            return View(paqueteVM);
        }

        //
        // POST: /PaqueteVMs/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            PaqueteVM paqueteVM = new PaqueteVM();
            Paquete paquete = repository.LoadById(id);
            Mapper.Map(paquete, paqueteVM);
            repository.Delete(paquete);
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