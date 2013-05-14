using System;
namespace EvolucionaMovil.Models
{
    public class DetalleServicioVM
    {
        public string Campo { get; set; }
        public int DetalleServicioId { get; set; }
        public Servicio Servicio { get; set; }
        public int ServicioId { get; set; }
        public string Valor { get; set; }
        public int Tipo { get; set; }
    }
}
