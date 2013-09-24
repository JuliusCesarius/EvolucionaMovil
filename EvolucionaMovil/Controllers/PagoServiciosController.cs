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
using EvolucionaMovil.Models.Helpers;
using System.Text;
using EvolucionaMovil.Models.Extensions;

namespace EvolucionaMovil.Controllers
{
    public class PagoServiciosController : CustomControllerBase
    {
        #region Repositorios
        private PagoServiciosRepository repository = new PagoServiciosRepository();
        private ServiciosRepository serviciosRepository = new ServiciosRepository();
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
            ViewBag.fechaInicio = parameters.fechaInicio != null ? ((DateTime)parameters.fechaInicio).GetCurrentTime().ToShortDateString() : "";
            ViewBag.FechaFin = parameters.fechaFin != null ? ((DateTime)parameters.fechaFin).GetCurrentTime().ToShortDateString() : "";
            ViewBag.OnlyAplicados = parameters.onlyAplicados;
            ViewBag.PayCenterId = parameters.PayCenterId;
            ViewBag.PayCenterName = parameters.PayCenterName;
            return View(getPagosServicio(parameters));
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.PayCenter })]
        public ViewResult Details(int id)
        {
            PagoVM pagoVM;
             bool isValid = true;
             if (User.IsInRole(enumRoles.PayCenter.ToString()))
             {
                 isValid = repository.IsAuthorized(PayCenterId, id);
             }
             if (!isValid)
             {
                 AddValidationMessage(enumMessageType.BRException, "No tiene autorización para este pago.");
                 pagoVM = new PagoVM();
                 pagoVM.FechaVencimiento = Convert.ToDateTime ("01/01/1999");
                 return View(pagoVM);
             }
             pagoVM = FillPagoVM(id);
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
            //Reinicio el ModelState porque no valido sus valores
            ModelState.Clear();

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

                    movimiento = estadoCuentaBR.ActualizarMovimiento(pago.MovimientoId, nuevoEstatus, comentario);
                    Succeed = estadoCuentaBR.Succeed;
                    ValidationMessages = estadoCuentaBR.ValidationMessages;

                    if (Succeed)
                    {
                        pago.Status = movimiento.Status;
                        Succeed = repository.Save();
                        if (Succeed)
                        {
                            ModelState.Clear();
                            AddValidationMessage(enumMessageType.Succeed, "El reporte de depósito ha sido " + nuevoEstatus.ToString() + " correctamente");
                            var paycenter = pRepository.LoadById(pago.PayCenter.PayCenterId);
                            if (paycenter != null){
                                Succeed = EmailHelper.Enviar(getMensajeCambioEstatus(nuevoEstatus.ToString(), pago), "El Pago de Servicio " + pago.Ticket.Folio + " ha sido " + nuevoEstatus.ToString(), paycenter.Email);
                            }
                            //No obtuve lo errores por que es una clase estática y va a almacenar los de todas las sesiones
                            //ValidationMessages.AddRange(EmailHelper
                            //AddValidationMessage(enumMessageType.Notification, "No pudo enviarse el email de aviso. Comuníquelo al administrador");
                            return Details(id);
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
            PagoVM pagoVM = new PagoVM();
            pagoVM.PayCenterId = PayCenterId;
            EstadoCuentaBR br = new EstadoCuentaBR();                
            if (PayCenterId > 0)
            {
                var saldo = br.GetSaldosPagoServicio(PayCenterId);
                ViewData["Eventos"] = saldo.EventosDisponibles;
                ViewData["SaldoActual"] = saldo.SaldoActual.ToString("C");
                ViewData["SaldoDisponible"] = saldo.SaldoDisponible.ToString("C");         
            }
            //TODO:Obtener el Máximo a financiar, esto es por pay center
            var comisionFinanciamiento = br.GetComisionFinanciamiento(PayCenterId);
            ViewData["Comision"] = comisionFinanciamiento.Comision;
            ViewData["MaximoFinanciar"] = comisionFinanciamiento.Financiamiento;
            return View(pagoVM);
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.PayCenter })]
        public ActionResult Create(PagoVM model)
        {
            if (PayCenterId == 0)
            {
                model.PayCenterName = string.Empty;
                AddValidationMessage(enumMessageType.DataValidation, "Por favor, seleccione primero un PayCenter.");
                return View(model);
            }

            EstadoCuentaBR br = new EstadoCuentaBR(repository.context);
            var saldo = br.GetSaldosPagoServicio(PayCenterId);
            ViewData["SaldoActual"] = saldo.SaldoActual.ToString("C");
            ViewData["SaldoDisponible"] = saldo.SaldoDisponible.ToString("C");
            ViewData["Eventos"] = saldo.EventosDisponibles;
           
            if (model.Importe <= 0)
            {
                AddValidationMessage(enumMessageType.DataValidation, "El importe no puede ser menor a $0.00.");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    
                            #region Crear Movimiento Inicial
                            Pago pago = new Pago();
                            PaycenterBR payCenterBR = new PaycenterBR();
                            var cuentaId = payCenterBR.GetOrCreateCuentaPayCenter(PayCenterId, enumTipoCuenta.Pago_de_Servicios, PROVEEDOR_EVOLUCIONAMOVIL);
                            //Devuelve si usa evento en el pago
                            bool usaEvento = false;
                            List<Movimiento> movimientos = br.CrearMovimientosPagoServicios(PayCenterId, (Decimal)model.Importe, PayCenterName, out usaEvento);
                            Succeed = br.Succeed;
                            ValidationMessages = br.ValidationMessages;

                           if (!Succeed)
                            {
                                return View(model);
                            }

                            #endregion

                            #region Registro de Pago
                            string Referencia = "";
                            Mapper.CreateMap<PagoVM, Pago>().ForMember(dest => dest.DetallePagos, opt => opt.Ignore());
                            Mapper.Map(model, pago);
                            pago.Servicio = model.Servicios.Where(x => x.Value == model.ServicioId).FirstOrDefault().Text;
                            pago.PayCenterId = PayCenterId;
                            pago.Movimiento = movimientos.Where(x => x.Motivo == (short)enumMotivo.Pago).First();
                            pago.UsoEvento = usaEvento;

                            var iDetalles = serviciosRepository.LoadDetallesServicioByServicioID(pago.ServicioId);
                            foreach (DetalleServicio d in iDetalles)
                            {
                                var valor = Request.Form[d.Campo.Replace(' ', '_').Replace('.', '_')];
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
                            try
                            {

                                //Verifica si tiene configurada la comisión que mostrará al cliente, se toma el valor para mostrar en el ticket
                                ParametrosRepository parametrosRepository = new ParametrosRepository();
                                var parametrosPayCenter = parametrosRepository.GetParametrosPayCenter(PayCenterId);
                                var parametrosGlobales = parametrosRepository.GetParametrosGlobales();

                                Ticket ticket = new Ticket();
                                ticket.ClienteEmail = "";
                                ticket.ClienteNombre = pago.ClienteNombre;
                                ticket.ClienteTelefono = "";
                                ticket.Comision = (parametrosPayCenter != null && parametrosPayCenter.ComisionCliente != null ? (Decimal)parametrosPayCenter.ComisionCliente : 0); //Comision configurada del paycenter
                                ticket.FechaCreacion = DateTime.UtcNow.GetCurrentTime();
                                ticket.Folio = createFolio(pago.PagoId);
                                ticket.Importe = pago.Importe;
                                ticket.Leyenda = parametrosGlobales != null ? parametrosGlobales.LeyendaTicket : null;
                                ticket.PagoId = pago.PagoId;
                                ticket.PayCenterId = pago.PayCenterId;
                                ticket.TipoServicio = pago.Servicio;
                                ticket.Referencia = Referencia;
                                ticket.PayCenterName = PayCenterName;
                                ticket.FechaVencimiento = pago.FechaVencimiento;

                                tRepository.Add(ticket);
                                Succeed = tRepository.Save();
                                if (!Succeed)
                                {
                                    AddValidationMessage(enumMessageType.UnhandledException, "Su pago ha sido Registrado con éxito. Sin embargo, no se pudo generar el ticket, favor de comunicarse con un ejecutivo. ");
                                }
                                //TODO: Enviar email de confirmación
                                PayCentersRepository PayCentersRepository = new PayCentersRepository();
                                EmailHelper.Enviar(getMensajeConfirmacion(pago), "El Pago de Servicio se ha sido registrado. ", PayCentersRepository.GetPayCenterEmail(PayCenterName));

                                return RedirectToAction("Ticket/" + ticket.PagoId.ToString());
                            }
                            catch (Exception ex)
                            {
                                AddValidationMessage(enumMessageType.UnhandledException, "Su pago ha sido Registrado con éxito. Sin embargo, no se pudo generar el ticket, favor de comunicarse con un ejecutivo. ");
                                return View(model);
                            }

                            #endregion
                    
                }
                catch (Exception e)
                {
                    AddValidationMessage(enumMessageType.BRException, "Ocurrió un error al registrar el pago: " + e.Message);
                }
            }
            else
            {
                AddValidationMessage(enumMessageType.BRException, "Los datos no son válidos");
            }
            return View(model);
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.PayCenter })]
        public ViewResult Ticket(int id)
        {
            TicketVM ticketVM = new TicketVM();
            Ticket ticket = tRepository.LoadByPagoId(id);
            if (ticket == null)
            {
                AddValidationMessage(enumMessageType.UnhandledException, "No se ha encontrado el Ticket");
                return View(new TicketVM());
            }
            Mapper.CreateMap<Ticket, TicketVM>().ForMember(dest => dest.Pago, opt => opt.Ignore());
            Mapper.Map(ticket, ticketVM);
            ticketVM.FechaVencimiento = ticket.Pago.FechaCreacion;
            ticket.Pago.DetallePagos.ToList().ForEach(x => ticketVM.DetallePagos.Add(x));

            PayCentersRepository payCenterRepository = new PayCentersRepository();
            ViewBag.LogoPayCenter = payCenterRepository.GetLogotipo(PayCenterId);

            ParametrosRepository parametrosRepository = new ParametrosRepository();
            var parametrosPayCenter = parametrosRepository.GetParametrosPayCenter(PayCenterId);
            if (parametrosPayCenter != null)
            {
                @ViewBag.MostrarComision = parametrosPayCenter.MostrarComision;
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

            IEnumerable<Pago> pagos;
            if (PayCenterId == 0)
            {
                pagos = repository.ListAll().OrderByDescending(m => m.FechaCreacion);
            }
            else
            {
                pagos = repository.GetByPayCenterId(PayCenterId).OrderByDescending(m => m.FechaCreacion);
            }

            SimpleGridResult<PagoServicioVM> simpleGridResult = new SimpleGridResult<PagoServicioVM>();

            var pagosServicioVM = pagos.Where(x =>
                (Parameters == null || (
                                (Parameters.fechaInicio == null || (Parameters.fechaInicio < x.FechaCreacion))
                        && (Parameters.fechaFin == null || Parameters.fechaFin > x.FechaCreacion)
                        && (Parameters.onlyAplicados ? x.Status == enumEstatusMovimiento.Aplicado.GetHashCode() : true)
                        )
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
                     FechaCreacion = x.FechaCreacion.GetCurrentTime().ToLongDateString(),
                     FechaVencimiento = x.FechaVencimiento.GetCurrentTime().ToLongDateString(),
                     Status = ((enumEstatusMovimiento)x.Status).ToString()
                 });

            //Filtrar por searchString: Lo puse después del primer filtro porque se complicaba obtener los strings de las tablas referenciadas como bancos, cuenta bancaria, etc.
            if (Parameters != null && !string.IsNullOrEmpty(Parameters.searchString))
            {
                pagosServicioVM = pagosServicioVM.Where(x => Parameters.searchString == null || (
                    x.Comentarios.ContainsInvariant(Parameters.searchString) ||
                    x.Folio.ContainsInvariant(Parameters.searchString) ||
                    x.NombreCliente.ContainsInvariant(Parameters.searchString) ||
                    x.Servicio.ContainsInvariant(Parameters.searchString) ||
                    x.Status.ContainsInvariant(Parameters.searchString)
                    ));
            }

            if (Parameters != null && Parameters.onlyAplicados)
                pagosServicioVM = pagosServicioVM.Where(x => x.Status == enumEstatusMovimiento.Aplicado.ToString());

            if (Parameters != null && !string.IsNullOrEmpty(Parameters.searchString))
                pagosServicioVM = pagosServicioVM.Where(x => x.NombreCliente.ToLower().Contains(Parameters.searchString.ToLower()) || x.Servicio.ToLower().Contains(Parameters.searchString.ToLower()));

            if (PayCenterId > 0)
            {
                EstadoCuentaBR estadoCuentaBR = new EstadoCuentaBR();
                var saldos = estadoCuentaBR.GetSaldosPagoServicio(PayCenterId);
                //todo:Checar que tipo de saldo debo de mostrar
                ViewData["Eventos"] = saldos.EventosDisponibles;
                ViewData["SaldoActual"] = saldos.SaldoActual.ToString("C");
                ViewData["SaldoDisponible"] = saldos.SaldoDisponible.ToString("C");
            }

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

        [HttpGet]
        public JsonResult getDetalleServicio(int servicioId)
        {
            List<DetalleServicioVM> lsDetalles = new List<DetalleServicioVM>();
            var iDetalles = serviciosRepository.ListAll().Where(x => x.ServicioId == servicioId).FirstOrDefault().DetalleServicios;
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
            var yy = DateTime.UtcNow.GetCurrentTime().Year.ToString().Substring(1, 2);
            var mm = DateTime.UtcNow.GetCurrentTime().Month.ToString().Length < 2 ? "0" + DateTime.UtcNow.GetCurrentTime().Month.ToString() : DateTime.UtcNow.GetCurrentTime().Month.ToString();
            var dd = DateTime.UtcNow.GetCurrentTime().Day.ToString().Length < 2 ? "0" + DateTime.UtcNow.GetCurrentTime().Day.ToString() : DateTime.UtcNow.GetCurrentTime().Day.ToString();
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
        private PagoVM FillPagoVM(Int32 id)
        {
            PagoVM pagoVM = new PagoVM();
            try
            {
                Pago pago = repository.ListAll().Where(x => x.PagoId == id).FirstOrDefault();
                Mapper.CreateMap<Pago, PagoVM>().ForMember(dest => dest.Servicios, opt => opt.Ignore());
                Mapper.Map(pago, pagoVM);
                pagoVM.PayCenterName = pago.PayCenter.Nombre;
                pagoVM.ServicioNombre = pagoVM.Servicios.Where(x => x.Value == pago.ServicioId).FirstOrDefault().Text;

                foreach (Movimientos_Estatus m in pago.Movimiento.Movimientos_Estatus.OrderByDescending(x => x.Movimiento_EstatusId))
                {
                    HistorialEstatusVM h = new HistorialEstatusVM();
                    h.Comentarios = m.Comentarios;
                    h.Estatus = ((enumEstatusMovimiento)m.Status).ToString();
                    h.Fecha = m.FechaCreacion.GetCurrentTime().ToShortDateString();
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

        [NonAction]
        private string getMensajeCambioEstatus(string status, Pago p)
        {

            StringBuilder cadena = new StringBuilder();
            cadena.AppendLine("<h2>Detalle de pago</h2><img src='" + RelativeURLHelper.ToFullUrl(p.PayCenter.Logotipo) + "' />");
            cadena.AppendLine("<table>");
            cadena.AppendLine("<tr><td><h3>PayCenter</h3></td><td><strong>" + p.PayCenter.Nombre + "<strong></td></tr></tr>");
            cadena.AppendLine("<tr><td>Servicio</td><div class='display-field fwb fsl'><h3>" + p.Servicio + "</h3></td></tr></tr>");

            foreach (DetallePago d in p.DetallePagos)
            {
                cadena.AppendLine("<tr><td>" + d.Campo + "</td><td>" + d.Valor + "</td></tr></tr>");
            }
            cadena.AppendLine("<tr><td>Fecha Vencimiento</td><td>" + p.FechaVencimiento.GetCurrentTime().ToShortDateString() + "</td></tr></tr>");
            cadena.AppendLine("<tr><td>Nombre Cliente</td><td><h3>" + p.ClienteNombre + "</h3></td></tr>");
            cadena.AppendLine("<tr><td>Importe</td><div class='display-field fwb fsxl'><h3>" + p.Importe.ToString("C", ci) + "</h3></td></tr>");
            cadena.AppendLine("<tr><td>Estatus</td><div class='display-field fwb fsxl '><span class='Procesando'><h2>" + ((enumEstatusMovimiento)p.Status).ToString() + "</h2></span></td></tr>");
            cadena.AppendLine("<table>");
            cadena.AppendLine("<br />");
            var lastEstatus = p.Movimiento.Movimientos_Estatus.Last();
            if (lastEstatus != null && !string.IsNullOrEmpty(lastEstatus.Comentarios))
            {
                cadena.AppendLine("Comentarios: <h4>" + lastEstatus.Comentarios + "</h4>");
            }
            cadena.AppendLine("<a alt='DetallePago' href='" + RelativeURLHelper.ToFullUrl("PagoServicios/Details/" + p.PagoId.ToString()) + "'>Ver detalle de este pago</a>");

            return cadena.ToString();
        }

        [NonAction]
        private string getMensajeConfirmacion(Pago p)
        {
            // emailTemplate= emailTemplate.Replace("@logoUrl", RelativeURLHelper.ToFullUrl(p.PayCenter.Logotipo));

            string Logo = "<img src=\"" + string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority) + p.PayCenter.Logotipo + "\" />";
            string emailTemplate = System.IO.File.ReadAllText(Server.MapPath("~/Content/Templates/TicketTemplate.htm"));
            emailTemplate = emailTemplate.Replace("@logoUrl", Logo);
            emailTemplate = emailTemplate.Replace("@Fecha", p.FechaCreacion.ToString());
            emailTemplate = emailTemplate.Replace("@Folio", p.Ticket.Folio);
            emailTemplate = emailTemplate.Replace("@Vendedor", p.Ticket.PayCenterName);
            emailTemplate = emailTemplate.Replace("@Servicio", p.Servicio);
            emailTemplate = emailTemplate.Replace("@Cliente", p.ClienteNombre);
            emailTemplate = emailTemplate.Replace("@Leyenda", p.Ticket.Leyenda);
            emailTemplate = emailTemplate.Replace("@Footer", "ESTE COMPROBANTE NO ES VÁLIDO PARA EFECTOS FISCALES EN TERMINOS DE OFICIO DE NO 325-SAT-VII-B-2650 DE FECHA 1 DE DICIEMBRE DE 1997");
            emailTemplate = emailTemplate.Replace("@Importe", p.Importe.ToString());

            StringBuilder detallePago = new StringBuilder();
            foreach (DetallePago d in p.DetallePagos)
            {
                detallePago.AppendLine("<tr><td style=\"font-size: .7em;width: 38%;vertical-align: text-top;margin: 0;position: relative;color: #aaa;text-align: left;\">"
                + d.Campo + "</td><td style=\"margin:0;width:38%;font-size: .8em!important;text-align: left;\">" + d.Valor + "</td></tr></tr>");
            }
            emailTemplate = emailTemplate.Replace("@DynamicFields", detallePago.ToString());

            return emailTemplate;
        }

        [HttpPost]
        public string GetPagoServicios(ServiceParameterVM parameters)
        {
            var pagoServiciosResult = getPagosServicio(parameters);
            return Newtonsoft.Json.JsonConvert.SerializeObject(pagoServiciosResult);
        }

     }
}
