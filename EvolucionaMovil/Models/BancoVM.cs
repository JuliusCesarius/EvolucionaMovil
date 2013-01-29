using System;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class BancoVM
    {
        public int BancoId { get; set; }
        public IEnumerable<CuentaBancaria> CuentasBancarias { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string Nombre { get; set; }
    }
}
