using System;
namespace EvolucionaMovil.Models
{
    public class UsuarioVM
    {
        public bool Bloqueado { get; set; }
        public string Email { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Password { get; set; }
        public int RolId { get; set; }
        public int UsuarioId { get; set; }
    }
}
