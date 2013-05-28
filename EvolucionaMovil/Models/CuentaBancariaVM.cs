using System;
namespace EvolucionaMovil.Models
{
    public class CuentaBancariaVM
    {
        public Banco Banco { get; set; }
        public int BancoId { get; set; }
        public string ClabeInterbancaria { get; set; }
        public int CuentaId { get; set; }
        public string Nombre { get; set; }
        public bool Comprobante { get; set; }
        private string _caption;
        public string Caption
        {
            get
            {
                _caption = !string.IsNullOrEmpty(NumeroCuenta) ? NumeroCuenta + " - " : string.Empty;
                _caption += !string.IsNullOrEmpty(ClabeInterbancaria) ? ClabeInterbancaria + " - " : string.Empty;
                _caption += !string.IsNullOrEmpty(NumeroDeTarjeta) ? NumeroDeTarjeta + " - " : string.Empty;
                _caption += Titular;
                return _caption;
            }
            set
            {
                _caption = value;
            }
        }
        public string Detalles { get; set; }
        public string NumeroCuenta { get; set; }
        public string NumeroDeTarjeta { get; set; }
        public string RFC { get; set; }
        public string Titular { get; set; }
    }
}
