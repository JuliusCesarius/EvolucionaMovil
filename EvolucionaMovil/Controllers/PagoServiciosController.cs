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
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Controllers
{ 
    public class PagoServiciosController : Controller
    {
        private PagoServiciosRepository repository = new PagoServiciosRepository();

        //
        // GET: /PagoServicios/

        public ViewResult Index()
        {
            return View(getPagosServicio(null).Result);
        }

        [HttpPost]
        public string GetPagoServicios(ServiceParameterVM parameters)
        {
            var pagoServiciosResult = getPagosServicio(parameters);

            return Newtonsoft.Json.JsonConvert.SerializeObject(pagoServiciosResult);
        }

        //
        // GET: /PagoServicios/Details/5

        public ViewResult Details(int id)
        {
            Pago pago = repository.LoadById(id);
            PagoVM pagoVM = new PagoVM();
            Mapper.Map(pago, pagoVM);
            return View(pagoVM);
        }

        //
        // GET: /PagoServicios/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /PagoServicios/Create

        [HttpPost]
        public ActionResult ReportarPago(PagoVM pagoVM)
        {
            Pago pago = new Pago();
            if (ModelState.IsValid)
            {
                Mapper.Map(pagoVM, pago);
                repository.Add(pago);
                repository.Save();
                return RedirectToAction("Index");
                //Eliminar este comentario
            }

            return View(pagoVM);
        }
        
        //
        // GET: /PagoServicios/Edit/5
 
        public ActionResult Edit(int id)
        {
            PagoVM pagoVM = new PagoVM();
            Pago pago = repository.LoadById(id);
            Mapper.Map(pago, pagoVM);
            return View(pagoVM);
        }

        //
        // POST: /PagoServicios/Edit/5

        [HttpPost]
        public ActionResult Edit(PagoVM pagoVM)
        {
            Pago pago = repository.LoadById(pagoVM.PagoId);
            if (ModelState.IsValid)
            {
                Mapper.Map(pagoVM, pago);
                repository.Save();
                pagoVM.PagoId = pago.PagoId;
                return RedirectToAction("Index");
            }
            return View(pagoVM);
        }

        //
        // GET: /PagoServicios/Delete/5
 
        public ActionResult Delete(int id)
        {
            PagoVM pagoVM = new PagoVM();
            Pago pago = repository.LoadById(id);
            Mapper.Map(pago, pagoVM);
            return View(pagoVM);
        }

        //
        // POST: /PagoServicios/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Pago pago = repository.LoadById(id);
            repository.Delete(pago);
            repository.Save();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        private SimpleGridResult<PagoServicioVM> getPagosServicio(ServiceParameterVM Parameters = null)
        {
            var pagos = repository.ListAll();
            EstadoDeCuentaRepository estadoDeCuentaRepository = new EstadoDeCuentaRepository();
            SimpleGridResult<PagoServicioVM> simpleGridResult = new SimpleGridResult<PagoServicioVM>();
            var pagosServicioVM = pagos.Where(x => Parameters == null
                || (Parameters.fechaInicio == null || (Parameters.fechaInicio < x.FechaCreacion)
                    && (Parameters.fechaFin == null || Parameters.fechaFin > x.FechaCreacion)
                )
                ).Select(x => new PagoServicioVM
                {
                    PayCenterId = x.PayCenterId,
                    Folio = x.Ticket != null ? x.Ticket.Folio:string.Empty,
                    Servicio = x.Servicio,
                    NombreCliente = x.ClienteNombre,
                    PayCenterName = x.PayCenter!=null?x.PayCenter.Nombre:"[Desconocido]",
                    PagoId = x.PagoId,
                    //todo:Optimizar esta consulta para que no haga un load por cada registro que levante.
                    Comentarios = estadoDeCuentaRepository.LoadById(x.MovimientoId).Movimientos_Estatus.Any() ? estadoDeCuentaRepository.LoadById(x.MovimientoId).Movimientos_Estatus.Last().Comentarios : string.Empty,
                    Monto = x.Importe.ToString("C"),
                    FechaCreacion = x.FechaCreacion.ToShortDateString(),
                    FechaVencimiento = x.FechaVencimiento.ToShortDateString(),
                    Status = ((enumEstatusMovimiento)x.Status).ToString()
                });
            //TODO:Leer Eventos del paycenter
            ViewData["Eventos"] = 56;
            //TODO:Checar una mejor forma de traer el saldo (Caché o algo)
            var repositoryEstadoDeCuenta = new Repositories.EstadoDeCuentaRepository();
            var edocuenta = repositoryEstadoDeCuenta.ListAll();
            ViewData["SaldoActual"] = (edocuenta.Where(x => x.IsAbono).Sum(x => x.Monto) - edocuenta.Where(x => !x.IsAbono).Sum(x => x.Monto)).ToString("C");

            if (Parameters != null)
            {
                simpleGridResult.CurrentPage = Parameters.pageNumber;
                simpleGridResult.PageSize = Parameters.pageSize;
                if (Parameters.pageSize > 0)
                {
                    var pageNumber = Parameters.pageNumber >= 0 ? Parameters.pageNumber : 0;
                    simpleGridResult.CurrentPage = pageNumber;
                    simpleGridResult.TotalRows = pagosServicioVM.Count();
                    pagosServicioVM = pagosServicioVM.Skip(pageNumber * Parameters.pageSize).Take(Parameters.pageSize);
                }
            }
            simpleGridResult.Result = pagosServicioVM;

            return simpleGridResult;
        }


    }
}
