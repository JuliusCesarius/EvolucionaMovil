using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using EvolucionaMovil.Attributes;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Models
{
    public class ReporteDepositoVM
    {
        public int BancoId { get; set; }
        public string Banco { get; set; }
        [Required]
        public int CuentaBancariaId { get; set; }
        public string CuentaBancaria { get; set; }
        public string Cuenta { get; set; }
        [Required]
        public string Referencia { get; set; }
        public string RutaFichaDeposito { get; set; }
        [Required]
        public decimal? Monto { get; set; }
        public string MontoString { get; set; }
        [Required]
        public DateTime? FechaPago { get; set; }
        public enumTipoCuenta TipoCuenta { get; set; }
        public string PayCenterName { get; set; }
        [Required]
        public int PayCenterId { get; set; }
        [Required]
        public int ProveedorId { get; set; }
    }

}