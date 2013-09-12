using System;
using System.Collections.Generic;
using EvolucionaMovil.Models.Enums;
namespace EvolucionaMovil.Models
{
    public class ProveedorVM
    {
        public IEnumerable<BancoVM> Bancos { get; set; }
        public string Descripcion { get; set; }
        public string Nombre { get; set; }
        public int ProveedorId { get; set; }
        public short TipoCuenta { get; set; }
        public string TipoCuentaString
        {
            get
            {
                return ((enumTipoCuenta)TipoCuenta).ToString().Replace("_"," ");
            }
            set
            {
                TipoCuenta = (short)(Enum.Parse(typeof(enumTipoCuenta),value)).GetHashCode();
            }
        }
        public DateTime FechaCreacion { get; set; }
        public string FechaCreacionString { get { return FechaCreacion.ToString("dd/MMMM/yyy") + " " + FechaCreacion.ToShortTimeString(); } }
    }
}
