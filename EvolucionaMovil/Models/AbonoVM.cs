﻿using System;
using System.Collections.Generic;
using EvolucionaMovil.Models.Enums;
using EvolucionaMovil.Models.Extensions;

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
        public enumTipoCuenta TipoCuenta { get; set; }
        public string StatusString { get { return ((enumEstatusMovimiento)this.Status).ToString(); } }
        public short Status { get; set; }
        public DateTime FechaCreacion { get; set; }
        public String FechaCreacionString { get { return FechaCreacion.ToString(); } }
        public DateTime? FechaPago { get; set; }
        public String FechaPagoString { get { return FechaPago.ToString(); } }
        public decimal? Monto { get; set; }
        public string MontoString { get; set; }
        public string PayCenterName { get; set; }
        public int PayCenterId { get; set; }
        public int ProveedorId { get; set; }
        public string Referencia { get; set; }
        public string RutaFichaDeposito { get; set; }
        public List<HistorialEstatusVM> HistorialEstatusVM { get; set; }
        public CambioEstatusVM CambioEstatusVM { get; set; }
        public string Clave { get; set; }
    }
}
