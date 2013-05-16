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
        private TicketRepository tRepository = new TicketRepository ();
        private PayCentersRepository pRepository = new PayCentersRepository();
        private ParametrosRepository parRepository = new ParametrosRepository();
        private EstadoDeCuentaRepository eRepository = new EstadoDeCuentaRepository();
        private PaquetesRepository pqrepository = new PaquetesRepository();
        private CultureInfo ci = new CultureInfo("es-MX");
        #endregion

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff })]
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
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff })]
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

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff })]
        public ViewResult Details(int id)
        {
            PagoVM pagoVM = FillPagoVM(id);   
            int RoleUser = GetRolUser(HttpContext.User.Identity.Name);
            ViewBag.Role = RoleUser;

            return View(pagoVM);
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff })]
        public ViewResult Details(PagoVM model) //Con esto se actualiza el status del pago
        {
            //Aquí van las acciones del PayCenter y Staf para el depósito
            var id = model.PagoId ;
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
                    Succeed = estadoCuentaBR.Succeed ;
                    ValidationMessages = estadoCuentaBR.ValidationMessages;

                    if (Succeed)
                    {
                        pago.Status = movimiento.Status;             
                        Succeed = repository.Save();                        
                        if (Succeed)
                        {
                            AddValidationMessage(enumMessageType.Message, "El reporte de depósito ha sido " + nuevoEstatus.ToString() + " correctamente");
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

            PagoVM  pagoVM = FillPagoVM(id);
            return View(pagoVM);
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff })]
        public ActionResult Create()
        {
            var paycenter = GetPayCenter();
            PagoVM pVM = new PagoVM();           
 
            EstadoCuentaBR br = new EstadoCuentaBR(repository.context);
            var saldo = br.GetSaldosPagoServicio(paycenter.PayCenterId); 

            ViewData["Eventos"] = pqrepository.GetEventosByPayCenter(paycenter.PayCenterId);
            ViewData["SaldoActual"] = saldo.SaldoActual;

            return View(pVM);
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff })]
        public ActionResult Create(PagoVM model)
        {
            Pago pago = new Pago();
            if (ModelState.IsValid)
            {
                #region Leer Configuracion PayCenter
                PayCenter payCenter = GetPayCenter(12);
                #endregion

                #region Crear Movimiento Inicial
                EstadoCuentaBR br = new EstadoCuentaBR(repository.context);
                var cuenta = payCenter.Cuentas.Where(c => c.TipoCuenta == (int)enumTipoCuenta.Pago_de_Servicios).FirstOrDefault();
                Movimiento mov = br.CrearMovimiento(payCenter.PayCenterId, enumTipoMovimiento.Cargo, 0, cuenta.CuentaId, (Decimal)model.Importe, enumMotivo.Pago);              
                #endregion               

                #region Registro de Pago
                string Referencia = "";
                Mapper.CreateMap<PagoVM, Pago>().ForMember(dest => dest.DetallePagos, opt => opt.Ignore());
                Mapper.Map(model, pago);
                pago.FechaCreacion = DateTime.Now;
                pago.Servicio = model.Servicios.Where(x => x.Value == model.ServicioId).FirstOrDefault().Text;
                pago.PayCenterId = payCenter.PayCenterId; //TODO:Quitar esto y hacerlo dinamico;  
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
                Ticket tVM = new Ticket();
                tVM.Baja = false;
                tVM.ClienteEmail = "";
                tVM.ClienteNombre = pago.ClienteNombre;
                tVM.ClienteTelefono = "";
                tVM.Comision = payCenter.Parametros.ComisionPayCenter != null ? (Decimal)payCenter.Parametros.ComisionCliente : 0; //Comision configurada del paycenter
                tVM.FechaCreacion = DateTime.Now;              
                tVM.Folio = createFolio(pago.PagoId);
                tVM.Importe = pago.Importe;
                tVM.Leyenda = "";
                tVM.PagoId = pago.PagoId;
                tVM.PayCenterId = pago.PayCenterId;
                tVM.Referencia = "";
                tVM.TipoServicio = pago.Servicio;
                tVM.Referencia = Referencia;
               
                tRepository.Add(tVM);
                tRepository.Save();
                #endregion

                return RedirectToAction("Ticket/" + tVM.PagoId.ToString());
            }

            return View(model);
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff })]
        public ViewResult Ticket(int id)
        {
            Ticket tVM = tRepository.ListAll().Where(x => x.PagoId == id).FirstOrDefault();
            TicketVM ticketVM = new TicketVM();
            Mapper.CreateMap<Ticket, TicketVM>().ForMember(dest => dest.Pago, opt => opt.Ignore());
            Mapper.Map(tVM, ticketVM);
            ticketVM.FechaPago = tVM.Pago.FechaCreacion;
            tVM.Pago.DetallePagos.ToList().ForEach(x => ticketVM.DetallePagos.Add(x));

            return View(ticketVM);
        }

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        private SimpleGridResult<PagoServicioVM> getPagosServicio(ServiceParameterVM Parameters = null)
        {
            var pagos = repository.ListAll();
            var movimientos = eRepository.GetMovimientos();
            EstadoCuentaBR br = new EstadoCuentaBR(repository.context);

            var paycenter = GetPayCenter();

            SimpleGridResult<PagoServicioVM> simpleGridResult = new SimpleGridResult<PagoServicioVM>();
            var pagosServicioVM = pagos.OrderByDescending(y=> y.PagoId).Where(x => Parameters == null
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
        private PayCenter GetPayCenter(int Id=0)
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
        private PagoVM FillPagoVM (Int32 id)
        {
            Pago pago = repository.ListAll().Where(x => x.PagoId == id).FirstOrDefault();
            PagoVM pagoVM = new PagoVM();
            Mapper.CreateMap<Pago, PagoVM>().ForMember(dest => dest.Servicios, opt => opt.Ignore());
            Mapper.Map(pago, pagoVM);
            pagoVM.ServicioNombre = pagoVM.Servicios.Where(x => x.Value == pago.ServicioId).FirstOrDefault().Text;

            foreach (Movimientos_Estatus m in pago.Movimiento.Movimientos_Estatus.OrderByDescending(x=> x.Movimiento_EstatusId))
            {
                HistorialEstatusVM h = new HistorialEstatusVM();
                h.Comentarios = m.Comentarios ;
                h.Estatus = ((enumEstatusMovimiento)m.Status).ToString();
                h.Fecha = m.FechaCreacion.ToShortDateString();
                h.UserName = m.UserName;
                pagoVM.HistorialEstatusVM.Add(h);
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
