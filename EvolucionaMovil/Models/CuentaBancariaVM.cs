using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace EvolucionaMovil.Models
{
    public class CuentaBancariaVM
    {
        const string EMPTY = "";
        public Banco Banco { get; set; }
        [Required(ErrorMessage = "No se ha especificado el banco al que pertenece")]
        public int BancoId { get; set; }
        [DefaultValue(EMPTY)]
        public string ClabeInterbancaria { get; set; }
        public int CuentaId { get; set; }
        [DefaultValue(EMPTY)]
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
        [DefaultValue(EMPTY)]
        public string NumeroCuenta { get; set; }
        [DefaultValue(EMPTY)]
        public string NumeroDeTarjeta { get; set; }
        public string RFC { get; set; }
        [Required(ErrorMessage = "Debe especificar un Titular")]
        public string Titular { get; set; }
        public IEnumerable<int> Proveedores { get; set; } 
    }
}
