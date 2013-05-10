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
        public string Status { get; set; }
        public string Motivo { get; set; }

        public string Cuenta { get; set; }
        public string PayCenter { get; set; }
        public string MontoString { get; set; }
        public Boolean isAbono { get; set; }
        public List<HistorialEstatusVM> HistorialEstatusVM { get; set; }
    }
}
