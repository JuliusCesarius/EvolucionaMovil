using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cabinet.patterns.clases;
using cabinet.patterns.interfaces;

namespace cabinet.patterns.clases
{
    public abstract class CrossValidation: ICrossValidation
    {
        public bool _succeed = true;
        public bool Succeed { get; set; }
        public List<CrossValidationMessage> ValidationMessages { get; set; }

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
        public bool AddValidationMessage(enums.enumMessageType MessageType, string Message)
        {
            if (ValidationMessages == null)
            {
                ValidationMessages = new List<CrossValidationMessage>();
            }
            ValidationMessages.Add(new CrossValidationMessage { MessageType = MessageType, Message = Message });
            return true;
        }

    }
}
