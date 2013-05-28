using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EvolucionaMovil.Models.Enums;
using EvolucionaMovil.Controllers;

namespace EvolucionaMovil.Attributes
{

    public class CaptchaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //Verifico captcha
            string captchaValue = filterContext.HttpContext.Request.Form.Get("CaptchaValue");
            string invisibleCaptchaValue = filterContext.HttpContext.Request.Form.Get("InvisibleCaptchaValue");

            bool cv = CaptchaController.IsValidCaptchaValue(captchaValue.ToUpper());
            bool icv = invisibleCaptchaValue == "";

            if (!cv || !icv)
            {
                filterContext.Controller.ViewData.ModelState.AddModelError(string.Empty, "Captcha error.");
                // return View(filterContext.Controller.ViewData.Model);
            }
            else
            {
            }
            base.OnActionExecuting(filterContext);
        }
    }
}