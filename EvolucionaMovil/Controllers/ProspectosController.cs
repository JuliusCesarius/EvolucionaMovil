using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EvolucionaMovil.Models;
using EvolucionaMovil.Repositories;
using AutoMapper;
using System.Net.Mail;
using EvolucionaMovil.Attributes;
using EvolucionaMovil.Models.Enums;
using EvolucionaMovil.Models.Classes;
using EvolucionaMovil.Models.Helpers;
using System.Text;
using System.Collections;

namespace EvolucionaMovil.Controllers
{
    public class ProspectosController : CustomControllerBase
    {
        private ProspectosRepository repository = new ProspectosRepository();

        //
        // GET: /Prospectos/
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.Administrator })]
        public ViewResult Index()
        {
            return View(repository.ListAll().ToListOfDestination<ProspectoVM>());
        }

        public ActionResult Preafiliacion()
        {
            return View();
        }

        [HttpPost]
        [Captcha]
        public ActionResult Preafiliacion(ProspectoVM prospectoVM)
        {
            bool exito = false;
            if (ModelState.IsValid)
            {
                Prospecto prospecto = new Prospecto();
                Mapper.Map(prospectoVM, prospecto);
                repository.Add(prospecto);
                exito = repository.Save();
                prospectoVM.ProspectoId = prospecto.ProspectoId;
                prospectoVM.GUID = (Guid)prospecto.ID;
            }
            if (exito)
            {
                return RedirectToAction("Confirmacion/" + prospectoVM.GUID.ToString());
            }
            else
            {
                //ViewBag.Mensajes = Mensajes;
                return View(prospectoVM);
            }
        }

        public ViewResult Confirmacion(Guid id)
        {
            Guid guid = Guid.Parse(id.ToString());
            Prospecto prospecto = repository.LoadByGUID(guid);
            var bodyMessage = GetMessageBody(prospecto.Email, Url.Action("Registrar", "PayCenters", new { id = prospecto.ID }, "http"));
            Succeed = EmailHelper.Enviar(bodyMessage, "Evoluciona Móvil - Confirmación de Preafiliación", prospecto.Email);
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        private string GetMessageBody(string EmailTo, string UrlRegister)
        {
            StringBuilder bodyString = new StringBuilder();
            bodyString.Append("<html>" +
            "<body>" +
                "<div style=\"font-family: Open Sans, lucida grande, Segoe UI, arial, verdana, lucida sans unicode, tahoma, sans-serif; width:585px; margin-left:auto; margin-right:auto; \">" +
                    "<h1 style=\"font-size:18px; margin:5px 0 5px 0; padding:0;\">Confirmación de preafiliación</h1>" +
                    "<p style=\"margin:0; padding:0; font-size:15px;\">¡Gracias por preafiliarte! Accede a la siguiente liga para finalizar el alta como PayCenter.</p>" +
                    "<a style=\"margin:5px 0 5px 0; padding:0; font-size: 15px;\" href=\"" + UrlRegister + "\">" + UrlRegister + "</a>" +
                "</div>" +
                "<div style=\"text-align:right; width:585px; margin-left:auto; margin-right:auto;\">" +
                    "<img src=\"" + string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority) + "/Content/themes/base/images/logo_evoluciona.jpg" + "\" alt=\"\" width=\"180px\" />" +
                "</div>" +
            "</body>" +
            "</html>");
            return bodyString.ToString();
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.Administrator })]
        public string GetProspectos(ServiceParameterVM parameters)
        {
            var ProspectosResult = getProspectos(parameters);
            return Newtonsoft.Json.JsonConvert.SerializeObject(ProspectosResult);
        }

        private SimpleGridResult<ProspectoPaycenterVM> getProspectos(ServiceParameterVM Parameters = null)
        {
            var prospectos = repository.GetProspectosPayCenter();
            
            //Aplicar filtros
           if (Parameters != null && (Parameters.fechaInicio != null || Parameters.fechaFin != null || Parameters.searchString != null))
            {
                 prospectos = prospectos.Where(x => (Parameters.fechaInicio == null || Parameters.fechaInicio <= x.FechaCreacion)
                                            && (Parameters.fechaFin == null || Parameters.fechaFin >= x.FechaCreacion)
                                            && ((string.IsNullOrEmpty(Parameters.searchString) || x.Nombre.Contains(Parameters.searchString))
                                             || (string.IsNullOrEmpty(Parameters.searchString) || x.Email.Contains(Parameters.searchString)))
                                            );
            
            }

            prospectos = prospectos.OrderByDescending(x => x.ProspectoId);


            SimpleGridResult<ProspectoPaycenterVM> simpleGridResult = new SimpleGridResult<ProspectoPaycenterVM>();
            IEnumerable<ProspectoPaycenterVM> ProspectosPaged = null;
            if (Parameters != null)
            {
                simpleGridResult.CurrentPage = Parameters.pageNumber;
                simpleGridResult.PageSize = Parameters.pageSize;
                if (Parameters.pageSize > 0)
                {
                    var pageNumber = Parameters.pageNumber >= 0 ? Parameters.pageNumber : 0;
                    simpleGridResult.CurrentPage = pageNumber;
                    simpleGridResult.TotalRows = prospectos.Count();
                    ProspectosPaged = prospectos.Skip(pageNumber * Parameters.pageSize).Take(Parameters.pageSize);
                }
            }
            simpleGridResult.Result = ProspectosPaged.OrderByDescending(x => x.FechaCreacion);
       
            return simpleGridResult;
        }


    
        [HttpPost, ActionName("Delete")]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator,enumRoles.Staff })]
        public string Delete(int id)
        {
            Prospecto Prospecto = repository.LoadById(id);
            repository.Delete(Prospecto);
            repository.Save();
            return "El Prospecto se ha eliminado con éxito.";
        }

    }
}