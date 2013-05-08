using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Attributes
{

    public class CustomAuthorize : AuthorizeAttribute
    {
        //Property to allow array instead of single string.
        private enumRoles[] _authorizedRoles;
        public enumRoles[] AuthorizedRoles
        {
            get { return _authorizedRoles ?? new enumRoles[0]; }
            set { _authorizedRoles = value; }
        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            //If its an unauthorized/timed out ajax request go to top window and redirect to logon.
            if (filterContext.Result is HttpUnauthorizedResult && filterContext.HttpContext.Request.IsAjaxRequest())
                filterContext.Result = new JavaScriptResult() { Script = "top.location = '/Account/LogOn?Expired=1';" };

            //If authorization results in HttpUnauthorizedResult, redirect to error page instead of Logon page.
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary {{ "controller", "Account" },
                                         { "action", "LogOn" },
                                         { "returnUrl",    filterContext.HttpContext.Request.RawUrl } });//send the user to login page with return url;
            }
            else if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectResult("~/Error/Authorization");
            }

        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            //Bypass role check if user is Admin, prevents having to add Admin role across the whole project.
            if (httpContext.User.IsInRole(enumRoles.Administrator.ToString()))
                return true;

            //If no roles are supplied to the attribute just check that the user is logged in.
            if (AuthorizedRoles.Length == 0)
                return true;

            //Check to see if any of the authorized roles fits into any assigned roles only if roles have been supplied.
            foreach (enumRoles r in AuthorizedRoles)
            {
                if (httpContext.User.IsInRole(r.ToString()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}