using System;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class BancoVM
    {
        public int BancoId { get; set; }
        public IEnumerable<CuentaBancariaVM> CuentasBancarias { get; set; }
        public string Nombre { get; set; }
    }
}
