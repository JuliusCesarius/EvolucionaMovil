using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Models.Enums;
using cabinet.patterns.clases;
using System.Web.Mvc;
using cabinet.patterns.interfaces;
using EvolucionaMovil.Models.Interfaces;
using EvolucionaMovil.Repositories;

namespace EvolucionaMovil.Models.Classes
{
    public class CustomControllerBase:Controller, ICrossValidation, IEvolucionaMovil
    {
        private const string HTTPMETHODPOST = "POST";
        private const string HTTPMETHODGET = "GET";
        private const string PAYCENTERIDSTRING = "PayCenterId";

        private bool _succeed=true;
        private int _payCenterId = 0;
        private PayCentersRepository payCentersRepository;
        private List<CrossValidationMessage> _validationMessages;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (payCentersRepository == null)
            {
                payCentersRepository = new PayCentersRepository();
            }
            if (HttpContext.User.IsInRole(enumRoles.PayCenter.ToString()))
            {
                _payCenterId = payCentersRepository.GetPayCenterByUserName(HttpContext.User.Identity.Name);
            }
            else
            {
                if (filterContext.HttpContext.Request.HttpMethod == HTTPMETHODPOST && filterContext.HttpContext.Request.Form.AllKeys.Contains(PAYCENTERIDSTRING) && !string.IsNullOrEmpty(filterContext.HttpContext.Request.Form.GetValues(PAYCENTERIDSTRING)[0]))
                {
                    _payCenterId = Convert.ToInt32(filterContext.HttpContext.Request.Form.GetValues(PAYCENTERIDSTRING)[0]);
                }
                else if (filterContext.HttpContext.Request.HttpMethod == HTTPMETHODGET && filterContext.HttpContext.Request.QueryString.AllKeys.Contains(PAYCENTERIDSTRING) && !string.IsNullOrEmpty(filterContext.HttpContext.Request.QueryString.GetValues(PAYCENTERIDSTRING)[0]))
                {
                    _payCenterId = Convert.ToInt32(filterContext.HttpContext.Request.QueryString.GetValues(PAYCENTERIDSTRING)[0]);
                }
            }
            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (ValidationMessages.Count > 0)
            {
                ViewBag.MessageType = ValidationMessages.First().MessageType.ToString();
                ViewBag.ValidationMessages = ValidationMessages;
            }
            base.OnActionExecuted(filterContext);
        }

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

        public int PayCenterId
        {
            get
            {
                return _payCenterId;
            }
            set
            {
                _payCenterId = value;
            }
        }
    }
}