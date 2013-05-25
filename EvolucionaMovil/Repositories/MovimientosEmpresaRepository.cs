using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;

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
    }
}