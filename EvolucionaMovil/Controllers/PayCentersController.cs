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
using EvolucionaMovil.Models.BR;
using cabinet.patterns.enums;

namespace EvolucionaMovil.Controllers
{
    public class PayCentersController : CustomControllerBase
    {
        private PayCentersRepository repository = new PayCentersRepository();

        //
        // GET: /PayCenters/
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.Administrator })]
        public ViewResult Index()
        {
            //var paycenters = repository.ListAll();
            //return View(paycenters.ToListOfDestination<PayCenterVM>());
            return View();
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.Administrator })]
        public string GetPayCenters(ServiceParameterVM parameters)
        {
            var estadoCuentaResult = getPaycenters(parameters);
            return Newtonsoft.Json.JsonConvert.SerializeObject(estadoCuentaResult);
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff })]
        public string GetSaldosPagoServicio(int PayCenterId)
        {
            EstadoCuentaBR br = new EstadoCuentaBR();
            var saldo = br.GetSaldosPagoServicio(PayCenterId);
            return Newtonsoft.Json.JsonConvert.SerializeObject(saldo);
        }

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff })]
        public string FindPayCenter(string term)
        {
            PayCentersRepository payCentersRepository = new PayCentersRepository();
            var payCenters = payCentersRepository.GetPayCenterBySearchString(term).Select(x => new { label = x.UserName + " - " + x.Nombre, value = x.PayCenterId });
            return Newtonsoft.Json.JsonConvert.SerializeObject(payCenters);
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

        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter })]
        public ActionResult Profile()
        {
            var payCenter = repository.LoadByIdName(HttpContext.User.Identity.Name);
            if (payCenter == null)
            {
                return RedirectToAction("Authorization", "Error");
            }
            var payCenterVM = FillPayCenterVM(payCenter.PayCenterId);
            return View("Details", payCenterVM);
        }

        //
        // GET: /PayCenters/Details/5
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.Administrator })]
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
        public ActionResult Registrar(Guid? id)
        {
            //Validar si no tiene usuario logueado, obligatoriamente debe pasar un Id
            if ((!HttpContext.User.Identity.IsAuthenticated) && (id == null))
            {
                return RedirectToAction("Authorization", "Error");
            }
            else if (HttpContext.User.Identity.IsAuthenticated && HttpContext.User.IsInRole(enumRoles.PayCenter.ToString())) //Si esta logueado un paycenter, no puede modificar sus datos porque ya está activo
            {
                return RedirectToAction("Authorization", "Error");
            }
            PayCenter paycenter = null;
            PayCenterVM paycenterVM = new PayCenterVM();

            ProspectosRepository repositoryProspecto = new ProspectosRepository();
            int ProspectoId = id != null ? repositoryProspecto.GetProspectoIdByGUID((Guid)id) : 0;

            if (ProspectoId > 0)
            {

                paycenter = repository.LoadByProspectoId(ProspectoId);
                if (paycenter != null)
                {
                    if (paycenter.Activo)
                    {
                        AddValidationMessage(enumMessageType.BRException, "El PayCenter ya se encuentra aprobado y activo, no es posible ser modificado por el Prospecto. Por favor, ingresa al sistema con tu usuario y contraseña.");
                    }
                    else
                    {
                        AddValidationMessage(enumMessageType.Notification, "Es necesario terminar de capturar la información correspondiente a su alta como PayCenter. Por favor, termine de capturar la información que se le solicita.");
                    }
                }
            }

            paycenterVM.ProspectoId = 0;
            if (id != null)
            {

                //en Modificación de Paycenter NO Activo dice que si no se encuentra paycenter debe redireccionar a un view NotFound, pero si es la primera vez que el prospecto se da de alta no aplica, entonces se hizo para cuando el prospecto no exista
                if (paycenter == null)
                {
                    Prospecto prospecto = repositoryProspecto.LoadById(ProspectoId);
                    if (prospecto != null)
                    {
                        paycenterVM.Celular = prospecto.Celular;
                        paycenterVM.Email = prospecto.Email;
                        paycenterVM.Nombre = prospecto.Empresa;
                        //paycenterVM.UserName = prospecto.Nombre;
                        paycenterVM.Telefono = prospecto.Telefono;
                        paycenterVM.ProspectoId = prospecto.ProspectoId;
                        paycenterVM.Representante = prospecto.Nombre;
                    }
                    else
                    {
                        return RedirectToAction("NotFound");
                    }
                }
                else
                {
                    Mapper.Map(paycenter, paycenterVM);
                    if (paycenter.PayCenterPadre != null)
                    {
                        paycenterVM.PayCenterPadre = paycenter.PayCenterPadre.UserName;
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
            Succeed = true;
            //Esto es para que no me marque requerido al validar cuando es actualización
            if (paycenterVM.PayCenterId > 0)
            {
                ModelState.Remove("Password");
                ModelState.Remove("RepeatPassword");
                ModelState.Remove("UserName");
            }

            if (ModelState.IsValid)
            {
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
                                Succeed = false;
                                AddValidationMessage(enumMessageType.UnhandledException, "El PayCenter ya se encuentra aprobado y activo, no es posible ser modificado por el Prospecto. Por favor, ingresa al sistema con tu usuario y contraseña.");
                            }
                        }
                        else
                        {
                            Succeed = false;
                            AddValidationMessage(enumMessageType.UnhandledException, "No se encontró el usuario del PayCenter.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Succeed = false;
                        AddValidationMessage(enumMessageType.UnhandledException, "Se ha producido un error al actualizar el usuario del PayCenter. " + ex.Message);
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
                        Succeed = false;
                        AddValidationMessage(enumMessageType.UnhandledException, "Se ha producido un error al crear el usuario del PayCenter. " + ex.Message);
                    }
                }

                if (Succeed)
                {
                    PayCenter paycenter;
                    bool modificando = paycenterVM.PayCenterId > 0;
                    if (paycenterVM.PayCenterId > 0)
                    {
                        paycenter = repository.LoadById(paycenterVM.PayCenterId);
                        paycenterVM.MaximoAFinanciar = paycenter.Parametros != null && paycenter.Parametros.MaximoAFinanciar != null ? paycenter.Parametros.MaximoAFinanciar.ToString() : string.Empty;
                        Mapper.Map(paycenterVM, paycenter);
                    }
                    else
                    {
                        paycenter = new PayCenter();
                        Mapper.Map(paycenterVM, paycenter);
                        if (paycenterVM.PayCenterPadreId == 0)
                        {
                            paycenter.PayCenterPadreId = null;
                            paycenter.PayCenterPadre = null;
                        }
                        repository.Add(paycenter);
                    }
                    Succeed = repository.Save();
                    if (!Succeed)
                    {
                        var mensaje = modificando ? "crear" : "actualizar";
                        AddValidationMessage(enumMessageType.UnhandledException, "No fue posible " + mensaje + " el paycenter. Favor de intentar más tarde o comunicarse con servicio a cliente.");
                        if (!modificando)
                        {
                            try
                            {
                                //Elimino el usuario en caso de haber fallado la creación del PayCenter
                                var user = Membership.GetUser(paycenter.UserName);
                                if (user != null)
                                {
                                    membership.Delete(user);
                                }
                            }
                            catch (Exception ex)
                            {
                                Succeed = false;
                                AddValidationMessage(enumMessageType.Notification, "El usuario creado no pudo ser eliminado");
                            }
                        }
                    }
                    else
                    {
                        paycenterVM.Activo = true; //Esto es sólo para que se deshabiliten los campos
                        AddValidationMessage(enumMessageType.Succeed, "El PayCenter se ha guardado con éxito. Si deseas modificar o terminar de completar tu información deberás acceder mediante el enlace que recibiste en tu correo o contactar al equipo de Evoluciona Móvil. En breve tu registro como PayCenter quedará activado.");
                    }
                }
            }
            return View(paycenterVM);
        }

        // GET: /PayCenters/Edit/5
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff, enumRoles.Administrator })]
        public ActionResult Edit(int id)
        {
            PayCenter paycenter = repository.LoadById(id);

            if (paycenter != null)
            {
                //Si el usuario logueado es un paycenter solo puede modificar sus datos
                if (HttpContext.User.IsInRole(enumRoles.PayCenter.ToString()) && paycenter.UserName != HttpContext.User.Identity.Name)
                {
                    return RedirectToAction("Authorization", "Error");
                }

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
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.PayCenter, enumRoles.Staff, enumRoles.Administrator })]
        public ActionResult Edit(PayCenterVM paycenterVM)
        {
            //Esto es para que no me marque requerido al validar cuando es actualización
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

                //<author>Moisés Cauich</author>
                //<comments>Se corrigieron las relaciones en el Entity Model</comments>
                //<before>
                //paycenterVM.UsuarioId = 1;
                //if (paycenterVM.ProspectoId == 0)
                //{
                //    paycenterVM.ProspectoId = 1;
                //}
                //</before>
                //<after />

                //Esto es para cuando se edita los datos de un paycenter que no tiene imagenes, no marque error
                if (paycenterVM.IFE == null)
                {
                    paycenterVM.IFE = string.Empty;
                }
                if (paycenterVM.Comprobante == null)
                {
                    paycenterVM.Comprobante = string.Empty;
                }

                //llenar los campos faltantes si estan nulos
                ValidaCapturaParcial(ref paycenterVM);

                Succeed = true;

                //Validar los datos necesarios para activar el usuario
                if (paycenterVM.Activo)
                {
                    Succeed = ValidaActivacion(ref paycenterVM);
                }

                //Validar si existe el usuario si el paycenter está siendo editado y actualizarlo
                if (Succeed)
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
                                Succeed = false;
                                AddValidationMessage(enumMessageType.UnhandledException ,"No se encontró el usuario del PayCenter.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Succeed = false;
                            AddValidationMessage(enumMessageType.UnhandledException ,"Se ha producido un error al actualizar el usuario del PayCenter. " + ex.Message);
                        }
                    }
                    else if (paycenterVM.Activo)
                    {
                        Succeed = false;
                        AddValidationMessage(enumMessageType.UnhandledException ,"No se ha creado el usuario del PayCenter, no se puede activar. desmarque la casilla de activo y guarde.");
                    }

                    if (Succeed)
                    {
                        PayCenter paycenter = repository.LoadById(paycenterVM.PayCenterId);
                        Mapper.Map(paycenterVM, paycenter);
                        if (paycenterVM.PayCenterPadreId <= 0)
                        {
                            paycenter.PayCenterPadre = null;
                        }

                        //Agregar valor del parámetro máximo a financiar
                        if (paycenter.Parametros != null)
                        {
                            paycenter.Parametros.MaximoAFinanciar = Convert.ToDecimal(paycenterVM.MaximoAFinanciar);
                        }
                        else
                        {
                            ParametrosPayCenter parametros = new ParametrosPayCenter() { 
                                PayCenterId = paycenterVM.PayCenterId, 
                                MaximoAFinanciar = Convert.ToDecimal(paycenterVM.MaximoAFinanciar),
                            };
                            repository.context.ParametrosPayCenters.AddObject(parametros);
                        }

                        repository.Save();

                        AddValidationMessage(enumMessageType.Succeed ,"El PayCenter se ha actualizado con éxito.");
                    }
                }
                else
                {
                    AddValidationMessage(enumMessageType.UnhandledException ,"Es necesario capturar todos los datos para activar al PayCenter. capture los datos faltantes o desmarque la casilla de activo y guarde.");
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
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Administrator })]
        public string DeleteConfirmed(int id)
        {
            PayCenter paycenter = repository.LoadById(id);
            repository.Delete(paycenter);
            repository.Save();
            return "El PayCenter se ha eliminado con éxito.";
        }

        [HttpPost]
        [CustomAuthorize(AuthorizedRoles = new[] { enumRoles.Staff, enumRoles.Administrator })]
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
            paycenters = paycenters.OrderByDescending(x => x.PayCenterId);
            SimpleGridResult<PayCenterVM> simpleGridResult = new SimpleGridResult<PayCenterVM>();
            IEnumerable<PayCenter> paycentersPaged = null;
            if (Parameters != null)
            {
                simpleGridResult.CurrentPage = Parameters.pageNumber;
                simpleGridResult.PageSize = Parameters.pageSize;
                if (Parameters.pageSize > 0)
                {
                    var pageNumber = Parameters.pageNumber >= 0 ? Parameters.pageNumber : 0;
                    simpleGridResult.CurrentPage = pageNumber;
                    simpleGridResult.TotalRows = paycenters.Count();
                    paycentersPaged = paycenters.Skip(pageNumber * Parameters.pageSize).Take(Parameters.pageSize);
                }
            }
            simpleGridResult.Result = paycentersPaged.ToListOfDestination<PayCenterVM>().OrderByDescending(x => x.FechaCreacion);

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

            //<author>Moisés Cauich</author>
            //<comments>Obtener los datos de EstadoCuentaBR</comments>
            //<before>
            ////Cargar los movimientos para calcular el saldo
            //EstadoDeCuentaRepository edoCuentaRepository = new EstadoDeCuentaRepository();
            //var edoCuenta = edoCuentaRepository.GetMovimientosByPayCenterId(paycenterVM.PayCenterId);
            //paycenterVM.SaldoActual = (edoCuenta.Where(x => x.IsAbono).Sum(x => x.Monto) - edoCuenta.Where(x => !x.IsAbono).Sum(x => x.Monto)).ToString("C");
            ////Cargar los eventos
            //PaquetesRepository paquetesRepository = new PaquetesRepository();
            //paycenterVM.Eventos = paquetesRepository.GetEventosByPayCenter(paycenterVM.PayCenterId).ToString();
            ////Asignar pagos realizados
            //paycenterVM.PagosRealizados = paycenter.Pagos.Count.ToString();
            //</before>
            //<after>
            EstadoCuentaBR estadoCuenta = new EstadoCuentaBR();
            var saldos = estadoCuenta.GetSaldosPagoServicio(Id);
            paycenterVM.SaldoActual = saldos.SaldoActual.ToString("C");
            paycenterVM.SaldoDisponible = saldos.SaldoDisponible.ToString("C");
            paycenterVM.Eventos = saldos.EventosDisponibles.ToString();
            paycenterVM.PagosRealizados = paycenter.Pagos.Count(p => p.Status == enumEstatusMovimiento.Aplicado.GetHashCode()).ToString();
            //</after>

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