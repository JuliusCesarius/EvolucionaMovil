using System;
namespace EvolucionaMovil.Models
{
    public class AbonoVM
    {
        public int AbonoId { get; set; }
        public int BancoId { get; set; }
        public int CuentaId { get; set; }
        public int EstatusId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Monto { get; set; }
        public PayCenter PayCenter { get; set; }
        public int PayCenterId { get; set; }
        public string Referencia { get; set; }
    }
}
