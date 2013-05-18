using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;

namespace EvolucionaMovil.Repositories
{
    public class ParametrosRepository : RepositoryBase<Parametro , EvolucionaMovilBDEntities>
    {
        public Parametro GetParametrosGlobales()
        {
            return base.ListAll().FirstOrDefault();
        }
        public ParametrosPayCenter GetParametrosPayCenter(int PayCenterId)
        {            
            return base.context.ParametrosPayCenters.FirstOrDefault();
        }
    }
}