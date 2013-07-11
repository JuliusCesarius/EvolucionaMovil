﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Repositories;
using EvolucionaMovil.Models.Enums;
using System.Data.Objects;
using cabinet.patterns.interfaces;
using cabinet.patterns.clases;
using cabinet.patterns.enums;

namespace EvolucionaMovil.Models.BR
{
    /// <summary>
    /// Esta clase centraliza las reglas de negocios que tienen que ver con el Manejo de Estados de cuenta y Saldos.
    /// Hace referencia al documento BR01_Manejo_De_Estados_De_Cuenta_y_Saldos
    /// </summary>
    internal class EstadoCuentaBR : cabinet.patterns.clases.BusinessRulesBase<Movimiento>
    {
        private const int PROVEEDOR_EVOLUCIONAMOVIL = 1;        
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
            Succeed = true;
            var movimientos = estadoDeCuentaRepository.GetMovimientos(enumTipoCuenta.Pago_de_Servicios.GetHashCode(), PayCenterId);
            var abonosAplicados = movimientos.Where(x => x.IsAbono && x.Status == (short)enumEstatusMovimiento.Aplicado).Sum(x => x.Monto);
            var cargosAplicados = movimientos.Where(x => !x.IsAbono && x.Status == (short)enumEstatusMovimiento.Aplicado).Sum(x => x.Monto);
            SaldosPagoServicio saldosPagoServicio = new SaldosPagoServicio
            {
                SaldoActual = abonosAplicados - cargosAplicados,
                SaldoPorAbonar = movimientos.Where(x => x.IsAbono && x.Status == (short)enumEstatusMovimiento.Procesando).Sum(x => x.Monto),
                SaldoPorCobrar = movimientos.Where(x => !x.IsAbono && x.Status == (short)enumEstatusMovimiento.Procesando).Sum(x => x.Monto),
            };
            saldosPagoServicio.SaldoPendiente = saldosPagoServicio.SaldoPorAbonar - saldosPagoServicio.SaldoPorCobrar;
            saldosPagoServicio.SaldoDisponible = saldosPagoServicio.SaldoActual - saldosPagoServicio.SaldoPorCobrar;
            saldosPagoServicio.EventosDisponibles = estadoDeCuentaRepository.GetEventosDisponibles(PayCenterId);
            saldosPagoServicio.EventosActuales = estadoDeCuentaRepository.GetEventosActuales(PayCenterId);
            return saldosPagoServicio;
        }

        /// <summary>
        /// Obtiene los Saldo Actual de los movimientos Aplicados de un PayCenter para pago de servicios
        /// </summary>
        /// <param name="PayCenterId">Id del PayCenter</param>
        /// <returns>Objeto SaldosPagoServicio con los saldos según los movimientos registrados del PayCenter</returns>
        internal decimal GetSaldoActual(int PayCenterId)
        {
            Succeed = true;
            var movimientos = estadoDeCuentaRepository.GetMovimientos(enumTipoCuenta.Pago_de_Servicios.GetHashCode(), PayCenterId);
            var abonosAplicados = movimientos.Where(x => x.IsAbono && x.Status == (short)enumEstatusMovimiento.Aplicado).Sum(x => x.Monto);
            var cargosAplicados = movimientos.Where(x => !x.IsAbono && x.Status == (short)enumEstatusMovimiento.Aplicado).Sum(x => x.Monto);

            return abonosAplicados - cargosAplicados;
        }

        #endregion

        #region Movimientos

