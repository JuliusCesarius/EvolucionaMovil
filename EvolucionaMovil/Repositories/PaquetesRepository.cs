using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;
using System.Data;

namespace EvolucionaMovil.Repositories
{
    public class PaquetesRepository : RepositoryBase<Paquete, EvolucionaMovilBDEntities>
    {
        public bool Add(CompraEvento Entity)
        {
            if (Entity.CompraEventoId>0)
            {
                //verifico si existen en la BD                    
                if (context.CompraEventos.Any(x=>x.CompraEventoId==Entity.CompraEventoId))
                {
                    //*****************UPDATE******************//
                    context.CompraEventos.Attach(Entity);
                    context.ObjectStateManager.ChangeObjectState(Entity, EntityState.Modified);
                }
                else
                {
                    //TODO:Analizar que debo hacer en caso de que no exista el Id especificado
                    throw new Exception("No se encuentra el objeto con el Id especificado");
                }
            }
            else
            {
                context.CompraEventos.AddObject(Entity);
            }
            return true;
        }

        public int GetEventosByPayCenter(int PayCenterId)
        {
            if(!context.CompraEventos.Any(x => x.PayCenterId == PayCenterId)){
                return 0;
            }
            return context.CompraEventos.Where(x => x.PayCenterId == PayCenterId).Sum(x =>  x.Eventos - x.Consumidos);
        }
    }
}