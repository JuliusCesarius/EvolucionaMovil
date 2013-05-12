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
using System.Globalization;
using System.Threading;
using EvolucionaMovil.Models.Classes;
using EvolucionaMovil.Attributes;
using EvolucionaMovil.Models.Extensions;

namespace EvolucionaMovil.Controllers
{ 
    public class EstadoDeCuentaController : CustomControllerBase
    {
        private List<string> Mensajes = new List<string>();
        private EstadoDeCuentaRepository repository = new EstadoDeCuentaRepository();
        private EstadoDeCuentaRepository _tempEstadoDeCuentaRepository;
        private EstadoDeCuentaRepository TempEstadoDeCuentaRepository
        {
            get
            {
                if (_tempEstadoDeCuentaRepository == null)
                {
                    _tempEstadoDeCuentaRepository = new EstadoDeCuentaRepository();
                }
                return _tempEstadoDeCuentaRepository;
            }
        }
        private decimal _saldo = 0;
        private CultureInfo ci = new CultureInfo("es-MX");
        //
        // GET: /EstadoDeCuenta/
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public ViewResult Index()
        {

            ViewBag.PageSize = 10;
            ViewBag.PageNumber = 0;
            ViewBag.SearchString = string.Empty;
            ViewBag.fechaInicio = string.Empty;
            ViewBag.FechaFin = string.Empty;
            ViewBag.OnlyAplicados = false;

            return View(getEstadoDeCuenta(new ServiceParameterVM { pageNumber = 0, pageSize = 10 }));
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public ViewResult Index(ServiceParameterVM parameters)
        {
            ViewBag.PageSize = parameters.pageSize;
            ViewBag.PageNumber = parameters.pageNumber;
            ViewBag.SearchString = parameters.searchString;
            ViewBag.fechaInicio = parameters.fechaInicio != null ? ((DateTime)parameters.fechaInicio).ToShortDateString() : "";
            ViewBag.FechaFin = parameters.fechaFin != null ? ((DateTime)parameters.fechaFin).ToShortDateString() : "";
            ViewBag.OnlyAplicados = parameters.onlyAplicados;
            ViewBag.PayCenterId = parameters.PayCenterId;
            ViewBag.PayCenterName = parameters.PayCenterName;

            return View(getEstadoDeCuenta(parameters));
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public ViewResult Details(int id)
        {
            //Cuenta cuenta = repository.LoadById(id);
            ReporteDepositoVM reporteDepositoVM = new ReporteDepositoVM();
            //Mapper.Map(cuenta, cuentaVM);
            return View(reporteDepositoVM);
        }

        #region Privates

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        private string getConceptoString(Movimiento Movimiento)
        {
            //todo:Devolver formateado el concepto según el motivo
            var usuarioOriginal = Movimiento.UserName != null?Movimiento.UserName:"Desconocido";
            return usuarioOriginal + " - " + ((enumMotivo)Movimiento.Motivo).ToString();
        }

        private SimpleGridResult<EstadoCuentaVM> getEstadoDeCuenta(ServiceParameterVM Parameters = null)
        {
            IEnumerable<Movimiento> movimientos;
            if (PayCenterId == 0)
            {
                movimientos = repository.GetMovimientos();
            }
            else
            {
                movimientos = repository.GetMovimientosByPayCenterId(PayCenterId);
            }

            SimpleGridResult<EstadoCuentaVM> simpleGridResult = new SimpleGridResult<EstadoCuentaVM>();
            var estadosDeCuentaVM = movimientos.Where(x =>
                    ( Parameters == null || (                            
                                (Parameters.fechaInicio == null || (Parameters.fechaInicio < x.FechaCreacion))
                        && (Parameters.fechaFin == null || Parameters.fechaFin > x.FechaCreacion)
                        && (Parameters.searchString == null || (x.UserName.ContainsInvariant(Parameters.searchString) || x.Clave.ContainsInvariant(Parameters.searchString) || ((enumMotivo)x.Motivo).ToString().ContainsInvariant(Parameters.searchString) || ((enumEstatusMovimiento)x.Status).ToString().ContainsInvariant(Parameters.searchString)))
                        && (Parameters.onlyAplicados?x.Status == enumEstatusMovimiento.Aplicado.GetHashCode():true)
                        )
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
                    Comentarios = getComentarioCambioEstatus(x),
                    Concepto = getConceptoString(x),
                    Abono = x.IsAbono ? x.Monto.ToString("C3", ci) : string.Empty,
                    Cargo = !x.IsAbono ? x.Monto.ToString("C3", ci) : string.Empty,
                    Saldo = ((enumEstatusMovimiento)x.Status) == enumEstatusMovimiento.Aplicado ? getSaldoAcumulado(x.IsAbono, x.Monto).ToString("C3", ci) : "-",
                    FechaCreacion = x.FechaCreacion.ToShortDateString(),
                    Status = ((enumEstatusMovimiento)x.Status).ToString()
                    //,
                    //HistorialEstatusVM = Comentarios(x.MovimientoId)
                    //
                });

            //Thread.CurrentThread.CurrentCulture; ;es-MX
            //  ci.NumberFormat.CurrencySymbol = "$";
            //TODO:Leer Eventos del paycenter
            ViewData["Eventos"] = 56;
            if (Parameters == null || !Parameters.onlyAplicados)
            {
                ViewData["SaldoActual"] = (movimientos.Where(x => x.IsAbono).Sum(x => x.Monto) - movimientos.Where(x => !x.IsAbono).Sum(x => x.Monto)).ToString("C3", ci);
            }
            else
            {
                ViewData["SaldoActual"] = (movimientos.Where(x => x.IsAbono && x.Status == enumEstatusMovimiento.Aplicado.GetHashCode()).Sum(x => x.Monto) - movimientos.Where(x => !x.IsAbono && x.Status == enumEstatusMovimiento.Aplicado.GetHashCode()).Sum(x => x.Monto)).ToString("C3", ci);
            }

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

        private string getComentarioCambioEstatus(Movimiento Movimiento)
        {
            //todo:Julius, tuve que recurrir a un repositorio temporal porque marca error en el data reader en producción. Investigar si se puede levantar el detalle de comentarios en el mismo repository
            var lastComment = TempEstadoDeCuentaRepository.GetUltimoCambioEstatus((enumMotivo)Movimiento.Motivo, Movimiento.Id);
            if (lastComment != null)
            {
                return lastComment.Comentarios;
            }
            else
            {
                return string.Empty;
            }
        }

        private decimal getSaldoAcumulado(bool IsAbono, decimal Monto)
        {
            _saldo = _saldo + (IsAbono ? Monto:-Monto);
            return _saldo;
        }

        #endregion
    }
}