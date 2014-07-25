using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;
using EvolucionaMovil.Models.Enums;
using System.Data.Objects;

namespace EvolucionaMovil.Repositories
{
    public class EstadoDeCuentaRepository : RepositoryBase<Movimiento, EvolucionaMovilBDEntities> 
    {
        public EstadoDeCuentaRepository()
            : base()
        {
        }

        public EstadoDeCuentaRepository(EvolucionaMovilBDEntities context)
            : base(context)
        {
        }
        public IEnumerable<Movimiento> GetMovimientos(int TipoCuentaId = -1, int PayCenterId = -1, int ProveedorId = -1)
        {
            return context.Movimientos.Join(context.CuentasPayCenters, m => m.CuentaId, c => c.CuentaId, (m, c) => new { m, c })
                .Where(mc => (mc.c.TipoCuenta == TipoCuentaId || TipoCuentaId == -1) && (mc.m.PayCenterId == PayCenterId || PayCenterId == -1) && (mc.c.ProveedorId == ProveedorId || ProveedorId == -1))
                .Select(mc=>mc.m)
                .OrderByDescending(m => m.FechaActualizacion) as ObjectQuery<Movimiento>;
        }

        public bool IsAuthorized(int PayCenterId, int MovimientoId)
        {
            return context.Movimientos.Any(x => x.PayCenterId == PayCenterId && x.MovimientoId == MovimientoId);
        }

        public int GetEventosDisponibles(int PayCenterId)
        {
            var eventos = context.CompraEventos.Where(x => x.PayCenterId == PayCenterId);
            if (eventos.Count() > 0)
            {
                var eventosComprados = eventos.Sum(x => x.Eventos);
                //Obtiene los pagos que usaron eventos, tanto aplicados como en proceso, para después restarselo a los eventos comprados
                var eventosUsados = context.Pagos.Where(x => x.PayCenterId == PayCenterId && x.UsoEvento && (x.Status == (short)enumEstatusMovimiento.Procesando || x.Status == (short)enumEstatusMovimiento.Aplicado));
                int numEventos = eventosComprados - eventosUsados.Count();
                return numEventos;
            }
            else
            {
                return 0;
            }
        }

        public int GetEventosActuales(int PayCenterId)
        {
            var eventos = context.CompraEventos.Where(x => x.PayCenterId == PayCenterId);
            if (eventos.Count() > 0)
            {
                var eventosComprados = eventos.Sum(x => x.Eventos);
                //Obtiene los pagos que usaron eventos y realmente se aplicaron, para después restarselo a los eventos comprados
                var eventosUsados = context.Pagos.Where(x => x.PayCenterId == PayCenterId && x.UsoEvento && x.Status == (short)enumEstatusMovimiento.Aplicado);
                int numEventos = eventosComprados - eventosUsados.Count();
                return numEventos;
            }
            else
            {
                return 0;
            }
           
        }

        /// <summary>
        /// Obtiene el registro del último registro generado por cambio de estatus
        /// </summary>
        /// <param name="Motivo">Motivo por el cual se realizó el movimiento</param>
        /// <param name="ReferenciaId">Identificador de la tabla (entidad) que generó el movimiento AbonoId, PagoId, etc</param>
        /// <returns>Último objeto Movimiento_Estatus generado para el movimiento encontrado</returns>
        /// <remarks>Si se encuentra más de un movimiento con esta combinación de parámetros marcará error porque sólo debe existir máximo 1</remarks>
        public Movimientos_Estatus GetUltimoCambioEstatus(enumMotivo Motivo, int ReferenciaId)
        {
            var motivo = Motivo.GetHashCode();
            var movimiento = context.Movimientos.Where(x => x.Motivo == motivo && x.Id == ReferenciaId).FirstOrDefault();
            if(movimiento==null){
                return null;
            }
            return movimiento.Movimientos_Estatus.LastOrDefault();
        }
        public Decimal GetSaldoActual(int CuentaId)
        {
            var estatusAplicado = enumEstatusMovimiento.Aplicado.GetHashCode();
            var movimientos = context.Movimientos.Where(m => (m.CuentaId == CuentaId && m.Status == estatusAplicado));
            
            var cargos = movimientos.Where(m => !m.IsAbono).Sum(x=>(Decimal?)x.Monto);
            var abonos = movimientos.Where(m => m.IsAbono).Sum(x => (Decimal?)x.Monto);
            cargos = cargos != null ? cargos : 0;
            abonos = abonos != null ? abonos : 0;
            return (decimal)abonos - (decimal)cargos;
        }
        public void AddAbono(Abono abono)
        {
            context.Abonos.AddObject(abono);
        }
        public IEnumerable<Movimiento> GetMovimientosByPadreId(int PadreId)
        {
            return context.Movimientos.Where(m => m.PadreId == PadreId && m.Baja == false).OrderByDescending(m => m.FechaActualizacion);
        }
        public IEnumerable<Movimiento> GetMovimientosByPayCenterId(int PayCenterId)
        {
            //ToDo: Checar si hay que validar algún estatus
            return context.Movimientos.Where(m => m.PayCenterId == PayCenterId && m.Baja == false).OrderByDescending(m => m.FechaActualizacion);
        }
        public IEnumerable<SP_MovimientosSel_Result> GetMovimientosList(int TipoCuentaId = -1, int PayCenterId = -1, int ProveedorId = -1)
        {
            return context.SP_MovimientosSel(TipoCuentaId, PayCenterId, ProveedorId).ToList();
        }
    }
}