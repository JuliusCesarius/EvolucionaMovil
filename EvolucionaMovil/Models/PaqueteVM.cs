using System;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class PaqueteVM
    {
        public short Creditos { get; set; }
        public int PaqueteId { get; set; }
        public decimal Precio { get; set; }
        public string PrecioString { get; set; }
        public string PrecioPorEvento { get; set; }
        public bool Selected { get; set; }
    }
}
