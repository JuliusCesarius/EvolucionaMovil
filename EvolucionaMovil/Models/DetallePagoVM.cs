using System;
namespace EvolucionaMovil.Models
{
    public class DetallePagoVM
    {
        public string Campo { get; set; }
        public int DetallePagoId { get; set; }
        public Pago Pago { get; set; }
        public int PagoId { get; set; }
        public string Valor { get; set; }
    }
}
