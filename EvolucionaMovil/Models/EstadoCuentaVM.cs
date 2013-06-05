using System;
using EvolucionaMovil.Models.Enums;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class EstadoCuentaVM
    {
        public int PayCenterId { get; set; }
        public int CuentaId { get; set; }
        public int MovimientoId { get; set; }

        public int Id { get; set; }        
        public int CuentaOrigenId { get; set; }
        public string Clave { get; set; }
        public string Comentarios { get; set; }
        public string Concepto { get; set; }
        public string Abono { get; set; }
        public string Cargo { get; set; }
        public string Saldo { get; set; }
        public string FechaCreacion { get; set; }
        public int Status { get; set; }
        public string Motivo { get; set; }

        public string Cuenta { get; set; }
        public string PayCenterName { get; set; }
        public string MontoString { get; set; }
        public string StatusString { get { return ((enumEstatusMovimiento)this.Status).ToString(); } }
        public Boolean isAbono { get; set; }
        public List<HistorialEstatusVM> HistorialEstatusVM { get; set; }
        public CambioEstatusVM CambioEstatusVM { get; set; }
    }
}
