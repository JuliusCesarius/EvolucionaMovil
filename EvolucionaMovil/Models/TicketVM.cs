using System;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class TicketVM
    {
        public string Folio { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string FechaCreacionString { get { return FechaCreacion.ToString("dd/MMMM/yyy") + " " + FechaCreacion.ToShortTimeString(); }}
        public String FechaVencimientoString { get { return FechaVencimiento.ToString("dd/MMMM/yyy"); } }
        public string ClienteEmail { get; set; }
        public string ClienteNombre { get; set; }
        public string ClienteTelefono { get; set; }
        public decimal Comision { get; set; }
        public decimal Importe { get; set; }
        public string ImporteString { get { return Importe.ToString("C"); } }
        public string Leyenda { get; set; }
        public Pago Pago { get; set; }
        public int PagoId { get; set; }
        public int PayCenterId { get; set; }
        public int TicketId { get; set; }
        public string TipoServicio { get; set; }
        public string Referencia { get; set; }
        public string PayCenterName { get; set; }

        public List<DetallePago> DetallePagos { get; set; }

        public TicketVM()
        {
            DetallePagos = new List<DetallePago>();
        }
    }
}
