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

namespace EvolucionaMovil.Controllers
{
    public class PagoServiciosController : Controller
    {
        private PagoServiciosRepository repository = new PagoServiciosRepository();
        private ServiciosRepository sRepository = new ServiciosRepository();
        private MovimientosRepository mRepository = new MovimientosRepository();
        private TicketRepository tRepository = new TicketRepository ();
        private PayCentersRepository pRepository = new PayCentersRepository();
        private ParametrosRepository parRepository = new ParametrosRepository();

        //
        // GET: /PagoServicios/

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

        public ViewResult Ticket(TicketVM tVM)
        {
            return View(tVM);
        }

        [HttpPost]
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
            Mapper.CreateMap<Pago, PagoVM>().ForMember(dest => dest.Servicios, opt => opt.Ignore());
            Mapper.Map(pago, pagoVM);
            pagoVM.ServicioNombre = pagoVM.Servicios.Where(x => x.Value == pago.ServicioId).FirstOrDefault().Text;

            //TODO:Leer el usuario que viene en la sesión
            int RoleUser = GetRolUser("staff");
            ViewBag.Role = RoleUser;

            return View(pagoVM);
        }

        [HttpPost]
        public ViewResult Details(PagoVM p) //Con esto se actualiza el status del pago
        {
            //Aquí van las acciones del PayCenter y Staf para el depósito
            var id = p.PagoId;
            var action = p.CambioEstatusVM.Estatus;
            string comentario = p.CambioEstatusVM.Comentario.TrimEnd();
            Pago pago = repository.LoadById(id);
            if (id > 0)
            {
                //Validar que el estatus actual del abono no este cancelado
                if (pago.Status != enumEstatusMovimiento.Cancelado.GetHashCode())
                {
                    //crear ParametrosRepository y crear instancia para obtener el parametro de MinutosProrrogaCancelacion                   
                    short minutosProrrogaCancelacion = parRepository.ListAll().FirstOrDefault().MinutosProrrogaCancelacion;

                    Boolean ComentarioValido = false;
                    Boolean UsuarioValido = false;
                    int Role = GetRolUser("staff");
                    Movimiento movimiento = mRepository.LoadById(pago.MovimientoId);
                    //validar que exista el moviento y sino mandar mensaje de error

                    if (movimiento != null)
                    {
                        switch (action)
                        {
                            case "Cancelar":
                                //Validar que el estatus actual del abono sea Procesando
                                if (pago.Status == enumEstatusMovimiento.Procesando.GetHashCode())
                                {
                                    //Si ya pasaron los minutos de prorroga se dispara la excepción con un Throw Ex("No es posible cancelar por eltiempo... blablabla");
                                    TimeSpan ts = DateTime.Now - pago.FechaCreacion;
                                    if (minutosProrrogaCancelacion > ts.TotalMinutes)
                                    {
                                        //Validar el Role del Usario conectado
                                        if (Role == EnumRoles.PayCenter.GetHashCode())
                                        {
                                            pago.Status = (short)(enumEstatusMovimiento.Cancelado.GetHashCode());
                                            //ViewBag.Mensaje = "El reporte de depósito ha sido cancelado exitosamente.";
                                            UsuarioValido = true;
                                            ComentarioValido = comentario.TrimEnd() != string.Empty ? true : false;
                                        }
                                    }
                                    else
                                        ViewBag.Mensaje = "No es posible cancelar el abono ya que ha expirado el tiempo de prórroga.";
                                }
                                else
                                    ViewBag.Mensaje = "No se puede cancelar el depósito sino esta en estatus de procesando.";

                                break;
                            case "Aplicar":

                                //Validar el Role del Usario conectado
                                if (Role == EnumRoles.Staff.GetHashCode() || Role == EnumRoles.Administrator.GetHashCode())
                                {

                                    UsuarioValido = true;
                                    ComentarioValido = true;
                                    //Validar que el estatus actual del abono sea Rechazado, entonces necesitamos el comentario
                                    if (pago.Status == enumEstatusMovimiento.Rechazado.GetHashCode() && comentario == string.Empty)
                                    {
                                        ComentarioValido = false;
                                    }
                                    else
                                    {
                                        if (pago.Status != enumEstatusMovimiento.Aplicado.GetHashCode())
                                        {
                                            pago.Status = (short)(enumEstatusMovimiento.Aplicado.GetHashCode());
                                            // ViewBag.Mensaje = "Se ha guardado exitosamente.";
                                        }
                                        else
                                            ViewBag.Mensaje = "No se puede Aplicar el depósito porque ya esta en estatus de aplicado.";


                                    }

                                }

                                break;
                            case "Rechazar":
                                //Validar el Role del Usario conectado
                                if (Role == EnumRoles.Staff.GetHashCode() || Role == EnumRoles.Administrator.GetHashCode())
                                {
                                    UsuarioValido = true;
                                    ComentarioValido = comentario.TrimEnd() != string.Empty ? true : false;
                                    if (pago.Status != enumEstatusMovimiento.Rechazado.GetHashCode())
                                    {

                                        pago.Status = (short)(enumEstatusMovimiento.Rechazado.GetHashCode());
                                        //ViewBag.Mensaje = "Se ha guardado exitosamente.";
                                    }
                                    else
                                        ViewBag.Mensaje = "No se puede rechazar el depósito porque ya esta en estatus de rechazado.";
                                }
                                break;
                        }
                        // valida usuario
                        if (UsuarioValido)
                        {

                            //Valida comendario
                            if (ComentarioValido)
                            {
                                if (ViewBag.Mensaje == null || ViewBag.Mensaje == string.Empty)
                                {
                                    movimiento.Status = pago.Status;
                                    Movimientos_Estatus movimiento_Estatus = new Movimientos_Estatus()
                                    {
                                        CuentaId = 19, // TODO: Checar de donde sale esto.
                                        FechaCreacion = DateTime.Now,
                                        MovimientoId = movimiento.MovimientoId,
                                        PayCenterId = pago.PayCenterId,
                                        Status = movimiento.Status,
                                        UserName = "staff", //Todo: Cambiar el user y tomarlo de la sesion
                                        Comentarios = comentario
                                    };
                                    movimiento.Movimientos_Estatus.Add(movimiento_Estatus);
                                    repository.Save();
                                    if (pago.Status == enumEstatusMovimiento.Cancelado.GetHashCode())
                                    {
                                        ViewBag.Mensaje = "El reporte de depósito ha sido cancelado exitosamente.";
                                    }
                                    else
                                    {
                                        ViewBag.Mensaje = "Se ha guardado exitosamente.";
                                    }
                                }
                                p.CambioEstatusVM.Comentario = string.Empty;
                                p.CambioEstatusVM.Estatus = string.Empty;
                            }
                            else
                            {
                                ViewBag.Mensaje = "Es necesario agregar un comentario para poder asignar el estatus.";
                            }
                        }
                        else
                        {
                            ViewBag.Mensaje = "El usuario no es valido.";
                        }
                    }
                    else
                    {
                        ViewBag.Mensaje = "No se encontro el movimiento para el depósito.";
                    }
                    //****No es necesario pasar el context (transación) porque solo sirve para consultar.


                }
                else
                {
                    ViewBag.Mensaje = "No se puede " + action.ToLower() + " el depósito porque el estatus es cancelado.";
                }
            }
            else
            {
                ViewBag.Mensaje = "No existe el depósito.";
            }
            //Llenar el VM con el método de llenado
            PagoVM pagoVM = new PagoVM() ; //TODO: Llenar esto

            return View(pagoVM);
        }

