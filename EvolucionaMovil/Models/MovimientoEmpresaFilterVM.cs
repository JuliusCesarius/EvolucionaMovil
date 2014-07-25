using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EvolucionaMovil.Models
{
    public class MovimientoEmpresaFilterVM
    {
        public string PayCenterName { get; set; }
        public int? Status { get; set; }
        public int? Motivo { get; set; }
        public string MovimientoOrigen { get; set; }
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}