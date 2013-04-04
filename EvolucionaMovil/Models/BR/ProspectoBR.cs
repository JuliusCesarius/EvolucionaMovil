using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using EvolucionaMovil.Repositories;

namespace EvolucionaMovil.Models.BR
{
    public class ProspectoBR : cabinet.patterns.clases.BusinessRulesBase<Prospecto>
    {
        public static ValidationResult EmailUnico(string value)
        {
            ProspectosRepository repository = new ProspectosRepository();
            bool Existe = repository.ExisteEmail(value);
            if (Existe) {
                return new ValidationResult("El Email proporcionado ya existe.");
            }
            return ValidationResult.Success;
        }
    }
}