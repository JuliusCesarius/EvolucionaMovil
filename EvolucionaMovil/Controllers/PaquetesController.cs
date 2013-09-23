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
using EvolucionaMovil.Models.Classes;
using EvolucionaMovil.Attributes;
using cabinet.patterns.enums;
using EvolucionaMovil.Models.BR;
using EvolucionaMovil.Models.Extensions;
using EvolucionaMovil.Models.Helpers;
using System.Text;
using System.Configuration;

namespace EvolucionaMovil.Controllers
{
    public class PaquetesController : CustomControllerBase
    {
        //Modificación prueba Karla
        private PaquetesRepository repository = new PaquetesRepository();
        private const int PROVEEDOR_EVOLUCIONAMOVIL = 1;
        //
        // GET: /PaqueteVMs/
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff})]
        public ViewResult Index()
        {
            return View(repository.ListAll().ToListOfDestination<PaqueteVM>());
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter })]
        public ViewResult Buy()
        {
            EstadoCuentaBR estadoCuenta = new EstadoCuentaBR();
            var saldos = estadoCuenta.GetSaldosPagoServicio(PayCenterId);
            ViewData["Eventos"] = saldos.EventosDisponibles;
            ViewData["SaldoActual"] = saldos.SaldoDisponible.ToString("C");

            return View(repository.ListAll().Select(x => new PaqueteVM { Creditos = x.Creditos, PaqueteId = x.PaqueteId, Precio = x.Precio }));
        }

        public ViewResult QueSon()
        {
            return View();
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter })]
        public ViewResult Buy(IEnumerable<PaqueteVM> model)
        {
            Succeed = false;
            var selected = model.Where(x => x.Selected);
            EstadoCuentaBR estadoCuentaBR = new EstadoCuentaBR(repository.context);
            var saldos = estadoCuentaBR.GetSaldosPagoServicio(PayCenterId);
            ViewData["Eventos"] = saldos.EventosDisponibles;
            ViewData["SaldoActual"] = saldos.SaldoDisponible.ToString("C");
            if (selected.Count() == 0)
            {
                AddValidationMessage(enumMessageType.BRException, "No ha seleccionado ningún paquete");
                return View(model);
            }

            decimal totalCompra = 0;
            foreach (var paquete in selected)
            {
                var precioPaquete = repository.GetPrecioByPaqueteId(paquete.PaqueteId);
                totalCompra += precioPaquete;
            }
            if (saldos.SaldoDisponible < totalCompra)
            {
                AddValidationMessage(enumMessageType.BRException, "No cuenta con saldo suficiente para comprar más paquetes");
                return View(model);
            }
            ViewData["TotalCompra"] = totalCompra.ToString("C");
            ViewData["EventosFinales"] = saldos.EventosDisponibles + selected.Sum(x=>x.Creditos);
            ViewData["SaldoActualFinal"] = (saldos.SaldoDisponible - selected.Sum(x => x.Precio)).ToString("C");
            return View("Confirm", selected);
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter })]
        public ViewResult Confirm(IEnumerable<PaqueteVM> model)
        {
            Succeed = false;
            //TODO:Validar que tenga el saldo suficiente. Quiero agregar un campo al PayCenter para determinar su saldo sin tener que recalcularlo
            PaycenterBR payCenterBR = new PaycenterBR();
            var cuentaId = payCenterBR.GetOrCreateCuentaPayCenter(PayCenterId, enumTipoCuenta.Pago_de_Servicios, PROVEEDOR_EVOLUCIONAMOVIL);

            EstadoCuentaBR estadoCuentaBR = new EstadoCuentaBR(repository.context);
            decimal totalCompra = 0;
            List<CompraEvento> paquetesComprados = new List<CompraEvento>();
            foreach (var paquete in model)
            {
                //TODO:Esta validación debería estar en un BR aparte
                var p = repository.LoadById(paquete.PaqueteId);
                if (p == null)
                {
                    AddValidationMessage(enumMessageType.BRException, "No se ha encontrado el paquete de " + paquete.Creditos.ToString() + "créditos");
                    break;
                }
                if (p.FechaVencimiento >= DateTime.UtcNow.GetCurrentTime())
                {
                    totalCompra += p.Creditos;
                    CompraEvento compraEvento = new CompraEvento
                    {
                        Consumidos = 0,
                        Eventos = p.Creditos,
                        FechaCreacion = DateTime.UtcNow.GetCurrentTime(),
                        Monto = p.Precio,
                        PaqueteId = p.PaqueteId,
                        PayCenterId = PayCenterId
                    };
                    //Agrego a la lista de paquetes que se van a agregar
                    paquetesComprados.Add(compraEvento);

                    //Agrego al repositorio
                    repository.Add(compraEvento);
                    estadoCuentaBR.CrearMovimiento(PayCenterId, enumTipoMovimiento.Cargo, 0, cuentaId, p.Precio, enumMotivo.Compra, PayCenterName, enumEstatusMovimiento.Aplicado);
                }
            }
            var saldos = estadoCuentaBR.GetSaldosPagoServicio(PayCenterId);
            if (saldos.SaldoDisponible < totalCompra)
            {
                AddValidationMessage(enumMessageType.BRException, "No cuenta con saldo suficiente para comprar más paquetes");
                return View(model);
            }
            Succeed = repository.Save();
            ViewBag.Succeed = Succeed;
            if (Succeed)
            {
                AddValidationMessage(enumMessageType.Succeed, "Se ha realizado la compra de " + totalCompra + " créditos exitosamente.");

                //Julius: Permite avisar a los emails configurados en el momento que se realizó la compra del paquete
                StringBuilder emailMessage = new StringBuilder();
                emailMessage.AppendLine("<p>El Paycenter <b>" + this.PayCenterName + "</b> ha realizado la compra de:<p>");
                emailMessage.AppendLine("<table>");
                paquetesComprados.ForEach(x =>
                {
                    emailMessage.AppendLine("<tr>");
                    emailMessage.AppendLine("<td>Paquete: <b>PAQ" + x.PaqueteId + "</b></td>");
                    emailMessage.AppendLine("<td>Eventos: <b>" + x.Eventos.ToString() + "</b></td>");
                    emailMessage.AppendLine("<td>Monto: <b>" + x.Monto.ToString("C") + "</b></td>");
                    emailMessage.AppendLine("</tr>");
                });
                emailMessage.AppendLine("<p>Fecha de compra: " + DateTime.Now.GetCurrentTime().ToString() + "</p>");
                emailMessage.AppendLine("</table>");
                var paquetesEmail = ConfigurationManager.AppSettings.Get("PaquetesEmail");
                EmailHelper.Enviar(emailMessage.ToString(), "Compra Paquete - " + this.PayCenterName, paquetesEmail);
            }
            else
            {
                //TODO: Leer de los mensajes que vengan del save
                //ValidationMessages = repository.ValidationMessages
                AddValidationMessage(enumMessageType.UnhandledException, "No fue posible realizar la compra. Intente más tarde");
            }
            return View(model);
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff })]
        public ViewResult Details(int id)
        {
            PaqueteVM paqueteVM = new PaqueteVM();
            Paquete paquete = repository.LoadById(id);
            Mapper.Map(paquete, paqueteVM);
            return View(paqueteVM);
        }

        //
        // GET: /PaqueteVMs/Create
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /PaqueteVMs/Create

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
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
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
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
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
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
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
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
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
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