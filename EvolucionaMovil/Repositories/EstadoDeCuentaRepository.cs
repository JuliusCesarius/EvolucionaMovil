using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;
using EvolucionaMovil.Models.Enums;

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
        public IEnumerable<Movimiento> GetMovimientos(int TipoCuentaId = -1, int PayCenterId = -1)
        {
            return context.Movimientos.Join(context.Cuentas, m => m.CuentaId, c => c.CuentaId, (m, c) => new { m, c })
                .Where(mc => (mc.c.TipoCuenta == TipoCuentaId || TipoCuentaId == -1) && (mc.m.PayCenterId == PayCenterId || PayCenterId == -1))
                .Select(mc=>mc.m)
                .OrderByDescending(m => m.FechaCreacion);
        }
        public Decimal GetSaldoActual(int CuentaId)
        {
            var estatusAplicado = enumEstatusMovimiento.Aplicado.GetHashCode();
            var cargos = context.Movimientos.Where(m => (m.CuentaId == CuentaId && !m.IsAbono && m.Status == estatusAplicado)).Sum(x=>x.Monto);
            var abonos = context.Movimientos.Where(m => (m.CuentaId == CuentaId && m.IsAbono && m.Status == estatusAplicado)).Sum(x=>x.Monto);
            return abonos - cargos;
        }
        public void AddAbono(Abono abono)
        {
            context.Abonos.AddObject(abono);
        }
        public IEnumerable<Movimiento> GetMovimientosByPayCenterId(int PayCenterId)
        {
            //ToDo: Checar si hay que validar algún estatus
            return context.Movimientos.Where(m => m.PayCenterId == PayCenterId && m.Baja == false).OrderByDescending(m => m.FechaCreacion);
        }
    }
}