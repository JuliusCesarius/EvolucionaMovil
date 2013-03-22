using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Models;
using cabinet.patterns.clases;
using System.Web.Security;

namespace EvolucionaMovil.Repositories
{
    public class PayCentersRepository:RepositoryBase<PayCenter,EvolucionaMovilBDEntities>
    {    
        public PayCentersRepository()
            : base()
        {
        }

        public PayCentersRepository(EvolucionaMovilBDEntities context)
            : base(context)
        {
        }
        public IEnumerable<MembershipUser> LoadUsuarios()
        {
            //todo:Obtener usuario. Aunque no recuerdo para qué, pero la tabla se eliminó y ahora lee los del membership
            return new List<MembershipUser>();
        }
        public IEnumerable<Cuenta> LoadTipoCuentas(Int32 PayCenterId)
        {
            return base.context.Cuentas.Where(x => x.PayCenterId == PayCenterId).ToList();
        }

        public IEnumerable<Prospecto> LoadProspectos()
        {
            return base.context.Prospectos.ToList();
        }
    }
}