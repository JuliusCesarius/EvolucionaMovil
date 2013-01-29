using System;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class PagoVM
    {
        public string ClienteNombre { get; set; }
        public IEnumerable<DetallePago> DetallePagos { get; set; }
        public string Estatus { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Importe { get; set; }
        public int PagoId { get; set; }
        public int PayCenterId { get; set; }
        public int ServicioId { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; }
    }
}