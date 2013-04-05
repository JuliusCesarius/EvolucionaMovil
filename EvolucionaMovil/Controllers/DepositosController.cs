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

namespace EvolucionaMovil.Controllers
{ 
    public class DepositosController : Controller
    {
        private List<string> Mensajes = new List<string>();
        private AbonoRepository repository = new AbonoRepository();

        private EstadoCuentaBR validations = new EstadoCuentaBR();

        //
        // GET: /Depositos/

        public ViewResult Index()
        {
            //Modificación de prueba José
            var abonos = repository.ListAll().ToListOfDestination<AbonoVM>();
           
            return View(abonos.ToList());
        }

        //
        // GET: /Depositos/Details/5

        public ViewResult Details(int id)
        {
            Abono abono = repository.LoadById(id);
            BancosRepository bancosRepository = new BancosRepository();
           var banco = bancosRepository.LoadById(abono.BancoId);
            
            //fill estatus movimientos
            //Todo: Son solo de pruebas.
           EstadoDeCuentaRepository estadoDeCuentaRepository = new EstadoDeCuentaRepository();
           var movimiento = estadoDeCuentaRepository.LoadById(2);
           AbonoVM abonoVM = new AbonoVM
           {
               Banco = banco.Nombre,
               CuentaBancaria = banco.CuentasBancarias.Where(x => x.CuentaId == abono.CuentaBancariaId).FirstOrDefault().NumeroCuenta,
               Status= abono.Status, //((enumEstatusMovimiento)abono.Status).ToString(),
               FechaCreacion = abono.FechaCreacion,
               FechaPago = abono.FechaPago,
               MontoString = abono.Monto.ToString("C"),
               PayCenter = abono.PayCenter.UserName,
               Referencia = abono.Referencia,
               TipoCuenta = ((enumTipoCuenta)abono.Cuenta.TipoCuenta).ToString(), // .Where(x => x.CuentaId == abono.CuentaId).FirstOrDefault().TipoCuenta,   //"[FALTA OBTENER TIPO CUENTA]",(enumTipoCuenta)abono.).ToString(),
               HistorialEstatusVM = movimiento.Movimientos_Estatus.Select(x=> new HistorialEstatusVM{Fecha = x.FechaCreacion.ToLongDateString(), Estatus =((enumEstatusMovimiento)x.Status).ToString(),Comentarios = x.Comentarios, UserName = x.UserName }).ToList() 
           };

            //TODO:Leer el usuario que viene en la sesión

           var roles = Roles.GetRolesForUser("staff");
            int Role =0;
            if (roles.Any(x=>x==EnumRoles.PayCenter.ToString())){
                Role = EnumRoles.PayCenter.GetHashCode();
            }
            else if (roles.Any(x => x == EnumRoles.Staff.ToString() || x == EnumRoles.Administrator.ToString()))
            {
                Role = EnumRoles.Staff.GetHashCode();
            }
            ViewBag.Role = Role;
            return View(abonoVM);
        }
        [HttpPost]
        public ViewResult Details(int id, string action)
        {
            //Aquí van las acciones del PayCenter y Staf para el depósito
            switch (action)
            {
                case "Cancelar":
                  //  abonoVM.Status = (short)(enumEstatusMovimiento.Cancelado.GetHashCode());
                    //Validar el Role del Uusario conectado
                    //crear ParametrosRepository y crear instancia para obtener el parametro de MinutosProrrogaCancelacion
                    //No es necesario pasar el context (transación) porque solo sirve para consultar.
                    //Validar que el estatus actual del abono sea Procesando
                    //Si ya pasaron los minutos de prorroga se dispara la excepción con un Throw Ex("No es posible cancelar por eltiempo... blablabla");

                    Abono abono = repository.LoadById(id);
                    abono.Status = (short)(enumEstatusMovimiento.Cancelado.GetHashCode());
                    var movimiento = abono.Cuenta.Movimientos.Where(x => x.Motivo == enumMotivo.Abono.GetHashCode() && x.Id == abono.AbonoId).FirstOrDefault();
                    //validar que exista el moviento y sino mandar mensaje de error
                    // poner en view bag
                    if (movimiento != null)
                    {
                        movimiento.Status = abono.Status;
                        Movimientos_Estatus movimiento_Estatus = new Movimientos_Estatus()
                        {
                            CuentaId = abono.CuentaId
                            //seguir llenado

                        };
                        movimiento.Movimientos_Estatus.Add(movimiento_Estatus);
                    }

                    repository.Save();
                    //Llenar el VM con el método de llenado
                    //return View(abonoVM)
                    break;
                case "Aplicar":
                    break;
                case "Rechazar":
                    break;
            }
            return View();
          //  AbonoVM abonoVM = new AbonoVM
          //{

          //};
            //Todo: Son solo de pruebas.
           //EstadoDeCuentaRepository estadoDeCuentaRepository = new EstadoDeCuentaRepository();
           //var movimiento = estadoDeCuentaRepository.LoadById(2);
           //abonoVM.HistorialEstatusVM = movimiento.Movimientos_Estatus.Select(x => new HistorialEstatusVM { Fecha = x.FechaCreacion.ToLongTimeString(), Estatus = ((enumEstatusMovimiento)x.Status).ToString(), Comentarios = x.Comentarios }).ToList();
          //  return View(abonoVM);

            
        }

        public ActionResult Report()
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
        public ActionResult Report(ReporteDepositoVM model)
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
                    Status = (Int16)enumEstatusMovimiento.Procesando,
                    FechaCreacion = DateTime.Now,
                    FechaPago = (DateTime)model.Fecha,
                    Monto = (Decimal)model.Monto,
                    PayCenterId = 1,
                    Referencia = model.Referencia
                };
                repository.Add(abono);
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

                EstadoDeCuentaRepository estadoDeCuentaRepository = new EstadoDeCuentaRepository(repository.context);

                foreach (var cuentaDepositoVM in model.CuentasDeposito.Where(x => x.Monto > 0))
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

                    estadoDeCuentaRepository.Add(movimiento);
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

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }
    }
}