using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Models;
using cabinet.patterns.clases;

namespace EvolucionaMovil.Repositories
{
    public class PayCentersRepository : RepositoryBase<PayCenter, EvolucionaMovilBDEntities>
    {
        public PayCentersRepository()
            : base()
        {
        }

        public PayCentersRepository(EvolucionaMovilBDEntities context)
            : base(context)
        {
        }

        //public IEnumerable<Usuario> LoadUsuarios()
        //{
        //    return base.context.Usuarios.ToList();
        //}

        public IEnumerable<Cuenta> LoadTipoCuentas(Int32 PayCenterId)
        {
            return base.context.Cuentas.Where(x => x.PayCenterId == PayCenterId).ToList();
        }

        //public IEnumerable<Prospecto> LoadProspectos()
        //{
        //    return base.context.Prospectos.ToList();
        //}

        public PayCenter LoadByProspectoId(Int32 prospectoId)
        {
            return base.context.PayCenters.FirstOrDefault(x => x.ProspectoId == prospectoId);
        }

        public PayCenter LoadByIdName(string stringSearch)
        {
            Int32 IdBuscar = 0;
            int.TryParse(stringSearch, out IdBuscar);
            return base.context.PayCenters.FirstOrDefault(x => x.PayCenterId == IdBuscar || x.UserName == stringSearch);
        }

        public bool ExisteUsuario(string UserName)
        {
            return context.PayCenters.Any(p => p.UserName == UserName);
        }

        public IEnumerable<PayCenter> GetPayCenterBySearchString(string searchString)
        {
            var payCenters = context.PayCenters.Where(p => (p.UserName.Contains(searchString) || p.Nombre.Contains(searchString) )&& p.Baja == false);
            return payCenters;
        }

        public string GetPayCenterNameById(int PayCenterId)
        {
            var payCenter = context.PayCenters.Where(p => p.PayCenterId == PayCenterId).FirstOrDefault();
            if (payCenter != null)
            {
                return payCenter.UserName;
            }
            else
            {
                return null;
            }
        }

        public int GetPayCenterByUserName(string UserName)
        {
            var payCenter = context.PayCenters.Where(p => p.UserName == UserName && p.Baja == false).FirstOrDefault();
            if (payCenter != null)
            {
                return payCenter.PayCenterId;
            }
            else
            {
                return 0;
            }
        }
    }
}