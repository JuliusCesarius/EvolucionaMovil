using System;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class CuentaVM
    {
        public int CuentaId { get; set; }
        public IEnumerable<Movimiento> Movimientos { get; set; }
        public PayCenter PayCenter { get; set; }
        public int PayCenterId { get; set; }
        public short TipoCuenta { get; set; }
    }
}
