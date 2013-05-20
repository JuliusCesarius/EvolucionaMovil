using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;

namespace EvolucionaMovil.Repositories
{
    public class TicketRepository : RepositoryBase<Ticket, EvolucionaMovilBDEntities> 
    {
        public TicketRepository()
            : base()
        {
        }
        public Ticket LoadByPagoId(int PagoId)
        {
            return context.Tickets.Where(x => x.PagoId == PagoId).FirstOrDefault();
        }
    }
}