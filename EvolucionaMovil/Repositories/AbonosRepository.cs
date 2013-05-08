using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;

namespace EvolucionaMovil.Repositories
{
    public class AbonoRepository : RepositoryBase<Abono, EvolucionaMovilBDEntities> 
    {
        public AbonoRepository()
            : base()
        {
        }

        public AbonoRepository(EvolucionaMovilBDEntities context)
            : base(context)
        {
        }
        public IEnumerable<Abono> GetByPayCenterId(int PayCenterId)
        {
            return context.Abonos.Where(x => x.PayCenterId == PayCenterId);
        }
    }
}