using System;
using System.ComponentModel.DataAnnotations;
namespace EvolucionaMovil.Models
{
    public class ProspectoVM
    {
        [RegularExpression("^[0-9]{10}", ErrorMessage = "El celular debe contener la lada sin guiones ni paréntesis. P.e. 9999999999")]
        public string Celular { get; set; }
        public string Comentario { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        [RegularExpression("^[_a-z0-9-]+(\\.[_a-z0-9-]+)*@[a-z0-9-]+(\\.[a-z0-9-]+)*(\\.[a-z]{2,3})$", ErrorMessage = "El Email no tiene un formato correcto.")]
        [CustomValidation(typeof(EvolucionaMovil.Models.BR.ProspectoBR), "EmailUnico")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        [Display(Name = "Empresa o Negocio")]
        public string Empresa { get; set; }
        public DateTime FechaCreacion { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        public string Nombre { get; set; }
        public int ProspectoId { get; set; }
        [Display(Name = "Teléfono")]
        [RegularExpression("^[0-9]{10}", ErrorMessage = "El teléfono debe contener la lada sin guiones ni paréntesis. P.e. 9999999999")]
        public string Telefono { get; set; }
        public Guid GUID { get; set; }
    }
}
