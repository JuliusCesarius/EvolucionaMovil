using System;
using EvolucionaMovil.Models.Enums;
namespace EvolucionaMovil.Models
{
    public class PagoServicioVM
    {
        public int PayCenterId { get; set; }
        public string PayCenterName { get; set; }
        public int CuentaId { get; set; }
        public int ServicioId { get; set; }

        public int PagoId { get; set; }        
        public string Folio { get; set; }
        public string Comentarios { get; set; }
        public string FechaVencimiento { get; set; }
        public string FechaCreacion { get; set; }
        public string NombreCliente { get; set; }
        public string Monto { get; set; }
        public string Servicio { get; set; }
        public string Status { get; set; }
    }
}
