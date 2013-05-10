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

namespace EvolucionaMovil.Controllers
{ 
    public class EstadoDeCuentaController : CustomControllerBase
    {
        private List<string> Mensajes = new List<string>();
        private EstadoDeCuentaRepository repository = new EstadoDeCuentaRepository();
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
            ViewBag.FechaFin = parameters.fechaInicio != null ? ((DateTime)parameters.fechaFin).ToShortDateString() : "";
            ViewBag.OnlyAplicados = parameters.onlyAplicados;
            ViewBag.PayCenterId = parameters.PayCenterId;
            ViewBag.PayCenterName = parameters.PayCenterName;

            return View(getEstadoDeCuenta(parameters));
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public string GetEstadoCuenta(ServiceParameterVM parameters)
        {
            var estadoCuentaResult = getEstadoDeCuenta(parameters);
            return Newtonsoft.Json.JsonConvert.SerializeObject(estadoCuentaResult);
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public ViewResult Details(int id)
        {
            //Cuenta cuenta = repository.LoadById(id);
           
         Movimiento movimiento =   repository.LoadById(id);
         PayCentersRepository PayCentersRepository = new PayCentersRepository();
         PayCenter PayCenter = PayCentersRepository.LoadById(movimiento.PayCenterId);

           int cuentaId = movimiento.Cuenta.CuentaId ;
           string cuenta  = ((enumTipoCuenta) movimiento.Cuenta.TipoCuenta).ToString();
            string motivo =((enumMotivo ) movimiento.Motivo).ToString(); 
              
            BancosRepository BancoRepository = new BancosRepository();
            //BancoRepository.LoadById();

            EstadoCuentaVM EstadoCuentaVM = new EstadoCuentaVM()
            {
               Clave = movimiento.Clave,
                MontoString = movimiento.Monto.ToString("C3"),
                FechaCreacion = movimiento.FechaCreacion.ToString(),
                Status = ((enumEstatusMovimiento)movimiento.Status).ToString(),
                Cuenta = cuenta,
               PayCenter  = PayCenter.Nombre,
                Motivo = ((enumMotivo)movimiento.Motivo).ToString(),
                Saldo = (movimiento.SaldoActual!= null ?((decimal)movimiento.SaldoActual).ToString("C3"): "0"),
                isAbono = movimiento.IsAbono ,
                HistorialEstatusVM = movimiento.Movimientos_Estatus.OrderByDescending(x => x.FechaCreacion).Where(x => x.Comentarios !="").Select(x => new HistorialEstatusVM { Fecha = x.FechaCreacion.ToString(), Estatus = ((enumEstatusMovimiento)x.Status).ToString(), Comentarios =x.Comentarios, UserName =x.UserName }).ToList()
            };

         
            //Mapper.Map(cuenta, cuentaVM);
            return View(EstadoCuentaVM);
        }

        #region Privates

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
                (Parameters.onlyAplicados == false ? Parameters == null || Parameters.fechaInicio == null || (Parameters.fechaInicio < x.FechaCreacion)
                    && (Parameters.fechaFin == null || Parameters.fechaFin > x.FechaCreacion) :
                    (Parameters.fechaInicio == null || (Parameters.fechaInicio < x.FechaCreacion))
                    && (Parameters.fechaFin == null || Parameters.fechaFin > x.FechaCreacion) &&
                    x.Status == enumEstatusMovimiento.Aplicado.GetHashCode()
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
                    Comentarios = Comentarios(x),//"TODO:Comentarios",
                    Concepto = getConceptoString((enumMotivo)x.Motivo, x.Id),
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

        private string Comentarios( Movimiento mov)
        {
          List<HistorialEstatusVM> HistorialEstatusVM = mov.Movimientos_Estatus.OrderByDescending(x => x.FechaCreacion).Where(x => x.MovimientoId == mov.MovimientoId && x.Comentarios !="").Select(x => new HistorialEstatusVM { Fecha = x.FechaCreacion.ToString(), Estatus = ((enumEstatusMovimiento)x.Status).ToString(), Comentarios =x.Comentarios, UserName =x.UserName }).ToList();

            string comentarios= "";
          foreach (HistorialEstatusVM item in HistorialEstatusVM)
          {
              comentarios = " <div class=''>" + "<span class=''>" +
                  comentarios + item.Comentarios + "</span>" +" - "+ "<span class=''>" + item.Fecha + "</span>"
                  + " </div>";
          }
            return comentarios;

            //<div class="listRow">
            //  <span class="listCell Estatus @item.Estatus fwb"> @item.Estatus </span>  
            //  <span class="listCell Comentarios"> @item.Comentarios </span>    
            //  <span class="listCell Usuario"> @item.UserName</span>     
            //  <span class="listCell Fecha"> @item.Fecha</span>
            //</div> 

        }

        private decimal getSaldoAcumulado(bool IsAbono, decimal Monto)
        {
            _saldo = _saldo + (IsAbono ? Monto:-Monto);
            return _saldo;
        }

        #endregion
    }
}