using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using EvolucionaMovil.Attributes;

namespace EvolucionaMovil.Models
{
    public class ReporteDepositoVM
    {
        public int BancoId { get; set; }
        public string Banco { get; set; }
        [Required]
        public int CuentaBancariaId { get; set; }
        public string CuentaBancaria { get; set; }
        [Required]
        public int CuentaId { get; set; }
        public string Cuenta { get; set; }
        [Required]
        public string Referencia { get; set; }
        [Required]
        [Mask("$999.99")]
        public decimal? Monto { get; set; }
        [Required]
        public DateTime? FechaPago { get; set; }
        public ICollection<CuentaDepositoVM> CuentasDeposito { get; set; }
        public string TipoCuenta { get; set; }
    }

    public class CuentaDepositoVM
    {
        public string Nombre { get; set; }
        public int CuentaId { get; set; }
        public decimal Monto { get; set; }
       
    }
}