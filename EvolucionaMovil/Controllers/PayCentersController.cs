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
using MvcMembership;
using System.Web.Security;
using EvolucionaMovil.Models.Classes;
using EvolucionaMovil.Attributes;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Controllers
{
    public class PayCentersController : CustomControllerBase
    {
        private PayCentersRepository repository = new PayCentersRepository();

        //
        // GET: /PayCenters/
        public ViewResult Index()
        {
            //var paycenters = repository.ListAll();
            //return View(paycenters.ToListOfDestination<PayCenterVM>());
            return View();
        }

        [HttpPost]
        public string GetPayCenters(ServiceParameterVM parameters)
        {
            var estadoCuentaResult = getPaycenters(parameters);
            return Newtonsoft.Json.JsonConvert.SerializeObject(estadoCuentaResult);
        }

        [HttpPost]
        public string GetPayCenterRecomienda(StringParameterVM parameter)
        {
            PayCenter paycenter = repository.LoadByIdName(parameter.searchString);

            PayCenterVM paycenterVM = null;
            if (paycenter != null)
            {
                paycenterVM = new PayCenterVM();
                Mapper.Map(paycenter, paycenterVM);
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(paycenterVM);
        }

        [CustomAuthorize(AuthorizedRoles= new []{enumRoles.PayCenter})]
        public ViewResult Profile()
        {
            var payCenter = repository.LoadByIdName(HttpContext.User.Identity.Name);
            var payCenterVM = FillPayCenterVM(payCenter.PayCenterId);
            return View("Details", payCenterVM);
        }

        //
        // GET: /PayCenters/Details/5
        public ViewResult Details(int id)
        {
            var PayCenterVM = FillPayCenterVM(id);
            return View(PayCenterVM);
        }

        //
        // GET: /PayCenters/Create
        //public ActionResult Create()
        //{
        //    //ViewBag.ProspectoId = new SelectList(repository.LoadProspectos().ToListOfDestination<ProspectoVM>(), "ProspectoId", "Email");
        //    //ViewBag.UsuarioId = new SelectList(repository.LoadUsuarios().ToListOfDestination<UsuarioVM>(), "UsuarioId", "Email");
        //    return View();
        //}

        // GET: /PayCenters/Create ProspectoId
        public ActionResult Registrar(Int32? id)
        {
            PayCenterVM paycenterVM = new PayCenterVM();
            paycenterVM.ProspectoId = 0;
            if (id != null && id > 0)
            {
                //Verificar que no este dado de alta previamente
                PayCenter paycenter = repository.LoadByProspectoId((int)id);

                //en Modificación de Paycenter NO Activo dice que si no se encuentra paycenter debe redireccionar a un view NotFound, pero si es la primera vez que el prospecto se da de alta no aplica, entonces se hizo para cuando el prospecto no exista
                if (paycenter == null)
                {
                    ProspectosRepository repositoryProspecto = new ProspectosRepository();
                    Prospecto prospecto = repositoryProspecto.LoadById((int)id);
                    if (prospecto != null)
                    {
                        paycenterVM.Celular = prospecto.Celular;
                        paycenterVM.Email = prospecto.Email;
                        paycenterVM.Nombre = prospecto.Empresa;
                        //paycenterVM.UserName = prospecto.Nombre;
                        paycenterVM.Telefono = prospecto.Telefono;
                        paycenterVM.ProspectoId = prospecto.ProspectoId;
                    }
                    else
                    {
                        return RedirectToAction("NotFound");
                    }
                }
                else
                {
                    Mapper.Map(paycenter, paycenterVM);
                    //<author>Julio Avila</author>
                    //<comments>Se cambió el nombre de la propiedad PayCenters2 a PayCenterPadre</comments>
                    //<before>
                    //paycenterVM.PayCenterPadre = paycenter.PayCenters2.UserName;
                    //</before>
                    //<after>
                    paycenterVM.PayCenterPadre = paycenter.PayCenterPadre.UserName;
                    //</after>
                    ////Buscar usuario para determinar si está activo
                    //AspNetMembershipProviderWrapper membership = new AspNetMembershipProviderWrapper();
                    //MembershipUser usuario = membership.Get(paycenterVM.Nombre);
                    //if (usuario != null)
                    //{
                    //    paycenterVM.Activo = usuario.IsApproved;
                    //    paycenterVM.UserName = usuario.UserName;

                    //    if (paycenterVM.Activo)
                    //    {
                    //        ViewBag.Mensajes = "El PayCenter ya se encuentra aprobado y activo, no es posible ser modificado por el Prospecto. Por favor, ingresa al sistema con tu usuario y contraseña.";
                    //    }
                    //}
                    //else
                    //{
                    //    paycenterVM.Activo = false;
                    //    paycenterVM.UserName = string.Empty;
                    //}

                    if (paycenterVM.Activo)
                    {
                        ViewBag.Mensajes = "El PayCenter ya se encuentra aprobado y activo, no es posible ser modificado por el Prospecto. Por favor, ingresa al sistema con tu usuario y contraseña.";
                    }
                }
            }

            return View(paycenterVM);
        }

        //
        // POST: /PayCenters/Create
        [HttpPost]
        public ActionResult Registrar(PayCenterVM paycenterVM)
        {
            //Esto es para que no me marque requerido al valida cuanto es actualización
            //ToDo: verificar si hay alguna manera de hacer obligatorio al crear y al actualizar no
            if (paycenterVM.PayCenterId > 0)
            {
                ModelState.Remove("Password");
                ModelState.Remove("RepeatPassword");
                ModelState.Remove("UserName");
            }

            if (ModelState.IsValid)
            {
                //<author>Moisés Cauich</author>
                //<comments>Ya se traen los valores correspondientes del view, solo cuando sean null se pone cadena vacía.</comments>
                //<before>
                //paycenterVM.Comprobante = string.Empty;
                //paycenterVM.IFE = string.Empty;
                //</before>
                //<after>
                if (paycenterVM.IFE == null)
                {
                    paycenterVM.IFE = string.Empty;
                }
                if (paycenterVM.Comprobante == null)
                {
                    paycenterVM.Comprobante = string.Empty;
                }
                paycenterVM.ThumbnailIFE = paycenterVM.IFE.Replace("UploadImages", "UploadImages/Thumbnails");
                paycenterVM.ThumbnailComprobante = paycenterVM.Comprobante.Replace("UploadImages", "UploadImages/Thumbnails");
                //</after>           

                //ToDo:Determinar como se van a manejar estos valores
                paycenterVM.UsuarioId = 1;
                if (paycenterVM.ProspectoId == 0)
                {
                    paycenterVM.ProspectoId = 1;
                }
                if (paycenterVM.PayCenterPadreId == 0)
                {
                    paycenterVM.PayCenterPadreId = 1;
                }

                //llenar los campos faltantes si estan nulos
                ValidaCapturaParcial(ref paycenterVM);

                #region "crear prospecto comentado"
                ////Crear prospecto o cargar el prospecto relacionado
                //if (paycenterVM.ProspectoId > 0)
                //{
                //      //ProspectosRepository repositoryProspecto = new ProspectosRepository();
                //      //Prospecto prospecto = repositoryProspecto.LoadById(paycenterVM.ProspectoId);
                //      //paycenterVM.Prospecto = prospecto;
                //}
                //else
                //{
                //    Prospecto prospecto = new Prospecto() { 
                //        Baja=false,
                //        Celular=paycenterVM.Celular,
                //        Comentario=string.Empty,
                //        Email=paycenterVM.Email,
                //        Empresa=paycenterVM.Empresa,
                //        FechaCreacion=paycenterVM.FechaCreacion,
                //        Nombre=paycenterVM.Nombre,
                //        Telefono=paycenterVM.Telefono,
                //        ProspectoId=0
                //    };
                //    paycenterVM.Prospecto = prospecto;
                //}
                #endregion

                //Validar si existe el usuario si el paycenter está siendo editado y actualizarlo
                bool Exito = true;
                AspNetMembershipProviderWrapper membership = new AspNetMembershipProviderWrapper();
                if (paycenterVM.PayCenterId > 0 && !string.IsNullOrWhiteSpace(paycenterVM.UserName)) //&& !string.IsNullOrWhiteSpace(paycenterVM.Password))
                {
                    try
                    {
                        MembershipUser usuario = membership.Get(paycenterVM.UserName);

                        if (usuario != null)
                        {
                            if (!usuario.IsApproved)
                            {
                                usuario.Email = paycenterVM.Email;
                                membership.Update(usuario);
                                //membership.ChangePassword(usuario, paycenterVM.Password);
                            }
                            else
                            {
                                paycenterVM.Activo = true;
                                Exito = false;
                                ViewBag.Mensajes = "El PayCenter ya se encuentra aprobado y activo, no es posible ser modificado por el Prospecto. Por favor, ingresa al sistema con tu usuario y contraseña.";
                            }
                        }
                        else
                        {
                            Exito = false;
                            ViewBag.MensajeError = "No se encontró el usuario del PayCenter.";
                        }
                    }
                    catch (Exception ex)
                    {
                        Exito = false;
                        ViewBag.MensajeError = "Se ha producido un error al actualizar el usuario del PayCenter. " + ex.Message;
                    }
                }
                else if (!(string.IsNullOrWhiteSpace(paycenterVM.UserName) || string.IsNullOrWhiteSpace(paycenterVM.Password)))
                {
                    try
                    {
                        var user = membership.Create(paycenterVM.UserName, paycenterVM.Password, paycenterVM.Email, null, null, false);
                        AspNetRoleProviderWrapper membershipRole = new AspNetRoleProviderWrapper();
                        membershipRole.AddToRole(user, "paycenter");
                    }
                    catch (Exception ex)
                    {
                        Exito = false;
                        ViewBag.MensajeError = "Se ha producido un error al crear el usuario del PayCenter. " + ex.Message;
                    }
                }

                if (Exito)
                {
                    PayCenter paycenter;
                    if (paycenterVM.PayCenterId > 0)
                    {
                        paycenter = repository.LoadById(paycenterVM.PayCenterId);
                        //Esto es porque el prospecto no debe modificar el dato
                        //ToDo: verificar si se puede excluir de otra manera

                        //<author>Julio Avila</author>
                        //<comments>Se cambió el campo a la tabla parámetros</comments>
                        //<before>
                        //paycenterVM.MaximoAFinanciar = paycenter.MaximoAFinanciar.ToString();
                        //</before>
                        //<after>
                        paycenterVM.MaximoAFinanciar = paycenter.Parametros != null && paycenter.Parametros.MaximoAFinanciar != null ? paycenter.Parametros.MaximoAFinanciar.ToString() : string.Empty;
                        //</after>
                        Mapper.Map(paycenterVM, paycenter);
                    }
                    else
                    {
                        paycenter = new PayCenter();
                        Mapper.Map(paycenterVM, paycenter);
                        repository.Add(paycenter);
                    }
                    repository.Save();

                    //return RedirectToAction("Index");
                    paycenterVM.Activo = true; //Esto es sólo para que se deshabiliten los campos
                    ViewBag.Mensajes = "El PayCenter se ha guardado con éxito. Si deseas modificar o terminar de completar tu información deberás acceder mediante el enlace que recibiste en tu correo o contactar al equipo de Evoluciona Móvil. En breve tu registro como PayCenter quedará activado.";
                }

            }

            return View(paycenterVM);
        }

        // GET: /PayCenters/Edit/5
        public ActionResult Edit(int id)
        {
            PayCenter paycenter = repository.LoadById(id);

            if (paycenter != null)
            {
                PayCenterVM paycenterVM = new PayCenterVM();
                Mapper.Map(paycenter, paycenterVM);
                //<author>Julio Avila</author>
                //<comments>Se cambió el nombre de la propiedad PayCenters2 a PayCenterPadre</comments>
                //<before>
                //paycenterVM.PayCenterPadre = paycenter.PayCenters2.UserName;
                //</before>
                //<after>

                //<author>Moisés Cauich</author>
                //<comments>Faltó esta linea, en el map esta ligando la propiedad PayCenterPadre del VM que es una cadena con la propiedad PayCenterPadre de la entidad que antes era PayCenters2</comments>
                //<before>
                //<after>
                paycenterVM.PayCenterPadre = paycenter.PayCenterPadre.UserName;
                //</after>

                ////Buscar usuario para determinar si está activo
                //AspNetMembershipProviderWrapper membership = new AspNetMembershipProviderWrapper();
                //MembershipUser usuario = membership.Get(paycenterVM.Nombre);
                //if (usuario != null)
                //{
                //    paycenterVM.Activo = usuario.IsApproved;
                //    paycenterVM.UserName = usuario.UserName;
                //}
                //else
                //{
                //    paycenterVM.Activo = false;
                //    paycenterVM.UserName = string.Empty;
                //}

                //ViewBag.ProspectoId = new SelectList(repository.LoadProspectos(), "ProspectoId", "Email", paycenter.ProspectoId);
                //ViewBag.UsuarioId = new SelectList(repository.LoadUsuarios(), "UsuarioId", "Email", paycenter.UsuarioId);

                return View(paycenterVM);
            }
            else
            {
                return RedirectToAction("NotFound");
            }
        }

        //
        // POST: /PayCenters/Edit/5
        [HttpPost]
        public ActionResult Edit(PayCenterVM paycenterVM)
        {
            //Esto es para que no me marque requerido al valida cuanto es actualización
            //ToDo: verificar si hay alguna manera de hacer obligatorio al crear y al actualizar no
            ModelState.Remove("Password");
            ModelState.Remove("RepeatPassword");
            ModelState.Remove("UserName");

            if (ModelState.IsValid)
            {
                //<author>Moisés Cauich</author>
                //<comments>Ya se traen los valores correspondientes del view.</comments>
                //<before>
                //paycenterVM.Comprobante = "";
                //paycenterVM.IFE = "";
                //</before>
                //<after>

                //ToDo:Determinar como se van a manejar estos valores
                paycenterVM.UsuarioId = 1;
                if (paycenterVM.ProspectoId == 0)
                {
                    paycenterVM.ProspectoId = 1;
                }
                if (paycenterVM.PayCenterPadreId == 0)
                {
                    paycenterVM.PayCenterPadreId = 1;
                }

                //llenar los campos faltantes si estan nulos
                ValidaCapturaParcial(ref paycenterVM);

                bool Exito = true;

                //Validar los datos necesarios para activar el usuario
                if (paycenterVM.Activo)
                {
                    Exito = ValidaActivacion(ref paycenterVM);
                }

                //Validar si existe el usuario si el paycenter está siendo editado y actualizarlo
                if (Exito)
                {
                    AspNetMembershipProviderWrapper membership = new AspNetMembershipProviderWrapper();
                    if (!string.IsNullOrWhiteSpace(paycenterVM.UserName))
                    {
                        try
                        {
                            MembershipUser usuario = membership.Get(paycenterVM.UserName);

                            if (usuario != null)
                            {
                                usuario.Email = paycenterVM.Email;
                                usuario.IsApproved = paycenterVM.Activo;
                                membership.Update(usuario);
                            }
                            else
                            {
                                Exito = false;
                                ViewBag.MensajeError = "No se encontró el usuario del PayCenter.";
                            }
                        }
                        catch (Exception ex)
                        {
                            Exito = false;
                            ViewBag.MensajeError = "Se ha producido un error al actualizar el usuario del PayCenter. " + ex.Message;
                        }
                    }
                    else if (paycenterVM.Activo)
                    {
                        Exito = false;
                        ViewBag.MensajeError = "No se ha creado el usuario del PayCenter, no se puede activar. desmarque la casilla de activo y guarde.";
                    }

                    if (Exito)
                    {
                        PayCenter paycenter = repository.LoadById(paycenterVM.PayCenterId);
                        Mapper.Map(paycenterVM, paycenter);
                        repository.Save();

                        ViewBag.GuardadoExito = "El PayCenter se ha actualizado con éxito.";
                    }
                }
                else
                {
                    ViewBag.MensajeError = "Es necesario capturar todos los datos para activar al PayCenter. capture los datos faltantes o desmarque la casilla de activo y guarde.";
                }
                //PayCenter paycenter = repository.LoadById(paycenterVM.PayCenterId);
                //Mapper.Map(paycenterVM, paycenter);
                //Mapper.Map(paycenterVM.Cuentas, paycenter.Cuentas);
                //Mapper.Map(paycenterVM.Abonos, paycenter.Abonos);
                //repository.Save();
                //return RedirectToAction("Index");
            }

            //ViewBag.ProspectoId = new SelectList(repository.LoadProspectos(), "ProspectoId", "Email", paycenterVM.ProspectoId);
            //ViewBag.UsuarioId = new SelectList(repository.LoadUsuarios(), "UsuarioId", "Email", paycenterVM.UsuarioId);
            return View(paycenterVM);
        }

        public ViewResult NotFound()
        {
            return View();
        }

        ////
        //// GET: /PayCenters/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    PayCenter paycenter = repository.LoadById(id);
        //    PayCenterVM paycenterVM = new PayCenterVM();
        //    Mapper.Map(paycenter, paycenterVM);
        //    return View(paycenterVM);
        //}

        //
        // POST: /PayCenters/Delete/5
        [HttpPost, ActionName("Delete")]
        public string DeleteConfirmed(int id)
        {
            PayCenter paycenter = repository.LoadById(id);
            repository.Delete(paycenter);
            repository.Save();
            return "El PayCenter se ha eliminado con éxito.";
        }

        [HttpPost]
        public string Activate(int id, string accion)
        {
            PayCenter paycenter = repository.LoadById(id);

            //Si la la acción solicitada es activar, validar que el paycenter tenga todos los campos necesarios
            if (accion == "Activar")
            {
                if (!ValidaActivacion(paycenter))
                {
                    return "El PayCenter no tiene capturado todos los datos necesarios para su activación.";
                }
                paycenter.Activo = true;
            }
            else
            {
                paycenter.Activo = false;
            }

            //Actualizar el usuario de membership
            AspNetMembershipProviderWrapper membership = new AspNetMembershipProviderWrapper();
            if (!string.IsNullOrWhiteSpace(paycenter.UserName))
            {
                try
                {
                    MembershipUser usuario = membership.Get(paycenter.UserName);

                    if (usuario != null)
                    {
                        usuario.IsApproved = paycenter.Activo;
                        membership.Update(usuario);
                    }
                    else
                    {
                        return "No se encontró el usuario del PayCenter, no se puede ejecutar la acción.";
                    }
                }
                catch
                {
                    return "Se ha producido un error al actualizar el usuario del PayCenter, no se pudo ejecutar la acción.";
                }
            }
            else
            {
                return "No se ha creado el usuario del PayCenter, no se puede ejecutar la acción.";
            }

            repository.Save();

            if (paycenter.Activo)
            {
                return "El PayCenter se ha activado con éxito.";
            }
            else
            {
                return "El PayCenter se ha desactivado con éxito.";
            }
        }

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        private void ValidaCapturaParcial(ref PayCenterVM paycenterVM)
        {
            if (paycenterVM.Celular == null)
            {
                paycenterVM.Celular = string.Empty;
            }
            if (paycenterVM.Email == null)
            {
                paycenterVM.Email = string.Empty;
            }
            if (paycenterVM.Email2 == null)
            {
                paycenterVM.Email2 = string.Empty;
            }
            if (paycenterVM.Domicilio == null)
            {
                paycenterVM.Domicilio = string.Empty;
            }
            if (paycenterVM.CP == null)
            {
                paycenterVM.CP = string.Empty;
            }
            if (paycenterVM.FechaIngreso == null)
            {
                paycenterVM.FechaIngreso = new DateTime(1900, 1, 1);
            }
        }

        private SimpleGridResult<PayCenterVM> getPaycenters(ServiceParameterVM Parameters = null)
        {
            var paycenters = repository.ListAll();

            //Aplicar filtros
            if (Parameters != null && (Parameters.fechaInicio != null || Parameters.fechaFin != null || Parameters.searchString != null))
            {
                //Separar el searchString para obtener el filtro de estatus y el de nombre
                string filtroEstatus = null, filtroNombre = null;
                if (Parameters.searchString != null)
                {
                    string[] filtros = Parameters.searchString.Split(',');
                    filtroEstatus = filtros[0];
                    if (filtros.Length > 1) filtroNombre = filtros[1];
                }

                //<author>Moisés Cauich</author>
                //<comments>Se cambió el where porque ya no se usan el searchString directamente, se usan las variables string obenenidas en el paso anterior</comments>
                //<before>
                //paycenters = paycenters.Where(x => (Parameters.fechaInicio == null || Parameters.fechaInicio <= x.FechaIngreso)
                //                            && (Parameters.fechaFin == null || Parameters.fechaFin >= x.FechaIngreso)
                //                            && (Parameters.searchString == null || Parameters.searchString == "Todos"
                //                                || (Parameters.searchString == "Activo" && x.Activo == true)
                //                                || (Parameters.searchString == "Inactivo" && x.Activo == false)));
                //</before>
                //<after>
                paycenters = paycenters.Where(x => (Parameters.fechaInicio == null || Parameters.fechaInicio <= x.FechaIngreso)
                                            && (Parameters.fechaFin == null || Parameters.fechaFin >= x.FechaIngreso)
                                            && (string.IsNullOrEmpty(filtroEstatus) || filtroEstatus == "Todos"
                                                || (filtroEstatus == "Activo" && x.Activo == true)
                                                || (filtroEstatus == "Inactivo" && x.Activo == false))
                                            && (string.IsNullOrEmpty(filtroNombre) || x.UserName.Contains(filtroNombre)));
                //</after>
            }

            SimpleGridResult<PayCenterVM> simpleGridResult = new SimpleGridResult<PayCenterVM>();
            var PaycentersVM = paycenters.ToListOfDestination<PayCenterVM>();

            if (Parameters != null)
            {
                simpleGridResult.CurrentPage = Parameters.pageNumber;
                simpleGridResult.PageSize = Parameters.pageSize;
                if (Parameters.pageSize > 0)
                {
                    var pageNumber = Parameters.pageNumber >= 0 ? Parameters.pageNumber : 0;
                    simpleGridResult.CurrentPage = pageNumber;
                    simpleGridResult.TotalRows = PaycentersVM.Count();
                    PaycentersVM = PaycentersVM.Skip(pageNumber * Parameters.pageSize).Take(Parameters.pageSize);
                }
            }
            simpleGridResult.Result = PaycentersVM;

            return simpleGridResult;
        }

        private PayCenterVM FillPayCenterVM(int Id)
        {
            PayCenter paycenter = repository.LoadById(Id);

            if (paycenter == null)
            {
                return null;
            }
            PayCenterVM paycenterVM = new PayCenterVM();
            Mapper.Map(paycenter, paycenterVM);
            //Cargar los movimientos para calcular el saldo
            EstadoDeCuentaRepository edoCuentaRepository = new EstadoDeCuentaRepository();
            var edoCuenta = edoCuentaRepository.GetMovimientosByPayCenterId(paycenterVM.PayCenterId);
            paycenterVM.SaldoActual = (edoCuenta.Where(x => x.IsAbono).Sum(x => x.Monto) - edoCuenta.Where(x => !x.IsAbono).Sum(x => x.Monto)).ToString("C");
            //Cargar los eventos
            PaquetesRepository paquetesRepository = new PaquetesRepository();
            paycenterVM.Eventos = paquetesRepository.GetEventosByPayCenter(paycenterVM.PayCenterId).ToString();
            //Asignar pagos realizados
            //ToDo: Checar si hay que validar algún estatus
            paycenterVM.PagosRealizados = paycenter.Pagos.Count.ToString();
            return paycenterVM;
        }

        /// <summary>
        /// Valida que esten capturados los campos necesarios para activar al paycenter
        /// </summary>
        /// <param name="paycenterVM"></param>
        /// <returns></returns>
        private bool ValidaActivacion(ref PayCenterVM paycenterVM)
        {
            if (string.IsNullOrWhiteSpace(paycenterVM.Nombre) || string.IsNullOrWhiteSpace(paycenterVM.Representante) ||
                string.IsNullOrWhiteSpace(paycenterVM.UserName) || string.IsNullOrWhiteSpace(paycenterVM.Telefono) ||
                string.IsNullOrWhiteSpace(paycenterVM.Email) || // || string.IsNullOrWhiteSpace(paycenterVM.Email2) ||
                string.IsNullOrWhiteSpace(paycenterVM.Domicilio) || string.IsNullOrWhiteSpace(paycenterVM.CP) ||
                string.IsNullOrWhiteSpace(paycenterVM.IFE) || string.IsNullOrWhiteSpace(paycenterVM.Comprobante)
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Valida que esten capturados los campos necesarios para activar al paycenter, valida la entidad
        /// </summary>
        /// <param name="paycenter"></param>
        /// <returns></returns>
        private bool ValidaActivacion(PayCenter paycenter)
        {
            if (string.IsNullOrWhiteSpace(paycenter.Nombre) || string.IsNullOrWhiteSpace(paycenter.Representante) ||
                string.IsNullOrWhiteSpace(paycenter.UserName) || string.IsNullOrWhiteSpace(paycenter.Telefono) ||
                string.IsNullOrWhiteSpace(paycenter.Email) || // || string.IsNullOrWhiteSpace(paycenterVM.Email2) ||
                string.IsNullOrWhiteSpace(paycenter.Domicilio) || string.IsNullOrWhiteSpace(paycenter.CP) ||
                string.IsNullOrWhiteSpace(paycenter.IFE) || string.IsNullOrWhiteSpace(paycenter.Comprobante)
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}