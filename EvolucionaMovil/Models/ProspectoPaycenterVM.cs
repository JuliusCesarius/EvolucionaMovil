using System;

namespace EvolucionaMovil.Models
{

    public class ProspectoPaycenterVM
    {
        public int ProspectoId { get; set; }
        public string Nombre { get; set; }
        public string Empresa { get; set; }
        public string Telefono { get; set; }
        public string Celular { get; set; }
        public string Email { get; set; }
        public string Comentario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool IsPayCenter { get; set; }
        public string PayCenterName { get; set; }
    }
}