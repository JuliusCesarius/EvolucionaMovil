using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
namespace EvolucionaMovil.Models
{
    public class PayCenterVM
    {
        public IEnumerable<Abono> Abonos { get; set; }
        [Required]
        public string Celular { get; set; }
        public string Comprobante { get; set; }
        public string CP { get; set; }
        public IEnumerable<Cuenta> Cuentas { get; set; }
        [Required]
        public string Domicilio { get; set; }
        [Required]
        public string Email { get; set; }
        public string Email2 { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaIngreso { get; set; }
        [Required]
        public string IFE { get; set; }
        [Required]
        public string Nombre { get; set; }
        public int PayCenterId { get; set; }
        public Prospecto Prospecto { get; set; }
        public int ProspectoId { get; set; }
        [Required]
        public string Representante { get; set; }
        [Required]
        public string Telefono { get; set; }
        public MembershipUser Usuario { get; set; }
        public int UsuarioId { get; set; }
    }
}
