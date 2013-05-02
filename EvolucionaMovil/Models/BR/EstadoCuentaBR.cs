using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Repositories;
using EvolucionaMovil.Models.Enums;
using System.Data.Objects;

namespace EvolucionaMovil.Models.BR
{
    /// <summary>
    /// Esta clase centraliza las reglas de negocios que tienen que ver con el Manejo de Estados de cuenta y Saldos.
    /// Hace referencia al documento BR01_Manejo_De_Estados_De_Cuenta_y_Saldos
    /// </summary>
    internal class EstadoCuentaBR : cabinet.patterns.clases.BusinessRulesBase<Movimiento>
    {
        private EvolucionaMovilBDEntities _context;
        private EstadoDeCuentaRepository _estadoDeCuentaRepository;
        private EstadoDeCuentaRepository estadoDeCuentaRepository
        {
            get
            {
                if (_estadoDeCuentaRepository == null)
                {
                    _estadoDeCuentaRepository = new EstadoDeCuentaRepository(_context);
                }
                return _estadoDeCuentaRepository;
            }
        }
        
        /// <summary>
        /// Permite manejar la creación, consulta e interacción con los movimientos del estado de cuenta.
        /// </summary>
        /// <param name="Context">El Context debe ser el mismo que utilice el controller para poder crear todo en transacción. Si no se le pasa controller, genera registros directamente.</param>
        public EstadoCuentaBR(EvolucionaMovilBDEntities Context)
        {
            _context = Context;
        }

        /// <summary>
        /// Permite manejar la creación, consulta e interacción con los movimientos del estado de cuenta.
        /// </summary>
        /// <param name="Context">El Context debe ser el mismo que utilice el controller para poder crear todo en transacción. Si no se le pasa controller, genera registros directamente.</param>
        public EstadoCuentaBR()
        {
        }

        #region Saldos

        /// <summary>
        /// Obtiene los saldos de los movimientos de un PayCenter para pago de servicios
        /// </summary>
        /// <remarks>BR01.01.A,BR01.01.B,BR01.01.C,BR01.01.D,BR01.01.E</remarks>
        /// <param name="PayCenterId">Id del PayCenter</param>
        /// <returns>Objeto SaldosPagoServicio con los saldos según los movimientos registrados del PayCenter</returns>
        internal SaldosPagoServicio GetSaldosPagoServicio(int PayCenterId)
        {
            var movimientos = estadoDeCuentaRepository.GetMovimientos(enumTipoCuenta.Pago_de_Servicios.GetHashCode(), PayCenterId);
            SaldosPagoServicio saldosPagoServicio = new SaldosPagoServicio
            {
                SaldoActual = (movimientos.Where(x => x.IsAbono && x.Status == enumEstatusMovimiento.Aplicado.GetHashCode()).Sum(x => x.Monto) - movimientos.Where(x => !x.IsAbono && x.Status == enumEstatusMovimiento.Aplicado.GetHashCode()).Sum(x => x.Monto)),
                SaldoPorAbonar = movimientos.Where(x => x.IsAbono && x.Status == enumEstatusMovimiento.Procesando.GetHashCode()).Sum(x => x.Monto),
                SaldoPorCobrar = movimientos.Where(x => !x.IsAbono && x.Status == enumEstatusMovimiento.Procesando.GetHashCode()).Sum(x => x.Monto),
           };
            saldosPagoServicio.SaldoPendiente = saldosPagoServicio.SaldoPorAbonar - saldosPagoServicio.SaldoPorCobrar;
            saldosPagoServicio.SaldoDisponible = saldosPagoServicio.SaldoActual - saldosPagoServicio.SaldoPorCobrar;
            return saldosPagoServicio;
        }

        #endregion

        #region Movimientos

