using System;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class ProveedorVM
    {
        public IEnumerable<BancoVM> Bancos { get; set; }
        public string Descripcion { get; set; }
        public string Nombre { get; set; }
        public int ProveedorId { get; set; }
        public short TipoCuenta { get; set; }
    }
}
