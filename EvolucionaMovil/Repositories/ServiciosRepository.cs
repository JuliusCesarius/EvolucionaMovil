using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Models;
using cabinet.patterns.clases;

namespace EvolucionaMovil.Repositories
{
    public class ServiciosRepository : RepositoryBase<Servicio, EvolucionaMovilBDEntities>
    {
        public IEnumerable<DetalleServicio> LoadDetallesServicioByServicioID(int ServicioId)
        {
            var detallesServicio = context.DetalleServicios.Where(x => !x.Baja && x.ServicioId == ServicioId);
            return detallesServicio;
        }
    }
}