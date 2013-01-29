using System;
namespace EvolucionaMovil.Models
{
    public class CompraEventoVM
    {
        public int CompraEventoId { get; set; }
        public short Consumidos { get; set; }
        public int CuentaId { get; set; }
        public short Eventos { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal Monto { get; set; }
        public Paquete Paquete { get; set; }
        public int PaqueteId { get; set; }
        public int PayCenterId { get; set; }
    }
}
