using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Models.Classes;
using EvolucionaMovil.Repositories;
using EvolucionaMovil.Attributes;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Controllers
{
    public class ProveedoresController : CustomControllerBase
    {
        private ProveedoresRepository repository = new ProveedoresRepository();

        //[CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff })]
        public string FindProveedores(string term)
        {
            ProveedoresRepository payCentersRepository = new ProveedoresRepository();
            var Proveedores = payCentersRepository.GetProveedorBySearchString (term).Select(x => new { label = x.Nombre, value = x.ProveedorId });
            return Newtonsoft.Json.JsonConvert.SerializeObject(Proveedores);
        }


    }
}