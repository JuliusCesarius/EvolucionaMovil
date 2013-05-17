using System;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class PaqueteVM
    {
        public short Creditos { get; set; }
        public int PaqueteId { get; set; }
        public decimal Precio { get; set; }
        public string PrecioString { get { return Precio.ToString("C"); } }
        public string PrecioPorEvento { get { return (Creditos > 0 ? (Precio / Creditos) : 0).ToString("C"); } }
        public bool Selected { get; set; }
        public short NumeroPaquete { get; set; }
    }
}
