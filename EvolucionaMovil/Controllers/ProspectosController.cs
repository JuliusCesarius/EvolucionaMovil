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

namespace EvolucionaMovil.Controllers
{
    public class ProspectosController : Controller
    {
        //private List<string> Mensajes = new List<string>();
        private ProspectosRepository repository = new ProspectosRepository();

        //
        // GET: /Prospectos/
        public ViewResult Index()
        {
            return View(repository.ListAll().ToListOfDestination<ProspectoVM>());
        }

        //
        // GET: /Prospectos/Details/5
        public ViewResult Details(int id)
        {
            Prospecto prospecto = repository.LoadById(id);
            ProspectoVM prospectoVM = new ProspectoVM();
            Mapper.Map(prospecto, prospectoVM);
            return View(prospectoVM);
        }

        //
        // GET: /Prospectos/Preafiliacion
        public ActionResult Preafiliacion()
        {
            return View();
        }

        //
        // POST: /Prospectos/Preafiliacion
        [HttpPost]
        public ActionResult Preafiliacion(ProspectoVM prospectoVM)
        {
            //bool exito = EmailUnico(prospectoVM.Email);
            //if (!exito)
            //{
            //    Mensajes.Add("El Email proporcionado ya existe.");
            //}
            bool exito = false;
            if (ModelState.IsValid) //&& exito)
            {
                Prospecto prospecto = new Prospecto();
                Mapper.Map(prospectoVM, prospecto);
                repository.Add(prospecto);
                exito = repository.Save();
                prospectoVM.ProspectoId = prospecto.ProspectoId;
            }
            if (exito)
            {
                return RedirectToAction("Confirmacion/" + prospectoVM.ProspectoId.ToString());
            }
            else
            {
                //ViewBag.Mensajes = Mensajes;
                return View(prospectoVM);
            }
        }

        public ViewResult Confirmacion(Int32 id)
        {
            Prospecto prospecto = repository.LoadById(id);
            EnviarCorreo(prospecto.Email, Url.Action("Registrar", "PayCenters", new { id = prospecto.ProspectoId }, "http"));
            return View();
        }

        //
        // GET: /Prospectos/Edit/5
        public ActionResult Edit(int id)
        {
            Prospecto prospecto = repository.LoadById(id);
            ProspectoVM prospectoVM = new ProspectoVM();
            Mapper.Map(prospecto, prospectoVM);
            return View(prospectoVM);
        }

        //
        // POST: /Prospectos/Edit/5

        [HttpPost]
        public ActionResult Edit(ProspectoVM prospectoVM)
        {
            if (ModelState.IsValid)
            {
                Prospecto prospecto = repository.LoadById(prospectoVM.ProspectoId);
                Mapper.Map(prospectoVM, prospecto);
                repository.Save();
                return RedirectToAction("Index");
            }
            return View(prospectoVM);
        }

        //
        // GET: /Prospectos/Delete/5
        public ActionResult Delete(int id)
        {
            Prospecto prospecto = repository.LoadById(id);
            ProspectoVM prospectoVM = new ProspectoVM();
            Mapper.Map(prospecto, prospectoVM);
            return View(prospectoVM);
        }

        //
        // POST: /Prospectos/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Prospecto prospecto = repository.LoadById(id);
            repository.Delete(prospecto);
            repository.Save();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        //Validar si existe el email en la base de datos
        //private static bool EmailUnico(string Email) {
        //    return !repository.ExisteEmail(Email);
        //}

        private void EnviarCorreo(string EmailTo, string UrlRegister)
        {
            //ToDo: Agregar los datos reales del servidor de correos, ya sea en el config o aquí por código, verificar si se puede obtener el Url principal de otra manera
            SmtpClient oServidor = new SmtpClient();

            MailMessage oCorreo = new MailMessage();
            oCorreo.To.Add(EmailTo);
            oCorreo.Subject = "Evoluciona Móvil - Confirmación de Preafiliación";
            oCorreo.Body = "<html>" +
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
            "</html>";
            oCorreo.IsBodyHtml = true;

            try
            {
                oServidor.Send(oCorreo);
            }
            catch
            {
                //ToDo: Determinar que se debe hacer si falla el envío.
            }
        }
    }
}