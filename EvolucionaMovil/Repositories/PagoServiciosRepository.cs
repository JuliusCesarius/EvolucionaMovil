using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;

namespace EvolucionaMovil.Repositories
{
    public class PagoServiciosRepository : RepositoryBase<Pago, EvolucionaMovilBDEntities>
    {
        public IEnumerable<Pago> GetByPayCenterId(int PayCenterId)
        {
            return context.Pagos.Where(x => x.PayCenterId == PayCenterId);
        }
    }

}