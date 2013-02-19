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

        public ViewResult Details(int id)
        {
            //Cuenta cuenta = repository.LoadById(id);
            ReporteDepositoVM reporteDepositoVM = new ReporteDepositoVM();
            //Mapper.Map(cuenta, cuentaVM);
            return View(reporteDepositoVM);
        }
        
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