        internal List<Movimiento> CrearMovimientosPagoServicios(int PayCenterId, decimal Monto, string PayCenterName, out bool UsaEvento)
        {
            Succeed = true;
            //Evaluar si va a usar Evento
            var movimientos = new List<Movimiento>();
            var saldos = GetSaldosPagoServicio(PayCenterId);
            var saldoActual = saldos.SaldoActual;
            //Obtener comisión a cobrar
            Decimal? comision = 0;
            //Valido primero parámetros del paycenter
            ParametrosRepository parametrosRepository = new ParametrosRepository();
            var parametrosPayCenter = parametrosRepository.GetParametrosPayCenter(PayCenterId);
            //Determina si va a usar evento
            UsaEvento = saldos.EventosDisponibles > 0;
            if (!UsaEvento)
            {
                var parametrosGlobales = parametrosRepository.GetParametrosGlobales();
                comision = parametrosPayCenter != null && parametrosPayCenter.ComisionPayCenter != null ? parametrosPayCenter.ComisionPayCenter : null;
                if (comision == null)
                {
                    comision = parametrosGlobales != null && parametrosGlobales.ComisionPayCenter != null ? parametrosPayCenter.ComisionPayCenter : null;
                }
                if (comision == null)
                {
                    AddValidationMessage(enumMessageType.BRException, "Está intentando reportar un pago de servicio pero no tiene configurada una comisión de cobro. Favor de reportarlo con un asesor.");
                    Succeed = false;
                    return movimientos;
                }
            }
            //Checar saldo
            Decimal montoTotal = Monto + (Decimal)comision;
            if (saldos.SaldoDisponible < montoTotal)
            {
                //Si no tiene saldo, checar financiamiento configurado
                var maximoAFinanciar = parametrosPayCenter != null && parametrosPayCenter.MaximoAFinanciar != null ? parametrosPayCenter.MaximoAFinanciar : 0;
                decimal saldoFinal = saldos.SaldoDisponible - montoTotal;
                if (maximoAFinanciar >= saldoFinal)
                {
                    AddValidationMessage(enumMessageType.BRException, "No cuenta con saldo suficiente para reportar este pago y tampoco es posible otorgar un financiamiento.");
                    Succeed = false;
                    return movimientos;
                }
            }

            //Obtengo la cuenta de Pago de servicio del PayCenter
            var cuentaId = new PaycenterBR().GetOrCreateCuentaPayCenter(PayCenterId, enumTipoCuenta.Pago_de_Servicios, PROVEEDOR_EVOLUCIONAMOVIL);
            //Generar movimientos correspondientes:
            //Verifico si se pasó el context en el constructor, si no, lo obtengo del repositorio e inicializo la variable que hace que guarde o nó al final del método
            bool autoSave = _context == null;
            if (_context == null)
            {
                _context = estadoDeCuentaRepository.context;
            }
            //Movimiento del pago
            var movimientoPago = CrearMovimiento(PayCenterId, enumTipoMovimiento.Cargo, 0, cuentaId, Monto, enumMotivo.Pago, PayCenterName);
            movimientos.Add(movimientoPago);

            //Inicializo repositorio de movimientos de la empresa
            MovimientosEmpresaRepository movimientosEmpresaRepository = new MovimientosEmpresaRepository(_context);

            //Movimiento de cargo (en caso de que aplique)
            Movimiento movimientoComision = null;
            if (comision > 0)
            {
                movimientoComision = CrearMovimiento(PayCenterId, enumTipoMovimiento.Cargo, 0, cuentaId, Monto, enumMotivo.Comision, PayCenterName);
                movimientos.Add(movimientoComision);
                //Movimiento de comision a favor de la empresa

                var movimientoEmpresaPago = new MovimientoEmpresa
                {
                    Clave = DateTime.Now.ToString("yyyyMMdd") + "0" + ((Int16)enumMotivo.Financiamiento).ToString() + new Random().Next(0, 99999).ToString(),
                    IsAbono = true,
                    Monto = (short)comision,
                    Motivo = (short)enumMotivo.Comision,
                    Movimiento = movimientoComision,
                    SaldoActual = saldoActual,
                    Status = (short)enumEstatusMovimiento.Procesando,
                    UserName = PayCenterName,
                    FechaCreacion = DateTime.UtcNow
                };
                movimientosEmpresaRepository.Add(movimientoEmpresaPago);
            }

            //Movimiento de financiamiento de la empresa
            var financiamientoPago = Monto - saldos.SaldoDisponible;
            var financiamientoComision = comision > 0 ? montoTotal - saldos.SaldoDisponible + financiamientoPago : 0;
            if (financiamientoPago > 0)
            {
                var movimientoEmpresaPago = new MovimientoEmpresa
                {
                    Clave = DateTime.Now.ToString("yyyyMMdd") + "0" + ((Int16)enumMotivo.Financiamiento).ToString() + new Random().Next(0, 99999).ToString(),
                    IsAbono = false,
                    Monto = financiamientoPago,
                    Motivo = (short)enumMotivo.Financiamiento,
                    Movimiento = movimientoPago,
                    SaldoActual = saldoActual,
                    Status = (short)enumEstatusMovimiento.Procesando,
                    UserName = PayCenterName,
                    FechaCreacion = DateTime.UtcNow
                };
                movimientosEmpresaRepository.Add(movimientoEmpresaPago);
            }
            if (financiamientoComision > 0)
            {
                var movimientoEmpresaComision = new MovimientoEmpresa
                {
                    Clave = DateTime.Now.ToString("yyyyMMdd") + "0" + ((Int16)enumMotivo.Financiamiento).ToString() + new Random().Next(0, 99999).ToString(),
                    IsAbono = false,
                    Monto = financiamientoComision,
                    Motivo = (short)enumMotivo.Financiamiento,
                    Movimiento = movimientoComision,
                    SaldoActual = saldoActual,
                    Status = (short)enumEstatusMovimiento.Procesando,
                    UserName = PayCenterName
                };
                movimientosEmpresaRepository.Add(movimientoEmpresaComision);
            }
            if (_context == null)
            {
                Succeed = estadoDeCuentaRepository.Save();
            }

            //Devolver Lista de movimientos
            return movimientos;
        }

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
        /// <remarks>BR01.02. Este método sólo genera movimientos en estatus Procesando</remarks>
        /// <returns></returns>
        internal Movimiento CrearMovimiento(int PayCenterId, enumTipoMovimiento TipoMovimiento, int Id, int CuentaId, decimal Monto, enumMotivo Motivo, string PayCenterName)
        {
            return CrearMovimiento(PayCenterId, TipoMovimiento, Id, CuentaId, Monto, Motivo, PayCenterName, null);
        }

