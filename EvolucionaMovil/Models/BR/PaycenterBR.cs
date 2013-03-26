using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using EvolucionaMovil.Repositories;

namespace EvolucionaMovil.Models.BR
{
    public class PaycenterBR : cabinet.patterns.clases.BusinessRulesBase<PayCenter>
    {
        public static ValidationResult UsuarioUnico(string value)
        {
            PayCentersRepository repository = new PayCentersRepository();
            bool Existe = repository.ExisteUsuario(value);
            if (Existe)
            {
                return new ValidationResult("El usuario ya existe en la base de datos.");
            }
            return ValidationResult.Success;
        }
    }
}