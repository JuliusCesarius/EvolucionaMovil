using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Models;
using cabinet.patterns.clases;
using System.Collections;

namespace EvolucionaMovil.Repositories
{
    public class ProspectosRepository : RepositoryBase<Prospecto,EvolucionaMovilBDEntities>
    {
        public bool ExisteEmail(string Email)
        {
            return context.Prospectos.Any(p => p.Email == Email);
        }
        public int GetProspectoIdByGUID(Guid GUID)
        {
            return context.Prospectos.Where(p => p.ID.Equals(GUID)).Select(p=>p.ProspectoId).FirstOrDefault();
        }
        public IQueryable<ProspectoPaycenterVM> GetProspectosPayCenter()
        {

            var prospectos = from prospecto in context.Prospectos
                             join payCenter in context.PayCenters on prospecto.ProspectoId equals payCenter.ProspectoId into payCentersJoin
                             from payCenterJoin in payCentersJoin.DefaultIfEmpty()
                             select new ProspectoPaycenterVM {
                                 ProspectoId = prospecto.ProspectoId,
                                 Nombre = prospecto.Nombre,
                                 Empresa = prospecto.Empresa,
                                 Telefono = prospecto.Telefono,
                                 Celular = prospecto.Celular,
                                 Email = prospecto.Email,
                                 Comentario = prospecto.Comentario,
                                 FechaCreacion = prospecto.FechaCreacion,
                                 IsPayCenter = payCenterJoin!= null,
                                 PayCenterName = payCenterJoin != null ? payCenterJoin.UserName : string.Empty
                             };

            return prospectos;
        }

    }
}
