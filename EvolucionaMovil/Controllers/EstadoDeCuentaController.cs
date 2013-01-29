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
using System.Collections;
using EvolucionaMovil.Models.BR;

namespace EvolucionaMovil.Controllers
{ 
    public class EstadoDeCuentaController : Controller
    {
        private List<string> Mensajes = new List<string>();
        private EstadoDeCuentaRepository repository = new EstadoDeCuentaRepository();
        private decimal _saldo = 0;
        private EstadoCuentaBR validations = new EstadoCuentaBR();
        //
        // GET: /EstadoDeCuenta/

        public ViewResult Index()
        {
            return View(getEstadoDeCuenta(new ServiceParameterVM { pageNumber = 0, pageSize = 20 }).Result);
        }
        [HttpPost]
        public string GetEstadoCuenta(ServiceParameterVM parameters)
        {
            var estadoCuentaResult = getEstadoDeCuenta(parameters);

            return Newtonsoft.Json.JsonConvert.SerializeObject(estadoCuentaResult);
        }

        ////
        //// GET: /EstadoDeCuenta/Details/5

        public ViewResult Details(int id)
        {
            //Cuenta cuenta = repository.LoadById(id);
            ReporteDepositoVM reporteDepositoVM = new ReporteDepositoVM();
            //Mapper.Map(cuenta, cuentaVM);
            return View(reporteDepositoVM);
        }

        ////
        //// GET: /EstadoDeCuenta/Create

        public ActionResult ReportarDeposito()
        {
            PayCentersRepository payCentersRepository = new PayCentersRepository();
            //ViewBag.PayCenterId = new SelectList(new PayCentersRepository().ListAll(), "PayCenterId", "IFE");
            ReporteDepositoVM model = new ReporteDepositoVM();
            model.CuentasDeposito = payCentersRepository.LoadTipoCuentas(1).Select(x => new CuentaDepositoVM { CuentaId = x.CuentaId, Monto = 0, Nombre = ((enumTipoCuenta)x.TipoCuenta).ToString().Replace('_', ' ') }).ToList();
            BancosRepository bancosRepository = new BancosRepository();
            var bancos = bancosRepository.ListAll().Where(x => x.CuentasBancarias.Count > 0);
            ViewBag.Bancos = bancos;
            ViewBag.Cuentas = bancos.SelectMany(x => x.CuentasBancarias).Select(x => new { BancoId = x.BancoId, CuentaId = x.CuentaId, NumeroCuenta = x.NumeroCuenta, Titular = x.Titular });
            return View(model);
        }

        [HttpPost]
        public ActionResult ReportarDeposito(ReporteDepositoVM model)
        {
            bool exito = false;
            exito = validations.isValidReference(model.Referencia, model.BancoId);
            if (!exito)
            {
                Mensajes.Add("La referencia especificada ya existe en el sistema. Favor de verificarla.");
            }

            if (ModelState.IsValid && exito)
            {                
                Abono abono = new Abono
                {
                    BancoId = model.BancoId,
                    CuentaBancariaId = model.CuentaId,
                    EstatusId = enumEstatusMovimiento.Procesando.GetHashCode(),
                    FechaCreacion = DateTime.Now,
                    FechaPago = (DateTime)model.Fecha,
                    Monto = (Decimal)model.Monto,
                    PayCenterId = 1,
                    Referencia = model.Referencia
                };                
                repository.AddAbono(abono);
                if (model.CuentasDeposito.Count == 1)
                {
                    model.CuentasDeposito.First().Monto = (decimal)model.Monto;
                }
                else
                {
                    if (model.CuentasDeposito.Sum(x => x.Monto) == 0)
                    {
                        model.CuentasDeposito.First().Monto = (decimal)model.Monto;
                    }
                }

                foreach (var cuentaDepositoVM in model.CuentasDeposito.Where(x=>x.Monto>0))
                {
                    Movimiento movimiento = new Movimiento();
                    //todo:ver como generar la clave de los movimientos
                    movimiento.Clave = DateTime.Now.ToString("yyyyMMdd");
                    movimiento.CuentaId = cuentaDepositoVM.CuentaId;
                    movimiento.FechaCreacion = DateTime.Now;
                    movimiento.IsAbono = true;
                    movimiento.Monto = cuentaDepositoVM.Monto;
                    movimiento.Motivo = (Int16)enumMotivo.Abono;
                    movimiento.PayCenterId = 1;
                    movimiento.Status = (Int16)enumEstatusMovimiento.Procesando;

                    repository.Add(movimiento);
                }

                exito = repository.Save();
            }
            if (exito)
            {
                return RedirectToAction("Index");
            }
            else
            {
                BancosRepository bancosRepository = new BancosRepository();
                var bancos = bancosRepository.ListAll().Where(x => x.CuentasBancarias.Count > 0);
                ViewBag.Bancos = bancos;
                ViewBag.Cuentas = bancos.SelectMany(x => x.CuentasBancarias).Select(x => new { BancoId = x.BancoId, CuentaId = x.CuentaId, NumeroCuenta = x.NumeroCuenta, Titular = x.Titular });
                Mensajes.Add("No fue posible guardar el reporte de depósito.");
                ViewBag.Mensajes = Mensajes;
                return View(model);
            }
        }

