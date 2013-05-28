﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Repositories
{
    public class ProveedoresRepository : RepositoryBase<Proveedor, EvolucionaMovilBDEntities>
    {

        public IEnumerable<Proveedor> GetProveedoresByTipo(enumTipoCuenta TipoCuenta)
        {
            return context.Proveedors.Where(x => x.TipoCuenta == (short)TipoCuenta.GetHashCode());
        }
    }
}