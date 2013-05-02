using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
namespace EvolucionaMovil.Controllers
{
    public class ImgFichaDepositoController:Controller
    {
        public ActionResult IndexPartial()
        {
            //ViewBag.Message = "MVC 3 File Upload";
            ViewBag.Ruta = "";
            return PartialView();
        }
        [HttpPost]
        public ActionResult IndexPartial(HttpPostedFileBase file)
        {
            if (file != null)
            {
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".")).ToLower();
                if (extension == ".png" || extension == ".jpg")
                {
                    // extract only the fielname             
                    var fileName = Path.GetFileName(file.FileName);
                    // store the file inside ~/FolderName/User-Image folder             
                    var path = Path.Combine(Server.MapPath("~/imgFichaDeposito/"), fileName);
                    // this is the string you have to save in your DB
                    string filepathToSave = "imgFichaDeposito/" + fileName;
                    ViewBag.Ruta = "/imgFichaDeposito/" + fileName;
                    try
                    {
                        file.SaveAs(path);
                    }
                    catch (Exception ex)
                    {

                        ViewBag.Mensajes = "No fue posible guardar la imagen del comprobante. - " + ex.Message;
                    }
                    
                    //--------------------------------------------

                    // Code to save file in DB by passing the file path

                    //----------------------------------------------
                }
                else
                {
                    ViewBag.Mensajes = "La imagen no cumple con el formato correcto.";
                }
            }
            else
            {
                ViewBag.Mensajes = "Seleccione la imagen de la ficha de depósito.";
            }
            return PartialView();
        }

    }
}