        /// <summary>
        /// Genera el movimiento y genera estatus y clave según condificiones de los BR
        /// </summary>
        /// <param name="PayCenterId"></param>
        /// <param name="TipoMovimiento"></param>
        /// <param name="Id">Identificador de la entidad que está generando el Movimiento (Deposito, Compra, ATraspaso, etc</param>
        /// <param name="CuentaId"></param>
        /// <param name="Monto"></param>
        /// <param name="Motivo"></param>
        /// <param name="PayCenterName"></param>
        /// <param name="Estatus">Si el estatus viene null, se guarda con estatus Procesando</param>
        /// <returns></returns>
        internal Movimiento CrearMovimiento(int PayCenterId, enumTipoMovimiento TipoMovimiento, int Id, int CuentaId, decimal Monto, enumMotivo Motivo, string PayCenterName, enumEstatusMovimiento? Estatus)
        {
            Movimiento movimiento = new Movimiento();

            movimiento.Clave = DateTime.Now.ToString("yyyyMMdd") + "0" + ((Int16)Motivo).ToString() + new Random().Next(0, 99999).ToString();
            movimiento.CuentaId = CuentaId;
            movimiento.FechaCreacion = DateTime.Now;
            movimiento.FechaActualizacion = DateTime.Now;
            movimiento.IsAbono = TipoMovimiento == enumTipoMovimiento.Abono;
            movimiento.Monto = Monto;
            movimiento.Motivo = (Int16)Motivo;
            movimiento.PayCenterId = PayCenterId;
            movimiento.Status = Estatus==null?(Int16)enumEstatusMovimiento.Procesando: (Int16)Estatus;
            movimiento.UserName = PayCenterName;
            movimiento.Id = Id;

            //BR01.04.b: La creación de un registro de movimiento, deberá venir acompañada de un registro de historial de estatus
            var currentUser = HttpContext.Current.User != null ? HttpContext.Current.User.Identity.Name : string.Empty;
            Int16 nuevoEstatusNumber = movimiento.Status;
            var movimientos_Estatus = new Movimientos_Estatus { PayCenterId = PayCenterId, CuentaId = CuentaId, UserName = currentUser, Status = nuevoEstatusNumber, FechaCreacion = DateTime.Now };
            movimiento.Movimientos_Estatus.Add(movimientos_Estatus);
            movimiento.UserName = currentUser;
            movimiento.SaldoActual = GetSaldoActual(PayCenterId);
            if (Estatus.HasValue && Estatus == enumEstatusMovimiento.Aplicado)
            {
                movimiento.SaldoActual += Monto;
            }

            estadoDeCuentaRepository.Add(movimiento);
            if (_context == null)
            {
                estadoDeCuentaRepository.Save();
            }
            return movimiento;
        }

