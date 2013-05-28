using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using cabinet.processPolicies.MVC.Models.Helpers;
using EvolucionaMovil.Controllers;
using System.Data.SqlClient;
using EvolucionaMovil.Models.Classes;

namespace EvolucionaMovil
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");
            //routes.MapRoute(
            //    "Guid",
            //    "{controller}/{action}/{guid}",
            //    new { controller = "Home", action = "Index" }
            //);
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            if (exception.Message == "File does not exist.")
            {
                //Todo:Quitar esta validación porque puede suceder en otros casos importantes
                return;
            }
            try
            {
                ErrorHandler.LogError(exception, null);
                if (!HttpContext.Current.IsDebuggingEnabled)
                {
                    var routeData = new RouteData();
                    routeData.Values["controller"] = "Error";
                    routeData.Values["action"] = "GenericError";
                    routeData.Values["exception"] = exception;

                    Response.Clear();
                    Server.ClearError();

                    var httpException = exception as HttpException;
                    if (httpException != null && Response.StatusCode == 404)
                    {
                        Response.StatusCode = httpException.GetHttpCode();
                        switch (Response.StatusCode)
                        {
                            //case 403:
                            //    routeData.Values["action"] = "Http403";
                            //    break;
                            case 404:
                                routeData.Values["action"] = "NotFound";
                                break;
                        }
                    }

                    IController errorsController = new ErrorController();
                    var rc = new RequestContext(new HttpContextWrapper(Context), routeData);
                    errorsController.Execute(rc);
                    HttpContext.Current.Response.End();
                }
            }
            catch (Exception ex)
            {
                //Aquí de plano se cicla, mejor llamo a una HTML
                Response.Redirect("../Error.html");
            }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //Inicializo la configuracion del AutoMapper
            AutoMapperBootStrapperHelper.Bootstrap();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}