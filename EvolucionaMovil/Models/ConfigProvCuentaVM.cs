using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolucionaMovil.Models
{
    public class ConfigProvCuentaVM
    {
        public IEnumerable<ConfigProvVM> Proveedores { get; set; }
        public int CuentaBancariaId { get; set; }
    }
    public class ConfigProvCuentaResponseVM
    {
        public IEnumerable<int> Proveedores { get; set; }
        public int CuentaBancariaId { get; set; }
    }
    public class ConfigProvVM
    {
        public int ProveedorId { get; set; }
        public bool Selected { get; set; }
    }
}
