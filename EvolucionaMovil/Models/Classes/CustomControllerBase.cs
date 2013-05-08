using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Models.Enums;
using cabinet.patterns.clases;
using System.Web.Mvc;
using cabinet.patterns.interfaces;

namespace EvolucionaMovil.Models.Classes
{
    public class CustomControllerBase:Controller, ICrossValidation
    {
        //internal const string ADMINISTRATOR_STRING = "Administrator";
        //internal const string PAYCENTER_STRING = "PayCenter";
        //internal const string STAFF_STRING = "Staff";
        //internal const string PROSPECTO_STRING = "Prospecto";

        private bool _succeed=true;
        private List<CrossValidationMessage> _validationMessages;

        public bool AddValidationMessage(int ValidationCode)
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
        public bool AddValidationMessage(cabinet.patterns.enums.enumMessageType MessageType, string Message)
        {
            if (ValidationMessages == null)
            {
                ValidationMessages = new List<CrossValidationMessage>();
            }
            ValidationMessages.Add(new CrossValidationMessage { MessageType = MessageType, Message = Message });
            return true;
        }

        public bool Succeed
        {
            get
            {
                return _succeed;
            }
            set
            {
                _succeed = value;
            }
        }

        public List<CrossValidationMessage> ValidationMessages
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