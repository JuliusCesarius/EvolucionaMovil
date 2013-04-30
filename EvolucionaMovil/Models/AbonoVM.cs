using System;
using System.Collections.Generic;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Models
{
    public class AbonoVM
    {
        public int AbonoId { get; set; }
        public int BancoId { get; set; }
        public string Banco { get; set; }
        public int CuentaBancariaId { get; set; }
        public string CuentaBancaria { get; set; }
        public int CuentaId { get; set; }
        public string TipoCuenta { get; set; }
        public string StatusString { get { return ((enumEstatusMovimiento)this.Status).ToString(); } }
        public short Status { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal? Monto { get; set; }
        public string MontoString { get; set; }
        public string PayCenter { get; set; }
        public int PayCenterId { get; set; }
        public string Referencia { get; set; }
        public List<HistorialEstatusVM> HistorialEstatusVM { get; set; }
        public CambioEstatusVM CambioEstatusVM { get; set; }
    }
}
