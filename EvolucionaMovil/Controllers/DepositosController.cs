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
            var abonos = repository.ListAll().ToListOfDestination<AbonoVM>();
            return View(abonos.ToList());
        }

        //
        // GET: /Depositos/Details/5

        public ViewResult Details(int id)
        {
            Abono abono = repository.LoadById(id);
            AbonoVM abonoVM = new AbonoVM();
            Mapper.Map(abono, abonoVM);
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
                    EstatusId = enumEstatusMovimiento.Procesando.GetHashCode(),
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