        /// <summary>
        /// Actualiza el Id usado como referencia a la tabla de la acción que generó el movimiento
        /// </summary>
        /// <param name="MovimientoId">Identificador del movimiento que se va a actualizar</param>
        /// <param name="Id">Id de la tabla que hace referencia a la entidad que generó el movimiento. Deposito, Compra, PagoServicio, Comisió, etc.</param>
        /// <remarks>Se creó para actualizar el Id ya que primero se tiene que guardar el registro para obtener el ID</remarks>
        /// <returns>El movimiento actualizado</returns>
        internal Movimiento ActualizaReferenciaIdMovimiento(int MovimientoId, int Id)
        {
            Succeed = true;
            if (Id <= 0)
            {
                Succeed = false;
                AddValidationMessage(enumMessageType.BRException, "El Id de referencia del movimiento debe ser mayor a 0.");
                return null;
            }
            Movimiento movimiento = estadoDeCuentaRepository.LoadById(MovimientoId);
            if (movimiento == null)
            {
                Succeed = false;
                AddValidationMessage(enumMessageType.BRException, "No se pudo encontrar el movimiento con  MovimientoId = " + MovimientoId.ToString());
                return null;
            }
            movimiento.Id = Id;

            if (_context == null)
            {
                estadoDeCuentaRepository.Save();
            }
            return movimiento;
        }

