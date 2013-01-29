using System;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class ServicioVM
    {
        public IEnumerable<DetalleServicio> DetalleServicios { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Imagen { get; set; }
        public string Nombre { get; set; }
        public int ServicioId { get; set; }
    }
}
