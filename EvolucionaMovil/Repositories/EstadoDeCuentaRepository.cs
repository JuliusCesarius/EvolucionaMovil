using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;

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
        public IEnumerable<Movimiento> GetMovimientos(int TipoCuentaId = -1)
        {
            return context.Movimientos.Where(m => m.CuentaId == TipoCuentaId || TipoCuentaId == -1).OrderByDescending(m => m.FechaCreacion);
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