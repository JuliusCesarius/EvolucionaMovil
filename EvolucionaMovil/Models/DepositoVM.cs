using System;
using System.Collections.Generic;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Models
{
    public class DepositoVM
    {
        public int AbonoId { get; set; }
        public string Banco { get; set; }
        public string CuentaBancaria { get; set; }
        public string TipoCuenta { get; set; }
        public string StatusString { get { return ((enumEstatusMovimiento)this.Status).ToString(); } }
        public short Status { get; set; }
        public string FechaCreacion { get; set; }
        public string FechaPago { get; set; }
        public string Monto { get; set; }
        public string PayCenter { get; set; }
        public int PayCenterId { get; set; }
        public string Referencia { get; set; }
        public string Comentarios { get; set; }
        public string ProveedorName { get; set; }
    }
}