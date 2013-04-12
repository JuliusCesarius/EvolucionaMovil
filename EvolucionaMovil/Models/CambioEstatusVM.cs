using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EvolucionaMovil.Models
{
    public class CambioEstatusVM
    {
        /// <summary>
        /// Estatus al cual va a cambia
        /// </summary>
        public string  Estatus { get; set; }
        /// <summary>
        /// Explicacion o Comentario del user
        /// </summary>
        public string  Comentario { get; set; }
    }
}