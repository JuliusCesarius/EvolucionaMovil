using System;
namespace EvolucionaMovil.Models
{
    public class CuentaBancariaVM
    {
        public Banco Banco { get; set; }
        public int BancoId { get; set; }
        public string ClabeInterbancaria { get; set; }
        public int CuentaId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Nombre { get; set; }
        public string NumeroCuenta { get; set; }
        public string NumeroDeTarjeta { get; set; }
        public string RFC { get; set; }
        public string Titular { get; set; }
    }
}