        /// <summary>
        /// Genera el movimiento y genera estatus y clave según condificiones de los BR
        /// </summary>
        /// <param name="PayCenterId"></param>
        /// <param name="TipoMovimiento"></param>
        /// <param name="Id">Identificador de la entidad que está generando el Movimiento (Deposito, Compra, ATraspaso, etc</param>
        /// <param name="CuentaId"></param>
        /// <param name="Monto"></param>
        /// <param name="Motivo"></param>
        /// <param name="?"></param>
        /// <remarks>BR01.02</remarks>
        /// <returns></returns>
        internal Movimiento CrearMovimiento(int PayCenterId, enumTipoMovimiento TipoMovimiento, int Id, int CuentaId, decimal Monto, enumMotivo Motivo){
            Movimiento movimiento = new Movimiento();

            movimiento.Clave = DateTime.Now.ToString("yyyyMMdd") + "0" + ((Int16)Motivo).ToString() + new Random().Next(0,99999).ToString();
            movimiento.CuentaId = CuentaId;
            movimiento.FechaCreacion = DateTime.Now;
            movimiento.IsAbono = TipoMovimiento==enumTipoMovimiento.Abono;
            movimiento.Monto = Monto;
            movimiento.Motivo = (Int16)Motivo;
            movimiento.PayCenterId = PayCenterId;
            movimiento.Status = (Int16)enumEstatusMovimiento.Procesando;
            movimiento.Id = Id;
            estadoDeCuentaRepository.Add(movimiento);
            if (_context == null)
            {
                estadoDeCuentaRepository.Save();
            }
            return movimiento;
        }

        #endregion

        #region Depósitos

        /// <summary>
        /// Valida que no exista registros con la Referencia en el Banco especificado
        /// </summary>
        /// <remarks>BR01.03.A:La Referencia bancaria indicada en los movimientos de tipo Depósito no deben repetirse a menos que tengan estatus diferente a Procesando o Aplicado o que sean de diferente BancoId</remarks>
        /// <param name="Referencia">Número de referencia Bancaria</param>
        /// <param name="BancoId">ID del banco al que pertenece</param>
        /// <returns>True si la referencia no está repetida según la regla, y False en caso de que no</returns>
        internal bool IsValidReferenciaDeposito(String Referencia, Int32 BancoId)
        {
            var statusProcesando = (Int16)enumEstatusMovimiento.Procesando.GetHashCode();
            var statusAplicado = (Int16)enumEstatusMovimiento.Aplicado.GetHashCode();
            return !estadoDeCuentaRepository.context.Abonos.Any(x => x.Referencia == Referencia && x.BancoId == BancoId && (x.Status == statusProcesando || x.Status == statusAplicado));
        }

        #endregion
        
        #region Cambio de Estatus



        #endregion

        #region Financiamiento



        #endregion

        #region Referenciados



        #endregion
    }
     
    public class SaldosPagoServicio{
        /// <summary>
        /// El Saldo Actual es el resultante de la suma de todos lo movimientos de tipo Abono y la resta de los movimientos de tipo Cargo, ambos con estatus Aplicado
        /// </summary>
        public decimal SaldoActual {get;set;}
        /// <summary>
        /// El Saldo Por Abonar es el resultante de la suma de todos los movimientos de tipo Cargo con estatus Procesando.
        /// </summary>
        public decimal SaldoPorAbonar {get;set;}
        /// <summary>
        /// El Saldo Por Cobrar es el resultante de la suma de todos los movimientos de tipo Cargo con estatus Procesando.
        /// </summary>
        public decimal SaldoPorCobrar {get;set;}
        /// <summary>
        /// El Saldo Pendiente es el resultante de la resta del Saldo Por Abonar menos el Saldo Por Cobrar.
        /// </summary>
        public decimal SaldoPendiente {get;set;}
        /// <summary>
        /// El Saldo Disponible es el resultante de la resta del Saldo Actual menos el Saldo Por Cobrar.
        /// </summary>
        public decimal SaldoDisponible {get;set;}
    }

}
