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
using System.Web.Security;
using EvolucionaMovil.Models.Classes;
using cabinet.patterns.enums;
using EvolucionaMovil.Attributes;
using System.Globalization;
using EvolucionaMovil.Models.BR;

namespace EvolucionaMovil.Controllers
{
    public class PagoServiciosController : CustomControllerBase
    {
        #region Repositorios
        private PagoServiciosRepository repository = new PagoServiciosRepository();
        private ServiciosRepository sRepository = new ServiciosRepository();
        private TicketRepository tRepository = new TicketRepository();
        private PayCentersRepository pRepository = new PayCentersRepository();
        private ParametrosRepository parRepository = new ParametrosRepository();
        private EstadoDeCuentaRepository eRepository = new EstadoDeCuentaRepository();
        private PaquetesRepository pqrepository = new PaquetesRepository();
        private CultureInfo ci = new CultureInfo("es-MX");
        #endregion

        private const int PROVEEDOR_EVOLUCIONAMOVIL = 1;

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.PayCenter })]
        public ViewResult Index()
        {
            ViewBag.PageSize = 10;
            ViewBag.PageNumber = 0;
            ViewBag.SearchString = string.Empty;
            ViewBag.fechaInicio = string.Empty;
            ViewBag.FechaFin = string.Empty;
            ViewBag.OnlyAplicados = false;
            return View(getPagosServicio(new ServiceParameterVM { pageNumber = 0, pageSize = 10 }));
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.PayCenter })]
        public ViewResult Index(ServiceParameterVM parameters)
        {
            ViewBag.PageSize = parameters.pageSize;
            ViewBag.PageNumber = parameters.pageNumber;
            ViewBag.SearchString = parameters.searchString;
            ViewBag.fechaInicio = parameters.fechaInicio != null ? ((DateTime)parameters.fechaInicio).ToShortDateString() : "";
            ViewBag.FechaFin = parameters.fechaInicio != null ? ((DateTime)parameters.fechaFin).ToShortDateString() : "";
            ViewBag.OnlyAplicados = parameters.onlyAplicados;
            return View(getPagosServicio(parameters));
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.PayCenter })]
        public ViewResult Details(int id)
        {
            PagoVM pagoVM = FillPagoVM(id);
            int RoleUser = GetRolUser(HttpContext.User.Identity.Name);
            ViewBag.Role = RoleUser;

            return View(pagoVM);
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.PayCenter })]
        public ViewResult Details(PagoVM model) //Con esto se actualiza el status del pago
        {
            //Aquí van las acciones del PayCenter y Staf para el depósito
            var id = model.PagoId;
            var action = model.CambioEstatusVM.Estatus;
            string comentario = model.CambioEstatusVM.Comentario != null ? model.CambioEstatusVM.Comentario.TrimEnd() : null;
            Pago pago = repository.ListAll().Where(x => x.PagoId == model.PagoId).FirstOrDefault();

            if (id > 0)
            {
                var movimiento = pago.Movimiento;

                //validar que exista el moviento y sino mandar mensaje de error
                if (movimiento != null)
                {
                    EstadoCuentaBR estadoCuentaBR = new EstadoCuentaBR(repository.context);
                    //Asigno valor default en caso de que entre en ningún case de switch
                    enumEstatusMovimiento nuevoEstatus = (enumEstatusMovimiento)movimiento.Status;
                    switch (action)
                    {
                        case "Cancelado":
                            nuevoEstatus = enumEstatusMovimiento.Cancelado;
                            break;
                        case "Aplicado":
                            nuevoEstatus = enumEstatusMovimiento.Aplicado;
                            break;
                        case "Rechazado":
                            nuevoEstatus = enumEstatusMovimiento.Rechazado;
                            break;
                    }

                    movimiento = estadoCuentaBR.ActualizarMovimiento(pago.MovimientoId, nuevoEstatus, comentario);
                    Succeed = estadoCuentaBR.Succeed;
                    ValidationMessages = estadoCuentaBR.ValidationMessages;

                    if (Succeed)
                    {
                        pago.Status = movimiento.Status;
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
                    AddValidationMessage(enumMessageType.BRException, "No se encontró el movimiento para el depósito.");
                }

            }
            else
            {
                AddValidationMessage(enumMessageType.BRException, "No existe el depósito.");
            }
            ValidationMessages.ForEach(x => ViewBag.Mensaje += x.Message);

            PagoVM pagoVM = FillPagoVM(id);
            return View(pagoVM);
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.PayCenter })]
        public ActionResult Create()
        {
            PagoVM pVM = new PagoVM();
            var paycenter = GetPayCenter();

            if (paycenter != null)
            {
                EstadoCuentaBR br = new EstadoCuentaBR(repository.context);
                var saldo = br.GetSaldosPagoServicio(paycenter.PayCenterId);

                ViewData["Eventos"] = pqrepository.GetEventosByPayCenter(paycenter.PayCenterId);
                ViewData["SaldoActual"] = saldo.SaldoActual;
            }
            else
                AddValidationMessage(enumMessageType.BRException, "El usuario no tiene un paycenter relacionado válido");

            return View(pVM);
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.PayCenter })]
        public ActionResult Create(PagoVM model)
        {
            var paycenter = GetPayCenter();
            if (paycenter != null)
            {
                Pago pago = new Pago();
                if (ModelState.IsValid)
                {
                    try
                    {
                        #region Crear Movimiento Inicial
                        EstadoCuentaBR br = new EstadoCuentaBR(repository.context);
                        PaycenterBR payCenterBR = new PaycenterBR();
                        var cuentaId = payCenterBR.GetOrCreateCuentaPayCenter(PayCenterId, enumTipoCuenta.Pago_de_Servicios, PROVEEDOR_EVOLUCIONAMOVIL);
                        Movimiento mov = br.CrearMovimiento(PayCenterId, enumTipoMovimiento.Cargo, 0, cuentaId, (Decimal)model.Importe, enumMotivo.Pago, PayCenterName);
                        #endregion

                        #region Registro de Pago
                        string Referencia = "";
                        Mapper.CreateMap<PagoVM, Pago>().ForMember(dest => dest.DetallePagos, opt => opt.Ignore());
                        Mapper.Map(model, pago);
                        pago.FechaCreacion = DateTime.Now;
                        pago.Servicio = model.Servicios.Where(x => x.Value == model.ServicioId).FirstOrDefault().Text;
                        pago.PayCenterId = PayCenterId;
                        pago.Movimiento = mov;

                        var iDetalles = sRepository.ListAll().Where(x => x.ServicioId == pago.ServicioId).FirstOrDefault().DetalleServicios;
                        foreach (DetalleServicio d in iDetalles)
                        {
                            var valor = Request.Form[d.Campo];
                            if (d.EsReferencia)
                                Referencia = valor;

                            pago.DetallePagos.Add(new DetallePago { Campo = d.Campo, Valor = valor });
                        }

                        repository.Add(pago);
                        repository.Save();

                        br.ActualizaReferenciaIdMovimiento(pago.MovimientoId, pago.PagoId);
                        repository.Save();

                        model.PagoId = pago.PagoId;
                        #endregion

                        #region Registro Ticket
                        Ticket oTicket = new Ticket();
                        oTicket.Baja = false;
                        oTicket.ClienteEmail = "";
                        oTicket.ClienteNombre = pago.ClienteNombre;
                        oTicket.ClienteTelefono = "";
                        oTicket.Comision = paycenter.Parametros.ComisionPayCenter != null ? (Decimal)paycenter.Parametros.ComisionCliente : 0; //Comision configurada del paycenter
                        oTicket.FechaCreacion = DateTime.Now;
                        oTicket.Folio = createFolio(pago.PagoId);
                        oTicket.Importe = pago.Importe;
                        oTicket.Leyenda = "";
                        oTicket.PagoId = pago.PagoId;
                        oTicket.PayCenterId = pago.PayCenterId;
                        oTicket.Referencia = "";
                        oTicket.TipoServicio = pago.Servicio;
                        oTicket.Referencia = Referencia;

                        tRepository.Add(oTicket);
                        tRepository.Save();
                        #endregion

                        return RedirectToAction("Ticket/" + oTicket.PagoId.ToString());
                    }
                    catch (Exception e)
                    {
                        AddValidationMessage(enumMessageType.BRException, "Ocurrió un error al registrar el pago: " + e.Message);
                    }
                }
                else
                    AddValidationMessage(enumMessageType.BRException, "Los datos no son válidos");
            }
            else
                AddValidationMessage(enumMessageType.BRException, "El usuario no tiene un paycenter relacionado válido");

            return View(model);
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.PayCenter })]
        public ViewResult Ticket(int id)
        {
            TicketVM ticketVM = new TicketVM();
            var paycenter = GetPayCenter();
            if (paycenter != null)
            {
                try
                {
                    Ticket tVM = tRepository.ListAll().Where(x => x.PagoId == id).FirstOrDefault();
                    Mapper.CreateMap<Ticket, TicketVM>().ForMember(dest => dest.Pago, opt => opt.Ignore());
                    Mapper.Map(tVM, ticketVM);
                    ticketVM.FechaPago = tVM.Pago.FechaCreacion;
                    tVM.Pago.DetallePagos.ToList().ForEach(x => ticketVM.DetallePagos.Add(x));

                    ViewBag.LogoPayCenter = paycenter.Logotipo;
                    @ViewBag.Comision = paycenter.Parametros.ComisionPayCenter.ToString() + "%";
                }
                catch (Exception e)
                {
                    AddValidationMessage(enumMessageType.BRException, "Ocurrio un error al cargar el ticket: " + e.Message);
                }
            }
            else
            {
                AddValidationMessage(enumMessageType.BRException, "El usuario no tiene un paycenter relacionado válido");
            }
            return View(ticketVM);
        }

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        private SimpleGridResult<PagoServicioVM> getPagosServicio(ServiceParameterVM Parameters = null)
        {
            SimpleGridResult<PagoServicioVM> simpleGridResult = new SimpleGridResult<PagoServicioVM>();
            var paycenter = GetPayCenter();

            if (paycenter != null)
            {
                var pagos = repository.ListAll();
                var movimientos = eRepository.GetMovimientos();
                EstadoCuentaBR br = new EstadoCuentaBR(repository.context);

                var pagosServicioVM = pagos.OrderByDescending(y => y.PagoId).Where(x => Parameters == null
                     || (Parameters.fechaInicio == null || (Parameters.fechaInicio < x.FechaCreacion)
                         && (Parameters.fechaFin == null || Parameters.fechaFin > x.FechaCreacion)
                     )
                     ).Select(x => new PagoServicioVM
                     {
                         PayCenterId = x.PayCenterId,
                         Folio = x.Ticket != null ? x.Ticket.Folio : "NA",
                         Servicio = x.Servicio,
                         NombreCliente = x.ClienteNombre,
                         PayCenterName = x.PayCenter != null ? x.PayCenter.Nombre : "[Desconocido]",
                         PagoId = x.PagoId,
                         //todo:Optimizar esta consulta para que no haga un load por cada registro que levante.
                         Comentarios = x.Movimiento.Movimientos_Estatus.Count > 0 ? x.Movimiento.Movimientos_Estatus.OrderByDescending(y => y.Movimiento_EstatusId).FirstOrDefault().Comentarios : "Sin comentarios",
                         Monto = x.Importe.ToString("C"),
                         FechaCreacion = x.FechaCreacion.ToShortDateString(),
                         FechaVencimiento = x.FechaVencimiento.ToShortDateString(),
                         Status = ((enumEstatusMovimiento)x.Status).ToString()
                     });

                if (Parameters != null && Parameters.onlyAplicados)
                    pagosServicioVM = pagosServicioVM.Where(x => x.Status == enumEstatusMovimiento.Aplicado.ToString());

                if (Parameters != null && !string.IsNullOrEmpty(Parameters.searchString))
                    pagosServicioVM = pagosServicioVM.Where(x => x.NombreCliente.ToLower().Contains(Parameters.searchString.ToLower()) || x.Servicio.ToLower().Contains(Parameters.searchString.ToLower()));

                ViewData["Eventos"] = pqrepository.GetEventosByPayCenter(paycenter.PayCenterId);
                var saldo = br.GetSaldosPagoServicio(paycenter.PayCenterId);
                ViewData["SaldoActual"] = saldo.SaldoActual;

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
            }
            else
                AddValidationMessage(enumMessageType.BRException, "El usuario no tiene un paycenter relacionado válido");

            return simpleGridResult;
        }

        [HttpGet]
        public JsonResult getDetalleServicio(int servicioId)
        {
            List<DetalleServicioVM> lsDetalles = new List<DetalleServicioVM>();
            var iDetalles = sRepository.ListAll().Where(x => x.ServicioId == servicioId).FirstOrDefault().DetalleServicios;
            foreach (DetalleServicio d in iDetalles)
            {
                DetalleServicioVM dVm = new DetalleServicioVM();
                Mapper.CreateMap<DetalleServicio, DetalleServicioVM>().ForMember(dest => dest.Servicio, opt => opt.Ignore());
                Mapper.Map(d, dVm);
                lsDetalles.Add(dVm);
            }
            return this.Json(lsDetalles, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        private string createFolio(int idTabla)
        {
            var yy = DateTime.Now.Year.ToString().Substring(1, 2);
            var mm = DateTime.Now.Month.ToString().Length < 2 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            var dd = DateTime.Now.Day.ToString().Length < 2 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            var tabla = idTabla.ToString("D6"); //999999
            return yy + mm + dd + tabla;
        }

        [NonAction]
        private int GetRolUser(string pUser)
        {
            var roles = Roles.GetRolesForUser(pUser);
            int Rol = 0;
            if (roles.Any(x => x == enumRoles.PayCenter.ToString()))
            {
                Rol = enumRoles.PayCenter.GetHashCode();
            }
            else if (roles.Any(x => x == enumRoles.Staff.ToString() || x == enumRoles.Administrator.ToString()))
            {
                Rol = enumRoles.Staff.GetHashCode();
            }

            return Rol;
        }

        [NonAction]
        private PayCenter GetPayCenter(int Id = 0)
        {
            //Buscar el payCenter
            if (Id == 0)
            {
                if (HttpContext.User.IsInRole(enumRoles.PayCenter.ToString()))
                {
                    Id = pRepository.GetPayCenterByUserName(HttpContext.User.Identity.Name);
                }
            }
            PayCenter payCenter = pRepository.LoadById(Id);

            return payCenter;
        }

        [NonAction]
        private PagoVM FillPagoVM(Int32 id)
        {
            PagoVM pagoVM = new PagoVM();
            try
            {
                Pago pago = repository.ListAll().Where(x => x.PagoId == id).FirstOrDefault();
                Mapper.CreateMap<Pago, PagoVM>().ForMember(dest => dest.Servicios, opt => opt.Ignore());
                Mapper.Map(pago, pagoVM);
                pagoVM.ServicioNombre = pagoVM.Servicios.Where(x => x.Value == pago.ServicioId).FirstOrDefault().Text;
                pagoVM.Estatus = ((enumEstatusMovimiento)pago.Status).ToString();

                foreach (Movimientos_Estatus m in pago.Movimiento.Movimientos_Estatus.OrderByDescending(x => x.Movimiento_EstatusId))
                {
                    HistorialEstatusVM h = new HistorialEstatusVM();
                    h.Comentarios = m.Comentarios;
                    h.Estatus = ((enumEstatusMovimiento)m.Status).ToString();
                    h.Fecha = m.FechaCreacion.ToShortDateString();
                    h.UserName = m.UserName;
                    pagoVM.HistorialEstatusVM.Add(h);
                }
            }
            catch (Exception e)
            {
                AddValidationMessage(enumMessageType.BRException, "Ocurrio un error al recuperar la información del pago: " + e.Message);
            }
            return pagoVM;
        }

        [HttpPost]
        public string GetPagoServicios(ServiceParameterVM parameters)
        {
            var pagoServiciosResult = getPagosServicio(parameters);
            return Newtonsoft.Json.JsonConvert.SerializeObject(pagoServiciosResult);
        }
    }
}
