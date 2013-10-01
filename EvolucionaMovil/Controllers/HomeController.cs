using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EvolucionaMovil.Repositories;
using EvolucionaMovil.Models;
using EvolucionaMovil.Models.Enums;
using EvolucionaMovil.Models.Extensions;
using EvolucionaMovil.Models.Classes;
using EvolucionaMovil.Attributes;

namespace EvolucionaMovil.Controllers
{
    public class HomeController : CustomControllerBase
    {
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator, enumRoles.Staff, enumRoles.PayCenter })]
        public ActionResult Index()
        {
            //string[] lines = System.IO.File.ReadAllLines(@"C:\servicioscampos.txt");
            //ServiciosRepository serviciosRepository = new ServiciosRepository();

            //foreach (string line in lines)
            //{
            //    var campos = line.Split('\t');
            //    var servicioId = Convert.ToInt32(campos.First());
            //    var servicio = serviciosRepository.LoadById(servicioId);
            //    var tipoDato = enumTipoDato.Cadena.GetHashCode();
            //    for (var i = 2; i < campos.Length - 1; i++)
            //    {
            //        var campo = campos[i].Trim();
            //        if (!string.IsNullOrEmpty(campo) && !campo.ContainsInvariant("Nombre del titular") && !campo.ContainsInvariant("Fecha límite de pago") && !campo.ContainsInvariant("Importe a Pagar"))
            //        {
            //            if (campo.ContainsInvariant("fecha"))
            //            {
            //                tipoDato = enumTipoDato.Fecha.GetHashCode();
            //            }
            //            else if (campo.ContainsInvariant("import"))
            //            {
            //                tipoDato = enumTipoDato.Dinero.GetHashCode();
            //            }
            //            servicio.DetalleServicios.Add(new DetalleServicio { Campo = campo, Tipo = (short)tipoDato, FechaCreacion = DateTime.UtcNow.GetCurrentTime(), ServicioId = servicioId });
            //        }
            //    }
            //}
            //serviciosRepository.Save();
            ServiciosRepository serviciosRepository = new ServiciosRepository();
            //Le resto el número de servicios que aparecen en el Home
            ViewBag.ServicesCount = serviciosRepository.GetServicesCount() - 12;
            return View();
            //return RedirectToAction("Logon","Account");
        }

        public ActionResult About()
        {

            return View();
        }
        public ActionResult Aviso_de_Privacidad()
        {
            return View();
        }
        public ActionResult Politicas_de_Uso()
        {
            return View();
        }
    }
}
