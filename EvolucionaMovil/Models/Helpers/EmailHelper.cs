using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;

namespace EvolucionaMovil.Models.Helpers
{
    public class EmailHelper : Controller
    {
        public static Boolean Enviar(string mensaje, string subject, string toMail, string from = "")
        {
            Boolean exito = true;
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("mail.evolucionamovil.mx");
                SmtpServer.Credentials = new System.Net.NetworkCredential("postmaster@evolucionamovil.mx", "evoluciona5500");

                if (string.IsNullOrEmpty(from))
                    from = "postmaster@evolucionamovil.mx";

                mail.From = new MailAddress(from, "Evoluciona Movil");
                mail.To.Add(toMail);
                mail.Subject = subject;
                mail.Body = mensaje;
                mail.IsBodyHtml = true;
                SmtpServer.Send(mail);
            }
            catch (Exception e)
            {
                exito = false;
            }
            return exito;
        }

    }
}
