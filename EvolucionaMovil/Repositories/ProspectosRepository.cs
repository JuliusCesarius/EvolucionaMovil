using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Models;
using cabinet.patterns.clases;

namespace EvolucionaMovil.Repositories
{
    public class ProspectosRepository : RepositoryBase<Prospecto,EvolucionaMovilBDEntities>
    {
        public bool ExisteEmail(string Email)
        {
            return context.Prospectos.Any(p => p.Email == Email);
        }
    }
}