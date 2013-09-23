using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Configuration;
using cabinet.patterns.interfaces;
using cabinet.patterns.clases;
using cabinet.patterns.enums;

namespace EvolucionaMovil.Models.Helpers
{
    public static class EmailHelper
    {
        public static Boolean Enviar(string mensaje, string subject, string toMail, string from = "")
        {
            Succeed = true;
            try
            {
                string smtpServer = ConfigurationManager.AppSettings.Get("SmtpServer");
                string smtpUser = ConfigurationManager.AppSettings.Get("SmtpUser");
                string smtpPassword = ConfigurationManager.AppSettings.Get("SmtpPassword");
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
                {
                    Succeed = false;
                    AddValidationMessage(enumMessageType.UnhandledException, "No fue posible enviar el correo de confirmación");
                    return false;
                }
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(smtpServer);
                SmtpServer.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);
                if (string.IsNullOrEmpty(from))
                {
                    from = smtpUser;
                }

                mail.From = new MailAddress(from, "Evoluciona Móvil");
                foreach (var m in toMail.Split(','))
                {
                    mail.To.Add(m);
                }
                mail.Subject = subject;
                mail.Body = mensaje;
                mail.IsBodyHtml = true;
                SmtpServer.Send(mail);
            }
            catch (Exception e)
            {
                Succeed = false;
                AddValidationMessage(enumMessageType.UnhandledException, "No fue posible enviar el correo de confirmación");
            }
            return Succeed;
        }

        public static bool AddValidationMessage(int ValidationCode)
        {
            //todo:implementar código para levantar el mensaje de la BD cuando esto se implemente
            if (ValidationMessages == null)
            {
                ValidationMessages = new List<CrossValidationMessage>();
            }
            //todo: messagetype y message vienen de la BD
            ValidationMessages.Add(new CrossValidationMessage { ValidationCode = ValidationCode });
            return true;
        }

        public static  bool AddValidationMessage(cabinet.patterns.enums.enumMessageType MessageType, string Message)
        {
            if (ValidationMessages == null)
            {
                ValidationMessages = new List<CrossValidationMessage>();
            }
            ValidationMessages.Add(new CrossValidationMessage { MessageType = MessageType, Message = Message });
            return true;
        }

        public static bool Succeed { get; set; }
        public static  List<CrossValidationMessage> _validationMessages { get; set; }

        public static  List<CrossValidationMessage> ValidationMessages
        {
            get
            {
                if (_validationMessages == null)
                {
                    _validationMessages = new List<CrossValidationMessage>();
                }
                return _validationMessages;
            }
            set
            {
                _validationMessages = value;
            }
        }
    }
}
