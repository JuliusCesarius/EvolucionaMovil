﻿using System;
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
             AbonoVM abonoVM = FillAbonoVM(id);
            //TODO:Leer el usuario que viene en la sesión
           int RoleUser = GetRolUser("staff");
           ViewBag.Estatus = abonoVM.Status.GetHashCode();
            ViewBag.Role = RoleUser;
            return View(abonoVM);
        }


        [HttpPost]
        public ViewResult Details(AbonoVM model)
        {
            //Aquí van las acciones del PayCenter y Staf para el depósito
            var id = model.AbonoId ;
            var action =model.CambioEstatusVM.Estatus ;
            string comentario =model.CambioEstatusVM.Comentario  ;
             Abono abono = repository.LoadById(id);
             if (id > 0)
             {
                 //Validar que el estatus actual del abono sea Procesando
                 if (abono.Status == enumEstatusMovimiento.Procesando.GetHashCode())
                 {                    
                     //crear ParametrosRepository y crear instancia para obtener el parametro de MinutosProrrogaCancelacion
                     ParametrosRepository parametrosRepository = new ParametrosRepository();
                     short MinutosProrrogaCancelacion = parametrosRepository.ListAll().FirstOrDefault().MinutosProrrogaCancelacion;
                     //Si ya pasaron los minutos de prorroga se dispara la excepción con un Throw Ex("No es posible cancelar por eltiempo... blablabla");
                     TimeSpan ts =  DateTime.Now - abono.FechaCreacion;
                     if (MinutosProrrogaCancelacion > ts.TotalMinutes)
                     {
                         Boolean ComentarioValido = false ;
                         Boolean UsuarioValido = false;
                         int Role = GetRolUser("staff");
                         var movimiento = abono.Cuenta.Movimientos.Where(x => x.Motivo == enumMotivo.Deposito.GetHashCode() && x.Id == abono.AbonoId).FirstOrDefault();
                         //validar que exista el moviento y sino mandar mensaje de error
                                                
                         if (movimiento != null)
                         {
                             switch (action)
                             {
                                 case "Cancelar":
                                     //Validar el Role del Usario conectado
                                     if (Role == EnumRoles.PayCenter.GetHashCode())
                                     {
                                         abono.Status = (short)(enumEstatusMovimiento.Cancelado.GetHashCode());
                                         ViewBag.mensage = "El reporte de depósito ha sido Cancelado exitosamente.";
                                         UsuarioValido = true;
                                         ComentarioValido = comentario.TrimEnd() != string.Empty ? true : false;
                                     }
                                    
                                     break;
                                 case "Aplicar":
                                
                                     //Validar el Role del Usario conectado
                                     if (Role == EnumRoles.Staff.GetHashCode() || Role == EnumRoles.Administrator.GetHashCode())
                                     {
                                         abono.Status = (short)(enumEstatusMovimiento.Aplicado.GetHashCode());
                                         ViewBag.mensage = "Se ha guardado exitosamente.";
                                         ComentarioValido = true;
                                         UsuarioValido = true;
                                     }
                                     
                                     break;
                                 case "Rechazar":
                                     //Validar el Role del Usario conectado
                                     if (Role == EnumRoles.Staff.GetHashCode() || Role == EnumRoles.Administrator.GetHashCode())
                                     {
                                         abono.Status = (short)(enumEstatusMovimiento.Rechazado.GetHashCode());
                                         ViewBag.mensage = "Se ha guardado exitosamente.";
                                         UsuarioValido = true;
                                         ComentarioValido = comentario.TrimEnd() != string.Empty ? true : false;
                                     }
                                     break;
                             }
                             // valida usuario
                             if(UsuarioValido){

                                 //Valida comendario
                                 if (ComentarioValido)
                                 {  
                                     movimiento.Status = abono.Status;
                                     Movimientos_Estatus movimiento_Estatus = new Movimientos_Estatus()
                                     {
                                         CuentaId = abono.CuentaId,
                                         FechaCreacion = DateTime.Now,
                                         MovimientoId = movimiento.MovimientoId,
                                         PayCenterId = abono.PayCenterId,
                                         Status = movimiento.Status,
                                         UserName = "staff", //Todo: Cambiar el user y tomarlo de la sesion
                                        Comentarios  = comentario 
                                     };
                                     movimiento.Movimientos_Estatus.Add(movimiento_Estatus);
                                     repository.Save();
                                     model.CambioEstatusVM.Comentario = string.Empty;
                                     model.CambioEstatusVM.Estatus  = string.Empty;
                                 }
                                 else{
                                     ViewBag.mensage = "Es necesario agregar un comentario para poder asignar el status.";
                                    }
                             }
                             else
                             {
                                ViewBag.mensage = "El usuario no es valido.";
                             }
                         }
                         else
                         {
                             ViewBag.mensage = "No se encontro el movimiento para el Depósito.";
                         }
                         //****No es necesario pasar el context (transación) porque solo sirve para consultar.
                     }
                     else
                     {
                         ViewBag.mensage = "No es posible cancelar el abono ya que ha expirado el tiempo de prórroga.";
                     }

                 }
                 else
                 {
                     ViewBag.mensage = "No se puede " + action + " el Depósito sino esta en estatus Procesando.";
                 }
             }
             else {
                 ViewBag.mensage = "No existe el Depósito.";
             }
           //Llenar el VM con el método de llenado
            AbonoVM abonoVM = FillAbonoVM(id);
           
            return View(abonoVM);    
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
            exito = validations.IsValidReferenciaDeposito(model.Referencia, model.BancoId);
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

                EstadoCuentaBR estadoCuentaBR = new EstadoCuentaBR(repository.context);

                foreach (var cuentaDepositoVM in model.CuentasDeposito.Where(x => x.Monto > 0))
                {
                    //TODO:Leer el usuario en este caso HttpResponseSubstitutionCallback PayCenter ();
                    estadoCuentaBR.CrearMovimiento(
                        1,
                        enumTipoMovimiento.Abono,
                        cuentaDepositoVM.CuentaId,
                        cuentaDepositoVM.Monto,
                        enumMotivo.Deposito
                      );
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
            EstadoDeCuentaRepository estadoDeCuentaRepository = new EstadoDeCuentaRepository();
            int movimientoId = 0;
            var movimiento = abono.Cuenta.Movimientos.Where(x => x.CuentaId == abono.CuentaId && x.Motivo == enumMotivo.Deposito.GetHashCode() && x.PayCenterId == abono.PayCenterId && x.Id == abono.AbonoId).FirstOrDefault();
            if (movimiento != null)
            {
                movimientoId = movimiento.MovimientoId;
            }
            else {
                ViewBag.mensage = "No existe el movimiento para el Depósito.";
            }
          //  var movimiento = estadoDeCuentaRepository.LoadById(movimientoId);
            AbonoVM abonoVM = new AbonoVM
            {
                AbonoId =id,
                Banco = banco.Nombre,
                CuentaBancaria = banco.CuentasBancarias.Where(x => x.CuentaId == abono.CuentaBancariaId).FirstOrDefault().NumeroCuenta,
                Status = abono.Status, //((enumEstatusMovimiento)abono.Status).ToString(),
                FechaCreacion = abono.FechaCreacion,
                FechaPago = abono.FechaPago,
                MontoString = abono.Monto.ToString("C"),
                PayCenter = abono.PayCenter.UserName,
                Referencia = abono.Referencia,
                TipoCuenta = ((enumTipoCuenta)abono.Cuenta.TipoCuenta).ToString(), 
                HistorialEstatusVM = movimiento != null ? movimiento.Movimientos_Estatus.Select(x => new HistorialEstatusVM { Fecha = x.FechaCreacion.ToString(), Estatus = ((enumEstatusMovimiento)x.Status).ToString(), Comentarios = x.Comentarios }).ToList() : null 
            };
            return abonoVM;
        }
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

  
        #endregion
    }
}