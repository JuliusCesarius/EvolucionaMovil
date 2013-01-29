﻿using System;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class TicketVM
    {
        public string ClienteEmail { get; set; }
        public string ClienteNombre { get; set; }
        public string ClienteTelefono { get; set; }
        public decimal Comision { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaPago { get; set; }
        public string Folio { get; set; }
        public decimal Importe { get; set; }
        public string Leyenda { get; set; }
        public Pago Pago { get; set; }
        public int PagoId { get; set; }
        public int PayCenterId { get; set; }
        public string ReferenciaBancaria { get; set; }
        public int TicketId { get; set; }
        public string TipoServicio { get; set; }
    }
}