        /// <summary>
        /// Permite actualizar el estatus, Id de referencia y los comentarios correspondientes al cambio de estatus. Crea registro histórico de cambio de estatus y valida las reglas de negocio
        /// </summary>
        /// <param name="MovimientoId">Identificador del movimiento que se va a actualizar</param>
        /// <param name="NuevoEstatus">Enum del nuevo estatus</param>
        /// <param name="Comentarios">Comentarios en caso de que así lo requiera el cambio de estatus</param>
        /// <returns>El movimiento actualizado</returns>
        internal Movimiento ActualizarMovimiento(int MovimientoId, enumEstatusMovimiento NuevoEstatus, string Comentarios)
        {
            Succeed = true;
            Movimiento movimiento = estadoDeCuentaRepository.LoadById(MovimientoId);
            if (movimiento == null)
            {
                Succeed = false;
                AddValidationMessage(enumMessageType.BRException, "No existe el movimiento especificado");
                return null;
            }

            //Valida reglas de negocio del cambio de estatus BR01.04
            if (NuevoEstatus.GetHashCode() == movimiento.Status)
            {
                Succeed = false;
                AddValidationMessage(enumMessageType.BRException, "Ya se encuentra en estatus " + NuevoEstatus.ToString());
                return null;
            }

            //BR01.04.d: Únicamente se permite NO guardar un comentario cuando cambia del estatus Procesando a Aplicado.
            if (string.IsNullOrEmpty(Comentarios))
            {
                if (movimiento.Status != Enums.enumEstatusMovimiento.Procesando.GetHashCode() || NuevoEstatus != Enums.enumEstatusMovimiento.Aplicado)
                {
                    Succeed = false;
                    AddValidationMessage(enumMessageType.BRException, "Es necesario proporcionar comentarios del cambio de Estatus");
                    return null;
                }
            }

            //BR01.04.e: Los estatus Aplicado y Cancelado es un estatus terminal. No es posible volver a cambiar de estatus.
            if (movimiento.Status == enumEstatusMovimiento.Cancelado.GetHashCode())
            {
                Succeed = false;
                AddValidationMessage(enumMessageType.BRException, "No es posible cambiar de estatus un movimiento Cancelado.");
                return null;
            }
            //Identifica si debe sumar o restar del saldo
            var factorSaldo = movimiento.IsAbono ? 1 : -1;
            switch (NuevoEstatus)
            {
                case enumEstatusMovimiento.Procesando:
                    //BR01.04.g: En un movimiento no es posible regresar al estatus Processando
                    Succeed = false;
                    AddValidationMessage(enumMessageType.BRException, "En un movimiento no es posible regresar al estatus Processando");
                    return null;
                case enumEstatusMovimiento.Aplicado:
                    //BR01.04.i: Un movimiento puede ser Aplicado únicamente si el usuario es de tipo Staff o Administrator.
                    if (!HttpContext.Current.User.IsInRole(enumRoles.Staff.ToString()) && !HttpContext.Current.User.IsInRole(enumRoles.Administrator.ToString()))
                    {
                        Succeed = false;
                        AddValidationMessage(enumMessageType.BRException, "El usuario no tiene permisos de realizar esta acción.");
                        return null;
                    }
                    //Actualiza el saldo. Lo multiplico por el valor de IsAbono, porque si NO es abono, es un cargo, y se tiene que restar del saldo, y bisaversa 
                    movimiento.SaldoActual = (movimiento.Monto * factorSaldo) + GetSaldoActual(movimiento.PayCenterId);
                    break;
                case enumEstatusMovimiento.Rechazado:
                    //BR01.04.j: Un movimiento puede ser Rechazado únicamente si el usuario es de tipo Staff o Administrator.
                    if (!HttpContext.Current.User.IsInRole(enumRoles.Staff.ToString()) && !HttpContext.Current.User.IsInRole(enumRoles.Administrator.ToString()))
                    {
                        Succeed = false;
                        AddValidationMessage(enumMessageType.BRException, "El usuario no tiene permisos de realizar esta acción.");
                        return null;
                    }
                    if (movimiento.Status == enumEstatusMovimiento.Aplicado.GetHashCode())
                    {
                        //Actualiza el saldo. Lo multiplico por el valor de IsAbono, porque si NO es abono, es un cargo, y se tiene que restar del saldo, y bisaversa 
                        movimiento.SaldoActual = GetSaldoActual(movimiento.PayCenterId) - (movimiento.Monto * factorSaldo);
                    }
                    break;
                case enumEstatusMovimiento.Cancelado:
                    //BR01.04.h: Un movimiento puede ser cancelado únicamente si el usuario es de tipo PayCenter.
                    if (!HttpContext.Current.User.IsInRole(enumRoles.PayCenter.ToString()))
                    {
                        Succeed = false;
                        AddValidationMessage(enumMessageType.BRException, "El usuario no tiene permisos de realizar esta acción.");
                        return null;
                    }
                    //BR01.04.f: Es posible Cancelar una Solicitud de Pago únicamente si se encuentra en estatus Procesando y no ha transcurrido el tiempo en minutos del parámetro global del sistema.
                    if (movimiento.Status != enumEstatusMovimiento.Procesando.GetHashCode())
                    {
                        Succeed = false;
                        AddValidationMessage(enumMessageType.BRException, "No es posible Cancelar el movimiento si no se encuentra en estatus procesando.");
                        return null;
                    }
                    else
                    {
                        ParametrosRepository parametrosRepository = new ParametrosRepository();
                        var parametrosGlobales = parametrosRepository.GetParametrosGlobales();
                        if (parametrosGlobales == null)
                        {
                            Succeed = false;
                            AddValidationMessage(enumMessageType.Notification, "No existen parámetros globales configurados");
                            return null;
                        }
                        var dif = DateTime.Now - movimiento.FechaCreacion;
                        //Excede los minutos de prórroga?
                        if (dif.TotalMinutes > parametrosGlobales.MinutosProrrogaCancelacion)
                        {
                            Succeed = false;
                            AddValidationMessage(enumMessageType.BRException, "No es posible Cancelar el movimiento debido a que ha excedido el tiempo máximo permitido (" + parametrosGlobales.MinutosProrrogaCancelacion.ToString() + " minutos).");
                            return null;
                        }
                    }
                    if (movimiento.Status == enumEstatusMovimiento.Aplicado.GetHashCode())
                    {
                        //Actualiza el saldo. Lo multiplico por el valor de IsAbono, porque si NO es abono, es un cargo, y se tiene que restar del saldo, y bisaversa 
                        movimiento.SaldoActual = GetSaldoActual(movimiento.PayCenterId) - (movimiento.Monto * factorSaldo);
                    }
                    break;
            }

            //BR01.04.a: Crear nuevo registro histórico de cambio de estatus
            var currentUser = HttpContext.Current.User != null ? HttpContext.Current.User.Identity.Name : string.Empty;
            Int16 nuevoEstatusNumber = (Int16)NuevoEstatus.GetHashCode();
            movimiento.Status = nuevoEstatusNumber;
            movimiento.FechaActualizacion = DateTime.Now;
            var movimientos_Estatus = new Movimientos_Estatus
            {
                PayCenterId = movimiento.PayCenterId,
                CuentaId = movimiento.CuentaId,
                UserName = currentUser,
                Status = nuevoEstatusNumber,
                FechaCreacion = DateTime.Now,
                Comentarios = Comentarios
            };
            movimiento.Movimientos_Estatus.Add(movimientos_Estatus);
         
            if (_context == null)
            {
                estadoDeCuentaRepository.Save();
            }
            return movimiento;
        }

