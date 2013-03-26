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

namespace EvolucionaMovil.Controllers
{
    public class PayCentersController : Controller
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

        //
        // GET: /PayCenters/Details/5
        public ViewResult Details(int id)
        {
            PayCenter paycenter = repository.LoadById(id);

            if (paycenter != null)
            {
                PayCenterVM paycenterVM = new PayCenterVM();
                Mapper.Map(paycenter, paycenterVM);
                return View(paycenterVM);
            }
            else
            {
                return View("NotFound");
            }
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
                        //ToDo: Agregar la empresa a la tabla
                        paycenterVM.Nombre = prospecto.Empresa;
                        //paycenterVM.NombreCorto = prospecto.Nombre;
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
                    paycenterVM.PayCenterPadre = paycenter.PayCenters2.NombreCorto;
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
                ModelState.Remove("NombreCorto");
            }

            if (ModelState.IsValid)
            {
                //TODO:Leer valor de la imagen del comprobante y el ife de domicilio
                paycenterVM.Comprobante = string.Empty;
                paycenterVM.IFE = string.Empty;

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
                if (paycenterVM.PayCenterId > 0 && !string.IsNullOrWhiteSpace(paycenterVM.NombreCorto)) //&& !string.IsNullOrWhiteSpace(paycenterVM.Password))
                {
                    try
                    {
                        MembershipUser usuario = membership.Get(paycenterVM.NombreCorto);

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
                else if (!(string.IsNullOrWhiteSpace(paycenterVM.NombreCorto) || string.IsNullOrWhiteSpace(paycenterVM.Password)))
                {
                    try
                    {
                        var user = membership.Create(paycenterVM.NombreCorto, paycenterVM.Password, paycenterVM.Email, null, null, false);
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
                        paycenterVM.MaximoAFinanciar = paycenter.MaximoAFinanciar.ToString();
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
                paycenterVM.PayCenterPadre = paycenter.PayCenters2.NombreCorto;

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
            ModelState.Remove("NombreCorto");

            if (ModelState.IsValid)
            {
                //TODO:Leer valor de la imagen del comprobante y el ife de domicilio
                paycenterVM.Comprobante = "";
                paycenterVM.IFE = "";

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
                    if (!string.IsNullOrWhiteSpace(paycenterVM.NombreCorto))
                    {
                        try
                        {
                            MembershipUser usuario = membership.Get(paycenterVM.NombreCorto);

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

        //
        // GET: /PayCenters/Delete/5
        public ActionResult Delete(int id)
        {
            PayCenter paycenter = repository.LoadById(id);
            PayCenterVM paycenterVM = new PayCenterVM();
            Mapper.Map(paycenter, paycenterVM);
            return View(paycenterVM);
        }

        //
        // POST: /PayCenters/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            PayCenter paycenter = repository.LoadById(id);
            repository.Delete(paycenter);
            repository.Save();
            return RedirectToAction("Index");
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
            SimpleGridResult<PayCenterVM> simpleGridResult = new SimpleGridResult<PayCenterVM>();
            var PaycentersVM = repository.ListAll().ToListOfDestination<PayCenterVM>();

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

        /// <summary>
        /// Valida que esten capturados los campos necesarios para activar al paycenter
        /// </summary>
        /// <param name="paycenterVM"></param>
        /// <returns></returns>
        private bool ValidaActivacion(ref PayCenterVM paycenterVM)
        {
            //ToDo: Activar validación de IFE y Comprobante cuando se agregue la funcionalidad
            if (string.IsNullOrWhiteSpace(paycenterVM.Nombre) || string.IsNullOrWhiteSpace(paycenterVM.Representante) ||
                string.IsNullOrWhiteSpace(paycenterVM.NombreCorto) || string.IsNullOrWhiteSpace(paycenterVM.Telefono) ||
                string.IsNullOrWhiteSpace(paycenterVM.Email) || string.IsNullOrWhiteSpace(paycenterVM.Email2) ||
                string.IsNullOrWhiteSpace(paycenterVM.Domicilio) || string.IsNullOrWhiteSpace(paycenterVM.CP) //|| 
                //string.IsNullOrWhiteSpace(paycenterVM.IFE) || string.IsNullOrWhiteSpace(paycenterVM.Comprobante)
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