        //
        // GET: /PagoServicios/Create

        public ActionResult Create()
        {
            PagoVM pVM = new PagoVM();
            //TODO: Esto hay que cambiarlo por el metodo generico que haga julio
            PaquetesRepository repository = new PaquetesRepository();
            ViewData["Eventos"] = repository.GetEventosByPayCenter(1);
            var repositoryEstadoDeCuenta = new Repositories.EstadoDeCuentaRepository();
            var edocuenta = repositoryEstadoDeCuenta.ListAll();
            ViewData["SaldoActual"] = (edocuenta.Where(x => x.IsAbono).Sum(x => x.Monto) - edocuenta.Where(x => !x.IsAbono).Sum(x => x.Monto)).ToString("C");

            return View(pVM);
        }

        //
        // POST: /PagoServicios/Create

        [HttpPost]
        public ActionResult Create(PagoVM pagoVM)
        {
            Pago pago = new Pago();
            if (ModelState.IsValid)
            {
                #region Leer Configuracion PayCenter
                PayCenter payCenter = pRepository.LoadById(15); //TODO: Cambiar esto a dinamico
                #endregion

                #region Crear Movimiento Inicial
                Movimiento mov = new Movimiento();
                mov.PayCenterId = 1;
                mov.CuentaId = 24;// Cuenta a la que pertenece
                mov.Monto = (Decimal)pagoVM.Importe;
                mov.Id = 1; //Id de la tabla de pagos
                mov.IsAbono = false;
                mov.CuentaOrigenId = 24; //Identificador de la cuenta desde la
                                         //cual se generó el movimiento en caso
                                         //de que así aplique. Normalmente
                                         //aplica con traspasos o comisiones
                mov.Status = (short)enumEstatusMovimiento.Procesando;
                mov.SaldoActual = 0;    //Monto del saldo actual de todos los
                                        //movimientos aplicados de la misma
                                        //cuenta. Sirve para no perder registro
                                        //del saldo en el momento del
                                        //movimiento. Mientras el Status no
                                        //esté Aplicado, el Saldo actual no se
                                        //verá afectado.;
                mov.Clave = "000000";   //Clave que permite identificar el
                                        //movimiento al usuario. Está formado
                                        //por 13 dígitos YYMMDDTTCCCCC,
                                        //donde:
                                        //YY = 2 últimos digitos del año (ej.13),
                                        //MM = número del mes (ej. 03)
                                        //DD = dia del mes (ej. 15)
                                        //TT = Tipo de cuenta (ej. 03)
                                        //CCCCC = Consecutivo (ej. 00356)

                mov.FechaCreacion = DateTime.Now;
                mov.Baja = false;
                mov.Motivo = (short)enumMotivo.Pago;

                mRepository.Add(mov);
                mRepository.Save();
                #endregion

                #region Registro de Pago
                Mapper.CreateMap<PagoVM, Pago>().ForMember(dest => dest.DetallePagos, opt => opt.Ignore());
                Mapper.Map(pagoVM, pago);
                pago.FechaCreacion = DateTime.Now;
                pago.Servicio = pagoVM.Servicios.Where(x => x.Value == pagoVM.ServicioId).FirstOrDefault().Text;
                pago.PayCenterId = 15; //TODO:Quitar esto y hacerlo dinamico;  
                pago.MovimientoId = mov.MovimientoId;
                
                var iDetalles = sRepository.ListAll().Where(x => x.ServicioId == pago.ServicioId).FirstOrDefault().DetalleServicios;
                foreach (DetalleServicio d in iDetalles)
                {
                    var x1 = Request.Form[d.Campo];
                    pago.DetallePagos.Add(new DetallePago { Campo = d.Campo, Valor = x1 });
                }
                repository.Add(pago);
                repository.Save();
                #endregion

                #region Registro Ticket
                Ticket tVM = new Ticket();
                tVM.Baja = false;
                tVM.ClienteEmail = "";
                tVM.ClienteNombre = pago.ClienteNombre;
                tVM.ClienteTelefono = "";
                tVM.Comision = (decimal)payCenter.Parametros.ComisionPayCenter; //Comision configurada del paycenter
                tVM.FechaCreacion = DateTime.Now;              
                tVM.Folio = createFolio(pago.PagoId);
                tVM.Importe = pago.Importe;
                tVM.Leyenda = "";
                tVM.PagoId = pago.PagoId;
                tVM.PayCenterId = pago.PayCenterId;
                tVM.Referencia = "";
                tVM.TipoServicio = pago.Servicio;                
               
                tRepository.Add(tVM);
                tRepository.Save();

                TicketVM ticketVM = new TicketVM();
                Mapper.CreateMap<Ticket, TicketVM>().ForMember(dest => dest.Pago, opt => opt.Ignore());
                Mapper.Map(tVM , ticketVM);
                ticketVM.FechaPago = pago.FechaCreacion;
                #endregion

                return RedirectToAction("Ticket",ticketVM );
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
                     Folio = x.Ticket != null ? x.Ticket.Folio : string.Empty,
                     Servicio = x.Servicio,
                     NombreCliente = x.ClienteNombre,
                     PayCenterName = x.PayCenter != null ? x.PayCenter.Nombre : "[Desconocido]",
                     PagoId = x.PagoId,
                     //todo:Optimizar esta consulta para que no haga un load por cada registro que levante.
                     Comentarios = "TODO:Comentarios",
                     Monto = x.Importe.ToString("C"),
                     FechaCreacion = x.FechaCreacion.ToShortDateString(),
                     FechaVencimiento = x.FechaVencimiento.ToShortDateString(),
                     Status = ((enumEstatusMovimiento)x.Status).ToString()
                 });

            if (Parameters != null && Parameters.onlyAplicados)
                pagosServicioVM = pagosServicioVM.Where(x => x.Status == enumEstatusMovimiento.Aplicado.ToString());

            if (Parameters != null && !string.IsNullOrEmpty(Parameters.searchString))
                pagosServicioVM = pagosServicioVM.Where(x => x.NombreCliente.ToLower().Contains(Parameters.searchString.ToLower()) || x.Servicio.ToLower().Contains(Parameters.searchString.ToLower()));
            
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
            if (roles.Any(x => x == EnumRoles.PayCenter.ToString()))
            {
                Rol = EnumRoles.PayCenter.GetHashCode();
            }
            else if (roles.Any(x => x == EnumRoles.Staff.ToString() || x == EnumRoles.Administrator.ToString()))
            {
                Rol = EnumRoles.Staff.GetHashCode();
            }

            return Rol;
        }
    }
}
