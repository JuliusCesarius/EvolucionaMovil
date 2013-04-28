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
        public ViewResult Details(PagoVM pagoVM)
        {
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
