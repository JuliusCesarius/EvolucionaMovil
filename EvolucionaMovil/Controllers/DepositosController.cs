using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using EvolucionaMovil.Models;
using EvolucionaMovil.Models.BR;
using EvolucionaMovil.Models.Enums;
using EvolucionaMovil.Repositories;
using System.Web.Security;
using System.Net.Mail ;
using EvolucionaMovil.Models.Classes;
using EvolucionaMovil.Attributes;
using cabinet.patterns.enums;
using EvolucionaMovil.Models.Extensions;

namespace EvolucionaMovil.Controllers
{
    public class DepositosController : CustomControllerBase
    {
        private AbonoRepository repository = new AbonoRepository();
        private EstadoDeCuentaRepository _estadoDeCuentaRepository;
        private EstadoDeCuentaRepository EstadoDeCuentaRepository
        {
            get
            {
                if (_estadoDeCuentaRepository == null)
                {
                    _estadoDeCuentaRepository = new EstadoDeCuentaRepository();
                }
                return _estadoDeCuentaRepository;
            }
        }

        private EstadoCuentaBR validations = new EstadoCuentaBR();

        //
        // GET: /Depositos/
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public ViewResult Index()
        {
            ViewBag.PageSize = 10;
            ViewBag.PageNumber = 0;
            ViewBag.SearchString = string.Empty;
            ViewBag.fechaInicio = string.Empty;
            ViewBag.FechaFin = string.Empty;
            ViewBag.OnlyAplicados = false;

            return View(getDepositos(new ServiceParameterVM { pageNumber = 0, pageSize = 10 }));
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public ViewResult Index(ServiceParameterVM parameters)
        {
            ViewBag.PageSize = parameters.pageSize;
            ViewBag.PageNumber = parameters.pageNumber;
            ViewBag.SearchString = parameters.searchString;
            ViewBag.fechaInicio = parameters.fechaInicio != null ? ((DateTime)parameters.fechaInicio).GetCurrentTime().ToShortDateString() : "";
            ViewBag.FechaFin = parameters.fechaFin != null ? ((DateTime)parameters.fechaFin).GetCurrentTime().ToShortDateString() : "";
            ViewBag.OnlyAplicados = parameters.onlyAplicados;
            ViewBag.PayCenterId = parameters.PayCenterId;
            ViewBag.PayCenterName = parameters.PayCenterName;
            ViewBag.ProveedorId = parameters.ProveedorId;
            ViewBag.ProveedorName = parameters.ProveedorName;

            return View(getDepositos(parameters));
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public ViewResult Details(int id)
        {
             bool isValid = true;
             if (User.IsInRole(enumRoles.PayCenter.ToString()))
             {
                 isValid = repository.IsAuthorized(PayCenterId, id);
             }
             if (!isValid)
             {
                 AddValidationMessage(enumMessageType.BRException, "No tiene autorización para este depósito");
                 return View(new AbonoVM());
             }

            AbonoVM abonoVM = FillAbonoVM(id);
            //Leer el usuario que viene en la sesión
            int RoleUser = GetRolUser(HttpContext.User.Identity.Name);

            ViewBag.Role = RoleUser;
            return View(abonoVM);
        }


        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public ViewResult Details(AbonoVM model)
        {
            //Aquí van las acciones del PayCenter y Staf para el depósito
            var id = model.AbonoId;
            var action = model.CambioEstatusVM.Estatus;
            string comentario = model.CambioEstatusVM.Comentario != null ? model.CambioEstatusVM.Comentario.TrimEnd() : null;
            Abono abono = repository.LoadById(id);

            if (id > 0)
            {
                var movimiento = abono.CuentaPayCenter.Movimientos.Where(x => x.Motivo == enumMotivo.Deposito.GetHashCode() && x.Id == abono.AbonoId).FirstOrDefault();

                //validar que exista el moviento y sino mandar mensaje de error
                if (movimiento != null)
                {
                    EstadoCuentaBR estadoCuentaBR = new EstadoCuentaBR(repository.context);
                    //Asigno valor default en caso de que entre en ningún case de switch
                    enumEstatusMovimiento nuevoEstatus = (enumEstatusMovimiento)movimiento.Status;
                    switch (action)
                    {
                        case "Cancelar":
                            nuevoEstatus = enumEstatusMovimiento.Cancelado;
                            break;
                        case "Aplicar":
                            nuevoEstatus = enumEstatusMovimiento.Aplicado;
                            break;
                        case "Rechazar":
                            nuevoEstatus = enumEstatusMovimiento.Rechazado;
                            break;
                    }
                    abono.Status = (Int16)nuevoEstatus;
                    movimiento = estadoCuentaBR.ActualizarMovimiento(movimiento.MovimientoId, nuevoEstatus, comentario);
                    this.Succeed = estadoCuentaBR.Succeed;
                    this.ValidationMessages = estadoCuentaBR.ValidationMessages;

                    if (Succeed)
                    {
                        Succeed = repository.Save();
                        if (Succeed)
                        {
                            AddValidationMessage(enumMessageType.Succeed, "El reporte de depósito ha sido " + nuevoEstatus.ToString() + " correctamente");
                        }
                        else
                        {
                            //TODO: implemtar código que traiga mensajes del repositorio
                        }
                    }
                }
                else
                {
                    ViewBag.Mensaje = "No se encontró el movimiento para el depósito.";
                }

            }
            else
            {
                ViewBag.Mensaje = "No existe el depósito.";
            }
            ValidationMessages.ForEach(x => ViewBag.Mensaje += x.Message);
            //Llenar el VM con el método de llenado
            AbonoVM abonoVM = FillAbonoVM(id);

            return View(abonoVM);
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public ActionResult Report()
        {
            ReporteDepositoVM model = new ReporteDepositoVM();
            model.PayCenterId = PayCenterId;
            if (PayCenterId > 0)
            {
                EstadoCuentaBR estadoCuenta = new EstadoCuentaBR();
                var saldos = estadoCuenta.GetSaldosPagoServicio(PayCenterId);
                ViewBag.SaldoActual = saldos.SaldoActual.ToString("C");
                ViewBag.SaldoDisponible = saldos.SaldoDisponible.ToString("C");
                ViewBag.Eventos = saldos.EventosDisponibles.ToString();
            }
            LlenarBancos_Cuentas();
            return View(model);
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public ActionResult Report(ReporteDepositoVM model)
        {
            Succeed = true;
            if (PayCenterId <= 0)
            {
                //Preguntar de esta validación
                AddValidationMessage(enumMessageType.BRException, "No ha especificado un paycenter válido. Favor de especificarlo.");
                Succeed = false;
            }
            PayCentersRepository payCentersRepository = new PayCentersRepository();

            if (!(validations.IsValidReferenciaDeposito(model.Referencia, model.BancoId)))
            {
                //Preguntar de esta validación
                AddValidationMessage(enumMessageType.BRException,"La referencia especificada ya existe en el sistema. Favor de verificarla.");
                Succeed = false;
            }

            if (Convert.ToDateTime(model.FechaPago).CompareTo(DateTime.UtcNow.GetCurrentTime()) == 1)
            {
                AddValidationMessage(enumMessageType.BRException, "La fecha de depósito debe ser menor o igual a la fecha actual.");
                Succeed = false;
            }

            if (Succeed)
            {
                model.PayCenterName = PayCenterName;
                AbonoVM abonoVM = new AbonoVM();
                Mapper.Map(model, abonoVM);
                abonoVM.MontoString = ((decimal)model.Monto).ToString("C");
                abonoVM.Status = (Int16)enumEstatusMovimiento.Procesando;
                abonoVM.PayCenterId = PayCenterId;
                abonoVM.PayCenterName = PayCenterName;
                abonoVM.FechaCreacion = DateTime.UtcNow.GetCurrentTime();
                abonoVM.FechaPago = (DateTime)model.FechaPago;
                abonoVM.ProveedorId = model.ProveedorId;
                return View("Confirm", abonoVM);
            }
            else
            {
                //model.CuentasDeposito = GetProveedoresByTipoCuenta(PayCenterName);
                LlenarBancos_Cuentas();
                return View(model);
            }
        }


        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff })]
        public ActionResult Confirm(AbonoVM model)
        {
            bool exito = true;

            if (!(validations.IsValidReferenciaDeposito(model.Referencia, model.BancoId)))
            {
                //todo:Preguntar de esta validacion
                AddValidationMessage(enumMessageType.BRException, "La referencia especificada ya existe en el sistema. Favor de verificarla.");
                exito = false;
            }

            if (Convert.ToDateTime(model.FechaPago).CompareTo(DateTime.UtcNow.GetCurrentTime()) == 1)
            {
                AddValidationMessage(enumMessageType.BRException, "La fecha de depósito debe ser menor o igual a la fecha actual.");
                exito = false;
            }


            if (exito)
            {
                if (ModelState.IsValid)
                {
                    PaycenterBR payCenterBR = new PaycenterBR();
                    model.CuentaId=payCenterBR.GetOrCreateCuentaPayCenter(PayCenterId, model.TipoCuenta,model.ProveedorId);
                    Abono abono = new Abono
                    {
                        BancoId = model.BancoId,
                        CuentaBancariaId = model.CuentaBancariaId,
                        CuentaId = model.CuentaId,
                        Status = (Int16)enumEstatusMovimiento.Procesando,
                        FechaCreacion = DateTime.UtcNow.GetCurrentTime(),
                        FechaPago = (DateTime)model.FechaPago,
                        Monto = (Decimal)model.Monto,
                        PayCenterId = PayCenterId,
                        Referencia = model.Referencia,
                        RutaFichaDeposito = model.RutaFichaDeposito
                    };
                    repository.Add(abono);

                    EstadoCuentaBR estadoCuentaBR = new EstadoCuentaBR(repository.context);
                    var movimiento = estadoCuentaBR.CrearMovimiento(PayCenterId, enumTipoMovimiento.Abono, model.AbonoId, model.CuentaId, (Decimal)model.Monto, enumMotivo.Deposito, PayCenterName);

                    exito = repository.Save();
                    //Julius: Tuve que guardar otra vez para guardar el abonoId generado en la BD
                    estadoCuentaBR.ActualizaReferenciaIdMovimiento(movimiento.MovimientoId, abono.AbonoId);
                    repository.Save();

                    model.AbonoId = abono.AbonoId;
                    AddValidationMessage(enumMessageType.Succeed, "Se ha registrado su depósito con éxito con clave " + movimiento.Clave + ". En breve será revisado y aplicado.");
                }
            }
            else
            {
                AddValidationMessage(enumMessageType.BRException, "No fue posible guardar el reporte de depósito.");
            }
            model.FechaCreacion = DateTime.UtcNow.GetCurrentTime();
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        #region Funciones

        /// <summary>
        /// LLena el AbonoVM
        /// </summary>
        /// <param name="id">AbonoId</param>
        /// <returns></returns>
        private AbonoVM FillAbonoVM(int id)
        {

            Abono abono = repository.LoadById(id);
            BancosRepository bancosRepository = new BancosRepository();
            var banco = bancosRepository.LoadById(abono.BancoId);

            //fill estatus movimientos          
            int movimientoId = 0;
            var movimiento = abono.CuentaPayCenter.Movimientos.Where(x => x.CuentaId == abono.CuentaId && x.Motivo == enumMotivo.Deposito.GetHashCode() && x.PayCenterId == abono.PayCenterId && x.Id == abono.AbonoId).FirstOrDefault();
            if (movimiento != null)
            {
                movimientoId = movimiento.MovimientoId;
            }
            else
            {
                ViewBag.Mensaje = "No existe el movimiento para el depósito.";
            }
            //  var movimiento = estadoDeCuentaRepository.LoadById(movimientoId);
            AbonoVM abonoVM = new AbonoVM
            {
                AbonoId = id,
                Banco = banco.Nombre,
                CuentaBancaria = banco.CuentasBancarias.Where(x => x.CuentaId == abono.CuentaBancariaId).FirstOrDefault().NumeroCuenta,
                Status = abono.Status, //((enumEstatusMovimiento)abono.Status).ToString(),
                FechaCreacion = abono.FechaCreacion,
                FechaPago = abono.FechaPago,
                MontoString = abono.Monto.ToString("C"),
                PayCenterName = abono.PayCenter.UserName,
                Referencia = abono.Referencia,
                RutaFichaDeposito = abono.RutaFichaDeposito,
                TipoCuenta = (enumTipoCuenta)abono.CuentaPayCenter.TipoCuenta,
                HistorialEstatusVM = movimiento != null ? movimiento.Movimientos_Estatus.OrderByDescending(x => x.FechaCreacion).Select(x => new HistorialEstatusVM { Fecha = x.FechaCreacion.ToString(), Estatus = ((enumEstatusMovimiento)x.Status).ToString(), Comentarios = x.Comentarios, UserName = x.UserName }).ToList() : null
            };
            return abonoVM;
        }
        private int GetRolUser(string pUser)
        {
            var roles = Roles.GetRolesForUser(pUser);
            int rolUser = 0;
            if (roles.Any(x => x == enumRoles.PayCenter.ToString()))
            {
                rolUser = enumRoles.PayCenter.GetHashCode();
            }
            else if (roles.Any(x => x == enumRoles.Staff.ToString() || x == enumRoles.Administrator.ToString()))
            {
                rolUser = enumRoles.Staff.GetHashCode();
            }

            return rolUser;
        }

        private void LlenarBancos_Cuentas()
        {
            //TODO: Julius: ver de que forma se hace esto mejor porque quedó muy complicado y le pega al rendimiento
            BancosRepository bancosRepository = new BancosRepository();
            //var bancos = bancosRepository.ListAll().Where(x => x.CuentasBancarias.Count > 0);
            ProveedoresRepository proveedoresRepository = new ProveedoresRepository();
            var proveedores = proveedoresRepository.ListAll().OrderBy(x=>x.Nombre);
            var proveedoresVM = proveedores.ToListOfDestination<ProveedorVM>();
            foreach (var proveedorVM in proveedoresVM)
            {
                var cuentasBancarias = proveedores.Where(x => x.ProveedorId == proveedorVM.ProveedorId).FirstOrDefault().CuentasBancarias;
                var cuentasGrupedByBanco = cuentasBancarias.GroupBy(x => x.Banco);
                proveedorVM.Bancos = cuentasGrupedByBanco.Select(x => x.Key).ToListOfDestination<BancoVM>();
                foreach (var bancoVM in proveedorVM.Bancos)
                {
                    bancoVM.CuentasBancarias = cuentasBancarias.Where(x => x.BancoId == bancoVM.BancoId).ToListOfDestination<CuentaBancariaVM>();
                }
            }
            ViewBag.Proveedores = proveedoresVM;
        }
        private string getBancoById(int BancoId, ref IEnumerable<Banco> Bancos)
        {
            string nombreBanco = string.Empty;
            if (Bancos == null)
            {
                return nombreBanco;
            }
            var banco = Bancos.Where(x => x.BancoId == BancoId).FirstOrDefault();
            if (banco == null)
            {
                return nombreBanco;
            }
            else
            {
                return banco.Nombre;
            }
        }

        private string getCuentaById(int CuentaBancariaId, ref IEnumerable<Banco> Bancos)
        {
            string nombreCuenta = string.Empty;
            if (Bancos == null)
            {
                return nombreCuenta;
            }
            var cuentaBancaria = Bancos.SelectMany(x => x.CuentasBancarias).Where(x => x.CuentaId == CuentaBancariaId).FirstOrDefault();
            if (cuentaBancaria == null)
            {
                return nombreCuenta;
            }
            else
            {
                return cuentaBancaria.NumeroCuenta;
            }
        }
        private string getComentarioCambioEstatus(int AbonoId)
        {
            var lastComment = EstadoDeCuentaRepository.GetUltimoCambioEstatus(enumMotivo.Deposito, AbonoId);
            if (lastComment != null)
            {
                return lastComment.Comentarios;
            }
            else
            {
                return string.Empty;
            }
        }
        private SimpleGridResult<DepositoVM> getDepositos(ServiceParameterVM Parameters = null)
        {
            IEnumerable<Abono> depositos;
            if (PayCenterId == 0 && Parameters.ProveedorId == 0)
            {
                depositos = repository.ListAll().OrderByDescending(m => m.FechaCreacion);
            }
            else
            {
                if (PayCenterId > 0 && Parameters.ProveedorId > 0)
                {
                    depositos = repository.GetByPayCenterIdProveedorId(PayCenterId, Parameters.ProveedorId).OrderByDescending(m => m.FechaCreacion);
                }
                else
                {
                    if (PayCenterId > 0)
                    {
                        depositos = repository.GetByPayCenterId(PayCenterId).OrderByDescending(m => m.FechaCreacion);
                    }
                    else
                    {
                        depositos = repository.GetByProveedorId(Parameters.ProveedorId).OrderByDescending(m => m.FechaCreacion);
                    }
                }
               
            }
        
            var bancos = new BancosRepository().ListAll();

            SimpleGridResult<DepositoVM> simpleGridResult = new SimpleGridResult<DepositoVM>();
            var abonosVM = depositos.Where(x =>
                (Parameters == null || (
                                (Parameters.fechaInicio == null || (Parameters.fechaInicio < x.FechaCreacion))
                        && (Parameters.fechaFin == null || Parameters.fechaFin > x.FechaCreacion)
                        && (Parameters.onlyAplicados ? x.Status == enumEstatusMovimiento.Aplicado.GetHashCode() : true)
                        )
                    )
                ).Select(x => new DepositoVM
                {
                    AbonoId = x.AbonoId,
                    PayCenterId = x.PayCenterId,
                    Banco = getBancoById(x.BancoId, ref bancos),
                    Comentarios = getComentarioCambioEstatus(x.AbonoId),
                    CuentaBancaria = getCuentaById(x.CuentaBancariaId, ref bancos),
                    FechaPago = x.FechaPago.GetCurrentTime().ToShortDateString(),
                    FechaCreacion = x.FechaCreacion.GetCurrentTime().ToShortDateString(),
                    Monto = x.Monto.ToString("C"),
                    PayCenter = x.PayCenter.UserName,
                    Referencia = x.Referencia,
                    Status = x.Status,
                    TipoCuenta = ((enumTipoCuenta)x.CuentaPayCenter.TipoCuenta).ToString(),
                    ProveedorName = "Jo"
                });

            //Filtrar por searchString: Lo puse después del primer filtro porque se complicaba obtener los strings de las tablas referenciadas como bancos, cuenta bancaria, etc.
            if (Parameters != null && !string.IsNullOrEmpty(Parameters.searchString))
            {
                abonosVM = abonosVM.Where(x => Parameters.searchString == null || (
                    x.Referencia.ContainsInvariant(Parameters.searchString) ||
                    x.Banco.ContainsInvariant(Parameters.searchString) ||
                    x.CuentaBancaria.ContainsInvariant(Parameters.searchString) ||
                    x.TipoCuenta.ContainsInvariant(Parameters.searchString) ||
                    x.StatusString.ContainsInvariant(Parameters.searchString) ||
                    ((enumEstatusMovimiento)x.Status).ToString().ContainsInvariant(Parameters.searchString)
                    ));
            }
            if (Parameters != null)
            {
                simpleGridResult.CurrentPage = Parameters.pageNumber;
                simpleGridResult.PageSize = Parameters.pageSize;
                if (Parameters.pageSize > 0)
                {
                    var pageNumber = Parameters.pageNumber >= 0 ? Parameters.pageNumber : 0;
                    simpleGridResult.CurrentPage = pageNumber;
                    simpleGridResult.TotalRows = abonosVM.Count();
                    abonosVM = abonosVM.Skip(pageNumber * Parameters.pageSize).Take(Parameters.pageSize);
                }
            }
            simpleGridResult.Result = abonosVM;

            return simpleGridResult;
        }

        #endregion
    }
}