        ////
        //// POST: /EstadoDeCuenta/Create

        //[HttpPost]
        //public ActionResult Create(CuentaVM cuentaVM)
        //{
        //    Cuenta cuenta = new Cuenta();
        //    if (ModelState.IsValid)
        //    {
        //        Mapper.Map(cuentaVM, cuenta);
        //        repository.Add(cuenta);
        //        repository.Save();
        //        return RedirectToAction("Index");  
        //    }

        //    ViewBag.PayCenterId = new SelectList(new PayCentersRepository().ListAll(), "PayCenterId", "IFE", cuenta.PayCenterId);
        //    return View(cuentaVM);
        //}
        
        ////
        //// GET: /EstadoDeCuenta/Edit/5
 
        //public ActionResult Edit(int id)
        //{
        //    Cuenta cuenta = repository.LoadById(id);
        //    ViewBag.PayCenterId = new SelectList(new PayCentersRepository().ListAll(), "PayCenterId", "IFE", cuenta.PayCenterId);
        //    return View(cuenta);
        //}

        ////
        //// POST: /EstadoDeCuenta/Edit/5

        //[HttpPost]
        //public ActionResult Edit(CuentaVM cuentaVM)
        //{
        //    Cuenta cuenta = repository.LoadById(cuentaVM.CuentaId);
        //    if (ModelState.IsValid)
        //    {
        //        Mapper.Map(cuentaVM, cuenta);
        //        repository.Save();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.PayCenterId = new SelectList(new PayCentersRepository().ListAll(), "PayCenterId", "IFE", cuenta.PayCenterId);
        //    return View(cuenta);
        //}

        ////
        //// GET: /EstadoDeCuenta/Delete/5
 
        //public ActionResult Delete(int id)
        //{
        //    CuentaVM cuentaVM = new CuentaVM();
        //    Cuenta cuenta = repository.LoadById(id);
        //    Mapper.Map(cuenta, cuentaVM);
        //    return View(cuenta);
        //}

        ////
        //// POST: /EstadoDeCuenta/Delete/5

        //[HttpPost, ActionName("Delete")]
        //public ActionResult DeleteConfirmed(int id)
        //{            
        //    Cuenta cuenta = repository.LoadById(id);
        //    repository.Delete(cuenta);
        //    repository.Save();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        private string getConceptoString(enumMotivo Motivo, int id)
        {
            //todo:Devolver formateado el concepto según el motivo
            return Motivo.ToString();
        }

        private SimpleGridResult<EstadoCuentaVM> getEstadoDeCuenta(ServiceParameterVM Parameters = null)
        {
            var movimientos = repository.GetMovimientos();
            SimpleGridResult<EstadoCuentaVM> simpleGridResult = new SimpleGridResult<EstadoCuentaVM>();
            var estadosDeCuentaVM = movimientos.Where(x=>Parameters==null
                || (Parameters.fechaInicio== null ||(Parameters.fechaInicio < x.FechaCreacion)
                    && (Parameters.fechaFin == null || Parameters.fechaFin > x.FechaCreacion)
                )
                ).Select(x => new EstadoCuentaVM
            {
                PayCenterId = x.PayCenterId,
                Motivo = ((enumMotivo)x.Motivo).ToString(),
                CuentaId = x.CuentaId,
                MovimientoId = x.MovimientoId,
                Id = x.Id,
                CuentaOrigenId = x.CuentaOrigenId,
                Clave = x.Clave,
                Comentarios = "TODO:Comentarios",
                Concepto = getConceptoString((enumMotivo)x.Motivo, x.Id),
                Abono = x.IsAbono ? x.Monto.ToString("C") : string.Empty,
                Cargo = !x.IsAbono ? x.Monto.ToString("C") : string.Empty,
                Saldo = ((enumEstatusMovimiento)x.Status) == enumEstatusMovimiento.Aplicado ? getSaldoAcumulado(x.IsAbono, x.Monto).ToString("C") : "-",
                FechaCreacion = x.FechaCreacion.ToShortDateString(),
                Status = ((enumEstatusMovimiento)x.Status).ToString(),
            });
            //TODO:Leer Eventos del paycenter
            ViewData["Eventos"] = 56;
            ViewData["SaldoActual"] = (movimientos.Where(x => x.IsAbono).Sum(x => x.Monto) - movimientos.Where(x => !x.IsAbono).Sum(x => x.Monto)).ToString("C");

            if (Parameters != null)
            {
                simpleGridResult.CurrentPage = Parameters.pageNumber;
                simpleGridResult.PageSize = Parameters.pageSize;
                if (Parameters.pageSize > 0)
                {
                    var pageNumber = Parameters.pageNumber >= 0 ? Parameters.pageNumber : 0;
                    simpleGridResult.CurrentPage = pageNumber;
                    simpleGridResult.TotalRows = estadosDeCuentaVM.Count();
                    estadosDeCuentaVM = estadosDeCuentaVM.Skip(pageNumber * Parameters.pageSize).Take(Parameters.pageSize);
                }
            }
            simpleGridResult.Result = estadosDeCuentaVM;

            return simpleGridResult;
        }

        private decimal getSaldoAcumulado(bool IsAbono, decimal Monto)
        {
            _saldo = _saldo + (IsAbono ? Monto:-Monto);
            return _saldo;
        }
    }
}