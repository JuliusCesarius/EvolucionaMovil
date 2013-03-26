using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace EvolucionaMovil.Models
{
    public class PayCenterVM
    {
        //public IEnumerable<Abono> Abonos { get; set; }
        public string Celular { get; set; }
        public string Comprobante { get; set; }
        [Display(Name = "Código Postal")]
        public string CP { get; set; }
        //public IEnumerable<Cuenta> Cuentas { get; set; }
        public string Domicilio { get; set; }
        [RegularExpression("^[_a-z0-9-]+(\\.[_a-z0-9-]+)*@[a-z0-9-]+(\\.[a-z0-9-]+)*(\\.[a-z]{2,3})$", ErrorMessage = "El Email no tiene un formato correcto.")]
        public string Email { get; set; }
        [RegularExpression("^[_a-z0-9-]+(\\.[_a-z0-9-]+)*@[a-z0-9-]+(\\.[a-z0-9-]+)*(\\.[a-z]{2,3})$", ErrorMessage = "El Email2 no tiene un formato correcto.")]
        public string Email2 { get; set; }
        public DateTime FechaCreacion { get; set; }
        [Display(Name = "Fecha Ingreso")]
        [DataType(DataType.Date)]
        public DateTime? FechaIngreso { get; set; }
        public string IFE { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        [Display(Name = "Empresa o Negocio")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "Requerido.")]
        [Display(Name = "Usuario")]
        [CustomValidation(typeof(EvolucionaMovil.Models.BR.PaycenterBR), "UsuarioUnico")]
        public string NombreCorto { get; set; }
        public int PayCenterId { get; set; }
        //public Prospecto Prospecto { get; set; }
        public int ProspectoId { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        [Display(Name = "Nombre del Representante")]
        public string Representante { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }
        //public Usuario Usuario { get; set; }
        public int UsuarioId { get; set; }
        //[Required(ErrorMessage = "Campo requerido.")]
        //[Display(Name = "Empresa o Negocio")]
        //public string Empresa { get; set; }
        public Int32 PayCenterPadreId { get; set; }
         [Display(Name = "PayCenter Recomienda")]
        public string PayCenterPadre { get; set; }
        public bool Activo { get; set; }
        //public string UserName { get; set; }
        [Required(ErrorMessage = "Requerido.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Requerido.")]
        [Display(Name = "Repetir Password")]
        [DataAnnotationsExtensions.EqualTo("Password",ErrorMessage="Los passwords capturados no coinciden.")]
        public string RepeatPassword { get; set; }
        [Display(Name = "Maximo a financiar")]
        public string MaximoAFinanciar { get; set; }
    }
}
