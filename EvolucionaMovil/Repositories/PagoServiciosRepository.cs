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

        public bool IsAuthorized(int PayCenterId, int PagoId)
        {
            return context.Pagos.Any(x => x.PayCenterId == PayCenterId && x.PagoId == PagoId);
        }

        public IEnumerable<SP_PagosSel_Result> GetPagosList(int PayCenterId)
        {
            return context.SP_PagosSel(PayCenterId).ToList();
        }
    }

}