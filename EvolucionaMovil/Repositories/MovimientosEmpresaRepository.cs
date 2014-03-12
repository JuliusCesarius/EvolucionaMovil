using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Repositories
{
    public class MovimientosEmpresaRepository : RepositoryBase<MovimientoEmpresa, EvolucionaMovilBDEntities>
    {
        public MovimientosEmpresaRepository()
            : base()
        {
        }

        public MovimientosEmpresaRepository(EvolucionaMovilBDEntities context)
            : base(context)
        {
        }

        internal decimal GetSaldoActual()
        {
            //Le paso el 9 para que solome traiga el estado de cuenta de EvolucionaMovil
            var movimientosEmpresas = context.MovimientoEmpresas;
            var status = (short)enumEstatusMovimiento.Aplicado;
            var abonosAplicados = movimientosEmpresas.Where(x => x.IsAbono && x.Status == status).ToList().Sum(x => x.Monto);
            var cargosAplicados = movimientosEmpresas.Where(x => !x.IsAbono && x.Status == status).ToList().Sum(x => x.Monto);

            return abonosAplicados - cargosAplicados;
        }
    }
}