        internal Movimiento RealizarTraspaso(int CuentaOrigenId, int CuentaDestinoId, decimal Monto)
        {
            throw new NotImplementedException();
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

        public class SaldosPagoServicio
        {
            /// <summary>
            /// El Saldo Actual es el resultante de la suma de todos lo movimientos de tipo Abono y la resta de los movimientos de tipo Cargo, ambos con estatus Aplicado
            /// </summary>
            public decimal SaldoActual { get; set; }
            /// <summary>
            /// El Saldo Por Abonar es el resultante de la suma de todos los movimientos de tipo Cargo con estatus Procesando.
            /// </summary>
            public decimal SaldoPorAbonar { get; set; }
            /// <summary>
            /// El Saldo Por Cobrar es el resultante de la suma de todos los movimientos de tipo Cargo con estatus Procesando.
            /// </summary>
            public decimal SaldoPorCobrar { get; set; }
            /// <summary>
            /// El Saldo Pendiente es el resultante de la resta del Saldo Por Abonar menos el Saldo Por Cobrar.
            /// </summary>
            public decimal SaldoPendiente { get; set; }
            /// <summary>
            /// El Saldo Disponible es el resultante de la resta del Saldo Actual menos el Saldo Por Cobrar.
            /// </summary>
            public decimal SaldoDisponible { get; set; }
            /// <summary>
            /// Número de eventos de pago de servicio disponibles
            /// </summary>
            public decimal EventosDisponibles { get; set; }
            /// <summary>
            /// Número de eventos de pago de servicio Aplicados
            /// </summary>
            public decimal EventosActuales { get; set; }
        }

    }
}
